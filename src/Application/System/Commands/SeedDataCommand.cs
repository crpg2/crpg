using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Limitations;
using Crpg.Domain.Entities.Notifications;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Servers;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Terrains;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Crpg.Application.System.Commands;

public record SeedDataCommand : IMediatorRequest
{
    internal class Handler : IMediatorRequestHandler<SeedDataCommand>
    {
        private static readonly Dictionary<SettlementType, int> StrategusSettlementDefaultTroops = new()
        {
            // TODO: to const
            [SettlementType.Village] = 1000,
            [SettlementType.Castle] = 4000,
            [SettlementType.Town] = 8000,
        };

        private readonly ICrpgDbContext _db;
        private readonly IItemsSource _itemsSource;
        private readonly IApplicationEnvironment _appEnv;
        private readonly ICharacterService _characterService;
        private readonly IExperienceTable _experienceTable;
        private readonly IActivityLogService _activityLogService;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IStrategusMap _strategusMap;
        private readonly ISettlementsSource _settlementsSource;

        public Handler(ICrpgDbContext db, IItemsSource itemsSource, IApplicationEnvironment appEnv,
            ICharacterService characterService, IExperienceTable experienceTable, IStrategusMap strategusMap,
            ISettlementsSource settlementsSource, IActivityLogService activityLogService, IUserNotificationService userNotificationService)
        {
            _db = db;
            _itemsSource = itemsSource;
            _appEnv = appEnv;
            _characterService = characterService;
            _experienceTable = experienceTable;
            _strategusMap = strategusMap;
            _settlementsSource = settlementsSource;
            _activityLogService = activityLogService;
            _userNotificationService = userNotificationService;
        }

