using System;
using System.Collections.Generic;
using System.Linq;

namespace VSTemporalReverser;

public sealed class VSTemporalReverserConfig
{
    public const string FileName = "VSTemporalReverserConfig.json";
    private static readonly string[] AllWoodTypes =
    [
        "birch",
        "oak",
        "maple",
        "pine",
        "acacia",
        "kapok",
        "redwood",
        "baldcypress",
        "larch",
        "ebony",
        "walnut",
        "purpleheart",
        "aged",
        "veryaged"
    ];

    // Legacy/manual JSON list. Still supported, but checkbox booleans are now the preferred path.
    public List<string> EnabledWoodTypes { get; set; } =
    [
        .. AllWoodTypes
    ];

    // ConfigLib-friendly checkbox settings
    public bool Birch { get; set; } = true;
    public bool Oak { get; set; } = true;
    public bool Maple { get; set; } = true;
    public bool Pine { get; set; } = true;
    public bool Acacia { get; set; } = true;
    public bool Kapok { get; set; } = true;
    public bool Redwood { get; set; } = true;
    public bool BaldCypress { get; set; } = true;
    public bool Larch { get; set; } = true;
    public bool Ebony { get; set; } = true;
    public bool Walnut { get; set; } = true;
    public bool Purpleheart { get; set; } = true;
    public bool AgedWood { get; set; } = true;
    public bool VeryAgedWood { get; set; } = true;

    public bool EnableDebugLogging { get; set; } = false;

    public void Normalize()
    {
        List<string> normalizedList = EnabledWoodTypes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // One-time migration path for older JSON configs that only used the list field.
        // If the checkboxes are still all at their defaults and the list differs, import it once.
        bool allCheckboxesEnabled =
            Birch &&
            Oak &&
            Maple &&
            Pine &&
            Acacia &&
            Kapok &&
            Redwood &&
            BaldCypress &&
            Larch &&
            Ebony &&
            Walnut &&
            Purpleheart &&
            AgedWood &&
            VeryAgedWood;

        bool legacyListDiffersFromDefaults =
            normalizedList.Count != AllWoodTypes.Length ||
            normalizedList.Except(AllWoodTypes, StringComparer.OrdinalIgnoreCase).Any() ||
            AllWoodTypes.Except(normalizedList, StringComparer.OrdinalIgnoreCase).Any();

        if (allCheckboxesEnabled && normalizedList.Count > 0 && legacyListDiffersFromDefaults)
        {
            Birch = normalizedList.Contains("birch", StringComparer.OrdinalIgnoreCase);
            Oak = normalizedList.Contains("oak", StringComparer.OrdinalIgnoreCase);
            Maple = normalizedList.Contains("maple", StringComparer.OrdinalIgnoreCase);
            Pine = normalizedList.Contains("pine", StringComparer.OrdinalIgnoreCase);
            Acacia = normalizedList.Contains("acacia", StringComparer.OrdinalIgnoreCase);
            Kapok = normalizedList.Contains("kapok", StringComparer.OrdinalIgnoreCase);
            Redwood = normalizedList.Contains("redwood", StringComparer.OrdinalIgnoreCase);
            BaldCypress = normalizedList.Contains("baldcypress", StringComparer.OrdinalIgnoreCase);
            Larch = normalizedList.Contains("larch", StringComparer.OrdinalIgnoreCase);
            Ebony = normalizedList.Contains("ebony", StringComparer.OrdinalIgnoreCase);
            Walnut = normalizedList.Contains("walnut", StringComparer.OrdinalIgnoreCase);
            Purpleheart = normalizedList.Contains("purpleheart", StringComparer.OrdinalIgnoreCase);
            AgedWood = normalizedList.Contains("aged", StringComparer.OrdinalIgnoreCase);
            VeryAgedWood = normalizedList.Contains("veryaged", StringComparer.OrdinalIgnoreCase);
        }

        EnabledWoodTypes =
        [
            .. GetEnabledWoodTypes()
        ];
    }

    public string[] GetEnabledWoodTypes()
    {
        List<string> woods = [];

        if (Birch) woods.Add("birch");
        if (Oak) woods.Add("oak");
        if (Maple) woods.Add("maple");
        if (Pine) woods.Add("pine");
        if (Acacia) woods.Add("acacia");
        if (Kapok) woods.Add("kapok");
        if (Redwood) woods.Add("redwood");
        if (BaldCypress) woods.Add("baldcypress");
        if (Larch) woods.Add("larch");
        if (Ebony) woods.Add("ebony");
        if (Walnut) woods.Add("walnut");
        if (Purpleheart) woods.Add("purpleheart");
        if (AgedWood) woods.Add("aged");
        if (VeryAgedWood) woods.Add("veryaged");

        return [.. woods];
    }
}
