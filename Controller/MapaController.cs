using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MapaClientes.Api.Data;
using MapaClientes.Api.Models;
using System.Globalization;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authorization;

namespace MapaClientes.Api.Controllers;

[ApiController]
[Route("api/mapa")]
public class MapaController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public MapaController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // ================= GET =================
    [HttpGet]
    [Authorize]
        public async Task<IActionResult> Get()
    {
        var agora = DateTime.UtcNow;

        var clientes = await _context.Clientes
            .Where(c => c.ExpiraEm == null || c.ExpiraEm > agora)
            .ToListAsync();

        return Ok(clientes);
    }
    // ================= POST =================
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Post([FromForm] IFormFile? arquivo)
    {
        var model = new ClienteMapa
        {
            Endereco = Request.Form["Endereco"],
            Cliente = Request.Form["Cliente"],
            Posto = Request.Form["Posto"],
            Equipamento = Request.Form["Equipamento"],
            Contrato = Request.Form["Contrato"],
            Observacao = Request.Form["Observacao"],
            CriadoPor   = User.FindFirst("nome")?.Value ?? "Desconhecido"  // ✅ ADICIONE
        };

        if (int.TryParse(Request.Form["Radios"], out int radios))
            model.Radios = radios;

        if (double.TryParse(Request.Form["Latitude"], NumberStyles.Any, CultureInfo.InvariantCulture, out double lat))
            model.Latitude = lat;

        if (double.TryParse(Request.Form["Longitude"], NumberStyles.Any, CultureInfo.InvariantCulture, out double lng))
            model.Longitude = lng;

        // ================= UPLOAD SUPABASE =================
        if (arquivo != null && arquivo.Length > 0)
        {
            try
            {
                var supabaseUrl = _config["Supabase:Url"];
                var supabaseKey = _config["Supabase:Key"];
                var bucket = _config["Supabase:Bucket"];

                var nomeUnico = $"{Guid.NewGuid()}_{arquivo.FileName}";

                using var client = new HttpClient();

                // 🔐 Headers obrigatórios
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", supabaseKey);

                client.DefaultRequestHeaders.Add("apikey", supabaseKey);

                var uploadUrl = $"{supabaseUrl}/storage/v1/object/{bucket}/{nomeUnico}";

                using var fileStream = arquivo.OpenReadStream();
                using var content = new StreamContent(fileStream);

                content.Headers.ContentType =
                    new MediaTypeHeaderValue(arquivo.ContentType);

                // 🔥 IMPORTANTE: PUT (não POST)
                var response = await client.PutAsync(uploadUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    return StatusCode(500, new
                    {
                        erro = "Erro ao enviar para Supabase",
                        detalhe = erro
                    });
                }

                //  SALVA URL COMPLETA (MELHOR PRÁTICA)
                model.NomeArquivo = arquivo.FileName;
                model.CaminhoArquivo =
                    $"{supabaseUrl}/storage/v1/object/public/{bucket}/{nomeUnico}";
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    erro = "Erro no upload",
                    detalhe = ex.Message
                });
            }
        }
        // EXPIRAÇÃO
        if (!string.IsNullOrEmpty(Request.Form["ExpiraEm"]))
        {
            if (DateTime.TryParse(Request.Form["ExpiraEm"], out DateTime expira))
            {
                model.ExpiraEm = expira.ToUniversalTime();
            }
        }

        // ================= SALVAR NO BANCO =================
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

    // ================= DELETE =================
    // ================= PUT (EDITAR) =================
[HttpPut("{id}")]
[Authorize]
public async Task<IActionResult> Put(int id, [FromForm] IFormFile? arquivo)
{
    var cliente = await _context.Clientes.FindAsync(id);

    if (cliente == null)
        return NotFound(new { mensagem = $"Cliente com ID {id} não encontrado" });

    // Atualiza os campos de texto (mesmo padrão do seu POST)
    cliente.Endereco    = Request.Form["Endereco"];
    cliente.Cliente     = Request.Form["Cliente"];
    cliente.Posto       = Request.Form["Posto"];
    cliente.Equipamento = Request.Form["Equipamento"];
    cliente.Contrato    = Request.Form["Contrato"];
    cliente.Observacao  = Request.Form["Observacao"];

    if (int.TryParse(Request.Form["Radios"], out int radios))
        cliente.Radios = radios;

    if (!string.IsNullOrEmpty(Request.Form["ExpiraEm"]))
    {
        if (DateTime.TryParse(Request.Form["ExpiraEm"], out DateTime expira))
            cliente.ExpiraEm = expira.ToUniversalTime();
    }
    else
    {
        cliente.ExpiraEm = null;
    }

    // Só faz upload se mandou arquivo novo
    if (arquivo != null && arquivo.Length > 0)
    {
        try
        {
            var supabaseUrl = _config["Supabase:Url"];
            var supabaseKey = _config["Supabase:Key"];
            var bucket      = _config["Supabase:Bucket"];
            var nomeUnico   = $"{Guid.NewGuid()}_{arquivo.FileName}";

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", supabaseKey);
            http.DefaultRequestHeaders.Add("apikey", supabaseKey);

            var uploadUrl = $"{supabaseUrl}/storage/v1/object/{bucket}/{nomeUnico}";

            using var fileStream = arquivo.OpenReadStream();
            using var content    = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue(arquivo.ContentType);

            var response = await http.PutAsync(uploadUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                return StatusCode(500, new { erro = "Erro ao enviar para Supabase", detalhe = erro });
            }

            cliente.NomeArquivo    = arquivo.FileName;
            cliente.CaminhoArquivo = $"{supabaseUrl}/storage/v1/object/public/{bucket}/{nomeUnico}";
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { erro = "Erro no upload", detalhe = ex.Message });
        }
    }

    try
    {
        await _context.SaveChangesAsync();
        return Ok(cliente);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
    }
} 

    [HttpGet("test-db")]
    public async Task<IActionResult> TestDb()
    {
        try
        {
            var count = await _context.Clientes.CountAsync();
            return Ok($"Banco conectado! Total de registros: {count}");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString());
        }
    }

    // ================= GEOCODE =================
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

