using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.InputSystem;

namespace Crpg.Module.Common.KeyBinder;

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
                Name = "{=KORWOuG3}Command Modifier (hold)",
                Description = "{=KORWOuG4}Modifier key ie: Ctrl",
                DefaultInputKey = InputKey.LeftControl,
            },
        },
    };
}
