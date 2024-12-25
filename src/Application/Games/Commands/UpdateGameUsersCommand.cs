using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Servers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Games.Commands;

/// <summary>
/// Give gold experience and break items of game users.
/// </summary>
public record UpdateGameUsersCommand : IMediatorRequest<UpdateGameUsersResult>
{
    public IList<GameUserUpdate> Updates { get; init; } = Array.Empty<GameUserUpdate>();
    public string Instance { get; init; } = string.Empty;

    internal class Handler : IMediatorRequestHandler<UpdateGameUsersCommand, UpdateGameUsersResult>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateGameUsersCommand>();
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICharacterService _characterService;
        private readonly IActivityLogService _activityLogService;
        private readonly IGameModeService _gameModeService;

        public Handler(ICrpgDbContext db, IMapper mapper, ICharacterService characterService, IActivityLogService activityLogService, IGameModeService gameModeService)
        {
            _db = db;
            _mapper = mapper;
            _characterService = characterService;
            _activityLogService = activityLogService;
            _gameModeService = gameModeService;
        }

        public async Task<Result<UpdateGameUsersResult>> Handle(UpdateGameUsersCommand req,
            CancellationToken cancellationToken)
        {
            List<UpdateGameUserResult> updateResults = new(req.Updates.Count);
            GameMode updateGameMode = _gameModeService.GameModeByInstanceAlias(
                    Enum.TryParse(req.Instance[^1..], ignoreCase: true, out GameModeAlias instanceAlias)
                        ? instanceAlias
                        : GameModeAlias.Z);
            {
                foreach (var update in req.Updates)
                {
                    var character = await _db.Characters
                        .Include(c => c.User!.ClanMembership)
                        .Include(ei => ei.EquippedItems)
                        .FirstOrDefaultAsync(c => c.Id == update.CharacterId, cancellationToken);

                    if (character == null)
                    {
                        Logger.LogWarning("Character with id '{0}' doesn't exist", update.CharacterId);
                        continue;
                    }

                    character!.Experience += 1;

                    try
                    {
                        var reward = GiveReward(character, update.Reward);
                        UpdateStatistics(updateGameMode, character, update.Statistics);
                        _characterService.UpdateRating(
                            character,
                            updateGameMode,
                            update.Statistics.Rating.Value,
                            update.Statistics.Rating.Deviation,
                            update.Statistics.Rating.Volatility,
                            isGameUserUpdate: true);

                        var brokenItems = await RepairOrBreakItems(character, update.BrokenItems, cancellationToken);

                        if (reward.Experience != 0)
                        {
                            int totalRepairCost = brokenItems.Sum(item => item.RepairCost);
                            _db.ActivityLogs.Add(_activityLogService.CreateCharacterEarnedLog(
                                character.UserId,
                                character.Id,
                                updateGameMode,
                                reward.Experience,
                                reward.Gold - totalRepairCost));
                        }

                        await _db.SaveChangesAsync(cancellationToken);

                        var gameUserViewModel = _mapper.Map<GameUserViewModel>(character.User!);

                        var relevantStatistic = character.Statistics.FirstOrDefault(s => s.GameMode == updateGameMode);
                        if (relevantStatistic != null)
                        {
                            gameUserViewModel.Character.Statistics = _mapper.Map<CharacterStatisticsViewModel>(relevantStatistic);
                        }

                        updateResults.Add(new UpdateGameUserResult
                        {
                            User = gameUserViewModel,
                            EffectiveReward = reward,
                            RepairedItems = brokenItems,
                        });
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        var entry = ex.Entries.FirstOrDefault();
                        if (entry != null)
                        {
                            await entry.ReloadAsync();
                        }

                        var gameUserViewModel = _mapper.Map<GameUserViewModel>(character.User!);

                        var relevantStatistic = character.Statistics.FirstOrDefault(s => s.GameMode == updateGameMode);
                        if (relevantStatistic != null)
                        {
                            gameUserViewModel.Character.Statistics = _mapper.Map<CharacterStatisticsViewModel>(relevantStatistic);
                        }

                        Logger.LogError(ex, "Concurrency error while saving updates for CharacterId: {0}", update.CharacterId);
                        updateResults.Add(new UpdateGameUserResult
                        {
                            User = gameUserViewModel,
                        });
                    }
                }
            }

            return new(new UpdateGameUsersResult
            {
                UpdateResults = updateResults,
            });
        }

        private GameUserEffectiveReward GiveReward(Character character, GameUserReward reward)
        {
            int level = character.Level;
            int experience = character.Experience;

            character.User!.Gold += reward.Gold;
            _characterService.GiveExperience(character, reward.Experience, useExperienceMultiplier: true);

            return new GameUserEffectiveReward
            {
                Experience = character.Experience - experience,
                Gold = reward.Gold,
                LevelUp = character.Level != level,
            };
        }

        private void UpdateStatistics(GameMode gameMode, Character character, CharacterStatisticsViewModel statistics)
        {
            CharacterStatistics? statisticsForGameMode = character.Statistics.FirstOrDefault(cs => cs.GameMode == gameMode);
            if (statisticsForGameMode != null)
            {
                statisticsForGameMode.Kills += statistics.Kills;
                statisticsForGameMode.Deaths += statistics.Deaths;
                statisticsForGameMode.Assists += statistics.Assists;
                statisticsForGameMode.PlayTime += statistics.PlayTime;
            }
            else
            {
                character.Statistics.Add(new CharacterStatistics
                {
                    Kills = statistics.Kills,
                    Deaths = statistics.Deaths,
                    Assists = statistics.Assists,
                    PlayTime = statistics.PlayTime,
                    GameMode = gameMode,
                });
            }
        }

        private async Task<List<GameRepairedItem>> RepairOrBreakItems(Character character,
            IList<GameUserDamagedItem> damagedItems, CancellationToken cancellationToken)
        {
            List<GameRepairedItem> repairedItems = new();
            List<int> userItemIdsToBreak = new();

            foreach (var damagedItem in damagedItems)
            {
                if (character.User!.Gold >= damagedItem.RepairCost)
                {
                    character.User.Gold -= damagedItem.RepairCost;
                    repairedItems.Add(new GameRepairedItem
                    {
                        ItemId = string.Empty,
                        RepairCost = damagedItem.RepairCost,
                        Broke = false,
                    });
                }
                else
                {
                    userItemIdsToBreak.Add(damagedItem.UserItemId);
                }
            }

            if (userItemIdsToBreak.Count == 0)
            {
                return repairedItems;
            }

            Logger.LogInformation("User '{0}' broke '{1}' items",
                character.UserId, userItemIdsToBreak.Count);

            var userItemsToBreak = await _db.UserItems
                .Where(ui => userItemIdsToBreak.Contains(ui.Id))
                .Include(ui => ui.EquippedItems)
                .ToArrayAsync(cancellationToken);
            foreach (var userItem in userItemsToBreak)
            {
                userItem.IsBroken = true;
                _db.EquippedItems.RemoveRange(userItem.EquippedItems);
                repairedItems.Add(new GameRepairedItem
                {
                    ItemId = userItem.ItemId,
                    RepairCost = 0,
                    Broke = true,
                });
                _db.ActivityLogs.Add(_activityLogService.CreateItemBrokeLog(character.UserId, userItem.ItemId));
            }

            return repairedItems;
        }
    }
}
