using System.Net;
using System.Net.Http.Json;

namespace MusicAlbumMicroservice.AutomatedTest;

public sealed class UsersApiTests
{
    [Fact]
    public async Task CreateUser_ReturnsCreatedUserAndEmptyLibrary()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/users", new { name = "Alex" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("Alex", user!.Name);
        Assert.NotEqual(Guid.Empty, user.Id);
        var library = await client.GetFromJsonAsync<List<MusicAlbumMicroservice.Domain.LibraryAlbum>>(
            $"/users/{user.Id}/library");
        Assert.Empty(library!);
    }
}
