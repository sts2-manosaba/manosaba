using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SaekiMiria.Cards
{
    [Pool(typeof(SaekiMiriaCardPool))]
    public class BombDefusalMaster : PathCustomCardModel
    {
        public override TargetType TargetType => TargetType.AnyEnemy;

        protected override bool HasEnergyCostX => true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        
            HoverTipFactory.FromPower<StrengthPower>(),
            HoverTipFactory.FromPower<WeakPower>()
        ];

        public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

        public BombDefusalMaster()
            : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy,true)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (base.Owner.Creature is not { } ownerCreature || cardPlay.Target is not { } target)
            {
                return;
            }

            int powerAmount = ResolveEnergyXValue();
            if (base.IsUpgraded)
            {
                powerAmount++;
            }

            await CreatureCmd.TriggerAnim(ownerCreature, "Cast", base.Owner.Character.CastAnimDelay);
            await PowerCmd.Apply<StrengthPower>(target, -powerAmount, ownerCreature, this);
            await PowerCmd.Apply<WeakPower>(target, powerAmount, ownerCreature, this);
        }
    }
}
