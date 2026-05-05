using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HasumiLeia.Powers;

public sealed class TheCenterPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        _ = context;

        if (Owner.Player == null || cardPlay.Target != Owner)
        {
            return;
        }

        if (!IsOtherTeammate(cardPlay.Card.Owner?.Creature))
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, cardPlay.Card);
    }

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (target != Owner || !IsOtherTeammate(potion.Owner?.Creature))
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
    }

    private bool IsOtherTeammate(Creature? sourceCreature)
    {
        if (Owner.Player == null || sourceCreature == null || sourceCreature == Owner || Owner.CombatState == null)
        {
            return false;
        }

        return Owner.CombatState.GetTeammatesOf(Owner).Contains(sourceCreature);
    }
}
