using System.Text.RegularExpressions;
using Xunit;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.DataIngestion.Chunkers;
using Microsoft.ML.Tokenizers;

namespace AiChatWeb.Local.Tests;

/// <summary>
/// Regression tests for the RAG ingestion chunking used by sample 08-02
/// (02-aichatweb-local). The app previously used <c>SemanticSimilarityChunker</c>,
/// which emitted many tiny, duplicated header-only fragments (e.g. the 40-char
/// "## 4. Operation ### 4.1 Basic Functions"). Those fragments dominated retrieval
/// for generic queries and broke grounding. The fix switched to
/// <see cref="SectionChunker"/>, which prepends each header to its section body and
/// only emits chunks that contain real content. These tests lock in that behavior.
/// </summary>
public class SectionChunkerRegressionTests
{
    private static readonly Regex HeadingLine = new(@"^\s*#{1,6}\s", RegexOptions.Compiled);

    private static string GpsWatchDocPath =>
        Path.Combine(AppContext.BaseDirectory, "TestData", "Example_GPS_Watch.md");

    private static SectionChunker CreateAppChunker() =>
        // Must mirror DataIngestor.IngestDataAsync exactly.
        new(new IngestionChunkerOptions(TiktokenTokenizer.CreateForModel("gpt-4o")));

    private static async Task<List<IngestionChunk<string>>> ChunkGpsWatchAsync()
    {
        var reader = new MarkdownReader();
        var document = await reader.ReadAsync(new FileInfo(GpsWatchDocPath), "Example_GPS_Watch.md", "text/markdown");

        var chunks = new List<IngestionChunk<string>>();
        await foreach (var chunk in CreateAppChunker().ProcessAsync(document))
        {
            chunks.Add(chunk);
        }

        return chunks;
    }

    private static bool IsHeaderOnly(string content)
    {
        var lines = content
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();

        return lines.Count > 0 && lines.All(l => HeadingLine.IsMatch(l));
    }

    [Fact]
    public void SampleDocumentIsPresentForTest()
    {
        Assert.True(File.Exists(GpsWatchDocPath), $"Expected sample document at {GpsWatchDocPath}");
    }

    [Fact]
    public async Task ProducesMultipleChunks()
    {
        var chunks = await ChunkGpsWatchAsync();
        Assert.NotEmpty(chunks);
        // The document is ~14KB of prose; a healthy section chunker yields several chunks.
        Assert.True(chunks.Count >= 2, $"Expected multiple chunks, got {chunks.Count}.");
    }

    [Fact]
    public async Task NoChunkIsAHeaderOnlyFragment()
    {
        var chunks = await ChunkGpsWatchAsync();

        var headerOnly = chunks
            .Where(c => IsHeaderOnly(c.Content))
            .Select(c => c.Content.Replace("\n", " ").Trim())
            .ToList();

        Assert.True(
            headerOnly.Count == 0,
            "SectionChunker must not emit header-only fragments (the SemanticSimilarityChunker regression). " +
            $"Found {headerOnly.Count}: {string.Join(" | ", headerOnly.Take(5))}");
    }

    [Fact]
    public async Task ChunksAreSubstantiveInAggregate()
    {
        var chunks = await ChunkGpsWatchAsync();

        foreach (var chunk in chunks)
        {
            Assert.False(
                string.IsNullOrWhiteSpace(chunk.Content),
                "Chunk content must not be empty.");
        }

        // The rejected SemanticSimilarityChunker flooded the store with ~180 chunks,
        // many of them tiny (~40-char) header-only fragments, which wrecked retrieval.
        // SectionChunker yields a modest number of substantive, prose-bearing chunks
        // (a few thin section-boundary chunks are acceptable). These aggregate checks
        // lock in "not fragmented into a flood of tiny header fragments".
        var lengths = chunks.Select(c => c.Content.Trim().Length).ToList();

        Assert.True(
            chunks.Count < 40,
            $"Chunk count {chunks.Count} is too high; the SemanticSimilarityChunker " +
            "regression produced ~180 fragments. SectionChunker yields far fewer.");

        var average = lengths.Average();
        Assert.True(
            average >= 200,
            $"Average chunk length {average:F0} is too small; healthy section chunks are " +
            "hundreds of chars, not the ~40-char fragments the old chunker produced.");

        var substantive = lengths.Count(len => len >= 100);
        Assert.True(
            substantive >= chunks.Count * 0.7,
            $"Only {substantive}/{chunks.Count} chunks carry substantive prose (>=100 chars); " +
            "expected at least 70%.");
    }

    [Fact]
    public async Task RetrievableContentIncludesKeyGpsFacts()
    {
        var chunks = await ChunkGpsWatchAsync();
        var allText = string.Join("\n", chunks.Select(c => c.Content));

        // Facts the model must be able to ground on — these live in the body, not headings.
        Assert.Contains("GLONASS", allText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("shock-resistant", allText, StringComparison.OrdinalIgnoreCase);
    }
}

