using System.Text.Json;
using Microsoft.Extensions.AI;
using Paragraf.Rag;

namespace Paragraf.IntegrationTests;

/// <summary>
/// A model that answers each request with the next scripted reply, serialised as JSON, and keeps what it was sent.
/// After the last reply it repeats it. Every reply costs 1,000 input and 100 output tokens.
/// </summary>
public sealed class ScriptedModel(params object[] answers) : IChatClient
{
    private int _next;
    private readonly Lock _lock = new();
    public List<List<ChatMessage>> Requests { get; } = [];

    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        object answer;
        lock (_lock)
        {
            Requests.Add(messages.ToList());
            answer = answers[Math.Min(_next++, answers.Length - 1)];
        }
        if (answer is Exception failure) throw failure; // a scripted outage
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, JsonSerializer.Serialize(answer)))
        {
            Usage = new UsageDetails { InputTokenCount = 1000, OutputTokenCount = 100 },
        });
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }

    /// <summary>An answer to a notice-period question, citing art. 36 § 1.</summary>
    public static object NoticeAnswer(string text = "The notice period is 3 months, because the employee has worked there for more than 3 years.") => new
    {
        status = "answered",
        answer = text,
        citations = new[] { new { article = "36", paragraph = "1", quote = "3 miesiące, jeżeli pracownik był zatrudniony co najmniej 3 lata" } },
    };
}

/// <summary>Embeddings that put every text on the same axis: the vector list is then arbitrary but valid.</summary>
public sealed class FixedEmbeddings : IEmbeddingGenerator<string, Embedding<float>>
{
    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
    {
        var vector = new float[ChunkStore.Dimensions];
        vector[0] = 1f;
        return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(values.Select(_ => new Embedding<float>(vector))));
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;
    public void Dispose() { }
}
