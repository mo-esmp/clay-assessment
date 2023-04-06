using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartLock.Domain.Locks;
using SmartLock.Domain.Users;

namespace SmartLock.Implementation.Data;

public class SmartLockDbContext : IdentityDbContext<UserEntity, RoleEntity, string, IdentityUserClaim<string>, UserRoleEntity,
    IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public SmartLockDbContext(DbContextOptions<SmartLockDbContext> options) : base(options)
    {
    }

    public DbSet<LockAccessHistoryEntity> LockAccesses { get; set; }

    public DbSet<LockEntity> Locks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartLockDbContext).Assembly);
    }
}