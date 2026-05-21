using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Characters.Common.Relics;

namespace manosaba.Characters.TsukishiroYuki;

public class TsukishiroYukiRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => TsukishiroYuki.Color;

    public override string BigEnergyIconPath => LockedCharacterStarterRelicPool.FallbackBigEnergyIconPath;
    public override string TextEnergyIconPath => LockedCharacterStarterRelicPool.FallbackTextEnergyIconPath;
}
