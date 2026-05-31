using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Manosaba.Characters.Common.Powers;

public class SusPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == CombatSide.Player && ShouldAddBadEnd() && Owner.Player != null)
        {
            List<BadEnd> cards = BadEnd.Create(Owner.Player, 1, combatState).ToList();
            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, addedByPlayer: true, CardPilePosition.Top);
            CardCmd.PreviewCardPileAdd(results);
        }
    }

    private bool ShouldAddBadEnd()
    {
        int suspicion = Owner.GetPowerAmount<SusPower>();
        if (Owner.Player?.PlayerCombatState?.GetPet<SakurabaEmaDog>() is { IsAlive: true })
        {
            return suspicion > 20;
        }

        return suspicion > 10;
    }
}
