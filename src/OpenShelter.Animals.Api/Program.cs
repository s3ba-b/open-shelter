var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Connect to the PostgreSQL "openshelterdb" resource via Aspire service discovery.
// Registers an NpgsqlDataSource plus a health check that proves the connection;
// no EF Core entities or migrations yet (that lands with the M2 Animals domain).
builder.AddNpgsqlDataSource("openshelterdb");

var app = builder.Build();

app.MapDefaultEndpoints();

// Trivial liveness/ping endpoint, reachable through the gateway at /animals/ping.
app.MapGet("/ping", () => Results.Ok(new { service = "animals-api", status = "ok" }));

app.Run();
