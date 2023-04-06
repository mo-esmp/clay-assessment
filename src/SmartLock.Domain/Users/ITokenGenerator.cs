namespace SmartLock.Domain.Users;

public interface ITokenGenerator
{
    string GenerateJwtToken(string userName, string userId, IEnumerable<string> roles);
}