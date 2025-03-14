using z76_backend.Infrastructure;
using z76_backend.Models;

namespace z76_backend.Services
{
    public interface IBaseService<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(Guid id);
        Task<int> Add(List<T> entities);
        Task<int> Update(List<T> records, string field);
        Task<int> Delete(Guid id);
        /// <summary>
        /// Lấy dữ liệu theo phân trang và filter
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="take"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<PagingResult> GetPagingAsync(string filters, int take, int limit);
        Task<object> GetPagingSummaryAsync(string filters);
        /// <summary>
        /// Lấy dữ liệu theo filter, dạng filter:
        /// [
        ///     {
        ///         Field = Tên trường,
        ///         Value = Giá trị
        ///         Operator = Dấu so sánh
        ///     }
        /// ]
        /// </summary>
        /// <param name="filters"></param>
        /// <returns></returns>
        Task<List<T>> GetAsync(List<FilterCondition> filters);
        Task<int> DeleteManyAsync(List<Guid> ids);

    }
}
