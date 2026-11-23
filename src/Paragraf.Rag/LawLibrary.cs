using Paragraf.Core.Law;

namespace Paragraf.Rag;

/// <summary>
/// The act in memory: parsed once at start from the text in the repository. Articles for the
/// prompt and for citation checks come from here; the database only answers "which chunks".
/// </summary>
public sealed class LawLibrary
{
    public Act Act { get; }
    public IReadOnlyList<Chunk> Chunks { get; }
    private readonly Dictionary<string, Chunk> _byId;

    public LawLibrary(Act act)
    {
        Act = act;
        Chunks = Chunker.Chunk(act);
        _byId = Chunks.ToDictionary(c => c.Id);
    }

    public static LawLibrary KodeksPracy(string dataDirectory) => new(LawParser.Parse(
        File.ReadAllText(Path.Combine(dataDirectory, "kodeks-pracy", "DU-2026-1245.txt")),
        eli: "DU/2026/1245",
        title: "Kodeks pracy",
        stateOfLaw: new DateOnly(2026, 8, 27)));

    public Chunk? Chunk(string id) => _byId.GetValueOrDefault(id);

    /// <summary>Finds the data directory by walking up from a start folder (the app's or the current one).</summary>
    public static string FindDataDirectory(params string[] starts)
    {
        foreach (var start in starts)
        {
            for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, "data");
                if (File.Exists(Path.Combine(candidate, "kodeks-pracy", "DU-2026-1245.txt"))) return candidate;
            }
        }
        throw new DirectoryNotFoundException("data/kodeks-pracy/DU-2026-1245.txt not found; set Paragraf:DataDirectory.");
    }
}
