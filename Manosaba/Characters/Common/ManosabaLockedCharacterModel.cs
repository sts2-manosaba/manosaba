using BaseLib.Abstracts;
using manosaba.Extensions;

namespace Manosaba.Characters.Common;

/// <summary>
/// Placeholder character shown as locked on the character select screen.
/// Combat assets fall back to <see cref="PlaceholderCharacterModel"/> until fully implemented.
/// </summary>
public abstract class ManosabaLockedCharacterModel : PlaceholderCharacterModel
{
    protected abstract string LockedCharacterId { get; }

    public override bool HideInCompendium => true;

    public override string CustomCharacterSelectIconPath =>
        (LockedCharacterId + "_char_select.png").CharacterImgPath(LockedCharacterId);

    public override string CustomCharacterSelectLockedIconPath =>
        (LockedCharacterId + "_char_select_locked.png").CharacterImgPath(LockedCharacterId);
}
