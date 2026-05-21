using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

using MegaCrit.Sts2.Core.Entities.Creatures;
namespace Manosaba.Characters.HikamiMeruru.Powers;

public sealed class AutomatedPotionMixPower : PathCustomPowerModel
{
    private static readonly List<PotionModel> PotionPool =
    [
        ModelDb.Potion<LesserPainKillerPotion>(),
        ModelDb.Potion<LesserBlockPotion>(),
        ModelDb.Potion<LesserFlexPotion>()
    ];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (side != Owner.Side || Owner.Player == null)
            return;

        for (int i = 0; i < (int)Amount; i++)
        {
            PotionModel potionModel = PotionPool[Owner.Player.RunState.Rng.CombatPotionGeneration.NextInt(PotionPool.Count)];
            await PotionCmd.TryToProcure(potionModel.ToMutable(), Owner.Player);
        }
    }
}
