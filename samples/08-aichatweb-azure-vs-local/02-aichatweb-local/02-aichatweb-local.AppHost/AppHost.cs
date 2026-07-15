var builder = DistributedApplication.CreateBuilder(args);

var webApp = builder.AddProject<Projects._02_aichatweb_local_Web>("aichatweb-app")
    .WithEnvironment("FoundryLocal__ModelAlias", "phi-4-mini");

builder.Build().Run();
