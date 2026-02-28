using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.AspireSample_ApiService>("apiservice")
    .WithHttpHealthCheck("/health");

var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

var dataPath = Path.Combine(projectRoot, "data", "pb_data");
var migrationsPath = Path.Combine(projectRoot, "data", "pb_migrations");
var publicPath = Path.Combine(projectRoot, "data", "pb_public"); // optional
var hooksPath = Path.Combine(projectRoot, "data", "pb_hooks"); // optional

var pocketBase = builder.AddContainer("pocketbase", "ghcr.io/muchobien/pocketbase:latest")
    .WithHttpEndpoint(targetPort: 8090)
    .WithArgs("serve", "--http=0.0.0.0:8090", "--dir=/pb_data", "--dev")
    .WithContainerName("aspire_sample_pocketbase")
    .WithBindMount(dataPath, "/pb_data", isReadOnly: false)
    .WithBindMount(migrationsPath, "/pb_migrations", isReadOnly: false)
    .WithBindMount(publicPath, "/pb_public", isReadOnly: false) // optional
    .WithBindMount(hooksPath, "/pb_hooks", isReadOnly: false); // optional

builder.AddProject<Projects.AspireSample_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithReference(pocketBase.GetEndpoint("http"))
    .WaitFor(pocketBase);

builder.Build().Run();
