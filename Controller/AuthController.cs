using Microsoft.AspNetCore.Mvc;
using MapaClientes.Api.Models;
using MapaClientes.Api.Data;

namespace MapaClientes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    // ✅ CONSTRUTOR FICA AQUI (fora dos métodos)
    public AuthController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized(new { message = "Usuário não encontrado" });

        bool senhaValida = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!senhaValida)
            return Unauthorized(new { message = "Senha inválida" });

        return Ok(new
        {
            token = "ainda-fake",
            email = user.Email
        });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] LoginRequest request)
    {
        if (_db.Users.Any(u => u.Email == request.Email))
            return BadRequest(new { message = "Usuário já existe" });

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = hash
        };

        _db.Users.Add(user);
        _db.SaveChanges();

        return Ok(new { message = "Usuário criado com sucesso" });
    }
}