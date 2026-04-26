using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.ShitoAlisa;

/// <summary>
/// BaseValue = 燃燒上限；EnchantedValue = 目前倒數。0 代表未啟用燃燒機制。
/// </summary>
public sealed class ShitoCombustDynamicVar : DynamicVar
{
    private decimal _maxCombust;

    public ShitoCombustDynamicVar(decimal maxCombust)
        : base("Combust", maxCombust)
    {
        _maxCombust = maxCombust;
    }

    public decimal MaxCombust => _maxCombust;

    public decimal CurrentCombust
    {
        get => BaseValue;
        set
        {
            BaseValue = value;
            PreviewValue = value;
            EnchantedValue = value;
        }
    }

    public void SetCombustFromExternal(int max)
    {
        _maxCombust = max;
        CurrentCombust = max;
    }

    public void TickOnDraw()
    {
        if (_maxCombust <= 0m)
            return;
        CurrentCombust = Math.Max(0m, CurrentCombust - 1m);
    }

    public void ApplyIgnite(decimal amount)
    {
        if (_maxCombust <= 0m)
            return;
        CurrentCombust = Math.Max(0m, CurrentCombust - amount);
    }

    public void ResetCurrentToMax()
    {
        CurrentCombust = _maxCombust;
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, MegaCrit.Sts2.Core.Entities.Creatures.Creature? target, bool runGlobalHooks)
    {
        PreviewValue = EnchantedValue;
    }

    public override string ToString()
    {
        if (_maxCombust <= 0m)
            return "0";
        return ((int)CurrentCombust).ToString();
    }
}
