using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Orbs;

namespace Manosaba.Patches
{
    [HarmonyPatch(typeof(NOrb), "Flash")]
    public static class Patch_NOrb_Flash_DisposedGuard
    {
        private static readonly AccessTools.FieldRef<NOrb, CpuParticles2D?> FlashParticleRef =
            AccessTools.FieldRefAccess<NOrb, CpuParticles2D?>("_flashParticle");

        [HarmonyPrefix]
        private static bool Prefix(NOrb __instance)
        {
            CpuParticles2D? flashParticle = FlashParticleRef(__instance);
            if (flashParticle == null || !GodotObject.IsInstanceValid(flashParticle))
            {
                return false;
            }

            flashParticle.Emitting = true;
            return false;
        }
    }
}
