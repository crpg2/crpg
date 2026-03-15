namespace Crpg.Application.Common;

// To synchronize with Crpg.Module.Common.CrpgConstants.
public class Constants
{
    public int WeaponProficiencyPointsForAgility { get; set; }
    public float[] WeaponProficiencyPointsForWeaponMasterCoefs { get; set; } = [];
    public float[] WeaponProficiencyPointsForLevelCoefs { get; set; } = [];
    public float[] WeaponProficiencyCostCoefs { get; set; } = [];
    public float DefaultExperienceMultiplier { get; set; }
    public float ExperienceMultiplierByGeneration { get; set; }
    public float MaxExperienceMultiplierForGeneration { get; set; }
    public int RespecializePriceForLevel30 { get; set; }
    public float RespecializePriceHalfLife { get; set; }
    public int FreeRespecializeIntervalDays { get; set; }
    public int FreeRespecializePostWindowHours { get; set; }
    public int MinimumRetirementLevel { get; set; }
    public float BaseExperienceGainPerSecond { get; set; }
    public float MultipliedExperienceGainPerSecond { get; set; }
    public float BaseGoldGainPerSecond { get; set; }
    public float MultipliedGoldGainPerSecond { get; set; }
    public float ItemRepairCostPerSecond { get; set; }
    public float ItemBreakChance { get; set; }
    public int BrokenItemRepairPenaltySeconds { get; set; }
    public float ItemSellCostPenalty { get; set; }
    public int ItemSellGracePeriodMinutes { get; set; }
    public float[] ItemReforgeCostPerRank { get; set; } = [];
    public int MinimumLevel { get; set; }
    public int MaximumLevel { get; set; }
    public int TournamentLevel { get; set; }
    public int NewUserStartingCharacterLevel { get; set; }
    public float[] ExperienceForLevelCoefs { get; set; } = [];
    public int HighLevelCutoff { get; set; }
    public int DefaultStrength { get; set; }
    public int DefaultAgility { get; set; }
    public int DefaultHealthPoints { get; set; }
    public int DefaultGeneration { get; set; }
    public int DefaultAttributePoints { get; set; }
    public int AttributePointsPerLevel { get; set; }
    public int DefaultSkillPoints { get; set; }
    public int SkillPointsPerLevel { get; set; }
    public int HealthPointsForStrength { get; set; }
    public int HealthPointsForIronFlesh { get; set; }
    public float DamageFactorForPowerStrike { get; set; }
    public float DamageFactorForPowerDraw { get; set; }
    public float DamageFactorForPowerThrow { get; set; }
    public float[] HandlingFactorForWeaponMaster { get; set; } = [];
    public float[] DurabilityFactorForShieldRecursiveCoefs { get; set; } = [];
    public float InfantryCoverageFactorForShieldCoef { get; set; }
    public float CavalryCoverageFactorForShieldCoef { get; set; }
    public float[] MountedRangedSkillInaccuracy { get; set; } = [];
    public float[] ShieldDefendStunMultiplierForSkillRecursiveCoefs { get; set; } = [];
    public float ArmorSetRequirementPowerMeanPValue { get; set; }
    public float DefaultRating { get; set; }
    public float DefaultRatingDeviation { get; set; }
    public float DefaultRatingVolatility { get; set; }
    public int DefaultGold { get; set; }
    public int DefaultHeirloomPoints { get; set; }
    public int ClanTagMinLength { get; set; }
    public int ClanTagMaxLength { get; set; }
    public string ClanTagRegex { get; set; } = string.Empty;
    public int ClanNameMinLength { get; set; }
    public int ClanNameMaxLength { get; set; }
    public int ClanDescriptionMinLength { get; set; }
    public int ClanDescriptionMaxLength { get; set; }
    public uint ClanColorMinValue { get; set; }
    public int ClanBannerKeyMaxLength { get; set; }
    public string ClanBannerKeyRegex { get; set; } = string.Empty;
    public double CampaignMapWidth { get; set; }
    public double CampaignMapHeight { get; set; }
    public double CampaignEquivalentDistance { get; set; }
    public double CampaignInteractionDistance { get; set; }
    public double CampaignViewDistance { get; set; }
    public double[] CampaignSpawningPositionCenter { get; set; } = [];
    public double CampaignSpawningPositionRadius { get; set; }
    public float CampaignTroopRecruitmentPerHour { get; set; }
    public int CampaignMinPartyTroops { get; set; }
    public int CampaignMaxPartyTroops { get; set; }
    public int CampaignBattleInitiationDurationHours { get; set; }
    public int CampaignBattleHiringDurationHours { get; set; }
    public int CampaignMercenaryMaxWage { get; set; }
    public int CampaignMercenaryNoteMaxLength { get; set; }
    public int CampaignBattleSideBriefingNoteMaxLength { get; set; }
    public int MarketplaceOfferDurationDays { get; set; }
    public int MarketplaceActiveOfferLimit { get; set; }
    public int MarketplaceListingFeePerDay { get; set; }
    public int MarketplaceGoldFeePercent { get; set; }
}
