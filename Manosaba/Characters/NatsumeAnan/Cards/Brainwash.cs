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
using MegaCrit.Sts2.Core.Models.Exceptions;

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

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        Creature? ownerCreature = TryGetOwnerCreatureForMajokaChecks();
        if (power is MajokaPower && ownerCreature != null && power.Owner == ownerCreature)
        {
            RefreshKotodamaCostFromMajoka();
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
    {
        if (ReferenceEquals(card, this))
        {
            RefreshKotodamaCostFromMajoka();
        }

        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        MegaCrit.Sts2.Core.Combat.ICombatState? combatState = CombatState;
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
            await CommonActions.Apply<BrainwashExtraTurnPower>(choiceContext, teammate.Creature, this, DynamicVars["BrainwashExtraTurnPower"].BaseValue);
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
        Creature? ownerCreature = TryGetOwnerCreatureForMajokaChecks();
        if (ownerCreature == null)
        {
            DynamicVars["KotodamaCost"].BaseValue = KotodamaRealCost;
            return;
        }

        int cap = DynamicVars["MajokaKotodamaCap"].IntValue;
        int majokaBasedGain = (int)Math.Floor(ownerCreature.GetPowerAmount<MajokaPower>() / 10m);
        int kotodamaCostReduction = Math.Clamp(majokaBasedGain, 0, cap);
        DynamicVars["KotodamaCost"].BaseValue = Math.Max(KotodamaRealCost - kotodamaCostReduction, 0);
    }

    private Creature? TryGetOwnerCreatureForMajokaChecks()
    {
        try
        {
            Player owner = Owner;
            return owner?.Creature;
        }
        catch (CanonicalModelException)
        {
            return null;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }
}
