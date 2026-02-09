using Microsoft.AspNetCore.Mvc;
using MapaClientes.Api.Models;

namespace MapaClientes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email ou senha inválidos" });
        }

        return Ok(new
        {
            token = "mocked-jwt-token",
            email = request.Email
        });
    }
}
