using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;

namespace Manosaba.Patches;

/// <summary>
/// Vanilla treats every <see cref="ModelDb.AllCharacters"/> entry as required to unlock random select.
/// Manosaba locked placeholders stay in <c>AllCharacters</c> but are excluded from <see cref="UnlockState.Characters"/>.
/// </summary>
internal static class ManosabaCharacterSelectRandomGate
{
    internal static bool CountsTowardRandomUnlockCheck(CharacterModel character) =>
        Patch_UnlockState_ManosabaLockedCharacters.CountsAsPlayableCharacter(character);

    internal static bool AreAllPlayableCharactersUnlocked(UnlockState unlockState)
    {
        foreach (CharacterModel c in ModelDb.AllCharacters)
        {
            if (!CountsTowardRandomUnlockCheck(c))
            {
                continue;
            }

            if (!unlockState.Characters.Contains(c))
            {
                return false;
            }
        }

        return true;
    }

    internal static bool ShouldShowRandomForLobbyPlayers(IEnumerable<LobbyPlayer> players)
    {
        foreach (LobbyPlayer player in players)
        {
            UnlockState unlockState = UnlockState.FromSerializable(player.unlockState);
            if (AreAllPlayableCharactersUnlocked(unlockState))
            {
                return true;
            }
        }

        return false;
    }
}
