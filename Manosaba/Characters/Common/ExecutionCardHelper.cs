using Manosaba.Characters.Common.Overrides;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common;

public static class ExecutionCardHelper
{
    public static bool IsExecutionCard(CardModel card) =>
        card.Keywords.Contains(ManosabaKeywords.Execution);
}
