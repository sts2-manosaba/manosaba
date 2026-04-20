using Manosaba.Characters.Common.Resources;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace manosaba.Characters.NatsumeAnan.Cards;

public abstract class NatsumeKotodamaCardModel : CharacterCustomEnergyCardModel<KotodamaEnergy>
{
    protected override KotodamaEnergy EnergyDefinition => KotodamaEnergy.Instance;

    protected NatsumeKotodamaCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override string CustomEnergyCostVarName => "KotodamaCost";

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            List<IHoverTip> tips = base.ExtraHoverTips.ToList();
            int cost = GetCustomEnergyCost();
            if (cost > 0)
            {
                tips.Add(new HoverTip(
                    new LocString("static_hover_tips", "MANOSABA-KOTODAMA_ENERGY.title"),
                    new LocString("static_hover_tips", "MANOSABA-KOTODAMA_ENERGY.description")));
            }

            return tips;
        }
    }
}
