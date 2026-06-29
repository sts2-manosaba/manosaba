using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using manosaba.Characters.HikamiMeruru;
using manosaba.Characters.HoshoMago;
using manosaba.Characters.JogasakiNoah;
using manosaba.Characters.KurobeNanoka;
using manosaba.Characters.NatsumeAnan;
using manosaba.Characters.NikaidoHiro;
using manosaba.Characters.SaekiMiria;
using manosaba.Characters.ShitoAlisa;
using manosaba.Characters.TachibanaSherry;
using manosaba.Characters.TonoHanna;
using manosaba.Characters.SawatariCoco.Cards;
using manosaba.Characters.SawatariCoco.Relics;
using Manosaba.Characters.SawatariCoco.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.SawatariCoco.Helper;

public static class SawatariCocoHelper
{
    private static readonly IReadOnlyList<CardPoolModel> OtherWitchCardPools =
    [
        ModelDb.CardPool<HasumiLeiaCardPool>(),
        ModelDb.CardPool<HikamiMeruruCardPool>(),
        ModelDb.CardPool<HoshoMagoCardPool>(),
        ModelDb.CardPool<JogasakiNoahCardPool>(),
        ModelDb.CardPool<KurobeNanokaCardPool>(),
        ModelDb.CardPool<NatsumeAnanCardPool>(),
        ModelDb.CardPool<NikaidoHiroCardPool>(),
        ModelDb.CardPool<SaekiMiriaCardPool>(),
        ModelDb.CardPool<ShitoAlisaCardPool>(),
        ModelDb.CardPool<TachibanaSherryCardPool>(),
        ModelDb.CardPool<TonoHannaCardPool>(),
    ];

    public static readonly IReadOnlyList<Type> HideAndSeekCardTypes =
    [
        typeof(HideInWardrobe),
        typeof(FoundYou),
    ];

    public static readonly IReadOnlyList<Type> ShoppingCardTypes =
    [
        typeof(TastySnack),
        typeof(NiceOutfit),
        typeof(FunGame),
    ];

    public static bool IsInLiveStreamMode(Creature creature)
        => creature.GetPowerAmount<LiveStreamModePower>() >= 1m;

    public static int GetTotalFanCount(Player? player)
        => player?.GetRelic<LiveStreamingEquipment>()?.TotalFanCount ?? 0;

    public static bool IsFanOf(Creature target, Creature? fanOwner)
    {
        if (fanOwner == null)
        {
            return false;
        }

        return target.GetPowerInstances<FanPower>().Any(power => power.Applier == fanOwner);
    }

    public static int CountFansOf(Creature ownerCreature, ICombatState combatState)
        => combatState.GetOpponentsOf(ownerCreature)
            .Count(enemy => enemy.IsAlive && IsFanOf(enemy, ownerCreature));

    public static async Task<bool> TryMakeFanAsync(PlayerChoiceContext choiceContext, Player player, Creature target)
    {
        if (player.Creature is not { } fanOwner || IsFanOf(target, fanOwner))
        {
            return false;
        }

        await PowerCmd.Apply<FanPower>(choiceContext, target, 1m, fanOwner, null);
        if (player.GetRelic<LiveStreamingEquipment>() is { } relic)
        {
            relic.TotalFanCount++;
        }

        return true;
    }

    public static async Task MakeAllAliveEnemiesFansAsync(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature?.CombatState is not { } combatState)
        {
            return;
        }

        foreach (Creature enemy in combatState.GetOpponentsOf(player.Creature).Where(creature => creature.IsAlive))
        {
            await TryMakeFanAsync(choiceContext, player, enemy);
        }
    }

    public static IReadOnlyList<CardPoolModel> GetOtherWitchCardPools()
        => OtherWitchCardPools;
}
