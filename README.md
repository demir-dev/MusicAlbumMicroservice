# Music Albums Library

I built this .NET 10 microservice to search Deezer albums and manage each user's personal library. I chose Deezer because its public search and album lookup require no API credentials.

## Run and test

Start Docker Desktop, then run from the repository root:

```sh
docker compose up --build -d
```

Open **[Scalar](http://localhost:8080/scalar)** directly to inspect and test every API. Create a user, search albums, add their catalogue IDs, list the library, then remove entries using their saved IDs.

SQLite and logs use persistent Docker volumes. `docker compose down` stops the service; adding `--volumes` also deletes its data. View logs with `docker compose logs -f api`.

Without Docker, install the .NET 10 SDK:

```sh
dotnet run --project src/MusicAlbumMicroservice.Api --launch-profile http
```

Local Scalar: http://localhost:5105/scalar. SQLite creates itself on startup; local database and log paths are relative to the working directory.

Run all tests with the .NET 10 SDK:

```sh
dotnet test MusicAlbumMicroservice.slnx
```

I included six xUnit unit tests for user rules, duplicate/batch additions, and provider mapping. Interfaces let me test these decisions with fake dependencies. My two automated HTTP tests exercise user creation and the library workflow using `WebApplicationFactory`, temporary SQLite, and a fake provider. After package restore, the tests need no Docker or internet. [Separate test instructions](tests/MusicAlbumMicroservice.AutomatedTest/README.md).

## Structure and architecture

I chose a layered architecture with separate projects to keep library operations independent of catalogue APIs and storage:

| Project | Responsibility |
|---|---|
| Domain | User and saved album models |
| Application | Use cases, repository/provider interfaces, expected failures as Results |
| Infrastructure | Deezer adapter, EF Core context, SQLite repositories |
| Api | Controllers, request validation, Scalar, separate logging/exception middlewares |
| UnitTests / AutomatedTest | Focused unit tests and HTTP workflows |

I use `Dockerfile` to build the service and `docker-compose.yaml` to configure storage and ports. Application depends on Domain; Infrastructure implements its interfaces. I register those implementations in API through dependency injection. Serilog connects request and business logs through a trace ID.

## Trade-offs

**Scope and provider choice**

I implemented Deezer only, as the assignment allows either catalogue. I kept its HTTP calls and response models in Infrastructure behind `IAlbumCatalogProvider`, which Application owns. This gives me a replacement boundary, but not a complete connector platform: the API currently assumes one provider, and does not connect a user's external music account.

**Saved data**

I store an album's provider, external ID, title, artist, cover URL, and album URL when it is added. A library remains readable without contacting Deezer. The cost is stale metadata and potentially broken links if the provider changes or removes a release. I preserve the saved text, not the external images or catalogue itself.

**Adding and identifying albums**

I resolve all new selections before saving a batch. One failed lookup means none of the new entries are saved; lookups are sequential, so large batches take longer. Existing entries are skipped, and the response contains only new additions. Different editions remain separate because I identify albums by provider and external ID, rather than guessing from their titles. Simultaneous duplicate requests can still return a database error.

**Running the exercise**

I chose SQLite and Docker Compose so the service needs no separate database server. This suits one running instance. `EnsureCreated` initializes the database but does not upgrade an existing schema. Search examines only the first 25 provider results, and filters their names locally. I omitted authentication as permitted: user IDs select libraries, but do not protect them.

**Testing and errors**

I kept the tests focused on user creation, album additions, mapping, and the HTTP workflow. Fake providers make the suite repeatable, but do not prove the live catalogue is available. Unexpected failures return `500` with a trace ID; the logs carry the detail rather than exposing it to callers.

## Where I would take it next

### Connectors and provider selection

A user might already have music saved in Spotify, Deezer, or another service and want one place to manage those selections. I would introduce a connector service that lists available providers and their capabilities: catalogue search, account connection, and library import where supported. Public catalogue search and connecting a personal account are separate operations; searching should not require a user connection when the provider allows public access.

I would add a provider registry and a user-connections table linking each user to their selected providers, external account references, connection status, and sync progress. Credentials would be protected separately and never returned with album metadata. The user could choose a provider to search, or choose which connected accounts to import from.

The connector service would select an adapter by provider name. Each adapter would own that provider's authentication, requests, paging, data mapping, and request limits. Search results and add requests would include both provider and album ID. Adding a third provider would then mean adding its adapter and registration, without adding provider branches to library operations. Search/add orchestration needs this initial change; listing and removing saved entries would keep the same behavior.

### Data and synchronization

I would separate provider album records from users' saved entries. A provider record would hold provider name, external album ID, title, artist, optional cover URL, album URL, availability, and last refresh time. A library entry would link a user to that record. This avoids copying the same provider metadata for every user while retaining a local snapshot if the source disappears.

I would make this service the system of record for the user's saved selections, local display preferences, and sync history. Providers would remain the authority for their own catalogue records. I would keep imported metadata separate from user edits, so a refresh does not overwrite a custom title or cover. Where provider terms allow it, managed copies of cover images could reduce broken links; storing a URL alone does not preserve an image.

I would start with an explicit, one-way import into the local library. Imports would remember their source and progress, so retrying does not create duplicates. Disconnecting a provider or losing access would not silently delete saved albums. Sync would refresh metadata and mark unavailable records; automatically mirroring removals back and forth needs a separate product decision.

The same release may appear in several catalogues. I would keep those source records distinct initially. If the product needs one combined album entry, I would add an internal album record linked to its provider versions, using release identifiers and edition details where available. Similar titles alone are not enough to merge a standard and expanded edition.

### Search and provider availability

I would add pagination so users can reach results beyond the first page. Each connector would translate its provider's paging into a consistent response; combined searches would retain progress separately for each provider.

In Infrastructure, I would give each provider adapter its own timeouts and retry policy, with bounded backoff and provider retry instructions. I would retry temporary failures rather than invalid requests or missing albums. A short-lived cache for searches and album lookups would reduce latency and repeated provider calls; cached data would expire so it does not become a permanent source of stale metadata. Background imports could pause and resume when a provider limits requests. A failure in one source should leave the others usable, and combined results should tell the user which source could not be reached.

### Reliable operation

Before sharing the service with real users, I would add authentication and library ownership checks. Before running several instances, I would move persistence to a shared database, introduce schema migrations and backups, and handle concurrent additions as safe retries. I would extend tests around imports, unavailable sources, paging, and concurrent requests as those features arrive.
