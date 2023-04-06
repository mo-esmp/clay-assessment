using Microsoft.EntityFrameworkCore;
using SmartLock.Domain;
using SmartLock.Domain.Locks;

namespace SmartLock.Implementation.Grains;

public class LockGrain : GrainBase, ILockGrain
{
    private LockEntity _lock;
    private readonly ILockConnector _lockConnector;

    // Lock access list can be stored in grain state or pushed to a queue
    private readonly List<LockAccessHistoryEntity> _lockAccesses;

    public LockGrain(IServiceProvider serviceProvider, ILockConnector lockConnector) : base(serviceProvider)
    {
        _lockAccesses = new(50);
        _lockConnector = lockConnector;
    }

    public Task<LockAccessResultDto> RequestAccessAsync(
        string requesterId,
        IEnumerable<string> requesterRoles,
        string requestForEmployeeId)
    {
        if (_lock == null)
            return Task.FromResult(new LockAccessResultDto(LockAccessResult.Denied));

        var hasAccess = _lock.RolesAccesses.Any(r => requesterRoles.Contains(r.Name))
            ? LockAccessResult.Granted
            : LockAccessResult.Denied;

        _lockAccesses.Add(LockAccessHistoryEntity.Create(_lock.Id, requesterId, hasAccess, requestForEmployeeId));
        LockAccessResultDto result = new(hasAccess);
        _lockConnector.SendMessageAsync(_lock.Id, result);

        return Task.FromResult(result);
    }

    public override async Task OnActivateAsync(CancellationToken ct)
    {
        var lockId = this.GetPrimaryKeyString();
        await using var context = GetDbContext();
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        _lock = await context.Locks
            .Include(u => u.RolesAccesses)
            .FirstOrDefaultAsync(u => u.Id == lockId, ct);

        if (_lock != null)
            RegisterTimer(SaveLockAccessAsync, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

        await base.OnActivateAsync(ct);
    }

    private async Task SaveLockAccessAsync(object data)
    {
        if (_lockAccesses.Count == 0)
            return;

        var count = _lockAccesses.Count;

        await using var context = GetDbContext();
        context.ChangeTracker.AutoDetectChangesEnabled = false;
        context.LockAccesses.AddRange(_lockAccesses.Take(count));
        await context.SaveChangesAsync();

        _lockAccesses.RemoveRange(0, count);
    }
}