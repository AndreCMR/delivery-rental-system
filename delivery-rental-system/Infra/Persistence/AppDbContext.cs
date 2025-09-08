using delivery_rental_system.Domain.Entities;
using delivery_rental_system.Domain.Entities.Rental;
using Microsoft.EntityFrameworkCore;
namespace delivery_rental_system.Infra.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Motorcycle> Motorcycles { get; set; }

    public DbSet<Year2024MotorcycleNotification> Year2024MotorcycleNotifications { get; set; }

    public DbSet<DeliveryMan> DeliveryMan { get; set; }
    public DbSet<Rental> Rental { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties()
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }
        }
    }
}