namespace z76_backend.Models
{
    public class SaveParameter<T>
    {
        /// <summary>
        /// data save
        /// </summary>
        public T? record { get; set; }
        /// <summary>
        /// update or insert?
        /// </summary>
        public int mode { get; set; }
        public List<T>? records { get; set; }
    }
}
