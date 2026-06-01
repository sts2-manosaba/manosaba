using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.Common.Commands;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Characters.Common.Powers;
using ManosabaTemporaryStrengthPower = Manosaba.Characters.Common.Powers.TemporaryStrengthPower;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.TachibanaSherry.Cards;

public static class CouldItBeThatSkillTokens
{
    private const int OptionsOffered = 2;

    private readonly record struct WeightedTokenEntry(
        Func<CombatState, Player, CardModel> Create,
        float Weight,
        Func<CombatState, bool>? CanOffer = null);

    /// <summary>事件 token 權重表；調整 <see cref="WeightedTokenEntry.Weight"/> 即可改變出現機率（允許重複抽中）。</summary>
    private static readonly WeightedTokenEntry[] TokenWeights =
    [
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatPrisonFoodToken>(player), 6f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatMeruruArrivalToken>(player), 8f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatEqualTreatmentToken>(player), 6f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatHandEraseToken>(player), 6f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatEmmaCharityToken>(player), 8f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatCombatFeelsGoodToken>(player), 8f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatBigFistsToken>(player), 8f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatSlitherToken>(player), 7f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatLeiaBountyToken>(player), 8f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatEquivalentExchangeToken>(player), 1f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatTruthRevealedToken>(player), 1f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatHannasFridgeToken>(player), 8f),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatWardenMassProductionToken>(player), 7f,
            static combatState => !CombatEnemyLayoutCmd.UsesEncounterSlotLayout(combatState)),
        new(static (combatState, player) => combatState.CreateCard<CouldItBeThatCrimeSceneToken>(player), 8f),
    ];

    public static List<CardModel> PickRandomOptions(CombatState combatState, Player player)
    {
        List<WeightedTokenEntry> eligible = GetEligibleEntries(combatState);
        if (eligible.Count == 0)
        {
            return [];
        }

        Rng rng = player.RunState.Rng.CombatCardSelection;
        List<CardModel> results = new(OptionsOffered);
        for (int i = 0; i < OptionsOffered; i++)
        {
            WeightedTokenEntry entry = rng.WeightedNextItem(eligible, static e => e.Weight);
            results.Add(entry.Create(combatState, player));
        }

        return results;
    }

    public static List<CardModel> CreateAll(CombatState combatState, Player player)
    {
        return GetEligibleEntries(combatState)
            .Select(entry => entry.Create(combatState, player))
            .ToList();
    }

    private static List<WeightedTokenEntry> GetEligibleEntries(CombatState combatState) =>
        TokenWeights.Where(entry => entry.CanOffer?.Invoke(combatState) ?? true).ToList();

    internal static IEnumerable<Creature> AllAlivePlayers(CombatState combatState, Creature ownerCreature)
    {
        return combatState.GetTeammatesOf(ownerCreature)
            .Where(creature => creature is { IsAlive: true, IsPlayer: true, Player: not null });
    }

    internal static IEnumerable<Creature> AllAliveEnemies(CombatState combatState, Creature ownerCreature)
    {
        return combatState.GetOpponentsOf(ownerCreature)
            .Where(creature => creature is { IsAlive: true });
    }
}

public abstract class CouldItBeThatEventTokenBase : PathCustomCardModel
{
    protected CouldItBeThatEventTokenBase(TargetType targetType)
        : base(67, CardType.Skill, CardRarity.Token, targetType, true)
    {
    }

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatPrisonFoodToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PoisonPower>(5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];

    public CouldItBeThatPrisonFoodToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature?.CombatState is not { } combatState)
        {
            return;
        }

        decimal poison = DynamicVars["PoisonPower"].BaseValue;
        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, Owner.Creature))
        {
            await PowerCmd.Apply<PoisonPower>(player, poison, Owner.Creature, this);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatMeruruArrivalToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(10m)];

    public CouldItBeThatMeruruArrivalToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature?.CombatState is not { } combatState)
        {
            return;
        }

        decimal heal = DynamicVars.Heal.BaseValue;
        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, Owner.Creature))
        {
            await CreatureCmd.Heal(player, heal);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatEqualTreatmentToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Unpowered)];

    public CouldItBeThatEqualTreatmentToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (Owner?.Creature is not { } ownerCreature || ownerCreature.CombatState is not { } combatState)
        {
            return;
        }

        decimal damage = DynamicVars.Damage.BaseValue;
        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, ownerCreature))
        {
            await CreatureCmd.Damage(choiceContext, player, damage, ValueProp.Unpowered, ownerCreature);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatHandEraseToken : CouldItBeThatEventTokenBase
{
    public CouldItBeThatHandEraseToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (Owner?.Creature?.CombatState is not { } combatState)
        {
            return;
        }

        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, Owner.Creature))
        {
            IReadOnlyList<CardModel> hand = PileType.Hand.GetPile(player.Player!).Cards;
            if (hand.Count == 0)
            {
                continue;
            }

            CardModel card = hand[player.Player!.RunState.Rng.CombatCardSelection.NextInt(hand.Count)];
            await CardCmd.Discard(choiceContext, [card]);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatEmmaCharityToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    public CouldItBeThatEmmaCharityToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        if (Owner?.Creature?.CombatState is not { } combatState)
        {
            return;
        }

        int cards = DynamicVars.Cards.IntValue;
        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, Owner.Creature))
        {
            await CardPileCmd.Draw(choiceContext, cards, player.Player!);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatCombatFeelsGoodToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<UltimateStrike>()];

    public CouldItBeThatCombatFeelsGoodToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature?.CombatState is not { } combatState)
        {
            return;
        }

        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, Owner.Creature))
        {
            CardModel strike = combatState.CreateCard<UltimateStrike>(player.Player!);
            await CardPileCmd.AddGeneratedCardsToCombat(
                [strike],
                PileType.Hand,
                addedByPlayer: true,
                CardPilePosition.Random);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatBigFistsToken : CouldItBeThatEventTokenBase
{
    private const string temporaryStrengthVar = "TemporaryStrength";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar(temporaryStrengthVar, 5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public CouldItBeThatBigFistsToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature is not { } ownerCreature || ownerCreature.CombatState is not { } combatState)
        {
            return;
        }

        decimal strength = DynamicVars[temporaryStrengthVar].BaseValue;
        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, ownerCreature))
        {
            await PowerCmd.Apply<ManosabaTemporaryStrengthPower>(player, strength, ownerCreature, this);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatSlitherToken : CouldItBeThatEventTokenBase
{
    public CouldItBeThatSlitherToken() : base(TargetType.Self)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner is not { } owner)
        {
            return Task.CompletedTask;
        }

        foreach (CardModel card in PileType.Hand.GetPile(owner).Cards.ToList())
        {
            int cost = owner.RunState.Rng.CombatCardSelection.NextInt(4);
            card.EnergyCost.SetThisTurnOrUntilPlayed(cost);
            card.InvokeEnergyCostChanged();
        }

        return Task.CompletedTask;
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatLeiaBountyToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new GoldVar(5)];

    public CouldItBeThatLeiaBountyToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature?.CombatState is not { } combatState)
        {
            return;
        }

        decimal gold = DynamicVars.Gold.BaseValue;
        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, Owner.Creature))
        {
            await PlayerCmd.GainGold(gold, player.Player!);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatEquivalentExchangeToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<BufferPower>(1m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BufferPower>()];

    public CouldItBeThatEquivalentExchangeToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature is not { } ownerCreature || ownerCreature.CombatState is not { } combatState)
        {
            return;
        }

        decimal buffer = DynamicVars["BufferPower"].BaseValue;
        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, ownerCreature))
        {
            if (player.CurrentHp != 1)
            {
                await CreatureCmd.SetCurrentHp(player, 1);
            }

            await PowerCmd.Apply<BufferPower>(player, buffer, ownerCreature, this);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatTruthRevealedToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(2m),
        new PowerVar<IntangiblePower>(1m),
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VulnerablePower>(),
        HoverTipFactory.FromPower<IntangiblePower>(),
    ];

    public CouldItBeThatTruthRevealedToken() : base(TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature is not { } ownerCreature || ownerCreature.CombatState is not { } combatState)
        {
            return;
        }

        decimal vulnerable = DynamicVars["VulnerablePower"].BaseValue;
        decimal intangible = DynamicVars["IntangiblePower"].BaseValue;

        foreach (Creature enemy in CouldItBeThatSkillTokens.AllAliveEnemies(combatState, ownerCreature))
        {
            await PowerCmd.Apply<VulnerablePower>(enemy, vulnerable, ownerCreature, this);
        }

        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, ownerCreature))
        {
            await PowerCmd.Apply<IntangiblePower>(player, intangible, ownerCreature, this);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatHannasFridgeToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [EnergyHoverTip];

    public CouldItBeThatHannasFridgeToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature?.CombatState is not { } combatState)
        {
            return;
        }

        decimal energy = DynamicVars.Energy.BaseValue;
        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, Owner.Creature))
        {
            await PlayerCmd.GainEnergy(energy, player.Player!);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatWardenMassProductionToken : CouldItBeThatEventTokenBase
{
    private const string wardensVar = "Wardens";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(wardensVar, 2m),
        new GoldVar(EventWardenBountyPower.GoldReward),
    ];

    public CouldItBeThatWardenMassProductionToken() : base(TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature?.CombatState is not { } combatState)
        {
            return;
        }

        int wardenCount = DynamicVars[wardensVar].IntValue;
        for (int i = 0; i < wardenCount; i++)
        {
            Creature? warden = await CombatEnemyLayoutCmd.TryAddEnemy<EventWarden>(combatState);
            if (warden == null)
            {
                break;
            }

            await PowerCmd.Apply<EventWardenBountyPower>(warden, 1m, Owner.Creature, this);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class CouldItBeThatCrimeSceneToken : CouldItBeThatEventTokenBase
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MajokaPower>(10m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

    public CouldItBeThatCrimeSceneToken() : base(TargetType.AllAllies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        if (Owner?.Creature is not { } ownerCreature || ownerCreature.CombatState is not { } combatState)
        {
            return;
        }

        decimal majoka = DynamicVars["MajokaPower"].BaseValue;
        foreach (Creature player in CouldItBeThatSkillTokens.AllAlivePlayers(combatState, ownerCreature))
        {
            await PowerCmd.Apply<MajokaPower>(player, majoka, ownerCreature, this);
        }
    }
}
