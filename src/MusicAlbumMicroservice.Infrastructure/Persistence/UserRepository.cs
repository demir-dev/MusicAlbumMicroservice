using Microsoft.EntityFrameworkCore;
using MusicAlbumMicroservice.Application.Users;
using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly LibraryDbContext _db;

    public UserRepository(
        LibraryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
        => _db.Users.AnyAsync(user => user.Id == userId, cancellationToken);
}
