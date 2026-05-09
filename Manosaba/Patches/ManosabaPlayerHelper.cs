using MegaCrit.Sts2.Core.Entities.Players;
using manosaba.Characters.HoshoMago;

namespace Manosaba.Patches;

internal static class ManosabaPlayerHelper
{
    public static bool IsManosabaPlayer(Player player)
    {
        string? poolNamespace = player.Character?.CardPool.GetType().Namespace;
        return poolNamespace != null && poolNamespace.StartsWith("manosaba.", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsHoshoMagoPlayer(Player player)
        => player.Character?.CardPool is HoshoMagoCardPool;
}
