using Microsoft.AspNetCore.Mvc;
using MapaClientes.Api.Models;

namespace MapaClientes.Api.Controllers;

[ApiController]
[Route("api/mapa")]
public class MapaController : ControllerBase
{
    // Lista em memória para teste
    private static readonly List<ClienteMapa> _clientes = new();
    // Contador para garantir IDs únicos
    private static int _nextId = 1;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_clientes);
    }

    [HttpPost]
    public IActionResult Post([FromBody] ClienteMapa model)
    {
        // Atribui o ID e incrementa para o próximo
        model.Id = _nextId++;
        
        _clientes.Add(model);

        // Retorna o objeto com o ID preenchido para o Front-end
        return Ok(model);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var cliente = _clientes.FirstOrDefault(c => c.Id == id);
        if (cliente == null) return NotFound();

        _clientes.Remove(cliente);
        return NoContent();
    }
}