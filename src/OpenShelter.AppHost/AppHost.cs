var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var openShelterDb = postgres.AddDatabase("openshelterdb");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var messaging = builder.AddRabbitMQ("messaging")
    .WithDataVolume();

var animalsApi = builder.AddProject<Projects.OpenShelter_Animals_Api>("animals-api")
    .WithReference(openShelterDb)
    .WaitFor(openShelterDb);

builder.AddProject<Projects.OpenShelter_Gateway>("gateway")
    .WithReference(animalsApi)
    .WaitFor(animalsApi);

builder.Build().Run();