        public async Task<Result> Handle(SeedDataCommand request, CancellationToken cancellationToken)
        {
            await CreateOrUpdateItems(cancellationToken);
            await CreateOrUpdateSettlements(cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            if (_appEnv.Environment == HostingEnvironment.Development)
            {
                await AddDevelopmentData(cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }

            return Result.NoErrors;
        }

        private async Task AddDevelopmentData(CancellationToken cancellationToken)
        {
            if (!await _db.Settings.AnyAsync())
            {
                _db.Settings.Add(new()
                {
                    Id = 1,
                    Discord = "https://discord.gg/c-rpg",
                    Steam = "https://steamcommunity.com/sharedfiles/filedetails/?id=2878356589",
                    Patreon = "https://www.patreon.com/crpg",
                    Github = "https://github.com/crpg2/crpg",
                    Reddit = "https://www.reddit.com/r/CRPG_Bannerlord",
                    ModDb = "https://www.moddb.com/mods/crpg",
                });
            }

            User takeo = new()
            {
                PlatformUserId = "76561197987525637",
                Name = "takeo",
                Gold = 30000,
                HeirloomPoints = 2,
                ExperienceMultiplier = 1.09f,
                Role = Role.Admin,
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_full.jpg"),
            };
            User namidaka = new()
            {
                PlatformUserId = "76561197979511363",
                Name = "Namidaka",
                Gold = 100000,
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/70/703178fb540263bd30d5b84562b1167985603273_full.jpg"),
            };
            User thradok = new()
            {
                PlatformUserId = "76561198011271387",
                Name = "Thradok Odai",
                Gold = 100000,
                Avatar = new Uri("https://avatars.cloudflare.steamstatic.com/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
            };
            User kinngrimm = new()
            {
                PlatformUserId = "76561197998594278",
                Name = "Kinngrimm",
                Gold = 100000,
                Avatar = new Uri("https://avatars.cloudflare.steamstatic.com/ed4f240198b8ad5ceebe4fad0160f13c1e0c3a1f_full.jpg"),
            };
            User orle = new()
            {
                PlatformUserId = "76561198016876889",
                Platform = Platform.Steam,
                Name = "orle",
                Role = Role.Admin,
                Gold = 1000000,
                HeirloomPoints = 12,
                ExperienceMultiplier = 1.09f,
                Avatar = new Uri("https://avatars.akamai.steamstatic.com/d51d5155b1a564421c0b3fd5fb7eed7c4474e73d_full.jpg"),
            };
            User peeky = new()
            {
                PlatformUserId = "76561199217717055",
                Platform = Platform.Steam,
                Name = "Peeky",
                Role = Role.Admin,
                Gold = 1000000,
                HeirloomPoints = 12,
                ExperienceMultiplier = 1.09f,
                Avatar = new Uri("https://avatars.fastly.steamstatic.com/d2eb4aa487279f3a52a2edac0566fd506cfc597f_full.jpg"),
            };
            User droob = new()
            {
                PlatformUserId = "76561198023558734",
                Platform = Platform.Steam,
                Name = "droob",
                Role = Role.Admin,
                Gold = 1000000,
                HeirloomPoints = 12,
                ExperienceMultiplier = 1.09f,
                Avatar = new Uri("https://avatars.cloudflare.steamstatic.com/2456d3a9f13512fb57d7b02bcf3b1249d662ad16_full.jpg"),
            };
            User kadse = new()
            {
                PlatformUserId = "76561198017779751",
                Platform = Platform.Steam,
                Name = "Kadse",
                Role = Role.Moderator,
                Region = Region.Eu,
                Gold = 100000,
                HeirloomPoints = 2,
                ExperienceMultiplier = 1.09f,
                Avatar = new Uri("https://avatars.akamai.steamstatic.com/8762690248c6809b0303cc803a1b2dacf3a12cd5_full.jpg"),
            };
            User ladoea = new()
            {
                PlatformUserId = "76561198041553690",
                Platform = Platform.Steam,
                Name = "ladoea",
                Role = Role.Moderator,
                Region = Region.Eu,
                Gold = 100000,
                HeirloomPoints = 2,
                ExperienceMultiplier = 1.09f,
                Avatar = new Uri("https://avatars.cloudflare.steamstatic.com/d7f24cffe8b7ba1ccdf1ca0d5244883584beb179_full.jpg"),
            };
            User laHire = new()
            {
                PlatformUserId = "76561198012340299",
                Name = "LaHire",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/31/31f7c86313e48dd924c08844f1cb2dd76e542a46_full.jpg"),
                Region = Region.Eu,
            };
            User elmaryk = new()
            {
                PlatformUserId = "76561197972800560",
                Name = "Elmaryk",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/05/059f27b9bdf15392d8b0114d8d106bd430398cf2_full.jpg"),
                Region = Region.Eu,
            };
            User azuma = new()
            {
                PlatformUserId = "76561198081821029",
                Name = "Azuma",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/57/57eab4bf98145304377078d0a3d73dc05d540714_full.jpg"),
                Region = Region.Eu,
            };
            User zorguy = new()
            {
                PlatformUserId = "76561197989897581",
                Name = "Zorguy",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/e1/e12361889a18f7e834447bd96b9389943200f693_full.jpg"),
                Region = Region.Eu,
            };
            User neostralie = new()
            {
                PlatformUserId = "76561197992190847",
                Name = "Neostralie",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/50/50696c5fc162251193044d50e84956a60b9b9750_full.jpg"),
            };
            User ecko = new()
            {
                PlatformUserId = "76561198003849595",
                Name = "Ecko",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b2/b22b63e50e6148d446735f9d10b53be3dbe8114a_full.jpg"),
            };
            User firebat = new()
            {
                PlatformUserId = "76561198034738782",
                Name = "Firebat",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/80/80cfe380953ec4b9c8c09c36b22278263c47f506_full.jpg"),
            };
            User sellka = new()
            {
                PlatformUserId = "76561197979977620",
                Name = "Sellka",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bf/bf1a595dea0ac57cfedc0d3156f58c966abc5c63_full.jpg"),
            };
            User leanir = new()
            {
                PlatformUserId = "76561198018585047",
                Name = "Laenir",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c1/c1eeba83d74ff6be9d9f42ca19fa15616a94dc2d_full.jpg"),
            };
            User opset = new()
            {
                PlatformUserId = "76561198009970770",
                Name = "Opset_the_Grey",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/36/36f6b77d3af6d18563101cea616590ba69b4ec81_full.jpg"),
            };
            User falcom = new()
            {
                PlatformUserId = "76561197963438590",
                Name = "[OdE]Falcom",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ff/ffbc4f2f33a16d764ce9aeb92495c05421738834_full.jpg"),
            };
            User brainfart = new()
            {
                PlatformUserId = "76561198007258336",
                Name = "Brainfart",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/06/06be92280c028dbf83951ccaa7857d1b46f50401_full.jpg"),
                Region = Region.Eu,
            };
            User kiwi = new()
            {
                PlatformUserId = "76561198050263436",
                Name = "Kiwi",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b1/b1eeebf4b5eaf0d0fd255e7bfd88dddac53a79b7_full.jpg"),
                Region = Region.Eu,
            };
            User ikarooz = new()
            {
                PlatformUserId = "76561198013940874",
                Name = "Ikarooz",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7f/7fd9de1adbc5a2d7d9f6f43905663051d1f3ad6b_full.jpg"),
                Region = Region.Eu,
            };
            User bryggan = new()
            {
                PlatformUserId = "76561198076068057",
                Name = "Bryggan",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b7/b7b0ba5b51367b8e667bac7be347c4b194e46c42_full.jpg"),
                Region = Region.Eu,
            };
            User schumetzq = new()
            {
                PlatformUserId = "76561198050714825",
                Name = "Schumetzq",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/02/02fd365a5cd57ab2a09ada405546c7e1732e6e09_full.jpg"),
                Region = Region.Eu,
            };
            User victorhh888 = new()
            {
                PlatformUserId = "76561197968139412",
                Name = "victorhh888",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/90/90fb01f63a3b68a4a6f06208c84cc03250f4786e_full.jpg"),
            };
            User distance = new()
            {
                PlatformUserId = "76561198874880658",
                Name = "远方",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/d1/d18e1efd0df9440d21a820e3f37ebfc57a2b9ed4_full.jpg"),
            };
            User bakhrat = new()
            {
                PlatformUserId = "76561198051386592",
                Name = "bakhrat 22hz",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/f3/f3b2fbe95be2dfe6f3f2d5ceaca04d75a1a81966_full.jpg"),
            };
            User lancelot = new()
            {
                PlatformUserId = "76561198015772903",
                Name = "Lancelot",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/e9/e9cb98a2cd5facedca0982a52eb47f37142c3555_full.jpg"),
            };
            User buddha = new()
            {
                PlatformUserId = "76561198036356550",
                Name = "Buddha.dll",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7f/7fab01b855c8e9704f0239fa716d182ad96e3ff8_full.jpg"),
            };
            User lerch = new()
            {
                PlatformUserId = "76561197988504032",
                Name = "Lerch_77",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c0/c0d5345e5592f47aeee066e73f27d884496e75e1_full.jpg"),
            };
            User tjens = new()
            {
                PlatformUserId = "76561197997439945",
                Name = "Tjens",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ce/ce5524c76a12dff71e0c02b3220907597ded1aca_full.jpg"),
            };
            User knitler = new()
            {
                PlatformUserId = "76561198034120910",
                Name = "Knitler",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/a1/a1174ff1fdc31ff8078511e16a73d9caeee4675b_full.jpg"),
            };
            User magnuclean = new()
            {
                PlatformUserId = "76561198044343808",
                Name = "Magnuclean",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/8a/8a7486e99e489a7e1f7ad356ab2dd4892e4e908e_full.jpg"),
            };
            User baronCyborg = new()
            {
                Platform = Platform.EpicGames,
                PlatformUserId = "76561198026044780",
                Name = "Baron Cyborg",
                Region = Region.Eu,
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/58/5838cfcd99e280d82f63d92472d6d5aecebfb812_full.jpg"),
            };
            User manik = new()
            {
                Platform = Platform.Microsoft,
                PlatformUserId = "76561198068833541",
                Name = "Manik",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ed/edf5af17958c09a5bbcb12e352d8fa9560c22aac_full.jpg"),
            };
            User ajroselle = new()
            {
                PlatformUserId = "76561199043634047",
                Name = "ajroselle",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
            };
            User skrael = new()
            {
                PlatformUserId = "76561197996473259",
                Name = "Skrael",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/95/950f9f3147d4c8530a5072825d01c34ee3f1afa1_full.jpg"),
            };
            User bedo = new()
            {
                PlatformUserId = "76561198068806579",
                Name = "bedo",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ce/ce19953febd356e443567298449acd7284050a83_full.jpg"),
            };
            User lambic = new()
            {
                PlatformUserId = "76561198065010536",
                Name = "Lambic",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/af/af03d6342998e9f6887ac12883279c78edec7272_full.jpg"),
            };
            User sanasar = new()
            {
                PlatformUserId = "76561198038834052",
                Name = "Sanasar",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/38/38b27ecb2cfd536bf553790e425ccd0a4ac9add7_full.jpg"),
            };
            User vlad007 = new()
            {
                PlatformUserId = "76561198007345621",
                Name = "Vlad007",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
            };
            User canp0g = new()
            {
                PlatformUserId = "76561198099388699",
                Name = "CaNp0G",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b2/b2dc0e2223189a9ba64377e3be43d0d99442432f_full.jpg"),
            };
            User shark = new()
            {
                PlatformUserId = "76561198035838802",
                Name = "Shark",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ed/edd897e10a88795339e102f3ff88730afd684dd9_full.jpg"),
            };
            User noobAmphetamine = new()
            {
                PlatformUserId = "76561198140492451",
                Name = "NoobamphetaminenoobAmphetamine",
                Region = Region.Eu,
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
            };
            User mundete = new()
            {
                PlatformUserId = "76561198298979454",
                Name = "Mundete",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/99/994d037cb361b375cf7f34d510664dca959e27d2_full.jpg"),
            };
            User aroyFalconer = new()
            {
                PlatformUserId = "76561198055090640",
                Name = "aroyfalconer",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
            };
            User insanitoid = new()
            {
                PlatformUserId = "76561198073114187",
                Name = "Insanitoid",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/23/23ca1018e64e454b05b558cbf9cc7d55d1e57fc5_full.jpg"),
            };
            User scarface = new()
            {
                PlatformUserId = "76561198279433049",
                Name = "Scarface",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7b/7b237d0943aa81b7f0637e46baff7eff9afa48ae_full.jpg"),
            };
            User xDem = new()
            {
                PlatformUserId = "76561197998420060",
                Name = "XDem",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/a1/a15730cb6852a7b3b8109ff70a8ab506ed221ea1_full.jpg"),
            };
            User disorot = new()
            {
                PlatformUserId = "76561198117963151",
                Name = "Disorot",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/7b/7bab1c0d1a1716a7648afdfd987c44bfb58367a8_full.jpg"),
            };
            User ace = new()
            {
                PlatformUserId = "76561198069571271",
                Name = "Ace",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ac/ac7445b35f7e18eebe0d2a728aaad139b0dca3c5_full.jpg"),
            };
            User sagar = new()
            {
                PlatformUserId = "76561198049628859",
                Name = "Sagar",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/01/0190fa213e030bcffdde532705df318f348e8d30_full.jpg"),
            };
            User greenShadow = new()
            {
                PlatformUserId = "76561198239298650",
                Name = "GreenShadow",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b7/b7f74b4cea3ce894e22890705466741276667e91_full.jpg"),
            };
            User hannibaru = new()
            {
                PlatformUserId = "76561198120421508",
                Name = "Hannibaru",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/af/af69a66c19d409449586fdd863a70ffca5a3924c_full.jpg"),
            };
            User drexx = new()
            {
                PlatformUserId = "76561198010855139",
                Name = "Drexx",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ee/ee56a301d3ec686b77c6d06c7517fbb57065b36b_full.jpg"),
            };
            User xarosh = new()
            {
                PlatformUserId = "76561198089566223",
                Name = "Xarosh",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/bc/bcc1c53ab76da0813e6456264ee6b588b30de7af_full.jpg"),
            };
            User tipsyToby = new()
            {
                PlatformUserId = "76561198084047374",
                Name = "TipsyToby1969",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/1c/1caacc14b003b71ddf09c56675c9462440dcb534_full.jpg"),
            };
            User localAlpha = new()
            {
                PlatformUserId = "76561198204128229",
                Name = "LocalAlpha",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5b58ff641803804038c3cb3529904b14bc22b2c_full.jpg"),
            };
            User alex = new()
            {
                PlatformUserId = "76561198049945204",
                Name = "Alex",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/c3/c300efbbcfae57c59095547ad9362c81c9001f07_full.jpg"),
            };
            User kedrynFuel = new()
            {
                PlatformUserId = "76561198124895605",
                Name = "KedrynFuel",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/d9/d94b47877d0f0a0e50f66d80a1de34bfbf94a56f_full.jpg"),
            };
            User luqero = new()
            {
                PlatformUserId = "76561197990543288",
                Name = "LuQeRo",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ad/adf81333c999516c251df9ca281553d487825f1c_full.jpg"),
            };
            User ilya = new()
            {
                PlatformUserId = "76561198116180462",
                Name = "ilya2106",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/f4/f4b04c6590153ebb1a43c9192627beb07bb613f3_full.jpg"),
            };
            User eztli = new()
            {
                PlatformUserId = "76561197995328883",
                Name = "Eztli",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/97/971a781269e5cd82b76d0cacc138f180bbfbb8d2_full.jpg"),
            };
            User telesto = new()
            {
                PlatformUserId = "76561198021932355",
                Name = "Telesto",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/fe/fef49e7fa7e1997310d705b2a6158ff8dc1cdfeb_full.jpg"),
            };
            User kypak = new()
            {
                PlatformUserId = "76561198133571210",
                Name = "Kypak",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/df/df6e263fe8cd9ec2d1a2a7d61da59d47f23a52cd_full.jpg"),
            };
            User devoidDragon = new()
            {
                PlatformUserId = "76561198018668459",
                Name = "DevoidDragon",
                Avatar = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/79/79a8119bd2a027755f93872d0d09b959909a0405_full.jpg"),
            };
            User krog = new()
            {
                PlatformUserId = "76561198070447937",
                Name = "krog",
                Gold = 40000,
                Avatar = new Uri("https://avatars.cloudflare.steamstatic.com/7668d01f842476a42dac041f85c9b336161bdbd0_full.jpg"),
            };

            User[] newUsers =
            {
                takeo, orle, peeky, droob, baronCyborg, magnuclean, knitler, tjens, lerch, buddha, lancelot, bakhrat, distance,
                victorhh888, schumetzq, bryggan, ikarooz, kiwi, brainfart, falcom, opset, leanir, sellka, firebat,
                ecko, neostralie, zorguy, azuma, elmaryk, namidaka, laHire, manik, ajroselle, skrael, bedo, lambic,
                sanasar, vlad007, canp0g, shark, noobAmphetamine, mundete, aroyFalconer, insanitoid, scarface,
                xDem, disorot, ace, sagar, greenShadow, hannibaru, drexx, xarosh, tipsyToby, localAlpha, alex,
                kedrynFuel, luqero, ilya, eztli, telesto, kypak, devoidDragon, krog, thradok, kinngrimm, kadse, ladoea,
            };

            var existingUsers = await _db.Users.ToDictionaryAsync(u => (u.Platform, u.PlatformUserId));
            foreach (var newUser in newUsers)
            {
                if (existingUsers.TryGetValue((newUser.Platform, newUser.PlatformUserId), out var existingUser))
                {
                    _db.Entry(existingUser).State = EntityState.Detached;

                    newUser.Id = existingUser.Id;
                    newUser.Version = existingUser.Version;
                    _db.Users.Update(newUser);
                }
                else
                {
                    _db.Users.Add(newUser);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);

            UserItem takeoItem1 = new() { User = takeo, ItemId = "crpg_thamaskene_steel_spatha_v1_h3" };
            UserItem takeoItem2 = new() { User = takeo, ItemId = "crpg_winds_fury_v1_h2" };
            UserItem orleItem1 = new() { User = orle, ItemId = "crpg_armet_h1", PersonalItem = new() };
            UserItem orleItem2 = new() { User = orle, ItemId = "crpg_decorated_scimitar_with_wide_grip_v1_h0", };
            UserItem orleItem3 = new() { User = orle, ItemId = "crpg_thamaskene_steel_spatha_v1_h2" };
            UserItem orleItem4 = new() { User = orle, ItemId = "crpg_decorated_short_spatha_v1_h1" };
            UserItem orleItem5 = new() { User = orle, ItemId = "crpg_scalpel_v1_h0" };
            UserItem orleItem6 = new() { User = orle, ItemId = "crpg_wolf_shoulder_v2_h3" };
            UserItem orleItem7 = new() { User = orle, ItemId = "crpg_battania_fur_boots_v2_h3" };
            UserItem orleItem8 = new() { User = orle, ItemId = "crpg_nordic_leather_cap_v2_h3" };
            UserItem orleItem9 = new() { User = orle, ItemId = "crpg_eastern_wrapped_armguards_v2_h3" };
            UserItem orleItem10 = new() { User = orle, ItemId = "crpg_blacksmith_hammer_v3_h0" };
            UserItem orleItem11 = new() { User = orle, ItemId = "crpg_scythe_v2_h3" };
            UserItem orleItem12 = new() { User = orle, ItemId = "crpg_rondel_v2_h3" };
            UserItem orleItem13 = new() { User = orle, ItemId = "crpg_crossbow_j_v4_h3" };
            UserItem orleItem14 = new() { User = orle, ItemId = "crpg_helping_hand_v3_h2" };
            UserItem orleItem15 = new() { User = orle, ItemId = "crpg_bolt_c_v4_h0" };
            UserItem orleItem16 = new() { User = orle, ItemId = "crpg_wooden_sword_v3_h3" };
            UserItem orleItem17 = new() { User = orle, ItemId = "crpg_basic_imperial_leather_armor_v2_h3" };
            UserItem orleItem18 = new() { User = orle, ItemId = "crpg_wooden_twohander_v3_h3" };
            UserItem orleItem19 = new() { User = orle, ItemId = "crpg_decorated_scimitar_with_wide_grip_v1_h1" };
            UserItem elmarykItem1 = new() { User = elmaryk, ItemId = "crpg_longsword_v2_h3" };
            UserItem elmarykItem2 = new() { User = elmaryk, ItemId = "crpg_avalanche_v2_h2" };
            UserItem laHireItem1 = new() { User = laHire, ItemId = "crpg_iron_cavalry_sword_v1_h1" };
            UserItem laHirekItem2 = new() { User = laHire, ItemId = "crpg_simple_saber_v1_h2" };
            UserItem laHirekItem3 = new() { User = laHire, ItemId = "crpg_steel_round_shield_v4_h0" };

            UserItem[] newUserItems =
            {
                takeoItem1, takeoItem2, orleItem1, orleItem2, orleItem3, orleItem4, orleItem5, orleItem6, orleItem7, orleItem8, orleItem9, orleItem10, orleItem11, orleItem12, orleItem13, orleItem14, orleItem15, orleItem16, orleItem17, orleItem18, orleItem19, elmarykItem1, elmarykItem2, laHireItem1, laHirekItem2, laHirekItem3,
            };

            var existingUserItems = await _db.UserItems.ToDictionaryAsync(pi => pi.ItemId);
            foreach (var newUserItem in newUserItems)
            {
                if (!existingUserItems.ContainsKey(newUserItem.ItemId))
                {
                    _db.UserItems.Add(newUserItem);
                }
            }

            Restriction takeoRestriction0 = new()
            {
                RestrictedUser = takeo,
                RestrictedByUser = orle,
                Duration = TimeSpan.FromDays(5),
                Type = RestrictionType.Join,
                Reason = "Reason0",
                CreatedAt = DateTime.UtcNow,
            };
            Restriction takeoRestriction1 = new()
            {
                RestrictedUser = takeo,
                RestrictedByUser = orle,
                Duration = TimeSpan.FromDays(5),
                Type = RestrictionType.Join,
                Reason = "Reason1",
                CreatedAt = DateTime.UtcNow,
            };
            Restriction baronCyborgRestriction0 = new()
            {
                RestrictedUser = baronCyborg,
                RestrictedByUser = orle,
                Duration = TimeSpan.FromDays(10),
                Type = RestrictionType.Join,
                Reason = "Reason2",
                CreatedAt = DateTime.UtcNow,
            };
            Restriction orleRestriction0 = new()
            {
                RestrictedUser = orle,
                RestrictedByUser = takeo,
                Duration = TimeSpan.Zero,
                Type = RestrictionType.Join,
                Reason = "INTERNAL REASON: Reason3",
                PublicReason = "PUBLIC REASON: Reason31",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
            };
            Restriction orleRestriction1 = new()
            {
                RestrictedUser = orle,
                RestrictedByUser = takeo,
                Duration = TimeSpan.FromDays(10),
                Type = RestrictionType.Join,
                Reason = "INTERNAL REASON: Lorem ipsum dolor sit amet consectetur adipisicing elit. Placeat deserunt temporibus consectetur perferendis illo cupiditate, dignissimos fugiat commodi, quibusdam necessitatibus mollitia neque, quam voluptatibus rem quas. Libero sapiente ullam aliquid.",
                PublicReason = "PUBLIC REASON: Lorem ipsum dolor sit amet consectetur adipisicing elit. Placeat deserunt temporibus consectetur perferendis illo cupiditate",
                CreatedAt = DateTime.UtcNow,
            };

            Restriction[] newRestrictions =
            {
                takeoRestriction0, takeoRestriction1, baronCyborgRestriction0,
            };

            _db.Restrictions.RemoveRange(await _db.Restrictions.ToArrayAsync());
            _db.Restrictions.AddRange(newRestrictions);

            Character takeoCharacter0 = new()
            {
                User = takeo,
                Name = takeo.Name,
                Generation = 2,
                Level = 23,
                Experience = _experienceTable.GetExperienceForLevel(23),
                Statistics = new List<CharacterStatistics>
                {
                    {
                        new CharacterStatistics
                        {
                            Kills = 1,
                            Deaths = 30,
                            Assists = 10,
                            PlayTime = new TimeSpan(10, 7, 5, 20),
                            GameMode = GameMode.CRPGDuel,
                            Rating = new()
                            {
                                Value = 2990,
                                Deviation = 350,
                                Volatility = 0.06f,
                                CompetitiveValue = 2990,
                            },
                        }
                    },
                },
            };
            Character takeoCharacter1 = new()
            {
                User = takeo,
                Name = "totoalala",
                Generation = 0,
                Level = 12,
                Experience = _experienceTable.GetExperienceForLevel(12),
                Statistics = new List<CharacterStatistics>
                {
                    {
                        new CharacterStatistics
                        {
                            Kills = 2,
                            Deaths = 3,
                            Assists = 6,
                            PlayTime = new TimeSpan(365, 0, 0, 2),
                            GameMode = GameMode.CRPGBattle,
                        }
                    },
                },
            };
            Character takeoCharacter2 = new()
            {
                User = takeo,
                Name = "Retire me",
                Generation = 0,
                Level = 31,
                Experience = _experienceTable.GetExperienceForLevel(31) + 100,
                Statistics = new List<CharacterStatistics>
                {
                    {
                        new CharacterStatistics
                        {
                            Kills = 2,
                            Deaths = 3,
                            Assists = 6,
                            PlayTime = new TimeSpan(3, 7, 0, 29),
                            GameMode = GameMode.CRPGBattle,
                        }
                    },
                },
            };
            Character namidakaCharacter0 = new()
            {
                User = namidaka,
                Name = "namichar",
                Level = 10,
                Experience = 146457,
                Statistics = new List<CharacterStatistics>
                {
                    {
                        new CharacterStatistics
                        {
                            Kills = 1,
                            Deaths = 30,
                            Assists = 10,
                            PlayTime = new TimeSpan(10, 7, 5, 20),
                            GameMode = GameMode.CRPGDuel,
                            Rating = new()
                            {
                                Value = 2400,
                                Deviation = 350,
                                Volatility = 0.06f,
                                CompetitiveValue = 2400,
                            },
                        }
                    },
                },
            };

            Character peekyCharacter0 = new()
            {
                User = peeky,
                Name = "Peeky Soldier",
                Level = 33,
                Generation = 3,
                Experience = _experienceTable.GetExperienceForLevel(33) + (_experienceTable.GetExperienceForLevel(34) - _experienceTable.GetExperienceForLevel(33)) / 2,
                Statistics = new List<CharacterStatistics>
                {
                    {
                        new CharacterStatistics
                        {
                            Kills = 13,
                            Deaths = 7,
                            Assists = 6,
                            PlayTime = new TimeSpan(365, 0, 0, 20),
                            GameMode = GameMode.CRPGConquest,
                            Rating = new()
                            {
                                Value = 1250,
                                Deviation = 100,
                                Volatility = 100,
                                CompetitiveValue = 1250,
                            },
                        }
                    },
                },
                Characteristics = new CharacterCharacteristics
                {
                    Attributes = new CharacterAttributes { Points = 100 },
                    Skills = new CharacterSkills { Points = 100 },
                },
            };

            Character orleCharacter0 = new()
            {
                User = orle,
                Name = "Orle Soldier",
                Level = 33,
                Generation = 3,
                Experience = _experienceTable.GetExperienceForLevel(33) + (_experienceTable.GetExperienceForLevel(34) - _experienceTable.GetExperienceForLevel(33)) / 2,
                Statistics = new List<CharacterStatistics>
                {
                    {
                        new CharacterStatistics
                        {
                            Kills = 2,
                            Deaths = 3,
                            Assists = 6,
                            PlayTime = new TimeSpan(365, 0, 0, 20),
                            GameMode = GameMode.CRPGDuel,
                            Rating = new()
                            {
                                Value = 1900,
                                Deviation = 100,
                                Volatility = 100,
                                CompetitiveValue = 1900,
                            },
                        }
                    },
                },
                Characteristics = new CharacterCharacteristics
                {
                    Attributes = new CharacterAttributes { Points = 100 },
                    Skills = new CharacterSkills { Points = 100 },
                },
            };

            Character orleCharacter1 = new()
            {
                User = orle,
                Name = "Orle Peasant",
                Level = 25,
                Experience = _experienceTable.GetExperienceForLevel(25) + (_experienceTable.GetExperienceForLevel(26) - _experienceTable.GetExperienceForLevel(25)) / 2,
            };
            Character orleCharacter2 = new()
            {
                User = orle,
                Name = "Orle Farmer",
                Level = 25,
                Experience = _experienceTable.GetExperienceForLevel(25) + (_experienceTable.GetExperienceForLevel(26) - _experienceTable.GetExperienceForLevel(25)) / 2,
            };
            Character droobCharacter0 = new()
            {
                User = droob,
                Name = "Droob Soldier",
                Level = 38,
                Generation = 3,
                Experience = _experienceTable.GetExperienceForLevel(38),
                Statistics = new List<CharacterStatistics>
                {
                    {
                        new CharacterStatistics
                        {
                            Kills = 1,
                            Deaths = 30,
                            Assists = 10,
                            PlayTime = new TimeSpan(10, 7, 5, 20),
                            GameMode = GameMode.CRPGBattle,
                            Rating = new()
                            {
                                Value = 1500,
                                Deviation = 350,
                                Volatility = 0.06f,
                                CompetitiveValue = 600,
                            },
                        }
                    },
                    {
                        new CharacterStatistics
                        {
                            Kills = 10,
                            Deaths = 0,
                            Assists = 10,
                            PlayTime = new TimeSpan(100, 3, 50, 15),
                            GameMode = GameMode.CRPGConquest,
                            Rating = new()
                            {
                                Value = 1200,
                                Deviation = 100,
                                Volatility = 100,
                                CompetitiveValue = 1200,
                            },
                        }
                    },
                    {
                        new CharacterStatistics
                        {
                            Kills = 133,
                            Deaths = 7,
                            Assists = 0,
                            PlayTime = new TimeSpan(100, 3, 50, 15),
                            GameMode = GameMode.CRPGDuel,
                            Rating = new()
                            {
                                Value = 5000,
                                Deviation = 5,
                                Volatility = 0.05f,
                                CompetitiveValue = 5000,
                            },
                        }
                    },
                },
                Characteristics = new CharacterCharacteristics
                {
                    Attributes = new CharacterAttributes { Points = 100 },
                    Skills = new CharacterSkills { Points = 100 },
                },
            };
            Character kadseCharacter0 = new()
            {
                User = kadse,
                Name = "Wario Kadse",
                Level = 33,
                Generation = 3,
                Experience = _experienceTable.GetExperienceForLevel(33) + (_experienceTable.GetExperienceForLevel(34) - _experienceTable.GetExperienceForLevel(33)) / 2,
                Statistics = new List<CharacterStatistics>
                {
                    {
                        new CharacterStatistics
                        {
                            Kills = 1,
                            Deaths = 30,
                            Assists = 10,
                            PlayTime = new TimeSpan(10, 7, 5, 20),
                            GameMode = GameMode.CRPGDuel,
                            Rating = new()
                            {
                                Value = 50,
                                Deviation = 350,
                                Volatility = 0.06f,
                                CompetitiveValue = 50,
                            },
                        }
                    },
                },
                Characteristics = new CharacterCharacteristics
                {
                    Attributes = new CharacterAttributes { Points = 100 },
                    Skills = new CharacterSkills { Points = 100 },
                },
            };
            Character ladoeaCharacter0 = new()
            {
                User = ladoea,
                Name = "ladoea woz ere",
                Level = 33,
                Generation = 3,
                Experience = _experienceTable.GetExperienceForLevel(33) + (_experienceTable.GetExperienceForLevel(34) - _experienceTable.GetExperienceForLevel(33)) / 2,
                Statistics = new List<CharacterStatistics>
                {
                    {
                        new CharacterStatistics
                        {
                            Kills = 1,
                            Deaths = 30,
                            Assists = 10,
                            PlayTime = new TimeSpan(10, 7, 5, 20),
                            GameMode = GameMode.CRPGDuel,
                            Rating = new()
                            {
                                Value = 50,
                                Deviation = 350,
                                Volatility = 0.06f,
                                CompetitiveValue = 50,
                            },
                        }
                    },
                },
                Characteristics = new CharacterCharacteristics
                {
                    Attributes = new CharacterAttributes { Points = 100 },
                    Skills = new CharacterSkills { Points = 100 },
                },
            };
            Character falcomCharacter0 = new()
            {
                User = falcom,
                Name = falcom.Name,
            };
            Character victorhh888Character0 = new()
            {
                User = victorhh888,
                Name = victorhh888.Name,
            };
            Character sellkaCharacter0 = new()
            {
                User = sellka,
                Name = sellka.Name,
            };
            Character krogCharacter0 = new()
            {
                User = krog,
                Name = krog.Name,
            };
            Character noobAmphetamine0 = new()
            {
                User = noobAmphetamine,
                Name = noobAmphetamine.Name,
            };
            Character baronCyborg0 = new()
            {
                User = baronCyborg,
                Name = baronCyborg.Name,
            };
            Character[] newCharacters =
            {
                takeoCharacter0, takeoCharacter1, takeoCharacter2, namidakaCharacter0, peekyCharacter0, orleCharacter0, orleCharacter1, orleCharacter2, droobCharacter0,
                falcomCharacter0, victorhh888Character0, sellkaCharacter0, krogCharacter0, kadseCharacter0, noobAmphetamine0, baronCyborg0, ladoeaCharacter0,
            };

            var existingCharacters = await _db.Characters.ToDictionaryAsync(c => c.Name);
            foreach (var newCharacter in newCharacters)
            {
                _characterService.ResetCharacterCharacteristics(newCharacter, respecialization: true);

                if (existingCharacters.TryGetValue(newCharacter.Name, out var existingCharacter))
                {
                    _db.Entry(existingCharacter).State = EntityState.Detached;

                    newCharacter.Id = existingCharacter.Id;
                    newCharacter.Version = existingCharacter.Version;
                    _db.Characters.Update(newCharacter);
                }
                else
                {
                    _db.Characters.Add(newCharacter);
                }
            }

            CharacterLimitations takeoCharacter0Limitations = new()
            {
                Character = takeoCharacter0,
                LastRespecializeAt = DateTime.UtcNow.AddDays(-1).AddMinutes(21),
            };
            CharacterLimitations takeoCharacter1Limitations = new()
            {
                Character = takeoCharacter1,
                LastRespecializeAt = DateTime.UtcNow.AddDays(-2),
            };
            CharacterLimitations takeoCharacter2Limitations = new()
            {
                Character = takeoCharacter2,
                LastRespecializeAt = DateTime.UtcNow.AddDays(-8),
            };
            CharacterLimitations orleCharacter0Limitations = new()
            {
                Character = orleCharacter0,
                LastRespecializeAt = DateTime.UtcNow.AddDays(-8),
            };
            CharacterLimitations orleCharacter2Limitations = new()
            {
                Character = orleCharacter2,
                LastRespecializeAt = DateTime.UtcNow.AddDays(-1).AddMinutes(-30),
            };
            CharacterLimitations kadseCharacter0Limitations = new()
            {
                Character = kadseCharacter0,
                LastRespecializeAt = DateTime.UtcNow.AddDays(-8),
            };
            CharacterLimitations droobCharacter0Limitations = new()
            {
                Character = droobCharacter0,
                LastRespecializeAt = DateTime.UtcNow.AddDays(-8),
            };
            CharacterLimitations[] newCharactersLimitations =
            {
                takeoCharacter0Limitations, takeoCharacter1Limitations, takeoCharacter2Limitations, orleCharacter0Limitations,
                orleCharacter2Limitations, kadseCharacter0Limitations, droobCharacter0Limitations,
            };

            var existingCharactersLimitations = await _db.CharacterLimitations.ToDictionaryAsync(l => l.CharacterId);
            foreach (var newCharacterLimitations in newCharactersLimitations)
            {
                if (existingCharactersLimitations.TryGetValue(newCharacterLimitations.Character!.Id, out var existingCharacterLimitations))
                {
                    _db.Entry(existingCharacterLimitations).State = EntityState.Detached;
                    newCharacterLimitations.CharacterId = existingCharacterLimitations.CharacterId;
                    _db.CharacterLimitations.Update(newCharacterLimitations);
                }
                else
                {
                    _db.CharacterLimitations.Add(newCharacterLimitations);
                }
            }

            Clan pecores = new()
            {
                Tag = "PEC",
                PrimaryColor = 4278190318,
                SecondaryColor = 4294957414,
                Name = "Pecores",
                BannerKey = string.Empty,
                Region = Region.Eu,
                Languages = { Languages.Fr, Languages.En, },
            };

            Clan droobClan = new()
            {
                Tag = "DROO",
                PrimaryColor = 4278190318,
                SecondaryColor = 4294957414,
                Name = "Droob clan",
                BannerKey = string.Empty,
                Region = Region.Eu,
            };

            ClanMember droobMember = new() { User = droob, Clan = droobClan, Role = ClanMemberRole.Leader, };

            ClanMember takeoMember = new() { User = takeo, Clan = pecores, Role = ClanMemberRole.Officer, };
            ClanMember orleMember = new() { User = orle, Clan = pecores, Role = ClanMemberRole.Leader, };
            ClanMember elmarykMember = new() { User = elmaryk, Clan = pecores, Role = ClanMemberRole.Officer, };
            ClanMember laHireMember = new() { User = laHire, Clan = pecores, Role = ClanMemberRole.Member };

            ClanArmoryItem takeoClanArmoryItem1 = new() { UserItem = takeoItem1, Lender = takeoMember };
            ClanArmoryItem takeoClanArmoryItem2 = new() { UserItem = takeoItem2, Lender = takeoMember };
            ClanArmoryItem orleClanArmoryItem1 = new() { UserItem = orleItem1, Lender = orleMember };
            ClanArmoryItem orleClanArmoryItem2 = new() { UserItem = orleItem2, Lender = orleMember };
            ClanArmoryItem orleClanArmoryItem3 = new() { UserItem = orleItem3, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem4 = new() { UserItem = orleItem4, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem5 = new() { UserItem = orleItem5, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem6 = new() { UserItem = orleItem6, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem7 = new() { UserItem = orleItem7, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem8 = new() { UserItem = orleItem8, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem9 = new() { UserItem = orleItem9, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem10 = new() { UserItem = orleItem10, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem11 = new() { UserItem = orleItem11, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem12 = new() { UserItem = orleItem12, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem13 = new() { UserItem = orleItem13, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem14 = new() { UserItem = orleItem14, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem15 = new() { UserItem = orleItem15, Lender = orleMember, };
            ClanArmoryItem orleClanArmoryItem16 = new() { UserItem = orleItem16, Lender = orleMember, };
            ClanArmoryItem elmarykClanArmoryItem1 = new() { UserItem = elmarykItem1, Lender = elmarykMember };
            ClanArmoryItem elmarykClanArmoryItem2 = new() { UserItem = elmarykItem2, Lender = elmarykMember };
            ClanArmoryItem laHireClanArmoryItem1 = new() { UserItem = laHireItem1, Lender = laHireMember };
            ClanArmoryItem laHireClanArmoryItem2 = new() { UserItem = laHirekItem2, Lender = laHireMember, };
            ClanArmoryItem laHireClanArmoryItem3 = new() { UserItem = laHirekItem3, Lender = laHireMember, };

            ClanArmoryItem[] newClanArmoryItems =
            {
                takeoClanArmoryItem1, takeoClanArmoryItem2,
                orleClanArmoryItem2, orleClanArmoryItem3, orleClanArmoryItem4, orleClanArmoryItem5, orleClanArmoryItem6, orleClanArmoryItem7,  orleClanArmoryItem8, orleClanArmoryItem9, orleClanArmoryItem10, orleClanArmoryItem11, orleClanArmoryItem12, orleClanArmoryItem13, orleClanArmoryItem14, orleClanArmoryItem15, orleClanArmoryItem16, elmarykClanArmoryItem1, elmarykClanArmoryItem2, laHireClanArmoryItem1, laHireClanArmoryItem2, laHireClanArmoryItem3,
            };

            foreach (var newClanArmoryItem in newClanArmoryItems)
            {
                if (!pecores.ArmoryItems.Contains(newClanArmoryItem))
                {
                    pecores.ArmoryItems.Add(newClanArmoryItem);
                }
            }

            ClanArmoryBorrowedItem orleBorrowedItem1 = new() { UserItem = laHireClanArmoryItem2.UserItem, Borrower = orleMember };
            ClanArmoryBorrowedItem orleBorrowedItem2 = new() { UserItem = laHireClanArmoryItem3.UserItem, Borrower = orleMember };
            ClanArmoryBorrowedItem elmarykBorrowedItem1 = new() { UserItem = orleClanArmoryItem2.UserItem, Borrower = elmarykMember };
            ClanArmoryBorrowedItem elmarykBorrowedItem2 = new() { UserItem = takeoClanArmoryItem1.UserItem, Borrower = elmarykMember };
            ClanArmoryBorrowedItem laHireBorrowedItem1 = new() { UserItem = takeoClanArmoryItem2.UserItem, Borrower = laHireMember };
            ClanArmoryBorrowedItem laHireBorrowedItem2 = new() { UserItem = orleClanArmoryItem15.UserItem, Borrower = laHireMember };

            ClanArmoryBorrowedItem[] newClanArmoryBorrowedItems =
            {
                orleBorrowedItem1, orleBorrowedItem2, elmarykBorrowedItem1, elmarykBorrowedItem2, laHireBorrowedItem1, laHireBorrowedItem2,
            };

            foreach (var newClanArmoryBorrowedItem in newClanArmoryBorrowedItems)
            {
                if (!pecores.ArmoryBorrowedItems.Contains(newClanArmoryBorrowedItem))
                {
                    pecores.ArmoryBorrowedItems.Add(newClanArmoryBorrowedItem);
                }
            }

            Clan ats = new()
            {
                Tag = "ATS",
                PrimaryColor = 4281348144,
                SecondaryColor = 4281348144,
                Name = "Among The Shadows",
                BannerKey = string.Empty,
                Region = Region.Na,
            };
            Clan legio = new()
            {
                Tag = "LEG",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "Legio",
                BannerKey = string.Empty,
                Region = Region.Eu,
                Languages = { Languages.Es, Languages.En, },
            };
            Clan theGrey = new()
            {
                Tag = "GREY",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "The Grey",
                BannerKey = string.Empty,
                Region = Region.Eu,
                Languages = { Languages.Pl, Languages.En, },
            };
            Clan ode = new()
            {
                Tag = "OdE",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "Ordre de l'étoile",
                BannerKey = string.Empty,
                Region = Region.Eu,
            };
            Clan virginDefenders = new()
            {
                Tag = "VD",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "Virgin Defenders",
                BannerKey = string.Empty,
                Region = Region.Eu,
            };
            Clan randomClan = new()
            {
                Tag = "RC",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "Random Clan",
                BannerKey = string.Empty,
                Region = Region.As,
            };
            Clan abcClan = new()
            {
                Tag = "ABC",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "ABC",
                BannerKey = string.Empty,
                Region = Region.As,
            };
            Clan defClan = new()
            {
                Tag = "DEF",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "DEF",
                BannerKey = string.Empty,
                Region = Region.Na,
            };
            Clan ghiClan = new()
            {
                Tag = "GHI",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "GHI",
                BannerKey = string.Empty,
                Region = Region.Oc,
            };
            Clan jklClan = new()
            {
                Tag = "JKL",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "JKL",
                BannerKey = string.Empty,
                Region = Region.Oc,
            };
            Clan mnoClan = new()
            {
                Tag = "MNO",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "MNO",
                BannerKey = string.Empty,
                Region = Region.Eu,
            };
            Clan pqrClan = new()
            {
                Tag = "PQR",
                PrimaryColor = 1234567,
                SecondaryColor = 890,
                Name = "Plan QR",
                BannerKey = string.Empty,
                Region = Region.Eu,
            };
            Clan[] newClans =
            {
                pecores, ats, legio, theGrey, ode, virginDefenders, randomClan, abcClan, defClan, ghiClan, jklClan,
                mnoClan, pqrClan, droobClan,
            };

            var existingClans = await _db.Clans.ToDictionaryAsync(c => c.Name);
            foreach (var newClan in newClans)
            {
                if (!existingClans.ContainsKey(newClan.Name))
                {
                    _db.Clans.Add(newClan);
                }
            }

            ClanMember neostralieMember = new() { User = neostralie, Clan = pecores, Role = ClanMemberRole.Officer };
            ClanMember azumaMember = new() { User = azuma, Clan = pecores, Role = ClanMemberRole.Member };
            ClanMember zorguyMember = new() { User = zorguy, Clan = pecores, Role = ClanMemberRole.Member };
            ClanMember eckoMember = new() { User = ecko, Clan = ats, Role = ClanMemberRole.Leader };
            ClanMember firebatMember = new() { User = firebat, Clan = ats, Role = ClanMemberRole.Officer };
            ClanMember sellkaMember = new() { User = sellka, Clan = ats, Role = ClanMemberRole.Member };
            ClanMember leanirMember = new() { User = leanir, Clan = legio, Role = ClanMemberRole.Leader, };
            ClanMember opsetMember = new() { User = opset, Clan = theGrey, Role = ClanMemberRole.Leader, };
            ClanMember falcomMember = new() { User = falcom, Clan = ode, Role = ClanMemberRole.Leader, };
            ClanMember brainfartMember = new() { User = brainfart, Clan = virginDefenders, Role = ClanMemberRole.Leader };
            ClanMember kiwiMember = new() { User = kiwi, Clan = virginDefenders, Role = ClanMemberRole.Officer };
            ClanMember ikaroozMember = new() { User = ikarooz, Clan = virginDefenders, Role = ClanMemberRole.Member };
            ClanMember brygganMember = new() { User = bryggan, Clan = virginDefenders, Role = ClanMemberRole.Member };
            ClanMember schumetzqMember = new() { User = schumetzq, Clan = virginDefenders, Role = ClanMemberRole.Member };
            ClanMember victorhh888Member = new() { User = victorhh888, Clan = randomClan, Role = ClanMemberRole.Leader };
            ClanMember distanceMember = new() { User = distance, Clan = randomClan, Role = ClanMemberRole.Officer };
            ClanMember bakhratMember = new() { User = bakhrat, Clan = randomClan, Role = ClanMemberRole.Member };
            ClanMember lancelotMember = new() { User = lancelot, Clan = abcClan, Role = ClanMemberRole.Leader };
            ClanMember buddhaMember = new() { User = buddha, Clan = abcClan, Role = ClanMemberRole.Member };
            ClanMember lerchMember = new() { User = lerch, Clan = defClan, Role = ClanMemberRole.Leader };
            ClanMember tjensMember = new() { User = tjens, Clan = ghiClan, Role = ClanMemberRole.Leader };
            ClanMember knitlerMember = new() { User = knitler, Clan = jklClan, Role = ClanMemberRole.Leader };
            ClanMember magnucleanMember = new() { User = magnuclean, Clan = mnoClan, Role = ClanMemberRole.Leader };
            ClanMember baronCyborgMember = new() { User = baronCyborg, Clan = pqrClan, Role = ClanMemberRole.Leader, };
            ClanMember noobAmphetamineMember = new() { User = noobAmphetamine, Clan = pecores, Role = ClanMemberRole.Member };

            ClanMember[] newClanMembers =
            {
                takeoMember, orleMember, elmarykMember, neostralieMember, laHireMember, azumaMember, zorguyMember,
                eckoMember, firebatMember, sellkaMember, leanirMember, opsetMember,
                falcomMember, brainfartMember, kiwiMember, ikaroozMember, brygganMember, schumetzqMember,
                victorhh888Member, distanceMember, bakhratMember, lancelotMember,
                buddhaMember, lerchMember, tjensMember, knitlerMember, magnucleanMember, baronCyborgMember, noobAmphetamineMember, droobMember,
            };
            var existingClanMembers = await _db.ClanMembers.ToDictionaryAsync(cm => cm.UserId);
            foreach (var newClanMember in newClanMembers)
            {
                if (!existingClanMembers.ContainsKey(newClanMember.User!.Id))
                {
                    _db.ClanMembers.Add(newClanMember);
                }
            }

            ClanInvitation schumetzqRequestForPecores = new()
            {
                Clan = pecores,
                Invitee = schumetzq,
                Inviter = schumetzq,
                Type = ClanInvitationType.Request,
                Status = ClanInvitationStatus.Pending,
            };
            ClanInvitation victorhh888MemberRequestForPecores = new()
            {
                Clan = pecores,
                Invitee = victorhh888,
                Inviter = victorhh888,
                Type = ClanInvitationType.Request,
                Status = ClanInvitationStatus.Pending,
            };
            ClanInvitation neostralieOfferToBrygganForPecores = new()
            {
                Clan = pecores,
                Inviter = neostralie,
                Invitee = bryggan,
                Type = ClanInvitationType.Offer,
                Status = ClanInvitationStatus.Pending,
            };

            var activityLogUserRewarded = _activityLogService.CreateUserRewardedLog(orle.Id, namidaka.Id, 120000, 3, orleItem1.ItemId);
            activityLogUserRewarded.CreatedAt = DateTime.UtcNow.AddDays(-1);

            ActivityLog[] commonActivityLogs =
            {
                _activityLogService.CreateUserCreatedLog(orle.Id),
                _activityLogService.CreateUserDeletedLog(orle.Id),
                _activityLogService.CreateUserRenamedLog(orle.Id, "Salt", "Duke Salt of Savoy"),
                activityLogUserRewarded,
                _activityLogService.CreateItemBoughtLog(orle.Id, orleItem1.ItemId, 12000),
                _activityLogService.CreateItemSoldLog(orle.Id, orleItem1.ItemId, 12000),
                _activityLogService.CreateItemBrokeLog(orle.Id, orleItem1.ItemId),
                _activityLogService.CreateItemUpgradedLog(orle.Id, orleItem1.ItemId, 2),
                _activityLogService.CreateItemReturnedLog(orle.Id, "crpg_item_1", 1, 1900),
                _activityLogService.CreateCharacterCreatedLog(orle.Id, orleCharacter0.Id),
                _activityLogService.CreateCharacterDeletedLog(orle.Id, orleCharacter0.Id, 13, 36),
                _activityLogService.CreateCharacterRespecializedLog(orle.Id, orleCharacter0.Id, 120000),
                _activityLogService.CreateCharacterRetiredLog(orle.Id, orleCharacter0.Id, 34),
                _activityLogService.CreateCharacterRewardedLog(orle.Id, takeo.Id, 5, 1000000),
            };

            ActivityLog[] gameServerActivityLogs =
            {
                new() { Type = ActivityLogType.ServerJoined, User = orle },
                new() { Type = ActivityLogType.ChatMessageSent, User = orle, Metadata = { new("message", "Fluttershy is best"), new("instance", "crpg01a"), } },
                new() { Type = ActivityLogType.ChatMessageSent, User = orle, Metadata = { new("message", "No, Rarity the best"), new("instance", "crpg01a"), }, },
                new() { Type = ActivityLogType.ChatMessageSent, User = takeo, CreatedAt = DateTime.UtcNow.AddMinutes(-3), Metadata = { new("message", "Do you get it?"), new("instance", "crpg01a"), }, },
                new() { Type = ActivityLogType.TeamHit, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(+3), Metadata = { new("targetUserId", takeo.Id.ToString()), new("damage", "123"), new("instance", "crpg01a"), }, },
                new() { Type = ActivityLogType.TeamHit, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(+6), Metadata = { new("targetUserId", namidaka.Id.ToString()), new("damage", "333"), new("instance", "crpg01a"), }, },
                new() { Type = ActivityLogType.TeamHitReported, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(+6), Metadata = { new("targetUserId", namidaka.Id.ToString()), new("reportedHits", "333"), new("decayedHits", "111"), new("unreportedHits", "222"), new("onReporterHits", "11"), new("damage", "123"), new("weaponName", "crpg_item_1"), }, },
                new() { Type = ActivityLogType.TeamHitReportedUserKicked, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(+6), Metadata = { new("reportedHits", "333"), new("decayedHits", "111"), new("unreportedHits", "222"), }, },
            };

            ActivityLog[] characterEarnedActivityLogs =
            {
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-1), Metadata = { new("characterId", orleCharacter0.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "122000"), new("gold", "1244") } },
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-12), Metadata = { new("characterId", orleCharacter0.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "7000"), new("gold", "989") } },
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-15), Metadata = { new("characterId", orleCharacter0.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "32000"), new("gold", "-900") } },
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-25), Metadata = { new("characterId", orleCharacter1.Id.ToString()), new("gameMode", "CRPGDTV"), new("experience", "32000"), new("gold", "1989") } },
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-35), Metadata = { new("characterId", orleCharacter1.Id.ToString()), new("gameMode", "CRPGDTV"), new("experience", "322000"), new("gold", "989") } },
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-11), Metadata = { new("characterId", orleCharacter0.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "1400"), new("gold", "1244") } },
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-23), Metadata = { new("characterId", orleCharacter0.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "200"), new("gold", "-12") } },
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-17), Metadata = { new("characterId", orleCharacter0.Id.ToString()), new("gameMode", "CRPGBattle"), new("experience", "993310"), new("gold", "133") } },
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-111), Metadata = { new("characterId", orleCharacter0.Id.ToString()), new("gameMode", "CRPGDTV"), new("experience", "122234"), new("gold", "-1222") } },
                new() { Type = ActivityLogType.CharacterEarned, User = orle, CreatedAt = DateTime.UtcNow.AddMinutes(-112), Metadata = { new("characterId", orleCharacter0.Id.ToString()), new("gameMode", "CRPGDTV"), new("experience", "3111"), new("gold", "-122") } },
            };

