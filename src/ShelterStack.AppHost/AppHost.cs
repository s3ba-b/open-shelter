var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithDataVolume();
var shelterStackDb = postgres.AddDatabase("shelterstackdb");
var identityDb = postgres.AddDatabase("identitydb");

var redis = builder.AddRedis("redis").WithDataVolume();

var messaging = builder.AddRabbitMQ("messaging").WithDataVolume();

var animalsApi = builder
    .AddProject<Projects.ShelterStack_Animals_Api>("animals-api")
    .WithReference(shelterStackDb)
    .WaitFor(shelterStackDb);

var identityApi = builder
    .AddProject<Projects.ShelterStack_Identity_Api>("identity-api")
    .WithReference(identityDb)
    .WaitFor(identityDb);

var gateway = builder
    .AddProject<Projects.ShelterStack_Gateway>("gateway")
    .WithReference(animalsApi)
    .WaitFor(animalsApi)
    .WithReference(identityApi)
    .WaitFor(identityApi);

// Staff-facing Blazor web app. It talks to the backend only through the gateway
// (never directly to a business service) and is the app's external HTTP endpoint.
builder
    .AddProject<Projects.ShelterStack_Web>("web")
    .WithReference(gateway)
    .WaitFor(gateway)
    .WithExternalHttpEndpoints();

builder.Build().Run();
