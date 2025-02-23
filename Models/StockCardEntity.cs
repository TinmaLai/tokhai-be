using System.ComponentModel.DataAnnotations;

namespace z76_backend.Models
{
    [TableName("StockCard")]
    public class StockCardEntity
    {
        [Key]
        public Guid id { get; set; }
        public string id_kho { get; set; }
        public string ten_kho { get; set; }
        public string ma_hang { get; set; }
        public string ten_hang { get; set; }
        public int month_of_data { get; set; }
        public int year_of_data { get; set; }
        public string nguoi_duyet { get; set; }
        public DateTime thoi_gian_gui { get; set; }
        public bool? trang_thai_duyet { get; set; }
    }
}
