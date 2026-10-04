using System.ComponentModel.DataAnnotations;

namespace MusicAlbumMicroservice.Api.Contracts;

public sealed class AddAlbumsRequest
{
    [Required, MinLength(1)]
    public string[] AlbumIds { get; init; } = [];
}
