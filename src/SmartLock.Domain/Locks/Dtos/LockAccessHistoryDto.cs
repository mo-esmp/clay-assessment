namespace SmartLock.Domain.Locks;

public record LockAccessHistoryDto(DateTime AccessDate, LockAccessResult AccessResult, string RequesterName,
    string RequestForEmployeeName);