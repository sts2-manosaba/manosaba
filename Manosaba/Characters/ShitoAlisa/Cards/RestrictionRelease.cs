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

/// <summary>限制解除�E�E 費、燃燁E3、耗盡手牌並獲征E8 張火琁E��升級移除消耗；卡面提示含火琁E��繞、E/summary>
[Pool(typeof(ShitoAlisaCardPool))]
public sealed class RestrictionRelease : ShitoAlisaCardModel
{
    private const int energyCost = 5;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const int FireballsGranted = 8;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Combust, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => WithCombust(3);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Fireball>(),
        HoverTipFactory.FromPower<FireballSwarmPower>(),
        HoverTipFactory.FromKeyword(ManosabaKeywords.Combust),
    ];

    public RestrictionRelease() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> hand = PileType.Hand.GetPile(Owner).Cards.ToList();
        foreach (CardModel card in hand)
            await CardCmd.Exhaust(choiceContext, card);

        ICombatState? combatState = CombatState;
        if (combatState == null)
            return;

        for (int i = 0; i < FireballsGranted; i++)
        {
            CardModel orb = combatState.CreateCard<Fireball>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(orb, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
