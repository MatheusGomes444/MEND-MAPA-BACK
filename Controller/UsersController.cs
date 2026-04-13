using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapaClientes.Api.Data;
using System.Security.Claims;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    private async Task<Usuario?> GetUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return null;

        return await _context.Usuarios.FindAsync(int.Parse(userId));
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
    {
        var user = await GetUser();
        if (user == null) return Unauthorized();

        user.Nome = dto.Nome;
        user.Email = dto.Email;

        await _context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var user = await GetUser();
        if (user == null) return Unauthorized();

        if (user.SenhaHash != dto.SenhaAtual)
            return BadRequest("Senha incorreta");

        user.SenhaHash = dto.NovaSenha;

        await _context.SaveChangesAsync();

        return Ok();
    }
}

public class UpdateUserDto
{
    public string Nome { get; set; }
    public string Email { get; set; }
}

public class ChangePasswordDto
{
    public string SenhaAtual { get; set; }
    public string NovaSenha { get; set; }
}