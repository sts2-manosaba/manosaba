using Godot;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace manosaba.Characters.Common;

public partial class ManosabaRestSiteCharacter : NRestSiteCharacter
{
	private Control? _controlRoot;
	public new void FlipX()
	{
		Vector2 scale;
		foreach (Node2D childSprite2D in GetChildSprite2D())
		{
			scale = childSprite2D.Scale;
			scale.X = 0f - childSprite2D.Scale.X;
			childSprite2D.Scale = scale;
			scale = childSprite2D.Position;
			scale.X = 0f - childSprite2D.Position.X;
			childSprite2D.Position = scale;
		}
		if (_controlRoot != null)
		{
			Control controlRoot = _controlRoot;
			scale = _controlRoot.Scale;
			scale.X = 0f - _controlRoot.Scale.X;
			controlRoot.Scale = scale;
		}
	}

	private IEnumerable<Node2D> GetChildSprite2D()
	{
		foreach (Node2D item in GetChildren().OfType<Node2D>())
		{
			if (!(item.GetClass() != "Sprite2D"))
			{
				yield return item;
			}
		}
	}
}
