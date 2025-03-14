﻿using z76_backend.Models;

namespace z76_backend.Infrastructure
{
    public interface IBaseRepository<T>
    {
        Task<List<T>> GetAll();
        Task<T> GetById(Guid id);
        Task<int> Add(List<T> records);
        Task<int> Update(List<T> records, string field);
        Task<int> Delete(Guid id);
        Task<List<T>> GetPagingAsync(List<FilterCondition> filters, int take, int limit);
        Task<object> GetPagingSummaryAsync(List<FilterCondition> filters);
        Task<List<T>> GetAsync(List<FilterCondition> filters);
        Task<List<T>> DeleteAsync(List<FilterCondition> filters);
        Task<int> DeleteManyAsync(List<Guid> ids);
    }
}
