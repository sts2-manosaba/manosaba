using BaseLib.Extensions;
using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Cards;

[Pool(typeof(JogasakiNoahCardPool))]
public abstract class BadDrawBase : PathCustomCardModel
{
    private const int EnergyCost = 0;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity Rarity = CardRarity.Token;
    private const TargetType TargetTypeValue = TargetType.AnyAlly;
    private const bool ShouldShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected abstract Type[] SiblingTypes { get; }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

    public BadDrawBase() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target?.Player?.Character == null)
            return;

        string targetCharacterId = cardPlay.Target.Player.Character.Id.ToString().RemovePrefix().ToLowerInvariant();
        if (targetCharacterId == "jogasaki_noah")
        {
            await PowerCmd.Apply<MajokaPower>(cardPlay.Target, 100m, Owner.Creature, this);
        }

        IEnumerable<CardModel> siblings = CardPile
            .GetCards(Owner, PileType.Hand, PileType.Draw, PileType.Discard, PileType.Play, PileType.Exhaust)
            .Where(c => c != this && SiblingTypes.Contains(c.GetType()))
            .Distinct()
            .ToList();

        foreach (CardModel sibling in siblings)
        {
            await CardPileCmd.RemoveFromCombat(sibling);
        }
    }
}
