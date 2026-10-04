namespace MusicAlbumMicroservice.Domain;

public sealed class LibraryAlbum
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public required string Provider { get; init; }
    public required string ProviderAlbumId { get; init; }
    public required string AlbumName { get; init; }
    public required string ArtistName { get; init; }
    public string? CoverUrl { get; init; }
    public required string AlbumUrl { get; init; }
}
