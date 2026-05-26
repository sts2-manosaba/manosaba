using BaseLib.Abstracts;
using manosaba.Extensions;

namespace Manosaba.Characters.Common.Relics;

/// <summary>
/// Holds <see cref="LockedCharacterStarterRelic"/> and is the <c>RelicPool</c> for all
/// <see cref="ManosabaLockedCharacterModel"/> slots until each character is fully implemented.
/// </summary>
public sealed class LockedCharacterStarterRelicPool : CustomRelicPoolModel
{
    public override string BigEnergyIconPath => "charui/manosaba_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/manosaba_energy_text.png".ImagePath();
}
