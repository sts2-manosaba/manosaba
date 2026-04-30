using BaseLib.Utils;
using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Characters.SaekiMiria.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Manosaba.Characters.SaekiMiria.Helper;

public static class MovieCardGenerator
{
    public static readonly IReadOnlyList<Func<Player, CombatState, MovieBase>> DefaultMovieFactories =
    [
        static (player, combatState) => combatState.CreateCard<HorrorMovie>(player),
        static (player, combatState) => combatState.CreateCard<ComedyMovie>(player),
        static (player, combatState) => combatState.CreateCard<FantasyMovie>(player),
        static (player, combatState) => combatState.CreateCard<ActionMovie>(player),
        static (player, combatState) => combatState.CreateCard<RomanticMovie>(player),
        static (player, combatState) => combatState.CreateCard<SpyMovie>(player),
    ];

    public static Task<MovieBase> CreateRandomMovieAsync(
        Player player,
        CombatState combatState,
        Rng rng,
        bool upgradeMovie = false,
        bool applyAbsoluteCinema = true,
        IReadOnlyList<Func<Player, CombatState, MovieBase>>? movieFactories = null)
        => CreateRandomMovieInternalAsync(
            player,
            combatState,
            rng,
            upgradeMovie,
            applyAbsoluteCinema,
            movieFactories ?? DefaultMovieFactories);

    public static async Task<CardModel> CreateCabinetKeyMovieAsync(
        Player player,
        CombatState combatState,
        Rng rng,
        int smallPaperChance,
        bool upgradeMovie,
        bool applyAbsoluteCinema = true,
        IReadOnlyList<Func<Player, CombatState, MovieBase>>? movieFactories = null)
    {
        if (smallPaperChance > 0 && rng.NextInt(smallPaperChance) == 0)
        {
            var smallPaper = combatState.CreateCard<SmallPaper>(player);
            if (upgradeMovie)
            {
                CardCmd.Upgrade(smallPaper);
            }

            await TryApplyAbsoluteCinemaAsync(player, smallPaper, applyAbsoluteCinema);
            return smallPaper;
        }

        return await CreateRandomMovieInternalAsync(
            player,
            combatState,
            rng,
            upgradeMovie,
            applyAbsoluteCinema,
            movieFactories ?? DefaultMovieFactories);
    }

    public static async Task TryApplyAbsoluteCinemaAsync(Player player, CardModel card, bool applyAbsoluteCinema = true)
    {
        if (!applyAbsoluteCinema)
        {
            return;
        }

        if (player.Creature.GetPower<AbsoluteCinemaPower>() is { } absoluteCinemaPower)
        {
            await absoluteCinemaPower.TryApplyToMovieAsync(card);
        }
    }

    private static async Task<MovieBase> CreateRandomMovieInternalAsync(
        Player player,
        CombatState combatState,
        Rng rng,
        bool upgradeMovie,
        bool applyAbsoluteCinema,
        IReadOnlyList<Func<Player, CombatState, MovieBase>> movieFactories)
    {
        Func<Player, CombatState, MovieBase>? factory = rng.NextItem(movieFactories);
        MovieBase movie = (factory ?? movieFactories[0])(player, combatState);

        if (upgradeMovie)
        {
            CardCmd.Upgrade(movie);
        }

        await TryApplyAbsoluteCinemaAsync(player, movie, applyAbsoluteCinema);
        return movie;
    }
}
