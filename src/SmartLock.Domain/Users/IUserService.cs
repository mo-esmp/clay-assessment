using Orleans.Services;

namespace SmartLock.Domain.Users;

public interface IUserService : IGrainService
{
    public Task<JwtTokenDto> GetJwtTokenAsync(string username, string password);
}