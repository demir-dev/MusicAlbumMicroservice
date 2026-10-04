using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MusicAlbumMicroservice.Application.Albums;

namespace MusicAlbumMicroservice.AutomatedTest;

internal sealed class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly string _database;

    public TestApiFactory()
    {
        _database = Path.Combine(Path.GetTempPath(), $"library-test-{Guid.NewGuid()}.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Library", $"Data Source={_database};Pooling=False");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAlbumCatalogProvider>();
            services.AddSingleton<IAlbumCatalogProvider, FakeCatalog>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            foreach (var suffix in new[] { "", "-wal", "-shm" })
                File.Delete(_database + suffix);
    }
}
