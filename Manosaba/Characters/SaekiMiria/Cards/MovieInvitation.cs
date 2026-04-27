using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.SaekiMiria.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class MovieInvitation : PathCustomCardModel
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    private const int energyCost = 1;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Rare;
    private const TargetType TargetTypeValue = TargetType.AnyAlly;
    private const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MovieInvitationPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MovieInvitationPower>(1m)];

    public MovieInvitation()
        : base(energyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature is not { } ownerCreature || cardPlay.Target is not { } target)
        {
            return;
        }

        MovieInvitationPower? power = await PowerCmd.Apply<MovieInvitationPower>(
            ownerCreature,
            DynamicVars["MovieInvitationPower"].BaseValue,
            ownerCreature,
            this);

        power?.SetInvitedTarget(target);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
