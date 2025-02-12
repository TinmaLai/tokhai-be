using System;
using System.Data;
using System.Drawing;
using System.IO;
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
        /// Nhận đầu vào là 2 file excel,
        /// trong đó mảng con đầu tiên là header (tiêu đề cột).
        /// Thực hiện merge dữ liệu theo cột "id" và trả về file Excel kết quả.
        /// </summary>
        /// <param name="request">Dữ liệu đầu vào gồm MasterData và DetailData</param>
        /// <returns>File Excel đã merge</returns>
        [HttpPost("update")]
        public IActionResult UpdateExcel(IFormFile taxFile, IFormFile detailFile)
        {
            if (taxFile == null || detailFile == null)
                return BadRequest("Vui lòng tải lên cả hai file Excel.");

            try
            {
                // 1. Đọc dữ liệu từ file detail thành List<string[]>
                var detailData = ReadExcelData(detailFile);

                // 2. Tiền xử lý: xây dựng Dictionary với key = "Ký hiệu hóa đơn_ Số hóa đơn"
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

                using (var taxPackage = new ExcelPackage(taxFile.OpenReadStream()))
                {
                    // Lấy sheet đầu tiên không bị ẩn (nếu có)
                    var taxSheet = taxPackage.Workbook.Worksheets.FirstOrDefault(ws => ws.Hidden == eWorkSheetHidden.Visible)
                                   ?? taxPackage.Workbook.Worksheets[0];

                    int colCount = taxSheet.Dimension.Columns;

                    // Lưu số dòng ban đầu của taxSheet (dòng gốc)
                    int originalRowCount = taxSheet.Dimension.Rows;
                    // Tìm hàng có ô đầu tiên (cột 1) có giá trị "STT" (header)
                    int headerRow = 0;
                    for (int r = 1; r <= originalRowCount; r++)
                    {
                        if (taxSheet.Cells[r, 1].Text.Trim().Equals("STT", StringComparison.OrdinalIgnoreCase))
                        {
                            headerRow = r;
                            break;
                        }
                    }
                    for (int r = 1; r <= originalRowCount; r++)
                    {
                        if (taxSheet.Cells[r, 6].Text.Trim().Equals("TỔNG CỘNG", StringComparison.OrdinalIgnoreCase))
                        {
                            originalRowCount = originalRowCount - (originalRowCount - r - 2);
                            break;
                        }
                    }
                    // Nếu không tìm thấy, mặc định headerRow = 1, và dữ liệu bắt đầu từ hàng 2
                    int startRow = headerRow > 0 ? headerRow + 1 : 19;
                    var mappings = new (int TaxCol, int DetailCol)[]
                    {
                        (2, 1),   // Mẫu số hóa đơn
                        (5, 4),   // Ngày tháng năm: TaxFile cột5 <-- DetailFile cột4
                        (6, 10),  // Tên người bán: TaxFile cột6 <-- DetailFile cột10
                        (7, 11),  // MST người bán: TaxFile cột7 <-- DetailFile cột11
                        (8, 17),  // Tên hàng hóa DV: TaxFile cột8 <-- DetailFile cột17
                        (9, 18),  // Đơn vị tính: TaxFile cột9 <-- DetailFile cột18
                        (10, 19), // Số lượng: TaxFile cột10 <-- DetailFile cột19
                        (11, 20), // Đơn giá: TaxFile cột11 <-- DetailFile cột20
                        (12, 23), // Chưa thuế: TaxFile cột12 <-- DetailFile cột23
                        (13, 22), // Thuế suất: TaxFile cột13 <-- DetailFile cột22
                        (14, 24)  // Tiền thuế: TaxFile cột14 <-- DetailFile cột24
                    };
                    // Duyệt qua các dòng (bỏ qua header ở dòng 1)
                    for (int row = startRow + 2; row <= originalRowCount; row++)
                    {
                        if (taxSheet.Cells[row, 17].Text.Trim() == "3.")
                        {
                            continue; // Bỏ qua dòng này nếu giá trị ô Q bằng "3."
                        }

                        string invoiceSymbol = taxSheet.Cells[row, 3].Text.Trim();
                        string invoiceNumber = taxSheet.Cells[row, 4].Text.Trim().TrimStart('0');
                        string taxCode = taxSheet.Cells[row, 7].Text.Trim();
                        var key = $"{invoiceSymbol}_{invoiceNumber}_{taxCode}";

                        if (!detailDict.TryGetValue(key, out var matchingRows))
                        {
                            // Không tìm thấy matching detail: tô màu vàng toàn dòng, cập nhật trạng thái
                            taxSheet.Cells[row, 1, row, colCount].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            taxSheet.Cells[row, 1, row, colCount].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        }
                        else if (matchingRows.Count == 1)
                        {
                            // TRƯỜNG HỢP 1: chỉ có 1 dòng match
                            // Định nghĩa mapping cho cả Group 1 và Group 2
                            // Mỗi phần tử là (TaxCol, DetailCol)
                            

                            bool hasError = false;
                            // Với 1 match, ta luôn so sánh dòng đó với matchingRows[0]
                            foreach (var map in mappings.Where(x => x.TaxCol == 12 || x.TaxCol == 14))
                            {
                                // 22 23
                                // Lấy giá trị của ô từ matchingRows[0]
                                string detailVal = matchingRows[0].Length >= map.DetailCol
                                                     ? matchingRows[0][map.DetailCol - 1].Trim()
                                                     : "";
                                // Lấy giá trị gốc của TaxFile
                                string original = taxSheet.Cells[row, map.TaxCol].Text.Trim();
                                // Nếu giá trị thay đổi thì đánh dấu màu đỏ
                                if (!string.Equals(original, detailVal, StringComparison.OrdinalIgnoreCase))
                                {
                                    taxSheet.Cells[row, map.TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    taxSheet.Cells[row, map.TaxCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    // Thêm border cho bên trái và bên phải của ô B2
                                    taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                                    // (Tùy chọn) Đặt màu cho border nếu cần
                                    taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Color.SetColor(Color.Black);
                                    taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Color.SetColor(Color.Black);
                                    hasError = true;
                                }
                            }
                            foreach (var map in mappings)
                            {
                                // 22 23
                                // Lấy giá trị của ô từ matchingRows[0]
                                string detailVal = matchingRows[0].Length >= map.DetailCol
                                                     ? matchingRows[0][map.DetailCol - 1].Trim()
                                                     : "";
                                // Lấy giá trị gốc của TaxFile
                                string original = taxSheet.Cells[row, map.TaxCol].Text.Trim();
                                // Bind giá trị mới vào ô TaxFile
                                taxSheet.Cells[row, map.TaxCol].Value = detailVal;
                                if (taxSheet.Cells[row, map.TaxCol].Value is double || taxSheet.Cells[row, map.TaxCol].Value is int || taxSheet.Cells[row, map.TaxCol].Value is decimal)
                                {
                                    taxSheet.Cells[row, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right; // Căn phải nếu là số
                                }
                                else
                                {
                                    taxSheet.Cells[row, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Căn trái nếu là chữ
                                }
                            }
                        }
                        else // matchingRows.Count >= 2
                        {
                            int n = matchingRows.Count;
                            bool errorGroup1 = false;
                            bool errorGroup2 = false;

                            // --- Phần Summary (dòng gốc) ---
                            // Group 1: mapping từ matchingRows[0]
                            //var mappingsGroup1 = new (int TaxCol, int DetailCol)[]
                            //{
                            //    (5, 4), (6, 10), (7, 11), (8, 17), (9, 18)
                            //};
                            //foreach (var map in mappingsGroup1)
                            //{
                            //    string original = taxSheet.Cells[row, map.TaxCol].Text.Trim();
                            //    string detailVal = matchingRows[0].Length >= map.DetailCol
                            //                         ? matchingRows[0][map.DetailCol - 1].Trim()
                            //                         : "";
                            //    taxSheet.Cells[row, map.TaxCol].Value = detailVal;
                            //    if (!string.Equals(original, detailVal, StringComparison.OrdinalIgnoreCase) && (map.TaxCol == 12 || map.TaxCol == 14))
                            //        errorGroup1 = true;
                            //    if (!string.Equals(original, detailVal, StringComparison.OrdinalIgnoreCase) && (map.TaxCol == 12 || map.TaxCol == 14))
                            //    {
                            //        // Đánh dấu ô theo giá trị từ matchingRows[0]
                            //        taxSheet.Cells[row, map.TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            //        taxSheet.Cells[row, map.TaxCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                            //        // Thêm border cho bên trái và bên phải của ô B2
                            //        taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            //        taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                            //        // (Tùy chọn) Đặt màu cho border nếu cần
                            //        taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Color.SetColor(Color.Black);
                            //        taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Color.SetColor(Color.Black);
                            //    }
                            //}
                            //foreach (var map in mappingsGroup1)
                            //{
                            //    string original = taxSheet.Cells[row, map.TaxCol].Text.Trim();
                            //    string detailVal = matchingRows[0].Length >= map.DetailCol
                            //                         ? matchingRows[0][map.DetailCol - 1].Trim()
                            //                         : "";
                            //    taxSheet.Cells[row, map.TaxCol].Value = detailVal;
                            //    if (!string.Equals(original, detailVal, StringComparison.OrdinalIgnoreCase) && (map.TaxCol == 12 || map.TaxCol == 14))
                            //        errorGroup1 = true;
                            //    if (!string.Equals(original, detailVal, StringComparison.OrdinalIgnoreCase) && (map.TaxCol == 12 || map.TaxCol == 14))
                            //    {
                            //        // Đánh dấu ô theo giá trị từ matchingRows[0]
                            //        taxSheet.Cells[row, map.TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            //        taxSheet.Cells[row, map.TaxCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                            //        // Thêm border cho bên trái và bên phải của ô B2
                            //        taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            //        taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                            //        // (Tùy chọn) Đặt màu cho border nếu cần
                            //        taxSheet.Cells[row, map.TaxCol].Style.Border.Left.Color.SetColor(Color.Black);
                            //        taxSheet.Cells[row, map.TaxCol].Style.Border.Right.Color.SetColor(Color.Black);
                            //    }
                            //}
                            // Group 2: so sánh với tổng các dòng detail, nhưng bind giá trị từ matchingRows[0]
                            //var mappingsGroup2 = new (int TaxCol, int DetailCol)[]
                            //{
                            //    (10, 19), (11, 20), (12, 23), (13, 22), (14, 24)
                            //};
                            var compareMappings = mappings.Where(x => x.TaxCol == 12 || x.TaxCol == 14).ToArray();
                            
                            double[] sums = new double[compareMappings.Length];
                            for (int i = 0; i < n; i++)
                            {
                                for (int j = 0; j < compareMappings.Where(x => x.TaxCol == 12 || x.TaxCol == 14).ToArray().Length; j++)
                                {
                                    double val = 0;
                                    double.TryParse(matchingRows[i].Length >= compareMappings[j].DetailCol ? matchingRows[i][compareMappings[j].DetailCol - 1] : "0", out val);
                                    sums[j] += val;
                                }
                            }
                            for (int j = 0; j < compareMappings.Length; j++)
                            {
                                string original = taxSheet.Cells[row, compareMappings[j].TaxCol].Text.Trim();
                                string detailVal = matchingRows[0].Length >= compareMappings[j].DetailCol
                                                     ? matchingRows[0][compareMappings[j].DetailCol - 1].Trim()
                                                     : "";
                                taxSheet.Cells[row, compareMappings[j].TaxCol].Value = detailVal; // bind từ matchingRows[0]
                                double origVal = 0;
                                double.TryParse(original, out origVal);
                                if (Math.Abs(origVal - sums[j]) > 0.0001)
                                {
                                    errorGroup2 = true;

                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                    // Thêm border cho bên trái và bên phải của ô B2
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                                    // (Tùy chọn) Đặt màu cho border nếu cần
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Style.Border.Left.Color.SetColor(Color.Black);
                                    taxSheet.Cells[row, compareMappings[j].TaxCol].Style.Border.Right.Color.SetColor(Color.Black);
                                }
                            }
                            for (int j = 0; j < mappings.Length; j++)
                            {
                                string original = taxSheet.Cells[row, mappings[j].TaxCol].Text.Trim();
                                string detailVal = matchingRows[0].Length >= mappings[j].DetailCol
                                                     ? matchingRows[0][mappings[j].DetailCol - 1].Trim()
                                                     : "";
                                taxSheet.Cells[row, mappings[j].TaxCol].Value = detailVal; // bind từ matchingRows[0]
                                if (taxSheet.Cells[row, mappings[j].TaxCol].Value is double || taxSheet.Cells[row, mappings[j].TaxCol].Value is int || taxSheet.Cells[row, mappings[j].TaxCol].Value is decimal)
                                {
                                    taxSheet.Cells[row, mappings[j].TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right; // Căn phải nếu là số
                                }
                                else
                                {
                                    taxSheet.Cells[row, mappings[j].TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Căn trái nếu là chữ
                                }
                            }

                            // --- Phần Chi tiết: chèn các dòng detail ---
                            // Với 1 dòng TaxFile khớp với n dòng detail, chèn (n - 1) dòng bên dưới dòng Summary.
                            for (int i = 1; i < n; i++)
                            {
                                taxSheet.InsertRow(row + i, 1);
                                // Group 1: copy giá trị từ dòng detail tương ứng (DetailFile: cột 4,10,11,17,18)
                                //foreach (var map in mappingsGroup1)
                                //{
                                //    string detailVal = matchingRows[i].Length >= map.DetailCol
                                //        ? matchingRows[i][map.DetailCol - 1].Trim()
                                //        : "";
                                //    taxSheet.Cells[row + i, map.TaxCol].Value = detailVal;

                                //    if (map.TaxCol == 12 || map.TaxCol == 14)
                                //    {
                                //        taxSheet.Cells[row + i, map.TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                //        taxSheet.Cells[row + i, map.TaxCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                //        // Thêm border cho bên trái và bên phải của ô B2
                                //        taxSheet.Cells[row + i, map.TaxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                //        taxSheet.Cells[row + i, map.TaxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                                //        // (Tùy chọn) Đặt màu cho border nếu cần
                                //        taxSheet.Cells[row + i, map.TaxCol].Style.Border.Left.Color.SetColor(Color.Black);
                                //        taxSheet.Cells[row + i, map.TaxCol].Style.Border.Right.Color.SetColor(Color.Black);
                                //    }
                                //}
                                // Group 2: copy trực tiếp giá trị của dòng detail tương ứng (DetailFile: cột 19,20,23,22,24)
                                foreach (var map in mappings)
                                {
                                    string detailVal = matchingRows[i].Length >= map.DetailCol
                                        ? matchingRows[i][map.DetailCol - 1].Trim()
                                        : "";
                                    taxSheet.Cells[row + i, map.TaxCol].Value = detailVal;

                                    if ((map.TaxCol == 12 || map.TaxCol == 14) && errorGroup2 == true)
                                    {
                                        taxSheet.Cells[row + i, map.TaxCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        taxSheet.Cells[row + i, map.TaxCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Red);
                                        // Thêm border cho bên trái và bên phải của ô B2
                                        taxSheet.Cells[row + i, map.TaxCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                        taxSheet.Cells[row + i, map.TaxCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                                        // (Tùy chọn) Đặt màu cho border nếu cần
                                        taxSheet.Cells[row + i, map.TaxCol].Style.Border.Left.Color.SetColor(Color.Black);
                                        taxSheet.Cells[row + i, map.TaxCol].Style.Border.Right.Color.SetColor(Color.Black);
                                    }
                                    if (taxSheet.Cells[row + i, map.TaxCol].Value is double || taxSheet.Cells[row + i, map.TaxCol].Value is int || taxSheet.Cells[row + i, map.TaxCol].Value is decimal)
                                    {
                                        taxSheet.Cells[row + i, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right; // Căn phải nếu là số
                                    }
                                    else
                                    {
                                        taxSheet.Cells[row + i, map.TaxCol].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left; // Căn trái nếu là chữ
                                    }
                                }

                                // --- Mới: Ở các dòng chi tiết, lấy giá trị ở ô 2,3,4 của dòng gốc (Summary) 
                                // để đưa vào ô 2,3,4 của dòng chi tiết.
                                taxSheet.Cells[row + i, 3].Value = taxSheet.Cells[row, 3].Value;
                                taxSheet.Cells[row + i, 4].Value = taxSheet.Cells[row, 4].Value;
                                taxSheet.Cells[row + i, 7].Value = taxSheet.Cells[row, 7].Value;
                            }

                            // Bỏ qua các dòng vừa chèn trong vòng lặp ngoài
                            row += (n - 1);
                            originalRowCount += (n - 1);
                        }
                    } // end for duyệt các dòng taxSheet

                    // --- Đánh số thứ tự tăng dần cho tất cả các dòng dữ liệu ---
                    // Chèn cột mới ở vị trí đầu để chứa STT
                    int finalRowCount = taxSheet.Dimension.Rows;
                    int order = 1;
                    for (int r = startRow + 2; r <= finalRowCount; r++)
                    {
                        taxSheet.Cells[r, 1].Value = order++;
                        // Căn giữa nội dung trong ô A1 theo chiều ngang
                        taxSheet.Cells[r, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        // Thêm border cho bên trái và bên phải của ô B2
                        taxSheet.Cells[r, 1].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        taxSheet.Cells[r, 1].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                        // (Tùy chọn) Đặt màu cho border nếu cần
                        taxSheet.Cells[r, 1].Style.Border.Left.Color.SetColor(Color.Black);
                        taxSheet.Cells[r, 1].Style.Border.Right.Color.SetColor(Color.Black);

                    }

                    // Xuất file Excel đã cập nhật
                    var stream = new MemoryStream();
                    taxPackage.SaveAs(stream);
                    stream.Position = 0;
                    return File(
                        stream,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "Updated_Tax_Report.xlsx"
                    );
                }
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
