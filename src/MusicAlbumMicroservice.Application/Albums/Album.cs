namespace MusicAlbumMicroservice.Application.Albums;

public sealed record Album(
    string Id,
    string AlbumName,
    string ArtistName,
    string? CoverUrl,
    string AlbumUrl);
