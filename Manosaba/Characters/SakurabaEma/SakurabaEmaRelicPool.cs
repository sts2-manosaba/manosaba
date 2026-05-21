using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Characters.Common.Relics;

namespace manosaba.Characters.SakurabaEma;

public class SakurabaEmaRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => SakurabaEma.Color;

    public override string BigEnergyIconPath => LockedCharacterStarterRelicPool.FallbackBigEnergyIconPath;
    public override string TextEnergyIconPath => LockedCharacterStarterRelicPool.FallbackTextEnergyIconPath;
}
