using System.Text.RegularExpressions;

namespace Paragraf.Core.Answers;

/// <summary>
/// Which language a text is in, among those the service meets: Polish, English and Ukrainian (answered), Russian
/// (asked by expats), and Spanish, German and French (seen when a model drifts). Counts common short words and
/// letters peculiar to each language. Good enough to tell an answer in the wrong language, not a general detector.
/// </summary>
public static partial class LanguageCheck
{
    private static readonly Dictionary<string, HashSet<string>> Words = new()
    {
        ["pl"] = ["i", "w", "z", "na", "nie", "się", "jest", "do", "to", "że", "o", "od", "po", "przez", "dla", "jeżeli", "który", "która", "pracownik", "pracodawca", "lub", "oraz", "ile", "czy", "jak", "mi", "mnie", "jaki", "jaka", "jakie", "jakiej", "kiedy", "którym", "musi", "mogę", "może", "pracy", "umowy", "umowę", "urlopu", "roku", "była", "był", "pod", "za", "przy", "ten", "ta", "są", "mam"],
        ["en"] = ["the", "and", "of", "to", "is", "in", "for", "that", "a", "an", "on", "be", "are", "you", "your", "if", "employee", "employer", "with", "what", "how", "does", "it", "i", "m", "my", "me", "can", "have", "still", "just", "them", "need", "from", "years", "give", "work", "this", "they", "will", "when", "after", "get", "or", "not", "no"],
        ["es"] = ["el", "la", "los", "las", "de", "que", "y", "en", "es", "por", "para", "con", "un", "una", "del", "se", "período", "empleado"],
        ["de"] = ["der", "die", "das", "und", "ist", "nicht", "ein", "eine", "zu", "mit", "für", "den", "dem", "von", "auf"],
        ["fr"] = ["le", "la", "les", "de", "des", "et", "est", "un", "une", "du", "que", "pour", "dans", "en"],
        ["uk"] = ["і", "та", "не", "на", "що", "як", "для", "це", "до", "від", "або", "якщо", "працівник", "роботодавець", "скільки", "мені", "чи"],
        ["ru"] = ["и", "не", "на", "что", "как", "для", "это", "до", "от", "или", "если", "работник", "работодатель", "сколько", "мне", "ли"],
    };

    [GeneratedRegex(@"\p{L}+")]
    private static partial Regex WordPattern();

    /// <summary>A language code, or null when the text is too short or unclear to tell.</summary>
    public static string? Detect(string text)
    {
        var words = WordPattern().Matches(text.ToLowerInvariant()).Select(m => m.Value).ToList();
        if (words.Count < 3) return null;
        var cyrillic = words.Count(w => w.Any(c => c is >= 'а' and <= 'я' or 'і' or 'ї' or 'є' or 'ґ' or 'ё'));
        var scores = new Dictionary<string, double>();
        foreach (var (lang, set) in Words)
        {
            var isCyrillicLang = lang is "uk" or "ru";
            if (isCyrillicLang != (cyrillic * 2 > words.Count)) continue;
            scores[lang] = words.Count(set.Contains);
        }
        var letters = string.Concat(words);
        if (scores.ContainsKey("pl")) scores["pl"] += letters.Count(c => "ąćęłńóśźż".Contains(c)) * 0.5;
        if (scores.ContainsKey("uk")) scores["uk"] += letters.Count(c => "іїєґ".Contains(c)) * 0.5;
        if (scores.ContainsKey("ru")) scores["ru"] += letters.Count(c => "ыэъё".Contains(c)) * 0.5;
        if (scores.ContainsKey("es")) scores["es"] += letters.Count(c => "ñ¿¡".Contains(c)) * 0.5;
        if (scores.ContainsKey("de")) scores["de"] += letters.Count(c => "äöüß".Contains(c)) * 0.5;
        var best = scores.OrderByDescending(s => s.Value).ToList();
        if (best.Count == 0 || best[0].Value < 2) return null;
        return best.Count > 1 && best[0].Value == best[1].Value ? null : best[0].Key;
    }

    /// <summary>
    /// The answer's language when it is clearly not the question's, else null. Quotes of the law (in Polish) inside
    /// an answer in another language are left out before detecting.
    /// </summary>
    public static (string Expected, string Actual)? Mismatch(string question, string answer)
    {
        var expected = Detect(question);
        var actual = DetectAnswer(answer);
        return expected is null || actual is null || expected == actual ? null : (expected, actual);
    }

    /// <summary>The language of an answer, leaving out the quotes of the law it contains.</summary>
    public static string? DetectAnswer(string answer) => Detect(QuotePattern().Replace(answer, " "));

    [GeneratedRegex("[„\"«“][^”\"»„“]*[”\"»“]")]
    private static partial Regex QuotePattern();

    public static string Name(string code) => code switch
    {
        "pl" => "Polish",
        "en" => "English",
        "uk" => "Ukrainian",
        "ru" => "Russian",
        "es" => "Spanish",
        "de" => "German",
        "fr" => "French",
        _ => code,
    };
}
