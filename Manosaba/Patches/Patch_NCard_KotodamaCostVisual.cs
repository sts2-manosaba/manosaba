using Godot;
using HarmonyLib;
using Manosaba.Characters.Common.Resources;
using manosaba.Characters.NatsumeAnan.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models.Exceptions;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Manosaba.Patches;

[HarmonyPatch(typeof(NCard), "Reload")]
public static class Patch_NCard_KotodamaCostVisual
{
    private const string DefaultStarTexturePath = "res://images/ui/combat/energy_star.png";
    private const string KotodamaTexturePath = "res://Manosaba/images/characters/natsume_anan/kotodama_energy.png";

    private static Texture2D? _defaultStarTexture;
    private static Texture2D? _kotodamaTexture;

    [HarmonyPostfix]
    private static void Reload_Postfix(NCard __instance)
    {
        ApplyCustomCostVisual(__instance);
    }

    [HarmonyPatch(typeof(NCard), "UpdateStarCostVisuals")]
    [HarmonyPostfix]
    private static void UpdateStarCostVisuals_Postfix(NCard __instance)
    {
        ApplyCustomCostVisual(__instance);
    }

    private static void ApplyCustomCostVisual(NCard __instance)
    {
        TextureRect? starIcon = __instance.GetNodeOrNull<TextureRect>("%StarIcon");
        Label? starLabel = __instance.GetNodeOrNull<Label>("%StarLabel");
        if (starIcon == null || starLabel == null)
        {
            return;
        }

        _defaultStarTexture ??= ResourceLoader.Load<Texture2D>(DefaultStarTexturePath);
        _kotodamaTexture ??= ResourceLoader.Load<Texture2D>(KotodamaTexturePath);

        if (_defaultStarTexture != null)
        {
            starIcon.Texture = _defaultStarTexture;
        }

        if (__instance.Model is not ICustomEnergyCostCard customEnergyCard)
        {
            return;
        }

        if (__instance.Model.HasStarCostX || __instance.Model.GetStarCostWithModifiers() > 0)
        {
            return;
        }

        if (__instance.Model is Urusai or BookOfGreatOldOnes)
        {
            if (_kotodamaTexture != null)
            {
                starIcon.Texture = _kotodamaTexture;
            }

            starIcon.Visible = true;
            starLabel.Text = "X";
            starLabel.AddThemeColorOverride("font_color", StsColors.cream);
            starLabel.AddThemeColorOverride("font_outline_color", StsColors.defaultStarCostOutline);
            return;
        }

        int cost = customEnergyCard.GetCustomEnergyCostForPlay();
        if (cost <= 0)
        {
            return;
        }

        if (_kotodamaTexture != null)
        {
            starIcon.Texture = _kotodamaTexture;
        }

        starIcon.Visible = true;
        starLabel.Text = cost.ToString();

        bool hasEnoughEnergy = true;
        try
        {
            hasEnoughEnergy = customEnergyCard.HasEnoughCustomEnergyForPlay();
        }
        catch (CanonicalModelException)
        {
            // Preview/library flows can bind canonical cards that have no mutable Owner.
            // In those contexts we still show the custom-energy cost icon/number, but skip affordability tinting.
            hasEnoughEnergy = true;
        }
        catch (NullReferenceException)
        {
            // Some inspect flows instantiate cards without a fully initialized player/character context.
            hasEnoughEnergy = true;
        }

        if (!hasEnoughEnergy)
        {
            starLabel.AddThemeColorOverride("font_color", StsColors.red);
            starLabel.AddThemeColorOverride("font_outline_color", StsColors.unplayableEnergyCostOutline);
        }
        else
        {
            starLabel.AddThemeColorOverride("font_color", StsColors.cream);
            starLabel.AddThemeColorOverride("font_outline_color", StsColors.defaultStarCostOutline);
        }
    }
}
