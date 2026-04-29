using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SaekiMiria.Helper;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class LuckTransfer : PathCustomCardModel
{
    private const string multiplierVar = "Multiplier";
    private const decimal debuffMultiplier = 2m;
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(multiplierVar, debuffMultiplier)];

    public LuckTransfer()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is not { } ownerCreature || cardPlay.Target is not { } target)
        {
            return;
        }

        List<PowerModel> originalDebuffs = ownerCreature.Powers
            .Where(p => p.TypeForCurrentAmount == PowerType.Debuff)
            .Where(MiriaConstants.IsAllowedLuckTransferPower)
            .Select(p => (PowerModel)p.ClonePreservingMutability())
            .ToList();

        if (originalDebuffs.Count == 0)
        {
            return;
        }

        foreach (PowerModel item in originalDebuffs)
        {
            decimal scaledAmount = item.Amount * debuffMultiplier;
            if (scaledAmount <= 0m)
            {
                continue;
            }

            PowerModel? powerById = target.GetPowerById(item.Id);
            if (powerById != null && !powerById.IsInstanced)
            {
                DoHackyThingsForSpecificPowers(powerById);
                await PowerCmd.ModifyAmount(powerById, scaledAmount, ownerCreature, this);
            }
            else
            {
                PowerModel power = (PowerModel)item.ClonePreservingMutability();
                DoHackyThingsForSpecificPowers(power);
                await PowerCmd.Apply(power, target, scaledAmount, ownerCreature, this);
            }
        }
    }

    private static void DoHackyThingsForSpecificPowers(PowerModel power)
    {
        if (power is ITemporaryPower temporaryPower)
        {
            temporaryPower.IgnoreNextInstance();
        }
        else if (power is ManosabaTemporaryStrengthPower temporaryStrengthPower)
        {
            temporaryStrengthPower.IgnoreNextInstance();
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
