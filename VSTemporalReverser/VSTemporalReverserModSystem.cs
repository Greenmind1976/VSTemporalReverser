using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vintagestory.API.Common;

namespace VSTemporalReverser;

public class VSTemporalReverserModSystem : ModSystem
{
    private const string Domain = "vstemporalreverser";
    public static VSTemporalReverserConfig Config { get; private set; } = new();
    private object? configLibModSystem;
    private bool configLibSubscribed;
    private Delegate? configLibSettingChangedHandler;
    private Delegate? configLibConfigsLoadedHandler;

    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        Config = api.LoadModConfig<VSTemporalReverserConfig>(VSTemporalReverserConfig.FileName) ?? new VSTemporalReverserConfig();
        Config.EnsureDefaults();
        ApplyConfig(api, Config);
        api.RegisterItemClass("ItemTemporalReverser", typeof(ItemTemporalReverser));
        api.RegisterItemClass("ItemRestoredToy", typeof(ItemRestoredToy));
        api.RegisterBlockClass("BlockRestoredBed", typeof(BlockRestoredBed));
        api.RegisterBlockClass("BlockRestoredCanopyBed", typeof(BlockRestoredCanopyBed));
        api.RegisterBlockClass("BlockRestoredBookSurface", typeof(BlockRestoredBookSurface));
        api.RegisterBlockEntityClass("RestoredBookSurface", typeof(BlockEntityRestoredBookSurface));
        TrySubscribeToConfigLib(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        TrySubscribeToConfigLib(api);
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
    }
}
