using BaseLib.Utils;
using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HikamiMeruru.Powers;

public sealed class MeruruInfirmaryPower : PathCustomPowerModel
{
    private const int PersistentBaseAmount = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner)
            return;

        int damageTaken = result.UnblockedDamage;
        if (damageTaken <= 0)
            return;

        await CommonActions.Apply<MeruruInfirmaryPower>(new ThrowingPlayerChoiceContext(), Owner, cardSource, damageTaken);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> creatures, ICombatState combatState)
    {
        if (side != Owner.Side)
            return;

        int stacks = Math.Max(0, (int)Amount - PersistentBaseAmount);
        if (stacks <= 0)
            return;

        int healAmount = stacks / 2;
        if (healAmount > 0 && Owner.Player != null)
        {
            PotionModel potion;
            if (healAmount >= 12)
            {
                potion = ModelDb.Potion<GreaterPainKillerPotion>().ToMutable();
            }
            else if (healAmount >= 5)
            {
                potion = ModelDb.Potion<PainKillerPotion>().ToMutable();
            }
            else
            {
                potion = ModelDb.Potion<LesserPainKillerPotion>().ToMutable();
            }
            await PotionCmd.TryToProcure(potion, Owner.Player);
        }

        decimal deltaToBase = PersistentBaseAmount - Amount;
        if (deltaToBase != 0m)
        {
            await CommonActions.Apply<MeruruInfirmaryPower>(new ThrowingPlayerChoiceContext(), Owner, null, deltaToBase);
        }
    }
}
