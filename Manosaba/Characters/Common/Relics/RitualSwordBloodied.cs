using System.Collections.Generic;
using BaseLib.Utils;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;

namespace Manosaba.Characters.Common.Relics;

[Pool(typeof(EventRelicPool))]
public sealed class RitualSwordBloodied : PathCustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(3m),
        new PowerVar<DexterityPower>(3m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<DexterityPower>(),
    ];

    public override async Task BeforeCombatStart()
    {
        if (Owner?.Creature == null)
        {
            return;
        }

        RoomType roomType = Owner.Creature.CombatState?.Encounter?.RoomType ?? RoomType.Monster;
        if (roomType is not (RoomType.Elite or RoomType.Boss))
        {
            return;
        }

        Flash();
        await CommonActions.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, null, DynamicVars["StrengthPower"].BaseValue);
        await CommonActions.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, null, DynamicVars["DexterityPower"].BaseValue);
    }
}
