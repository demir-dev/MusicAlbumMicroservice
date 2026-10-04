using MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Models.Errors;

namespace MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Models.Response;

internal sealed class DeezerSearchResponse
{
    public List<DeezerAlbum> Data { get; init; } = [];
    public DeezerError? Error { get; init; }
}
