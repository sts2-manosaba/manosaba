using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class LandMine : PathCustomCardModel
{
    private const int EnergyCost = 0;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Token;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TheBombPower>()];

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public LandMine()
        : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TheBombPower>(Owner.Creature, 1m, Owner.Creature, this);
    }
}
