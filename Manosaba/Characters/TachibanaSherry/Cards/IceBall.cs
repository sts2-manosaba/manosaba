using BaseLib.Utils;
using System;
using System.Linq;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards
{
    [Pool(typeof(TachibanaSherryCardPool))]
    public class IceBall : PathCustomCardModel
    {
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [];
        private const int energyCost = 0;
        private const CardType type = CardType.Attack;
        private const CardRarity rarity = CardRarity.Token;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = false;
        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        protected override IEnumerable<DynamicVar> CanonicalVars => [
            new CalculationBaseVar(6m),
            new ExtraDamageVar(2m),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) =>
                card.Owner != null ? PileType.Exhaust.GetPile(card.Owner).Cards.Count(c => c is IceBall) : 0)
        ];

        public IceBall() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public static IEnumerable<IceBall> Create(Player owner, int amount, CombatState combatState)
        {
            List<IceBall> list = [];
            for (int i = 0; i < amount; i++)
            {
                list.Add(combatState.CreateCard<IceBall>(owner));
            }

            return list;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await DamageCmd.Attack(base.DynamicVars.CalculatedDamage)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
        }
    }
}
