using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards;

[Pool(typeof(JogasakiNoahCardPool))]
public class ComplementaryColor : PathCustomCardModel
{
    private const int EnergyCostValue = 1;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = true;

    public ComplementaryColor() : base(EnergyCostValue, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<OrbModel>? orbList = Owner?.PlayerCombatState?.OrbQueue?.Orbs;
        if (orbList == null || orbList.Count == 0)
        {
            return;
        }

        int orbCountToRead = IsUpgraded ? 2 : 1;
        int actualCount = Math.Min(orbCountToRead, orbList.Count);
        List<OrbModel> sourceOrbs = new(actualCount);
        for (int i = 0; i < actualCount; i++)
        {
            sourceOrbs.Add(orbList[orbList.Count - 1 - i]);
        }

        foreach (OrbModel sourceOrb in sourceOrbs)
        {
            OrbModel? complementaryOrb = GetComplementaryOrb(sourceOrb);
            if (complementaryOrb == null)
            {
                continue;
            }

            await OrbCmd.Channel(choiceContext, complementaryOrb.ToMutable(), Owner);
        }
    }

    private static OrbModel? GetComplementaryOrb(OrbModel orb)
    {
        return orb switch
        {
            RedPaintOrb => ModelDb.Orb<GreenPaintOrb>(),
            GreenPaintOrb => ModelDb.Orb<RedPaintOrb>(),
            BluePaintOrb => ModelDb.Orb<OrangePaintOrb>(),
            OrangePaintOrb => ModelDb.Orb<BluePaintOrb>(),
            YellowPaintOrb => ModelDb.Orb<PurplePaintOrb>(),
            PurplePaintOrb => ModelDb.Orb<YellowPaintOrb>(),
            BlackPaintOrb => ModelDb.Orb<WhitePaintOrb>(),
            WhitePaintOrb => ModelDb.Orb<BlackPaintOrb>(),
            _ => null
        };
    }
}
