using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.NikaidoHiro.Powers;

public sealed class LifeSentencePower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SusPower>()];

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> creatures)
    {
        _ = choiceContext;
        _ = creatures;

        if (side != Owner.Side || Amount <= 0m)
        {
            return;
        }

        decimal suspicion = Owner.GetPowerAmount<SusPower>();
        if (suspicion <= 0m)
        {
            return;
        }

        Flash();
        await CreatureCmd.GainBlock(Owner, suspicion * Amount, ValueProp.Unpowered, null);
    }
}
