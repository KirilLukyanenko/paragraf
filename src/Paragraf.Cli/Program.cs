using System.Text;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Paragraf.Cli.Evals;
using Paragraf.Rag;

Console.OutputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = [],
    EnvironmentName = Environments.Development, // reads user secrets
});
builder.Logging.SetMinimumLevel(LogLevel.Warning);
builder.Services.AddParagraf(builder.Configuration);
builder.Services.AddSingleton<EvalRunner>();
using var host = builder.Build();
var services = host.Services;
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var command = args.FirstOrDefault() ?? "help";
var options = Options.Parse(args.Skip(1).ToArray());

switch (command)
{
    case "ingest":
    {
        var result = await services.GetRequiredService<Ingestion>().RunAsync(Console.WriteLine, cts.Token);
        Console.WriteLine($"{result.Chunks} chunks, {result.Tokens} tokens embedded, ${result.CostUsd:0.0000}");
        return 0;
    }
    case "ask":
    {
        var question = options.Positional.FirstOrDefault() ?? throw new ArgumentException("ask \"question\" [--as-of yyyy-MM-dd]");
        var engine = services.GetRequiredService<ParagrafEngine>();
        var settings = options.Apply(engine.Defaults);
        var answer = await engine.AskAsync(question, options.AsOf, settings, cts.Token);
        Console.WriteLine($"[{answer.Status}] as of {answer.AsOf:yyyy-MM-dd}");
        Console.WriteLine(answer.Text);
        Console.WriteLine();
        foreach (var c in answer.Citations)
        {
            Console.WriteLine($"  {(c.Verified ? "OK" : "!! " + c.Problem)} {c.Display ?? c.Citation.Article}: „{c.Citation.Quote}”");
        }
        Console.WriteLine();
        Console.WriteLine($"sources: {string.Join(", ", answer.Retrieval.Articles.Select(a => "art. " + a.Article.Number.ToDisplay()))}");
        if (answer.Retrieval.Rewrite is { } rw) Console.WriteLine($"search query: {rw.PolishQuery} [{string.Join(", ", rw.Keywords)}]");
        var cost = answer.CostUsd + Pricing.Cost(settings.RewriteModel, answer.Retrieval.RewriteUsage);
        Console.WriteLine($"retrieval {answer.Retrieval.Elapsed.TotalSeconds:0.0}s, answer {answer.GenerationTime.TotalSeconds:0.0}s, " +
                          $"{answer.Usage?.InputTokenCount} in / {answer.Usage?.OutputTokenCount} out tokens, ${cost:0.0000}");
        return 0;
    }
    case "eval":
    {
        var data = LawLibrary.FindDataDirectory(AppContext.BaseDirectory, Environment.CurrentDirectory);
        var repo = Directory.GetParent(data)!.FullName;
        IReadOnlyList<EvalQuestion> questions = EvalQuestion.Load(Path.Combine(repo, "evals", "questions.jsonl"));
        if (options.Ids.Count > 0) questions = questions.Where(q => options.Ids.Contains(q.Id)).ToList();
        if (options.Categories.Count > 0) questions = questions.Where(q => options.Categories.Contains(q.Category)).ToList();
        if (options.Sample is { } n)
        {
            // An even spread over the file, which is ordered by category.
            var step = Math.Max(1, questions.Count / n);
            questions = questions.Where((_, i) => i % step == 0).Take(n).ToList();
        }

        var engine = services.GetRequiredService<ParagrafEngine>();
        var evalOptions = new EvalOptions
        {
            Name = options.Name ?? $"{DateTime.UtcNow:yyyyMMdd-HHmm}",
            Settings = options.Apply(engine.Defaults),
            RetrievalOnly = options.RetrievalOnly,
            JudgeModel = options.JudgeModel ?? "claude-sonnet-5-5",
            Parallelism = options.Parallelism ?? 4,
        };
        Console.WriteLine($"{questions.Count} questions: {EvalRunner.Describe(evalOptions)}");
        var (summary, results) = await services.GetRequiredService<EvalRunner>().RunAsync(questions, evalOptions, Console.WriteLine, cts.Token);
        await EvalRunner.WriteAsync(Path.Combine(repo, "evals", "runs"), summary, results);
        Console.WriteLine();
        Console.WriteLine(EvalRunner.Markdown(summary, results).Split("## Misses")[0]);
        Console.WriteLine($"written to evals/runs/{summary.Name}.json and .md");
        return 0;
    }
    default:
        Console.WriteLine("""
            paragraf ingest                         embed the Labour Code and load it into PostgreSQL
            paragraf ask "question" [options]       answer one question
            paragraf eval [options]                 run the eval set (evals/questions.jsonl)

            options: --as-of yyyy-MM-dd  --model M  --effort low|medium|high  --articles K  --lexical-weight W
                     --no-vector --no-lexical --no-named --no-rewrite
                     eval: --retrieval-only --name N --sample N --ids q001,q002 --category lookup --judge M --parallel N
            """);
        return command == "help" ? 0 : 1;
}

