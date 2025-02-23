using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using z76_backend.Enums;
using z76_backend.Models;
using z76_backend.Services;

namespace z76_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockCardController : BaseController<StockCardEntity, IStockCardService>
    {
        private readonly IStockCardService _service;

        public StockCardController(IStockCardService service) : base(service)
        {
            _service = service;
        }
    }
}
