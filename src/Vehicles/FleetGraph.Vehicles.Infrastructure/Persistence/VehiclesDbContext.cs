using FleetGraph.Vehicles.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetGraph.Vehicles.Infrastructure.Persistence;

public sealed class VehiclesDbContext(DbContextOptions<VehiclesDbContext> options) : DbContext(options)
{
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("vehicles");

        modelBuilder.Entity<Vehicle>(b =>
        {
            b.ToTable("vehicles");
            b.HasKey(v => v.Id);
            b.Property(v => v.Id).ValueGeneratedOnAdd();
            b.Property(v => v.Vin).HasMaxLength(17).IsRequired();
            b.HasIndex(v => v.Vin).IsUnique();
            b.Property(v => v.RegistrationNumber).HasMaxLength(12).IsRequired();
            b.Property(v => v.Manufacturer).HasMaxLength(60).IsRequired();
            b.Property(v => v.Model).HasMaxLength(60).IsRequired();
            b.Property(v => v.OwnerEmail).HasMaxLength(200);
            b.Property(v => v.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
