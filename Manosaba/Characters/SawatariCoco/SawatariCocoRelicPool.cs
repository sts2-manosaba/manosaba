using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Characters.Common.Relics;

namespace manosaba.Characters.SawatariCoco;

public class SawatariCocoRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => SawatariCoco.Color;

    public override string BigEnergyIconPath => LockedCharacterStarterRelicPool.FallbackBigEnergyIconPath;
    public override string TextEnergyIconPath => LockedCharacterStarterRelicPool.FallbackTextEnergyIconPath;
}
