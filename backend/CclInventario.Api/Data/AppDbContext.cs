using CclInventario.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CclInventario.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("productos", "public");
            e.HasKey(p => p.Id);
            // PostgreSQL sin comillas usa minúsculas; el SQL manual y PG crean id/nombre/cantidad.
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            e.Property(p => p.Cantidad).HasColumnName("cantidad").IsRequired();
        });
    }
}
