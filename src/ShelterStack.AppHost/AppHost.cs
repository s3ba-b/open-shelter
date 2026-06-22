var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var shelterStackDb = postgres.AddDatabase("shelterstackdb");
var identityDb = postgres.AddDatabase("identitydb");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var messaging = builder.AddRabbitMQ("messaging")
    .WithDataVolume();

var animalsApi = builder.AddProject<Projects.ShelterStack_Animals_Api>("animals-api")
    .WithReference(shelterStackDb)
    .WaitFor(shelterStackDb);

var identityApi = builder.AddProject<Projects.ShelterStack_Identity_Api>("identity-api")
    .WithReference(identityDb)
    .WaitFor(identityDb);

builder.AddProject<Projects.ShelterStack_Gateway>("gateway")
    .WithReference(animalsApi)
    .WaitFor(animalsApi)
    .WithReference(identityApi)
    .WaitFor(identityApi);

builder.Build().Run();
