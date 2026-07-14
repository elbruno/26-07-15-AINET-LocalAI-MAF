using System.ClientModel;
using FoundryLocal.Samples.Common;
using OpenAI;
using OpenAI.Chat;

var settings = FoundryLocalSampleSupport.LoadSettings();

Console.WriteLine("Foundry Local hello world (non-streaming)");
Console.WriteLine($"Endpoint: {settings.BaseUrl}");

var preflight = await FoundryLocalSampleSupport.RunPreflightAsync(settings.BaseUrl, settings.Model);
if (!preflight.Ok)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine(FoundryLocalSampleSupport.BuildPreflightGuidance(preflight.ErrorMessage!, settings));
    return 1;
}

if (!string.Equals(preflight.SelectedModel, settings.Model, StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine($"Configured model '{settings.Model}' was not found. Using '{preflight.SelectedModel}' instead.");
}

var chatClient = new ChatClient(
    preflight.SelectedModel!,
    new ApiKeyCredential(settings.ApiKey),
    new OpenAIClientOptions { Endpoint = new Uri(settings.BaseUrl) });

var completion = await chatClient.CompleteChatAsync(
[
    new SystemChatMessage("You are a concise assistant."),
    new UserChatMessage("Say hello from Foundry Local in one short sentence.")
]);

var response = string.Concat(completion.Value.Content.Select(c => c.Text)).Trim();
Console.WriteLine();
Console.WriteLine("Model response:");
Console.WriteLine(string.IsNullOrWhiteSpace(response) ? "(empty response)" : response);

return 0;
