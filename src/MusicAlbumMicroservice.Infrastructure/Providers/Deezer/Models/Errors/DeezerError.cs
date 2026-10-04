namespace MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Models.Errors;

internal sealed class DeezerError
{
    public int Code { get; init; }
    public string Message { get; init; } = "";
}
