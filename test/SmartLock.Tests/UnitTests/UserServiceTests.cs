using Microsoft.AspNetCore.Identity;
using Moq;
using SmartLock.Domain;
using SmartLock.Domain.Users;
using SmartLock.Implementation.Services;

namespace SmartLock.Tests.UnitTests;

public class UserServiceTests
{
    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ShouldThrowException()
    {
        // Arrange
        var tokenGenerator = new Mock<ITokenGenerator>();

        var store = new Mock<IUserStore<UserEntity>>();
        var userManager = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);
        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync((UserEntity)null);

        var userService = new UserService(userManager.Object, tokenGenerator.Object);

        // Act
        var ex = await Record.ExceptionAsync(() => userService.GetJwtTokenAsync("test", "test"));

        // Assert
        Assert.IsType<DomainException>(ex);
        Assert.Equal("Invalid username or password.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ShouldThrowException()
    {
        // Arrange
        var tokenGenerator = new Mock<ITokenGenerator>();

        var store = new Mock<IUserStore<UserEntity>>();
        var userManager = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);
        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new UserEntity());
        userManager.Setup(x => x.CheckPasswordAsync(It.IsAny<UserEntity>(), It.IsAny<string>())).ReturnsAsync(false);

        var userService = new UserService(userManager.Object, tokenGenerator.Object);

        // Act
        var ex = await Record.ExceptionAsync(() => userService.GetJwtTokenAsync("test", "test"));

        // Assert
        Assert.IsType<DomainException>(ex);
        Assert.Equal("Invalid username or password.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenUserAndPasswordIsValid_ShouldReturnToken()
    {
        // Arrange
        var tokenGenerator = new Mock<ITokenGenerator>();
        tokenGenerator.Setup(x => x.GenerateJwtToken(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<IEnumerable<string>>())).Returns("Token");

        var store = new Mock<IUserStore<UserEntity>>();
        var userManager = new Mock<UserManager<UserEntity>>(store.Object, null, null, null, null, null, null, null, null);
        userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(new UserEntity());
        userManager.Setup(x => x.CheckPasswordAsync(It.IsAny<UserEntity>(), It.IsAny<string>())).ReturnsAsync(true);

        var userService = new UserService(userManager.Object, tokenGenerator.Object);

        // Act
        var result = await userService.GetJwtTokenAsync("test", "test");

        // Assert
        Assert.Equal("Token", result.Token);
    }
}