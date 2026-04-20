using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Manosaba.Characters.Common.Commands
{
    public static class ManosabaVfxCmd
    {
        private const float DefaultPollIntervalSeconds = 0.05f;
        private const float DefaultWaitTimeoutSeconds = 30f;

        public static Node2D? PlaySceneAtCombatCenter(string scenePath, bool fitCoverViewport = false, string[]? spriteNodeNames = null)
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
                FitSpriteToViewportCover(vfxNode, viewportSize, spriteNodeNames);
            }

            return vfxNode;
        }

        public static Node2D? PlaySceneAtCombatViewportPosition(
            string scenePath,
            float normalizedX,
            float normalizedY,
            float offsetX = 0f,
            float offsetY = 0f,
            bool fitCoverViewport = false,
            string[]? spriteNodeNames = null)
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
            float clampedX = Mathf.Clamp(normalizedX, 0f, 1f);
            float clampedY = Mathf.Clamp(normalizedY, 0f, 1f);
            vfxNode.GlobalPosition = new Vector2(
                viewportSize.X * clampedX + offsetX,
                viewportSize.Y * clampedY + offsetY);

            if (fitCoverViewport)
            {
                FitSpriteToViewportCover(vfxNode, viewportSize, spriteNodeNames);
            }

            return vfxNode;
        }

        public static async Task<Node2D?> PlaySceneAtCombatCenterAndWait(
            string scenePath,
            bool fitCoverViewport = false,
            string[]? spriteNodeNames = null,
            float timeoutSeconds = DefaultWaitTimeoutSeconds)
        {
            Node2D? vfxNode = PlaySceneAtCombatCenter(scenePath, fitCoverViewport, spriteNodeNames);
            if (vfxNode == null)
            {
                return null;
            }

            float elapsed = 0f;
            while (elapsed < timeoutSeconds && GodotObject.IsInstanceValid(vfxNode) && vfxNode.IsInsideTree())
            {
                await Cmd.Wait(DefaultPollIntervalSeconds);
                elapsed += DefaultPollIntervalSeconds;
            }

            return vfxNode;
        }

        public static async Task<Node2D?> PlaySceneAtCombatViewportPositionAndWait(
            string scenePath,
            float normalizedX,
            float normalizedY,
            float offsetX = 0f,
            float offsetY = 0f,
            bool fitCoverViewport = false,
            string[]? spriteNodeNames = null,
            float timeoutSeconds = DefaultWaitTimeoutSeconds)
        {
            Node2D? vfxNode = PlaySceneAtCombatViewportPosition(
                scenePath,
                normalizedX,
                normalizedY,
                offsetX,
                offsetY,
                fitCoverViewport,
                spriteNodeNames);
            if (vfxNode == null)
            {
                return null;
            }

            float elapsed = 0f;
            while (elapsed < timeoutSeconds && GodotObject.IsInstanceValid(vfxNode) && vfxNode.IsInsideTree())
            {
                await Cmd.Wait(DefaultPollIntervalSeconds);
                elapsed += DefaultPollIntervalSeconds;
            }

            return vfxNode;
        }

        private static void FitSpriteToViewportCover(Node2D vfxNode, Vector2 viewportSize, string[]? spriteNodeNames)
        {
            List<Sprite2D> sprites = [];
            if (spriteNodeNames != null && spriteNodeNames.Length > 0)
            {
                foreach (string spriteNodeName in spriteNodeNames)
                {
                    Sprite2D? sprite = vfxNode.GetNodeOrNull<Sprite2D>(spriteNodeName);
                    if (sprite == null)
                    {
                        continue;
                    }

                    sprites.Add(sprite);
                }
            }
            else
            {
                foreach (Node node in vfxNode.FindChildren("*", "Sprite2D", true, false))
                {
                    if (node is Sprite2D sprite)
                    {
                        sprites.Add(sprite);
                    }
                }
            }

            foreach (Sprite2D sprite in sprites)
            {
                if (sprite.Texture == null)
                {
                    continue;
                }

                Vector2 textureSize = sprite.Texture.GetSize();
                if (textureSize.X <= 0 || textureSize.Y <= 0)
                {
                    continue;
                }

                float coverScale = Mathf.Max(viewportSize.X / textureSize.X, viewportSize.Y / textureSize.Y);
                sprite.Scale = new Vector2(coverScale, coverScale);
            }
        }
    }
}
