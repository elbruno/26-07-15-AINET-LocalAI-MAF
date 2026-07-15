using ElBruno.LocalEmbeddings;
using ElBruno.MAF.FoundryLocal;
using ElBruno.MAF.FoundryLocal.Components;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using _02_aichatweb_local.Web.Components;
using _02_aichatweb_local.Web.Services;
using _02_aichatweb_local.Web.Services.Ingestion;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var modelAlias = builder.Configuration["FoundryLocal:ModelAlias"] ?? "phi-4-mini";
var foundryOptions = Options.Create(new FoundryLocalOptions
{
    ModelAlias = modelAlias,
    DownloadIfMissing = true,
    UnloadOnExit = false
});
var runtimeOptions = Options.Create(new ChatRuntimeOptions
{
    Temperature = 0.1,
    MaxOutputTokens = 1024,
    Streaming = false
});
var lifecycle = new FoundryLocalModelLifecycleService(
    foundryOptions,
    runtimeOptions,
    NullLogger<FoundryLocalModelLifecycleService>.Instance);
var chatClient = new FoundryLocalChatClientAdapter(
    lifecycle,
    NullLogger<FoundryLocalChatClientAdapter>.Instance);
var embeddingGenerator = await LocalEmbeddingGenerator.CreateAsync();

builder.Services.AddSingleton(lifecycle);
builder.Services.AddFoundryLocalComponents(modelAlias);
builder.Services.AddChatClient(chatClient)
    .UseFunctionInvocation(NullLoggerFactory.Instance, cfg =>
    {
        cfg.MaximumIterationsPerRequest = 8;
        cfg.IncludeDetailedErrors = true;
    })
    .UseOpenTelemetry(configure: c =>
        c.EnableSensitiveData = builder.Environment.IsDevelopment());
builder.Services.AddEmbeddingGenerator(embeddingGenerator);

var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";
builder.Services.AddSqliteVectorStore(_ => vectorStoreConnectionString);
builder.Services.AddSqliteCollection<string, IngestedChunk>(IngestedChunk.CollectionName, vectorStoreConnectionString);
builder.Services.AddSingleton<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddKeyedSingleton("ingestion_directory", new DirectoryInfo(Path.Combine(builder.Environment.WebRootPath, "Data")));

var app = builder.Build();
app.Lifetime.ApplicationStopping.Register(() => lifecycle.DisposeAsync().AsTask().GetAwaiter().GetResult());

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
