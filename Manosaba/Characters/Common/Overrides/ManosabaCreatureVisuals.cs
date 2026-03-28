using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Manosaba.Characters.Common.Overrides
{
    public partial class ManosabaCreatureVisuals : NCreatureVisuals
    {
        private Creature _creature;
        private Sprite2D _alive;
        private Sprite2D _dead;
        public override void _Ready()
        {
            base._Ready();

            _alive = GetNode<Sprite2D>("%Alive");
            _dead = GetNode<Sprite2D>("%Dead");
            var creatureNode = GetParent() as NCreature;
            if (creatureNode == null) return;
            _creature = creatureNode.Entity;
            _creature.CurrentHpChanged += OnHpChanged;
            _creature.Died += OnDied;
            _creature.Revived += OnRevived;

            ApplyVisualByHp(_creature.CurrentHp);
        }

        private void OnHpChanged(int _, int __) => ApplyVisualByHp(_creature.CurrentHp);
        private void OnDied(Creature _) => ApplyVisualByHp(0);
        private void OnRevived(Creature _) => ApplyVisualByHp(_creature.CurrentHp);

        private void ApplyVisualByHp(int hp)
        {
            bool isDead = hp <= 0;
            _alive.Visible = !isDead;
            _dead.Visible = isDead;
        }
    }
}
