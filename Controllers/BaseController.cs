using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Eventing.Reader;
using z76_backend.Enums;
using z76_backend.Infrastructure;
using z76_backend.Models;
using z76_backend.Services;

namespace z76_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<T, TService> : ControllerBase where TService : IBaseService<T> where T : class
    {
        private readonly TService _service;

        public BaseController(TService service)
        {
            _service = service;
        }
        //[Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<T>>> GetAll()
        {
            var result = await _service.GetAll();
            return Ok(result);
        }
        //[Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<T>> GetById(Guid id)
        {
            var item = await _service.GetById(id);
            return item == null ? NotFound() : Ok(item);
        }
        //[Authorize]
        //[HttpPost]
        //public async Task<IActionResult> SaveEntity(SaveParameter<T> param)
        //{
        //    switch (param.mode)
        //    {
        //        case (int)ModeEnum.ADD:
        //            return Ok(await _service.Add(param.record));
        //        case (int)ModeEnum.UPDATE:
        //            return Ok(await _service.Update(param.records));
        //        default:
        //            return Ok(false);
        //    }
        //}
        //[Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var data = await _service.Delete(id);
            return Ok(data);
        }
        //[Authorize]
        [HttpPost("Paging")]
        public async Task<IActionResult> GetPaging(PagingParameter param)
        {
            switch (param.type)
            {
                case (int)PagingTypeEnum.Data:
                    var data = await _service.GetPagingAsync(param.filters, param.take, param.limit);
                    return Ok(data);
                case (int)PagingTypeEnum.Summary:
                    var summaryData = await _service.GetPagingSummaryAsync(param.filters);
                    return Ok(summaryData);
                default:
                    return Ok();
            }
            
        }
    }

}
