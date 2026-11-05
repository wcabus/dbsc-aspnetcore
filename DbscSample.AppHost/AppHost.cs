var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DbscSample_Host>("sample-app");

builder.Build().Run();
