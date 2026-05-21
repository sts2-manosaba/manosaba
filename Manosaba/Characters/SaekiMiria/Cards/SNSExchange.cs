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
    public class SnsExchange : PathCustomCardModel
    {
        //public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AllAllies;
        private const bool shouldShowInCardLibrary = true;
        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MajokaPower>(10m), new CardsVar(2)];
        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];
        public SnsExchange() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner?.Creature is not { } ownerCreature || base.CombatState is not { } combatState)
            {
                return;
            }

            await CommonActions.Apply<MajokaPower>(choiceContext, ownerCreature, this, DynamicVars["MajokaPower"].BaseValue);
            IEnumerable<Creature> enumerable = from c in combatState.GetTeammatesOf(ownerCreature)
                                               where c != null && c.IsAlive && c.IsPlayer && c.Player != null
                                               select c;
            foreach (Creature item in enumerable)
            {
                if (item.Player == null)
                {
                    continue;
                }

                await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, item.Player);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["MajokaPower"].UpgradeValueBy(5);
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}
