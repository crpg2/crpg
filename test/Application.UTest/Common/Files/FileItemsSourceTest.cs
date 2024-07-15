using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Crpg.Application.Common.Files;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Files;

public class FileItemsSourceTest
{
    [Test]
    public void TestCanDeserializeFile()
    {
        FileItemsSource source = new();
        Assert.That(() => source.LoadItems(), Throws.Nothing);
    }

    [Test]
    public async Task CheckNoDuplicatedId()
    {
        List<string> duplicates = new();
        HashSet<string> ids = new();

        var items = await new FileItemsSource().LoadItems();
        foreach (var item in items)
        {
            if (ids.Contains(item.Id))
            {
                duplicates.Add(item.Id);
            }

            ids.Add(item.Id);
        }

        Assert.That(duplicates, Is.Empty,
           "Duplicate items: " + string.Join(", ", duplicates));
    }

    [Test]
    public async Task CheckNoTestItems()
    {
        var items = await new FileItemsSource().LoadItems();
        List<string> errors = new();
        foreach (var item in items)
        {
            if ((item.Id.Contains("test") || item.Id.Contains("dummy") || item.Name.Contains('_')) && !item.Id.Contains("elitesteppe"))
            {
                errors.Add(item.Id);
            }
        }

        Assert.That(errors, Is.Empty,
            $"Test items detected:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
    }

    [Test]
    public async Task CheckItemTier()
    {
        var items = await new FileItemsSource().LoadItems();
        List<string> errors = new();
        foreach (var item in items)
        {
            if (item.Tier > 13.1)
            {
                errors.Add(item.Id);
            }
        }

        Assert.That(errors, Is.Empty,
            $"Item with too higher tier:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
    }

    [Test]
    public async Task CheckPriceRange()
    {
        var items = await new FileItemsSource().LoadItems();
        List<string> errors = new();
        foreach (var item in items)
        {
            if (item.Price <= 0 || item.Price > 100_000)
            {
                errors.Add(item.Id);
            }
        }

        Assert.That(errors, Is.Empty,
            $"Items with zero, or negative price or price too high:{Environment.NewLine}- " + string.Join($"{Environment.NewLine}- ", errors));
    }

    [Test]
    public async Task CheckBotItemsExist()
    {
        var items = (await new FileItemsSource().LoadItems())
            .Select(i => i.Id)
            .ToHashSet();
        static string GetFilePath([CallerFilePath] string path = "")
        {
            return path;
        }

        string filepath = GetFilePath();
        string charactersFilePath = Path.Combine(filepath, "../../../../../src/Module.Server/ModuleData/characters.xml");
        string dtvCharactersFilePath = Path.Combine(filepath, "../../../../../src/Module.Server/ModuleData/dtv/dtv_characters.xml");
        string dtvItemsFilePath = Path.Combine(filepath, "../../../../../src/Module.Server/ModuleData/dtv/dtv_weapons.xml");
        Console.WriteLine(filepath);
        string charactersXmlPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                                   + "/ModuleData/dtv/dtv_characters.xml";
        XDocument charactersDoc = XDocument.Load(charactersFilePath);
        XDocument dtvCharactersDoc = XDocument.Load(dtvCharactersFilePath);
        XDocument dtvItemsDoc = XDocument.Load(dtvItemsFilePath);
        string[] itemIdsFromCharacterXml = charactersDoc
            .Descendants("equipment")
            .Select(el => el.Attribute("id")!.Value["Item.".Length..])
            .ToArray();
        string[] itemIdsFromDtvCharacterXml = dtvCharactersDoc
            .Descendants("equipment")
            .Select(el => el.Attribute("id")!.Value["Item.".Length..])
            .ToArray();
        var dtvItemIdsFromXml = dtvItemsDoc
            .Descendants("Item")
            .Select(el => el.Attribute("id")!.Value)
            .ToHashSet();

        var combinedItemIds = itemIdsFromCharacterXml.Concat(itemIdsFromDtvCharacterXml);
        var combinedItems = items.Concat(dtvItemIdsFromXml);

        Assert.Multiple(() =>
        {
            foreach (string itemId in combinedItemIds)
            {
                if (!combinedItems.Contains(itemId))
                {
                    string closestItemId = TestHelper.FindClosestString(itemId, combinedItems);
                    Assert.Fail($"Character item {itemId} was not found in items.json. Did you mean {closestItemId}?");
                    charactersDoc
                    .Descendants("equipment")
                    .First(el => el.Attribute("id")!.Value == "Item." + itemId).Attribute("id")!.Value = "Item." + closestItemId;
                }
            }

            // uncomment to automatically replace with suggestions
            // charactersDoc.Save(charactersFilePath);
        });
    }
}
