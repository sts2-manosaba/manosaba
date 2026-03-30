using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards
{
    [Pool(typeof(JogasakiNoahCardPool))]
    public class Palette : PathCustomCardModel
    {
        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Basic;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(1)];

        public Palette() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            IReadOnlyList<OrbModel> paintOrbs = JogasakiNoahOrbPool.AllOrbs;

            for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
            {
                OrbModel randomOrb = paintOrbs[Owner.RunState.Rng.CombatOrbGeneration.NextInt(paintOrbs.Count)];
                await OrbCmd.Channel(choiceContext, randomOrb.ToMutable(), Owner);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Repeat.UpgradeValueBy(1);
        }
    }
}
