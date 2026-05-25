using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.JogasakiNoah.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards;

[Pool(typeof(JogasakiNoahCardPool))]
public class PaintRefill : PathCustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PaintRefillPower>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Unique];

    public PaintRefill() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        await PowerCmd.Apply<PaintRefillPower>(ownerCreature, 1m, ownerCreature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
