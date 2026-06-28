using BaseLib.Utils;
using Manosaba.Characters.SawatariCoco.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(TokenCardPool))]
public sealed class PlayWhatGame : PathCustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<LetsPlayAGamePower>()];

    public PlayWhatGame() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        decimal letsPlayAGameAmount = Owner.Creature.GetPowerAmount<LetsPlayAGamePower>();
        if (letsPlayAGameAmount > 0m)
        {
            await CommonActions.Apply<LetsPlayAGamePower>(choiceContext, Owner.Creature, this, -letsPlayAGameAmount);
        }
    }
}
