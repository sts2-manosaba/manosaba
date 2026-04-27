using BaseLib.Utils;
using manosaba.Characters.NatsumeAnan.Powers;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.NatsumeAnan.Cards;

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class Brainwash : NatsumeKotodamaCardModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];

    private static int KotodamaRealCost => 20;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("KotodamaCost", KotodamaRealCost),
        new DynamicVar("MajokaKotodamaCap", 10m),
        new PowerVar<BrainwashExtraTurnPower>(1m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. base.ExtraHoverTips,
        HoverTipFactory.FromPower<MajokaPower>(),
    ];

    public Brainwash() : base(0, CardType.Power, CardRarity.Ancient, TargetType.AllAllies, true)
    {
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is MajokaPower && power.Owner == Owner.Creature)
        {
            RefreshKotodamaCostFromMajoka();
        }
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (ReferenceEquals(card, this))
        {
            RefreshKotodamaCostFromMajoka();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        MegaCrit.Sts2.Core.Combat.CombatState? combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        IEnumerable<Player> teammates = combatState
            .GetTeammatesOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive && creature.IsPlayer)
            .Select(creature => creature.Player)
            .OfType<Player>();

        foreach (Player teammate in teammates)
        {
            await PowerCmd.Apply<BrainwashExtraTurnPower>(teammate.Creature, DynamicVars["BrainwashExtraTurnPower"].BaseValue, Owner.Creature, this);
        }
        _ = choiceContext;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MajokaKotodamaCap"].UpgradeValueBy(5m);
        RefreshKotodamaCostFromMajoka();
    }

    private void RefreshKotodamaCostFromMajoka()
    {
        int cap = DynamicVars["MajokaKotodamaCap"].IntValue;
        int majokaBasedGain = (int)Math.Floor(Owner.Creature.GetPowerAmount<MajokaPower>() / 10m);
        int kotodamaCostReduction = Math.Clamp(majokaBasedGain, 0, cap);
        DynamicVars["KotodamaCost"].BaseValue = Math.Max(KotodamaRealCost - kotodamaCostReduction, 0);
    }
}
