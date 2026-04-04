using Microsoft.EntityFrameworkCore;
using MapaClientes.Api.Models;

namespace MapaClientes.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ClienteMapa> Clientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClienteMapa>().ToTable("clientes");
            modelBuilder.Entity<Usuario>().ToTable("usuarios");
        }
    }
}