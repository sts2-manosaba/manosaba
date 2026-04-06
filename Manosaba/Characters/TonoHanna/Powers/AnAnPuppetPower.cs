using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.TonoHanna.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.TonoHanna.Powers;

/// <summary>Same behavior as <see cref="MegaCrit.Sts2.Core.Models.Powers.PiercingWailPower"/> (temporary Strength loss, restored at turn end); uses mod art <c>an_an_puppet_power.png</c>.</summary>
public sealed class AnAnPuppetPower : ManosabaTemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<AnAnPuppet>();

    protected override bool IsPositive => false;

    public override LocString Title => new LocString("powers", Id.Entry + ".title");
}
