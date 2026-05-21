using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(TokenCardPool))]
    public class Vote : PathCustomCardModel
    {

        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Token;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;

        public override bool CanBeGeneratedInCombat => false;
        public override bool CanBeGeneratedByModifiers => false;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VotePower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new PowerVar<VotePower>(1)];

        public Vote() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public static IEnumerable<Vote> Create(Player owner, int amount, ICombatState combatState)
        {
            List<Vote> list = new List<Vote>();
            for (int i = 0; i < amount; i++)
            {
                list.Add(combatState.CreateCard<Vote>(owner));
            }

            return list;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
            if (base.Owner.Creature is { } ownerCreature && cardPlay.Target?.Monster?.Creature is { } targetCreature)
            {
                await CommonActions.Apply<VotePower>(choiceContext, targetCreature, this, DynamicVars["VotePower"].BaseValue);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}
