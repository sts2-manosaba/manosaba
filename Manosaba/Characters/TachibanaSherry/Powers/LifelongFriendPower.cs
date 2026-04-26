using System;
using BaseLib.Utils;
using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TachibanaSherry.Powers
{
    public sealed class LifelongFriendPower : PathCustomPowerModel
    {
        private Creature? _partner;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Boulders>()];

        public void SetPartner(Creature partner)
        {
            _partner = partner;
        }

        public override async Task BeforePlayPhaseStart(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Entities.Players.Player player)
        {
            if (player.Creature != Owner)
                return;

            for (int stack = 0; stack < (int)Amount; stack++)
            {
                decimal current = Owner.GetPowerAmount<MajokaPower>();
                if (current > 0)
                {
                    decimal remove = Math.Min(10m, current);
                    await PowerCmd.Apply<MajokaPower>(Owner, -remove, Owner, null);
                }

                if (Owner.CombatState != null)
                {
                    CardModel boulder = Owner.CombatState.CreateCard(ModelDb.Card<Boulders>(), Owner.Player);
                    CardCmd.ApplyKeyword(boulder, CardKeyword.Exhaust);
                    boulder.EnergyCost.SetThisTurnOrUntilPlayed(0);
                    await CardPileCmd.AddGeneratedCardToCombat(boulder, PileType.Hand, true);
                }
            }
        }

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (wasRemovalPrevented)
                return;

            if (creature == Owner && _partner != null && _partner.IsAlive)
            {
                await CreatureCmd.Kill(_partner);
                return;
            }

            if (_partner != null && creature == _partner && Owner.IsAlive)
            {
                await CreatureCmd.Kill(Owner);
            }
        }
    }
}
