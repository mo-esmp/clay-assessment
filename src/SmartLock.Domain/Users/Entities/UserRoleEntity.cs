using Microsoft.AspNetCore.Identity;

namespace SmartLock.Domain.Users;

public class UserRoleEntity : IdentityUserRole<string>
{
    public UserEntity User { get; set; }

    public RoleEntity Role { get; set; }
}