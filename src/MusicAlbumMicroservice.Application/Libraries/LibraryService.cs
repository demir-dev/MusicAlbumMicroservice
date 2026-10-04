using Microsoft.Extensions.Logging;
using MusicAlbumMicroservice.Application.Common;
using MusicAlbumMicroservice.Application.Albums;
using MusicAlbumMicroservice.Application.Users;
using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.Application.Libraries;

public sealed class LibraryService
{
    private readonly IUserRepository _users;
    private readonly ILibraryRepository _library;
    private readonly IAlbumCatalogProvider _catalog;
    private readonly ILogger<LibraryService> _logger;

    public LibraryService(IUserRepository users, ILibraryRepository library, IAlbumCatalogProvider catalog,
        ILogger<LibraryService> logger)
    {
        _users = users;
        _library = library;
        _catalog = catalog;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<LibraryAlbum>>> GetAlbumsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        if (!await _users.ExistsAsync(userId, cancellationToken))
            return Result<IReadOnlyList<LibraryAlbum>>.Failure(ErrorCode.UserNotFound,
                $"User '{userId}' was not found.");
        return Result<IReadOnlyList<LibraryAlbum>>.Success(await _library.GetAlbumsAsync(userId, cancellationToken));
    }

    public async Task<Result<IReadOnlyList<LibraryAlbum>>> AddAlbumsAsync(
        Guid userId, IReadOnlyList<string> albumIds, CancellationToken cancellationToken = default)
    {
        if (albumIds is null || albumIds.Count == 0 || albumIds.Any(string.IsNullOrWhiteSpace))
            return Result<IReadOnlyList<LibraryAlbum>>.Failure(ErrorCode.InvalidInput,
                "At least one non-empty album ID is required.");

        if (!await _users.ExistsAsync(userId, cancellationToken))
            return Result<IReadOnlyList<LibraryAlbum>>.Failure(ErrorCode.UserNotFound,
                $"User '{userId}' was not found.");
        var existing = await _library.GetAlbumsAsync(userId, cancellationToken);
        var savedIds = existing.Where(album => album.Provider == _catalog.Name)
            .Select(album => album.ProviderAlbumId).ToHashSet();
        var additions = new List<LibraryAlbum>();

        foreach (var albumId in albumIds.Distinct())
        {
            if (savedIds.Contains(albumId))
                continue;

            var album = await _catalog.GetAlbumAsync(albumId, cancellationToken);
            if (album is null)
                return Result<IReadOnlyList<LibraryAlbum>>.Failure(ErrorCode.AlbumNotFound,
                    $"Album '{albumId}' was not found.");

            if (!savedIds.Add(album.Id))
                continue;

            additions.Add(new LibraryAlbum
            {
                UserId = userId,
                Provider = _catalog.Name,
                ProviderAlbumId = album.Id,
                AlbumName = album.AlbumName,
                ArtistName = album.ArtistName,
                CoverUrl = album.CoverUrl,
                AlbumUrl = album.AlbumUrl
            });
        }

        if (additions.Count > 0)
            await _library.AddAlbumsAsync(additions, cancellationToken);

        _logger.LogInformation("Added {AlbumCount} albums to user {UserId}'s library.", additions.Count, userId);
        return Result<IReadOnlyList<LibraryAlbum>>.Success(additions);
    }

    public async Task<Result<bool>> RemoveAlbumAsync(
        Guid userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        if (!await _users.ExistsAsync(userId, cancellationToken))
            return Result<bool>.Failure(ErrorCode.UserNotFound, $"User '{userId}' was not found.");

        if (!await _library.RemoveAlbumAsync(userId, albumId, cancellationToken))
            return Result<bool>.Failure(ErrorCode.AlbumNotFound, "The album is not in this user's library.");

        _logger.LogInformation("Removed album {AlbumId} from user {UserId}'s library.", albumId, userId);
        return Result<bool>.Success(true);
    }
}