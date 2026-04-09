using BaseLib.Utils;
using Godot;
using manosaba.Characters.HoshoMago;
using manosaba.Extensions;
using Manosaba.Characters.Common.Commands;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HoshoMago.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HoshoMago.Cards;

public abstract class HoshoMagoArcanaBase : PathCustomCardModel
{
    protected override HashSet<CardTag> CanonicalTags => [ManosabaCardTags.Tarot];
    protected HoshoMagoArcanaBase(
        int energyCost = 1,
        CardType cardType = CardType.Skill,
        TargetType targetType = TargetType.Self)
        : base(energyCost, cardType, CardRarity.Ancient, targetType, true)
    {
    }

    // Route Major Arcana cards to their dedicated tarot art folder.
    public override string PortraitPath => Path.Join("tarot", Path.GetFileName(base.PortraitPath)).CardsImagePath();

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheFool : HoshoMagoArcanaBase
{
    public TheFool() : base(0)
    {
    }
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<EnergyNextTurnPower>(Owner.Creature, DynamicVars.Energy.IntValue, Owner.Creature, this);
        await PowerCmd.Apply<TheFoolPower>(Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheMagician : HoshoMagoArcanaBase
{
    public TheMagician() : base(0)
    {
    }
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Deck?.Cards == null)
        {
            return;
        }

        int tarotCardsInDeck = Owner.Deck.Cards.Count(card => card.Tags.Contains(ManosabaCardTags.Tarot));
        int majokaToGain = tarotCardsInDeck * 5;
        if (majokaToGain <= 0)
        {
            return;
        }

        await PowerCmd.Apply<MajokaPower>(Owner.Creature, majokaToGain, Owner.Creature, this);
    }
    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheHighPriestess : HoshoMagoArcanaBase
{
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(7m),
        new CalculationExtraVar(4m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier(static (CardModel card, Creature? _) =>
            PileType.Hand.GetPile(card.Owner).Cards.Count(c => c.Type == CardType.Status))
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> statusesInHand = PileType.Hand.GetPile(Owner).Cards
            .Where(card => card.Type == CardType.Status)
            .ToList();
        decimal blockToGain = DynamicVars.CalculatedBlock.Calculate(cardPlay.Target);

        foreach (CardModel status in statusesInHand)
        {
            await CardCmd.Exhaust(choiceContext, status);
        }

        await CreatureCmd.GainBlock(Owner.Creature, blockToGain, DynamicVars.CalculatedBlock.Props, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheEmpress : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    public TheEmpress() : base(1, CardType.Attack, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        bool hasTheEmperorInDeck = Owner?.Deck?.Cards.Any(card => card is TheEmperor) == true;
        decimal damage = DynamicVars.Damage.BaseValue * (hasTheEmperorInDeck ? 2m : 1m);

        IEnumerable<DamageResult> results = await CreatureCmd.Damage(
            choiceContext,
            cardPlay.Target,
            damage,
            ValueProp.Move,
            Owner.Creature,
            this);
        int totalUnblockedDamage = results.Sum(result => result.UnblockedDamage);
        if (totalUnblockedDamage > 0)
        {
            await CreatureCmd.Heal(Owner.Creature, totalUnblockedDamage * 0.5m);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheEmperor : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(25m, ValueProp.Move)];

    public TheEmperor() : base(2, CardType.Attack, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        bool hasTheEmpressInDeck = Owner?.Deck?.Cards.Any(card => card is TheEmpress) == true;
        decimal damage = DynamicVars.Damage.BaseValue * (hasTheEmpressInDeck ? 2m : 1m);

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheHierophant : HoshoMagoArcanaBase
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<VotePower>(),
        HoverTipFactory.FromPower<WeakPower>(),
        HoverTipFactory.FromPower<VulnerablePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal removedVote = Owner.Creature.GetPowerAmount<VotePower>();
        if (removedVote > 0)
        {
            await PowerCmd.Apply<VotePower>(Owner.Creature, -removedVote, Owner.Creature, this);
        }

        decimal debuffAmount = 2m + removedVote * 2m;

        List<Creature> enemies = CombatState.GetOpponentsOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive)
            .ToList();

        foreach (Creature enemy in enemies)
        {
            await PowerCmd.Apply<WeakPower>(enemy, debuffAmount, Owner.Creature, this);
            await PowerCmd.Apply<VulnerablePower>(enemy, debuffAmount, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheLovers : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<TheLoversPower>(50)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TheLoversPower>()];

    public TheLovers() : base(1, CardType.Skill, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        await PowerCmd.Apply<TheLoversPower>(cardPlay.Target, DynamicVars["TheLoversPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TheLoversPower"].UpgradeValueBy(25m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheChariot : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move), new RepeatVar(4)];

    public TheChariot() : base(2, CardType.Attack, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(2m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class Strength : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(3)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public Strength() : base(1, CardType.Power, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheHermit : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<IntangiblePower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<IntangiblePower>()];

    public TheHermit() : base(2, CardType.Power, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<IntangiblePower>(Owner.Creature, DynamicVars["IntangiblePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class WheelOfFortune : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (CardModel card in PileType.Hand.GetPile(Owner).Cards.ToList())
        {
            await CardPileCmd.Add(card, PileType.Draw);
        }

        await CardPileCmd.Shuffle(choiceContext, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(2m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class Justice : HoshoMagoArcanaBase
{
    public Justice() : base(1, CardType.Skill, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<Creature> enemies = CombatState.GetOpponentsOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive)
            .ToList();
        await RedistributeHpByLostHp(enemies);

        if (!IsUpgraded)
        {
            return;
        }

        List<Creature> allies = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive && creature.IsPlayer)
            .ToList();
        await RedistributeHpByLostHp(allies);
    }

    private static async Task RedistributeHpByLostHp(List<Creature> creatures)
    {
        if (creatures.Count <= 0)
        {
            return;
        }

        int totalLostHp = creatures.Sum(creature => Math.Max(0, creature.MaxHp - creature.CurrentHp));
        decimal averageLostHp = (decimal)totalLostHp / creatures.Count;

        for (int i = 0; i < creatures.Count; i++)
        {
            Creature creature = creatures[i];
            int hp = (int)Math.Round(creature.MaxHp - averageLostHp, MidpointRounding.AwayFromZero);
            hp = Math.Clamp(hp, 0, creature.MaxHp);
            await CreatureCmd.SetCurrentHp(creature, hp);
        }
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheHangedMan : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public TheHangedMan() : base(0, CardType.Skill, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        await PowerCmd.Apply<StrengthPower>(target, -DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);

        NCreature? node = NCombatRoom.Instance?.GetCreatureNode(target);
        if (node == null)
        {
            return;
        }

        float liftY = node.Hitbox.Size.Y;

        Vector2 scale = node.Visuals.GetCurrentBody().Scale;
        node.Visuals.GetCurrentBody().Scale = new Vector2(scale.X, -Mathf.Abs(scale.Y));
        node.Visuals.GetCurrentBody().Position = new Vector2(
            node.Visuals.GetCurrentBody().Position.X,
            node.Visuals.GetCurrentBody().Position.Y - liftY
        );
    }
    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class Death : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DoomPower>(10)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DoomPower>()];

    public Death() : base(1, CardType.Attack, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal enemyBaseDoom = DynamicVars["DoomPower"].BaseValue + (IsUpgraded ? 5m : 0m);
        decimal allyBaseDoom = DynamicVars["DoomPower"].BaseValue;

        List<Creature> enemies = CombatState.GetOpponentsOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive)
            .ToList();
        List<Creature> allies = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive && creature.IsPlayer)
            .ToList();

        if (enemies.Count > 0)
        {
            await PowerCmd.Apply<DoomPower>(enemies, enemyBaseDoom, Owner.Creature, this);
        }

        if (allies.Count > 0)
        {
            await PowerCmd.Apply<DoomPower>(allies, allyBaseDoom, Owner.Creature, this);
        }

        foreach (Creature enemy in enemies)
        {
            decimal doomAmount = enemy.GetPowerAmount<DoomPower>();
            if (doomAmount > 0)
            {
                await PowerCmd.Apply<DoomPower>(enemy, doomAmount, Owner.Creature, this);
            }
        }
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class Temperance : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<TemperancePower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TemperancePower>()];

    public Temperance() : base(3, CardType.Power, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TemperancePower>(Owner.Creature, DynamicVars["TemperancePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheDevil : HoshoMagoArcanaBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<TheDevilEyeOfNoEscapeToken>(),
        HoverTipFactory.FromCard<TheDevilAwakenedMadnessPowerToken>(),
        HoverTipFactory.FromCard<TheDevilBestowTrialToken>()
    ];

    public TheDevil() : base(1, CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.CombatState == null)
        {
            return;
        }

        List<CardModel> options =
        [
            Owner.Creature.CombatState.CreateCard<TheDevilEyeOfNoEscapeToken>(Owner),
            Owner.Creature.CombatState.CreateCard<TheDevilAwakenedMadnessPowerToken>(Owner),
            Owner.Creature.CombatState.CreateCard<TheDevilBestowTrialToken>(Owner)
        ];

        CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner);
        if (selected == null)
        {
            return;
        }

        await CardCmd.AutoPlay(choiceContext, selected, null);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheTower : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ArtifactPower>(), HoverTipFactory.FromPower<TheTowerPower>()];

    public TheTower() : base(2, CardType.Attack, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<Creature> enemies = CombatState.GetOpponentsOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive)
            .ToList();

        if (enemies.Count > 0)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .Execute(choiceContext);
        }

        foreach (Creature enemy in enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            await PowerCmd.Remove<ArtifactPower>(enemy);
            await PowerCmd.Apply<TheTowerPower>(enemy, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["TheTowerPower"].UpgradeValueBy(1m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheStar : HoshoMagoArcanaBase
{
    private const int InterceptPerExtraTeammate = 7;
    private const decimal BlockAmount = 10m;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("InterceptPower", InterceptPerExtraTeammate)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block), HoverTipFactory.FromPower<CoveredPower>(), HoverTipFactory.FromPower<TheStarPower>()];

    public TheStar() : base(2, CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, BlockAmount, ValueProp.Move, cardPlay);

        List<Creature> teammates = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive && creature.IsPlayer && creature != Owner.Creature)
            .ToList();

        foreach (Creature teammate in teammates)
        {
            await PowerCmd.Apply<CoveredPower>(teammate, 1m, Owner.Creature, this);
        }

        int extraTeammates = Math.Max(0, teammates.Count - 1);
        if (extraTeammates <= 0)
        {
            return;
        }

        decimal interceptAmount = extraTeammates * DynamicVars["InterceptPower"].BaseValue;
        bool hasSunMoonStar = Owner?.Deck?.Cards.Any(card => card is TheSun)
            == true
            && Owner.Deck.Cards.Any(card => card is TheMoon)
            && Owner.Deck.Cards.Any(card => card is TheStar);
        if (hasSunMoonStar)
        {
            interceptAmount *= 2m;
        }

        await PowerCmd.Apply<TheStarPower>(Owner.Creature, interceptAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["InterceptPower"].UpgradeValueBy(3m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheMoon : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Rate", 50m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromPower<PoisonPower>(),
        HoverTipFactory.FromPower<DoomPower>(),
        HoverTipFactory.FromPower<BurnPower>()
    ];

    public TheMoon() : base(1, CardType.Attack, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        decimal totalStacks = target.GetPowerAmount<PoisonPower>()
            + target.GetPowerAmount<DoomPower>()
            + target.GetPowerAmount<BurnPower>();
        if (totalStacks <= 0)
        {
            return;
        }

        decimal damage = totalStacks * (DynamicVars["Rate"].BaseValue / 100m);

        bool hasSunMoonStar = Owner?.Deck?.Cards.Any(card => card is TheSun)
            == true
            && Owner.Deck.Cards.Any(card => card is TheMoon)
            && Owner.Deck.Cards.Any(card => card is TheStar);
        if (hasSunMoonStar)
        {
            damage *= 2m;
        }

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Rate"].UpgradeValueBy(25m);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheSun : HoshoMagoArcanaBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<TheSunPower>(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<TheSunPower>()];

    public TheSun() : base(2, CardType.Power, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        bool hasSunMoonStar = Owner?.Deck?.Cards.Any(card => card is TheSun)
            == true
            && Owner.Deck.Cards.Any(card => card is TheMoon)
            && Owner.Deck.Cards.Any(card => card is TheStar);

        if (!hasSunMoonStar)
        {
            await PowerCmd.Apply<TheSunPower>(Owner.Creature, DynamicVars["TheSunPower"].BaseValue, Owner.Creature, this);
            return;
        }

        List<Creature> allies = CombatState.GetTeammatesOf(Owner.Creature)
            .Where(creature => creature != null && creature.IsAlive && creature.IsPlayer)
            .ToList();

        foreach (Creature ally in allies)
        {
            await PowerCmd.Apply<TheSunPower>(ally, DynamicVars["TheSunPower"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class Judgement : HoshoMagoArcanaBase
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip, HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public Judgement() : base(1, CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        List<PowerModel> debuffs = Owner.Creature.Powers
            .Where(power => power != null && power.Type == PowerType.Debuff)
            .ToList();

        foreach (PowerModel debuff in debuffs)
        {
            await PowerCmd.Remove(debuff);
        }

        if (debuffs.Count > 0)
        {
            bool hasHierophantPriestessJustice = Owner?.Deck?.Cards.Any(card => card is TheHierophant)
                == true
                && Owner.Deck.Cards.Any(card => card is TheHighPriestess)
                && Owner.Deck.Cards.Any(card => card is Justice);

            int energyGain = debuffs.Count;
            if (hasHierophantPriestessJustice)
            {
                energyGain *= 2;
            }

            await PlayerCmd.GainEnergy(energyGain, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

[Pool(typeof(HoshoMagoCardPool))]
public sealed class TheWorld : HoshoMagoArcanaBase
{
    private const string VfxScenePath = "res://Manosaba/scenes/hosho_mago/vfx/the_world.tscn";
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public TheWorld() : base(0)
    {
    }

    // Match Grand Finale style: only playable when the draw pile is empty.
    protected override bool IsPlayable => base.IsPlayable && PileType.Draw.GetPile(Owner).Cards.Count == 0;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null)
        {
            return;
        }

        List<CardModel> tarotCards = ModelDb.CardPool<HoshoMagoCardPool>()
            .AllCards
            .Where(card => card.Tags.Contains(ManosabaCardTags.Tarot) && card is not TheWorld)
            .ToList();
        ManosabaVfxCmd.PlaySceneAtCombatCenter(VfxScenePath, fitCoverViewport: true, spriteNodeNames: ["StillA", "StillB"]);
        if (tarotCards.Count > 0)
        {
            List<CardModel> previews = tarotCards
                .Select(card => CombatState.CreateCard(card, Owner))
                .ToList();
            IReadOnlyList<CardPileAddResult> previewResults = await CardPileCmd.AddGeneratedCardsToCombat(
                previews,
                PileType.Discard,
                addedByPlayer: false,
                CardPilePosition.Random);
            CardCmd.PreviewCardPileAdd(previewResults, 1.2f, CardPreviewStyle.MessyLayout);
            await Cmd.Wait(3.4f);

            foreach (CardPileAddResult result in previewResults)
            {
                if (result.success && result.cardAdded != null)
                {
                    await CardPileCmd.RemoveFromCombat(result.cardAdded);
                }
            }
        }
        await ManosabaCombatCmd.ForceWinWithoutDeathOrEscape(CombatState);
    }
}