            ActivityLog[] clanActivityLogs =
            {
                _activityLogService.CreateClanApplicationCreatedLog(takeo.Id, 1),
                _activityLogService.CreateClanApplicationCreatedLog(namidaka.Id, 1),
                _activityLogService.CreateClanApplicationCreatedLog(orle.Id, 1),
                _activityLogService.CreateClanApplicationAcceptedLog(orle.Id, 1),
                _activityLogService.CreateClanApplicationDeclinedLog(orle.Id, 1),
                _activityLogService.CreateClanMemberRoleChangeLog(orle.Id, 1, takeo.Id, ClanMemberRole.Officer, ClanMemberRole.Leader),
                _activityLogService.CreateClanMemberLeavedLog(orle.Id, 1),
                _activityLogService.CreateClanMemberKickedLog(orle.Id, 1, takeo.Id),
                _activityLogService.CreateClanCreatedLog(orle.Id, 1),
                _activityLogService.CreateClanDeletedLog(orle.Id, 1),
                _activityLogService.CreateAddItemToClanArmoryLog(takeo.Id, pecores.Id, takeoItem1.Id),
                _activityLogService.CreateRemoveItemFromClanArmoryLog(takeo.Id, pecores.Id, takeoItem1.Id),
                _activityLogService.CreateReturnItemToClanArmoryLog(takeo.Id, pecores.Id, orleItem1.Id),
                _activityLogService.CreateBorrowItemFromClanArmoryLog(takeo.Id, pecores.Id, orleItem1.Id),
            };

