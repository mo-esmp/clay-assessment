using Microsoft.AspNetCore.Identity;
using SmartLock.Domain.Locks;

namespace SmartLock.Domain.Users;

public class RoleEntity : IdentityRole
{
    public ICollection<UserRoleEntity> UserRoles { get; set; }

    public ICollection<LockEntity> Locks { get; set; }
}