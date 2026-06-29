using BaseLib.Utils;
using manosaba.Characters.SawatariCoco;
using manosaba.Characters.SawatariCoco.Visuals;
using Manosaba.Characters.SawatariCoco.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace manosaba.Characters.SawatariCoco.Cards;

[Pool(typeof(SawatariCocoCardPool))]
public sealed class FanService : PathCustomCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override bool IsPlayable => base.IsPlayable && Owner.Character is global::manosaba.Characters.SawatariCoco.SawatariCoco;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<FanServiceSkillPower>(),
        HoverTipFactory.FromCard<FanServiceAutographToken>(),
        HoverTipFactory.FromCard<FanServiceRoastTimeToken>(),
    ];

    public FanService() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        FanServiceSkillPower? existingPower = ownerCreature.GetPower<FanServiceSkillPower>();
        if (existingPower != null)
        {
            existingPower.OnDuplicateCardPlayed();
        }
        else
        {
            await PowerCmd.Apply<FanServiceSkillPower>(choiceContext, ownerCreature, 1m, ownerCreature, this);
        }

        FanServiceSkillButtonUi.EnsureShown(Owner);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
