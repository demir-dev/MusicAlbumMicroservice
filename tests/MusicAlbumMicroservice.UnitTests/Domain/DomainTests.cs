using MusicAlbumMicroservice.Domain;

namespace MusicAlbumMicroservice.UnitTests.Domain;

public sealed class DomainTests
{
    [Fact]
    public void User_TrimsNameAndAssignsId()
    {
        var user = new User("  Alex  ");
        Assert.Equal("Alex", user.Name);
        Assert.NotEqual(Guid.Empty, user.Id);
    }

    [Fact]
    public void User_RejectsBlankName()
    {
        Assert.Throws<ArgumentException>(() => new User("   "));
    }
}
