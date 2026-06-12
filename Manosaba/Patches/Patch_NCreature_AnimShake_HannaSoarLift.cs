using System;
using Godot;
using HarmonyLib;
using Manosaba.Characters.TonoHanna.Visuals;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Manosaba.Patches;

/// <summary>
/// Debuff application calls <see cref="NCreature.AnimShake"/>, which resets <see cref="NCreatureVisuals.Position"/>
/// to <see cref="Vector2.Zero"/> and wipes Hanna's soar lift offset.
/// </summary>
[HarmonyPatch(typeof(NCreature), nameof(NCreature.AnimShake))]
internal static class Patch_NCreature_AnimShake_HannaSoarLift
{
    private static readonly AccessTools.FieldRef<NCreature, Tween?> ShakeTweenRef =
        AccessTools.FieldRefAccess<NCreature, Tween?>("_shakeTween");

    [HarmonyPrefix]
    private static bool Prefix(NCreature __instance)
    {
        if (!HannaSoarLiftVisual.TryGetLiftedY(__instance.Entity, out float liftedY))
            return true;

        NCreatureVisuals visuals = __instance.Visuals;
        Tween? shakeTween = ShakeTweenRef(__instance);
        if ((shakeTween != null && shakeTween.IsRunning()) || visuals.IsPlayingHurtAnimation())
            return false;

        visuals.Position = new Vector2(0f, liftedY);
        Tween newTween = __instance.CreateTween();
        ShakeTweenRef(__instance) = newTween;
        newTween.TweenMethod(
                Callable.From((float t) =>
                {
                    visuals.Position = new Vector2(
                        10f * Mathf.Sin(t * 4f) * Mathf.Sin(t / 2f),
                        liftedY);
                }),
                0f,
                (float)Math.PI * 2f,
                1.0)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        return false;
    }
}
