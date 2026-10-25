using FleetGraph.Service.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetGraph.Service.Infrastructure.Persistence;

public sealed class ServiceDbContext(DbContextOptions<ServiceDbContext> options) : DbContext(options)
{
    public DbSet<Workshop> Workshops => Set<Workshop>();

    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();

    public DbSet<ServiceOrderLine> ServiceOrderLines => Set<ServiceOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("service");

        modelBuilder.Entity<Workshop>(b =>
        {
            b.ToTable("workshops");
            b.HasKey(w => w.Id);
            b.Property(w => w.Name).HasMaxLength(100).IsRequired();
            b.Property(w => w.City).HasMaxLength(60).IsRequired();
        });

        modelBuilder.Entity<ServiceOrder>(b =>
        {
            b.ToTable("service_orders");
            b.HasKey(o => o.Id);
            b.Property(o => o.Description).HasMaxLength(500).IsRequired();
            b.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            b.HasIndex(o => o.VehicleId);
            b.Ignore(o => o.IsOpen);

            b.HasOne<Workshop>()
                .WithMany()
                .HasForeignKey(o => o.WorkshopId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(o => o.Lines)
                .WithOne()
                .HasForeignKey(l => l.ServiceOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(o => o.Lines)
                .HasField("_lines")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ServiceOrderLine>(b =>
        {
            b.ToTable("service_order_lines");
            b.HasKey(l => l.Id);
            // PartId references the Parts subgraph: no foreign key across bounded contexts.
            b.HasIndex(l => l.PartId);
        });
    }
}
