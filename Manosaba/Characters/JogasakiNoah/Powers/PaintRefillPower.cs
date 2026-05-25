using manosaba.Extensions;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.JogasakiNoah.Powers;

public class PaintRefillPower : PathCustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;

    public override string CustomPackedIconPath => "power.png".PowerImagePath();
    public override string CustomBigIconPath => "power.png".PowerImagePath();
    public override string CustomBigBetaIconPath => "power.png".PowerImagePath();

    public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
    {
        _ = targets;
        if (Amount < 1m || Owner?.Player is not { } player || orb.Owner != player || !IsPaintOrb(orb))
        {
            return;
        }

        Flash();
        await CardPileCmd.Draw(choiceContext, 1m, player);
    }

    private static bool IsPaintOrb(OrbModel orb) =>
        orb is RedPaintOrb
        or BluePaintOrb
        or YellowPaintOrb
        or OrangePaintOrb
        or GreenPaintOrb
        or PurplePaintOrb
        or BlackPaintOrb
        or WhitePaintOrb;
}
