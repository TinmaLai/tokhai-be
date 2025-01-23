using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly string _key = "super-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-keysuper-secret-key"; // Phải trùng với key ở Program.cs

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        // TODO: Thực hiện đăng nhập API cung cấp
        if (model.Username == "admin@gmail.com" && model.Password == "password") // Check user (dữ liệu giả)
        {
            var token = GenerateJwtToken(model.Username);
            return Ok(new { Token = token, UserId = Guid.NewGuid() });
        }
        return Unauthorized();
    }

    private string GenerateJwtToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_key);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, username) }),
            Expires = DateTime.UtcNow.AddHours(24), // Token hết hạn sau 1 giờ
            Issuer = "https://localhost:7152/api",
            Audience = "https://localhost:7152/api",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class LoginModel
{
    public string Username { get; set; }
    public string Password { get; set; }
}
