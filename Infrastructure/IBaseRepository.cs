using z76_backend.Models;

namespace z76_backend.Infrastructure
{
    public interface IBaseRepository<T>
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(Guid id);
        Task<int> Add(T record);
        Task<int> Update(IEnumerable<T> records, string field);
        Task<int> Delete(Guid id);
        Task<List<T>> GetPagingAsync(List<FilterCondition> filters, int take, int limit);
        Task<object> GetPagingSummaryAsync(List<FilterCondition> filters);
        Task<List<T>> GetAsync(List<FilterCondition> filters);
    }
}
