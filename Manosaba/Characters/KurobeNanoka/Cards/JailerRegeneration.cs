using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using Manosaba.Characters.KurobeNanoka.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public sealed class JailerRegeneration : PathCustomCardModel
{
    private const string HealVar = "Heal";
    private const decimal BaseHealAmount = 10m;
    private const int energyCost = 2;
    private const CardType CardTypeValue = CardType.Power;
    private new const CardRarity Rarity = CardRarity.Rare;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<JailerRegenerationPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(HealVar, BaseHealAmount),
        new PowerVar<JailerRegenerationPower>(BaseHealAmount),
    ];

    public JailerRegeneration()
        : base(energyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        await PowerCmd.Apply<JailerRegenerationPower>(Owner.Creature, DynamicVars["JailerRegenerationPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
