using System.Text.Json.Serialization;
using MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Models.Errors;

namespace MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Models.Response;

internal sealed class DeezerAlbum
{
    public long Id { get; init; }
    public string Title { get; init; } = "";
    public string Link { get; init; } = "";
    [JsonPropertyName("cover_medium")]
    public string? CoverMedium { get; init; }
    public DeezerArtist Artist { get; init; } = new();
    public DeezerError? Error { get; init; }
}
