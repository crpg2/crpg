using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;

/// <summary>
/// Fix for banners not falling to the ground and not being focusable/pickable on the client.
/// https://forums.taleworlds.com/index.php?threads/dropped-banner-items-are-non-functional-on-multiplayer-clients-no-falling-no-interaction.469898/
/// </summary>
[HarmonyPatch]
internal static class BannerPhysicsPatch
{
    private static readonly AccessTools.FieldRef<SpawnedItemEntity, Vec3> FakeSimVelRef =
        AccessTools.FieldRefAccess<SpawnedItemEntity, Vec3>("_fakeSimulationVelocity");

    private static readonly AccessTools.FieldRef<SpawnedItemEntity, bool> PhysicsStoppedRef =
        AccessTools.FieldRefAccess<SpawnedItemEntity, bool>("_physicsStopped");

    private static readonly AccessTools.FieldRef<SpawnedItemEntity, GameEntity> OwnerEntityRef =
        AccessTools.FieldRefAccess<SpawnedItemEntity, GameEntity>("_ownerGameEntity");

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SpawnedItemEntity), "OnTickParallel2")]
    public static void Postfix_ClientBannerPhysics(SpawnedItemEntity __instance, float dt)
    {
        if (!GameNetwork.IsClientOrReplay)
        {
            return;
        }

        if (PhysicsStoppedRef(__instance))
        {
            return;
        }

        MissionWeapon weapon = __instance.WeaponCopy;
        if (weapon.IsEmpty || !weapon.IsBanner())
        {
            return;
        }

        GameEntity entity = OwnerEntityRef(__instance);
        if (entity == null)
        {
            return;
        }

        ref Vec3 vel = ref FakeSimVelRef(__instance);
        MatrixFrame frame = entity.GetGlobalFrame();
        vel.z -= dt * 9.8f;
        frame.origin += vel * dt;
        entity.SetGlobalFrame(in frame);

        float groundHeight = entity.Scene.GetGroundHeightAtPosition(frame.origin);
        if (groundHeight > frame.origin.z + 0.3f)
        {
            PhysicsStoppedRef(__instance) = true;
            entity.ConvertDynamicBodyToRayCast();
        }
    }
}
