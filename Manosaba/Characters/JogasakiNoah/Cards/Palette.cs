using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.JogasakiNoah.Cards
{
    [Pool(typeof(JogasakiNoahCardPool))]
    public class Palette : PathCustomCardModel
    {
        private static readonly IReadOnlyList<(OrbModel Orb, int Weight)> OrbChanceTable =
        [
            (ModelDb.Orb<RedPaintOrb>(), 40),
            (ModelDb.Orb<BluePaintOrb>(), 40),
            (ModelDb.Orb<YellowPaintOrb>(), 20)
        ];

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Basic;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<RedPaintOrb>(), HoverTipFactory.FromOrb<YellowPaintOrb>(), HoverTipFactory.FromOrb<BluePaintOrb>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(4, ValueProp.Move)];

        public Palette() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

            OrbModel randomOrb = RollOrbFromChanceTable();
            await OrbCmd.Channel(choiceContext, randomOrb.ToMutable(), Owner);
        }

        private OrbModel RollOrbFromChanceTable()
        {
            int roll = Owner.RunState.Rng.CombatOrbGeneration.NextInt(100);
            int cumulative = 0;
            foreach ((OrbModel orb, int weight) in OrbChanceTable)
            {
                cumulative += weight;
                if (roll < cumulative)
                {
                    return orb;
                }
            }

            return OrbChanceTable[^1].Orb;
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(2);
        }
    }
}
