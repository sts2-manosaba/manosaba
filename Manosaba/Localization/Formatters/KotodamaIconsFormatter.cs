using System;
using System.Linq;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;

namespace Manosaba.Localization.Formatters;

public sealed class KotodamaIconsFormatter : IFormatter
{
    private const string Icon = "[img]res://Manosaba/images/characters/natsume_anan/kotodama_energy_text.png[/img]";

    public string Name
    {
        get => "kotodamaIcons";
        set => throw new NotImplementedException();
    }

    public bool CanAutoDetect { get; set; }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        object? currentValue = formattingInfo.CurrentValue;

        int amount;
        DynamicVar? dynamicVar = null;
        if (currentValue is DynamicVar varValue)
        {
            dynamicVar = varValue;
            amount = (int)varValue.PreviewValue;
        }
        else if (currentValue is decimal decimalValue)
        {
            amount = (int)decimalValue;
        }
        else if (currentValue is int intValue)
        {
            amount = intValue;
        }
        else if (currentValue is string)
        {
            if (!int.TryParse(formattingInfo.FormatterOptions, out amount))
            {
                return false;
            }
        }
        else
        {
            throw new LocException($"Unknown value='{formattingInfo.CurrentValue}' type={formattingInfo.CurrentValue?.GetType()}");
        }

        amount = Math.Max(0, amount);
        string output;
        if (amount > 0 && amount < 4)
        {
            output = string.Concat(Enumerable.Repeat(Icon, amount));
        }
        else
        {
            string amountText = dynamicVar == null ? amount.ToString() : dynamicVar.ToHighlightedString(inverse: false);
            output = $"{amountText}{Icon}";
        }

        formattingInfo.Write(output);
        return true;
    }
}