            _db.ActivityLogs.RemoveRange(await _db.ActivityLogs.ToArrayAsync());
            _db.ActivityLogs.AddRange(
                            commonActivityLogs
                    .Concat(gameServerActivityLogs)
                    .Concat(characterEarnedActivityLogs)
                    .Concat(clanActivityLogs));

            UserNotification[] orleNotifications =
            {
                _userNotificationService.CreateUserRewardedToUserNotification(orle.Id, 100, 1, orleItem1.ItemId),
                _userNotificationService.CreateCharacterRewardedToUserNotification(orle.Id, orleCharacter0.Id, 122211),
                _userNotificationService.CreateItemReturnedToUserNotification(orle.Id, orleItem1.ItemId, 2, 1222),
                _userNotificationService.CreateClanApplicationCreatedToOfficersNotification(orle.Id, pecores.Id, takeo.Id),
                _userNotificationService.CreateClanApplicationCreatedToUserNotification(orle.Id, pecores.Id),
                _userNotificationService.CreateClanApplicationAcceptedToUserNotification(orle.Id, pecores.Id),
                _userNotificationService.CreateClanApplicationDeclinedToUserNotification(orle.Id, pecores.Id),
                _userNotificationService.CreateClanMemberRoleChangedToUserNotification(orle.Id, pecores.Id, takeo.Id, ClanMemberRole.Officer, ClanMemberRole.Leader),
                _userNotificationService.CreateClanMemberLeavedToLeaderNotification(orle.Id, pecores.Id, takeo.Id),
                _userNotificationService.CreateClanMemberKickedToExMemberNotification(orle.Id, pecores.Id),
                _userNotificationService.CreateClanArmoryBorrowItemToLenderNotification(orle.Id, pecores.Id, orleItem1.ItemId, takeo.Id),
                _userNotificationService.CreateClanArmoryRemoveItemToBorrowerNotification(orle.Id, pecores.Id, takeoItem1.ItemId, takeo.Id),
            };

