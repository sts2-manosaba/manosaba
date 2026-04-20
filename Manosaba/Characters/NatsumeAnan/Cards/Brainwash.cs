using BaseLib.Utils;
using manosaba.Characters.NatsumeAnan.Powers;
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
    private static int KotodamaRealCost => 20;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("KotodamaCost", KotodamaRealCost),
        new DynamicVar("MajokaKotodamaCap", 10m),
        new PowerVar<BrainwashExtraTurnPower>(1m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

    public Brainwash() : base(0, CardType.Power, CardRarity.Ancient, TargetType.AllAllies, true)
    {
    }

    public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is MajokaPower && power.Owner == Owner.Creature)
        {
            int cap = DynamicVars["MajokaKotodamaCap"].IntValue;
            int majokaBasedGain = (int)Math.Floor(Owner.Creature.GetPowerAmount<MajokaPower>() / 10m);
            int kotodamaCostReduction = Math.Clamp(majokaBasedGain, 0, cap);
            if (kotodamaCostReduction > 0)
            {
                DynamicVars["KotodamaCost"].BaseValue = Math.Max(KotodamaRealCost - kotodamaCostReduction, 0);
            }
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        IEnumerable<Player> teammates = CombatState
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
    }
}
