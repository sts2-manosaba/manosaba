using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.Common.Commands
{
    public static class ManosabaVfxCmd
    {
        public static Node2D? PlaySceneAtCombatCenter(string scenePath, bool fitCoverViewport = false, string spriteNodeName = "Still")
        {
            NCombatRoom? combatRoom = NCombatRoom.Instance;
            NGame? game = NGame.Instance;
            if (combatRoom == null || game == null)
            {
                return null;
            }

            Node2D vfxNode = PreloadManager.Cache.GetScene(scenePath).Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
            combatRoom.CombatVfxContainer.AddChildSafely(vfxNode);

            Vector2 viewportSize = game.GetViewportRect().Size;
            vfxNode.GlobalPosition = viewportSize * 0.5f;

            if (fitCoverViewport)
            {
                FitSpriteToViewportCover(vfxNode, viewportSize, spriteNodeName);
            }

            return vfxNode;
        }

        private static void FitSpriteToViewportCover(Node2D vfxNode, Vector2 viewportSize, string spriteNodeName)
        {
            Sprite2D? sprite = vfxNode.GetNodeOrNull<Sprite2D>(spriteNodeName);
            if (sprite?.Texture == null)
            {
                return;
            }

            Vector2 textureSize = sprite.Texture.GetSize();
            if (textureSize.X <= 0 || textureSize.Y <= 0)
            {
                return;
            }

            float coverScale = Mathf.Max(viewportSize.X / textureSize.X, viewportSize.Y / textureSize.Y);
            sprite.Scale = new Vector2(coverScale, coverScale);
        }
    }
}
