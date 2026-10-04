using Microsoft.AspNetCore.Mvc;
using MusicAlbumMicroservice.Api.Contracts;
using MusicAlbumMicroservice.Application.Libraries;
using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.Api.Controllers;

[ApiController]
[Route("users/{userId:guid}/library")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class LibrariesController : ControllerBase
{
    private readonly LibraryService _library;

    public LibrariesController(
        LibraryService library)
    {
        _library = library;
    }

    [HttpGet]
    [EndpointSummary("List the albums saved in a user’s library")]
    [ProducesResponseType(typeof(IReadOnlyList<LibraryAlbum>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LibraryAlbum>>> Get([FromRoute] Guid userId, CancellationToken ct)
    {
        var result = await _library.GetAlbumsAsync(userId, ct);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    [HttpPost("albums")]
    [EndpointSummary("Add albums by catalogue ID; duplicates are skipped")]
    [ProducesResponseType(typeof(IReadOnlyList<LibraryAlbum>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<LibraryAlbum>>> Add(
        [FromRoute] Guid userId, [FromBody] AddAlbumsRequest request, CancellationToken ct)
    {
        if (request.AlbumIds.Any(string.IsNullOrWhiteSpace))
        {
            ModelState.AddModelError(nameof(request.AlbumIds), "Album IDs cannot be empty.");
            return ValidationProblem(modelStateDictionary: ModelState);
        }

        var result = await _library.AddAlbumsAsync(userId, request.AlbumIds, ct);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    [HttpDelete("albums/{albumId:guid}")]
    [EndpointSummary("Remove a saved album from a user’s library")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Remove([FromRoute] Guid userId, [FromRoute] Guid albumId, CancellationToken ct)
    {
        var result = await _library.RemoveAlbumAsync(userId, albumId, ct);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error!);
    }
}
