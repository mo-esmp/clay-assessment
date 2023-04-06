using SmartLock.Domain.Locks;
using SmartLock.Domain.Users;
using SmartLock.WebApi.Extensions;
using System.ComponentModel;

namespace SmartLock.WebApi.Controllers.V1;

[Route("api/v1/[controller]")]
public class LocksController : ApiControllerBase
{
    private readonly IClusterClient _orleansClient;

    public LocksController(IClusterClient orleansClient)
    {
        _orleansClient = orleansClient;
    }

    /// <summary>
    ///   Opens a door for a user
    /// </summary>
    /// <remarks>
    /// There are two locks:
    ///
    ///     Entrance lock, id: 1d0579fa-5dc4-4e6a-aea0-503b417b33ca
    ///     Storage lock, id: 618582f1-06a5-4876-a6fd-20397cb77fd8
    ///
    /// </remarks>
    /// <response code="200">Access granted</response>
    ///<response code="400">If the posted body is not valid or server side validation failed.</response>
    ///<response code="401">Requester is not authenticated.</response>
    ///<response code="403">Requester does not have enough permission.</response>
    [HttpPost("{id}/access")]
    [DisplayName("Open a lock remotely for another employee")]
    [ProducesResponseType(typeof(JwtTokenDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostAccessAsync(string id, [FromBody] LockAccessRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem();

        var grain = _orleansClient.GetGrain<ILockGrain>(id);
        var result = await grain.RequestAccessAsync(User.GetId(), User.GetRoles(), request.EmployeeId);

        return Ok(result);
    }

    /// <summary>
    ///   Returns lock access history
    /// </summary>
    /// <response code="200">Return list of access</response>
    ///<response code="204">When no data found.</response>
    ///<response code="401">Requester is not authenticated.</response>
    ///<response code="403">Requester does not have enough permission.</response>
    [HttpGet("{id}/history")]
    [DisplayName("Get lock access history")]
    public async Task<IEnumerable<LockAccessHistoryDto>> GetHistoryAsync(
        [FromServices] ILockService service,
        string id,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10)
    {
        if (page < 1)
            page = 1;

        if (size is < 10 or > 100)
            size = 10;

        return await service.GetLockAccessesAsync(id, page, size);
    }
}