using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Xml.Linq;
using Crpg.Application.Common.Files;
using Crpg.Domain.Entities.Items;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Files;

public class FileItemsSourceTest
{
    [Test]
    public async Task Tests()
    {
        HashSet<string> ids = [];

        var items = await new FileItemsSource().LoadItems();

        Assert.Multiple(() =>
        {
            foreach (var item in items)
            {
                Assert.That(ids.Add(item.Id), Is.True, $"Duplicate item: {item.Id}");
                Assert.That((!item.Id.Contains("test")
                             && !item.Id.Contains("dummy")
                             && !item.Name.Contains('_'))
                            || item.Id.Contains("elitesteppe"), Is.True, $"{item.Id}: test item detected");
                Assert.That(item.Tier <= 13.1 || item.BaseId.Contains("disabled"), Is.True, $"{item.Id}: item tier too high");
                Assert.That(item.Price, Is.GreaterThan(0).And.LessThan(100_000),
                    $"{item.Id}: item has negative price or price is too high");

                if (item.Type == ItemType.Bow)
                {
                    if (item.Weapons.Count != 2)
                    {
                        Assert.Fail($"{item.Id}: bow should have 2 weapon components");
                        continue;
                    }

                    var w0 = item.Weapons[0];
                    var w1 = item.Weapons[1];

                    var flags0 = w0.Flags & ~WeaponFlags.AutoReload;
                    var flags1 = w1.Flags & ~WeaponFlags.AutoReload;

                    if (flags0 != flags1)
                    {
                        Assert.Fail($"{item.Id}: flags differ beyond AutoReload ({w0.Flags} vs {w1.Flags})");
                        continue;
                    }

                    string clone0 = JsonSerializer.Serialize(w0 with { Flags = flags0 });
                    string clone1 = JsonSerializer.Serialize(w1 with { Flags = flags1 });

                    if (clone0 != clone1)
                    {
                        Assert.Fail($"{item.Id}: weapon components differ beyond AutoReload");
                    }
                }
            }
        });
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

                    var botEquipmentElement = charactersDoc
                        .Descendants("equipment")
                        .FirstOrDefault(el => el.Attribute("id")!.Value == "Item." + itemId);

                    if (botEquipmentElement != null)
                    {
                        botEquipmentElement.Attribute("id")!.Value = "Item." + closestItemId;
                    }

                    var dtvEquipmentElement = dtvCharactersDoc
                        .Descendants("equipment")
                        .FirstOrDefault(el => el.Attribute("id")!.Value == "Item." + itemId);

                    if (dtvEquipmentElement != null)
                    {
                        dtvEquipmentElement.Attribute("id")!.Value = "Item." + closestItemId;
                    }
                }
            }

            // uncomment to automatically replace with suggestions
            // charactersDoc.Save(charactersFilePath);
            // dtvCharactersDoc.Save(dtvCharactersFilePath);
        });
    }
}
