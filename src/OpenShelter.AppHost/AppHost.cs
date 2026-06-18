var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume();
var openShelterDb = postgres.AddDatabase("openshelterdb");

var redis = builder.AddRedis("redis")
    .WithDataVolume();

var messaging = builder.AddRabbitMQ("messaging")
    .WithDataVolume();

builder.Build().Run();
