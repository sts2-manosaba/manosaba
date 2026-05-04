using BaseLib.Utils;
using manosaba.Characters.Common;
using manosaba.Characters.SaekiMiria;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System;

namespace Manosaba.Characters.SaekiMiria.Cards
{
    [Pool(typeof(SaekiMiriaCardPool))]
    public class KeyEvidence : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Attack;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SusPower>()];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];

        public KeyEvidence() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int voteStacks = Owner.Creature.GetPowerAmount<SusPower>();
            int hitCount = voteStacks;

            if (cardPlay.Target is not { } target)
            {
                return;
            }

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(target)
                .WithHitCount(hitCount)
                .Execute(choiceContext);

            await PowerCmd.Apply<SusPower>(Owner.Creature, -voteStacks, Owner.Creature, null);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(1m);
        }
    }
}
