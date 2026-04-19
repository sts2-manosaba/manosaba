using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.KurobeNanoka.Helpers;
using Manosaba.Characters.KurobeNanoka.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.KurobeNanoka.Cards;

[Pool(typeof(KurobeNanokaCardPool))]
public class SorryIMissed : GunBase
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.GunShot];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BulletCost", 1m),
        new PowerVar<AccuratePower>(150m),
    ];

    public SorryIMissed() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!TrySpendBulletsOnPlay())
            return;

        NanokaHelper.PlayGunFireSfx();
        await PowerCmd.Apply<AccuratePower>(Owner.Creature, DynamicVars["AccuratePower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AccuratePower"].UpgradeValueBy(50m);
    }
}
