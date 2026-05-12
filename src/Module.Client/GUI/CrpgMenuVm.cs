using TaleWorlds.Library;

namespace Crpg.Module.GUI;

internal abstract class CrpgMenuVm : ViewModel
{
    protected sealed class MenuNode(string label, IReadOnlyList<MenuNode> children, Action? execute = null, Func<bool>? isEnabled = null)
    {
        public string Label { get; } = label;
        public IReadOnlyList<MenuNode> Children { get; } = children;
        public Action? Execute { get; } = execute;
        public bool IsEnabled => isEnabled?.Invoke() ?? true;
        public bool IsLeaf => Children.Count == 0;
    }

    private Action? _currentPageRebuild;

    public event Action? CloseRequested;

    protected void RaiseCloseRequested() => CloseRequested?.Invoke();

    [DataSourceProperty]
    public bool IsEnabled { get; set => SetField(ref field, value, nameof(IsEnabled)); }

    [DataSourceProperty]
    public MBBindingList<CrpgMenuItemVm> Items { get; set => SetField(ref field, value, nameof(Items)); } = [];

    [DataSourceProperty]
    public CrpgMenuItemVm? Back { get; set => SetField(ref field, value, nameof(Back)); }

    [DataSourceProperty]
    public string MenuTitle { get; set => SetField(ref field, value, nameof(MenuTitle)); } = string.Empty;

    public void ToggleMenu()
    {
        IsEnabled = !IsEnabled;
        if (IsEnabled)
        {
            (_currentPageRebuild ?? BuildRootPage).Invoke();
        }
    }

    public void CloseMenu()
    {
        IsEnabled = false;
    }

    public void ExecuteClose()
    {
        RaiseCloseRequested();
    }

    public void ExecuteBack()
    {
        if (Back != null)
        {
            Back.ExecuteItem();
        }
        else
        {
            RaiseCloseRequested();
        }
    }

    public void RefreshCurrentPage()
    {
        _currentPageRebuild?.Invoke();
    }

    protected abstract void BuildRootPage();

    protected static bool HasAnyEnabled(IReadOnlyList<MenuNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.IsLeaf ? node.IsEnabled : HasAnyEnabled(node.Children))
            {
                return true;
            }
        }

        return false;
    }

    protected void BuildPage(MenuNode node, Action backAction, string breadcrumb = "")
    {
        MenuTitle = breadcrumb;
        var rows = new MBBindingList<CrpgMenuItemVm>();
        foreach (var child in node.Children)
        {
            var c = child;
            if (c.IsLeaf)
            {
                rows.Add(new CrpgMenuItemVm(c.Label, c.IsEnabled ? c.Execute : null, c.IsEnabled));
            }
            else
            {
                string childCrumb = string.IsNullOrEmpty(breadcrumb) ? c.Label : $"{breadcrumb} → {c.Label}";
                Action? navAction = c.IsEnabled ? () => BuildPage(c, () => BuildPage(node, backAction, breadcrumb), childCrumb) : null;
                rows.Add(new CrpgMenuItemVm(c.Label + " >>", navAction, c.IsEnabled));
            }
        }

        SetPage(rows, backAction, () => BuildPage(node, backAction, breadcrumb));
    }

    protected void SetPage(MBBindingList<CrpgMenuItemVm> rows, Action? backAction = null, Action? rebuild = null)
    {
        _currentPageRebuild = rebuild;
        Items = rows;
        Back = backAction != null ? new CrpgMenuItemVm("<<--", backAction) : null;
    }

    protected void SetRootPage(MBBindingList<CrpgMenuItemVm> rows, string title)
    {
        MenuTitle = title;
        _currentPageRebuild = BuildRootPage;
        Items = rows;
        Back = null;
    }
}
