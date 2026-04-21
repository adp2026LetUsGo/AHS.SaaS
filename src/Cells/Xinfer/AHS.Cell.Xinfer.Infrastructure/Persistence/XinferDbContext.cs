using Microsoft.EntityFrameworkCore;
using AHS.Cell.Xinfer.Domain.Aggregates;
using AHS.Cell.Xinfer.Application.Ports;
using AHS.Cell.Xinfer.Application.Persistence.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace AHS.Cell.Xinfer.Infrastructure.Persistence;

public sealed class XinferDbContext(DbContextOptions<XinferDbContext> options) 
    : DbContext(options), IXinferDbContext
{
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<TemperatureZone> TemperatureZones => Set<TemperatureZone>();
    public DbSet<ModelVersion> Models => Set<ModelVersion>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var aggregates = ChangeTracker.Entries<AHS.Common.Domain.AggregateRoot>()
            .Where(x => x.Entity.UncommittedEvents.Count != 0)
            .Select(x => x.Entity)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var evt in aggregate.UncommittedEvents)
            {
                var message = new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    OccurredAt = DateTimeOffset.UtcNow,
                    EventType = evt.GetType().Name,
                    // Native AOT safe serialization using Source Generation
                    PayloadJson = System.Text.Json.JsonSerializer.Serialize(evt, evt.GetType(), AHS.Cell.Xinfer.Application.XinferJsonContext.Default)
                };
                OutboxMessages.Add(message);
            }
            aggregate.ClearUncommitted();
        }

        return await base.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public Task<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) 
        => Database.BeginTransactionAsync(ct);

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

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("outbox_messages");
            b.HasKey(x => x.Id);
            b.HasQueryFilter(x => x.ProcessedAt == null); // Only pending by default
        });

        modelBuilder.Entity<ModelVersion>(b =>
        {
            b.ToTable("model_versions");
            b.HasKey(x => x.Id);
        });
    }
}
