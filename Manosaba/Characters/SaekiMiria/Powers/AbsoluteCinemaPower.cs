using Manosaba.Characters.SaekiMiria.Cards;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.SaekiMiria.Powers;

public sealed class AbsoluteCinemaPower : PathCustomPowerModel
{
    private bool _consumed;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override bool AllowNegative => false;
    public override int DisplayAmount => Amount;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.ReplayStatic)];

    public async Task<bool> TryApplyToMovieAsync(CardModel card)
    {
        if (_consumed || Amount <= 0m || card is not MovieBase movie)
        {
            return false;
        }

        movie.BaseReplayCount += Amount;
        _consumed = true;
        await PowerCmd.Remove(this);
        return true;
    }
}
