# AlphaQuantum.AcquisitionUniverse for .NET

C# helpers for acquisition teams: a thesis brief builder, a composite score with an ownership rule, ranking and signal coverage. Runs locally, no API calls, no dependencies.

```bash
dotnet add package AlphaQuantum.AcquisitionUniverse
```

## The setting

Corporate development groups often keep their own pipeline tool in .NET. When a screened list arrives, the same few questions come up: how strong is each fit, who is independent, and how much evidence stands behind each company. This package gives that tool the answers. The thinking behind it comes from [corporate development](https://www.acquisitionuniverse.com/for/corporate-development.php) workflows at Acquisition Universe.

## Score and rank

```csharp
using AlphaQuantum.AcquisitionUniverse;

var ranked = Scoring.Rank(new[]
{
    new Company("T-01", MandateFit: 91, OutreachSuitability: 80, TransitionContext: 60),
    new Company("T-02", MandateFit: 96, OutreachSuitability: 90, TransitionContext: 70, GroupOwned: true),
});

foreach (var r in ranked)
    Console.WriteLine($"{r.Company.Id}: {r.Composite}");
```

T-02 fits better but is group-owned, so its outreach component is zero and T-01 leads. Pass `new Weights(0.6, 0.3, 0.1)` as the second argument to change the balance.

## Brief

```csharp
var brief = new ThesisBrief
{
    Name = "Industrial automation, Great Lakes",
    Vertical = "Industrial automation integrators",
    Regions = new[] { "Ohio", "Michigan" },
    EmployeesMin = 15,
    EmployeesMax = 120,
    Exclusions = new[] { "group-owned" },
};

if (brief.Validate().Count == 0)
    Console.WriteLine(brief.ToText());
```

## Coverage

```csharp
var cov = Signals.Coverage(new Dictionary<string, string?>
{
    [Signals.All[0]] = "founder led",
    [Signals.All[13]] = null,
});
Console.WriteLine($"{cov.Visible} of {cov.Checked} visible");
```

## Members

| Type | Purpose |
|---|---|
| `Signals.All` | the 15 signal names |
| `Signals.Coverage(map)` | visible and checked counts |
| `Scoring.Composite(company, weights?)` | 0 to 100 |
| `Scoring.Rank(companies, weights?)` | `List<Ranked>` |
| `ThesisBrief` | `Validate()` and `ToText()` |
| `Weights` | defaults 0.7, 0.2, 0.1 |

## For advisors

Advisory shops use the ranking to explain why a call list is ordered the way it is. The page for [M&A advisors](https://www.acquisitionuniverse.com/for/ma-advisors.php) discusses that use. If you work with advertisers, see the [cookieless audience segmentation](https://www.cookielessaudiences.com/features/cookieless-audience-segmentation.php) service, and for hiring software the [resume parsing with OCR](https://www.resumereaderapi.com/) product.

<!--expanded-->
## Pipeline software in .NET

Finance teams often keep their tooling in .NET, close to the Excel add ins and the reporting systems they already use. A package for acquisition work should fit that world. This one does: records for the data, static methods for the arithmetic, and no hidden dependencies.

The package performs no network calls. Target lists describe the companies you plan to approach, and a library that never transmits them is a library your information security team will approve quickly.

## Records and pattern matching

The company type is a record, which gives value equality and `with` expressions for free. That is handy when you want to test how a change would affect the ranking without mutating the original.

```csharp
var original = new Company("T-07", MandateFit: 88, OutreachSuitability: 74, TransitionContext: 52);
var ifIndependent = original with { GroupOwned = false };
var ifOwned = original with { GroupOwned = true };

Console.WriteLine(Scoring.Composite(ifIndependent)); // higher
Console.WriteLine(Scoring.Composite(ifOwned));       // lower
```

The difference between the two lines is the cost of group ownership for that company, expressed in points. Showing it to a committee makes the rule concrete.

## Ranking inside a report

A report generator can call `Rank`, take the top rows and write them to a spreadsheet through whichever library you prefer. Keep the ranking separate from the writing, so you can test each part.

```csharp
var top = Scoring.Rank(companies).Take(25).Select((r, i) => new
{
    Rank = i + 1,
    r.Company.Id,
    r.Composite,
    Owned = r.Company.GroupOwned ? "yes" : "no",
});
```

Add the signal coverage next to each company if you hold the signal values. A column of coverage ratios tells readers which scores rest on rich evidence.

## Weights as configuration

Treat weights as configuration, not as constants buried in code. Bind them from settings:

```csharp
builder.Services.Configure<WeightsOptions>(builder.Configuration.GetSection("Weights"));

public sealed class WeightsOptions
{
    public double MandateFit { get; set; } = 0.7;
    public double OutreachSuitability { get; set; } = 0.2;
    public double TransitionContext { get; set; } = 0.1;
    public Weights ToWeights() => new(MandateFit, OutreachSuitability, TransitionContext);
}
```

Version the settings file, so that a ranking from March can be reproduced in June with the weights that were in force.

## Validation in forms

`ThesisBrief.Validate()` returns a list of strings, which maps neatly onto form validation messages. In an ASP.NET page, call it on submit and add each problem to the model state. Use `ToText()` to produce an email body, and the text will end with the address for requesting a pilot.

The most common validation problems are worth knowing. An empty vertical makes the brief meaningless. A missing region makes the footprint signal useless. An inverted size range is a typo. An empty exclusion list usually means the group has not decided what disqualifies a target.

<!--extra-->
## Vertical signals and reporting

A screening adds signals specific to your vertical on top of the fifteen common ones. For boiler and steam services, the list includes ASME code stamps, R-stamp repair authorization, combustion tuning programmes and twenty four hour emergency coverage. For coatings and surface finishing it includes NADCAP accreditation, aerospace approvals, ITAR registration and quoted turnaround commitments. Keep them in a dictionary on each company and show them in a second table in your report. The package counts only its fifteen known signals, so vertical extras never distort coverage, and you can compute a second ratio for them with a few lines of LINQ.

In the report itself, put the ranked table first, the coverage column beside it and the exclusions on a separate sheet. Partners read the first page, analysts read the rest, and everybody can see where a number came from.

A closing thought for enterprise teams: the biggest risk in a sourcing tool is quiet drift. Weights change, a signal is added, an import mapping is edited, and nobody records it. Treat the scoring configuration like code, review changes, and keep a changelog that a new analyst can read in five minutes.

<!--further-->
## Further reading and practical notes

The [.NET documentation](https://learn.microsoft.com/en-us/dotnet/) covers records, the options pattern, dependency injection and the testing tools mentioned above. For teams that run outreach campaigns on top of target lists, the [Federal Trade Commission](https://www.ftc.gov/) publishes guidance for businesses on advertising and commercial messages in the United States.

Practical notes for .NET teams. Put the scoring behind an interface only if you expect to swap it. Otherwise the static methods are simpler, and tests are just as easy. Register the weights as options, and log them at start up. Add an analyser rule that bans `double` equality comparisons in your scoring tests, and compare with a tolerance, because rounding to one decimal makes exact equality fragile in code that does arithmetic in a different order.

If your reports go to Excel, write the composite as a number and apply a number format in the cell instead of writing text. Partners sort and filter, and text breaks sorting. Write the coverage ratio as a percentage format and the group ownership flag as a plain yes or no. Add a header row with units, such as "composite (0 to 100)".

Think about concurrency if your ranking runs inside a web application. The functions are pure and the records are immutable, so they are safe to call from many threads. Cache the sorted list for a short time to avoid recomputing on every page view, and clear the cache when a weight or a record changes.

Last, document your process. A one page note that says how the brief is written, how the score is computed, how the list is reviewed and who may contact a company will serve you better in due diligence than any tool. Buyers who can describe their sourcing process win the trust of sellers and advisors.

## Testing

xUnit tests for this package are short. Cover the extremes, the group ownership rule, rounding, the custom weights and the coverage function. The package's own tests do that, and yours should add the cases that reflect your committee's habits, such as how you treat companies with missing scores.

## About the 15 signals

The package lists the signals in `Signals.All`. They describe publicly visible facts about a company, such as whether it is founder led, how many principals it names, how long it has operated, which certifications it claims and how it sells. Each is recorded in a screening file with a quote and a source, or marked as not visible. The package never invents a value, and neither should your code.

## Further reading

The [family office deal sourcing page](https://www.acquisitionuniverse.com/for/family-offices.php) describes how a long horizon buyer finds independent companies. Teams in advertising may enjoy the [cookieless audience segmentation guide](https://www.cookielessaudiences.com/features/cookieless-audience-segmentation.php), and teams in hiring can read the description of [resume parsing with OCR](https://www.resumereaderapi.com/) for scanned documents.

## Support

Report package problems on the repository. For questions about screening engagements, write to info@alpha-quantum.com. The package is MIT licensed.

## FAQ

**Does it need internet access?** No.

**Why records?** They give value equality and short syntax for scores.

MIT license. info@alpha-quantum.com
