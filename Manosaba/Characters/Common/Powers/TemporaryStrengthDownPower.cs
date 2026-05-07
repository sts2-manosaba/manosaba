using Manosaba.Characters.Common.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Powers;

/// <summary>
/// Reusable temporary Strength down power that can inherit title/hover context from the source card.
/// </summary>
public sealed class TemporaryStrengthDownPower : ManosabaTemporaryStrengthPower
{
    private AbstractModel? _originModel;

    public override AbstractModel OriginModel => _originModel ?? ModelDb.Card<Dissolve>();

    protected override bool IsPositive => false;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource != null)
        {
            _originModel = cardSource;
        }

        return base.AfterApplied(applier, cardSource);
    }
}
