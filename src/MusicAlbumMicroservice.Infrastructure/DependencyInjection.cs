using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MusicAlbumMicroservice.Application.Users;
using MusicAlbumMicroservice.Application.Libraries;
using MusicAlbumMicroservice.Infrastructure.Persistence;
using MusicAlbumMicroservice.Application.Albums;
using MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Implementation;

namespace MusicAlbumMicroservice.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddHttpClient<IAlbumCatalogProvider, DeezerAlbumCatalogProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.deezer.com/");
            client.Timeout = TimeSpan.FromSeconds(60);
        });

        services.AddDbContext<LibraryDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ILibraryRepository, LibraryRepository>();

        return services;
    }
}