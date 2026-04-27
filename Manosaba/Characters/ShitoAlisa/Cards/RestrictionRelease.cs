using System.Collections.Generic;
using System.Linq;
using BaseLib.Utils;
using manosaba.Characters.ShitoAlisa;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.ShitoAlisa.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.ShitoAlisa.Cards;

/// <summary>限制解除：5 費、燃燒 3、耗盡手牌並獲得 8 張火球；升級移除消耗；卡面提示含火球環繞。</summary>
[Pool(typeof(ShitoAlisaCardPool))]
public sealed class RestrictionRelease : ShitoAlisaCardModel
{
    private new const int EnergyCost = 5;
    private const CardType TypeValue = CardType.Skill;
    private new const CardRarity Rarity = CardRarity.Rare;
    private const TargetType TargetTypeValue = TargetType.Self;
    private new const bool ShouldShowInCardLibrary = true;

    private const int FireballsGranted = 8;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Combust, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithCombust(3);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Fireball>(),
        HoverTipFactory.FromPower<FireballSwarmPower>(),
        HoverTipFactory.FromKeyword(ManosabaKeywords.Combust),
    ];

    public RestrictionRelease() : base(EnergyCost, TypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        foreach (CardModel card in hand)
            await CardCmd.Exhaust(choiceContext, card);

        CombatState? combatState = CombatState;
        if (combatState == null)
            return;

        for (int i = 0; i < FireballsGranted; i++)
        {
            CardModel orb = combatState.CreateCard<Fireball>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(orb, PileType.Hand, addedByPlayer: true);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
