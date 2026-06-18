using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI.Notifications;

/// <summary>
/// MissionView that loads the notification container widget into the scene.
///
/// Predefined (named) logs — accessible from any mission behavior:
///   // Register once (e.g. in game mode or a static initializer):
///   CrpgHudNotificationUiHandler.RegisterLog("killfeed", new CrpgHudNotificationLogOptions { ... });
///
///   // Add entries from anywhere:
///   CrpgHudNotificationUiHandler.AddToLog("killfeed", new CrpgHudNotificationOptions { Text = "...", ... });
///
/// Dynamic (anonymous) logs — created and owned by the caller:
///   var log = CrpgHudNotificationWidget.Current?.CreateLog(new CrpgHudNotificationLogOptions { ... });
///   log?.AddEntry(new CrpgHudNotificationOptions { ... });
///
/// Floating notifications:
///   CrpgHudNotificationUiHandler.ShowNotification(new CrpgHudNotificationOptions { ... });.
/// </summary>
internal class CrpgHudNotificationUiHandler : MissionView
{
    private class NoOpViewModel : ViewModel
    {
    }

    // Log configs persist across missions; active log widgets are per-mission.
    private static readonly Dictionary<string, CrpgHudNotificationLogOptions> LogConfigs = new()
    {
        ["statuslog"] = new CrpgHudNotificationLogOptions
        {
            HorizontalAlignment = TaleWorlds.GauntletUI.HorizontalAlignment.Center,
            VerticalAlignment = TaleWorlds.GauntletUI.VerticalAlignment.Bottom,
            PositionX = 0f,
            PositionY = -150f,
            MaxEntries = 5,
            GrowUpward = true,
        },
    };
    private readonly Dictionary<string, CrpgHudNotificationLog> _activeLogs = new();

    private GauntletLayer? _gauntletLayer;
    private GauntletMovieIdentifier? _movie;

    public static CrpgHudNotificationUiHandler? Instance { get; private set; }

    /// <summary>
    /// Registers a named log configuration. Call this once (e.g. from a game mode or static init)
    /// before the mission starts. The actual widget is created lazily on the first AddToLog call.
    /// </summary>
    public static void RegisterLog(string name, CrpgHudNotificationLogOptions options)
    {
        LogConfigs[name] = options;
    }

    /// <summary>Clears all entries from a named log without destroying it.</summary>
    public static void ClearLog(string name) => Instance?.ClearLogInternal(name);

    /// <summary>
    /// Adds an entry to a named log. Creates the log widget on first call if the name was registered.
    /// </summary>
    public static void AddToLog(string name, CrpgHudNotificationOptions entry) =>
        Instance?.AddToLogInternal(name, entry);

    public static void ShowNotification(CrpgHudNotificationOptions options) =>
        CrpgHudNotificationWidget.Current?.AddNotification(options);

    public override void EarlyStart()
    {
        base.EarlyStart();
        Instance = this;
        _gauntletLayer = new GauntletLayer("CrpgHudNotification", ViewOrderPriority + 2);
        _movie = _gauntletLayer.LoadMovie("CrpgHudNotificationContainer", new NoOpViewModel());
        MissionScreen.AddLayer(_gauntletLayer);
    }

    public override void OnMissionScreenFinalize()
    {
        foreach (var log in _activeLogs.Values)
        {
            log.Destroy();
        }

        _activeLogs.Clear();
        _gauntletLayer!.ReleaseMovie(_movie);
        MissionScreen.RemoveLayer(_gauntletLayer);
        Instance = null;
        base.OnMissionScreenFinalize();
    }

    private void AddToLogInternal(string name, CrpgHudNotificationOptions entry)
    {
        if (!_activeLogs.TryGetValue(name, out var log))
        {
            if (!LogConfigs.TryGetValue(name, out var logOptions))
            {
                return;
            }

            log = CrpgHudNotificationWidget.Current?.CreateLog(logOptions);
            if (log == null)
            {
                return;
            }

            _activeLogs[name] = log;
        }

        log.AddEntry(entry);
    }

    private void ClearLogInternal(string name)
    {
        if (_activeLogs.TryGetValue(name, out var log))
        {
            log.ClearEntries();
        }
    }
}
