using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MusicAlbumMicroservice.Application.Albums;
using MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Models.Errors;
using MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Models.Response;

namespace MusicAlbumMicroservice.Infrastructure.Providers.Deezer.Implementation;

public sealed class DeezerAlbumCatalogProvider : IAlbumCatalogProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeezerAlbumCatalogProvider> _logger;

    public DeezerAlbumCatalogProvider(
        HttpClient httpClient,
        ILogger<DeezerAlbumCatalogProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public string Name => "deezer";

    public async Task<IReadOnlyList<Album>> SearchAsync(
        string? albumName, string? artistName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(albumName) && string.IsNullOrWhiteSpace(artistName))
            throw new ArgumentException("An album name or artist name is required.");

        try
        {
            var query = string.Join(" ", new[] { albumName?.Trim(), artistName?.Trim() }
                .Where(value => !string.IsNullOrWhiteSpace(value)));
            var result = await _httpClient.GetFromJsonAsync<DeezerSearchResponse>(
                             $"search/album?q={Uri.EscapeDataString(query)}&limit=25", cancellationToken)
                         ?? throw new HttpRequestException("Deezer returned an empty response.");

            ThrowIfError(result.Error);
            var albums = result.Data
                .Where(album => string.IsNullOrWhiteSpace(albumName)
                                || album.Title.Contains(albumName.Trim(), StringComparison.OrdinalIgnoreCase))
                .Where(album => string.IsNullOrWhiteSpace(artistName)
                                || album.Artist.Name.Contains(artistName.Trim(), StringComparison.OrdinalIgnoreCase))
                .DistinctBy(album => album.Id)
                .Select(Map).ToList();
            _logger.LogInformation("Deezer search returned {AlbumCount} albums.", albums.Count);
            return albums;
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(exception, "Deezer album search timed out.");
            throw;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Deezer album search failed. HTTP status: {StatusCode}.", exception.StatusCode);
            throw;
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Deezer album search returned invalid JSON.");
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Deezer album search failed unexpectedly.");
            throw;
        }
    }

    public async Task<Album?> GetAlbumAsync(string albumId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(albumId);
        try
        {
            using var response = await _httpClient.GetAsync(
                $"album/{Uri.EscapeDataString(albumId)}", cancellationToken);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Deezer album {AlbumId} was not found.", albumId);
                return null;
            }

            response.EnsureSuccessStatusCode();
            var album = await response.Content.ReadFromJsonAsync<DeezerAlbum>(cancellationToken)
                        ?? throw new HttpRequestException("Deezer returned an empty response.");

            // Deezer also reports missing albums through an error body with HTTP 200.
            if (album.Error?.Code == 800)
            {
                _logger.LogInformation("Deezer album {AlbumId} was not found.", albumId);
                return null;
            }

            ThrowIfError(album.Error);
            _logger.LogInformation("Retrieved Deezer album {AlbumId}.", albumId);
            return Map(album);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(exception, "Deezer lookup for album {AlbumId} timed out.", albumId);
            throw;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Deezer lookup for album {AlbumId} failed. HTTP status: {StatusCode}.",
                albumId, exception.StatusCode);
            throw;
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Deezer lookup for album {AlbumId} returned invalid JSON.", albumId);
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Deezer lookup for album {AlbumId} failed unexpectedly.", albumId);
            throw;
        }
    }

    private static Album Map(DeezerAlbum album) => new(
        album.Id.ToString(CultureInfo.InvariantCulture), album.Title, album.Artist.Name,
        album.CoverMedium, album.Link);

    private static void ThrowIfError(DeezerError? error)
    {
        if (error is not null)
            throw new HttpRequestException($"Deezer error {error.Code}: {error.Message}");
    }
}