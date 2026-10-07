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

## FAQ

**Does it need internet access?** No.

**Why records?** They give value equality and short syntax for scores.

MIT license. info@alpha-quantum.com
