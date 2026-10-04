using Microsoft.AspNetCore.Mvc;
using MusicAlbumMicroservice.Api.Contracts;

namespace MusicAlbumMicroservice.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Check that the service is running")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get() => Ok(new HealthResponse("healthy"));
}

