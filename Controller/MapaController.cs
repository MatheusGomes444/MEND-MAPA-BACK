using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapaClientes.Api.Data;
using MapaClientes.Api.Models;
using System.Globalization;

namespace MapaClientes.Api.Controllers;

[ApiController]
[Route("api/mapa")]
public class MapaController : ControllerBase
{
    private readonly AppDbContext _context;

    public MapaController(AppDbContext context)
    {
        _context = context;
    }

    // GET TODOS
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var clientes = await _context.Clientes.ToListAsync();
        return Ok(clientes);
    }

    // POST (SALVAR)
    [HttpPost]
    public async Task<IActionResult> Post([FromForm] IFormFile? arquivo)
    {
        var model = new ClienteMapa
        {
            Endereco = Request.Form["Endereco"],
            Cliente = Request.Form["Cliente"],
            Posto = Request.Form["Posto"],
            Equipamento = Request.Form["Equipamento"],
            Contrato = Request.Form["Contrato"],
            Observacao = Request.Form["Observacao"]
        };

        if (int.TryParse(Request.Form["Radios"], out int radios))
            model.Radios = radios;

        if (double.TryParse(Request.Form["Latitude"], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
            model.Latitude = lat;

        if (double.TryParse(Request.Form["Longitude"], NumberStyles.Any, CultureInfo.InvariantCulture, out double lng))
            model.Longitude = lng;

        if (arquivo != null && arquivo.Length > 0)
        {
            var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(pastaUploads)) Directory.CreateDirectory(pastaUploads);

            var nomeUnico = $"{Guid.NewGuid()}_{arquivo.FileName}";
            var caminhoCompleto = Path.Combine(pastaUploads, nomeUnico);

            using var stream = new FileStream(caminhoCompleto, FileMode.Create);
            await arquivo.CopyToAsync(stream);

            model.NomeArquivo = arquivo.FileName;
            model.CaminhoArquivo = nomeUnico;
        }

 try
{
    _context.Clientes.Add(model);
    await _context.SaveChangesAsync();
    return Ok(model);
}
catch (Exception ex)
{
    return StatusCode(500, new
    {
        erro = ex.Message,
        detalhe = ex.InnerException?.Message
    });
}
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    [HttpGet("test-db")]
public async Task<IActionResult> TestDb()
{
    try
    {
        await _context.Database.CanConnectAsync();
        return Ok("Conectou no banco!");
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

    // GEOCODE
    [HttpGet("geocode")]
    public async Task<IActionResult> Geocode(string endereco)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("MapaClientesApp/1.0");

        var url =
            $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(endereco)}&countrycodes=br&limit=1";

        var response = await http.GetAsync(url);
        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var dados = System.Text.Json.JsonSerializer.Deserialize<List<NominatimResponse>>(json);

        if (dados == null || dados.Count == 0) return NotFound();

        double.TryParse(dados[0].lat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat);
        double.TryParse(dados[0].lon, NumberStyles.Any, CultureInfo.InvariantCulture, out double lng);

        return Ok(new { lat, lng });
    }

    private class NominatimResponse
    {
        public string lat { get; set; } = string.Empty;
        public string lon { get; set; } = string.Empty;
    }
}