            _db.UserNotifications.RemoveRange(await _db.UserNotifications.ToArrayAsync());
            _db.UserNotifications.AddRange(orleNotifications);

            ClanInvitation[] newClanInvitations = { schumetzqRequestForPecores, victorhh888MemberRequestForPecores, neostralieOfferToBrygganForPecores };

            var existingClanInvitations = await _db.ClanInvitations.ToDictionaryAsync(i => (i.InviteeId, i.InviterId));
            foreach (var newClanInvitation in newClanInvitations)
            {
                if (!existingClanInvitations.ContainsKey((newClanInvitation.Invitee!.Id, newClanInvitation.Inviter!.Id)))
                {
                    _db.ClanInvitations.Add(newClanInvitation);
                }
            }

            Task<Settlement> GetSettlementByName(string name) =>
                _db.Settlements.FirstAsync(s => s.Name == name && s.Region == Region.Eu);

            var epicrotea = await GetSettlementByName("Epicrotea");
            var mecalovea = await GetSettlementByName("Mecalovea");
            var marathea = await GetSettlementByName("Marathea");
            var stathymos = await GetSettlementByName("Stathymos");
            var gersegosCastle = await GetSettlementByName("Gersegos Castle");
            var dyopalis = await GetSettlementByName("Dyopalis");
            var rhesosCastle = await GetSettlementByName("Rhesos Castle");
            var potamis = await GetSettlementByName("Potamis");
            var carphenion = await GetSettlementByName("Carphenion");
            var ataconiaCastle = await GetSettlementByName("Ataconia Castle");
            var ataconia = await GetSettlementByName("Ataconia");
            var elipa = await GetSettlementByName("Elipa");
            var rhotae = await GetSettlementByName("Rhotae");
            var hertogeaCastle = await GetSettlementByName("Hertogea Castle");
            var hertogea = await GetSettlementByName("Hertogea");
            var nideon = await GetSettlementByName("Nideon");
            var leblenion = await GetSettlementByName("Leblenion");
            var rhemtoil = await GetSettlementByName("Rhemtoil");

