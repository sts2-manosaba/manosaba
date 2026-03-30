using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Potions
{
    [Pool(typeof(JogasakiNoahPotionPool))]
    public class DrawingBoard : PathCustomPotionModel
    {
        private ModelId _storedMonsterId = ModelId.none;
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override PotionRarity Rarity => PotionRarity.Token;
        public override TargetType TargetType => TargetType.AnyPlayer;

        public override bool CanBeGeneratedInCombat => true;

        public void SetStoredMonster(ModelId id)
        {
            AssertMutable();
            _storedMonsterId = id;
        }

        protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
        {
            if (_storedMonsterId == ModelId.none) return;

            MonsterModel monster = ModelDb.GetById<MonsterModel>(_storedMonsterId).ToMutable();
            Creature pet = Owner.Creature.CombatState.CreateCreature(monster, Owner.Creature.Side, null);
            await PlayerCmd.AddPet(pet, Owner);
        }
    }
}
