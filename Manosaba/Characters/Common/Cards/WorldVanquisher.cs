using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Characters.Common.Commands;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(CommonCardPool))]
    public class WorldVanquisher : PathCustomCardModel
    {
        private const string VfxScenePath = "res://Manosaba/scenes/common/vfx/world_vanquisher.tscn";
        private const int energyCost = 0;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];
        protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("MajokaPower", 200)];

        public WorldVanquisher() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            CombatState? combatState = CombatState;
            Creature? ownerCreature = Owner.Creature;
            if (combatState == null || ownerCreature == null)
            {
                return;
            }
            await CardPileCmd.Draw(choiceContext, base.DynamicVars.Cards.BaseValue, base.Owner);
            if (cardPlay.IsAutoPlay)
            {
                int totalMajoka = 0;
                foreach (Creature c in base.Owner.Creature.CombatState.Creatures)
                {
                    totalMajoka += c.GetPowerAmount<MajokaPower>();
                }
                int playerTimesCost = base.Owner.Creature.CombatState.Creatures.Count(c => c.IsPlayer) * DynamicVars["MajokaPower"].IntValue;
                if (totalMajoka >= playerTimesCost)
                {
                    IReadOnlyList<Creature> hittableEnemies = combatState.GetOpponentsOf(ownerCreature)
                    .Where(e => e.IsHittable)
                    .ToList();
                    if (hittableEnemies.Count == 0)
                    {
                        return;
                    }

                    await ManosabaCombatCmd.ForceWinWithoutDeathOrEscape(combatState);
                }
            }
        }

        public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
        {
            CardPile? pile = base.Pile;
            if (pile != null && (pile.Type == PileType.Discard || pile.Type == PileType.Hand || pile.Type == PileType.Draw || pile.Type == PileType.Exhaust) && side == CombatSide.Player)
            {
                CombatState combatState = Owner.Creature.CombatState;
                int totalMajoka = 0;
                foreach (Creature c in combatState.Creatures)
                {
                    totalMajoka += c.GetPowerAmount<MajokaPower>();
                }
                int playerTimesCost = combatState.Creatures.Count(c => c.IsPlayer) * DynamicVars["MajokaPower"].IntValue;
                if (totalMajoka >= playerTimesCost)
                {
                    await ManosabaVfxCmd.PlaySceneAtCombatCenterAndWait(VfxScenePath, fitCoverViewport: true, spriteNodeNames: ["StillA", "StillB"]);
                    await CardCmd.AutoPlay(choiceContext, this, null);
                }
            }
        }

        public override async Task AfterPowerAmountChanged(PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power is MajokaPower)
            {
                CombatState combatState = Owner.Creature.CombatState;
                int totalMajoka = 0;
                foreach (Creature c in combatState.Creatures)
                {
                    totalMajoka += c.GetPowerAmount<MajokaPower>();
                }
                int playerTimesCost = combatState.Creatures.Count(c => c.IsPlayer) * DynamicVars["MajokaPower"].IntValue;
                if (totalMajoka >= playerTimesCost)
                {
                    SfxCmd.Play("event:/Manosaba/audio/bgm/world_vanquisher.mp3", 0.8f);
                }
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}
