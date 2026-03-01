using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.ActivityLogs;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Api;

internal class StubCrpgClient : ICrpgClient
{
    private static readonly Random Random = new();

    public Task<CrpgResult<CrpgUser>> GetUserAsync(Platform platform, string platformUserId, CrpgRegion region,
        CancellationToken cancellationToken = default)
    {
        CrpgUser user = new()
        {
            Id = Random.Next(),
            Platform = Platform.Steam,
            PlatformUserId = Guid.NewGuid().ToString(),
            Gold = 0,
            Role = CrpgUserRole.User,
            Character = new CrpgCharacter
            {
                Id = Random.Next(),
                Name = "Peasant",
                Generation = 0,
                Level = 30,
                Experience = 0,
                Class = CrpgCharacterClass.Crossbowman,
                ForTournament = false,
                Characteristics = new CrpgCharacterCharacteristics
                {
                    Attributes = new CrpgCharacterAttributes
                    {
                        Points = 0,
                        Strength = 18,
                        Agility = 18,
                    },
                    Skills = new CrpgCharacterSkills
                    {
                        Points = 0,
                        IronFlesh = 5,
                        PowerStrike = 5,
                        PowerDraw = 5,
                        PowerThrow = 5,
                        Athletics = 5,
                        Riding = 5,
                        WeaponMaster = 5,
                        MountedArchery = 5,
                        Shield = 5,
                    },
                    WeaponProficiencies = new CrpgCharacterWeaponProficiencies
                    {
                        Points = 0,
                        OneHanded = 150,
                        TwoHanded = 150,
                        Polearm = 150,
                        Bow = 150,
                        Throwing = 150,
                        Crossbow = 150,
                    },
                },
                EquippedItems = new[]
                {
                    GetRandomEquippedItem(CrpgItemSlot.Head, ItemObject.ItemTypeEnum.HeadArmor),
                    GetRandomEquippedItem(CrpgItemSlot.Body, ItemObject.ItemTypeEnum.BodyArmor),
                    GetRandomEquippedItem(CrpgItemSlot.Leg, ItemObject.ItemTypeEnum.LegArmor),
                    GetRandomEquippedItem(CrpgItemSlot.Weapon0, ItemObject.ItemTypeEnum.TwoHandedWeapon),
                    GetRandomEquippedItem(CrpgItemSlot.Weapon1, ItemObject.ItemTypeEnum.Crossbow),
                    GetRandomEquippedItem(CrpgItemSlot.Weapon2, ItemObject.ItemTypeEnum.Bolts),
                },
                Statistics =
                new()
                {
                    Kills = 0,
                    Deaths = 0,
                    Assists = 0,
                    PlayTime = TimeSpan.FromSeconds(0),
                    Rating = new CrpgCharacterRating
                    {
                        Value = 0,
                        Deviation = 0,
                        Volatility = 0,
                    },
                },
            },
            Restrictions = Array.Empty<CrpgRestriction>(),
            ClanMembership = null,
        };

        return Task.FromResult(new CrpgResult<CrpgUser> { Data = user });
    }

    public Task<CrpgResult<CrpgUser>> GetTournamentUserAsync(Platform platform, string platformUserId, CancellationToken cancellationToken = default)
    {
        return GetUserAsync(platform, platformUserId, CrpgRegion.Eu, cancellationToken: cancellationToken);
    }

    public Task CreateActivityLogsAsync(IList<CrpgActivityLog> activityLogs, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<CrpgResult<CrpgClan>> GetClanAsync(int clanId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<CrpgResult<CrpgUsersUpdateResponse>> UpdateUsersAsync(CrpgGameUsersUpdateRequest req, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CrpgResult<CrpgUsersUpdateResponse>
        {
            Data = new CrpgUsersUpdateResponse { UpdateResults = Array.Empty<UpdateCrpgUserResult>() },
        });
    }

    public Task<CrpgResult<IList<CrpgUserItem>>> GetUserItemsAsync(Platform platform, string platformUserId,
        CancellationToken cancellationToken = default)
    {
        List<CrpgUserItem> userItems = [];
        foreach (var itemType in new[]
                 {
                     ItemObject.ItemTypeEnum.HeadArmor,
                     ItemObject.ItemTypeEnum.Cape,
                     ItemObject.ItemTypeEnum.BodyArmor,
                     ItemObject.ItemTypeEnum.HandArmor,
                     ItemObject.ItemTypeEnum.LegArmor,
                     ItemObject.ItemTypeEnum.HorseHarness,
                     ItemObject.ItemTypeEnum.Horse,
                     ItemObject.ItemTypeEnum.Shield,
                     ItemObject.ItemTypeEnum.Bow,
                     ItemObject.ItemTypeEnum.Crossbow,
                     ItemObject.ItemTypeEnum.OneHandedWeapon,
                     ItemObject.ItemTypeEnum.TwoHandedWeapon,
                     ItemObject.ItemTypeEnum.Polearm,
                     ItemObject.ItemTypeEnum.Thrown,
                     ItemObject.ItemTypeEnum.Arrows,
                     ItemObject.ItemTypeEnum.Bolts,
                     ItemObject.ItemTypeEnum.Banner,
                 })
        {
            for (int i = 0; i < 3; i += 1)
            {
                userItems.Add(new CrpgUserItem
                {
                    Id = Random.Next(),
                    ItemId = GetRandomItemId(itemType),
                    Rank = Random.Next(0, 3),
                    IsBroken = Random.Next(0, 100) <= 3,
                });
            }
        }

        return Task.FromResult(new CrpgResult<IList<CrpgUserItem>> { Data = userItems });
    }

    public Task<CrpgResult<CrpgRestriction>> RestrictUserAsync(CrpgRestrictionRequest req, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }

    private CrpgEquippedItem GetRandomEquippedItem(CrpgItemSlot slot, ItemObject.ItemTypeEnum itemType)
    {
        return new CrpgEquippedItem
        {
            Slot = slot,
            UserItem = new CrpgUserItem
            {
                Id = Random.Next(),
                ItemId = GetRandomItemId(itemType),
            },
        };
    }

    private string GetRandomItemId(ItemObject.ItemTypeEnum itemType)
    {
        return MBObjectManager.Instance
            .GetObjectTypeList<ItemObject>()
            .GetRandomElementWithPredicate(i => i.StringId.StartsWith("crpg_", StringComparison.Ordinal) && i.Type == itemType)
            .StringId;
    }
}
