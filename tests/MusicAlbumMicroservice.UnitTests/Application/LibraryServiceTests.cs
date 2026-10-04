using Microsoft.Extensions.Logging.Abstractions;
using MusicAlbumMicroservice.Application.Common;
using MusicAlbumMicroservice.Application.Albums;
using MusicAlbumMicroservice.Application.Libraries;
using MusicAlbumMicroservice.Application.Users;
using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.UnitTests.Application;

public sealed class LibraryServiceTests
{
    [Fact]
    public async Task AddAlbums_SkipsDuplicates()
    {
        var store = new TestStore();
        var service = new LibraryService(store, store, new TestCatalog(), NullLogger<LibraryService>.Instance);
        var userId = Guid.NewGuid();

        await service.AddAlbumsAsync(userId, ["1", "1", "2"]);
        var repeated = await service.AddAlbumsAsync(userId, ["1"]);

        Assert.Equal(new[] { "1", "2" }, store.Albums.Select(album => album.ProviderAlbumId));
        Assert.True(repeated.IsSuccess);
        Assert.Empty(repeated.Value!);
    }

    [Fact]
    public async Task AddAlbums_WhenAnAlbumIsMissing_DoesNotSaveTheBatch()
    {
        var store = new TestStore();
        var service = new LibraryService(store, store, new TestCatalog(), NullLogger<LibraryService>.Instance);

        var result = await service.AddAlbumsAsync(Guid.NewGuid(), ["1", "missing"]);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.AlbumNotFound, result.Error!.Code);

        Assert.Empty(store.Albums);
    }

    private sealed class TestCatalog : IAlbumCatalogProvider
    {
        public string Name => "deezer";

        public Task<Album?> GetAlbumAsync(string albumId, CancellationToken cancellationToken = default)
        {
            Album? album = albumId == "missing" ? null
                : new Album(albumId, "Album", "Artist", null, "https://example.com/album");
            return Task.FromResult(album);
        }

        public Task<IReadOnlyList<Album>> SearchAsync(
            string? albumName, string? artistName, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class TestStore : IUserRepository, ILibraryRepository
    {
        public List<LibraryAlbum> Albums { get; } = [];

        public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<IReadOnlyList<LibraryAlbum>> GetAlbumsAsync(
            Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<LibraryAlbum>>(
                Albums.Where(album => album.UserId == userId).ToList());

        public Task AddAlbumsAsync(IReadOnlyList<LibraryAlbum> albums, CancellationToken cancellationToken = default)
        {
            Albums.AddRange(albums);
            return Task.CompletedTask;
        }

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<bool> RemoveAlbumAsync(Guid userId, Guid albumId, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