            Party orleParty = new()
            {
                User = orle,
                Troops = 1,
                // Troops = 100,
                // Position = epicrotea.Position,
                Position = rhotae.Position,
                // Position = new Point(114.21076699552688, -109.37351870100285),
                Status = PartyStatus.IdleInSettlement,
                // TargetedSettlement = epicrotea,
                TargetedSettlement = rhotae,
            };
            Party brainfartParty = new()
            {
                User = brainfart,
                Troops = 1000,
                Position = new Point(112, -88),
                Status = PartyStatus.Idle,
            };
            Party kiwiParty = new()
            {
                User = kiwi,
                Troops = 1,
                Position = new Point(142, -90),
                Status = PartyStatus.Idle,
            };
            Party ikaroozParty = new()
            {
                User = ikarooz,
                Troops = 20,
                Position = new Point(130, -102),
                Status = PartyStatus.Idle,
            };
            Party laHireParty = new()
            {
                User = laHire,
                Troops = 20,
                Position = new Point(135, -97),
                Status = PartyStatus.Idle,
            };
            Party brygganParty = new()
            {
                User = bryggan,
                Troops = 1,
                Position = new Point(131, -102),
                Status = PartyStatus.Idle,
            };
            Party elmarykParty = new()
            {
                User = elmaryk,
                Troops = 6,
                Position = new Point(108, -98),
                Status = PartyStatus.Idle,
            };
            Party schumetzqParty = new()
            {
                User = schumetzq,
                Troops = 7,
                Position = new Point(119, -105),
                Status = PartyStatus.Idle,
            };
            Party azumaParty = new()
            {
                User = azuma,
                Troops = 121,
                Position = new Point(106, -112),
                Status = PartyStatus.Idle,
            };
            Party zorguyParty = new()
            {
                User = zorguy,
                Troops = 98,
                Position = new Point(114, -114),
                Status = PartyStatus.Idle,
            };
            Party eckoParty = new()
            {
                User = ecko,
                Troops = 55,
                Position = new Point(117, -112),
                Status = PartyStatus.Idle,
            };
            Party firebatParty = new()
            {
                User = firebat,
                Troops = 29,
                Position = new Point(105, -111),
                Status = PartyStatus.Idle,
            };
            Party laenirParty = new()
            {
                User = leanir,
                Troops = 1,
                Position = new Point(103, -102),
                Status = PartyStatus.Idle,
            };
            Party opsetParty = new()
            {
                User = opset,
                Troops = 1,
                Position = new Point(113, -112),
                Status = PartyStatus.Idle,
            };
            Party falcomParty = new()
            {
                User = falcom,
                Troops = 4,
                Position = epicrotea.Position,
                Status = PartyStatus.IdleInSettlement,
                TargetedSettlement = epicrotea,
            };
            Party victorhh888Party = new()
            {
                User = victorhh888,
                Troops = 9,
                Position = epicrotea.Position,
                Status = PartyStatus.RecruitingInSettlement,
            };
            Party sellkaParty = new()
            {
                User = sellka,
                Troops = 3,
                Position = dyopalis.Position,
                Status = PartyStatus.RecruitingInSettlement,
                TargetedSettlement = dyopalis,
            };
            Party distanceParty = new()
            {
                User = distance,
                Troops = 1,
                Position = rhotae.Position,
                Status = PartyStatus.RecruitingInSettlement,
                TargetedSettlement = rhotae,
            };
            Party bakhratParty = new()
            {
                User = bakhrat,
                Troops = 120,
                Position = rhotae.Position,
                Status = PartyStatus.RecruitingInSettlement,
                TargetedSettlement = rhotae,
            };
            Party lancelotParty = new()
            {
                User = lancelot,
                Troops = 243,
                Position = rhotae.Position,
                Status = PartyStatus.Idle,
                TargetedSettlement = rhotae,
            };
            Party buddhaParty = new()
            {
                User = buddha,
                Troops = 49,
                Position = nideon.Position,
                Status = PartyStatus.IdleInSettlement,
                TargetedSettlement = rhotae,
            };
            Party lerchParty = new()
            {
                User = lerch,
                Troops = 10,
                Position = new Point(107, -102),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = rhotae,
            };
            Party tjensParty = new()
            {
                User = tjens,
                Troops = 500,
                Position = new Point(112, -93),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = rhotae,
            };
            Party knitlerParty = new()
            {
                User = knitler,
                Troops = 3,
                Position = new Point(124, -102),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = rhotae,
            };
            Party magnucleanParty = new()
            {
                User = magnuclean,
                Troops = 100,
                Position = new Point(120, -88),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = rhemtoil,
            };
            Party baronCyborgParty = new()
            {
                User = baronCyborg,
                Troops = 9,
                Position = new Point(120, -88),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = mecalovea,
            };
            Party scarfaceParty = new()
            {
                User = scarface,
                Troops = 25,
                Position = new Point(119, -105),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = hertogeaCastle,
            };
            Party neostralieParty = new()
            {
                User = neostralie,
                Troops = 1,
                Position = new Point(128, -97),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = potamis,
            };
            Party manikParty = new()
            {
                User = manik,
                Troops = 1,
                Position = new Point(129, -102),
                Status = PartyStatus.MovingToAttackParty,
                TargetedParty = neostralieParty,
            };
            Party ajroselleParty = new()
            {
                User = ajroselle,
                Troops = 1,
                Position = new Point(130, -107),
                Status = PartyStatus.MovingToAttackParty,
                TargetedParty = manikParty,
            };
            Party skraelParty = new()
            {
                User = skrael,
                Troops = 1,
                Position = new Point(126, -93),
                Status = PartyStatus.MovingToAttackParty,
                TargetedParty = neostralieParty,
            };
            Party bedoParty = new()
            {
                User = bedo,
                Troops = 300,
                Position = new Point(114, -101),
                Status = PartyStatus.MovingToAttackSettlement,
                TargetedSettlement = gersegosCastle,
            };
            Party lambicParty = new()
            {
                User = lambic,
                Troops = 87,
                Position = new Point(113, -98),
                Status = PartyStatus.MovingToAttackSettlement,
                TargetedSettlement = gersegosCastle,
            };
            Party sanasarParty = new()
            {
                User = sanasar,
                Troops = 21,
                Position = new Point(119, -101),
                Status = PartyStatus.MovingToAttackSettlement,
                TargetedSettlement = rhotae,
            };
            Party vlad007Party = new()
            {
                User = vlad007,
                Troops = 21,
                Position = new Point(119, -101),
                Status = PartyStatus.MovingToAttackSettlement,
                TargetedSettlement = rhotae,
            };
            Party canp0GParty = new()
            {
                User = canp0g,
                Troops = 1,
                Position = rhesosCastle.Position,
                Status = PartyStatus.MovingToPoint,
                Waypoints = new MultiPoint(new[] { new Point(125, -97) }),
            };
            Party sharkParty = new()
            {
                User = shark,
                Troops = 1,
                Position = new Point(105, -107),
                Status = PartyStatus.MovingToPoint,
                Waypoints = new MultiPoint(new[] { new Point(121, -99) }),
            };
            Party noobAmphetamineParty = new()
            {
                User = noobAmphetamine,
                Troops = 1,
                Position = new Point(107, -100),
                Status = PartyStatus.MovingToPoint,
                Waypoints = new MultiPoint(new[] { new Point(112, -88) }),
            };
            Party mundeteParty = new()
            {
                User = mundete,
                Troops = 1,
                Position = new Point(112, -99),
                Status = PartyStatus.FollowingParty,
                TargetedParty = sharkParty,
            };
            Party aroyFalconerParty = new()
            {
                User = aroyFalconer,
                Troops = 1,
                Position = new Point(123, -88),
                Status = PartyStatus.MovingToPoint,
                Waypoints = new MultiPoint(new[] { new Point(135, -98) }),
            };
            Party insanitoidParty = new()
            {
                User = insanitoid,
                Troops = 1,
                Position = new Point(135, -98),
                Status = PartyStatus.MovingToPoint,
                Waypoints = new MultiPoint(new[] { new Point(123, -88) }),
            };
            Party namidakaParty = new()
            {
                User = namidaka,
                Troops = 11,
                Position = new Point(135, -99),
                Status = PartyStatus.FollowingParty,
                TargetedParty = insanitoidParty,
            };
            Party xDemParty = new()
            {
                User = xDem,
                Troops = 250,
                Position = new Point(nideon.Position.X - 0.2, nideon.Position.Y - 0.2),
                Status = PartyStatus.InBattle,
                TargetedSettlement = nideon,
            };
            Party disorotParty = new()
            {
                User = disorot,
                Troops = 89,
                Position = new Point(nideon.Position.X + 0.2, nideon.Position.Y + 0.2),
                Status = PartyStatus.InBattle,
            };
            Party aceParty = new()
            {
                User = ace,
                Troops = 104,
                Position = new Point(nideon.Position.X - 0.2, nideon.Position.Y + 0.2),
                Status = PartyStatus.InBattle,
            };
            Party sagarParty = new()
            {
                User = sagar,
                Troops = 300,
                Position = new Point(nideon.Position.X + 0.2, nideon.Position.Y - 0.2),
                Status = PartyStatus.Idle,
            };
            Party greenShadowParty = new()
            {
                User = greenShadow,
                Troops = 31,
                Position = new Point(106.986, -110.171),
                Status = PartyStatus.InBattle,
            };
            Party hannibaruParty = new()
            {
                User = hannibaru,
                Troops = 42,
                Position = new Point(107.109, -110.328),
                Status = PartyStatus.InBattle,
            };
            Party drexxParty = new()
            {
                User = drexx,
                Troops = 53,
                Position = new Point(107.304, -110.203),
                Status = PartyStatus.InBattle,
            };
            Party xaroshParty = new()
            {
                User = xarosh,
                Troops = 64,
                Position = new Point(107.210, -110.062),
                Status = PartyStatus.InBattle,
            };
            Party tipsyTobyParty = new()
            {
                User = tipsyToby,
                Troops = 75,
                Position = new Point(107.304, -110.046),
                Status = PartyStatus.Idle,
            };
            Party localAlphaParty = new()
            {
                User = localAlpha,
                Troops = 75,
                Position = new Point(107.304, -110.046),
                Status = PartyStatus.Idle,
            };
            Party alexParty = new()
            {
                User = alex,
                Troops = 86,
                Position = new Point(107, -106),
                Status = PartyStatus.InBattle,
                TargetedSettlement = hertogea,
            };
            Party kedrynFuelParty = new()
            {
                User = kedrynFuel,
                Troops = 97,
                Position = new Point(107, -106.2),
                Status = PartyStatus.FollowingParty,
                TargetedParty = alexParty,
            };
            Party luqeroParty = new()
            {
                User = luqero,
                Troops = 108,
                Position = hertogea.Position,
                Status = PartyStatus.IdleInSettlement,
                TargetedSettlement = hertogea,
            };
            Party ilyaParty = new()
            {
                User = ilya,
                Troops = 119,
                Position = hertogea.Position,
                Status = PartyStatus.IdleInSettlement,
                TargetedSettlement = hertogea,
            };
            Party eztliParty = new()
            {
                User = eztli,
                Troops = 86,
                Position = leblenion.Position,
                Status = PartyStatus.InBattle,
                TargetedSettlement = leblenion,
            };

