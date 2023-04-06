namespace SmartLock.Domain.Users;

public interface IUserGrain : IGrainWithStringKey
{
    Task<UserEntity> GetUserAsync(string userId);
}