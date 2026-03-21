namespace Crpg.Module.Common.HotConstants;

/// <summary>
/// Hot constants for AI combat properties. Default values are the formulas evaluated at meleeLevel=0.
/// </summary>
internal static class CrpgAiHotConstants
{
    public static readonly HotConstant AIBlockOnDecideAbility =
        HotConstant.Create(0, 0.2f, nameof(AIBlockOnDecideAbility));

    public static readonly HotConstant AIParryOnDecideAbility =
        HotConstant.Create(1, 0.2f, nameof(AIParryOnDecideAbility));

    public static readonly HotConstant AIParryOnAttackAbility =
        HotConstant.Create(2, 0f, nameof(AIParryOnAttackAbility));

    public static readonly HotConstant AIParryOnAttackingContinueAbility =
        HotConstant.Create(3, 0.2f, nameof(AIParryOnAttackingContinueAbility));

    public static readonly HotConstant AiParryDecisionChangeValue =
        HotConstant.Create(4, 0.05f, nameof(AiParryDecisionChangeValue));

    public static readonly HotConstant AIRealizeBlockingFromIncorrectSideAbility =
        HotConstant.Create(5, 0f, nameof(AIRealizeBlockingFromIncorrectSideAbility));

    public static readonly HotConstant AiRandomizedDefendDirectionChance =
        HotConstant.Create(6, 1f, nameof(AiRandomizedDefendDirectionChance));

    public static readonly HotConstant AiDefendWithShieldDecisionChanceValue =
        HotConstant.Create(7, 0.5f, nameof(AiDefendWithShieldDecisionChanceValue));
}
