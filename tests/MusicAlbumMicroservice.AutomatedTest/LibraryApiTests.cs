using System.Net;
using System.Net.Http.Json;
using MusicAlbumMicroservice.Application.Albums;
using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.AutomatedTest;

public sealed class LibraryApiTests
{
    [Fact]
    public async Task SearchAddAndRemoveAlbum_UpdatesLibraryWithoutDuplicates()
    {
        using var factory = new TestApiFactory();
        using var client = factory.CreateClient();
        var userResponse = await client.PostAsJsonAsync("/users", new { name = "Alex" });
        userResponse.EnsureSuccessStatusCode();
        var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();
        var libraryUrl = $"/users/{user!.Id}/library";

        var search = await client.GetFromJsonAsync<List<Album>>("/albums/search?albumName=Discovery");
        var album = Assert.Single(search!);
        var addition = await client.PostAsJsonAsync($"{libraryUrl}/albums",
            new { albumIds = new[] { album.Id, album.Id } });
        addition.EnsureSuccessStatusCode();
        var saved = Assert.Single((await addition.Content.ReadFromJsonAsync<List<LibraryAlbum>>())!);

        var library = await client.GetFromJsonAsync<List<LibraryAlbum>>(libraryUrl);
        Assert.Equal(saved.Id, Assert.Single(library!).Id);
        Assert.Equal(album.AlbumName, saved.AlbumName);

        var removal = await client.DeleteAsync($"{libraryUrl}/albums/{saved.Id}");
        Assert.Equal(HttpStatusCode.NoContent, removal.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<List<LibraryAlbum>>(libraryUrl))!);
    }
}
