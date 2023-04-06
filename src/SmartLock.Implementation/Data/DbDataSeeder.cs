using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartLock.Domain.Locks;
using SmartLock.Domain.Users;

namespace SmartLock.Implementation.Data;

public class DbDataSeeder
{
    private readonly SmartLockDbContext _context;

    public DbDataSeeder(SmartLockDbContext context)
    {
        _context = context;
    }

    public async Task SeedDbAsync(CancellationToken ct = default)
    {
        if (await _context.Locks.AnyAsync(ct))
            return;

        var (entranceLock, storageLock) = AddLocks();
        var (user1, user2) = AddUsers();
        var (manager, employee) = AddRoles(entranceLock, storageLock);

        //_context.UserRoles.AddRange(
        //    new UserRoleEntity { RoleId = role1.Id, UserId = user1.Id },
        //    new UserRoleEntity { RoleId = role2.Id, UserId = user2.Id }
        //);

        await _context.SaveChangesAsync(ct);
    }

    private (LockEntity entranceLock, LockEntity storageLock) AddLocks()
    {
        LockEntity entranceLock = new()
        {
            Id = DefaultData.EntranceLockId,
            Name = "Entrance Lock"
        };
        LockEntity storageLock = new()
        {
            Id = DefaultData.StorageLockId,
            Name = "Storage Room"
        };

        _context.Locks.AddRange(entranceLock, storageLock);

        return (entranceLock, storageLock);
    }

    private (UserEntity user1, UserEntity user2) AddUsers()
    {
        UserEntity user1 = new()
        {
            Id = DefaultData.ManagerId,
            Email = DefaultData.ManagerEmail,
            NormalizedEmail = DefaultData.ManagerEmail.ToUpper(),
            EmailConfirmed = true,
            FirstName = "John",
            LastName = "Doe",
            UserName = DefaultData.ManagerEmail,
            NormalizedUserName = DefaultData.ManagerEmail.ToUpper(),
        };
        UserEntity user2 = new()
        {
            Id = DefaultData.EmployeeId,
            Email = DefaultData.EmployeeEmail,
            NormalizedEmail = DefaultData.EmployeeEmail.ToUpper(),
            EmailConfirmed = true,
            FirstName = "Jill",
            LastName = "Doe",
            UserName = DefaultData.EmployeeEmail,
            NormalizedUserName = DefaultData.EmployeeEmail.ToUpper()
        };

        PasswordHasher<UserEntity> ph = new();
        user1.PasswordHash = ph.HashPassword(user1, DefaultData.UserPassword);
        user2.PasswordHash = ph.HashPassword(user2, DefaultData.UserPassword);
        _context.Users.AddRange(user1, user2);

        return (user1, user2);
    }

    private (RoleEntity manager, RoleEntity employee) AddRoles(LockEntity entranceLock, LockEntity storageLock)
    {
        RoleEntity manager = new()
        {
            Id = DefaultData.ManagerRoleId,
            Name = DefaultData.ManagerRoleName,
            NormalizedName = DefaultData.ManagerRoleName.ToUpper(),
            Locks = new List<LockEntity> { entranceLock, storageLock }
        };
        RoleEntity employee = new()
        {
            Id = DefaultData.EmployeeRoleId,
            Name = DefaultData.EmployeeRoleName,
            NormalizedName = DefaultData.EmployeeRoleName.ToUpper(),
            Locks = new List<LockEntity> { entranceLock }
        };
        _context.Roles.AddRange(manager, employee);

        return (manager, employee);
    }
}