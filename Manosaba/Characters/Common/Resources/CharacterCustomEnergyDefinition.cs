using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;

namespace Manosaba.Characters.Common.Resources;

public abstract class CharacterCustomEnergyDefinition
{
    public abstract string EnergyId { get; }

    public abstract string CharacterId { get; }

    public virtual int InitialEnergyPerCombat => 0;

    public virtual int TurnStartEnergyGain => 0;

    public virtual int MinEnergy => 0;

    public virtual int MaxEnergy => 999;

    public virtual string NotEnoughMessageTable => "combat_messages";

    public virtual string NotEnoughMessageKey => "NOT_ENOUGH_STARS";

    public virtual string NotEnoughFallbackText => "Not enough resources.";

    public virtual LocString GetNotEnoughMessageLocString()
    {
        return new LocString(NotEnoughMessageTable, NotEnoughMessageKey);
    }

    public virtual int Clamp(int value)
    {
        return Math.Clamp(value, MinEnergy, MaxEnergy);
    }

    public bool AppliesTo(Player? player)
    {
        string? entry = player?.Character?.Id.Entry;
        if (string.IsNullOrEmpty(entry))
        {
            return false;
        }

        return entry.EndsWith(CharacterId, StringComparison.OrdinalIgnoreCase);
    }
}
