using BaseLib.Utils;
using manosaba.Extensions;
using Manosaba.Characters.Common.Commands;
using Manosaba.Characters.Common.Monsters;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using JogasakiNoahCharacter = manosaba.Characters.JogasakiNoah.JogasakiNoah;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace manosaba.Characters.NatsumeAnan.Cards;

[Pool(typeof(NatsumeAnanCardPool))]
public sealed class NovelCreation : NatsumeKotodamaCardModel
{
    public override bool GainsBlock => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. base.ExtraHoverTips,
        HoverTipFactory.FromCard<SettingFlameRabbit>(),
        HoverTipFactory.FromCard<SettingHolyWhiteSnake>(),
        HoverTipFactory.FromCard<SettingClawedCockatrice>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("KotodamaCost", 5m),
        new BlockVar(5m, ValueProp.Move),
        new EnergyVar(3),
    ];

    public NovelCreation() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        MegaCrit.Sts2.Core.Combat.ICombatState? combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);

        Player? targetNoah = SelectRandomJogasakiNoahTeammate();
        if (targetNoah == null)
        {
            return;
        }

        CardModel settingCard = RollNovelSettingCard(targetNoah, combatState);
        CardPileAddResult result = await CardPileCmd.AddGeneratedCardToCombat(settingCard, PileType.Hand, Owner);
        CardCmd.PreviewCardPileAdd(result);

        _ = choiceContext;
    }

    private Player? SelectRandomJogasakiNoahTeammate()
    {
        MegaCrit.Sts2.Core.Combat.ICombatState? combatState = CombatState;
        if (combatState == null)
        {
            return null;
        }

        List<Player> noahTeammates = combatState.GetTeammatesOf(Owner.Creature)
            .Where(creature => creature.IsAlive && creature.Player != null)
            .Select(creature => creature.Player!)
            .Where(IsJogasakiNoah)
            .Distinct()
            .ToList();

        if (noahTeammates.Count == 0)
        {
            return null;
        }

        return Owner.RunState.Rng.CombatTargets.NextItem(noahTeammates);
    }

    private CardModel RollNovelSettingCard(Player targetNoah, MegaCrit.Sts2.Core.Combat.ICombatState combatState)
    {
        int roll = Owner.RunState.Rng.CombatCardGeneration.NextInt(100);
        if (roll < 33)
        {
            return combatState.CreateCard<SettingFlameRabbit>(targetNoah);
        }

        if (roll < 66)
        {
            return combatState.CreateCard<SettingHolyWhiteSnake>(targetNoah);
        }

        if (roll < 99)
        {
            return combatState.CreateCard<SettingClawedCockatrice>(targetNoah);
        }

        return combatState.CreateCard<SettingCrimsonValstrax>(targetNoah);
    }

    private static bool IsJogasakiNoah(Player player)
    {
        return player.Character.Id.Entry.EndsWith(JogasakiNoahCharacter.CharacterId, StringComparison.OrdinalIgnoreCase);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class SettingFlameRabbit : PathCustomCardModel
{
    public override string PortraitPath => "novel_creation.png".CardsImagePath();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public SettingFlameRabbit() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        NovelSettingNoahFriendsSfx.TryPlay();
        await NovelSettingMonsterCmd.Summon<NovelFlameRabbit>(choiceContext, Owner, this, 25m);
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class SettingHolyWhiteSnake : PathCustomCardModel
{
    public override string PortraitPath => "novel_creation.png".CardsImagePath();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public SettingHolyWhiteSnake() : base(2, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        NovelSettingNoahFriendsSfx.TryPlay();
        await NovelSettingMonsterCmd.Summon<NovelHolyWhiteSnake>(choiceContext, Owner, this, 50m);
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class SettingClawedCockatrice : PathCustomCardModel
{
    public override string PortraitPath => "novel_creation.png".CardsImagePath();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public SettingClawedCockatrice() : base(1, CardType.Skill, CardRarity.Token, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        NovelSettingNoahFriendsSfx.TryPlay();
        await NovelSettingMonsterCmd.Summon<NovelClawedCockatrice>(choiceContext, Owner, this, 20m);
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class SettingCrimsonValstrax : PathCustomCardModel
{
    private const string ValstraxAmbushMessageVfxScenePath = "res://Manosaba/scenes/natsume_anan/vfx/valstrax_ambush_message.tscn";
    public override string PortraitPath => "novel_creation.png".CardsImagePath();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public SettingCrimsonValstrax() : base(3, CardType.Skill, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;
        await ManosabaVfxCmd.PlaySceneAtCombatViewportPositionAndWait(
            ValstraxAmbushMessageVfxScenePath,
            normalizedX: 1f,
            normalizedY: 0.5f);
        SfxCmd.Play("event:/Manosaba/audio/bgm/valstrax_theme.mp3", 0.8f);
        await NovelSettingMonsterCmd.Summon<NovelCrimsonValstrax>(choiceContext, Owner, this, 500m);
    }

    protected override void OnUpgrade()
    {
    }
}
