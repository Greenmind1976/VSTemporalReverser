using System;
using System.Collections.Generic;
using System.Linq;

namespace VSTemporalReverser;

public sealed class VSTemporalReverserConfig
{
    public const string FileName = "VSTemporalReverserConfig.json";
    private static readonly int[] AllowedRawMaterialStackSizes = [64, 128, 256];

    public int SchemaVersion { get; set; } = 10;

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

    public bool EnableRustWardDamage { get; set; } = true;

    public float RustWardDamage { get; set; } = 0.25f;

    public float RustWardRadius { get; set; } = 4f;

    public float RustWardPushback { get; set; } = 0.5f;

    public float RestoreCooldownSeconds { get; set; } = 1.5f;

    public bool EnableDebugMode { get; set; } = false;

    public bool EnableCustomRawMaterialStackSizes { get; set; } = false;

    public int RawMaterialStackSize { get; set; } = 64;

    public bool DeconstructMetalOutputsToIngots { get; set; } = true;

    public bool AllowClosedCanopyBedSleepWhenNotTired { get; set; } = true;

    public void EnsureDefaults()
    {
        if (SchemaVersion < 10)
        {
            SchemaVersion = 10;
        }

        RustWardDamage = Math.Clamp(RustWardDamage, 0f, 10f);

        RustWardRadius = Math.Clamp(RustWardRadius, 2f, 6f);
        RustWardPushback = Math.Clamp(RustWardPushback, 0.5f, 3f);
        RestoreCooldownSeconds = Math.Clamp(RestoreCooldownSeconds, 0f, 3f);
        RawMaterialStackSize = NormalizeRawMaterialStackSize(RawMaterialStackSize);
    }

    private static int NormalizeRawMaterialStackSize(int requestedSize)
    {
        return AllowedRawMaterialStackSizes
            .OrderBy(size => Math.Abs(size - requestedSize))
            .ThenBy(size => size)
            .First();
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
