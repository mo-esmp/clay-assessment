using SmartLock.Domain.Users;

namespace SmartLock.Domain.Locks;

public class LockAccessHistoryEntity
{
    public int Id { get; set; }

    public DateTime AccessDate { get; set; }

    public LockAccessResult AccessResult { get; set; }

    public string LockId { get; set; }
    public LockEntity Lock { get; set; }

    public string RequesterId { get; set; }
    public UserEntity Requester { get; set; }

    public string? RequestForEmployeeId { get; set; }
    public UserEntity? RequestForEmployee { get; set; }

    public static LockAccessHistoryEntity Create(string lockId, string requesterId, LockAccessResult accessResult,
        string requestForEmployeeId = null)
        => new()
        {
            LockId = lockId,
            AccessDate = DateTime.UtcNow,
            AccessResult = accessResult,
            RequesterId = requesterId,
            RequestForEmployeeId = requestForEmployeeId
        };
}