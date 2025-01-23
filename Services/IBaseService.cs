using z76_backend.Infrastructure;
using z76_backend.Models;

namespace z76_backend.Services
{
    public interface IBaseService<T> where T : class
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> GetById(Guid id);
        Task<int> Add(T entity);
        Task<int> Update(IEnumerable<T> records);
        Task<int> Delete(Guid id);
        /// <summary>
        /// Lấy dữ liệu theo phân trang và filter
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="take"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<PagingResult> GetPagingAsync(string filters, int take, int limit);
    }
}
