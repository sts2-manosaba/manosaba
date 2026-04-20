using BaseLib.Utils;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.SaekiMiria.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using SaekiMiriaCharacter = manosaba.Characters.SaekiMiria.SaekiMiria;

namespace manosaba.Characters.NatsumeAnan.Cards;

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class StrikeNatsumeAnan : NatsumeKotodamaCardModel
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    public StrikeNatsumeAnan() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
            return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class DefendNatsumeAnan : NatsumeKotodamaCardModel
{
    public override bool GainsBlock => true;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

    public DefendNatsumeAnan() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class TraumaNatsumeAnan : NatsumeKotodamaCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MajokaPower>(10m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

    public TraumaNatsumeAnan() : base(0, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;
        await PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars["MajokaPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MajokaPower"].UpgradeValueBy(5m);
    }
}

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class FlashOfInspiration : NatsumeKotodamaCardModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("KotodamaGain", 2m)];

    public FlashOfInspiration() : base(1, CardType.Skill, CardRarity.Basic, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        KotodamaEnergy.Gain(Owner, DynamicVars["KotodamaGain"].IntValue);
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["KotodamaGain"].UpgradeValueBy(1m);
    }
}

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class Instigate : NatsumeKotodamaCardModel
{
    private static readonly IReadOnlyList<Func<Player, CombatState, MovieBase>> MovieFactories =
    [
        static (player, combatState) => combatState.CreateCard<HorrorMovie>(player),
        static (player, combatState) => combatState.CreateCard<ComedyMovie>(player),
        static (player, combatState) => combatState.CreateCard<CassetteShapedRock>(player),
        static (player, combatState) => combatState.CreateCard<FantasyMovie>(player),
        static (player, combatState) => combatState.CreateCard<ActionMovie>(player),
        static (player, combatState) => combatState.CreateCard<RomanticMovie>(player),
        static (player, combatState) => combatState.CreateCard<SpyMovie>(player),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("KotodamaCost", 2m)];

    public Instigate() : base(0, CardType.Skill, CardRarity.Basic, TargetType.AnyPlayer, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player? targetPlayer = cardPlay.Target?.Player;
        if (targetPlayer == null)
            return;

        await CardPileCmd.AutoPlayFromDrawPile(choiceContext, targetPlayer, 1, CardPilePosition.Top, forceExhaust: false);

        if (!IsSaekiMiria(targetPlayer) || CombatState == null)
            return;

        CardModel movie = Owner.RunState.Rng.CombatCardGeneration.NextItem(MovieFactories)(Owner, CombatState);
        CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(movie, PileType.Hand, addedByPlayer: true);
        CardCmd.PreviewCardPileAdd(result);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["KotodamaCost"].UpgradeValueBy(-1m);
    }

    private static bool IsSaekiMiria(Player player)
    {
        return player.Character.Id.Entry.EndsWith(SaekiMiriaCharacter.CharacterId, StringComparison.OrdinalIgnoreCase);
    }
}
