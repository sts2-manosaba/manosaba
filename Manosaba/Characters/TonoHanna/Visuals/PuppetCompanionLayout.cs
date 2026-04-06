using Godot;

namespace Manosaba.Characters.TonoHanna.Visuals;

/// <summary>
/// Thirteen slots in local space on <see cref="MegaCrit.Sts2.Core.Nodes.Combat.NCreature"/> (Y up = negative):
/// head row ×5, left-shoulder-left row ×2, right-shoulder-right row ×2, left-hand-left row ×2, right-hand-right row ×2.
/// </summary>
public static class PuppetCompanionLayout
{
    public const int SlotAlisa = 0;
    public const int SlotAnAn = 1;
    public const int SlotCoco = 2;
    public const int SlotEma = 3;
    public const int SlotHanna = 4;
    public const int SlotHiro = 5;
    public const int SlotLeia = 6;
    public const int SlotMargo = 7;
    public const int SlotMeruru = 8;
    public const int SlotMiria = 9;
    public const int SlotNanoka = 10;
    public const int SlotNoah = 11;
    public const int SlotSherry = 12;

    /// <summary>Local positions relative to creature root (same count as <see cref="PuppetCollectionRing.MaxSlots"/>).</summary>
    public static readonly Vector2[] SlotLocalPositions =
    {
        // 頭上橫一排 5：Alisa, AnAn, Coco, Ema, Hanna
        new(-76f, -274f),
        new(-38f, -282f),
        new(0f, -286f),
        new(38f, -282f),
        new(76f, -274f),
        // 左肩左邊一排 2：Hiro, Leia
        new(-138f, -228f),
        new(-100f, -228f),
        // 右肩右邊一排 2：Margo, Meruru
        new(100f, -228f),
        new(138f, -228f),
        // 左手左邊一排 2：Miria, Nanoka
        new(-148f, -178f),
        new(-112f, -178f),
        // 右手右邊一排 2：Noah, Sherry
        new(112f, -178f),
        new(148f, -178f),
    };

    public static Vector2 GetSlotPosition(int slotIndex)
    {
        return SlotLocalPositions[slotIndex];
    }
}
