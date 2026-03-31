using BaseLib.Utils;
using Manosaba;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

using Manosaba.Characters.TachibanaSherry.Powers;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class QuickWit : PathCustomCardModel
    {
        private const int energyCost = 2;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<QuickWitPower>()];

        public QuickWit() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (!ManosabaFeatureFlags.AprilFoolsModeEnabled)
            {
                await PowerCmd.Apply<QuickWitPower>(Owner.Creature, 1m, Owner.Creature, this);
                return;
            }

            IEnumerable<Creature> teammates = base.CombatState.GetTeammatesOf(base.Owner.Creature);
            foreach (Creature creature in teammates)
            {
                if (creature == null || !creature.IsAlive || !creature.IsPlayer)
                    continue;

                await PowerCmd.Apply<QuickWitPower>(creature, 1m, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Innate);
        }

    }
}
