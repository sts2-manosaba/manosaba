using BaseLib.Utils;
using manosaba.Characters.HoshoMago;
using manosaba.Extensions;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.HoshoMago.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Manosaba.Characters.HoshoMago.Cards;

[Pool(typeof(TokenCardPool))]
public sealed class TheDevilEyeOfNoEscapeToken : PathCustomCardModel
{
    public override string PortraitPath => "the_devil_token.png".CardsImagePath();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<EyeOfNoEscapePower>(3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<EyeOfNoEscapePower>(), HoverTipFactory.FromPower<MadnessPower>()];

    public TheDevilEyeOfNoEscapeToken() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        if (CombatState == null)
        {
            return;
        }

        List<Creature> targets = CombatState.GetTeammatesOf(Owner.Creature)
            .Concat(CombatState.GetOpponentsOf(Owner.Creature))
            .Where(creature => creature != null && creature.IsAlive)
            .Distinct()
            .ToList();

        if (targets.Count == 0)
        {
            return;
        }

        foreach (Creature target in targets)
        {
            await CommonActions.Apply<EyeOfNoEscapePower>(choiceContext, target, this, DynamicVars["EyeOfNoEscapePower"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class TheDevilAwakenedMadnessPowerToken : PathCustomCardModel
{
    public override string PortraitPath => "the_devil_token.png".CardsImagePath();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<AwakenedMadnessPower>(3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<AwakenedMadnessPower>(), HoverTipFactory.FromPower<MadnessPower>()];

    public TheDevilAwakenedMadnessPowerToken() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        if (CombatState == null)
        {
            return;
        }

        List<Creature> targets = CombatState.GetTeammatesOf(Owner.Creature)
            .Concat(CombatState.GetOpponentsOf(Owner.Creature))
            .Where(creature => creature != null && creature.IsAlive)
            .Distinct()
            .ToList();

        if (targets.Count == 0)
        {
            return;
        }

        foreach (Creature target in targets)
        {
            await CommonActions.Apply<AwakenedMadnessPower>(choiceContext, target, this, DynamicVars["AwakenedMadnessPower"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class TheDevilBestowTrialToken : PathCustomCardModel
{
    public override string PortraitPath => "the_devil_token.png".CardsImagePath();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public TheDevilBestowTrialToken() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        if (CombatState == null)
        {
            return;
        }

        bool isBossRoom = CombatState.Encounter?.RoomType == RoomType.Boss;
        List<Creature> targets = CombatState.GetTeammatesOf(Owner.Creature)
            .Concat(CombatState.GetOpponentsOf(Owner.Creature))
            .Where(creature => creature != null && creature.IsAlive)
            .Distinct()
            .ToList();

        foreach (Creature creature in targets)
        {
            if (isBossRoom && creature.Monster != null)
            {
                continue;
            }

            int hpThreshold = creature.MaxHp / 2;
            if (creature.CurrentHp <= hpThreshold)
            {
                continue;
            }

            await CreatureCmd.SetCurrentHp(creature, hpThreshold);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
