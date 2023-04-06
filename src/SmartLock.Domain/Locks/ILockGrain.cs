namespace SmartLock.Domain.Locks;

/// <summary>
///   Lock grain contains all logic related a physical lock Extends the <see cref="IGrainWithStringKey"/>
/// </summary>
/// <seealso cref="IGrainWithStringKey"/>
public interface ILockGrain : IGrainWithStringKey
{
    /// <summary>
    ///   Requests to the open the lock remotely for another employee asynchronous.
    /// </summary>
    /// <param name="requesterId">The requester identifier.</param>
    /// <param name="requesterRoles">The requester roles.</param>
    /// <param name="requestForEmployeeId">The access request for employee identifier (optional).</param>
    /// <returns>Task&lt;LockAccessResultDto&gt;.</returns>
    Task<LockAccessResultDto> RequestAccessAsync(
        string requesterId,
        IEnumerable<string> requesterRoles,
        string requestForEmployeeId = null);
}