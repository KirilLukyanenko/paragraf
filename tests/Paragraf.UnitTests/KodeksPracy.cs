using Paragraf.Core.Law;

namespace Paragraf.UnitTests;

/// <summary>The consolidated Labour Code in the repository, parsed once for all tests.</summary>
public static class KodeksPracy
{
    public static readonly DateOnly StateOfLaw = new(2026, 8, 27);

    private static readonly Lazy<string> RawText = new(() => File.ReadAllText(Path.Combine(RepoRoot(), "data", "kodeks-pracy", "DU-2026-1245.txt")));
    private static readonly Lazy<Act> Parsed = new(() => LawParser.Parse(RawText.Value, "DU/2026/1245", "Kodeks pracy", StateOfLaw));

    public static string Text => RawText.Value;
    public static Act Act => Parsed.Value;

    public static string RepoRoot([System.Runtime.CompilerServices.CallerFilePath] string sourceFile = "")
    {
        // Builds may go to a folder outside the repository, so start from this source file.
        var dir = new DirectoryInfo(Path.GetDirectoryName(sourceFile)!);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Paragraf.slnx"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
