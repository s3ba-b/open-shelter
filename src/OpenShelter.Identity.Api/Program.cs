var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Connect to the PostgreSQL "identitydb" resource via Aspire service discovery,
// registering an NpgsqlDataSource plus a health check that proves the connection.
builder.AddNpgsqlDataSource("identitydb");

var app = builder.Build();

app.MapDefaultEndpoints();

// Trivial liveness/ping endpoint, reachable through the gateway at /identity/ping.
app.MapGet("/ping", () => Results.Ok(new { service = "identity-api", status = "ok" }));

app.Run();
