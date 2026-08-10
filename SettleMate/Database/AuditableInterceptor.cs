using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SettleMate.Abstractions;

namespace SettleMate.Database;

public class AuditableInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context!;
        var entries = context.ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreateTimestamps(DateTime.UtcNow, DateTime.UtcNow);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdateTimestamps(DateTime.UtcNow);
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
