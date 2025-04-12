using BetterPartyFinder.Windows.Config;
using BetterPartyFinder.Windows.Main;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BetterPartyFinder;

// ReSharper disable once ClassNeverInstantiated.Global
public class Plugin : IDalamudPlugin
{
    internal static string Name => "Better Party Finder";

    [PluginService] public static IDalamudPluginInterface Interface { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IDataManager Data { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IPartyFinderGui PartyFinderGui { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] public static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;

    public readonly WindowSystem WindowSystem = new(Name);
    public ConfigWindow ConfigWindow { get; init; }
    public MainWindow MainWindow { get; init; }

    internal Configuration Config { get; }
    private Filter Filter { get; }
    private Commands Commands { get; }
    internal HookManager HookManager { get; }

    public Plugin() {
        Config = Configuration.Load() ?? new Configuration();

        HookManager = new HookManager();
        Filter = new Filter(this);
        Commands = new Commands(this);

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Interface.UiBuilder.Draw += DrawUI;
        Interface.UiBuilder.OpenMainUi += OpenMainUi;
        Interface.UiBuilder.OpenConfigUi += OpenConfigUi;

        AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "LookingForGroup", SetMainUiOpen);
        AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "LookingForGroup", SetMainUiClose);
    }

    public void Dispose() {
        AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, "LookingForGroup", SetMainUiOpen);
        AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, "LookingForGroup", SetMainUiClose);

        Interface.UiBuilder.Draw -= DrawUI;
        Interface.UiBuilder.OpenMainUi -= OpenMainUi;
        Interface.UiBuilder.OpenConfigUi -= OpenConfigUi;

        WindowSystem.RemoveAllWindows();

        Commands.Dispose();
        Filter.Dispose();
    }

    #region Draws
    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void OpenMainUi()
    {
        MainWindow.Toggle();
    }

    public void OpenConfigUi()
    {
        ConfigWindow.Toggle();
    }

    // Used with AddonLifecycle
    public void SetMainUiOpen(AddonEvent _, AddonArgs __)
    {
        if (Config.ShowWhenPfOpen)
            MainWindow.IsOpen = true;
    }

    public void SetMainUiClose(AddonEvent _, AddonArgs __)
    {
        if (Config.ShowWhenPfOpen)
            MainWindow.IsOpen = false;
    }
    #endregion
}