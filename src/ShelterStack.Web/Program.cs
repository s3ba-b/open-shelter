using ShelterStack.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Shared Aspire wiring: OpenTelemetry, health checks, resilience, and service discovery.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// All backend calls go through the gateway, never directly to a business service.
// The base address is a logical service name resolved by Aspire service discovery
// (configured via AddServiceDefaults) to the gateway's real endpoint at runtime.
builder.Services.AddHttpClient(
    "gateway",
    client => client.BaseAddress = new Uri("https+http://gateway")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// Aspire health/liveness endpoints (/health, /alive).
app.MapDefaultEndpoints();

app.Run();
