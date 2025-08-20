using Microsoft.AspNetCore.Mvc;
using Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace API.Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Tentativa de login via API Gateway para o usuário: {Username}", request.Username);
            
            // Simulação de autenticação. Em produção, verificar contra um banco de dados.
            if (request.Username == "admin" && request.Password == "123456")
            {
                var token = GenerateJwtToken(request.Username);
                _logger.LogInformation("Login bem-sucedido via API Gateway para o usuário: {Username}", request.Username);
                return Ok(new { token });
            }

            _logger.LogWarning("Falha no login via API Gateway para o usuário: {Username}", request.Username);
            return Unauthorized(new { message = "Credenciais inválidas" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante o login via API Gateway para o usuário: {Username}", request.Username);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    private string GenerateJwtToken(string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("your_super_secret_key_32_chars_long");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "User"),
                new Claim("sub", "1")
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = "EcommerceMicroservices",
            Audience = "EcommerceUsers",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}