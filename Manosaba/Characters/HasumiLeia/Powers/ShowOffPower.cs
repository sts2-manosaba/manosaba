using BaseLib.Extensions;
using Manosaba.Characters.Common.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Powers;

public class ShowOffPower : PathCustomPowerModel
{
    private class Data
    {
        public readonly Dictionary<CardModel, int> amountsForPlayedCards = new Dictionary<CardModel, int>();
    }

    public const string strengthAppliedKey = "StrengthApplied";
    private bool _shouldExpireAtEndOfNextTurn = false;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType
    {
        get
        {
            if (base.DynamicVars["StrengthApplied"].IntValue != 0)
            {
                return PowerStackType.Counter;
            }

            return PowerStackType.None;
        }
    }

    public override int DisplayAmount => base.DynamicVars["StrengthApplied"].IntValue;

    public override bool IsInstanced => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
    
        new PowerVar<StrengthPower>(1m),
        new DynamicVar("StrengthApplied", 0m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override object InitInternalData()
    {
        return new Data();
    }


    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        Console.WriteLine($"ShowOffPower AfterDamageReceived: target={target.Name}, damage={result.UnblockedDamage}, isPoweredAttack={props.IsPoweredAttack_()}, dealer={(dealer != null ? dealer.Name : "null")}");
        if (_shouldExpireAtEndOfNextTurn)
            return;
        Console.WriteLine($"AfterDamageReceived called with target={target.Name}, damage={result.UnblockedDamage}, isPoweredAttack={props.IsPoweredAttack_()}, dealer={(dealer != null ? dealer.Name : "null")}");
        if(target != base.Owner)
        {
            Console.WriteLine("Damage received by " + target.Name + ", not the owner of ShowOffPower. Ignoring.");
            return;
        }
        if(dealer == null)
        {
            Console.WriteLine("No dealer for the damage. Ignoring.");
            return;
        }
        if(!props.IsPoweredAttack_())
        {
            Console.WriteLine("Damage is not from a powered attack. Ignoring.");
            return;
        }
        if (result.WasFullyBlocked)
        {
            Console.WriteLine("ShowOffPower triggered by damage from " + dealer.Name);
            Flash();
            await PowerCmd.Apply<StrengthPower>(base.Owner, Amount, base.Owner, null, silent: true);
            base.DynamicVars["StrengthApplied"].BaseValue += (decimal)base.DynamicVars.Strength.IntValue;
            InvokeDisplayAmountChanged();
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == base.Owner.Side)
        {
            if (!_shouldExpireAtEndOfNextTurn)
            {
                return;
            }

            await PowerCmd.Remove(this);
            await PowerCmd.Apply<StrengthPower>(base.Owner, -base.DynamicVars["StrengthApplied"].BaseValue, base.Owner, null, silent: true);
        }
        else
        {
            _shouldExpireAtEndOfNextTurn = true;
        }
    }
}

