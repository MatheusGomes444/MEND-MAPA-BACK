using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using MapaClientes.Api.Data;

public class ExpiracaoService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public ExpiracaoService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var agora = DateTime.UtcNow;

                var expirados = await context.Clientes
                    .Where(c => c.ExpiraEm != null && c.ExpiraEm <= agora)
                    .ToListAsync();

                if (expirados.Any())
                {
                    context.Clientes.RemoveRange(expirados);
                    await context.SaveChangesAsync();
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}