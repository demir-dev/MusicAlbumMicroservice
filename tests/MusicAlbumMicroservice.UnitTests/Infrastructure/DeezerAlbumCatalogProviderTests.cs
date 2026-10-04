using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Implementation;

namespace MusicAlbumMicroservice.UnitTests.Infrastructure;

public sealed class DeezerAlbumCatalogProviderTests
{
    [Fact]
    public async Task Search_MapsAlbumsAndFiltersArtist()
    {
        var handler = new StubHandler("""
            {"data":[
              {"id":302127,"title":"Discovery","link":"https://www.deezer.com/album/302127",
               "cover_medium":"https://example.com/cover.jpg","artist":{"name":"Daft Punk"}},
              {"id":2,"title":"Discovery","link":"https://example.com/2","artist":{"name":"Other"}}
            ]}
            """);
        var provider = CreateProvider(handler);

        var results = await provider.SearchAsync("Discovery", "Daft Punk");

        var album = Assert.Single(results);
        Assert.Equal("302127", album.Id);
        Assert.Equal("Daft Punk", album.ArtistName);
        Assert.Equal("https://example.com/cover.jpg", album.CoverUrl);
    }

    [Fact]
    public async Task GetAlbum_WhenDeezerReportsMissingData_ReturnsNull()
    {
        var provider = CreateProvider(new StubHandler("""
            {"error":{"code":800,"message":"no data"}}
            """));

        Assert.Null(await provider.GetAlbumAsync("0"));
    }

    private static DeezerAlbumCatalogProvider CreateProvider(StubHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("https://api.deezer.com/") },
            NullLogger<DeezerAlbumCatalogProvider>.Instance);

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly string _body;

        public StubHandler(string body)
        {
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }
}
