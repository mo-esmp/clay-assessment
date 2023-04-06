using Microsoft.EntityFrameworkCore;
using SmartLock.Domain.Locks;
using SmartLock.Implementation.Data;

namespace SmartLock.Implementation.Services;

public class LockService : ILockService
{
    private readonly SmartLockDbContext _context;

    public LockService(SmartLockDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LockAccessHistoryDto>> GetLockAccessesAsync(string lockId, int page, int count)
    {
        // Using repository pattern would be better in case of hiding underlying data access
        // technology and ease of mocking in writing unit tests
        return await _context.LockAccesses
            .Where(l => l.LockId == lockId)
            .Skip((page - 1) * count)
            .Take(count)
            .Select(l => new LockAccessHistoryDto(
                l.AccessDate,
                l.AccessResult,
                $"{l.Requester.FirstName} {l.Requester.LastName}",
                l.RequestForEmployee != null
                    ? $"{l.RequestForEmployee.FirstName} {l.RequestForEmployee.LastName}"
                    : null)
            )
            .ToListAsync();
    }
}