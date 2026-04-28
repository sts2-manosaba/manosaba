using MegaCrit.Sts2.Core.Entities.Players;

namespace Manosaba.Characters.Common.Resources;

public interface ICustomEnergySaveCarrier
{
    CharacterCustomEnergyDefinition SavedEnergyDefinition { get; }

    int SavedCustomEnergyValue { get; set; }

    Player? SavedCustomEnergyOwner { get; }
}
