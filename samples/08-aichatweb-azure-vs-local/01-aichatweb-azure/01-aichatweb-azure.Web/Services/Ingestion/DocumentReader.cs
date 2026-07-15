using System.Text;
using ElBruno.MarkItDotNet;
using Microsoft.Extensions.DataIngestion;

namespace _01_aichatweb_azure.Web.Services.Ingestion;

internal sealed class DocumentReader(DirectoryInfo rootDirectory) : IngestionDocumentReader
{
    private readonly MarkdownReader _markdownReader = new();
    private readonly MarkdownConverter _markdownConverter = new();

    public override Task<IngestionDocument> ReadAsync(FileInfo source, string identifier, string? mediaType = null, CancellationToken cancellationToken = default)
    {
        if (Path.IsPathFullyQualified(identifier))
        {
            // Normalize the identifier to its relative path
            identifier = Path.GetRelativePath(rootDirectory.FullName, identifier);
        }

        mediaType = GetCustomMediaType(source) ?? mediaType;
        return base.ReadAsync(source, identifier, mediaType, cancellationToken);
    }

    public override Task<IngestionDocument> ReadAsync(Stream source, string identifier, string mediaType, CancellationToken cancellationToken = default)
        => mediaType switch
        {
            "application/pdf" => ConvertPdfAsync(source, identifier, cancellationToken),
            "text/markdown" => _markdownReader.ReadAsync(source, identifier, mediaType, cancellationToken),
            _ => throw new InvalidOperationException($"Unsupported media type '{mediaType}'"),
        };

    private static string? GetCustomMediaType(FileInfo source)
        => source.Extension switch
        {
            ".md" => "text/markdown",
            _ => null
        };

    private async Task<IngestionDocument> ConvertPdfAsync(Stream source, string identifier, CancellationToken cancellationToken)
    {
        var markdown = await _markdownConverter.ConvertAsync(source, ".pdf", cancellationToken);
        using var markdownStream = new MemoryStream(Encoding.UTF8.GetBytes(markdown));
        return await _markdownReader.ReadAsync(markdownStream, identifier, "text/markdown", cancellationToken);
    }
}
