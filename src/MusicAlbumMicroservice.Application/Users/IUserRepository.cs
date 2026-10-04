using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.Application.Users;

public interface IUserRepository
{
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}
