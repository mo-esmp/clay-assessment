using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SmartLock.WebApi.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public abstract class ApiControllerBase : ControllerBase
{
}