var builder = DistributedApplication.CreateBuilder(args);

var analyticsApi = builder.AddProject<Projects.Analytics_Api>("analytics-api");

builder.AddProject<Projects.Analytics_Web>("analytics-web")
    .WithReference(analyticsApi)
    .WaitFor(analyticsApi);

builder.Build().Run();
