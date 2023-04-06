namespace SmartLock.Domain.Locks;

public interface ILockService
{
    Task<IEnumerable<LockAccessHistoryDto>> GetLockAccessesAsync(string lockId, int page, int count);
}