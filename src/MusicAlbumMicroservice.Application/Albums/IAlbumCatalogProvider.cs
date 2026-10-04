namespace MusicAlbumMicroservice.Application.Albums;

public interface IAlbumCatalogProvider
{
    string Name { get; }
    Task<IReadOnlyList<Album>> SearchAsync(string? albumName, string? artistName, CancellationToken cancellationToken = default);
    Task<Album?> GetAlbumAsync(string albumId, CancellationToken cancellationToken = default);
}
