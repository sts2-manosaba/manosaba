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
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MovieInvitationPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MovieInvitationPower>(1m)];

    public MovieInvitation()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
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
