using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class StarSplash : PathCustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    private const string MidThrustDamageVar = "MidThrustDamage";
    private const string SweepDamageVar = "SweepDamage";
    private const string RisingSlashDamageVar = "RisingSlashDamage";
    private const string UpperThrustDamageVar = "UpperThrustDamage";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(MidThrustDamageVar, 2m),
        new DynamicVar(SweepDamageVar, 3m),
        new DynamicVar(RisingSlashDamageVar, 5m),
        new DynamicVar(UpperThrustDamageVar, 3m),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.SwordTechnique];

    public StarSplash()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars[MidThrustDamageVar].BaseValue)
            .WithHitCount(3)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        await DamageCmd.Attack(DynamicVars[SweepDamageVar].BaseValue)
            .WithHitCount(2)
            .WithHitFx("vfx/vfx_big_slash")
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        await DamageCmd.Attack(DynamicVars[RisingSlashDamageVar].BaseValue)
            .WithHitFx("vfx/vfx_flying_slash")
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);

        await DamageCmd.Attack(DynamicVars[UpperThrustDamageVar].BaseValue)
            .WithHitCount(2)
            .WithHitFx("vfx/vfx_dramatic_stab", null, "blunt_attack.mp3")
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[MidThrustDamageVar].UpgradeValueBy(1m);
        DynamicVars[SweepDamageVar].UpgradeValueBy(1m);
        DynamicVars[RisingSlashDamageVar].UpgradeValueBy(1m);
        DynamicVars[UpperThrustDamageVar].UpgradeValueBy(1m);
    }
}
