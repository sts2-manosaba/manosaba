using BaseLib.Utils;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.JogasakiNoah.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace manosaba.Characters.JogasakiNoah.Relics
{

    [Pool(typeof(JogasakiNoahRelicPool))]
    public sealed class SprayCanOfNoah : LevelingPathCustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;
        protected override int MaxRelicLevel => 5;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("OrbSlots", 6m)];

        public override Task AfterObtained()
        {
            ApplyRelicLevelEffects();
            return Task.CompletedTask;
        }

        public override async Task BeforeCombatStart()
        {
            ApplyRelicLevelEffects();
            await OrbCmd.AddSlots(base.Owner, base.DynamicVars["OrbSlots"].IntValue);
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (base.Owner.Creature != player.Creature)
            {
                return;
            }

            int randomOrbsToChannel = RelicLevel switch
            {
                >= 5 => 3,
                >= 3 => 2,
                _ => 1
            };
            for (int i = 0; i < randomOrbsToChannel; i++)
            {
                OrbModel randomOrb = JogasakiNoahOrbPool.RollRandomGeneratedOrb(base.Owner);
                await OrbCmd.Channel(choiceContext, randomOrb.ToMutable(), base.Owner);
            }
        }

        public override async Task AfterDamageReceived(
            PlayerChoiceContext choiceContext,
            Creature target,
            DamageResult result,
            ValueProp props,
            Creature? dealer,
            CardModel? cardSource)
        {
            _ = props;
            _ = dealer;
            _ = cardSource;

            if (target != Owner.Creature || result.UnblockedDamage <= 0 || !HasSekketsusoujitsuCardInDeck())
            {
                return;
            }

            if (Owner.RunState.Rng.Niche.NextInt(100) >= 50)
            {
                return;
            }

            Flash();
            BloodOrb bloodOrb = (BloodOrb)ModelDb.Orb<BloodOrb>().ToMutable();
            bloodOrb.AddLayers(result.UnblockedDamage - 3m);
            await OrbCmd.Channel(choiceContext, bloodOrb, Owner);
        }

        protected override void OnRelicLevelChanged(int oldLevel, int newLevel)
        {
            ApplyRelicLevelEffects();
        }

        private void ApplyRelicLevelEffects()
        {
            int bonusSlots = (RelicLevel - 1) / 2;
            base.DynamicVars["OrbSlots"].BaseValue = 6m + bonusSlots;
        }

        private bool HasSekketsusoujitsuCardInDeck()
        {
            return Owner.Deck?.Cards.Any(card => card.Keywords.Contains(ManosabaKeywords.Sekketsusoujitsu)) == true;
        }
    }
}
