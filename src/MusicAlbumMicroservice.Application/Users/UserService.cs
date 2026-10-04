using Microsoft.Extensions.Logging;
using MusicAlbumMicroservice.Application.Common;
using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.Application.Users;

public sealed class UserService
{
    private readonly IUserRepository _users;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository users, ILogger<UserService> logger)
    {
        _users = users;
        _logger = logger;
    }

    public async Task<Result<User>> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<User>.Failure(ErrorCode.InvalidInput, "A user name is required.");

        var user = new User(name);
        await _users.AddAsync(user, cancellationToken);
        _logger.LogInformation("Created user {UserId}.", user.Id);
        return Result<User>.Success(user);
    }
}