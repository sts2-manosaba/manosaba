using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.NatsumeAnan.Cards;

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class Urusai : NatsumeKotodamaCardModel
{
    private int _resolvedKotodamaX;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
    ];

    public Urusai() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy, true)
    {
    }

    public int PrepareKotodamaXCostForPlay()
    {
        _resolvedKotodamaX = Math.Max(0, KotodamaEnergy.Get(Owner));
        return _resolvedKotodamaX;
    }

    public void OverrideResolvedKotodamaXCostForPlay(int value)
    {
        _resolvedKotodamaX = Math.Max(0, value);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        int hits = _resolvedKotodamaX;
        _resolvedKotodamaX = 0;

        if (hits <= 0)
        {
            return;
        }

        MegaCrit.Sts2.Core.Combat.CombatState? combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hits)
            .FromCard(this)
            .TargetingRandomOpponents(combatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
