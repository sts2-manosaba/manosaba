using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards;

[Pool(typeof(JogasakiNoahCardPool))]
public class ComplementaryColor : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(1)];

    public ComplementaryColor() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner == null)
        {
            return;
        }

        IReadOnlyList<OrbModel>? orbList = Owner.PlayerCombatState?.OrbQueue?.Orbs;
        if (orbList == null || orbList.Count == 0)
        {
            return;
        }

        OrbModel? complementaryOrb = GetComplementaryOrb(orbList[^1])?.ToMutable();
        if (complementaryOrb == null)
        {
            return;
        }

        await OrbCmd.Channel(choiceContext, complementaryOrb, Owner);

        if (Owner.PlayerCombatState?.OrbQueue?.Orbs.Contains(complementaryOrb) != true)
        {
            return;
        }

        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            await OrbCmd.Passive(choiceContext, complementaryOrb, null);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1);
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
            BloodOrb => ModelDb.Orb<BloodOrb>(),
            _ => null
        };
    }
}
