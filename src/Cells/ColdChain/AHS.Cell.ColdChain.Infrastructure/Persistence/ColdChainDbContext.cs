// src/Cells/ColdChain/AHS.Cell.ColdChain.Infrastructure/Persistence/ColdChainDbContext.cs
using Microsoft.EntityFrameworkCore;
using AHS.Cell.ColdChain.Domain.Aggregates;
using AHS.Common.Infrastructure.Tenancy;

namespace AHS.Cell.ColdChain.Infrastructure.Persistence;

public sealed class ColdChainDbContext(DbContextOptions<ColdChainDbContext> options) : DbContext(options)
{
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<TemperatureZone> TemperatureZones => Set<TemperatureZone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
