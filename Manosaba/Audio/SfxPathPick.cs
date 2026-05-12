namespace Manosaba.Audio;

/// <summary>
/// Picks one non-empty audio path for <see cref="CharacterSfxBase"/> A-zone getters
/// (<c>CharacterAttack</c> / <c>CharacterCast</c> / <c>CharacterDeath</c>).
/// Each call re-rolls; engine reads <c>AttackSfx</c> / <c>CastSfx</c> / <c>DeathSfx</c> once per trigger where applicable.
/// </summary>
public static class SfxPathPick
{
    public static string? PickRandomNonEmpty(params string?[] candidates) =>
        candidates is not { Length: > 0 } ? null : PickRandomNonEmpty(candidates.AsSpan());

    public static string? PickRandomNonEmpty(ReadOnlySpan<string?> candidates)
    {
        int count = 0;
        foreach (string? c in candidates)
        {
            if (!string.IsNullOrWhiteSpace(c))
                count++;
        }

        if (count == 0)
            return null;

        int pick = Random.Shared.Next(count);
        foreach (string? c in candidates)
        {
            if (string.IsNullOrWhiteSpace(c))
                continue;
            if (pick-- == 0)
                return c;
        }

        return null;
    }
}
