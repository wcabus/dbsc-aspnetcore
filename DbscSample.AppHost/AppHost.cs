var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithDataVolume("dbsc-redis-data");

builder.AddProject<Projects.DbscSample_Host>("sample-app")
    .WithReference(redis)
    .WaitFor(redis);

builder.Build().Run();
