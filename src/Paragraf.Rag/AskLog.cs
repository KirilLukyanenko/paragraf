using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using Npgsql;

namespace Paragraf.Rag;

/// <summary>
/// The questions the public API answered, kept for a limited time (<c>Paragraf:KeepAsksDays</c>, 30 by default), for
/// three things: a repeated question gets the earlier answer without a model call; a visitor can mark an answer as
/// helpful or not; and the owner reads the misses (<c>paragraf asks</c>) to turn them into eval questions.
/// </summary>
public sealed partial class AskLog(NpgsqlDataSource dataSource, TimeProvider clock, int keepDays = 30)
{
    private readonly Lock _lock = new();
    private DateTimeOffset _purgedAt = DateTimeOffset.MinValue;

    public int KeepDays => keepDays;

    /// <summary>One ask. <see cref="Response"/> is the API's JSON; <see cref="CachedFrom"/> is set when it was a repeat.</summary>
    public sealed record Entry(
        Guid Id, DateTimeOffset At, string Question, DateOnly AsOf, string Model, string Version, string Status,
        bool Grounded, bool Cacheable, decimal CostUsd, string? TraceId, Guid? CachedFrom, bool? Helpful, string Response);

    public async Task EnsureSchemaAsync(CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await connection.ExecuteAsync("""
            create table if not exists asks (
                id           uuid primary key,
                at           timestamptz not null,
                question     text not null,
                question_key text not null,
                as_of        date not null,
                model        text not null,
                version      text not null,
                status       text not null,
                grounded     boolean not null,
                cacheable    boolean not null,
                cost_usd     numeric(12, 6) not null,
                trace_id     text,
                cached_from  uuid,
                helpful      boolean,
                response     jsonb not null
            );
            create index if not exists asks_repeat on asks (question_key, as_of, model, version, at desc)
                where cached_from is null and cacheable;
            create index if not exists asks_at on asks (at);
            """);
    }

    /// <summary>The same question in other spacing or case is the same question.</summary>
    public static string Key(string question)
    {
        var normal = Spaces().Replace(question.Trim(), " ").ToLowerInvariant();
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normal)));
    }

    /// <summary>
    /// The latest cacheable answer to the same question for the same date, from the same model and build, younger than
    /// <paramref name="maxAge"/>. The build is part of the key, so a deployment with a fix never serves an old answer.
    /// </summary>
    public async Task<Entry?> FindRepeatAsync(string question, DateOnly asOf, string model, string version, TimeSpan maxAge, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var row = await connection.QuerySingleOrDefaultAsync<Row>($"""
            select {Columns} from asks
            where question_key = @key and as_of = @asOf and model = @model and version = @version
              and cached_from is null and cacheable and at > @since
            order by at desc limit 1
            """, new { key = Key(question), asOf, model, version, since = clock.GetUtcNow() - maxAge });
        return row?.ToEntry();
    }

    public async Task RecordAsync(Entry entry, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await connection.ExecuteAsync("""
            insert into asks (id, at, question, question_key, as_of, model, version, status, grounded, cacheable, cost_usd, trace_id, cached_from, response)
            values (@Id, @At, @Question, @Key, @AsOf, @Model, @Version, @Status, @Grounded, @Cacheable, @CostUsd, @TraceId, @CachedFrom, @Response::jsonb)
            """,
            new
            {
                entry.Id, entry.At, entry.Question, Key = Key(entry.Question), entry.AsOf, entry.Model, entry.Version, entry.Status,
                entry.Grounded, entry.Cacheable, entry.CostUsd, entry.TraceId, entry.CachedFrom, entry.Response,
            });
        await PurgeIfDueAsync(connection);
    }

    /// <summary>Marks an answer as helpful or not; false when there is no such ask (or it has been purged).</summary>
    public async Task<bool> RateAsync(Guid id, bool helpful, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        return await connection.ExecuteAsync("update asks set helpful = @helpful where id = @id", new { id, helpful }) == 1;
    }

    public async Task<IReadOnlyList<Entry>> SinceAsync(DateTimeOffset since, CancellationToken ct = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        var rows = await connection.QueryAsync<Row>($"select {Columns} from asks where at > @since order by at", new { since });
        return rows.Select(r => r.ToEntry()).ToList();
    }

    /// <summary>Asks older than the retention period are deleted, checked at most once an hour.</summary>
    private async Task PurgeIfDueAsync(NpgsqlConnection connection)
    {
        var now = clock.GetUtcNow();
        lock (_lock)
        {
            if (now - _purgedAt < TimeSpan.FromHours(1)) return;
            _purgedAt = now;
        }
        await connection.ExecuteAsync("delete from asks where at < @cutoff", new { cutoff = now.AddDays(-keepDays) });
    }

    private const string Columns =
        "id, at, question, as_of as AsOf, model, version, status, grounded, cacheable, cost_usd as CostUsd, " +
        "trace_id as TraceId, cached_from as CachedFrom, helpful, response::text as Response";

    /// <summary>The row as Npgsql reads it: timestamptz and date come back as DateTime.</summary>
    private sealed class Row
    {
        public Guid Id { get; init; }
        public DateTime At { get; init; }
        public string Question { get; init; } = "";
        public DateTime AsOf { get; init; }
        public string Model { get; init; } = "";
        public string Version { get; init; } = "";
        public string Status { get; init; } = "";
        public bool Grounded { get; init; }
        public bool Cacheable { get; init; }
        public decimal CostUsd { get; init; }
        public string? TraceId { get; init; }
        public Guid? CachedFrom { get; init; }
        public bool? Helpful { get; init; }
        public string Response { get; init; } = "";

        public Entry ToEntry() => new(Id, new DateTimeOffset(DateTime.SpecifyKind(At, DateTimeKind.Utc)), Question, DateOnly.FromDateTime(AsOf),
            Model, Version, Status, Grounded, Cacheable, CostUsd, TraceId, CachedFrom, Helpful, Response);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();
}
