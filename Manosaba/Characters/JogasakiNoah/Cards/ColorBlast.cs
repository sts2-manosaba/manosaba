using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;


namespace Manosaba.Characters.JogasakiNoah.Cards
{
    [Pool(typeof(JogasakiNoahCardPool))]
    public class ColorBlast : PathCustomCardModel
    {
        private const int energyCost = 3;
        public override OrbEvokeType OrbEvokeType => OrbEvokeType.All;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

        public ColorBlast() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            IReadOnlyList<OrbModel> paintOrbs = JogasakiNoahOrbPool.AllOrbs;
            int orbCount = base.Owner.PlayerCombatState.OrbQueue.Orbs.Count;
            for (int i = 0; i < orbCount; i++)
            {
                await OrbCmd.EvokeNext(choiceContext, base.Owner);
                OrbModel randomOrb = paintOrbs[Owner.RunState.Rng.CombatOrbGeneration.NextInt(paintOrbs.Count)];
                await OrbCmd.Channel(choiceContext, randomOrb.ToMutable(), Owner);
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }
}
