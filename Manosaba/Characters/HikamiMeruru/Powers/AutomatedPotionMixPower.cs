using Manosaba.Characters.HikamiMeruru.PotionCraft;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Manosaba.Characters.HikamiMeruru.Powers;

public sealed class AutomatedPotionMixPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side || Owner.Player == null)
            return;

        for (int i = 0; i < (int)Amount; i++)
        {
            PotionRecipe? recipe = PotionCraftService.FindFirstCraftableRecipe(Owner.Player.PotionSlots);
            if (recipe == null)
                break;

            await PotionCraftService.TryCraft(Owner.Player, Owner.Player.PotionSlots, recipe);
        }
    }
}
