using Microsoft.AspNetCore.Mvc;
using z76_backend.Infrastructure;
using z76_backend.Models;
using Newtonsoft.Json;

namespace z76_backend.Services
{
    public abstract class BaseService<T> : IBaseService<T> where T : class
    {
        private readonly BaseRepository<T> _repository;
        public BaseService(IConfiguration _configuration)
        {
            _repository = new BaseRepository<T>(_configuration.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _repository.GetAll();
        }

        public async Task<T> GetById(Guid id)
        {
            return await _repository.GetById(id);
        }

        public async Task<int> Add(T entity)
        {
            return await _repository.Add(entity);
        }

        public async Task<int> Update(IEnumerable<T> records)
        {
            return await _repository.Update(records);
        }

        public async Task<int> Delete(Guid id)
        {
            return await _repository.Delete(id);
        }
        public async Task<PagingResult> GetPagingAsync(string filters, int take, int limit)
        {
            var result = new PagingResult();
            var filterConditions = JsonConvert.DeserializeObject<List<FilterCondition>>(filters);
            if(filterConditions != null)
            {
                result.Data = await _repository.GetPagingAsync(filterConditions, take, limit);
            }
            return result;
        }
    }

}
