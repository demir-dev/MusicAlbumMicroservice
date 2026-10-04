using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.Application.Libraries;

public interface ILibraryRepository
{
    Task<IReadOnlyList<LibraryAlbum>> GetAlbumsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAlbumsAsync(IReadOnlyList<LibraryAlbum> albums, CancellationToken cancellationToken = default);
    Task<bool> RemoveAlbumAsync(Guid userId, Guid albumId, CancellationToken cancellationToken = default);
}
