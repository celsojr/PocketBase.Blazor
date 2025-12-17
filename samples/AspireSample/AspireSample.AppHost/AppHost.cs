using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireSample_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

var pocketBase = builder.AddContainer("pocketbase", "ghcr.io/muchobien/pocketbase:latest")
    .WithHttpEndpoint(targetPort: 8090)
    .WithVolume("pocketbase-data", "/pb_data");

builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(pocketBase.GetEndpoint("http"))
    .WaitFor(pocketBase);

builder.Build().Run();
