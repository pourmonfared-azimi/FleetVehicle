using FleetGraph.Parts.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetGraph.Parts.Infrastructure.Persistence;

public sealed class PartsDbContext(DbContextOptions<PartsDbContext> options) : DbContext(options)
{
    public DbSet<Part> Parts => Set<Part>();

    public DbSet<StockLevel> StockLevels => Set<StockLevel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("parts");

        modelBuilder.Entity<Part>(b =>
        {
            b.ToTable("parts");
            b.HasKey(p => p.Id);
            b.Property(p => p.PartNumber).HasMaxLength(30).IsRequired();
            b.HasIndex(p => p.PartNumber).IsUnique();
            b.Property(p => p.Name).HasMaxLength(120).IsRequired();
            b.Property(p => p.Category).HasConversion<string>().HasMaxLength(20);
            b.Property(p => p.UnitPrice).HasPrecision(12, 2);
        });

        modelBuilder.Entity<StockLevel>(b =>
        {
            b.ToTable("stock_levels");
            b.HasKey(s => s.Id);
            b.HasIndex(s => s.PartId).IsUnique();
            b.Ignore(s => s.NeedsReorder);
            b.HasOne<Part>().WithOne().HasForeignKey<StockLevel>(s => s.PartId);
            // Optimistic concurrency would go here (xmin on Postgres) for concurrent stock updates.
        });
    }
}
