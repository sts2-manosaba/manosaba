using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Characters.JogasakiNoah.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.JogasakiNoah.Cards;

public abstract class SekketsusoujitsuAttackCard : PathCustomCardModel
{
    protected SekketsusoujitsuAttackCard(int energyCost, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary)
        : base(energyCost, CardType.Attack, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Sekketsusoujitsu];

    protected static decimal BloodDamageMultiplier(CardModel card, decimal baseDamage)
    {
        return SekketsusoujitsuHelper.BloodOrbDamageBonus(card)
            + SekketsusoujitsuHelper.RedScaleDamageBonus(card, baseDamage);
    }

    protected async Task<decimal> ConsumeBloodOrbDamageBonus(PlayerChoiceContext choiceContext)
    {
        return await SekketsusoujitsuHelper.EvokeNextBloodOrb(choiceContext, Owner);
    }

    protected decimal ApplyRedScale(decimal baseDamage)
    {
        return baseDamage + SekketsusoujitsuHelper.RedScaleDamageBonus(this, baseDamage);
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class PiercingBlood : SekketsusoujitsuAttackCard
{
    private const int BaseDamage = 30;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Sekketsusoujitsu, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(BaseDamage),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) => BloodDamageMultiplier(card, card.DynamicVars.CalculationBase.BaseValue))
    ];

    public PiercingBlood() : base(1, CardRarity.Token, TargetType.AnyEnemy, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        decimal bloodBonus = await ConsumeBloodOrbDamageBonus(choiceContext);
        await DamageCmd.Attack(ApplyRedScale(DynamicVars.CalculationBase.BaseValue) + bloodBonus)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(15);
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class SuperNova : SekketsusoujitsuAttackCard
{
    private const int BaseDamage = 4;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Sekketsusoujitsu, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(BaseDamage),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) => BloodDamageMultiplier(card, card.DynamicVars.CalculationBase.BaseValue)),
        new RepeatVar(4)
    ];

    public SuperNova() : base(2, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        decimal bloodBonus = await ConsumeBloodOrbDamageBonus(choiceContext);
        int repeat = DynamicVars.Repeat.IntValue + (Owner.Creature.HasPower<SekirinyakudouPower>() ? 2 : 0);
        await DamageCmd.Attack(ApplyRedScale(DynamicVars.CalculationBase.BaseValue) + bloodBonus)
            .WithHitCount(repeat)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2);
    }
}

[Pool(typeof(JogasakiNoahCardPool))]
public sealed class Sekirinyakudou : PathCustomCardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<SekirinyakudouPower>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Sekketsusoujitsu];

    public Sekirinyakudou() : base(3, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return PowerCmd.Apply<SekirinyakudouPower>(Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(JogasakiNoahCardPool))]
public sealed class Kesseiseki : SekketsusoujitsuAttackCard
{
    private const int BaseDamage = 20;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(BaseDamage),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) => BloodDamageMultiplier(card, card.DynamicVars.CalculationBase.BaseValue))
    ];

    public Kesseiseki() : base(2, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        decimal bloodBonus = await ConsumeBloodOrbDamageBonus(choiceContext);
        await DamageCmd.Attack(ApplyRedScale(DynamicVars.CalculationBase.BaseValue) + bloodBonus)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(10);
    }
}

[Pool(typeof(JogasakiNoahCardPool))]
public sealed class Sekibaku : PathCustomCardModel
{
    protected override bool IsPlayable => SekketsusoujitsuHelper.HasBloodOrb(Owner);
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<BloodOrb>(), HoverTipFactory.FromPower<StrengthPower>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Sekketsusoujitsu];

    public Sekibaku() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        decimal strengthLoss = await SekketsusoujitsuHelper.EvokeNextBloodOrb(choiceContext, Owner);
        if (strengthLoss > 0m)
        {
            await PowerCmd.Apply<TemporaryStrengthDownPower>(cardPlay.Target, strengthLoss, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(JogasakiNoahCardPool))]
public sealed class Karibarai : SekketsusoujitsuAttackCard
{
    private const int BaseDamage = 8;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(BaseDamage),
        new ExtraDamageVar(1m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) => BloodDamageMultiplier(card, card.DynamicVars.CalculationBase.BaseValue))
    ];

    public Karibarai() : base(1, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal bloodBonus = await ConsumeBloodOrbDamageBonus(choiceContext);
        decimal damage = ApplyRedScale(DynamicVars.CalculationBase.BaseValue) + bloodBonus;
        if (Owner.Creature.HasPower<SekirinyakudouPower>() && CombatState != null)
        {
            await DamageCmd.Attack(damage)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .Execute(choiceContext);
            return;
        }

        if (cardPlay.Target == null)
        {
            return;
        }

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(4);
    }
}

[Pool(typeof(JogasakiNoahCardPool))]
public sealed class Hyakuren : PathCustomCardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<PiercingBlood>(),
        HoverTipFactory.FromCard<SuperNova>(),
        HoverTipFactory.FromOrb<BloodOrb>()
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Sekketsusoujitsu];

    public Hyakuren() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        List<CardModel> options =
        [
            CombatState.CreateCard<PiercingBlood>(Owner),
            CombatState.CreateCard<SuperNova>(Owner)
        ];

        if (IsUpgraded)
        {
            foreach (CardModel option in options)
            {
                CardCmd.Upgrade(option);
            }
        }

        CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner);
        if (selected != null)
        {
            await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, true);
        }

        SekketsusoujitsuHelper.AddLayersToBloodOrbs(Owner, 2m);
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(JogasakiNoahCardPool))]
public sealed class Ketsujin : PathCustomCardModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(4m)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Sekketsusoujitsu];

    public Ketsujin() : base(2, CardType.Power, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(JogasakiNoahCardPool))]
public sealed class Shiou : PathCustomCardModel
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Sekketsusoujitsu];

    public Shiou() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal block = DynamicVars.Block.BaseValue;
        if (Owner.Creature.HasPower<SekirinyakudouPower>())
        {
            block += Math.Floor(block * 0.5m);
        }

        return CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
    }
}
