using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(CommonCardPool))]
    public class WorldVanquisher : PathCustomCardModel
    {

        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("MajokaPower", 200)];

        public WorldVanquisher() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
            if (cardPlay.IsAutoPlay)
            {
                var combatState = base.CombatState;
                if (combatState == null)
                    return;
                List<Creature> creatures = combatState.Enemies.ToList();
                foreach (Creature creature in creatures)
                {
                    if (creature == null || creature.IsDead) continue;
                    await DoomPower.DoomKill([creature]);
                }
            }
        }

        public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, CombatState combatState)
        {
            CardPile? pile = base.Pile;
            if (pile != null && (pile.Type == PileType.Discard || pile.Type == PileType.Hand || pile.Type == PileType.Draw) && player == base.Owner)
            {
                int totalMajoka = 0;
                foreach (Creature c in combatState.Creatures)
                {
                    totalMajoka += c.GetPowerAmount<MajokaPower>();
                }
                int playerTimesCost = combatState.Creatures.Count(c => c.IsPlayer) * DynamicVars["MajokaPower"].IntValue;
                if (totalMajoka >= playerTimesCost)
                    await CardCmd.AutoPlay(choiceContext, this, null);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}
