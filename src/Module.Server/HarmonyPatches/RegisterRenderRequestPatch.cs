using TaleWorlds.Engine;

namespace Crpg.Module.HarmonyPatches;

/// <summary>
/// Overrides thumbnail render resolution from the default 256x240 to 512x240
/// for higher quality item exports.
/// </summary>
public class RegisterRenderRequestPatch
{
    public static void Prefix(ref ThumbnailRenderRequest request)
    {
        // Only override resolution for requests that specify a size
        // (CreateWithTexture requests leave Width/Height at 0).
        if (request.Width > 0)
        {
            request.Width = 512;
            request.Height = 240;
        }
    }
}
