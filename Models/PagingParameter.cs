namespace z76_backend.Models
{
    public class PagingParameter
    {
        public string filters { get; set; }
        public int take { get; set; }
        public int limit { get; set; }
        public int type { get; set; }
    }
}
