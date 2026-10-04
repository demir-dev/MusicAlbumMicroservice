using Microsoft.AspNetCore.Mvc;
using MusicAlbumMicroservice.Application.Albums;

namespace MusicAlbumMicroservice.Api.Controllers;

[ApiController]
[Route("albums")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public sealed class AlbumsController : ControllerBase
{
    private readonly IAlbumCatalogProvider _catalog;

    public AlbumsController(
        IAlbumCatalogProvider catalog)
    {
        _catalog = catalog;
    }

    [HttpGet("search")]
    [EndpointSummary("Search albums by album name and artist name")]
    [ProducesResponseType(typeof(IReadOnlyList<Album>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<Album>>> Search(
        [FromQuery] string? albumName, [FromQuery] string? artistName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(albumName) && string.IsNullOrWhiteSpace(artistName))
        {
            ModelState.AddModelError("search", "An album name or artist name is required.");
            return ValidationProblem(modelStateDictionary: ModelState);
        }

        return Ok(await _catalog.SearchAsync(albumName, artistName, ct));
    }
}
