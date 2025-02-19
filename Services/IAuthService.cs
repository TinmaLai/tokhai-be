using z76_backend.Models;

namespace z76_backend.Services
{
    public interface IAuthService
    {
        public string GenerateAccessToken(UserEntity user);
        public (string Token, DateTime Expires, DateTime Created) GenerateRefreshToken();
    }
}
