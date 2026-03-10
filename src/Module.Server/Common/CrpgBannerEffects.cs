using TaleWorlds.Core;

namespace Crpg.Module.Common;

internal class CrpgBannerEffects
{
    private static BannerEffect _none = null!;

    public static void Initialize(Game game)
    {
        _none = new BannerEffect("None");
        _none.Initialize("{=}Does nothing", "{=}No.", 0, 0, 0, EffectIncrementType.AddFactor);
        game.ObjectManager.RegisterPresumedObject(_none);
    }
}
