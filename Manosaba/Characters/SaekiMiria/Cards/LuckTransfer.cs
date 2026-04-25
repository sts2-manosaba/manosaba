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
    private const int energyCost = 1;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Uncommon;
    private const TargetType TargetTypeValue = TargetType.Self;
    private const bool ShouldShowInCardLibrary = true;
    private const decimal DebuffMultiplier = 3m;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Multiplier", DebuffMultiplier)];

    public LuckTransfer()
        : base(energyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        List<PowerModel> originalDebuffs = Owner.Creature.Powers
            .Where(p => p.TypeForCurrentAmount == PowerType.Debuff)
            .Where(MiriaConstants.IsAllowedLuckTransferPower)
            .Select(p => (PowerModel)p.ClonePreservingMutability())
            .ToList();

        if (originalDebuffs.Count == 0)
        {
            return;
        }

        foreach (Creature enemy in CombatState.HittableEnemies)
        {
            foreach (PowerModel item in originalDebuffs)
            {
                decimal scaledAmount = item.Amount * DynamicVars["Multiplier"].BaseValue;
                if (scaledAmount <= 0m)
                {
                    continue;
                }

                PowerModel? powerById = enemy.GetPowerById(item.Id);
                if (powerById != null && !powerById.IsInstanced)
                {
                    DoHackyThingsForSpecificPowers(powerById);
                    await PowerCmd.ModifyAmount(powerById, scaledAmount, Owner.Creature, this);
                }
                else
                {
                    PowerModel power = (PowerModel)item.ClonePreservingMutability();
                    DoHackyThingsForSpecificPowers(power);
                    await PowerCmd.Apply(power, enemy, scaledAmount, Owner.Creature, this);
                }
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
        DynamicVars["Multiplier"].UpgradeValueBy(2m);
    }
}
