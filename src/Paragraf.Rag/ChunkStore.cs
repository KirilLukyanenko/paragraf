using Dapper;
using Npgsql;
using Paragraf.Core.Law;
using Pgvector;
using Pgvector.Dapper;

namespace Paragraf.Rag;

/// <summary>
/// Chunks of the act in PostgreSQL with their embeddings (pgvector, HNSW index, cosine distance),
/// searched with the date filter in the same query.
/// </summary>
public sealed class ChunkStore(NpgsqlDataSource dataSource)
{
    public const int Dimensions = 1536;

    static ChunkStore() => SqlMapper.AddTypeHandler(new VectorTypeHandler());

    public static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseVector();
        return builder.Build();
    }

    /// <summary>Chunks are derived data: each ingestion drops and recreates their table, so the schema follows the code.</summary>
    public async Task RecreateSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await connection.ExecuteAsync("create extension if not exists vector");
        await connection.ReloadTypesAsync(ct);
        await connection.ExecuteAsync($"""
            drop table if exists chunks;
            create table chunks (
                id          text primary key,
                act         text not null,
                article     text not null,
                paragraph   text,
                valid_from  date,
                valid_until date,
                citation    text not null,
                headings    text not null,
                body        text not null,
                embedding   vector({Dimensions}) not null
            );
            create index chunks_embedding on chunks using hnsw (embedding vector_cosine_ops);
            create table if not exists ingestions (
                id              bigint generated always as identity primary key,
                act             text not null,
                embedding_model text not null,
                chunks          int not null,
                at              timestamptz not null default now()
            );
            """);
    }

    /// <summary>Replaces every chunk of the act in one transaction, so a search never sees half an act.</summary>
    public async Task ReplaceAsync(string act, string embeddingModel, IReadOnlyList<(Chunk Chunk, float[] Embedding)> rows, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        await connection.ExecuteAsync("delete from chunks where act = @act", new { act }, tx);
        foreach (var (chunk, embedding) in rows)
        {
            await connection.ExecuteAsync("""
                insert into chunks (id, act, article, paragraph, valid_from, valid_until, citation, headings, body, embedding)
                values (@Id, @Act, @Article, @Paragraph, @From, @Until, @Citation, @Headings, @Body, @Embedding)
                """,
                new
                {
                    chunk.Id, Act = act, chunk.Article, chunk.Paragraph,
                    From = chunk.Validity.From, Until = chunk.Validity.Until,
                    chunk.Citation, chunk.Headings, chunk.Body,
                    Embedding = new Vector(embedding),
                }, tx);
        }
        await connection.ExecuteAsync("insert into ingestions (act, embedding_model, chunks) values (@act, @embeddingModel, @count)",
            new { act, embeddingModel, count = rows.Count }, tx);
        await tx.CommitAsync(ct);
    }

    public sealed record Hit(string Id, string Article, string? Paragraph, double Score);

    private const string InForce = "(valid_from is null or valid_from <= @date) and (valid_until is null or valid_until > @date)";

    public async Task<IReadOnlyList<Hit>> NearestAsync(string act, float[] embedding, DateOnly date, int limit, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(ct);
        // Filtering after the HNSW scan can leave fewer than `limit` rows; a wider beam keeps enough.
        await connection.ExecuteAsync("set local hnsw.ef_search = 200", transaction: tx);
        var hits = await connection.QueryAsync<Hit>($"""
            select id, article, paragraph, (1 - (embedding <=> @embedding))::float8 as score
            from chunks
            where act = @act and {InForce}
            order by embedding <=> @embedding
            limit @limit
            """, new { act, embedding = new Vector(embedding), date, limit }, tx);
        return hits.ToList();
    }

    public async Task<int> CountAsync(string act, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>("select count(*) from chunks where act = @act", new { act });
    }
}