            // Users with no party: telesto, kypak, devoidDragon.

            Party[] newParties =
            {
                orleParty, brainfartParty, kiwiParty, ikaroozParty, laHireParty, brygganParty, elmarykParty, schumetzqParty,
                azumaParty, zorguyParty, eckoParty, firebatParty, laenirParty, opsetParty, falcomParty,
                victorhh888Party, sellkaParty, distanceParty, bakhratParty, lancelotParty, buddhaParty, lerchParty,
                tjensParty, knitlerParty, magnucleanParty, baronCyborgParty, scarfaceParty, neostralieParty,
                manikParty, ajroselleParty, skraelParty, bedoParty, lambicParty, sanasarParty, vlad007Party,
                canp0GParty, sharkParty, noobAmphetamineParty, mundeteParty, aroyFalconerParty, insanitoidParty,
                namidakaParty, xDemParty, disorotParty, aceParty, sagarParty, greenShadowParty, hannibaruParty,
                drexxParty, xaroshParty, tipsyTobyParty, localAlphaParty, eztliParty,
            };

            var existingParties = (await _db.Parties.ToArrayAsync())
                .Select(u => u.Id)
                .ToHashSet();
            foreach (var newParty in newParties)
            {
                if (!existingParties.Contains(newParty.User!.Id))
                {
                    _db.Parties.Add(newParty);
                }
            }

