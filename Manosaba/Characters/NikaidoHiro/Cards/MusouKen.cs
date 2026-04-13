using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(NikaidoHiroCardPool))]
    public class MusouKen : PathCustomCardModel
    {
        private const int energyCost = 3;
        private const CardType type = CardType.Attack;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.AnyEnemy;
        private const bool shouldShowInCardLibrary = true;

        protected override bool IsPlayable => base.IsPlayable && Owner.Creature.HasPower<HighStancePower>();

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MusouKenPower>(35m)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MusouKenPower>(), HoverTipFactory.FromPower<HighStancePower>()];

        public MusouKen() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            MusouKenPower? power = await PowerCmd.Apply<MusouKenPower>(
                Owner.Creature,
                DynamicVars["MusouKenPower"].BaseValue,
                Owner.Creature,
                this);
            power?.SetTarget(cardPlay.Target);
            power?.SetSourceCard(this);

            PlayerCmd.EndTurn(Owner, canBackOut: false);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["MusouKenPower"].UpgradeValueBy(15m);
        }
    }
}
