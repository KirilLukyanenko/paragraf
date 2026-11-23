using Dapper;
using Npgsql;
using Paragraf.Core.Law;
using Paragraf.Core.Search;
using Pgvector;
using Pgvector.Dapper;

namespace Paragraf.Rag;

/// <summary>
/// Chunks of the act in PostgreSQL: the text, its embedding (pgvector, HNSW index) and its lexical
/// terms (a tsvector over <see cref="LexicalTerms"/>), so both halves of the hybrid search are one
/// SQL query each and filter by the date the same way.
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

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await connection.ExecuteAsync("create extension if not exists vector");
        await connection.ReloadTypesAsync(ct);
        await connection.ExecuteAsync($"""
            create table if not exists chunks (
                id          text primary key,
                act         text not null,
                article     text not null,
                paragraph   text,
                valid_from  date,
                valid_until date,
                citation    text not null,
                headings    text not null,
                body        text not null,
                terms       tsvector not null,
                embedding   vector({Dimensions}) not null
            );
            create index if not exists chunks_embedding on chunks using hnsw (embedding vector_cosine_ops);
            create index if not exists chunks_terms on chunks using gin (terms);
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
                insert into chunks (id, act, article, paragraph, valid_from, valid_until, citation, headings, body, terms, embedding)
                values (@Id, @Act, @Article, @Paragraph, @From, @Until, @Citation, @Headings, @Body, to_tsvector('simple', @Terms), @Embedding)
                """,
                new
                {
                    chunk.Id, Act = act, chunk.Article, chunk.Paragraph,
                    From = chunk.Validity.From, Until = chunk.Validity.Until,
                    chunk.Citation, chunk.Headings, chunk.Body,
                    Terms = LexicalTerms.Document(chunk.Content),
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

    public async Task<IReadOnlyList<Hit>> LexicalAsync(string act, IReadOnlyList<string> terms, DateOnly date, int limit, CancellationToken ct = default)
    {
        if (terms.Count == 0) return [];
        // Terms are letters and digits only (LexicalTerms), so joining them with | is a safe tsquery.
        var query = string.Join(" | ", terms.Distinct());
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var hits = await connection.QueryAsync<Hit>($"""
            select id, article, paragraph, ts_rank_cd(terms, q)::float8 as score
            from chunks, to_tsquery('simple', @query) q
            where act = @act and {InForce} and terms @@ q
            order by score desc, id
            limit @limit
            """, new { act, query, date, limit });
        return hits.ToList();
    }

    public async Task<int> CountAsync(string act, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.ExecuteScalarAsync<int>("select count(*) from chunks where act = @act", new { act });
    }
}
