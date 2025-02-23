using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using z76_backend.Models;
using z76_backend.Services;

namespace z76_backend.Controllers
{
    public class UserController : BaseController<UserEntity, IUserService>
    {
        private readonly IUserService _service;

        public UserController(IUserService service) : base(service)
        {
            _service = service;
        }
        [Authorize]
        [HttpGet("list")]
        public async Task<IActionResult> GetAllUser()
        {
            var result = await _service.GetAllUser();
            return Ok(result);
        }
        [HttpPost("updateUser")]
        public async Task<IActionResult> UpdateUser(List<UserDto> userDtos)
        {
            var result = await _service.UpdateUser(userDtos);
            return Ok(result);
        }
    }
}
