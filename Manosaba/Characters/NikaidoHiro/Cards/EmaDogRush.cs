using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(NikaidoHiroCardPool))]
    public class EmaDogRush : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Attack;
        private const CardRarity rarity = CardRarity.Common;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new SummonVar(2),
            new DamageVar(6, ValueProp.Move)
        ];

        public EmaDogRush() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            Creature? target = cardPlay.Target;
            if (target == null)
            {
                return;
            }

            SummonResult summonResult = await SakurabaEmaDogCmd.Summon(choiceContext, Owner, DynamicVars.Summon.BaseValue, this);
            Creature? ema = summonResult.Creature;
            if (ema == null || !ema.IsAlive)
            {
                return;
            }

            await CreatureCmd.Damage(choiceContext, target, DynamicVars.Damage, ema, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3);
        }
    }
}
