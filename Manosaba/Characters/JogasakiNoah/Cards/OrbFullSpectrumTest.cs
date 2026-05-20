using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models.CardPools;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards;

[Pool(typeof(QuestCardPool))]
public class OrbFullSpectrumTest : PathCustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Quest;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromOrb<RedPaintOrb>(),
        HoverTipFactory.FromOrb<OrangePaintOrb>(),
        HoverTipFactory.FromOrb<YellowPaintOrb>(),
        HoverTipFactory.FromOrb<GreenPaintOrb>(),
        HoverTipFactory.FromOrb<BluePaintOrb>(),
        HoverTipFactory.FromOrb<PurplePaintOrb>(),
        HoverTipFactory.FromOrb<BlackPaintOrb>(),
        HoverTipFactory.FromOrb<WhitePaintOrb>()
    ];

    public OrbFullSpectrumTest() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<OrbModel> orbs =
        [
            ModelDb.Orb<RedPaintOrb>(),
            ModelDb.Orb<OrangePaintOrb>(),
            ModelDb.Orb<YellowPaintOrb>(),
            ModelDb.Orb<GreenPaintOrb>(),
            ModelDb.Orb<BluePaintOrb>(),
            ModelDb.Orb<PurplePaintOrb>(),
            ModelDb.Orb<BlackPaintOrb>(),
            ModelDb.Orb<WhitePaintOrb>()
        ];

        foreach (OrbModel orb in orbs)
        {
            await OrbCmd.Channel(choiceContext, orb.ToMutable(), Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
