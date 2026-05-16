using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace Manosaba.Characters.HikamiMeruru.PotionCraft;

public sealed class PotionCraftGameAction : GameAction
{
    public override ulong OwnerId => Player.NetId;

    public override GameActionType ActionType
    {
        get
        {
            if (!WasEnqueuedInCombat)
            {
                return GameActionType.NonCombat;
            }

            return GameActionType.CombatPlayPhaseOnly;
        }
    }

    public Player Player { get; }

    public uint FirstPotionSlotIndex { get; }

    public uint SecondPotionSlotIndex { get; }

    public bool WasEnqueuedInCombat { get; }

    public PotionCraftGameAction(Player player, uint firstPotionSlotIndex, uint secondPotionSlotIndex, bool wasEnqueuedInCombat)
    {
        Player = player;
        FirstPotionSlotIndex = firstPotionSlotIndex;
        SecondPotionSlotIndex = secondPotionSlotIndex;
        WasEnqueuedInCombat = wasEnqueuedInCombat;
    }

    protected override async Task ExecuteAction()
    {
        PotionModel? first = Player.GetPotionAtSlotIndex((int)FirstPotionSlotIndex);
        PotionModel? second = Player.GetPotionAtSlotIndex((int)SecondPotionSlotIndex);
        if (first == null || second == null || PotionCraftService.FindRecipeForPair(first, second) == null)
        {
            Cancel();
            return;
        }

        await PotionCraftService.TryCraftPair(Player, first, second);
    }

    protected override void CancelAction()
    {
        if (TestMode.IsOff && NRun.Instance != null && LocalContext.IsMe(Player))
        {
            PotionModel? first = Player.GetPotionAtSlotIndex((int)FirstPotionSlotIndex);
            PotionModel? second = Player.GetPotionAtSlotIndex((int)SecondPotionSlotIndex);
            if (first != null)
            {
                NRun.Instance.GlobalUi.TopBar.PotionContainer.OnPotionUseOrDiscardCanceled(first);
            }

            if (second != null)
            {
                NRun.Instance.GlobalUi.TopBar.PotionContainer.OnPotionUseOrDiscardCanceled(second);
            }
        }
    }

    public override INetAction ToNetAction()
    {
        return new NetPotionCraftGameAction
        {
            firstPotionSlotIndex = FirstPotionSlotIndex,
            secondPotionSlotIndex = SecondPotionSlotIndex,
            wasEnqueuedInCombat = WasEnqueuedInCombat
        };
    }

    public override string ToString()
    {
        return $"{nameof(PotionCraftGameAction)} for player {Player.NetId} slots: {FirstPotionSlotIndex}, {SecondPotionSlotIndex} in combat: {WasEnqueuedInCombat}";
    }
}

public struct NetPotionCraftGameAction : INetAction, IPacketSerializable
{
    public uint firstPotionSlotIndex;

    public uint secondPotionSlotIndex;

    public bool wasEnqueuedInCombat;

    public GameAction ToGameAction(Player player)
    {
        return new PotionCraftGameAction(player, firstPotionSlotIndex, secondPotionSlotIndex, wasEnqueuedInCombat);
    }

    public void Serialize(PacketWriter writer)
    {
        writer.WriteUInt(firstPotionSlotIndex, 4);
        writer.WriteUInt(secondPotionSlotIndex, 4);
        writer.WriteBool(wasEnqueuedInCombat);
    }

    public void Deserialize(PacketReader reader)
    {
        firstPotionSlotIndex = reader.ReadUInt(4);
        secondPotionSlotIndex = reader.ReadUInt(4);
        wasEnqueuedInCombat = reader.ReadBool();
    }

    public override string ToString()
    {
        return $"{nameof(NetPotionCraftGameAction)} slots {firstPotionSlotIndex}, {secondPotionSlotIndex} in combat: {wasEnqueuedInCombat}";
    }
}
