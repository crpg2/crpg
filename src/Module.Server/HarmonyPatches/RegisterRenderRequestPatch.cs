#if CRPG_CLIENT
using HarmonyLib;
using TaleWorlds.Engine;

namespace Crpg.Module.HarmonyPatches;

/// <summary>
/// Overrides thumbnail render resolution from the default 256x240 to 512x240
/// for higher quality item exports.
/// </summary>
[HarmonyPatch(typeof(ThumbnailCreatorView), "RegisterRenderRequest")]
public class RegisterRenderRequestPatch
{
    public static bool IsEnabled { get; set; }

    public static void Prefix(ref ThumbnailRenderRequest request)
    {
        if (!IsEnabled)
        {
            return;
        }

        // Only override resolution for requests that specify a size
        // (CreateWithTexture requests leave Width/Height at 0).
        if (request.Width > 0)
        {
            request.Width = 512;
            request.Height = 240;
        }
    }
}
#endif
