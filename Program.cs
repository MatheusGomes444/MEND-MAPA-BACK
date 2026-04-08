using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MapaClientes.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ================= 🔥 FIX RAILWAY + SUPABASE =================
// força uso de IPv4 (resolve Network unreachable)
AppContext.SetSwitch("System.Net.DisableIPv6", true);

// ================= PORTA (Railway) =================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ================= CONTROLLERS =================
builder.Services.AddControllers();

// ================= CORS =================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        p => p.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ================= DATABASE =================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 🔥 valida se existe
if (string.IsNullOrEmpty(connectionString))
    throw new Exception("Connection string não encontrada!");

// 🔥 LOG PRA DEBUG (pode remover depois)
Console.WriteLine("Connection String carregada!");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // 🔥 retry automático (instabilidade Railway/Supabase)
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        );
    })
);

// ================= JWT =================
var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey))
    throw new Exception("Chave JWT não encontrada");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(jwtKey)
        )
    };
});

var app = builder.Build();

// ================= MIDDLEWARE =================
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();