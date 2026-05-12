using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.GUI.AnimationMenu;

internal class CrpgAnimationMenuVm : CrpgMenuVm
{
    public event Action<CrpgAnimEntry>? AnimRequested;

    public CrpgAnimationMenuVm()
    {
        BuildRootPage();
    }

    protected override void BuildRootPage()
    {
        // MenuTitle = CrpgAnimationData.MenuTitle;
        var rows = new MBBindingList<CrpgMenuItemVm>();
        foreach (var category in CrpgAnimationData.Categories)
        {
            var node = ToNode(category);
            bool enabled = node.IsEnabled;
            Action? navAction = enabled ? () => BuildPage(node, BuildRootPage, node.Label) : null;
            rows.Add(new CrpgMenuItemVm(node.Label + " >>", navAction, enabled));
        }

        // SetPage(rows);
        InformationManager.DisplayMessage(new InformationMessage($"Root page built with {CrpgAnimationData.MenuTitle} title."));
        SetRootPage(rows, CrpgAnimationData.MenuTitle);
    }

    private MenuNode CreateAnimationNode(CrpgAnimEntry e, Dictionary<string, int> nameCounts)
    {
        string finalName = DeduplicateName(e.DisplayName, nameCounts);

        return new MenuNode(
            finalName,
            new List<MenuNode>(),
            () => PlayAnimation(e),
            () =>
            {
                bool mounted = Mission.Current?.MainAgent?.MountAgent != null;

                return (!e.MountRequired || mounted)
                    && (e.Channel != 0 || !mounted)
                    && (!e.OnlyOnFoot || !mounted);
            });
    }

    private MenuNode ToNode(CrpgAnimCategory category)
    {
        Dictionary<string, int> nameCounts = [];
        List<MenuNode> children = [];

        for (int i = 0; i < category.SubCategories.Count; i++)
        {
            children.Add(ToNode(category.SubCategories[i]));
        }

        for (int i = 0; i < category.Animations.Count; i++)
        {
            children.Add(CreateAnimationNode(category.Animations[i], nameCounts));
        }

        MenuNode[] childArray = new MenuNode[children.Count];
        for (int i = 0; i < children.Count; i++)
        {
            childArray[i] = children[i];
        }

        return new MenuNode(
            category.DisplayName,
            childArray,
            isEnabled: () => HasAnyEnabled(childArray));
    }

    private static string DeduplicateName(string name, Dictionary<string, int> counts)
    {
        if (!counts.TryGetValue(name, out int count))
        {
            count = 0;
        }

        count++;
        counts[name] = count;

        return count == 1
            ? name
            : $"{name} {count}";
    }

    private void PlayAnimation(CrpgAnimEntry anim)
    {
        AnimRequested?.Invoke(anim);
        RaiseCloseRequested();
    }
}
