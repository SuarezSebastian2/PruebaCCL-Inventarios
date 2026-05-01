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
            e.ToTable("productos");
            e.HasKey(p => p.Id);
            e.Property(p => p.Nombre).HasMaxLength(200).IsRequired();
            e.Property(p => p.Cantidad).IsRequired();
        });
    }
}