            Battle nideonBattle = new()
            {
                Phase = BattlePhase.Preparation,
                Region = Region.Eu,
                Position = nideon.Position,
                Fighters =
                {
                    new BattleFighter
                    {
                        Party = xDemParty,
                        Side = BattleSide.Attacker,
                        Commander = true,
                    },
                    new BattleFighter
                    {
                        Party = disorotParty,
                        Side = BattleSide.Attacker,
                        Commander = false,
                    },
                    new BattleFighter
                    {
                        Party = null,
                        Settlement = nideon,
                        Side = BattleSide.Defender,
                        Commander = true,
                    },
                    new BattleFighter
                    {
                        Party = aceParty,
                        Side = BattleSide.Defender,
                        Commander = false,
                    },
                },
                FighterApplications =
                {
                    new BattleFighterApplication
                    {
                        Party = sagarParty,
                        Side = BattleSide.Defender,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                },
            };
            Battle plainBattle = new()
            {
                Phase = BattlePhase.Preparation,
                Region = Region.Eu,
                Position = new Point(107.187, -110.164),
                Fighters =
                {
                    new BattleFighter { Party = xaroshParty, Side = BattleSide.Attacker, Commander = true },
                    new BattleFighter { Party = greenShadowParty, Side = BattleSide.Attacker, Commander = false },
                    new BattleFighter { Party = drexxParty, Side = BattleSide.Defender, Commander = true },
                    new BattleFighter { Party = hannibaruParty, Side = BattleSide.Defender, Commander = false },
                },
                FighterApplications =
                {
                    new BattleFighterApplication
                    {
                        Party = tipsyTobyParty,
                        Side = BattleSide.Attacker,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                    new BattleFighterApplication
                    {
                        Party = localAlphaParty,
                        Side = BattleSide.Defender,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                },
                CreatedAt = DateTime.UtcNow,
            };
            Battle hertogeaBattle = new()
            {
                Phase = BattlePhase.Hiring,
                Region = Region.Eu,
                Position = hertogea.Position,
                Fighters =
                {
                    new BattleFighter
                    {
                        Party = alexParty,
                        Side = BattleSide.Attacker,
                        Commander = true,
                    },
                    new BattleFighter
                    {
                        Party = null,
                        Settlement = hertogea,
                        Side = BattleSide.Defender,
                        Commander = true,
                    },
                },
                FighterApplications =
                {
                    new BattleFighterApplication
                    {
                        Party = kedrynFuelParty,
                        Side = BattleSide.Attacker,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                    new BattleFighterApplication
                    {
                        Party = luqeroParty,
                        Side = BattleSide.Defender,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                    new BattleFighterApplication
                    {
                        Party = ilyaParty,
                        Side = BattleSide.Defender,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                },
                CreatedAt = DateTime.UtcNow.AddHours(-2),
            };
            Battle leblenionBattle = new()
            {
                Phase = BattlePhase.Hiring,
                Region = Region.Eu,
                Position = leblenion.Position,
                Fighters =
                {
                    new BattleFighter
                    {
                        Party = eztliParty,
                        Side = BattleSide.Attacker,
                        Commander = true,
                    },
                    new BattleFighter
                    {
                        Party = null,
                        Settlement = leblenion,
                        Side = BattleSide.Defender,
                        Commander = true,
                    },
                },
                MercenaryApplications =
                {
                    new BattleMercenaryApplication
                    {
                        Character = falcomCharacter0,
                        Side = BattleSide.Attacker,
                        Status = BattleMercenaryApplicationStatus.Pending,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = victorhh888Character0,
                        Side = BattleSide.Defender,
                        Status = BattleMercenaryApplicationStatus.Pending,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = sellkaCharacter0,
                        Side = BattleSide.Defender,
                        Status = BattleMercenaryApplicationStatus.Pending,
                    },
                },
                CreatedAt = DateTime.UtcNow.AddHours(-4),
            };

            Battle[] newBattles = { nideonBattle, plainBattle, hertogeaBattle, leblenionBattle };
            if (!await _db.Battles.AnyAsync())
            {
                _db.Battles.AddRange(newBattles);
            }

            Terrain[] terrains =
            {
                new()
                {
                    Type = TerrainType.ThickForest,
                    Boundary = new Polygon(new LinearRing(new Coordinate[] { new(99.210197, -101.434375), new(100.263933, -100.115272), new(100.749973, -99.783815), new(101.136596, -99.253485), new(101.445895, -98.734204), new(101.920889, -98.557427), new(101.920889, -98.557427), new(102.561578, -98.634767), new(103.467381, -98.446942), new(104.085977, -98.38065), new(104.174348, -97.761932), new(104.59411, -97.342087), new(105.168522, -97.264747), new(105.787118, -97.640398), new(105.865278, -98.253125), new(105.732492, -98.95625), new(105.451157, -99.7625), new(104.810658, -100.004687), new(104.162348, -100.48125), new(102.95946, -100.410937), new(102.365826, -101.090625), new(101.944034, -101.309375), new(101.483188, -101.715625), new(101.194182, -101.965625), new(100.616171, -102.160937), new(100.584927, -102.746875), new(100.131891, -102.934375), new(99.460148, -102.403125), new(99.210197, -101.434375), })),
                },
                new()
                {
                    Type = TerrainType.ShallowWater,
                    Boundary = new Polygon(new LinearRing(new Coordinate[] { new(104.174348, -97.761932), new(104.130833, -98.066596), new(103.420365, -98.192822), new(102.686134, -97.974021), new(101.694142, -97.184773), new(101.819117, -97.0363), new(102.756433, -97.677076), new(103.365688, -97.91932), new(104.174348, -97.761932), })),
                },
            };

            _db.Terrains.RemoveRange(await _db.Terrains.ToArrayAsync());
            _db.Terrains.AddRange(terrains);
        }

        private async Task CreateOrUpdateItems(CancellationToken cancellationToken)
        {
            var itemsById = (await _itemsSource.LoadItems()).ToDictionary(i => i.Id);
            var dbItemsById = await _db.Items.ToDictionaryAsync(i => i.Id, cancellationToken);

            foreach (ItemCreation item in itemsById.Values)
            {
                Item itemToCreate = ItemCreationToItem(item);
                CreateOrUpdateItem(dbItemsById, itemToCreate);
            }

            // Remove items that were deleted from the item source
            foreach (Item dbItem in dbItemsById.Values)
            {
                if (itemsById.ContainsKey(dbItem.Id))
                {
                    continue;
                }

                var userItems = await _db.UserItems
                    .Include(ui => ui.User)
                    .Include(ui => ui.Item)
                    .Where(ui => ui.ItemId == dbItem.Id)
                    .ToArrayAsync(cancellationToken);
                foreach (var userItem in userItems)
                {
                    userItem.User!.Gold += userItem.Item!.Price;
                    // Trick to avoid UpdatedAt to be updated.
                    userItem.User.UpdatedAt = userItem.User.UpdatedAt;
                    if (userItem.Item!.Rank > 0)
                    {
                        userItem.User.HeirloomPoints += userItem.Item!.Rank;
                    }

                    _db.UserItems.Remove(userItem);
                    _db.ActivityLogs.Add(_activityLogService.CreateItemReturnedLog(userItem.User.Id, userItem.Item.Id, userItem.Item.Rank, userItem.Item.Price));
                    _db.UserNotifications.Add(_userNotificationService.CreateItemReturnedToUserNotification(userItem.User.Id, userItem.Item.Id, userItem.Item.Rank, userItem.Item.Price));
                }

                var itemsToDelete = dbItemsById.Values.Where(i => i.Id == dbItem.Id).ToArray();
                foreach (var i in itemsToDelete)
                {
                    _db.Entry(i).State = EntityState.Deleted;
                }
            }
        }

        private void CreateOrUpdateItem(Dictionary<string, Item> dbItemsByMbId, Item item)
        {
            if (dbItemsByMbId.TryGetValue(item.Id, out Item? dbItem))
            {
                item.Enabled = dbItem.Enabled; // Items seed should not overwrite the enabled flag.

                var dbItemEntry = _db.Entry(dbItem);
                dbItemEntry.CurrentValues.SetValues(item);
                // Explicitly modify owned entities because it seems like SetValues is not working for them.
                dbItem.Armor = item.Armor;
                dbItem.Mount = item.Mount;
                dbItem.PrimaryWeapon = item.PrimaryWeapon;
                dbItem.SecondaryWeapon = item.SecondaryWeapon;
                dbItem.TertiaryWeapon = item.TertiaryWeapon;
            }
            else
            {
                // auto disable item
                if (item.Id.StartsWith("crpg_disabled_"))
                {
                    item.Enabled = false;
                }

                _db.Items.Add(item);
            }
        }

        private Item ItemCreationToItem(ItemCreation item)
        {
            Item res = new()
            {
                Id = item.Id,
                BaseId = item.BaseId,
                Name = item.Name,
                Culture = item.Culture,
                Type = item.Type,
                Price = item.Price,
                Weight = item.Weight,
                Tier = item.Tier,
                Rank = item.Rank,
                Requirement = item.Requirement,
                Flags = item.Flags,
                Enabled = true,
            };

            if (item.Armor != null)
            {
                res.Armor = new ItemArmorComponent
                {
                    HeadArmor = item.Armor!.HeadArmor,
                    BodyArmor = item.Armor!.BodyArmor,
                    ArmArmor = item.Armor!.ArmArmor,
                    LegArmor = item.Armor!.LegArmor,
                    MaterialType = item.Armor.MaterialType,
                    FamilyType = item.Armor.FamilyType,
                };
            }

            if (item.Mount != null)
            {
                res.Mount = new ItemMountComponent
                {
                    BodyLength = item.Mount!.BodyLength,
                    ChargeDamage = item.Mount.ChargeDamage,
                    Maneuver = item.Mount.Maneuver,
                    Speed = item.Mount.Speed,
                    HitPoints = item.Mount.HitPoints,
                    FamilyType = item.Mount.FamilyType,
                };
            }

            if (item.Weapons.Count > 0)
            {
                res.PrimaryWeapon = IteamWeaponComponentFromViewModel(item.Weapons[0]);
            }

            if (item.Weapons.Count > 1)
            {
                res.SecondaryWeapon = IteamWeaponComponentFromViewModel(item.Weapons[1]);
            }

            if (item.Weapons.Count > 2)
            {
                res.TertiaryWeapon = IteamWeaponComponentFromViewModel(item.Weapons[2]);
            }

            return res;
        }

        private ItemWeaponComponent IteamWeaponComponentFromViewModel(ItemWeaponComponentViewModel weaponComponent)
        {
            return new()
            {
                Class = weaponComponent.Class,
                ItemUsage = weaponComponent.ItemUsage,
                Accuracy = weaponComponent.Accuracy,
                MissileSpeed = weaponComponent.MissileSpeed,
                StackAmount = weaponComponent.StackAmount,
                Length = weaponComponent.Length,
                Balance = weaponComponent.Balance,
                Handling = weaponComponent.Handling,
                BodyArmor = weaponComponent.BodyArmor,
                Flags = weaponComponent.Flags,
                ThrustDamage = weaponComponent.ThrustDamage,
                ThrustDamageType = weaponComponent.ThrustDamageType,
                ThrustSpeed = weaponComponent.ThrustSpeed,
                SwingDamage = weaponComponent.SwingDamage,
                SwingDamageType = weaponComponent.SwingDamageType,
                SwingSpeed = weaponComponent.SwingSpeed,
            };
        }

        private async Task CreateOrUpdateSettlements(CancellationToken cancellationToken)
        {
            var settlementsByName = (await _settlementsSource.LoadStrategusSettlements())
                .ToDictionary(i => i.Name);
            var dbSettlementsByNameRegion = await _db.Settlements
                .ToDictionaryAsync(di => (di.Name, di.Region), cancellationToken);

            foreach (var settlementCreation in settlementsByName.Values)
            {
                foreach (var region in GetRegions())
                {
                    // TODO: if AS and OC share the same map the settlements should be shared equally.
                    if (region == Region.Oc)
                    {
                        continue;
                    }

                    Settlement settlement = new()
                    {
                        Name = settlementCreation.Name,
                        Type = settlementCreation.Type,
                        Culture = settlementCreation.Culture,
                        Region = region,
                        Position = _strategusMap.TranslatePositionForRegion(settlementCreation.Position, Region.Eu, region),
                        Scene = settlementCreation.Scene,
                        Troops = StrategusSettlementDefaultTroops[settlementCreation.Type],
                        OwnerId = null,
                    };

                    // TODO: hack FIXME: only in dev START
                    // if (_appEnv.Environment == HostingEnvironment.Development)
                    // {
                    //     if (settlement.Name == "Rhotae")
                    //     {
                    //         SettlementItem testitem1 = new() { ItemId = "crpg_14_decor_paltedboots_noble1_v1_h0", Count = 10 };
                    //         var rhotaeItems = new List<SettlementItem>
                    //     {
                    //         testitem1,
                    //     };
                    //         settlement.OwnerId = 2;
                    //         settlement.Items = rhotaeItems;
                    //     }

                    //     if (settlement.Name == "Thersenion")
                    //     {
                    //         settlement.OwnerId = 2;
                    //     }

                    //     // TODO: hack FIXME: only in dev END
                    // }

                    if (dbSettlementsByNameRegion.TryGetValue((settlement.Name, settlement.Region), out Settlement? dbSettlement))
                    {
                        _db.Entry(dbSettlement).State = EntityState.Detached;

                        settlement.Id = dbSettlement.Id;
                        _db.Settlements.Update(settlement);
                    }
                    else
                    {
                        _db.Settlements.Add(settlement);
                    }
                }
            }

            foreach (var dbSettlement in dbSettlementsByNameRegion.Values)
            {
                if (!settlementsByName.ContainsKey(dbSettlement.Name))
                {
                    _db.Settlements.Remove(dbSettlement);
                }
            }
        }

        private IEnumerable<Region> GetRegions() => Enum.GetValues(typeof(Region)).Cast<Region>();
    }
}
