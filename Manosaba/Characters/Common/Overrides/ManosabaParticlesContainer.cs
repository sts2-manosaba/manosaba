using Godot;
using Godot.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;

namespace Manosaba.Characters.Common.Overrides
{
	public partial class ManosabaParticlesContainer : NParticlesContainer
	{
		public override void _Ready()
		{
			base._Ready();
			Traverse.Create(this).Field("_particles").SetValue(new Array<GpuParticles2D>());
		}
	}
}
