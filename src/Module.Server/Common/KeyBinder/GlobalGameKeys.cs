using Crpg.Module.Common.KeyBinder;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.InputSystem;

namespace Crpg.Module.Server.Common.Keybinder;

public class GlobalGameKeys : IUseKeyBinder
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
                Id = "key_command_modifier",
                Name = "Command Modifier (hold)",
                Description = "Modifier key ie: Ctrl",
                DefaultInputKey = InputKey.LeftControl,
            },
        },
    };
}
