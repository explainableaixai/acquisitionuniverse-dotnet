namespace AlphaQuantum.AcquisitionUniverse;

/// <summary>Local toolkit for acquisition target screening. No network calls.</summary>
public static class Signals
{
    public const string PilotUrl = "https://www.acquisitionuniverse.com/free-pilot.php";

    /// <summary>The 15 common signals, each recorded with a verbatim quote and a source URL, or "not visible".</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        "Founder or family association",
        "Visible leadership bench depth",
        "Operating history and continued independence",
        "Strategic fit to thesis",
        "Geographic and branch footprint",
        "Service-led vs product-led model",
        "Recurring-offering indicators",
        "Vertical specialization and end-market exposure",
        "Acquisition-program or roll-up readiness",
        "Management professionalization",
        "Hiring posture and functional investment",
        "Website and news activity trajectory",
        "Partner and channel ecosystem position",
        "Compliance and regulated-market readiness",
        "Digital-commercial maturity",
    };

    /// <summary>Visible and checked counts. A null value or "not visible" counts as silent.</summary>
    public static (int Visible, int Checked) Coverage(IReadOnlyDictionary<string, string?> found)
    {
        var known = All.Where(found.ContainsKey).ToList();
        var visible = known.Count(s => !string.IsNullOrEmpty(found[s]) && found[s] != "not visible");
        return (visible, known.Count);
    }
}

public sealed record Weights(double MandateFit = 0.7, double OutreachSuitability = 0.2, double TransitionContext = 0.1);

public sealed record Company(
    string Id,
    double MandateFit = 0,
    double OutreachSuitability = 0,
    double TransitionContext = 0,
    bool GroupOwned = false);

public sealed record Ranked(Company Company, double Composite);

public static class Scoring
{
    /// <summary>Weighted composite on a 0 to 100 scale. A group-owned company scores zero on outreach suitability.</summary>
    public static double Composite(Company c, Weights? w = null)
    {
        w ??= new Weights();
        var outreach = c.GroupOwned ? 0 : c.OutreachSuitability;
        var total = w.MandateFit + w.OutreachSuitability + w.TransitionContext;
        var v = (c.MandateFit * w.MandateFit + outreach * w.OutreachSuitability + c.TransitionContext * w.TransitionContext) / total;
        return Math.Round(v, 1, MidpointRounding.AwayFromZero);
    }

    /// <summary>Companies with their composite, highest first.</summary>
    public static List<Ranked> Rank(IEnumerable<Company> companies, Weights? w = null) =>
        companies.Select(c => new Ranked(c, Composite(c, w))).OrderByDescending(r => r.Composite).ToList();
}

public sealed class ThesisBrief
{
    public string Name { get; init; } = "Untitled thesis";
    public string Vertical { get; init; } = "";
    public IReadOnlyList<string> Subverticals { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Regions { get; init; } = Array.Empty<string>();
    public int? EmployeesMin { get; init; }
    public int? EmployeesMax { get; init; }
    public IReadOnlyList<string> MustHave { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Exclusions { get; init; } = Array.Empty<string>();
    public string Notes { get; init; } = "";

    /// <summary>Problems that would make the brief ambiguous to a screening team.</summary>
    public List<string> Validate()
    {
        var problems = new List<string>();
        if (Vertical.Length == 0) problems.Add("vertical is empty");
        if (Regions.Count == 0) problems.Add("no regions listed");
        if (EmployeesMin > EmployeesMax) problems.Add("EmployeesMin is above EmployeesMax");
        if (Exclusions.Count == 0) problems.Add("no exclusions listed, group-owned companies are the usual first one");
        return problems;
    }

    private static string Join(IReadOnlyList<string> v, string sep, string empty) => v.Count == 0 ? empty : string.Join(sep, v);

    /// <summary>Plain text brief, ready to paste into a pilot request.</summary>
    public string ToText()
    {
        var size = EmployeesMin is null && EmployeesMax is null
            ? "not specified"
            : $"{(EmployeesMin?.ToString() ?? "?")} to {(EmployeesMax?.ToString() ?? "?")} employees";
        var lines = new List<string>
        {
            $"Thesis: {Name}",
            $"Vertical: {Vertical}",
            $"Subverticals: {Join(Subverticals, ", ", "all")}",
            $"Regions: {Join(Regions, ", ", "not specified")}",
            $"Size: {size}",
            $"Must have: {Join(MustHave, "; ", "none")}",
            $"Exclude: {Join(Exclusions, "; ", "none")}",
        };
        if (Notes.Length > 0) lines.Add($"Notes: {Notes}");
        lines.Add("");
        lines.Add($"Request a pilot: {Signals.PilotUrl}");
        return string.Join("\n", lines);
    }
}