internal sealed class Options
{
    public List<string> Positional { get; } = [];
    public DateOnly AsOf { get; private set; } = DateOnly.FromDateTime(DateTime.Today);
    public string? Model { get; private set; }
    public ReasoningEffort? Effort { get; private set; }
    public int? Articles { get; private set; }
    public double? LexicalWeight { get; private set; }
    public bool NoVector { get; private set; }
    public bool NoLexical { get; private set; }
    public bool NoNamed { get; private set; }
    public bool NoRewrite { get; private set; }
    public bool RetrievalOnly { get; private set; }
    public string? Name { get; private set; }
    public int? Sample { get; private set; }
    public HashSet<string> Ids { get; } = [];
    public HashSet<string> Categories { get; } = [];
    public string? JudgeModel { get; private set; }
    public int? Parallelism { get; private set; }

    public static Options Parse(string[] args)
    {
        var o = new Options();
        for (var i = 0; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"{args[i]} needs a value");
            switch (args[i])
            {
                case "--as-of": o.AsOf = DateOnly.Parse(Next()); break;
                case "--model": o.Model = Next(); break;
                case "--effort":
                    o.Effort = Next() switch
                    {
                        "low" => ReasoningEffort.Low,
                        "medium" => ReasoningEffort.Medium,
                        "high" => ReasoningEffort.High,
                        var e => throw new ArgumentException($"unknown effort {e}"),
                    };
                    break;
                case "--articles": o.Articles = int.Parse(Next()); break;
                case "--lexical-weight": o.LexicalWeight = double.Parse(Next(), System.Globalization.CultureInfo.InvariantCulture); break;
                case "--no-vector": o.NoVector = true; break;
                case "--no-lexical": o.NoLexical = true; break;
                case "--no-named": o.NoNamed = true; break;
                case "--no-rewrite": o.NoRewrite = true; break;
                case "--retrieval-only": o.RetrievalOnly = true; break;
                case "--name": o.Name = Next(); break;
                case "--sample": o.Sample = int.Parse(Next()); break;
                case "--ids": foreach (var id in Next().Split(',')) o.Ids.Add(id.Trim()); break;
                case "--category": foreach (var c in Next().Split(',')) o.Categories.Add(c.Trim()); break;
                case "--judge": o.JudgeModel = Next(); break;
                case "--parallel": o.Parallelism = int.Parse(Next()); break;
                default: o.Positional.Add(args[i]); break;
            }
        }
        return o;
    }

    public ParagrafSettings Apply(ParagrafSettings s) => s with
    {
        Retrieval = s.Retrieval with
        {
            Vector = s.Retrieval.Vector && !NoVector,
            Lexical = s.Retrieval.Lexical && !NoLexical,
            NamedArticles = s.Retrieval.NamedArticles && !NoNamed,
            Rewrite = s.Retrieval.Rewrite && !NoRewrite,
            Articles = Articles ?? s.Retrieval.Articles,
            LexicalWeight = LexicalWeight ?? s.Retrieval.LexicalWeight,
        },
        Answer = s.Answer with
        {
            Model = Model ?? s.Answer.Model,
            Effort = Effort ?? s.Answer.Effort,
        },
    };
}
