using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using z76_backend.Controllers;
using z76_backend.Enums;
using z76_backend.Infrastructure;
using z76_backend.Models;
using z76_backend.Services;
using ZstdSharp.Unsafe;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBaseRepository<RefreshTokenEntity> _repo;
    private readonly IConfiguration _configuration;

    public AuthController(IServiceProvider serviceProvider, IAuthService service, IBaseRepository<RefreshTokenEntity> refreshTokenRepository, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _authService = _serviceProvider.GetRequiredService<IAuthService>();
        _userService = _serviceProvider.GetRequiredService<IUserService>();
        _repo = refreshTokenRepository;
        _configuration = configuration;
    }

    // Đăng ký người dùng mới
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterParam request)
    {
        var checkUserExists = await _userService.GetAsync(new List<FilterCondition>()
        {
            new FilterCondition()
            {
                Field = nameof(UserEntity.username),
                Value = request.username,
                Operator = FilterOperator.Equal
            }
        });
        if (checkUserExists?.Count == 0)
        {
            var user = new UserEntity
            {
                user_id = Guid.NewGuid(),
                username = request.username,
                password = ComputeHash(request.password),
                full_name = request.full_name,
                stock_manage = request.stock_manage
            };
            var addUserStatus = await _userService.Add(user);
            return Ok(new
            {
                Data = user,
                Message = "Tạo mới người dùng thành công"
            });
        } else
        {
            return BadRequest("Tài khoản đã tồn tại trong hệ thống!");
        }
        
    }
    // Đăng nhập
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginParam request)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationMinutes = jwtSettings.GetValue<int>("AccessTokenExpirationMinutes");

        var users = await _userService.GetAsync(new List<FilterCondition>()
        {
            new FilterCondition()
            {
                Field = nameof(UserEntity.username),
                Value = request.username,
                Operator = FilterOperator.Equal
            }
        });
        if (users == null || users?.Count == 0 || users[0].password != ComputeHash(request.password))
        {
            return Unauthorized(new
            {
                Message = "Sai tài khoản hoặc mật khẩu"
            });
        }

        // Tạo access token
        var accessToken = _authService.GenerateAccessToken(new UserEntity { user_id = users[0].user_id, username = users[0].username });
        // Tạo refresh token
        var refreshTokenData = _authService.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshTokenEntity
        {
            user_id = users[0].user_id,
            token = refreshTokenData.Token,
            expires = refreshTokenData.Expires,
            created = refreshTokenData.Created,
            is_revoked = false
        };

        await _repo.Add(refreshTokenEntity);

        return Ok(new
        {
            user_id = users[0].user_id,
            username = users[0].username,
            full_name = users[0].full_name,
            stock_manage = users[0].stock_manage,
            AccessToken = accessToken,
            RefreshToken = refreshTokenData.Token,
            expires_in = expirationMinutes
        });
    }
    // Refresh token
    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh([FromBody] RefreshParam request)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var expirationMinutes = jwtSettings.GetValue<int>("AccessTokenExpirationMinutes");
        var tokens = await _repo.GetAsync(new List<FilterCondition>()
        {
            new FilterCondition()
            {
                Field = nameof(RefreshTokenEntity.token),
                Value = request.RefreshToken,
                Operator = FilterOperator.Equal
            }
        });
        if (tokens == null || tokens[0].is_revoked == true || tokens[0].expires < DateTime.UtcNow)
        {
            return Unauthorized("Invalid or expired refresh token.");
        }
        tokens.ForEach(x => x.is_revoked = true);
        // Thu hồi refresh token cũ
        await _repo.Update(tokens, "token");

        // Lấy thông tin user theo Id
        var user = await _userService.GetById(tokens[0].user_id);
        if (user == null)
        {
            return Unauthorized("User not found.");
        }

        var newAccessToken = _authService.GenerateAccessToken(new UserEntity { user_id = user.user_id, username = user.username });
        var newRefreshTokenData = _authService.GenerateRefreshToken();

        var newRefreshTokenEntity = new RefreshTokenEntity
        {
            user_id = user.user_id,
            token = newRefreshTokenData.Token,
            expires = newRefreshTokenData.Expires,
            created = newRefreshTokenData.Created,
            is_revoked = false
        };

        await _repo.Add(newRefreshTokenEntity);

        return Ok(new
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenData.Token,
            expire_ins = expirationMinutes
        });
    }
    // Đăng xuất: Thu hồi refresh token
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutParam request)
    {
        var refreshTokens = await _repo.GetAsync(new List<FilterCondition>()
        {
            new FilterCondition()
            {
                Field = nameof(RefreshTokenEntity.token),
                Value = request.RefreshToken,
                Operator = FilterOperator.Equal
            }
        });
        var currentRefreshToken = refreshTokens.FirstOrDefault();
        await _repo.DeleteAsync(new List<FilterCondition>()
        {
            new FilterCondition()
            {
                Field = nameof(RefreshTokenEntity.user_id),
                Value = currentRefreshToken?.user_id ?? Guid.Empty,
                Operator = FilterOperator.Equal
            },
            new FilterCondition()
            {
                Field = nameof(RefreshTokenEntity.token),
                Value = request.RefreshToken,
                Operator = FilterOperator.Equal
            }
        });
        //await _repo.Update(new List<RefreshTokenEntity>()
        //{
        //    new RefreshTokenEntity()
        //    {
        //        user_id = currentRefreshToken?.user_id ?? Guid.Empty,
        //        created = currentRefreshToken?.created ?? DateTime.Now,
        //        expires = currentRefreshToken?.expires ?? DateTime.Now,
        //        token = request.RefreshToken,
        //        is_revoked = true
        //    }
        //}, "token");
        return Ok("Logged out successfully.");
    }

    // Hàm băm mật khẩu (sử dụng SHA256)
    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}
