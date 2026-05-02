using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Exceptions;

namespace Manosaba.Characters.Common.Resources;

public abstract class CharacterCustomEnergyCardModel<TEnergy> : PathCustomCardModel, ICustomEnergyCostCard
    where TEnergy : CharacterCustomEnergyDefinition
{
    protected abstract TEnergy EnergyDefinition { get; }

    protected CharacterCustomEnergyCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        CharacterCustomEnergyService.Register(EnergyDefinition);
    }

    protected virtual string CustomEnergyCostVarName => "CustomEnergyCost";

    protected int GetCustomEnergyCost()
    {
        return DynamicVars.TryGetValue(CustomEnergyCostVarName, out var costVar)
            ? Math.Max(0, costVar.IntValue)
            : 0;
    }

    protected bool TrySpendCustomEnergyOnPlay()
    {
        int cost = GetEffectiveCustomEnergyCost();
        if (cost <= 0)
        {
            return true;
        }

        Player? owner = TryGetOwnerForCustomEnergyChecks();
        if (owner == null)
        {
            return false;
        }

        return CharacterCustomEnergyService.TrySpend(owner, EnergyDefinition, cost);
    }

    protected bool HasEnoughCustomEnergy()
    {
        int cost = GetEffectiveCustomEnergyCost();
        if (cost <= 0)
        {
            return true;
        }

        Player? owner = TryGetOwnerForCustomEnergyChecks();
        if (owner == null)
        {
            // Preview/library cards may not have a runtime owner yet.
            return true;
        }

        return CharacterCustomEnergyService.HasEnough(owner, EnergyDefinition, cost);
    }

    protected override bool ShouldGlowRedInternal => !HasEnoughCustomEnergy();

    public CharacterCustomEnergyDefinition GetCustomEnergyDefinitionForPlay()
    {
        return EnergyDefinition;
    }

    public int GetCustomEnergyCostForPlay()
    {
        return GetEffectiveCustomEnergyCost();
    }

    public bool HasEnoughCustomEnergyForPlay()
    {
        return HasEnoughCustomEnergy();
    }

    public bool TrySpendCustomEnergyForPlay()
    {
        return TrySpendCustomEnergyOnPlay();
    }

    private Player? TryGetOwnerForCustomEnergyChecks()
    {
        try
        {
            Player owner = Owner;
            if (owner.Character == null || owner.RunState == null)
            {
                return null;
            }

            return owner;
        }
        catch (CanonicalModelException)
        {
            return null;
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    private int GetEffectiveCustomEnergyCost()
    {
        return CustomEnergyFreeCost.IsFree(this) ? 0 : GetCustomEnergyCost();
    }
}
