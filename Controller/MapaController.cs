using Microsoft.AspNetCore.Mvc;
using MapaClientes.Api.Models;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace MapaClientes.Api.Controllers;

[ApiController]
[Route("api/mapa")]
public class MapaController : ControllerBase
{
    private static readonly List<ClienteMapa> _clientes = new();
    private static int _nextId = 1;

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(_clientes);
    }


    [HttpGet("download/{id}")]
    public IActionResult Download(int id)
    {
        var cliente = _clientes.FirstOrDefault(c => c.Id == id);

        if (cliente == null || string.IsNullOrEmpty(cliente.CaminhoArquivo))
            return NotFound();

        var caminho = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads",
            cliente.CaminhoArquivo
        );

        var bytes = System.IO.File.ReadAllBytes(caminho);

        return File(bytes, "application/octet-stream", cliente.NomeArquivo);
    }

  
   [HttpPost]
   public async Task<IActionResult> Post([FromForm] ClienteMapa model, IFormFile? arquivo)
   {
    model.Id = _nextId++;

    model.Latitude = double.Parse(
        Request.Form["Latitude"],
        CultureInfo.InvariantCulture
    );

    model.Longitude = double.Parse(
        Request.Form["Longitude"],
        CultureInfo.InvariantCulture
    );

    if (arquivo != null && arquivo.Length > 0)
    {
        var pastaUploads = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        if (!Directory.Exists(pastaUploads))
            Directory.CreateDirectory(pastaUploads);

        var nomeUnico = $"{Guid.NewGuid()}_{arquivo.FileName}";
        var caminhoCompleto = Path.Combine(pastaUploads, nomeUnico);

        using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
        {
            await arquivo.CopyToAsync(stream);
        }

        model.NomeArquivo = arquivo.FileName;
        model.CaminhoArquivo = nomeUnico;
    }

    _clientes.Add(model);

    return Ok(model);
    }

    [HttpGet("geocode")]
public async Task<IActionResult> Geocode(string endereco)
{
    using var http = new HttpClient();
    http.DefaultRequestHeaders.UserAgent.ParseAdd("MapaClientesApp/1.0");

    var tentativas = new List<string>
    {
        endereco,
        endereco.Replace("-", ","),
        endereco.Replace("SP", "São Paulo"),
        endereco.Split(",")[0] + ", São Paulo, Brasil"
    };

    foreach (var tentativa in tentativas)
    {
        var url =
            $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(tentativa)}&countrycodes=br&limit=1";

        var response = await http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            continue;

        var json = await response.Content.ReadAsStringAsync();
        var dados = JsonSerializer.Deserialize<List<NominatimResponse>>(json);

        if (dados != null && dados.Count > 0)
        {
            return Ok(new
            {
                lat = double.Parse(dados[0].lat, CultureInfo.InvariantCulture),
                lng = double.Parse(dados[0].lon, CultureInfo.InvariantCulture)
            });
        }
    }

    return NotFound("Endereço não encontrado");
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


    private class NominatimResponse
    {
        public string lat { get; set; }
        public string lon { get; set; }
    }
}