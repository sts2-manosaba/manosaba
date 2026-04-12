using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using manosaba.Characters.SaekiMiria;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HasumiLeia.Cards
{
    [Pool(typeof(HasumiLeiaCardPool))]
    public class Clusterphobia : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MajokaPower>(20m)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];
        public Clusterphobia() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars["MajokaPower"].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["MajokaPower"].UpgradeValueBy(10m);
        }
    }
}
