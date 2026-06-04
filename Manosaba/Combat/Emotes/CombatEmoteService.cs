using BaseLib.Abstracts;
using Godot;
using Manosaba.Multiplayer.Messages.Game;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Manosaba.Combat.Emotes;

public static class CombatEmoteService
{
    private static readonly Dictionary<ulong, NCombatEmotePopup> ActivePopupsByNetId = new();

    public static void SendLocal(string stickerId)
    {
        if (!CanUseCombatEmotes())
        {
            return;
        }

        Texture2D? texture = EmoteCatalog.TryGetTexture(stickerId);
        if (texture == null)
        {
            GD.PushWarning($"[Manosaba] Unknown emote sticker id: {stickerId}");
            return;
        }

        Player? me = TryGetLocalPlayer();
        if (me == null)
        {
            return;
        }

        ShowForPlayer(me, texture);

        RunManager runManager = RunManager.Instance;
        if (runManager.NetService?.Type.IsMultiplayer() == true)
        {
            CustomMessageWrapper.Send(new CombatEmoteMessage { StickerId = stickerId }, runManager.NetService);
        }
    }

    public static void ShowFromNetwork(ulong senderId, string stickerId)
    {
        if (!CanUseCombatEmotes())
        {
            return;
        }

        Player? localPlayer = TryGetLocalPlayer();
        if (localPlayer != null && localPlayer.NetId == senderId)
        {
            return;
        }

        Texture2D? texture = EmoteCatalog.TryGetTexture(stickerId);
        if (texture == null)
        {
            GD.PushWarning($"[Manosaba] Received unknown emote sticker id from peer: {stickerId}");
            return;
        }

        Player? sender = ResolvePlayer(senderId);
        if (sender == null)
        {
            return;
        }

        ShowForPlayer(sender, texture);
    }

    public static void ClearActivePopups()
    {
        foreach (NCombatEmotePopup popup in ActivePopupsByNetId.Values)
        {
            if (GodotObject.IsInstanceValid(popup) && !popup.IsQueuedForDeletion())
            {
                popup.QueueFreeSafely();
            }
        }

        ActivePopupsByNetId.Clear();
    }

    private static void ShowForPlayer(Player player, Texture2D texture)
    {
        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null)
        {
            return;
        }

        Vector2 spawnPosition = GetEmoteSpawnPosition(player);
        if (spawnPosition == Vector2.Zero)
        {
            return;
        }

        if (ActivePopupsByNetId.TryGetValue(player.NetId, out NCombatEmotePopup? existing) &&
            GodotObject.IsInstanceValid(existing) &&
            !existing.IsQueuedForDeletion())
        {
            existing.QueueFreeSafely();
            ActivePopupsByNetId.Remove(player.NetId);
        }

        NCombatEmotePopup popup = NCombatEmotePopup.Create(texture);
        room.CombatVfxContainer.AddChildSafely(popup);
        popup.PlayAt(spawnPosition);
        ActivePopupsByNetId[player.NetId] = popup;
        popup.TreeExited += () => ActivePopupsByNetId.Remove(player.NetId);
    }

    private static Vector2 GetEmoteSpawnPosition(Player player)
    {
        Creature? creature = player.Creature;
        if (creature == null)
        {
            return Vector2.Zero;
        }

        NCombatRoom? room = NCombatRoom.Instance;
        if (room == null)
        {
            return Vector2.Zero;
        }

        NCreature? creatureNode = room.GetCreatureNode(creature);
        if (creatureNode == null)
        {
            return Vector2.Zero;
        }

        Vector2 position = creatureNode.Visuals.IntentPosition.GlobalPosition;
        position.Y -= 80f;
        return position;
    }

    private static Player? ResolvePlayer(ulong netId)
    {
        CombatState? state = CombatManager.Instance.DebugOnlyGetState();
        if (state == null)
        {
            return null;
        }

        return state.GetPlayer(netId);
    }

    private static Player? TryGetLocalPlayer()
    {
        CombatState? state = CombatManager.Instance.DebugOnlyGetState();
        if (state == null)
        {
            return null;
        }

        try
        {
            return LocalContext.GetMe(state);
        }
        catch (Exception)
        {
            return state.Players.Count == 1 ? state.Players[0] : null;
        }
    }

    private static bool CanUseCombatEmotes()
    {
        RunManager runManager = RunManager.Instance;
        return runManager.IsInProgress &&
               runManager.NetService?.Type.IsMultiplayer() == true &&
               NCombatRoom.Instance != null;
    }
}
