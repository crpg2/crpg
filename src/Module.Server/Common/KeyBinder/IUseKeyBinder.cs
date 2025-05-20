using Crpg.Module.Common.KeyBinder.Models;

namespace Crpg.Module.Common.KeyBinder;

// Implement this interface if you use key binder.
public interface IUseKeyBinder
{
    public BindedKeyCategory BindedKeys { get; }
}

// Example Usage in MissionViews

/*
// DebugKeyBindView.cs -- example
using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI;

public class DebugKeyBindView : MissionView, IUseKeyBinder
{
    private static readonly string KeyCategoryId = "crpg_general"; // could be anything you want to create a new category
    BindedKeyCategory IUseKeyBinder.BindedKeys => new BindedKeyCategory
    {
        CategoryId = KeyCategoryId, // category id
        Category = "Crpg", // category display name
        Keys = new List<BindedKey>
        {
            new BindedKey()
            {
                Id = "key_debug_Id",
                Name = "debug_Name",
                Description = "debug description.",
                DefaultInputKey = InputKey.C,
            },
            new BindedKey()
            {
                Id = "key_debug_Id3",
                Name = "debug_Name3",
                Description = "debug description 3",
                DefaultInputKey = InputKey.K,
            },
        },
    };

    private GameKey? debugKey;
    private GameKey? debugKey3;

    public DebugKeyBindView()
    {
    }

    public override void EarlyStart()
    {
        debugKey = HotKeyManager.GetCategory(KeyCategoryId).GetGameKey("key_debug_Id");
        debugKey3 = HotKeyManager.GetCategory(KeyCategoryId).GetGameKey("key_debug_Id3");
    }

    public override void OnMissionTick(float dt)
    {
        if (debugKey != null && (Input.IsKeyPressed(debugKey.KeyboardKey.InputKey) || Input.IsKeyPressed(debugKey.ControllerKey.InputKey)))
        {
            InformationManager.DisplayMessage(new InformationMessage("Debug key bind 1 pressed!!"));
        }
        if (debugKey3 != null && (Input.IsKeyPressed(debugKey3.KeyboardKey.InputKey) || Input.IsKeyPressed(debugKey3.ControllerKey.InputKey)))
        {
            InformationManager.DisplayMessage(new InformationMessage("Debug key bind 3 pressed!!"));
        }
    }
}
*/
/*
// DebugKeyBindView2cs - Example
using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace Crpg.Module.GUI;

public class DebugKeyBindView2 : MissionView, IUseKeyBinder
{
    private static readonly string KeyCategoryId = KeyBinder.Categories.CrpgGeneral.CategoryId;
    BindedKeyCategory IUseKeyBinder.BindedKeys => new()
    {
        CategoryId = KeyCategoryId,
        Category = KeyBinder.Categories.CrpgGeneral.CategoryName,
        Keys = new List<BindedKey>
        {
            new()
            {
                Id = "key_debug_Id2",
                Name = "debug_Name2",
                Description = "debug description 2.",
                DefaultInputKey = InputKey.L,
            },
        },
    };

    private GameKey? debugKey;

    public DebugKeyBindView2()
    {
    }

    public override void EarlyStart()
    {
        debugKey = HotKeyManager.GetCategory(KeyCategoryId).GetGameKey("key_debug_Id2");
    }

    public override void OnMissionTick(float dt)
    {
        if (debugKey != null && (Input.IsKeyPressed(debugKey.KeyboardKey.InputKey) || Input.IsKeyPressed(debugKey.ControllerKey.InputKey)))
        {
            InformationManager.DisplayMessage(new InformationMessage("Debug key bind 2 pressed!!"));
        }
    }
}
*/
