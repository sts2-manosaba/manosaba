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
            if (Owner?.Creature?.CombatState is not { } combatState)
            {
                return;
            }

            MonsterModel? canonicalMonster = ModelDb.GetById<MonsterModel>(_storedMonsterId);
            if (canonicalMonster == null)
            {
                return;
            }

            MonsterModel monster = canonicalMonster.ToMutable();
            Creature pet = combatState.CreateCreature(monster, CombatSide.Player, null);
            await PlayerCmd.AddPet(pet, Owner);
            NCreature? node = NCombatRoom.Instance?.GetCreatureNode(pet);
            if (node != null)
            {
                Vector2 s = node.Visuals.GetCurrentBody().Scale;
                node.Visuals.GetCurrentBody().Scale = new Vector2(-Mathf.Abs(s.X), s.Y);
            }

            Creature perspective = pet.PetOwner?.Creature ?? Owner.Creature;
            if (pet.CombatState == null)
            {
                return;
            }

            List<Creature> petTargets = pet.CombatState.GetOpponentsOf(perspective)
                .Where(c => c != null && c.IsAlive)
                .ToList();
            if (PetEnemyAiPower.TryPrepareForNextTurnWithTargets(pet, petTargets, rollNewMove: true))
            {
                PetEnemyAiPower.TryAdvanceToValidMove(pet, petTargets);
            }
            await PowerCmd.Apply<PetEnemyAiPower>(pet, 1m, Owner.Creature, null);
        }
    }
}
