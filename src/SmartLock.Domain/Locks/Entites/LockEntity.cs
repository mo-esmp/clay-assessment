using SmartLock.Domain.Users;

namespace SmartLock.Domain.Locks;

public class LockEntity
{
    public string Id { get; set; }

    public string Name { get; set; }

    public ICollection<LockAccessHistoryEntity> LockAccesses { get; set; }

    public ICollection<RoleEntity> RolesAccesses { get; set; }
}