using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace z76_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelDataController : Controller
    {
        [HttpPost]
        public IActionResult InsertExcel(List<List<dynamic>> param)
        {
            // Đặt LicenseContext
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Đường dẫn đến file Excel
            var filePath = "D:/Code/z76-backend/maubangke.xlsx";

            // Mở file Excel
            using (var package = new ExcelPackage(filePath))
            {
                // Get the worksheet
                var worksheet = package.Workbook.Worksheets["Sheet1"]; // Get the first worksheet

                // Insert a row at position 19
                int rowToInsert = 19;
                for (int i = 0; i < param.Count; i++)
                {
                    worksheet.InsertRow(rowToInsert, 1); // Insert 1 row at row 19
                    int indexCol = 1;
                    for (int j = 0; j < param[i].Count; j++) 
                    {
                        // Lấy giá trị từ param[i][j] và xác định kiểu dữ liệu tự động
                        var cellValue = param[i][j];

                        // Xác định kiểu dữ liệu và set giá trị vào ô tương ứng
                        if (cellValue is int)
                        {
                            worksheet.Cells[rowToInsert, indexCol].Value = (int)cellValue;
                        }
                        else if (cellValue is decimal)
                        {
                            worksheet.Cells[rowToInsert, indexCol].Value = (decimal)cellValue;
                        }
                        else if (cellValue is string)
                        {
                            worksheet.Cells[rowToInsert, indexCol].Value = (string)cellValue;
                        }
                        else if (cellValue is DateTime)
                        {
                            worksheet.Cells[rowToInsert, indexCol].Value = (DateTime)cellValue;
                        }
                        else
                        {
                            worksheet.Cells[rowToInsert, indexCol].Value = cellValue?.ToString(); // Nếu kiểu khác, chuyển sang chuỗi
                        }
                        worksheet.Cells[rowToInsert,indexCol].Style.Border.Top.Style = ExcelBorderStyle.Thin;      // Viền trên
                        worksheet.Cells[rowToInsert,indexCol].Style.Border.Left.Style = ExcelBorderStyle.Thin;     // Viền trái
                        worksheet.Cells[rowToInsert,indexCol].Style.Border.Right.Style = ExcelBorderStyle.Thin;    // Viền phải
                        worksheet.Cells[rowToInsert,indexCol].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;   // Viền dưới

                        worksheet.Cells[rowToInsert,indexCol].Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        worksheet.Cells[rowToInsert,indexCol].Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        worksheet.Cells[rowToInsert,indexCol].Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                        worksheet.Cells[rowToInsert, indexCol].Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                        indexCol = indexCol + 1;
                    }
                    rowToInsert++;
                }
                // Save the changes
                package.Save();
            }

            Console.WriteLine("Thêm dòng thành công!");
            return Ok();
        }
    }
}
