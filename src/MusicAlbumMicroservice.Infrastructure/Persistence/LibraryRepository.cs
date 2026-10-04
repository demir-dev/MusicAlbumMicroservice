using Microsoft.EntityFrameworkCore;
using MusicAlbumMicroservice.Application.Libraries;
using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.Infrastructure.Persistence;

public sealed class LibraryRepository : ILibraryRepository
{
    private readonly LibraryDbContext _db;

    public LibraryRepository(
        LibraryDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LibraryAlbum>> GetAlbumsAsync(
        Guid userId, CancellationToken cancellationToken = default)
        => await _db.LibraryAlbums.AsNoTracking().Where(album => album.UserId == userId)
            .OrderBy(album => album.AlbumName).ToListAsync(cancellationToken);

    public async Task AddAlbumsAsync(IReadOnlyList<LibraryAlbum> albums, CancellationToken cancellationToken = default)
    {
        _db.LibraryAlbums.AddRange(albums);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> RemoveAlbumAsync(Guid userId, Guid albumId, CancellationToken cancellationToken = default)
        => await _db.LibraryAlbums.Where(album => album.UserId == userId && album.Id == albumId)
            .ExecuteDeleteAsync(cancellationToken) > 0;
}
