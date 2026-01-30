using Microsoft.AspNetCore.Mvc;
using MapaClientes.Api.Models;

namespace MapaClientes.Api.Controllers;

[ApiController]
[Route("api/mapa")]
public class MapaController : ControllerBase
{
    private static readonly List<ClienteMapa> _clientes = new();

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_clientes);
    }

    [HttpPost]
    public IActionResult Post([FromBody] ClienteMapa model)
    {
        model.Id = _clientes.Count + 1;
        _clientes.Add(model);
        return Ok(model);
    }
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var cliente = _clientes.FirstOrDefault(c => c.Id == id);

        if (cliente == null)
            return NotFound();

        _clientes.Remove(cliente);
        return NoContent();
    }

}
