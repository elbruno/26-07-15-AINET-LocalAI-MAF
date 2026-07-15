var builder = DistributedApplication.CreateBuilder(args);

var markitdown = builder.AddContainer("markitdown", "mcp/markitdown")
    .WithArgs("--http", "--host", "0.0.0.0", "--port", "3001")
    .WithHttpEndpoint(targetPort: 3001, name: "http");

var webApp = builder.AddProject<Projects._02_aichatweb_local_Web>("aichatweb-app")
    .WithEnvironment("FoundryLocal__ModelAlias", "phi-4-mini");
webApp
    .WithEnvironment("MARKITDOWN_MCP_URL", markitdown.GetEndpoint("http"));

builder.Build().Run();
