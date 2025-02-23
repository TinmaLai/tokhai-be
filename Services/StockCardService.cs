using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using z76_backend.Infrastructure;
using z76_backend.Models;

namespace z76_backend.Services
{
    public class StockCardService : BaseService<StockCardEntity>, IStockCardService
    {
        private readonly IConfiguration _configuration;
        private readonly IBaseRepository<StockCardEntity> _repository;

        public StockCardService(IConfiguration configuration, IBaseRepository<StockCardEntity> repository) : base(configuration)
        {
            _configuration = configuration;
            _repository = repository;
        }
    }
}
