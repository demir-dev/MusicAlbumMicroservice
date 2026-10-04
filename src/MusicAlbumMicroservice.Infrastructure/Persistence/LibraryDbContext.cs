using Microsoft.EntityFrameworkCore;
using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.Infrastructure.Persistence;

public sealed class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<LibraryAlbum> LibraryAlbums => Set<LibraryAlbum>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasKey(user => user.Id);
        modelBuilder.Entity<User>().Property(user => user.Name).IsRequired();

        var albums = modelBuilder.Entity<LibraryAlbum>();
        albums.HasKey(album => album.Id);
        albums.HasOne<User>().WithMany().HasForeignKey(album => album.UserId);
        albums.HasIndex(album => new { album.UserId, album.Provider, album.ProviderAlbumId }).IsUnique();
    }
}
