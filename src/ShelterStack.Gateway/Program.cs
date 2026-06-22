var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Reverse proxy: routes are loaded from config and destinations are resolved
// through Aspire service discovery (e.g. "http://animals-api").
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapReverseProxy();

app.Run();
