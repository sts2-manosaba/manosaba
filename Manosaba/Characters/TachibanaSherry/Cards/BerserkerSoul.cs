using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.TachibanaSherry.Cards;

/// <summary>狂戰士之魂：力量未達上限時捨棄手牌，連續抽打攻擊牌於隨機敵人直至抽到非攻擊牌或達 {MaxPlays} 次；無可打擊敵人或皆為無限血量時停止。</summary>
[Pool(typeof(TachibanaSherryCardPool))]
public sealed class BerserkerSoul : PathCustomCardModel
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const string StrengthCapKey = "StrengthCap";
    private const decimal StrengthCap = 16m;
    private const string MaxPlaysKey = "MaxPlays";
    private const int MaxPlays = 7;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar(StrengthCapKey, StrengthCap),
        new IntVar(MaxPlaysKey, MaxPlays),
    ];

    protected override bool IsPlayable =>
        base.IsPlayable && Owner.Creature.GetPowerAmount<StrengthPower>() < StrengthCap;

    public BerserkerSoul() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Creature is not { } || CombatState is not { } combatState)
        {
            return;
        }

        List<CardModel> handCards = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c != cardPlay.Card)
            .ToList();
        if (handCards.Count > 0)
        {
            await CardCmd.Discard(choiceContext, handCards);
        }

        for (int i = 0;
             i < MaxPlays
             && !CombatManager.Instance.IsOverOrEnding
             && CanContinueBerserkerChain(combatState);
             i++)
        {
            if (await CardPileCmd.Draw(choiceContext, Owner) is not CardModel drawn)
            {
                break;
            }

            if (drawn.Type != CardType.Attack)
            {
                break;
            }

            if (!CanContinueBerserkerChain(combatState))
            {
                break;
            }

            Creature? target = Owner.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
            await CardCmd.AutoPlay(choiceContext, drawn, target);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }

    /// <summary>比照 <c>HellraiserPower</c>：無可打擊目標或全為無限血量時停止連鎖。</summary>
    private static bool CanContinueBerserkerChain(ICombatState combatState)
    {
        IReadOnlyList<Creature> hittableEnemies = combatState.HittableEnemies;
        if (hittableEnemies.Count == 0)
        {
            return false;
        }

        return !hittableEnemies.All(static c => c.HpDisplay.IsInfinite());
    }
}
