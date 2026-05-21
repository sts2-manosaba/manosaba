using BaseLib.Utils;
using System.Collections.Generic;
using System.Linq;

using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Powers;

public class QuickWitPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private static readonly IReadOnlyList<int> StrengthGainValues = Enumerable.Range(-2, 5).ToList();
    private static readonly IReadOnlyList<int> DexterityDeltaValues = Enumerable.Range(-2, 5).ToList();
    private static readonly IReadOnlyList<int> HpDeltaValues = Enumerable.Range(-2, 5).ToList();
    private static readonly IReadOnlyList<int> ClueValues = new List<int> { 0, 1, 2 };

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<SherryDetectiveRewardPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
        HoverTipFactory.Static(StaticHoverTip.Block),
        HoverTipFactory.FromPower<CluePower>(),
    ];

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        var rng = Owner.Player.RunState.Rng.CombatTargets;
        bool maxRolls = Owner.GetPowerAmount<SherryDetectiveRewardPower>() > 0m;

        for (int stack = 0; stack < (int)Amount; stack++)
        {
            int strengthGain = maxRolls ? StrengthGainValues.Max() : rng.NextItem(StrengthGainValues);
            int dexDelta = maxRolls ? DexterityDeltaValues.Max() : rng.NextItem(DexterityDeltaValues);
            int hpDelta = maxRolls ? HpDeltaValues.Max() : rng.NextItem(HpDeltaValues);
            int clueStacks = maxRolls ? ClueValues.Max() : rng.NextItem(ClueValues);

            await CommonActions.Apply<StrengthPower>(choiceContext, Owner, null, strengthGain);
            if (dexDelta != 0)
            {
                await CommonActions.Apply<DexterityPower>(choiceContext, Owner, null, dexDelta);
            }

            if (hpDelta >= 0)
            {
                await CreatureCmd.Heal(Owner, hpDelta);
            }
            else
            {
                await CreatureCmd.Damage(choiceContext, Owner, -hpDelta, ValueProp.Unpowered, Owner);
            }

            if (clueStacks > 0)
            {
                await CommonActions.Apply<CluePower>(choiceContext, Owner, null, clueStacks);
            }
        }
    }
}
