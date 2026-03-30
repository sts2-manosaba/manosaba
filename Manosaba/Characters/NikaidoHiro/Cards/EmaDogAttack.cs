using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(NikaidoHiroCardPool))]
    public class EmaDogAttack : PathCustomCardModel
    {

        private const int energyCost = 2;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        protected override bool IsPlayable => Owner.PlayerCombatState.GetPet<SakurabaEmaDog>() is not null ? Owner.PlayerCombatState.GetPet<SakurabaEmaDog>().IsAlive : false;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<EmaDogAttackPower>(1)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<EmaDogAttackPower>()];

        public EmaDogAttack() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<EmaDogAttackPower>(Owner.PlayerCombatState.GetPet<SakurabaEmaDog>(), DynamicVars["EmaDogAttackPower"].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
