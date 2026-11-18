using MedLink.Infrastructure;
using MedLink.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/health/database", async (
    ApplicationDbContext dbContext,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        return canConnect
            ? Results.Ok(new { status = "healthy", database = "reachable" })
            : Results.Json(
                new { status = "unhealthy", database = "unreachable" },
                statusCode: StatusCodes.Status503ServiceUnavailable);
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Database connectivity check failed.");

        return Results.Json(
            new { status = "unhealthy", database = "unreachable" },
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();
