using Manosaba.Extensions;
using manosaba.Characters.ShitoAlisa;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.ShitoAlisa.Cards;

/// <summary>亞里莎卡牌基底：內建燃燒變數（0 = 未啟用）。</summary>
public abstract class ShitoAlisaCardModel : PathCustomCardModel
{
    protected ShitoAlisaCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected static IEnumerable<DynamicVar> WithCombust(decimal combustMax, params DynamicVar[] vars)
    {
        yield return new ShitoCombustDynamicVar(combustMax);
        foreach (DynamicVar v in vars)
            yield return v;
    }

    // Combust countdown is handled by a global draw hook patch so any card
    // that receives Combust (not only this subclass) will tick correctly.
}
