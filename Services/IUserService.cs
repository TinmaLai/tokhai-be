using Microsoft.AspNetCore.Mvc;
using z76_backend.Models;

namespace z76_backend.Services
{
    public interface IUserService : IBaseService<UserEntity>
    {
        Task<List<UserDto>> GetAllUser();
        Task<int> UpdateUser(List<UserDto> userDtos);
    }
}
