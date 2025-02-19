﻿using Microsoft.AspNetCore.Mvc;
using z76_backend.Infrastructure;
using z76_backend.Models;
using Newtonsoft.Json;
using Mysqlx.Crud;

namespace z76_backend.Services
{
    public abstract class BaseService<T> : IBaseService<T> where T : class
    {
        private readonly IBaseRepository<T> _repository;
        public BaseService(IConfiguration _configuration)
        {
            _repository = new BaseRepository<T>(_configuration);
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

        public async Task<int> Update(IEnumerable<T> records, string field)
        {
            return await _repository.Update(records, field);
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
        public async Task<object> GetPagingSummaryAsync(string filters)
        {
            var filterConditions = JsonConvert.DeserializeObject<List<FilterCondition>>(filters);
            if (filterConditions != null)
            {
                return await _repository.GetPagingSummaryAsync(filterConditions);
            }
            return null;
        }
        public async Task<List<T>> GetAsync(List<FilterCondition> filters)
        {
            //var filterConditions = JsonConvert.DeserializeObject<List<FilterCondition>>(filters);
            if (filters != null)
            {
                return await _repository.GetAsync(filters);
            }
            return null;
        }
    }

}
