using System.Xml.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;

namespace Crpg.Module.GUI.AnimationMenu;

internal static class CrpgAnimationData
{
    public static string MenuTitle { get; private set; } = string.Empty;
    public static IReadOnlyList<CrpgAnimCategory> Categories { get; } = Load();
    private static IReadOnlyList<CrpgAnimCategory> Load()
    {
        string path = ModuleHelper.GetXmlPath("cRPG", "crpg_menus/crpg_animation_menu");
        var doc = XDocument.Load(path);
        if (doc.Root is null)
        {
            return [];
        }

        MenuTitle = new TextObject((string?)doc.Root.Attribute("display_name") ?? string.Empty).ToString();
        InformationManager.DisplayMessage(new InformationMessage($"MenuTitle: {MenuTitle}"));

        return [.. doc.Root.Elements("menu_category").Select(ParseCategory)];
    }

    private static CrpgAnimCategory ParseCategory(XElement e) => new()
    {
        DisplayName = new TextObject((string?)e.Attribute("display_name") ?? string.Empty).ToString(),
        SubCategories = [.. e.Elements("menu_subcategory").Select(ParseCategory)],
        Animations = [.. e.Elements("menu_item").Select(ParseItem)],
    };

    private static CrpgAnimEntry ParseItem(XElement e) => new()
    {
        ActionId = (string?)e.Attribute("action_type") ?? string.Empty,
        DisplayName = new TextObject((string?)e.Attribute("display_name") ?? string.Empty).ToString(),
        Channel = (int?)e.Attribute("action_channel") ?? 1,
        MovementCancels = (bool?)e.Attribute("movement_cancels") ?? false,
        MountRequired = (bool?)e.Attribute("mount_required") ?? false,
        OnlyOnFoot = (bool?)e.Attribute("only_on_foot") ?? false,
    };
}

internal sealed class CrpgAnimEntry
{
    public string ActionId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Channel { get; set; } = 1;
    public bool MovementCancels { get; set; }
    public bool MountRequired { get; set; }
    public bool OnlyOnFoot { get; set; }
}

internal sealed class CrpgAnimCategory
{
    public string DisplayName { get; set; } = string.Empty;
    public IReadOnlyList<CrpgAnimCategory> SubCategories { get; set; } = [];
    public IReadOnlyList<CrpgAnimEntry> Animations { get; set; } = [];
}
