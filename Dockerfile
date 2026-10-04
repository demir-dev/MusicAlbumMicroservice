FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/MusicAlbumMicroservice.Domain/*.csproj src/MusicAlbumMicroservice.Domain/
COPY src/MusicAlbumMicroservice.Application/*.csproj src/MusicAlbumMicroservice.Application/
COPY src/MusicAlbumMicroservice.Infrastructure/*.csproj src/MusicAlbumMicroservice.Infrastructure/
COPY src/MusicAlbumMicroservice.Api/*.csproj src/MusicAlbumMicroservice.Api/
RUN dotnet restore src/MusicAlbumMicroservice.Api/MusicAlbumMicroservice.Api.csproj
COPY src/ src/
RUN dotnet publish src/MusicAlbumMicroservice.Api/MusicAlbumMicroservice.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/data /app/logs && chown -R $APP_UID:$APP_UID /app/data /app/logs
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "MusicAlbumMicroservice.Api.dll"]
