using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using manosaba.Extensions;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        private const int EnergyCostValue = 1;
        private const CardType CardTypeValue = CardType.Skill;
        private const CardRarity Rarity = CardRarity.Uncommon;
        private const TargetType TargetTypeValue = TargetType.TargetedNoCreature;
        private const bool ShouldShowInCardLibrary = true;

        internal int? PendingInsertIndex { get; set; }

        public override string PortraitPath => "palette.png".CardsImagePath();

        protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [
            HoverTipFactory.FromOrb<RedPaintOrb>(),
            HoverTipFactory.FromOrb<YellowPaintOrb>(),
            HoverTipFactory.FromOrb<BluePaintOrb>()
        ];

        protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(1)];

        public PaletteGap() : base(EnergyCostValue, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int insertIndex = ConsumeInsertIndexOrDefault();

            for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
            {
                OrbModel randomOrb = RollOrbFromChanceTable();
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
