using System.ComponentModel.DataAnnotations;

namespace z76_backend.Models
{
    [TableName("StockCard")]
    public class StockCardEntity
    {
        [Key]
        public Guid id { get; set; }
        public string ma_kho { get; set; }
        public string ten_kho { get; set; }
        public string ma_hang { get; set; }
        public string ten_hang { get; set; }
        public bool? trang_thai_doi_chieu { get; set; }
    }
}
