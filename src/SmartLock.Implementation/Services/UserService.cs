using Microsoft.AspNetCore.Identity;
using SmartLock.Domain;
using SmartLock.Domain.Users;

namespace SmartLock.Implementation.Services;

public class UserService : IUserService
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly ITokenGenerator _tokenGenerator;

    public UserService(UserManager<UserEntity> userManager, ITokenGenerator tokenGenerator)
    {
        _userManager = userManager;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<JwtTokenDto> GetJwtTokenAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user == null)
        {
            throw new DomainException("Invalid username or password.");
        }

        var result = await _userManager.CheckPasswordAsync(user, password);
        if (!result)
        {
            throw new DomainException("Invalid username or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var token = _tokenGenerator.GenerateJwtToken(user.UserName, user.Id, roles);
        return new JwtTokenDto(token);
    }
}