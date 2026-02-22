using System.Reflection;
using Crpg.Module.Common.KeyBinder.Models;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace Crpg.Module.Common.KeyBinder;
public static class KeyBinder
{
    public static readonly ICollection<BindedKeyCategory> KeysCategories = new List<BindedKeyCategory>();
    public static readonly IDictionary<string, GameKeyBinderContext> KeyContexts = new Dictionary<string, GameKeyBinderContext>();

    // for predefined categories
    public readonly struct KeyCategoryInfo
    {
        public string CategoryId { get; }
        public string CategoryName { get; }

        public KeyCategoryInfo(string id, string name)
        {
            CategoryId = id;
            CategoryName = name;
        }
    }

    // predefined categories. you can also make your own where you create the gamekey by just using whatever string values you want
    public static class Categories
    {
        public static readonly KeyCategoryInfo CrpgGeneral = new("crpg_general", "Crpg General");
    }

    // creates and/or appends keys to a category
    public static void RegisterKeyGroup(BindedKeyCategory group)
    {
        if (group == null || string.IsNullOrWhiteSpace(group.CategoryId))
        {
            return;
        }

        var existingCategory = KeysCategories.FirstOrDefault(c => c.CategoryId == group.CategoryId);
        if (existingCategory != null)
        {
            // Append keys if they don't already exist by Id
            foreach (var newKey in group.Keys)
            {
                if (!existingCategory.Keys.Any(k => k.Id == newKey.Id))
                {
                    existingCategory.Keys.Add(newKey);
                }
                else
                {
                    Debug.Print($"[KeyBinder] Key '{newKey.Id}' already exists in category '{group.CategoryId}', skipping.", 0, Debug.DebugColor.Yellow);
                }
            }
        }
        else
        {
            KeysCategories.Add(group);
        }
    }

    // needs to be in OnSubModuleLoad Client before harmony patches applied
    public static void Initialize()
    {
        AutoRegister();

        var textManager = TaleWorlds.MountAndBlade.Module.CurrentModule.GlobalTextManager;
        var emptyTags = new List<GameTextManager.ChoiceTag>();

        foreach (var category in KeysCategories)
        {
            if (category == null || string.IsNullOrWhiteSpace(category.CategoryId))
            {
                continue;
            }

            KeyContexts[category.CategoryId] = new GameKeyBinderContext(category.CategoryId, category.Keys);

            // Category display name
            textManager.GetGameText("str_key_category_name")
                       .AddVariationWithId(category.CategoryId, new TextObject(category.Category), emptyTags);

            foreach (var key in category.Keys)
            {
                string variationId = $"{category.CategoryId}_{key.KeyId}";

                textManager.GetGameText("str_key_name")
                           .AddVariationWithId(variationId, new TextObject(key.Name), emptyTags);

                textManager.GetGameText("str_key_description")
                           .AddVariationWithId(variationId, new TextObject(key.Description), emptyTags);
            }
        }
    }

    // needs to be in OnSubModuleLoad Client after harmony patches applied
    public static void RegisterContexts()
    {
        // Retrieve existing categories from HotKeyManager
        var keyList = HotKeyManager.GetAllCategories().ToList();

        // Add our custom contexts if they don't already exist in the list
        foreach (var context in KeyContexts.Values)
        {
            if (!keyList.Contains(context))
            {
                keyList.Add(context);
            }

            // Register all contexts, including custom ones
            HotKeyManager.RegisterInitialContexts(keyList, true); // Assuming this accepts the list
        }
    }

    // Searches all project for IUseKeyBinder to build categories and keys list for harmony patch
    private static void AutoRegister()
    {
        var binderTypes = Assembly.GetExecutingAssembly()
            .DefinedTypes
            .Where(t => typeof(IUseKeyBinder).IsAssignableFrom(t) && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null);

        foreach (var type in binderTypes)
        {
            if (Activator.CreateInstance(type) is IUseKeyBinder binder && binder.BindedKeys != null)
            {
                // KeysCategories.Add(binder.BindedKeys);
                RegisterKeyGroup(binder.BindedKeys);
            }
        }
    }
}
