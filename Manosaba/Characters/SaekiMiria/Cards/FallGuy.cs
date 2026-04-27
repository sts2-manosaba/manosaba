using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.SaekiMiria.Cards
{
    [Pool(typeof(SaekiMiriaCardPool))]
    public class FallGuy : PathCustomCardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AnyAlly;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VotePower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move)];
        public FallGuy() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {

            if (Owner.Creature is not { } ownerCreature || cardPlay.Target is not { } target)
            {
                return;
            }

            var votePower = target.GetPowerAmount<VotePower>();
            await PowerCmd.Apply<VotePower>(target, -votePower, ownerCreature, this);
            await PowerCmd.Apply<VotePower>(ownerCreature, votePower, ownerCreature, this);
            await CreatureCmd.GainBlock(ownerCreature, DynamicVars.Block, cardPlay);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(3m);
        }
    }
}
