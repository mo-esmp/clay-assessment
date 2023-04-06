using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SmartLock.Domain.Locks;
using SmartLock.Domain.Users;
using SmartLock.Implementation.Data;
using SmartLock.WebApi.Models;
using System.Net;
using System.Net.Http.Headers;

namespace SmartLock.Tests.IntegrationTests;

public class LocksControllersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LocksControllersTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    [Fact]
    public async Task When_User_Has_Access_To_The_Lock_Then_Can_Open_Lock_For_Employee()
    {
        // Arrange
        await GetTokenAsync(DefaultData.ManagerEmail);
        var request = new JsonContent(new LockAccessRequest(DefaultData.EmployeeId));

        // Act
        var response = await _client.PostAsync($"api/v1/locks/{DefaultData.StorageLockId}/access", request);
        var accessResult =
            JsonConvert.DeserializeObject<LockAccessResultDto>(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.NotNull(accessResult);
        Assert.Equal(LockAccessResult.Granted, accessResult.AccessResult);
    }

    [Fact]
    public async Task When_User_Has_No_Access_To_The_Lock_Then_Cannot_Open_Lock_For_Employee()
    {
        // Arrange
        await GetTokenAsync(DefaultData.EmployeeEmail);
        var request = new JsonContent(new LockAccessRequest(DefaultData.ManagerId));

        // Act
        var response = await _client.PostAsync($"api/v1/locks/{DefaultData.StorageLockId}/access", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task When_User_Has_Access_To_The_Lock_History_Then_Can_Get_Histories()
    {
        // Arrange
        await GetTokenAsync(DefaultData.ManagerEmail);

        // Act
        var response = await _client.GetAsync($"api/v1/locks/{DefaultData.StorageLockId}/history");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task When_User_Has_No_Access_To_The_Lock_History_Then_Cannot_Get_Histories()
    {
        // Arrange
        await GetTokenAsync(DefaultData.EmployeeEmail);

        // Act
        var response = await _client.GetAsync($"api/v1/locks/{DefaultData.StorageLockId}/history");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task GetTokenAsync(string username)
    {
        var request = new JsonContent(new TokenRequest(username, DefaultData.UserPassword));
        var response = await _client.PostAsync("api/v1/users/token", request);
        var result = await response.Content.ReadFromJsonAsync<JwtTokenDto>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result?.Token);
    }
}