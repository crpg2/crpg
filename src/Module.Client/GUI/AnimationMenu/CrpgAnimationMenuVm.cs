using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.AnimationMenu;

/// <summary>
/// Main ViewModel for the emote menu.
/// Manages a two-level navigation stack: category list → animation list.
///
/// Properties bound in CrpgAnimationMenu.xml:
///   IsEnabled       — controls overall panel visibility
///   Items           — the current page of menu rows (categories or emotes)
///   Back            — the "← Back" navigation item (null on the root page)
///   Next            — reserved for future paging (currently unused)
///   OpenMenuKeyName — key hint text shown below the panel
/// </summary>
internal class CrpgAnimationMenuVm : ViewModel
{
    private bool _isEnabled;
    private MBBindingList<CrpgAnimationMenuItemVm> _items = new();
    private CrpgAnimationMenuItemVm? _back;
    private CrpgAnimationMenuItemVm? _next;
    private string _openMenuKeyName = string.Empty;

    public CrpgAnimationMenuVm()
    {
        BuildCategoryPage();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Bound properties
    // ──────────────────────────────────────────────────────────────────────────

    [DataSourceProperty]
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled == value) return;
            _isEnabled = value;
            OnPropertyChangedWithValue(value);
        }
    }

    [DataSourceProperty]
    public MBBindingList<CrpgAnimationMenuItemVm> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChangedWithValue(value);
        }
    }

    /// <summary>Null when on the root category page; set when inside a category.</summary>
    [DataSourceProperty]
    public CrpgAnimationMenuItemVm? Back
    {
        get => _back;
        set
        {
            _back = value;
            OnPropertyChangedWithValue(value);
        }
    }

    /// <summary>Reserved for multi-page support. Currently always null.</summary>
    [DataSourceProperty]
    public CrpgAnimationMenuItemVm? Next
    {
        get => _next;
        set
        {
            _next = value;
            OnPropertyChangedWithValue(value);
        }
    }

    /// <summary>
    /// Human-readable key name shown as a hint below the panel,
    /// e.g. "[B] Emote Menu". Set by the UiHandler after the key is resolved.
    /// </summary>
    [DataSourceProperty]
    public string OpenMenuKeyName
    {
        get => _openMenuKeyName;
        set
        {
            if (_openMenuKeyName == value) return;
            _openMenuKeyName = value;
            OnPropertyChangedWithValue(value);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public interface used by UiHandler
    // ──────────────────────────────────────────────────────────────────────────

    public void ToggleMenu()
    {
        IsEnabled = !IsEnabled;
        if (IsEnabled)
        {
            BuildCategoryPage(); // always open at the root
        }
    }

    public void CloseMenu()
    {
        IsEnabled = false;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Navigation
    // ──────────────────────────────────────────────────────────────────────────

    private void BuildCategoryPage()
    {
        var rows = new MBBindingList<CrpgAnimationMenuItemVm>();
        foreach (var category in CrpgAnimationData.Categories)
        {
            var cat = category; // capture for lambda
            rows.Add(new CrpgAnimationMenuItemVm(cat.DisplayName, () => BuildEmotePage(cat)));
        }

        Items = rows;
        Back = null;
        Next = null;
    }

    private void BuildEmotePage(CrpgEmoteCategory category)
    {
        var rows = new MBBindingList<CrpgAnimationMenuItemVm>();
        foreach (var emote in category.Emotes)
        {
            var entry = emote; // capture for lambda
            rows.Add(new CrpgAnimationMenuItemVm(entry.DisplayName, () => PlayEmote(entry)));
        }

        Items = rows;
        Back = new CrpgAnimationMenuItemVm("< Back", BuildCategoryPage);
        Next = null;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Emote playback
    // ──────────────────────────────────────────────────────────────────────────

    private void PlayEmote(CrpgEmoteEntry emote)
    {
        var agent = Mission.Current?.MainAgent;
        if (agent == null || !agent.IsActive())
        {
            CloseMenu();
            return;
        }

        // Play the animation. ignorePriority=true lets it override combat idles.
        var actionIndex = ActionIndexCache.Create(emote.ActionId);
        agent.SetActionChannel(0, actionIndex, ignorePriority: true, startProgress: emote.StartProgress);

        // Apply vertical / horizontal offsets (used for sitting poses so the
        // character drops to the right height rather than floating mid-air).
        if (emote.ZOffset != 0f || emote.YOffset != 0f)
        {
            var pos = agent.Position;
            pos.z += emote.ZOffset;
            pos.y += emote.YOffset;
            agent.TeleportToPosition(pos);
        }

        CloseMenu();
    }
}
