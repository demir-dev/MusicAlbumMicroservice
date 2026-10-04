using System.ComponentModel.DataAnnotations;

namespace MusicAlbumMicroservice.Api.Contracts;

public sealed class CreateUserRequest
{
    [Required]
    public string Name { get; init; } = "";
}
