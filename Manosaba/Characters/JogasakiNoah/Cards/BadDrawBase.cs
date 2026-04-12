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
    private const int energyCost = 0;
    private const CardType cardTypeValue = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetTypeValue = TargetType.AnyAlly;
    private const bool shouldShowInCardLibrary = false;

    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    protected abstract Type[] SiblingTypes { get; }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MajokaPower>()];

    public BadDrawBase() : base(energyCost, cardTypeValue, rarity, targetTypeValue, shouldShowInCardLibrary)
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
            .Where(c => SiblingTypes.Contains(c.GetType()))
            .Distinct()
            .ToList();

        foreach (CardModel sibling in siblings)
        {
            await CardPileCmd.RemoveFromCombat(sibling);
        }
    }
}
