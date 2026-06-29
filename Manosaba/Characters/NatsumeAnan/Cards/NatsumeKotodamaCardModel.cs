using Manosaba.Characters.Common.Resources;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace manosaba.Characters.NatsumeAnan.Cards;

public abstract class NatsumeKotodamaCardModel : CharacterCustomEnergyCardModel<KotodamaEnergy>
{
    private static readonly string[] KotodamaTextMarkers = ["kotodama", "言靈", "言霊", "언령"];

    protected override KotodamaEnergy EnergyDefinition => KotodamaEnergy.Instance;

    protected NatsumeKotodamaCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override string CustomEnergyCostVarName => "KotodamaCost";

    protected int ResolveKotodamaXValue(int spentKotodama)
    {
        int xValue = Math.Max(0, spentKotodama);
        return CombatState is { } combatState
            ? Math.Max(0, Hook.ModifyXValue(combatState, this, xValue))
            : xValue;
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            List<IHoverTip> tips = base.ExtraHoverTips.ToList();
            if (ShouldShowKotodamaHoverTip())
            {
                tips.Add(new HoverTip(
                    new LocString("static_hover_tips", "MANOSABA-KOTODAMA_ENERGY.title"),
                    new LocString("static_hover_tips", "MANOSABA-KOTODAMA_ENERGY.description"),
                    KotodamaEnergy.GetHoverTipIcon()));
            }

            return tips;
        }
    }

    private bool ShouldShowKotodamaHoverTip()
    {
        if (GetCustomEnergyCost() > 0)
        {
            return true;
        }

        if (DynamicVars.Keys.Any(key => key.Contains("Kotodama", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        string? description = Description.GetRawText();
        if (string.IsNullOrEmpty(description))
        {
            return false;
        }

        return KotodamaTextMarkers.Any(marker => description.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}
