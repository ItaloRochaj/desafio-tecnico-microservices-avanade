using Microsoft.AspNetCore.Mvc;
using Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace API.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly TokenService _tokenService;

    public AuthController(ILogger<AuthController> logger, TokenService tokenService)
    {
        _logger = logger;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest(new { message = "Usuário e senha são obrigatórios." });
        }

        if (request.Username == "admin" && request.Password == "123456")
        {
            var token = _tokenService.GenerateToken(request.Username);
            return Ok(new { token });
        }

        return Unauthorized(new { message = "Credenciais inválidas" });
    }

}