using Paragraf.Core.Law;
using Paragraf.Rag;
using Testcontainers.PostgreSql;

namespace Paragraf.IntegrationTests;

/// <summary>The store against a real PostgreSQL with pgvector, as in production (Docker required).</summary>
public sealed class ChunkStoreTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("pgvector/pgvector:pg17")
        .WithPassword(Guid.NewGuid().ToString("N"))
        .Build();

    private static readonly DateOnly BeforeAmendment = new(2026, 11, 1);
    private static readonly DateOnly AfterAmendment = new(2026, 11, 23);

    private static string SourceDirectory([System.Runtime.CompilerServices.CallerFilePath] string file = "") => Path.GetDirectoryName(file)!;

    public async ValueTask InitializeAsync() => await _db.StartAsync(TestContext.Current.CancellationToken);
    public async ValueTask DisposeAsync() => await _db.DisposeAsync();

    /// <summary>A unit vector along one axis: nearest-neighbour order is then known exactly.</summary>
    private static float[] Axis(int i, float weight = 1f)
    {
        var v = new float[ChunkStore.Dimensions];
        v[i] = weight;
        v[(i + 1) % v.Length] = 1f - weight;
        return v;
    }

    private static Chunk Chunk(string id, string article, Validity validity) =>
        new(id, article, null, validity, "Dział", $"Art. {article}.", $"Art. {article}.");

    private async Task<ChunkStore> SeededStore()
    {
        var store = new ChunkStore(ChunkStore.CreateDataSource(_db.GetConnectionString()));
        await store.RecreateSchemaAsync(ct: TestContext.Current.CancellationToken);
        await store.ReplaceAsync("act", "test-embeddings",
        [
            (Chunk("1", "1", Validity.Always), Axis(0)),
            (Chunk("2", "2", Validity.Always), Axis(1)),
            (Chunk("3-old", "3", new Validity(null, new DateOnly(2026, 11, 5))), Axis(2, 0.9f)),
            (Chunk("3-new", "3", new Validity(new DateOnly(2026, 11, 5), null)), Axis(2, 0.95f)),
        ], TestContext.Current.CancellationToken);
        return store;
    }

    [Fact]
    public async Task Nearest_chunks_come_in_order_of_cosine_distance()
    {
        var store = await SeededStore();
        var hits = await store.NearestAsync("act", Axis(1), AfterAmendment, limit: 2, ct: TestContext.Current.CancellationToken);
        Assert.Equal("2", hits[0].Id);
        Assert.Equal(1.0, hits[0].Score, precision: 3);
    }

    [Fact]
    public async Task Only_the_version_in_force_on_the_date_is_found()
    {
        var store = await SeededStore();
        var before = await store.NearestAsync("act", Axis(2), BeforeAmendment, limit: 10, ct: TestContext.Current.CancellationToken);
        var after = await store.NearestAsync("act", Axis(2), AfterAmendment, limit: 10, ct: TestContext.Current.CancellationToken);
        Assert.Contains(before, h => h.Id == "3-old");
        Assert.DoesNotContain(before, h => h.Id == "3-new");
        Assert.Contains(after, h => h.Id == "3-new");
        Assert.DoesNotContain(after, h => h.Id == "3-old");
    }

    [Fact]
    public async Task Replacing_an_act_leaves_no_old_chunks_and_recreating_the_schema_empties_it()
    {
        var store = await SeededStore();
        await store.ReplaceAsync("act", "test-embeddings", [(Chunk("9", "9", Validity.Always), Axis(9))], TestContext.Current.CancellationToken);
        Assert.Equal(1, await store.CountAsync("act", ct: TestContext.Current.CancellationToken));
        await store.RecreateSchemaAsync(ct: TestContext.Current.CancellationToken);
        Assert.Equal(0, await store.CountAsync("act", ct: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task The_real_code_chunks_fit_the_schema()
    {
        var library = LawLibrary.KodeksPracy(LawLibrary.FindDataDirectory(SourceDirectory(), AppContext.BaseDirectory));
        var store = new ChunkStore(ChunkStore.CreateDataSource(_db.GetConnectionString()));
        await store.RecreateSchemaAsync(ct: TestContext.Current.CancellationToken);
        await store.ReplaceAsync(library.Act.Eli, "test-embeddings",
            library.Chunks.Select((c, i) => (c, Axis(i % ChunkStore.Dimensions))).ToList(), TestContext.Current.CancellationToken);
        Assert.Equal(library.Chunks.Count, await store.CountAsync(library.Act.Eli, ct: TestContext.Current.CancellationToken));
    }
}
