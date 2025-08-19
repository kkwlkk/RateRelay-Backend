using System.Reflection;
using Microsoft.EntityFrameworkCore;
using RateRelay.Domain.Attributes;
using RateRelay.Domain.Entities;
using RateRelay.Infrastructure.DataAccess.Attributes;
using RateRelay.Infrastructure.DataAccess.Converters;

namespace RateRelay.Infrastructure.DataAccess.Context;

public class RateRelayDbContext(DbContextOptions<RateRelayDbContext> options) : DbContext(options)
{
    private bool _allowHardDeletes = false;

    public void SetHardDeleteMode(bool allowHardDeletes)
    {
        _allowHardDeletes = allowHardDeletes;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var baseEntityType = typeof(BaseEntity);
        var entitiesToRegister = Assembly.GetAssembly(baseEntityType)
            ?.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && baseEntityType.IsAssignableFrom(t));

        if (entitiesToRegister is not null)
        {
            foreach (var entity in entitiesToRegister)
            {
                modelBuilder.Entity(entity);
            }
        }

        ApplyEncryptionToProperties(modelBuilder);
        ApplySoftDeleteFilters(modelBuilder);
    }

    private static void ApplyEncryptionToProperties(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes();
        
        foreach (var entityType in entityTypes)
        {
            var encryptedProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(string) && 
                           p.GetCustomAttribute<EncryptedAttribute>() != null);

            foreach (var property in encryptedProperties)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion<EncryptedStringConverter>();
            }
        }
    }

    private void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType)) continue;
            if (entityType.ClrType.GetCustomAttribute<ExcludeFromSoftDeleteAttribute>() is not null) continue;

            var method = typeof(RateRelayDbContext).GetMethod(nameof(ApplySoftDeleteFilter),
                    BindingFlags.NonPublic | BindingFlags.Static)
                ?.MakeGenericMethod(entityType.ClrType);

            method?.Invoke(null, [modelBuilder]);
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.DateCreatedUtc = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.DateModifiedUtc = DateTime.UtcNow;
                    break;
                case EntityState.Deleted when !_allowHardDeletes:
                    entry.Entity.DateDeletedUtc = DateTime.UtcNow;
                    entry.State = EntityState.Modified;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static void ApplySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => !e.DateDeletedUtc.HasValue);
    }
}