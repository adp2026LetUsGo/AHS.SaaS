// src/Cells/Xinfer/AHS.Cell.Xinfer.Infrastructure/Persistence/XinferDbContext.cs
using Microsoft.EntityFrameworkCore;
using AHS.Cell.Xinfer.Domain.Aggregates;
using AHS.Common.Infrastructure.Tenancy;

namespace AHS.Cell.Xinfer.Infrastructure.Persistence;

public sealed class XinferDbContext(DbContextOptions<XinferDbContext> options) : DbContext(options)
{
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<TemperatureZone> TemperatureZones => Set<TemperatureZone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.Entity<Shipment>(b =>
        {
            b.ToTable("shipments");
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
            b.Property(x => x.OriginLocation).IsRequired();
            b.Property(x => x.DestinationLocation).IsRequired();
        });

        modelBuilder.Entity<TemperatureZone>(b =>
        {
            b.ToTable("temperature_zones");
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).HasColumnName("tenant_id").IsRequired();
            b.Property(x => x.ZoneId).IsRequired();
        });
    }
}
