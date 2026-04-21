using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;

namespace Manosaba.Characters.JogasakiNoah.Cards
{
    public sealed class PaletteGapPlayCardAction : GameAction
    {
        private CardModel? _card;

        public override ulong OwnerId => Player.NetId;

        public override GameActionType ActionType => GameActionType.CombatPlayPhaseOnly;

        public Player Player { get; }

        public NetCombatCard NetCombatCard { get; }

        public ModelId CardModelId { get; }

        public uint? TargetId { get; }

        public int? InsertIndex { get; }

        public PlayerChoiceContext? PlayerChoiceContext { get; private set; }

        public Creature? Target => Player.Creature.CombatState?.GetCreature(TargetId);

        public PaletteGapPlayCardAction(CardModel cardModel, Creature? target, int? insertIndex)
        {
            if (target != null && !target.CombatId.HasValue)
            {
                throw new InvalidOperationException($"Cannot target card against target {target} with no combat ID!");
            }

            Player = cardModel.Owner;
            NetCombatCard = NetCombatCard.FromModel(cardModel);
            CardModelId = cardModel.Id;
            TargetId = target?.CombatId;
            InsertIndex = insertIndex;
        }

        public PaletteGapPlayCardAction(Player player, NetCombatCard netCombatCard, ModelId cardModelId, uint? targetId, int? insertIndex)
        {
            Player = player;
            NetCombatCard = netCombatCard;
            CardModelId = cardModelId;
            TargetId = targetId;
            InsertIndex = insertIndex;
        }

        protected override async Task ExecuteAction()
        {
            _card = NetCombatCard.ToCardModel();
            Creature target = await Player.Creature.CombatState.GetCreatureAsync(TargetId, 10.0);
            CardPile? pile = _card.Pile;
            if (pile == null || pile.Type != PileType.Hand)
            {
                return;
            }

            bool missingRequiredTarget = target == null && (_card.TargetType == TargetType.AnyEnemy || _card.TargetType == TargetType.AnyAlly);
            if (missingRequiredTarget)
            {
                Log.Warn($"Attempted to play card {_card} with TargetType of type 'Any', but no target was passed to the play card action!");
            }

            if (!_card.CanPlay(out UnplayableReason _, out AbstractModel _) || !_card.IsValidTarget(target))
            {
                Cancel();
                return;
            }

            string targetDesc = target != null
                ? $"targeting {target.LogName} (index {Player.Creature.CombatState?.Creatures.IndexOf(target)})"
                : "no target";
            Log.Info($"Player {_card.Owner.NetId} playing card {_card.Id.Entry} ({targetDesc})");

            if (_card is PaletteGap paletteGap && InsertIndex.HasValue)
            {
                paletteGap.PendingInsertIndex = Math.Max(0, InsertIndex.Value);
            }

            (int energySpent, int starsSpent) = await _card.SpendResources();
            ResourceInfo resources = new()
            {
                EnergySpent = energySpent,
                EnergyValue = energySpent,
                StarsSpent = starsSpent,
                StarValue = starsSpent
            };

            PlayerChoiceContext = new GameActionPlayerChoiceContext(this);
            await _card.OnPlayWrapper(PlayerChoiceContext, target, isAutoPlay: false, resources);
        }

        protected override void CancelAction()
        {
            if (TestMode.IsOn && !RunManager.Instance.IsInProgress)
            {
                return;
            }

            _card ??= NetCombatCard.ToCardModelOrNull();
            if (_card != null)
            {
                if (LocalContext.IsMe(Player))
                {
                    NPlayerHand.Instance?.TryCancelCardPlay(_card);
                }
            }
        }

        public override INetAction ToNetAction()
        {
            return new NetPaletteGapPlayCardAction
            {
                card = NetCombatCard,
                modelId = CardModelId,
                targetId = TargetId,
                insertIndex = InsertIndex
            };
        }

        public override string ToString()
        {
            CardModel value = NetCombatCard.ToCardModelOrNull();
            return $"{nameof(PaletteGapPlayCardAction)} card: {value} index: {NetCombatCard.CombatCardIndex} targetid: {TargetId} insert: {InsertIndex}";
        }
    }

    public struct NetPaletteGapPlayCardAction : INetAction, IPacketSerializable
    {
        public NetCombatCard card;

        public ModelId modelId;

        public uint? targetId;

        public int? insertIndex;

        public GameAction ToGameAction(Player player)
        {
            return new PaletteGapPlayCardAction(player, card, modelId, targetId, insertIndex);
        }

        public void Serialize(PacketWriter writer)
        {
            writer.Write(card);
            writer.WriteModelEntry(modelId);
            writer.WriteBool(targetId.HasValue);
            if (targetId.HasValue)
            {
                writer.WriteUInt(targetId.GetValueOrDefault(), 6);
            }

            writer.WriteBool(insertIndex.HasValue);
            if (insertIndex.HasValue)
            {
                writer.WriteInt(insertIndex.Value);
            }
        }

        public void Deserialize(PacketReader reader)
        {
            card = reader.Read<NetCombatCard>();
            modelId = reader.ReadModelIdAssumingType<CardModel>();
            if (reader.ReadBool())
            {
                targetId = reader.ReadUInt(6);
            }
            else
            {
                targetId = null;
            }

            if (reader.ReadBool())
            {
                insertIndex = reader.ReadInt();
            }
            else
            {
                insertIndex = null;
            }
        }

        public override string ToString()
        {
            return $"{nameof(NetPaletteGapPlayCardAction)} ({card}) target: {targetId?.ToString() ?? "null"} insert: {insertIndex?.ToString() ?? "null"}";
        }
    }
}
