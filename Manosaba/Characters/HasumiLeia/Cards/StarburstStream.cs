using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class StarburstStream : PathCustomCardModel
{
    protected override bool HasEnergyCostX => true;

    public const string EnergyRequirementVar = "EnergyRequirement";

    private const int energyCost = 0;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    private const int HitCount = 16;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(0m, ValueProp.Move),
        new RepeatVar(HitCount),
        new DynamicVar(EnergyRequirementVar, 5m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.SwordTechnique];

    protected override bool IsPlayable
    {
        get
        {
            if (Owner?.Creature == null || Owner.PlayerCombatState == null)
            {
                return false;
            }

            if (Owner.Creature.GetPowerAmount<SecondSwordPower>() <= 0m)
            {
                return false;
            }

            int requiredEnergy = DynamicVars[EnergyRequirementVar].IntValue;
            return Owner.PlayerCombatState.Energy >= requiredEnergy;
        }
    }

    public StarburstStream()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target)
        {
            return;
        }

        int x = ResolveEnergyXValue();
        DynamicVars.Damage.BaseValue = x;

        // First 14 hits: mix big_slash and flying_slash.
        for (int i = 0; i < 14; i++)
        {
            string vfx = (i % 2 == 0) ? "vfx/vfx_big_slash" : "vfx/vfx_flying_slash";
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .WithHitFx(vfx)
                .FromCard(this)
                .Targeting(target)
                .Execute(choiceContext);
        }

        // Last 2 hits: dramatic thrust.
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(2)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // Lower minimum required energy by 2 (5 -> 3).
        DynamicVars[EnergyRequirementVar].UpgradeValueBy(-2m);
    }
}
