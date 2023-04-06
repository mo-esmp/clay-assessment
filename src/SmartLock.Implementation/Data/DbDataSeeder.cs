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
        var (managerUser, employeeUser) = AddUsers();
        var (managerRole, employeeRole) = AddRoles(entranceLock, storageLock);

        _context.UserRoles.AddRange(
            new UserRoleEntity { RoleId = managerRole.Id, UserId = managerUser.Id },
            new UserRoleEntity { RoleId = employeeRole.Id, UserId = employeeUser.Id }
        );

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

    private (UserEntity manager, UserEntity employee) AddUsers()
    {
        UserEntity manager = new()
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
        UserEntity employee = new()
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
        manager.PasswordHash = ph.HashPassword(manager, DefaultData.UserPassword);
        employee.PasswordHash = ph.HashPassword(employee, DefaultData.UserPassword);
        _context.Users.AddRange(manager, employee);

        return (manager, employee);
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