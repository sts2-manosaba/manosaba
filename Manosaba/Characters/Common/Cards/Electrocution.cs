using BaseLib.Utils;
using manosaba.Characters.Common;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.Common.Cards
{
    [Pool(typeof(CommonCardPool))]
    public class Electrocution : PathCustomCardModel
    {
        protected override bool HasEnergyCostX => true;
        private const int energyCost = 0;
        private const CardType type = CardType.Attack;
        private const CardRarity rarity = CardRarity.Uncommon;
        private const TargetType targetType = TargetType.AllEnemies;
        private const bool shouldShowInCardLibrary = true;

        protected string EvokeSfx => "event:/sfx/characters/defect/defect_lightning_evoke";
        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Unpowered)];

        public Electrocution() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int num = ResolveEnergyXValue();
            for (int i = 0; i < num; i++)
            {
                await Cmd.Wait(0.15f);
                Strike(choiceContext, DynamicVars.Damage.BaseValue);
            }
        }

        private async void Strike(PlayerChoiceContext choiceContext, Decimal value)
        {
            List<Creature> list = (from e in base.CombatState.GetOpponentsOf(base.Owner.Creature)
                                   where e.IsHittable
                                   select e).ToList();
            if (list.Count == 0)
            {
                return;
            }

            Creature target = base.Owner.RunState.Rng.CombatTargets.NextItem(list);

            VfxCmd.PlayOnCreature(target, "vfx/vfx_attack_lightning");

            PlayEvokeSfx();
            await CreatureCmd.Damage(choiceContext, target, value, ValueProp.Unpowered, base.Owner.Creature);
            return;
        }

        protected void PlayEvokeSfx()
        {
            if (EvokeSfx != "")
            {
                SfxCmd.Play(EvokeSfx);
            }
        }

        protected override void OnUpgrade()
        {
            base.DynamicVars.Damage.UpgradeValueBy(6m);
        }
    }
}
