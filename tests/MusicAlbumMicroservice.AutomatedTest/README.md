# Automated API tests

Requires the .NET 10 SDK. From the repository root:

```sh
dotnet restore
dotnet test tests/MusicAlbumMicroservice.AutomatedTest/MusicAlbumMicroservice.AutomatedTest.csproj
```

The two tests use xUnit, ASP.NET Core `WebApplicationFactory`, and `HttpClient`. The API runs in-process; no separately running service, Docker, or Deezer access is needed.

- `UsersApiTests`: create a user and verify their library starts empty.
- `LibraryApiTests`: search, add duplicate album IDs, list the saved entry, remove it, and verify the library is empty.

Each test uses a temporary SQLite database and a fake catalogue provider. The shared test factory deletes its database after shutdown.
