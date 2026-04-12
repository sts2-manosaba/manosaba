using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using manosaba.Characters.HikamiMeruru;
using manosaba.Characters.SaekiMiria;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Cards
{
    [Pool(typeof(HasumiLeiaCardPool))]
    public sealed class DefensiveStance : PathCustomCardModel
    {
        private const string _powerKey = "Power";

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Power", 1m)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

        public DefensiveStance()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self,true)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
            await PowerCmd.Apply<ShadowmeldPower>(base.Owner.Creature, base.DynamicVars["Power"].BaseValue, base.Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            base.EnergyCost.UpgradeBy(-1);
        }
    }
}
