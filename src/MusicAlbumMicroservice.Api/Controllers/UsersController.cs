using Microsoft.AspNetCore.Mvc;
using MusicAlbumMicroservice.Api.Contracts;
using MusicAlbumMicroservice.Application.Users;
using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.Api.Controllers;

[ApiController]
[Route("users")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class UsersController : ControllerBase
{
    private readonly UserService _users;

    public UsersController(
        UserService users)
    {
        _users = users;
    }

    [HttpPost]
    [EndpointSummary("Create a user and an empty personal library")]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<User>> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await _users.CreateAsync(request.Name, ct);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error!);

        var user = result.Value!;
        return CreatedAtAction(nameof(LibrariesController.Get), "Libraries", new { userId = user.Id }, user);
    }
}
