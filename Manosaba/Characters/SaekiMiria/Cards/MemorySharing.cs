using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Characters.SaekiMiria.Powers;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(SaekiMiriaCardPool))]
    public class MemorySharing : PathCustomCardModel
    {
        private const int energyCost = 3;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Ancient;
        private const TargetType targetType = TargetType.AllAllies;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MemorySharingPower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Mahou, CardKeyword.Eternal];

        
        public MemorySharing() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {

            IEnumerable<Player> enumerable = from c in base.CombatState.GetTeammatesOf(base.Owner.Creature)
                                             where c != null && c.IsAlive && c.IsPlayer
                                             select c.Player;

            foreach (Player item in enumerable)
            {
                Console.WriteLine($"Applying MemorySharingPower to {item.Creature.Name}");
                MemorySharingPower? selfPower = await PowerCmd.Apply<MemorySharingPower>(item.Creature, 1m, base.Owner.Creature, this);
                selfPower?.SetApplier(base.Owner.Creature);
                if (base.IsUpgraded) selfPower?.SetUpgraded();
            }
            
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Retain);
        }
    }
}
