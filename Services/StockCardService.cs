using z76_backend.Models;

namespace z76_backend.Services
{
    public class StockCardService : BaseService<StockCardEntity>, IStockCardService
    {
        private readonly IConfiguration _configuration;
        public StockCardService(IConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }
    }
}
