var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var openShelterDb = postgres.AddDatabase("openshelterdb");
var identityDb = postgres.AddDatabase("identitydb");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var messaging = builder.AddRabbitMQ("messaging")
    .WithDataVolume();

var animalsApi = builder.AddProject<Projects.OpenShelter_Animals_Api>("animals-api")
    .WithReference(openShelterDb)
    .WaitFor(openShelterDb);

var identityApi = builder.AddProject<Projects.OpenShelter_Identity_Api>("identity-api")
    .WithReference(identityDb)
    .WaitFor(identityDb);

builder.AddProject<Projects.OpenShelter_Gateway>("gateway")
    .WithReference(animalsApi)
    .WaitFor(animalsApi)
    .WithReference(identityApi)
    .WaitFor(identityApi);

builder.Build().Run();
