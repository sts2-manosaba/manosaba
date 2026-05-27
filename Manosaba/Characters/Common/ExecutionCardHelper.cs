using Manosaba.Characters.Common.Cards;
using Manosaba.Characters.HasumiLeia.Cards;
using Manosaba.Characters.SaekiMiria.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common;

public static class ExecutionCardHelper
{
    private static readonly Lazy<HashSet<ModelId>> ExecutionCardIds = new(BuildExecutionCardIds);

    public static bool IsExecutionCard(CardModel card) => ExecutionCardIds.Value.Contains(card.Id);

    private static HashSet<ModelId> BuildExecutionCardIds() =>
    [
        ModelDb.Card<Boulders>().Id,
        ModelDb.Card<Bullseye>().Id,
        ModelDb.Card<ConcentratedFire>().Id,
        ModelDb.Card<DeathPendulum>().Id,
        ModelDb.Card<Gallows>().Id,
        ModelDb.Card<Dissolve>().Id,
        ModelDb.Card<Electrocution>().Id,
        ModelDb.Card<Guillotine>().Id,
        ModelDb.Card<Hotpot>().Id,
        ModelDb.Card<Immolation>().Id,
        ModelDb.Card<Suicide>().Id,
        ModelDb.Card<FallGuy>().Id,
        ModelDb.Card<Clusterphobia>().Id,
    ];
}
