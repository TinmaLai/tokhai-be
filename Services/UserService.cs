using z76_backend.Models;

namespace z76_backend.Services
{
    public class UserService : BaseService<UserEntity>, IUserService
    {
        private readonly IConfiguration _configuration;
        public UserService(IConfiguration configuration) : base(configuration)
        {
            _configuration = configuration;
        }
    }
}
