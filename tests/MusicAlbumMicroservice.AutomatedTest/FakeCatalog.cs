using MusicAlbumMicroservice.Application.Albums;

namespace MusicAlbumMicroservice.AutomatedTest;

internal sealed class FakeCatalog : IAlbumCatalogProvider
{
    private static readonly Album TestAlbum = new("302127", "Discovery", "Daft Punk", null,
        "https://www.deezer.com/album/302127");

    public string Name => "deezer";

    public Task<Album?> GetAlbumAsync(string albumId, CancellationToken cancellationToken = default)
        => Task.FromResult<Album?>(albumId == TestAlbum.Id ? TestAlbum : null);

    public Task<IReadOnlyList<Album>> SearchAsync(string? albumName, string? artistName,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<Album>>([TestAlbum]);
}
