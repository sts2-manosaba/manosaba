using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using manosaba.Extensions;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards
{
    [Pool(typeof(JogasakiNoahCardPool))]
    public class PaletteGap : PathCustomCardModel
    {
        private static readonly IReadOnlyList<(OrbModel Orb, int Weight)> OrbChanceTable =
        [
            (ModelDb.Orb<RedPaintOrb>(), 40),
            (ModelDb.Orb<BluePaintOrb>(), 40),
            (ModelDb.Orb<YellowPaintOrb>(), 20)
        ];

        private const int energyCost = 1;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Token;
        private const TargetType targetType = TargetType.TargetedNoCreature;
        private const bool shouldShowInCardLibrary = false;
        public override bool CanBeGeneratedInCombat => false;
        public override bool CanBeGeneratedByModifiers => false;

        internal int? PendingInsertIndex { get; set; }

        public override string PortraitPath => "palette.png".CardsImagePath();

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromOrb<RedPaintOrb>(),
            HoverTipFactory.FromOrb<YellowPaintOrb>(),
            HoverTipFactory.FromOrb<BluePaintOrb>()
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(1)];

        public PaletteGap() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int insertIndex = ConsumeInsertIndexOrDefault();
            OrbModel randomOrb = RollOrbFromChanceTable();

            for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
            {
                await OrbGapChannelHelper.ChannelAt(choiceContext, randomOrb.ToMutable(), Owner, insertIndex);
                insertIndex++;
            }
        }

        private int ConsumeInsertIndexOrDefault()
        {
            int currentCount = Owner.PlayerCombatState?.OrbQueue?.Orbs?.Count ?? 0;
            int index = PendingInsertIndex ?? currentCount;
            PendingInsertIndex = null;
            return Math.Max(0, index);
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
            DynamicVars.Repeat.UpgradeValueBy(1);
        }
    }
}
