using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Cards
{
    [Pool(typeof(HasumiLeiaCardPool))]
    public sealed class StandOut : PathCustomCardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        public override bool GainsBlock => true;
        private const int energyCost = 2;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StandOutPower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(16, ValueProp.Move)];

        public StandOut() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
            CombatState? combatState = Owner.Creature.CombatState;
            if (combatState != null)
            {
                foreach (Creature teammate in combatState.GetTeammatesOf(Owner.Creature).Where(c => c.IsAlive && c.IsPlayer && c != Owner.Creature))
                {
                    await PowerCmd.Apply<CoveredPower>(teammate, 1m, base.Owner.Creature, this);
                }
            }

            await PowerCmd.Apply<StandOutPower>(Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Block.UpgradeValueBy(7m);
        }
    }
}
