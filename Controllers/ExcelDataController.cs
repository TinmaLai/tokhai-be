using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace z76_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelDataController : Controller
    {
        public ExcelDataController()
        {
            // Cấu hình EPPlus cho mục đích phi thương mại
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// Nhận đầu vào 2 file Excel, thực hiện merge dữ liệu theo cột “id” (dựa trên Ký hiệu hóa đơn, Số hóa đơn, MST)
        /// và tạo ra 2 file Excel:
        ///   - Updated_Tax_Report.xlsx: file gốc sau merge (như code có sẵn)
        ///   - Updated_Tax_Report_With_Status.xlsx: file trên được chèn thêm cột “Trạng thái hóa đơn” (lấy giá trị từ cột AB của detailFile)
        /// Cuối cùng đóng gói 2 file Excel này vào 1 file zip trả về client.
        /// </summary>
        [HttpPost("update")]
        public IActionResult UpdateExcel(IFormFile taxFile, IFormFile detailFile)
        {
            if (taxFile == null || detailFile == null)
                return BadRequest("Vui lòng tải lên cả hai file Excel.");

            try
            {
                // 1. Đọc dữ liệu từ file detail thành List<string[]>
                var detailData = ReadExcelData(detailFile);

                // 2. Tiền xử lý: xây dựng Dictionary với key = "Ký hiệu hóa đơn_ Số hóa đơn_ MST"
                // (số hóa đơn được TrimStart('0'))
                var detailDict = new Dictionary<string, List<string[]>>();
                foreach (var row in detailData)
                {
                    if (row.Length < 3) continue;
                    var key = $"{row[1].Trim()}_{row[2].Trim().TrimStart('0')}_{row[10].Trim()}";
                    if (!detailDict.ContainsKey(key))
                        detailDict[key] = new List<string[]>();
                    detailDict[key].Add(row);
                }

                // Cấu hình căn lề, cột số,…
                var numberColumns = new List<int> { 10, 11, 12, 14 };

                var rightAlignColumns = new List<int> { 5, 10, 11, 12, 14 };
                var centerAlignColumns = new List<int> { 2, 9, 13 };
                var textColumns = new List<int> { 1, 2, 3, 4, 6, 7, 8, 9, 15 };

                // Các biến dùng chung (sẽ dùng sau khi xử lý file gốc)
                int headerRow = 0;
                int startRow = 0;
                int finalRowCount = 0;

                // MemoryStream chứa file Excel gốc (đã merge)
                MemoryStream msOriginal = new MemoryStream();

                // --- PHẦN 1: XỬ LÝ TAXFILE (merge dữ liệu) ---
                using (var taxPackage = new ExcelPackage(taxFile.OpenReadStream()))
                {
                    // Lấy sheet đầu tiên không bị ẩn (nếu có)
                    var taxSheet = taxPackage.Workbook.Worksheets.FirstOrDefault(ws => ws.Hidden == eWorkSheetHidden.Visible)
                                   ?? taxPackage.Workbook.Worksheets[0];

                    int colCount = taxSheet.Dimension.Columns;
                    int originalRowCount = taxSheet.Dimension.Rows;

                    // Tìm hàng header (ô đầu tiên có giá trị "STT")
                    for (int r = 1; r <= originalRowCount; r++)
                    {
                        if (taxSheet.Cells[r, 1].Text.Trim().Equals("STT", StringComparison.OrdinalIgnoreCase))
                        {
                            headerRow = r;
                            break;
                        }
                    }
                    var totalRowCount = 0;
                    // Điều chỉnh số dòng nếu gặp dòng "TỔNG CỘNG"
                    for (int r = 1; r <= originalRowCount; r++)
                    {
                        if (taxSheet.Cells[r, 6].Text.Trim().Equals("TỔNG CỘNG", StringComparison.OrdinalIgnoreCase))
                        {
                            originalRowCount = originalRowCount - (originalRowCount - r + 2);
                            break;
                        }
                    }
                    // Xác định startRow (dòng bắt đầu dữ liệu)
                    startRow = headerRow > 0 ? headerRow + 1 : 19;

                    // Định nghĩa mapping giữa cột TaxFile và DetailFile
                    var mappings = new (int TaxCol, int DetailCol)[]
                    {
                        (2, 1),   // Mẫu số hóa đơn
                        (5, 4),   // Ngày tháng năm: TaxFile cột5 <-- DetailFile cột4
                        (6, 10),  // Tên người bán: TaxFile cột6 <-- DetailFile cột10
                        (7, 11),  // MST người bán: TaxFile cột7 <-- DetailFile cột11
                        (8, 17),  // Tên hàng hóa/DV: TaxFile cột8 <-- DetailFile cột17
                        (9, 18),  // Đơn vị tính: TaxFile cột9 <-- DetailFile cột18
                        (10, 19), // Số lượng: TaxFile cột10 <-- DetailFile cột19
                        (11, 20), // Đơn giá: TaxFile cột11 <-- DetailFile cột20
                        (12, 23), // Chưa thuế: TaxFile cột12 <-- DetailFile cột23
                        (13, 22), // Thuế suất: TaxFile cột13 <-- DetailFile cột22
                        (14, 24)  // Tiền thuế: TaxFile cột14 <-- DetailFile cột24
                    };

                    // Tính trước các mapping dùng cho cột 12 và 14 để so sánh tổng
                    var compareMappings = mappings.Where(x => x.TaxCol == 12 || x.TaxCol == 14).ToArray();

                    // Duyệt qua các dòng dữ liệu (bỏ qua header)
                    for (int row = startRow + 2; row <= originalRowCount; row++)
                    {
                        // Lưu lại giá trị cần dùng nhiều lần
                        string cellQ = taxSheet.Cells[row, 17].Text.Trim();
                        if (cellQ == "3.")
                            continue;

                        // Xây dựng key từ các cột: invoiceSymbol (cột 3), invoiceNumber (cột 4) và taxCode (cột 7)
                        string invoiceSymbol = taxSheet.Cells[row, 3].Text.Trim();
                        string invoiceNumber = taxSheet.Cells[row, 4].Text.Trim().TrimStart('0');
                        string taxCode = taxSheet.Cells[row, 7].Text.Trim();
                        var key = $"{invoiceSymbol}_{invoiceNumber}_{taxCode}";

                        if (!detailDict.TryGetValue(key, out var matchingRows))
                        {
                            // Không có matching detail: tô màu vàng toàn dòng
                            taxSheet.Cells[row, 1, row, colCount].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            taxSheet.Cells[row, 1, row, colCount].Style.Fill.BackgroundColor.SetColor(Color.LemonChiffon);
                        }
                        else if (matchingRows.Count == 1)
                        {
                            // Trường hợp chỉ có 1 dòng match
                            foreach (var map in mappings.Where(x => x.TaxCol == 12 || x.TaxCol == 14))
                            {
                                string detailVal = matchingRows[0].Length >= map.DetailCol
                                                     ? matchingRows[0][map.DetailCol - 1].Trim()
                                                     : "";
                                string original = taxSheet.Cells[row, map.TaxCol].Text.Trim();
                                if (!string.Equals(original, detailVal, StringComparison.OrdinalIgnoreCase))
                                {
                                    taxSheet.Cells[row, map.TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    taxSheet.Cells[row, map.TaxCol].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                                }
                            }
                            foreach (var map in mappings)
                            {
                                string detailVal = matchingRows[0].Length >= map.DetailCol
                                                     ? matchingRows[0][map.DetailCol - 1].Trim()
                                                     : "";
                                if (map.TaxCol == 13)
                                    taxSheet.Cells[row, map.TaxCol].Value = detailVal.Trim('%');
                                else
                                    taxSheet.Cells[row, map.TaxCol].Value = detailVal;
                                if (numberColumns.Contains(map.TaxCol))
                                {
                                    if (double.TryParse(detailVal, out double numberVal))
                                    {
                                        taxSheet.Cells[row, map.TaxCol].Value = numberVal;
                                    }
                                    else
                                    {
                                        taxSheet.Cells[row, map.TaxCol].Value = detailVal;
                                    }
                                    taxSheet.Cells[row, map.TaxCol].Style.Numberformat.Format = "#,##0.00";
                                }
                                // Thiết lập border và căn lề
                                taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Color.SetColor(Color.Black);
                                taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Color.SetColor(Color.Black);

                                if (rightAlignColumns.Contains(map.TaxCol))
                                    taxSheet.Cells[row, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                else if (centerAlignColumns.Contains(map.TaxCol))
                                    taxSheet.Cells[row, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                else
                                    taxSheet.Cells[row, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            }
                        }
                        else // matchingRows.Count >= 2
                        {
                            int n = matchingRows.Count;
                            bool errorGroup1 = false;
                            bool errorGroup2 = false;
                            double[] sums = new double[compareMappings.Length];
                            for (int i = 0; i < n; i++)
                            {
                                for (int j = 0; j < compareMappings.Length; j++)
                                {
                                    double val = 0;
                                    if (matchingRows[i].Length >= compareMappings[j].DetailCol)
                                        double.TryParse(matchingRows[i][compareMappings[j].DetailCol - 1], out val);
                                    sums[j] += val;
                                }
                            }
                            for (int j = 0; j < compareMappings.Length; j++)
                            {
                                string detailVal = matchingRows[0].Length >= compareMappings[j].DetailCol
                                                     ? matchingRows[0][compareMappings[j].DetailCol - 1].Trim()
                                                     : "";
                                double origVal = 0;
                                double.TryParse(taxSheet.Cells[row, compareMappings[j].TaxCol].Text.Trim(), out origVal);
                                if (Math.Abs(origVal - sums[j]) > 0.0001)
                                {
                                    if(compareMappings[j].TaxCol == 12)
                                    {
                                        errorGroup1 = true;
                                    } else if(compareMappings[j].TaxCol == 14)
                                    {
                                        errorGroup2 = true;
                                    }
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Style.Fill.BackgroundColor.SetColor(Color.LightCoral); // bôi đỏ
                                }
                                if (compareMappings[j].TaxCol == 13)
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Value = detailVal.Trim('%');
                                else
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Value = detailVal;
                                if (numberColumns.Contains(compareMappings[j].TaxCol))
                                {
                                    if (double.TryParse(detailVal, out double numberVal))
                                    {
                                        taxSheet.Cells[row, compareMappings[j].TaxCol].Value = numberVal;
                                    }
                                    else
                                    {
                                        taxSheet.Cells[row, compareMappings[j].TaxCol].Value = detailVal;
                                    }
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Style.Numberformat.Format = "#,##0.00";
                                }
                            }
                            foreach (var map in mappings)
                            {
                                string detailVal = matchingRows[0].Length >= map.DetailCol
                                                     ? matchingRows[0][map.DetailCol - 1].Trim()
                                                     : "";
                                if (map.TaxCol == 13)
                                    taxSheet.Cells[row, map.TaxCol].Value = detailVal.Trim('%');
                                else
                                    taxSheet.Cells[row, map.TaxCol].Value = detailVal;
                                if (numberColumns.Contains(map.TaxCol))
                                {
                                    if (double.TryParse(detailVal, out double numberVal))
                                    {
                                        taxSheet.Cells[row, map.TaxCol].Value = numberVal;
                                    }
                                    else
                                    {
                                        taxSheet.Cells[row, map.TaxCol].Value = detailVal;
                                    }
                                    taxSheet.Cells[row, map.TaxCol].Style.Numberformat.Format = "#,##0.00";
                                }
                                taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Color.SetColor(Color.Black);
                                taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Color.SetColor(Color.Black);
                                if (rightAlignColumns.Contains(map.TaxCol))
                                    taxSheet.Cells[row, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                else if (centerAlignColumns.Contains(map.TaxCol))
                                    taxSheet.Cells[row, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                else
                                    taxSheet.Cells[row, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            }
                            if (n > 1)
                            {
                                // Chèn các dòng detail mới
                                taxSheet.InsertRow(row + 1, n - 1);
                                for (int i = 1; i < n; i++)
                                {
                                    int targetRow = row + i;

                                    // Copy toàn bộ format, style từ dòng summary (row) sang dòng mới (targetRow)
                                    // Lưu ý: nếu bạn có số cột cố định, có thể thay taxSheet.Dimension.Columns bằng số cột cụ thể.
                                    //taxSheet.Cells[row, 1, row, taxSheet.Dimension.Columns]
                                    //        .Copy(taxSheet.Cells[targetRow, 1, targetRow, taxSheet.Dimension.Columns]);

                                    // Bind dữ liệu cho các cột theo mapping
                                    foreach (var map in mappings)
                                    {
                                        string detailVal = matchingRows[i].Length >= map.DetailCol
                                                            ? matchingRows[i][map.DetailCol - 1].Trim()
                                                            : "";
                                        taxSheet.Cells[row, map.TaxCol]
                                            .Copy(taxSheet.Cells[targetRow, map.TaxCol]);
                                        if (map.TaxCol == 13)
                                            taxSheet.Cells[targetRow, map.TaxCol].Value = detailVal.Trim('%');
                                        else
                                            taxSheet.Cells[targetRow, map.TaxCol].Value = detailVal;
                                        if (numberColumns.Contains(map.TaxCol))
                                        {
                                            if (double.TryParse(detailVal, out double numberVal))
                                            {
                                                taxSheet.Cells[targetRow, map.TaxCol].Value = numberVal;
                                            }
                                            else
                                            {
                                                taxSheet.Cells[targetRow, map.TaxCol].Value = detailVal;
                                            }
                                            taxSheet.Cells[targetRow, map.TaxCol].Style.Numberformat.Format = "#,##0.00";
                                        }
                                        // Nếu có lỗi tính tổng (errorGroup2) thì cập nhật lại màu cho cột 12, 14
                                        if (map.TaxCol == 12 && errorGroup1)
                                        {
                                            taxSheet.Cells[targetRow, map.TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            taxSheet.Cells[targetRow, map.TaxCol].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                                        } else if(map.TaxCol == 14 && errorGroup2)
                                        {
                                            taxSheet.Cells[targetRow, map.TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                            taxSheet.Cells[targetRow, map.TaxCol].Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                                        }
                                    }
                                    // Nếu cần, copy thêm giá trị từ một số cột của dòng summary (ví dụ: cột 3,4,7)
                                    taxSheet.Cells[targetRow, 3].Value = taxSheet.Cells[row, 3].Value;
                                    taxSheet.Cells[targetRow, 4].Value = taxSheet.Cells[row, 4].Value;
                                    taxSheet.Cells[targetRow, 7].Value = taxSheet.Cells[row, 7].Value;
                                }
                                row += (n - 1);
                                originalRowCount += (n - 1);
                            }
                        }
                    }
                    for (int r = 1; r <= taxSheet.Dimension.Rows; r++)
                    {
                        if (taxSheet.Cells[r, 6].Text.Trim().Equals("TỔNG CỘNG", StringComparison.OrdinalIgnoreCase))
                        {
                            totalRowCount = taxSheet.Dimension.Rows - r;
                            break;
                        }
                    }
                    // --- Đánh số thứ tự (STT) cho các dòng dữ liệu ---
                    finalRowCount = taxSheet.Dimension.Rows - totalRowCount - 2;
                    int order = 1;
                    for (int r = startRow + 2; r <= finalRowCount; r++)
                    {
                        taxSheet.Cells[r, 1].Value = order++;
                        taxSheet.Cells[r, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        taxSheet.Cells[r, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        taxSheet.Cells[r, 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        taxSheet.Cells[r, 1].Style.Border.Left.Color.SetColor(Color.Black);
                        taxSheet.Cells[r, 1].Style.Border.Right.Color.SetColor(Color.Black);
                    }

                    // Lưu file Excel đã cập nhật vào MemoryStream
                    taxPackage.SaveAs(msOriginal);
                    msOriginal.Position = 0;
                } // end using taxPackage

                // --- PHẦN 2: TẠO FILE EXCEL VỚI CỘT "TRẠNG THÁI HÓA ĐƠN" ---
                // Ở file này, ta sẽ chèn thêm 1 cột ở vị trí đầu tiên và cập nhật giá trị từ detailFile (cột AB, tương đương cột 28)
                MemoryStream msStatus = new MemoryStream();
                using (var taxStatusPackage = new ExcelPackage(new MemoryStream(msOriginal.ToArray())))
                {
                    // Lấy sheet đầu tiên không bị ẩn (nếu có)
                    var taxStatusSheet = taxStatusPackage.Workbook.Worksheets.FirstOrDefault(ws => ws.Hidden == eWorkSheetHidden.Visible)
                                   ?? taxStatusPackage.Workbook.Worksheets[0];
                    // Chèn 1 cột mới vào vị trí cột 1
                    taxStatusSheet.InsertColumn(1, 1);

                    // Merge ô header của cột mới (ô (headerRow, 1)) với ô bên dưới (headerRow+1, 1)
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Merge = true;
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Value = "Trạng thái hóa đơn";
                    // Căn giữa nội dung theo chiều ngang và dọc
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    // Bôi đậm chữ
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Font.Bold = true;
                    // border
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Border.Left.Color.SetColor(Color.Black);
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Border.Right.Color.SetColor(Color.Black);
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Border.Top.Color.SetColor(Color.Black);
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Border.Bottom.Color.SetColor(Color.Black);
                    // Set lại background của ô merge về màu trắng (tránh hiện mặc định màu xám)
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    taxStatusSheet.Cells[headerRow, 1, headerRow + 1, 1].Style.Fill.BackgroundColor.SetColor(Color.White);

                    // Vì đã chèn cột mới, các cột dữ liệu gốc bị dịch sang phải:
                    //   invoiceSymbol: từ cột 3 -> cột 4
                    //   invoiceNumber: từ cột 4 -> cột 5
                    //   taxCode:      từ cột 7 -> cột 8
                    int sheetFinalRow = taxStatusSheet.Dimension.Rows;
                    for (int row = startRow + 2; row <= sheetFinalRow; row++)
                    {
                        string invoiceSymbol = taxStatusSheet.Cells[row, 4].Text.Trim();
                        string invoiceNumber = taxStatusSheet.Cells[row, 5].Text.Trim().TrimStart('0');
                        string taxCode = taxStatusSheet.Cells[row, 8].Text.Trim();
                        var key = $"{invoiceSymbol}_{invoiceNumber}_{taxCode}";

                        if (detailDict.TryGetValue(key, out var matchingRows) && matchingRows.Count > 0)
                        {
                            // Lấy giá trị ở cột AB (tức cột 28) của detailFile (array index 27)
                            string status = matchingRows[0].Length >= 28 ? matchingRows[0][27].Trim() : "";
                            taxStatusSheet.Cells[row, 1].Value = status;
                        }
                        else
                        {
                            taxStatusSheet.Cells[row, 1].Value = "";
                        }
                    }
                    taxStatusPackage.SaveAs(msStatus);
                    msStatus.Position = 0;
                }

                // --- PHẦN 3: ĐÓNG GÓI 2 FILE EXCEL VÀO 1 FILE ZIP ---
                MemoryStream zipStream = new MemoryStream();
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    // Thêm file Excel gốc
                    var entry1 = archive.CreateEntry("Updated_Tax_Report.xlsx");
                    using (var entryStream = entry1.Open())
                    {
                        msOriginal.CopyTo(entryStream);
                    }
                    msOriginal.Position = 0;

                    // Thêm file Excel có cột "Trạng thái hóa đơn"
                    var entry2 = archive.CreateEntry("Updated_Tax_Report_With_Status.xlsx");
                    using (var entryStream = entry2.Open())
                    {
                        msStatus.CopyTo(entryStream);
                    }
                }
                zipStream.Position = 0;

                return File(zipStream, "application/octet-stream", "Updated_Tax_Reports.zip");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xử lý: {ex.Message}");
            }
        }

        /// <summary>
        /// Đọc dữ liệu từ file Excel thành List<string[]>
        /// Mỗi string[] tương ứng với một dòng (index 0 = cột A, 1 = cột B, …)
        /// Lấy sheet đầu tiên không bị ẩn (visible) nếu có.
        /// </summary>
        private List<string[]> ReadExcelData(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Hidden == eWorkSheetHidden.Visible)
                            ?? package.Workbook.Worksheets[0];
            var data = new List<string[]>();
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;
            for (int row = 1; row <= rowCount; row++)
            {
                string[] rowData = new string[colCount];
                for (int col = 1; col <= colCount; col++)
                {
                    rowData[col - 1] = worksheet.Cells[row, col].Text;
                }
                data.Add(rowData);
            }
            return data;
        }
    }

    public class MergeRequest
    {
        public string[][] TaxData { get; set; } // Bảng kê hoàn thuế
        public string[][] DetailData { get; set; } // Bảng kê chi tiết hóa đơn
    }
}
