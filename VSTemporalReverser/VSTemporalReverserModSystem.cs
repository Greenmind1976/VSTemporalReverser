using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VSTemporalReverser;

public class VSTemporalReverserModSystem : ModSystem
{
    private const string Domain = "vstemporalreverser";
    private const int TemporalGearMaxStackSize = 64;
    private static readonly string[] RawMaterialItemPrefixes =
    [
        "ingot-",
        "metalbit-",
        "nugget-",
        "metalplate-",
        "metalchain-",
        "metalscale-",
        "metallamellae-",
        "rod-",
        "plank-",
        "clay-",
        "seed-",
        "cloth-",
        "leather-",
        "hide-"
    ];

    private static readonly string[] RawMaterialItemExactCodes =
    [
        "stick",
        "firewood",
        "firewood-aged",
        "flaxfibers",
        "flaxtwine",
        "drygrass",
        "papyrustops",
        "papyrusroot",
        "resin",
        "temporal-dust"
    ];
    public static VSTemporalReverserConfig Config { get; private set; } = new();
    private object? configLibModSystem;
    private bool configLibSubscribed;
    private Delegate? configLibSettingChangedHandler;
    private Delegate? configLibConfigsLoadedHandler;
    private ICoreServerAPI? sapi;
    private ICoreAPI? coreApi;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        coreApi = api;
        Config = api.LoadModConfig<VSTemporalReverserConfig>(VSTemporalReverserConfig.FileName) ?? new VSTemporalReverserConfig();
        Config.EnsureDefaults();
        ApplyConfig(api, Config);
        api.RegisterItemClass("ItemTemporalReverser", typeof(ItemTemporalReverser));
        api.RegisterItemClass("ItemRestoredToy", typeof(ItemRestoredToy));
        api.RegisterBlockClass("BlockTemporalReconstructionDevice", typeof(BlockTemporalReconstructionDevice));
        api.RegisterBlockClass("BlockTemporalDeconstructorDevice", typeof(BlockTemporalDeconstructorDevice));
        api.RegisterBlockClass("BlockRestoredBed", typeof(BlockRestoredBed));
        api.RegisterBlockClass("BlockRestoredCanopyBed", typeof(BlockRestoredCanopyBed));
        api.RegisterBlockClass("BlockRestoredBookSurface", typeof(BlockRestoredBookSurface));
        api.RegisterBlockEntityClass("TemporalReconstructionDevice", typeof(BlockEntityTemporalReconstructionDevice));
        api.RegisterBlockEntityClass("TemporalDeconstructorDevice", typeof(BlockEntityTemporalDeconstructorDevice));
        api.RegisterBlockEntityClass("RestoredBookSurface", typeof(BlockEntityRestoredBookSurface));
        TrySubscribeToConfigLib(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        TrySubscribeToConfigLib(api);
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);
        RefreshConfigDrivenStackSizes(api);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        sapi = api;
#pragma warning disable CS0618
        api.RegisterCommand(
            "trspawntool",
            "Debug: spawn a tool or other durable item with exact remaining durability.",
            "/trspawntool <itemcode> <remainingdurability>",
            OnDebugSpawnToolCommand,
            Privilege.controlserver);
#pragma warning restore CS0618
    }

    public static string[] GetEnabledWoodTypes(IEnumerable<string> fallbackPool)
    {
        string[] fallback = fallbackPool
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        string[] enabled = Config.GetEnabledWoodTypes();

        if (enabled.Length == 0)
        {
            return fallback;
        }

        string[] filtered = fallback
            .Where(code => enabled.Contains(code, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        return filtered.Length > 0 ? filtered : fallback;
    }

    private void ApplyConfig(ICoreAPI api, VSTemporalReverserConfig config)
    {
        Config = config;
        api.StoreModConfig(Config, VSTemporalReverserConfig.FileName);
    }

    private static void RefreshConfigDrivenStackSizes(ICoreAPI api)
    {
        SetTemporalGearStackSize(api);
        ApplyConfiguredRawMaterialStackSizes(api);
    }

    private static void SetTemporalGearStackSize(ICoreAPI api)
    {
        Item? temporalGear = api.World?.GetItem(new AssetLocation("game", "gear-temporal"));
        if (temporalGear != null)
        {
            temporalGear.MaxStackSize = TemporalGearMaxStackSize;
        }
    }

    private static void ApplyConfiguredRawMaterialStackSizes(ICoreAPI api)
    {
        if (!Config.EnableCustomRawMaterialStackSizes)
        {
            return;
        }

        int configuredSize = Config.RawMaterialStackSize;

        foreach (Item? item in api.World.Items)
        {
            if (item == null)
            {
                continue;
            }

            AssetLocation? code = item.Code;
            string path = code?.Path ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            if (RawMaterialItemExactCodes.Contains(path, StringComparer.OrdinalIgnoreCase)
                || RawMaterialItemPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                item.MaxStackSize = configuredSize;
            }
        }
    }

    private void OnDebugSpawnToolCommand(IServerPlayer player, int groupId, CmdArgs args)
    {
        if (!Config.EnableDebugMode)
        {
            player.SendMessage(groupId, "Temporal Reverser debug mode is disabled.", EnumChatType.CommandError, null);
            return;
        }

        string? codeArg = args.PopWord();
        int? remainingDurability = args.PopInt();
        if (string.IsNullOrWhiteSpace(codeArg) || remainingDurability == null || remainingDurability.Value < 0)
        {
            player.SendMessage(groupId, "Usage: /trspawntool <itemcode> <remainingdurability>", EnumChatType.CommandError, null);
            return;
        }

        AssetLocation code = codeArg.Contains(':')
            ? new AssetLocation(codeArg)
            : new AssetLocation("game", codeArg);

        Item? item = sapi?.World.GetItem(code);
        if (item == null)
        {
            player.SendMessage(groupId, $"Item not found: {code}", EnumChatType.CommandError, null);
            return;
        }

        ItemStack stack = new(item);
        int maxDurability = item.GetMaxDurability(stack);
        if (maxDurability <= 0)
        {
            player.SendMessage(groupId, $"{stack.GetName()} does not use durability.", EnumChatType.CommandError, null);
            return;
        }

        int clampedRemaining = Math.Clamp(remainingDurability.Value, 0, maxDurability);
        item.SetDurability(stack, clampedRemaining);

        bool given = player.InventoryManager.TryGiveItemstack(stack, true);
        if (!given)
        {
            sapi?.World.SpawnItemEntity(stack, player.Entity.Pos.XYZ.AddCopy(0, 0.5, 0));
        }

        player.SendMessage(
            groupId,
            $"Spawned {stack.GetName()} with {clampedRemaining}/{maxDurability} durability.",
            EnumChatType.CommandSuccess,
            null);
    }

    private void TrySubscribeToConfigLib(ICoreAPI api)
    {
        if (configLibSubscribed) return;
        if (!api.ModLoader.IsModEnabled("configlib")) return;

        configLibModSystem = api.ModLoader.GetModSystem("ConfigLib.ConfigLibModSystem");
        if (configLibModSystem == null) return;

        Type systemType = configLibModSystem.GetType();

        EventInfo? settingChangedEvent = systemType.GetEvent("SettingChanged");
        if (settingChangedEvent != null)
        {
            MethodInfo? mi = GetType().GetMethod(nameof(OnConfigLibSettingChanged), BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi != null && configLibSettingChangedHandler == null)
            {
                configLibSettingChangedHandler = Delegate.CreateDelegate(settingChangedEvent.EventHandlerType!, this, mi);
                settingChangedEvent.AddEventHandler(configLibModSystem, configLibSettingChangedHandler);
            }
        }

        EventInfo? configsLoadedEvent = systemType.GetEvent("ConfigsLoaded");
        if (configsLoadedEvent != null)
        {
            MethodInfo? mi = GetType().GetMethod(nameof(OnConfigLibConfigsLoaded), BindingFlags.NonPublic | BindingFlags.Instance);
            if (mi != null && configLibConfigsLoadedHandler == null)
            {
                configLibConfigsLoadedHandler = Delegate.CreateDelegate(configsLoadedEvent.EventHandlerType!, this, mi);
                configsLoadedEvent.AddEventHandler(configLibModSystem, configLibConfigsLoadedHandler);
            }
        }

        configLibSubscribed = true;
    }

#pragma warning disable IDE0060
    private void OnConfigLibSettingChanged(string domain, object _config, object setting)
    {
        if (!string.Equals(domain, Domain, StringComparison.OrdinalIgnoreCase)) return;

        try
        {
            MethodInfo? assign = setting.GetType().GetMethod("AssignSettingValue", [typeof(object)]);
            assign?.Invoke(setting, [Config]);
        }
        catch
        {
        }

        Config.EnsureDefaults();
        if (coreApi != null)
        {
            ApplyConfig(coreApi, Config);
            RefreshConfigDrivenStackSizes(coreApi);
        }
    }
#pragma warning restore IDE0060

    private void OnConfigLibConfigsLoaded()
    {
        try
        {
            if (configLibModSystem == null) return;

            Type systemType = configLibModSystem.GetType();
            MethodInfo? getConfig = systemType.GetMethod("GetConfig", [typeof(string)]);
            object? cfg = getConfig?.Invoke(configLibModSystem, [Domain]);
            if (cfg == null) return;

            MethodInfo? assignAll = cfg.GetType().GetMethod("AssignSettingsValues", [typeof(object)]);
            assignAll?.Invoke(cfg, [Config]);
        }
        catch
        {
        }

        Config.EnsureDefaults();
        if (coreApi != null)
        {
            ApplyConfig(coreApi, Config);
            RefreshConfigDrivenStackSizes(coreApi);
        }
    }
}
