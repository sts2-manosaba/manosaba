using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Characters.Common.Overrides
{
	public partial class ManosabaCharacterVisuals : NCreatureVisuals
	{
		private NCreature? _creatureNode;
		private Creature? _creature;

		public override void _Ready()
		{
			base._Ready();

			_creatureNode = GetParent() as NCreature;
			_creature = _creatureNode?.Entity;
			if (_creature?.Player == null)
			{
				return;
			}

			_creature.CurrentHpChanged += OnHpChanged;
			_creature.Died += OnDied;
			_creature.Revived += OnRevived;

			ApplyCombatVisibility();
		}

		public override void _ExitTree()
		{
			if (_creature != null)
			{
				_creature.CurrentHpChanged -= OnHpChanged;
				_creature.Died -= OnDied;
				_creature.Revived -= OnRevived;
			}

			base._ExitTree();
		}

		private void OnHpChanged(int _, int __) => ApplyCombatVisibility();
		private void OnDied(Creature _) => ApplyCombatVisibility();
		private void OnRevived(Creature _) => ApplyCombatVisibility();

		private void ApplyCombatVisibility()
		{
			if (_creatureNode == null || _creature == null)
			{
				return;
			}

			if (RunManager.Instance.IsSinglePlayerOrFakeMultiplayer)
			{
				_creatureNode.Visible = true;
				return;
			}

			_creatureNode.Visible = _creature.IsAlive;
		}
	}
}
