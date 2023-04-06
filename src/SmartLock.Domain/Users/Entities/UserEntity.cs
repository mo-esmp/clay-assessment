using Microsoft.AspNetCore.Identity;

namespace SmartLock.Domain.Users;

public class UserEntity : IdentityUser
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public ICollection<UserRoleEntity> UserRoles { get; set; }
}