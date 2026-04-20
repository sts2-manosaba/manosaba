using Manosaba.Characters.Common.Resources;
using MegaCrit.Sts2.Core.Entities.Players;

namespace manosaba.Characters.NatsumeAnan;

public sealed class KotodamaEnergy : CharacterCustomEnergyDefinition
{
    public static KotodamaEnergy Instance { get; } = new();

    public override string EnergyId => "kotodama_energy";

    public override string CharacterId => NatsumeAnan.CharacterId;

    public override int InitialEnergyPerCombat => 0;

    public override int TurnStartEnergyGain => 0;

    public override int MaxEnergy => 999;

    public override string NotEnoughMessageTable => "static_hover_tips";

    public override string NotEnoughMessageKey => "MANOSABA-KOTODAMA_NOT_ENOUGH";

    public override string NotEnoughFallbackText => "I don't have enough Kotodama.";

    private KotodamaEnergy()
    {
    }

    public static void Register()
    {
        CharacterCustomEnergyService.Register(Instance);
    }

    public static int Get(Player player)
    {
        return CharacterCustomEnergyService.Get(player, Instance);
    }

    public static int Gain(Player player, int amount)
    {
        return CharacterCustomEnergyService.Gain(player, Instance, amount);
    }

    public static int Lose(Player player, int amount)
    {
        return CharacterCustomEnergyService.Lose(player, Instance, amount);
    }

    public static bool HasEnough(Player player, int amount)
    {
        return CharacterCustomEnergyService.HasEnough(player, Instance, amount);
    }

    public static bool TrySpend(Player player, int amount)
    {
        return CharacterCustomEnergyService.TrySpend(player, Instance, amount);
    }
}
