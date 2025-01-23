namespace z76_backend.Models
{
    public class FilterCondition
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
        public List<FilterCondition> Ors { get; set; }
        public string Alias { get; set; }
    }
}
