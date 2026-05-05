using Manosaba.Characters.Common.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HasumiLeia.Powers;

public sealed class SmartSpearPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
    public override int DisplayAmount => Amount;

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        _ = choiceContext;
        _ = fromHandDraw;

        if (Owner.Player == null || card.Owner != Owner.Player || !IsSimpleSpearPart(card))
        {
            return;
        }

        if (Owner.CombatState is null)
        {
            return;
        }

        bool addedAny = false;
        for (int i = 0; i < Math.Max(0, Amount); i++)
        {
            addedAny |= await TryAddPartFromDrawPileToHand();
        }

        if (addedAny)
        {
            Flash();
        }
    }

    private static bool IsSimpleSpearPart(CardModel card) =>
        card is SSArrow or SSBroom or SSRibbon or SSRapier;

    private async Task<bool> TryAddPartFromDrawPileToHand()
    {
        if (Owner.Player == null)
        {
            return false;
        }

        List<CardModel> drawPileParts = PileType.Draw.GetPile(Owner.Player).Cards
            .Where(IsSimpleSpearPart)
            .ToList();
        if (drawPileParts.Count == 0)
        {
            return false;
        }

        HashSet<Type> typesInHand = GetSpearPartTypesIn(PileType.Hand.GetPile(Owner.Player).Cards);
        List<CardModel> preferred = drawPileParts
            .Where(card => !typesInHand.Contains(card.GetType()))
            .ToList();

        IReadOnlyList<CardModel> pool = preferred.Count > 0 ? preferred : drawPileParts;
        int index = Owner.CombatState!.RunState.Rng.CombatCardGeneration.NextInt(pool.Count);
        await CardPileCmd.Add(pool[index], PileType.Hand);
        return true;
    }

    private static HashSet<Type> GetSpearPartTypesIn(IEnumerable<CardModel> cards)
    {
        HashSet<Type> types = [];
        foreach (CardModel c in cards)
        {
            if (c is SSArrow)
            {
                types.Add(typeof(SSArrow));
            }
            else if (c is SSBroom)
            {
                types.Add(typeof(SSBroom));
            }
            else if (c is SSRibbon)
            {
                types.Add(typeof(SSRibbon));
            }
            else if (c is SSRapier)
            {
                types.Add(typeof(SSRapier));
            }
        }

        return types;
    }
}
