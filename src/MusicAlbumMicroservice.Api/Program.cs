using Microsoft.EntityFrameworkCore;
using MusicAlbumMicroservice.Api.Middlewares;
using Serilog;
using Scalar.AspNetCore;
using MusicAlbumMicroservice.Application.Libraries;
using MusicAlbumMicroservice.Application.Users;
using MusicAlbumMicroservice.Infrastructure;
using MusicAlbumMicroservice.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog((services, logging) => logging
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration.GetConnectionString("Library")
                                   ?? "Data Source=library.db");
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<LibraryService>();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);

var app = builder.Build();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapControllers();
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("OpenApi:Enabled"))
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Music Albums Library"));
}
app.Run();

public partial class Program;