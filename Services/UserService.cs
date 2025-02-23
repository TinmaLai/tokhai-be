using System.ComponentModel.DataAnnotations;
using System.Reflection;
using z76_backend.Infrastructure;
using z76_backend.Models;

namespace z76_backend.Services
{
    public class UserService : BaseService<UserEntity>, IUserService
    {
        private readonly IConfiguration _configuration;
        private readonly IBaseRepository<UserEntity> _repo;

        public UserService(IConfiguration configuration, IBaseRepository<UserEntity> repo) : base(configuration)
        {
            _configuration = configuration;
            _repo = repo;
        }
        public async Task<List<UserDto>> GetAllUser()
        {
            var users = await _repo.GetAll();
            var userDtos = new List<UserDto>();
            users.ForEach(x =>
            {
                userDtos.Add(new UserDto()
                {
                    user_id = x.user_id,
                    username = x.username,
                    stock_manage = x.stock_manage,
                    full_name = x.full_name
                });
            });
            return userDtos;
        }
        public async Task<int> UpdateUser(List<UserDto> userDtos)
        {
            var users = new List<UserEntity>();
            userDtos.ForEach(x =>
            {
                users.Add(new UserEntity()
                {
                    user_id = x.user_id,
                    full_name = x.full_name,
                   stock_manage = x.stock_manage
                });
            });
            var updateRows = await _repo.Update(users, nameof(UserDto.user_id));
            return updateRows;
        }
    }
}
