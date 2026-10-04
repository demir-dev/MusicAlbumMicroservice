namespace MusicAlbumMicroservice.Domain;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    private User() { }

    public User(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = Guid.NewGuid();
        Name = name.Trim();
    }
}
