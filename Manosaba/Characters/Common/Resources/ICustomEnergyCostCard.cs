namespace Manosaba.Characters.Common.Resources;

public interface ICustomEnergyCostCard
{
    CharacterCustomEnergyDefinition GetCustomEnergyDefinitionForPlay();

    int GetCustomEnergyCostForPlay();

    bool HasEnoughCustomEnergyForPlay();

    bool TrySpendCustomEnergyForPlay();
}
