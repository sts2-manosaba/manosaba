using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SawatariCoco.Equipment;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.SawatariCoco.Powers;

/// <summary>Resolves <see cref="StringVar.StringValue"/> as a cards.json title entry at display time.</summary>
public sealed class EquipmentPieceNameVar : StringVar
{
    public EquipmentPieceNameVar(string name)
        : base(name)
    {
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(StringValue))
        {
            return string.Empty;
        }

        return new LocString("cards", $"{StringValue}.title").GetFormattedText();
    }
}

public abstract class EquipmentSlotPowerBase : PathCustomPowerModel
{
    public const string PieceNameVar = "EquippedPieceName";

    protected abstract EquipmentSlot Slot { get; }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EquipmentPieceNameVar(PieceNameVar),
        new DynamicVar("SlotScore", 0m),
    ];

    public EquipmentSeries EquippedSeries { get; private set; } = EquipmentSeries.None;

    public int SlotScore => (int)DynamicVars["SlotScore"].BaseValue;

    public void SetEquippedPiece(EquipmentSeries series, string pieceTitleLocEntry, int score)
    {
        EquippedSeries = series;
        DynamicVars["SlotScore"].BaseValue = score;
        if (DynamicVars[PieceNameVar] is StringVar nameVar)
        {
            nameVar.StringValue = pieceTitleLocEntry;
        }
    }

    public void ClearSlot()
    {
        SetEquippedPiece(EquipmentSeries.None, string.Empty, 0);
    }
}

public sealed class EquipmentHeadwearSlotPower : EquipmentSlotPowerBase
{
    protected override EquipmentSlot Slot => EquipmentSlot.Headwear;
}

public sealed class EquipmentTopSlotPower : EquipmentSlotPowerBase
{
    protected override EquipmentSlot Slot => EquipmentSlot.Top;
}

public sealed class EquipmentSkirtSlotPower : EquipmentSlotPowerBase
{
    protected override EquipmentSlot Slot => EquipmentSlot.Skirt;
}

public sealed class EquipmentShoesSlotPower : EquipmentSlotPowerBase
{
    protected override EquipmentSlot Slot => EquipmentSlot.Shoes;
}

/// <summary>Tracks total equipment score and per-combat score milestone rewards.</summary>
public sealed class EquipmentScorePower : PathCustomPowerModel
{
    private bool _milestone3000Triggered;
    private bool _milestone6000Triggered;
    private bool _milestone10000Triggered;
    private int _highestScoreThisCombat;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HighestScore", 0m),
    ];

    public int HighestScoreThisCombat => _highestScoreThisCombat;

    public void ResetForCombat()
    {
        _milestone3000Triggered = false;
        _milestone6000Triggered = false;
        _milestone10000Triggered = false;
        _highestScoreThisCombat = 0;
        DynamicVars["HighestScore"].BaseValue = 0;
    }

    public async Task UpdateTotalScoreAsync(PlayerChoiceContext choiceContext, int totalScore, CardModel? source)
    {
        if (totalScore > _highestScoreThisCombat)
        {
            _highestScoreThisCombat = totalScore;
            DynamicVars["HighestScore"].BaseValue = totalScore;
        }

        decimal delta = totalScore - Amount;
        if (delta != 0m)
        {
            await CommonActions.Apply<EquipmentScorePower>(choiceContext, Owner, source, delta);
        }

        if (totalScore >= 3000 && !_milestone3000Triggered)
        {
            _milestone3000Triggered = true;
            await CommonActions.Apply<DexterityPower>(choiceContext, Owner, source, 1m);
        }

        if (totalScore >= 6000 && !_milestone6000Triggered)
        {
            _milestone6000Triggered = true;
            await CommonActions.Apply<StrengthPower>(choiceContext, Owner, source, 1m);
        }

        if (totalScore >= 10000 && !_milestone10000Triggered)
        {
            _milestone10000Triggered = true;
            await CommonActions.Apply<MajokaPower>(choiceContext, Owner, source, 20m);
        }
    }
}

/// <summary>Tracks which full-set bonuses already triggered this combat.</summary>
public sealed class EquipmentSetBonusPower : PathCustomPowerModel
{
    private bool _punkCatTriggered;
    private bool _cybercatTriggered;
    private bool _mysteriousCatTriggered;
    private bool _cutieCatsTriggered;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

    public void ResetForCombat()
    {
        _punkCatTriggered = false;
        _cybercatTriggered = false;
        _mysteriousCatTriggered = false;
        _cutieCatsTriggered = false;
    }

    public bool HasTriggered(EquipmentSeries series) => series switch
    {
        EquipmentSeries.PunkCat => _punkCatTriggered,
        EquipmentSeries.Cybercat => _cybercatTriggered,
        EquipmentSeries.MysteriousCat => _mysteriousCatTriggered,
        EquipmentSeries.CutieCats => _cutieCatsTriggered,
        _ => true,
    };

    public void MarkTriggered(EquipmentSeries series)
    {
        switch (series)
        {
            case EquipmentSeries.PunkCat:
                _punkCatTriggered = true;
                break;
            case EquipmentSeries.Cybercat:
                _cybercatTriggered = true;
                break;
            case EquipmentSeries.MysteriousCat:
                _mysteriousCatTriggered = true;
                break;
            case EquipmentSeries.CutieCats:
                _cutieCatsTriggered = true;
                break;
        }
    }
}

/// <summary>Full-set bonus from Mysterious Cat: +2 max energy for rest of combat.</summary>
public sealed class MysteriousCatSetEnergyPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    public override decimal ModifyMaxEnergy(MegaCrit.Sts2.Core.Entities.Players.Player player, decimal amount)
    {
        if (Owner.Player == null || player != Owner.Player)
        {
            return amount;
        }

        return amount + Amount;
    }
}
