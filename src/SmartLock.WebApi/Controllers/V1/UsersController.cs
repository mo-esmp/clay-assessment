using SmartLock.Domain.Users;

namespace SmartLock.WebApi.Controllers.V1;

[AllowAnonymous]
[Route("api/v1/[controller]")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    ///   Get JWT token for sending request to protected APIs
    /// </summary>
    /// <remarks>
    /// There are two employees:
    ///
    ///     Manager user, email: manager@company.com - id: 63d396e7-13b1-45d1-a388-25894721205a - role: Manager
    ///     Employee user, email: employee@company.com - id: ae982851-743c-4b9f-9b67-108579cdece6 - role: Employee
    ///     Password for login: UserPassword
    ///
    /// </remarks>
    /// <response code="200">JWT token</response>
    ///<response code="400">If the posted body is not valid or server side validation failed or invalid username or password.</response>
    [HttpPost("token")]
    [AllowAnonymous]
    [SwaggerRequestExample(typeof(TokenRequest), typeof(UserLoginExampleValue))]
    [ProducesResponseType(typeof(JwtTokenDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem();

        var jwtToken = await _userService.GetJwtTokenAsync(request.Username, request.Password);
        return Ok(jwtToken);
    }
}