using Microsoft.EntityFrameworkCore;
using SmartLock.Domain.Users;

namespace SmartLock.Implementation.Grains;

public class UserGrain : GrainBase, IUserGrain
{
    private UserEntity _user;

    public UserGrain(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    public Task<UserEntity> GetUserAsync(string userId)
    {
        return _user == null
            ? Task.FromResult<UserEntity>(null)
            : Task.FromResult(_user);
    }

    public override async Task OnActivateAsync(CancellationToken ct)
    {
        var userId = this.GetPrimaryKeyString();

        await using var context = GetDbContext();
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        _user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        await base.OnActivateAsync(ct);
    }
}