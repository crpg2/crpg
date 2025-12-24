using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class UpdateCharacterItemsCommandTest : TestBase
{
    [Test]
    public async Task FullUpdate()
    {
        // Arrange user
        var user = new User();
        ActDb.Users.Add(user);

        // Create all new UserItems and their Items
        UserItem CreateUserItem(string id, ItemType type)
        {
            var item = new Item { Id = id, Type = type, Enabled = true };
            var userItem = new UserItem { User = user, Item = item };
            ActDb.Items.Add(item);
            ActDb.UserItems.Add(userItem);
            return userItem;
        }

        var headNew = CreateUserItem("2", ItemType.HeadArmor);
        var shoulderNew = CreateUserItem("4", ItemType.ShoulderArmor);
        var bodyNew = CreateUserItem("6", ItemType.BodyArmor);
        var handNew = CreateUserItem("8", ItemType.HandArmor);
        var legNew = CreateUserItem("10", ItemType.LegArmor);
        var mountHarnessNew = CreateUserItem("12", ItemType.MountHarness);
        var mountNew = CreateUserItem("14", ItemType.Mount);
        var weapon0New = CreateUserItem("16", ItemType.Arrows);
        var weapon1New = CreateUserItem("18", ItemType.Crossbow);
        var weapon2New = CreateUserItem("20", ItemType.Shield);
        var weapon3New = CreateUserItem("22", ItemType.TwoHandedWeapon);
        var weaponExtraNew = CreateUserItem("24", ItemType.Banner);

        // Create character and attach old equipped items (for initial state)
        var character = new Character { User = user };
        ActDb.Characters.Add(character);

        // Initial equipped items (can point to the same user items)
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Head, UserItem = headNew });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Shoulder, UserItem = shoulderNew });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Body, UserItem = bodyNew });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Hand, UserItem = handNew });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Leg, UserItem = legNew });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.MountHarness, UserItem = mountHarnessNew });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Mount, UserItem = mountNew });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Weapon0, UserItem = weapon0New });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Weapon1, UserItem = weapon1New });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Weapon2, UserItem = weapon2New });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.Weapon3, UserItem = weapon3New });
        character.EquippedItems.Add(new EquippedItem { Character = character, Slot = ItemSlot.WeaponExtra, UserItem = weaponExtraNew });

        await ActDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper, characterService);
        var cmd = new UpdateCharacterItemsCommand
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel>
            {
                new() { Slot = ItemSlot.Head, UserItemId = headNew.Id },
                new() { Slot = ItemSlot.Shoulder, UserItemId = shoulderNew.Id },
                new() { Slot = ItemSlot.Body, UserItemId = bodyNew.Id },
                new() { Slot = ItemSlot.Hand, UserItemId = handNew.Id },
                new() { Slot = ItemSlot.Leg, UserItemId = legNew.Id },
                new() { Slot = ItemSlot.MountHarness, UserItemId = mountHarnessNew.Id },
                new() { Slot = ItemSlot.Mount, UserItemId = mountNew.Id },
                new() { Slot = ItemSlot.Weapon0, UserItemId = weapon0New.Id },
                new() { Slot = ItemSlot.Weapon1, UserItemId = weapon1New.Id },
                new() { Slot = ItemSlot.Weapon2, UserItemId = weapon2New.Id },
                new() { Slot = ItemSlot.Weapon3, UserItemId = weapon3New.Id },
                new() { Slot = ItemSlot.WeaponExtra, UserItemId = weaponExtraNew.Id },
            },
        };

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors, Is.Null);

        // Print all slots and item IDs
        var userItemIdBySlot = result.Data!.ToDictionary(i => i.Slot, ei => ei.UserItem.Id);

        // Assert
        Assert.That(userItemIdBySlot[ItemSlot.Head], Is.EqualTo(headNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Shoulder], Is.EqualTo(shoulderNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Body], Is.EqualTo(bodyNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Hand], Is.EqualTo(handNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Leg], Is.EqualTo(legNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.MountHarness], Is.EqualTo(mountHarnessNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Mount], Is.EqualTo(mountNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Weapon0], Is.EqualTo(weapon0New.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Weapon1], Is.EqualTo(weapon1New.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Weapon2], Is.EqualTo(weapon2New.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Weapon3], Is.EqualTo(weapon3New.Id));
        Assert.That(userItemIdBySlot[ItemSlot.WeaponExtra], Is.EqualTo(weaponExtraNew.Id));
    }

    [Test]
    public async Task PartialUpdate()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        UserItem headOld = new() { User = user, Item = new Item { Id = "1", Type = ItemType.HeadArmor, Enabled = true } };
        UserItem headNew = new() { User = user, Item = new Item { Id = "2", Type = ItemType.HeadArmor, Enabled = true } };
        UserItem bodyNew = new() { User = user, Item = new Item { Id = "3", Type = ItemType.BodyArmor, Enabled = true } };
        UserItem handOld = new() { User = user, Item = new Item { Id = "4", Type = ItemType.HandArmor, Enabled = true } };
        UserItem legOld = new() { User = user, Item = new Item { Id = "5", Type = ItemType.LegArmor, Enabled = true } };

        ArrangeDb.Items.AddRange(headOld.Item!, handOld.Item!, legOld.Item!, headNew.Item!, bodyNew.Item!);
        ArrangeDb.UserItems.AddRange(headOld, handOld, legOld, headNew, bodyNew);

        Character character = new()
        {
            Name = "name",
            User = user,
            EquippedItems =
            {
                new EquippedItem { UserItem = headOld, Slot = ItemSlot.Head },
                new EquippedItem { UserItem = handOld, Slot = ItemSlot.Hand },
                new EquippedItem { UserItem = legOld, Slot = ItemSlot.Leg },
            },
        };
        ArrangeDb.Characters.Add(character);

        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper, characterService);

        var cmd = new UpdateCharacterItemsCommand
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel>
            {
                new() { UserItemId = headNew.Id, Slot = ItemSlot.Head }, // replace head
                new() { UserItemId = bodyNew.Id, Slot = ItemSlot.Body }, // add body
                new() { UserItemId = null, Slot = ItemSlot.Hand },       // remove hand
            },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        var userItemIdBySlot = result.Data!.ToDictionary(i => i.Slot, i => i.UserItem.Id);

        Assert.That(userItemIdBySlot[ItemSlot.Head], Is.EqualTo(headNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Body], Is.EqualTo(bodyNew.Id));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Hand));
        Assert.That(userItemIdBySlot[ItemSlot.Leg], Is.EqualTo(legOld.Id));
    }

    [Test]
    public async Task CharacterNotFound()
    {
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper, characterService);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = 1,
            UserId = user.Entity.Id,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task CharacterNotOwned()
    {
        var character = ArrangeDb.Characters.Add(new Character());
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper, characterService);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Entity.Id,
            UserId = user.Entity.Id,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task UserNotFound()
    {
        var character = ArrangeDb.Characters.Add(new Character());
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper, characterService);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Entity.Id,
            UserId = 1,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task ItemNotFound()
    {
        var character = ArrangeDb.Characters.Add(new Character());
        var user = ArrangeDb.Users.Add(new User
        {
            Characters = new List<Character> { character.Entity },
        });
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper, characterService);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Entity.Id,
            UserId = user.Entity.Id,
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = 1, Slot = ItemSlot.Head } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemNotFound));
    }

    [Test]
    public async Task ItemDisabled()
    {
        UserItem userItem = new() { Item = new Item { Type = ItemType.HeadArmor, Enabled = false } };
        Character character = new();
        User user = new()
        {
            Characters = { character },
            Items = { userItem },
        };
        ArrangeDb.Characters.Add(character);
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper, characterService);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = userItem.Id, Slot = ItemSlot.Head } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemDisabled));
    }

    [Test]
    public async Task PersonalItem()
    {
        Item item = new() { Type = ItemType.HeadArmor, Enabled = false };
        Character character = new();
        UserItem userItem = new() { Item = item, PersonalItem = new() };
        User user = new() { Characters = { character }, Items = { userItem } };
        ArrangeDb.Characters.Add(character);
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper, characterService);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = userItem.Id, Slot = ItemSlot.Head } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors, Is.Null);

        var userItemIdBySlot = result.Data!.ToDictionary(i => i.Slot, ei => ei.UserItem.Id);
        Assert.That(userItemIdBySlot[ItemSlot.Head], Is.EqualTo(userItem.Id));
    }

    [Test]
    public async Task ItemBroken()
    {
        UserItem userItem = new() { IsBroken = true, Item = new Item { Type = ItemType.HeadArmor, Enabled = true } };
        Character character = new();
        User user = new()
        {
            Characters = { character },
            Items = { userItem },
        };
        ArrangeDb.Characters.Add(character);
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper, characterService);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = userItem.Id, Slot = ItemSlot.Head } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors, Is.Not.Null.And.Not.Empty, "Expected an error result but got success.");
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemBroken));
    }

    [Test]
    public async Task ItemNotOwned()
    {
        User user = new();
        Character character = new() { User = user, UserId = user.Id };
        user.Characters.Add(character);

        User anotherUser = new() { Id = 999 };
        UserItem userItem = new()
        {
            Item = new Item { Type = ItemType.HeadArmor, Enabled = true },
            User = anotherUser,
            UserId = anotherUser.Id,
        };

        ArrangeDb.Users.Add(user);
        ArrangeDb.Users.Add(anotherUser);
        ArrangeDb.Characters.Add(character);
        ArrangeDb.UserItems.Add(userItem);
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper, characterService);

        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id, // important: character’s user
            Items = new List<EquippedItemIdViewModel>
            {
                new() { UserItemId = userItem.Id, Slot = ItemSlot.Head }, // unowned
            },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null, "Expected errors because the item is not owned by the character");
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemNotFound));
    }

    [TestCase(ItemType.HeadArmor, ItemSlot.Shoulder)]
    [TestCase(ItemType.ShoulderArmor, ItemSlot.Body)]
    [TestCase(ItemType.BodyArmor, ItemSlot.Hand)]
    [TestCase(ItemType.HandArmor, ItemSlot.Leg)]
    [TestCase(ItemType.LegArmor, ItemSlot.MountHarness)]
    [TestCase(ItemType.MountHarness, ItemSlot.Mount)]
    [TestCase(ItemType.Mount, ItemSlot.Weapon0)]
    [TestCase(ItemType.Shield, ItemSlot.Head)]
    [TestCase(ItemType.Bow, ItemSlot.Shoulder)]
    [TestCase(ItemType.Crossbow, ItemSlot.Body)]
    [TestCase(ItemType.OneHandedWeapon, ItemSlot.Hand)]
    [TestCase(ItemType.TwoHandedWeapon, ItemSlot.Leg)]
    [TestCase(ItemType.Polearm, ItemSlot.MountHarness)]
    [TestCase(ItemType.Thrown, ItemSlot.Mount)]
    [TestCase(ItemType.Arrows, ItemSlot.Head)]
    [TestCase(ItemType.Bolts, ItemSlot.Shoulder)]
    [TestCase(ItemType.Pistol, ItemSlot.Body)]
    [TestCase(ItemType.Musket, ItemSlot.Hand)]
    [TestCase(ItemType.Bullets, ItemSlot.Leg)]
    [TestCase(ItemType.Banner, ItemSlot.MountHarness)]
    public async Task WrongSlotForItemType(ItemType itemType, ItemSlot itemSlot)
    {
        Character character = new();
        UserItem userItem = new() { Item = new Item { Type = itemType, Enabled = true } };
        User user = new()
        {
            Items = { userItem },
            Characters = { character },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper, characterService);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = userItem.Id, Slot = itemSlot } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemBadSlot));
    }

    [TestCase(ItemFlags.DropOnAnyAction)]
    [TestCase(ItemFlags.DropOnWeaponChange)]
    [TestCase(ItemFlags.DropOnWeaponChange | ItemFlags.DropOnAnyAction)]
    public async Task DropOnWeaponChangeItemOnWeaponSlot(ItemFlags itemFlags)
    {
        Character character = new();
        UserItem userItem = new()
        {
            Item = new Item
            {
                Type = ItemType.Banner,
                Flags = itemFlags,
                Enabled = true,
            },
        };
        User user = new()
        {
            Items = { userItem },
            Characters = { character },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var characterService = new CharacterService(
            experienceTable: Mock.Of<IExperienceTable>(),
            competitiveRatingModel: Mock.Of<ICompetitiveRatingModel>(),
            constants: new Constants());

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper, characterService);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = userItem.Id, Slot = ItemSlot.Weapon0 } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemBadSlot));
    }
}
