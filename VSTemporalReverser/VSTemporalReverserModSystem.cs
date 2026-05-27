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
    private const int DefaultDebugBatchCount = 24;
    private const int MaxDebugBatchCount = 48;
    private static readonly Random DebugRandom = new();
    private static readonly string[] DebugDamagedToolCodes =
    [
        "game:axe-iron",
        "game:pickaxe-iron",
        "game:hammer-iron",
        "game:saw-iron",
        "game:shovel-iron",
        "game:hoe-iron",
        "game:knife-iron",
        "game:cleaver-iron",
        "game:spear-generic-iron",
        "game:scythe-iron",
        "game:prospectingpick-iron",
        "game:bow-simple",
        "game:axe-steel",
        "game:pickaxe-steel",
        "game:hammer-steel",
        "game:saw-steel",
        "game:shovel-steel",
        "game:hoe-steel",
        "game:knife-steel",
        "game:cleaver-steel",
        "game:spear-generic-steel",
        "game:scythe-steel",
        "game:prospectingpick-steel"
    ];
    private static readonly string[] DebugSupportedDurableDeconstructionCodes =
    [
        "game:axe-iron",
        "game:pickaxe-iron",
        "game:hammer-iron",
        "game:saw-iron",
        "game:shovel-iron",
        "game:hoe-iron",
        "game:knife-iron",
        "game:cleaver-iron",
        "game:spear-generic-iron",
        "game:scythe-iron",
        "game:prospectingpick-iron",
        "game:chisel-iron",
        "game:crowbar-iron",
        "game:shears-iron",
        "game:wrench-iron",
        "game:tongsmetal-standard-iron",
        "game:axe-steel",
        "game:pickaxe-steel",
        "game:hammer-steel",
        "game:saw-steel",
        "game:shovel-steel",
        "game:hoe-steel",
        "game:knife-steel",
        "game:cleaver-steel",
        "game:spear-generic-steel",
        "game:scythe-steel",
        "game:prospectingpick-steel",
        "game:chisel-steel",
        "game:crowbar-steel",
        "game:shears-steel",
        "game:wrench-steel",
        "game:tongsmetal-standard-steel",
        "game:solderingiron"
    ];
    private static readonly string[] DebugReverserClutterTypes =
    [
        "anvil-broken1",
        "anvil-broken2",
        "anvil-broken3",
        "brazier1",
        "brazier2",
        "brazier3",
        "brazier4",
        "chandelier-ruined1",
        "chandelier-ruined2",
        "chandelier-ruined3",
        "lantern/ground1",
        "lantern/ground2",
        "lantern/ground3",
        "lantern/ground4",
        "lantern/ground5",
        "lantern/ground6",
        "candlestub-single",
        "candlestubs-bunch1",
        "candlestubs-bunch2",
        "lecturn-ruined",
        "bookshelves/lecturn-aged-empty",
        "bookshelves/lecturn-aged-book-closed",
        "bookshelves/lecturn-ruined",
        "bookshelves/bookstand-book-closed",
        "chair-metal1-ruined1",
        "chair-metal1-ruined2",
        "chair-ruined1",
        "chair-ruined2",
        "table/metal1-ruined1",
        "table/metal1-ruined2",
        "table-ruined1",
        "table-ruined2",
        "bed/metal2-ruined1",
        "bed/metal2-ruined2",
        "bed/bed-metal-ruined1",
        "bed/bed-metal-ruined2",
        "bed/bed-ruined1",
        "bed/bed-fancy-ruined1",
        "crate/crate-medium-books",
        "crate/crate-medium-pottery",
        "crate/crate-medium-pottery-alt",
        "crate/crate-small-pottery",
        "crate/crate-large-junk",
        "crate/crate-medium-junk",
        "crate/crate-large-pottery",
        "crate/large-pottery1",
        "crate/large-clothing1",
        "crate/medium-toybox1",
        "pile-tools1",
        "pile-tools2",
        "pile-tools3",
        "pile-woodworkingtools",
        "tool-axe",
        "tool-hammer",
        "tool-knife",
        "tool-hoe",
        "tool-shovel",
        "tool-spear",
        "toy4",
        "toy8",
        "toy12",
        "shelf-toys1",
        "shelf-toys2",
        "music-box1",
        "music-box2"
    ];
    private static readonly string[] RawMaterialItemPrefixes =
    [
        "ingot-",
        "metalbit-",
        "nugget-",
        "metalplate-",
        "metalchain-",
        "metalnailsandstrips-",
        "metalscale-",
        "metallamellae-",
        "metalsheet-",
        "rod-",
        "plank-",
        "clay-",
        "seed-",
        "seeds-",
        "cloth-",
        "leather-",
        "hide-",
        "glass-"
    ];

    private static readonly HashSet<string> RawMaterialItemExactCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "stick",
        "firewood",
        "firewood-aged",
        "flaxfibers",
        "flaxtwine",
        "drygrass",
        "papyrustops",
        "papyrusroot",
        "resin",
        "paper-parchment",
        "candle",
        "beeswax",
        "gear-temporal",
        "temporal-dust"
    };
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
        api.RegisterBlockClass("BlockTemporalDisposalUnit", typeof(BlockTemporalDisposalUnit));
        api.RegisterBlockClass("BlockRestoredBed", typeof(BlockRestoredBed));
        api.RegisterBlockClass("BlockRestoredCanopyBed", typeof(BlockRestoredCanopyBed));
        api.RegisterBlockClass("BlockRestoredMetalTable", typeof(BlockRestoredMetalTable));
        api.RegisterBlockClass("BlockRestoredMetalTableExtension", typeof(BlockRestoredMetalTableExtension));
        api.RegisterBlockClass("BlockRestoredMetalTableLow", typeof(BlockRestoredMetalTableLow));
        api.RegisterBlockClass("BlockRestoredMetalTableLowExtension", typeof(BlockRestoredMetalTableLowExtension));
        api.RegisterBlockClass("BlockRestoredBookSurface", typeof(BlockRestoredBookSurface));
        api.RegisterBlockClass("BlockRestoredTorchHolder", typeof(BlockRestoredTorchHolder));
        api.RegisterBlockEntityClass("TemporalReconstructionDevice", typeof(BlockEntityTemporalReconstructionDevice));
        api.RegisterBlockEntityClass("TemporalDeconstructorDevice", typeof(BlockEntityTemporalDeconstructorDevice));
        api.RegisterBlockEntityClass("TemporalDisposalUnit", typeof(BlockEntityTemporalDisposalUnit));
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
        api.RegisterCommand(
            "trbatch",
            "Debug: spawn a randomized reverser clutter test batch.",
            "/trbatch [count]",
            OnDebugSpawnBatchCommand,
            Privilege.controlserver);
        api.RegisterCommand(
            "trdtools",
            "Debug: spawn a batch of damaged metal tools.",
            "/trdtools",
            OnDebugSpawnDamagedToolsCommand,
            Privilege.controlserver);
        api.RegisterCommand(
            "trbatchdur",
            "Debug: spawn a batch of tools and weapons with exact remaining durability.",
            "/trbatchdur [remainingdurability]",
            OnDebugSpawnBatchDurabilityCommand,
            Privilege.controlserver);
        api.RegisterCommand(
            "trbatchsupported",
            "Debug: spawn a randomized batch of supported durable deconstruction items.",
            "/trbatchsupported [count] [remainingdurability]",
            OnDebugSpawnSupportedBatchCommand,
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
        ApplyConfiguredRawMaterialStackSizes(api);
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

            if (RawMaterialItemExactCodes.Contains(path) || HasRawMaterialPrefix(path))
            {
                item.MaxStackSize = configuredSize;
            }
        }
    }

    private static bool HasRawMaterialPrefix(string path)
    {
        for (int i = 0; i < RawMaterialItemPrefixes.Length; i++)
        {
            if (path.StartsWith(RawMaterialItemPrefixes[i], StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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

    private void OnDebugSpawnBatchCommand(IServerPlayer player, int groupId, CmdArgs args)
    {
        if (!Config.EnableDebugMode)
        {
            player.SendMessage(groupId, "Temporal Reverser debug mode is disabled.", EnumChatType.CommandError, null);
            return;
        }

        Block? clutterBlock = sapi?.World.GetBlock(new AssetLocation("game", "clutter"));
        if (clutterBlock == null)
        {
            player.SendMessage(groupId, "Could not find the vanilla clutter block.", EnumChatType.CommandError, null);
            return;
        }

        int requestedCount = args.PopInt() ?? DefaultDebugBatchCount;
        int batchCount = Math.Clamp(requestedCount, 1, Math.Min(MaxDebugBatchCount, DebugReverserClutterTypes.Length));
        string[] selection = DebugReverserClutterTypes
            .OrderBy(_ => Guid.NewGuid())
            .Take(batchCount)
            .ToArray();

        int spawnedCount = 0;
        foreach (string clutterType in selection)
        {
            ItemStack stack = new(clutterBlock, 1);
            stack.Attributes.SetString("type", clutterType);
            stack.Attributes.SetBool("collected", true);
            stack.ResolveBlockOrItem(sapi!.World);

            bool given = player.InventoryManager.TryGiveItemstack(stack, true);
            if (!given)
            {
                sapi.World.SpawnItemEntity(stack, player.Entity.Pos.XYZ.AddCopy(0, 0.5, 0));
            }

            spawnedCount++;
        }

        player.SendMessage(
            groupId,
            $"Spawned {spawnedCount} randomized clutter test items for the reverser.",
            EnumChatType.CommandSuccess,
            null);
    }

    private void OnDebugSpawnDamagedToolsCommand(IServerPlayer player, int groupId, CmdArgs args)
    {
        if (!Config.EnableDebugMode)
        {
            player.SendMessage(groupId, "Temporal Reverser debug mode is disabled.", EnumChatType.CommandError, null);
            return;
        }

        int spawnedCount = 0;
        foreach (string codeText in DebugDamagedToolCodes)
        {
            Item? item = sapi?.World.GetItem(new AssetLocation(codeText));
            if (item == null)
            {
                continue;
            }

            ItemStack stack = new(item);
            int maxDurability = item.GetMaxDurability(stack);
            if (maxDurability <= 0)
            {
                continue;
            }

            int remainingDurability = Math.Max(1, maxDurability / 10);
            item.SetDurability(stack, remainingDurability);

            bool given = player.InventoryManager.TryGiveItemstack(stack, true);
            if (!given)
            {
                sapi?.World.SpawnItemEntity(stack, player.Entity.Pos.XYZ.AddCopy(0, 0.5, 0));
            }

            spawnedCount++;
        }

        player.SendMessage(
            groupId,
            $"Spawned {spawnedCount} damaged tools.",
            EnumChatType.CommandSuccess,
            null);
    }

    private void OnDebugSpawnBatchDurabilityCommand(IServerPlayer player, int groupId, CmdArgs args)
    {
        if (!Config.EnableDebugMode)
        {
            player.SendMessage(groupId, "Temporal Reverser debug mode is disabled.", EnumChatType.CommandError, null);
            return;
        }

        int requestedRemainingDurability = args.PopInt() ?? 1;
        int spawnedCount = 0;

        foreach (string codeText in DebugDamagedToolCodes)
        {
            Item? item = sapi?.World.GetItem(new AssetLocation(codeText));
            if (item == null)
            {
                continue;
            }

            ItemStack stack = new(item);
            int maxDurability = item.GetMaxDurability(stack);
            if (maxDurability <= 0)
            {
                continue;
            }

            int remainingDurability = Math.Clamp(requestedRemainingDurability, 1, maxDurability);
            item.SetDurability(stack, remainingDurability);

            bool given = player.InventoryManager.TryGiveItemstack(stack, true);
            if (!given)
            {
                sapi?.World.SpawnItemEntity(stack, player.Entity.Pos.XYZ.AddCopy(0, 0.5, 0));
            }

            spawnedCount++;
        }

        player.SendMessage(
            groupId,
            $"Spawned {spawnedCount} tools and weapons with {requestedRemainingDurability} durability.",
            EnumChatType.CommandSuccess,
            null);
    }

    private void OnDebugSpawnSupportedBatchCommand(IServerPlayer player, int groupId, CmdArgs args)
    {
        if (!Config.EnableDebugMode)
        {
            player.SendMessage(groupId, "Temporal Reverser debug mode is disabled.", EnumChatType.CommandError, null);
            return;
        }

        int requestedCount = args.PopInt() ?? 12;
        int? requestedRemainingDurability = args.PopInt();
        List<string> availableCodes = GetAvailableDebugItemCodes(DebugSupportedDurableDeconstructionCodes);
        int batchCount = Math.Clamp(requestedCount, 1, Math.Min(MaxDebugBatchCount, availableCodes.Count));
        string[] selection = GetRandomSelection([.. availableCodes], batchCount);

        int spawnedCount = 0;
        foreach (string codeText in selection)
        {
            Item? item = sapi?.World.GetItem(new AssetLocation(codeText));
            if (item == null)
            {
                continue;
            }

            ItemStack stack = new(item);
            int maxDurability = item.GetMaxDurability(stack);
            if (maxDurability > 0 && requestedRemainingDurability.HasValue)
            {
                int remainingDurability = Math.Clamp(requestedRemainingDurability.Value, 1, maxDurability);
                item.SetDurability(stack, remainingDurability);
            }

            bool given = player.InventoryManager.TryGiveItemstack(stack, true);
            if (!given)
            {
                sapi?.World.SpawnItemEntity(stack, player.Entity.Pos.XYZ.AddCopy(0, 0.5, 0));
            }

            spawnedCount++;
        }

        string durabilitySuffix = requestedRemainingDurability.HasValue
            ? $" at {requestedRemainingDurability.Value} durability"
            : string.Empty;

        player.SendMessage(
            groupId,
            $"Spawned {spawnedCount} randomized supported deconstruction items{durabilitySuffix}.",
            EnumChatType.CommandSuccess,
            null);
    }

    private static string[] GetRandomSelection(string[] source, int count)
    {
        string[] copy = (string[])source.Clone();
        int limit = Math.Min(count, copy.Length);

        for (int i = 0; i < limit; i++)
        {
            int swapIndex = DebugRandom.Next(i, copy.Length);
            (copy[i], copy[swapIndex]) = (copy[swapIndex], copy[i]);
        }

        return copy.Take(limit).ToArray();
    }

    private List<string> GetAvailableDebugItemCodes(IEnumerable<string> codes)
    {
        List<string> available = [];

        foreach (string codeText in codes)
        {
            if (sapi?.World.GetItem(new AssetLocation(codeText)) != null)
            {
                available.Add(codeText);
            }
        }

        return available;
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

        SyncConfigFromConfigLib();

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
        SyncConfigFromConfigLib();

        Config.EnsureDefaults();
        if (coreApi != null)
        {
            ApplyConfig(coreApi, Config);
            RefreshConfigDrivenStackSizes(coreApi);
        }
    }

    private void SyncConfigFromConfigLib()
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
    }
}
