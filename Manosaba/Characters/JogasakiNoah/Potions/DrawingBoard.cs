using BaseLib.Utils;
using Godot;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.JogasakiNoah.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Manosaba.Characters.JogasakiNoah.Potions
{
    [Pool(typeof(JogasakiNoahPotionPool))]
    public class DrawingBoard : PathCustomPotionModel
    {
        [SavedProperty]
        public ModelId _storedMonsterId { get; set; } = ModelId.none;
        public SavedProperties? SaveData => SavedProperties.From(this);
        public bool IsNetworked => true;
        public override PotionUsage Usage => PotionUsage.CombatOnly;
        public override PotionRarity Rarity => PotionRarity.Token;
        public override TargetType TargetType => TargetType.Self;

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
            Creature pet = Owner.Creature.CombatState.CreateCreature(monster, CombatSide.Player, null);
            await PlayerCmd.AddPet(pet, Owner);
            NCreature node = NCombatRoom.Instance?.GetCreatureNode(pet);
            if (node != null)
            {
                Vector2 s = node.Visuals.Body.Scale;
                node.Visuals.Body.Scale = new Vector2(-Mathf.Abs(s.X), s.Y);
            }

            Creature perspective = pet.PetOwner?.Creature ?? Owner.Creature;
            pet.PrepareForNextTurn(pet.CombatState.GetOpponentsOf(perspective), true);
            await PowerCmd.Apply<PetEnemyAiPower>(pet, 1m, Owner.Creature, null);
        }
    }
}
