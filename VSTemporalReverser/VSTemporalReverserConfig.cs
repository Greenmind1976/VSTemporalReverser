using System.Collections.Generic;
using System.Linq;

namespace VSTemporalReverser;

public sealed class VSTemporalReverserConfig
{
    public const string FileName = "VSTemporalReverserConfig.json";

    public int SchemaVersion { get; set; } = 2;

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

    public bool EnableRaccoons { get; set; } = true;

    public bool EnableMice { get; set; } = true;

    public bool EnableMoths { get; set; } = true;

    public bool EnableDebugMode { get; set; } = false;

    public void EnsureDefaults()
    {
        if (SchemaVersion < 2)
        {
            SchemaVersion = 2;
        }
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
