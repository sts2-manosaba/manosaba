using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Config;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class CeremonialSword : PathCustomCardModel
{
    private static bool _sfxPlayedThisSession;

    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<StrengthPower>(),
        HoverTipFactory.FromPower<SecondSwordPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(2m)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public CeremonialSword()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public static void ResetSfxForNewRun()
    {
        _sfxPlayedThisSession = false;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        ManosabaFxPlayMode sfxPlayMode = ManosabaConfig.UnsheatheBiosEffectFrequency;
        if (sfxPlayMode != ManosabaFxPlayMode.Never &&
            (sfxPlayMode != ManosabaFxPlayMode.OncePerRun || !_sfxPlayedThisSession))
        {
            SfxCmd.Play("event:/Manosaba/audio/bgm/Bios.mp3", 0.8f);
            if (sfxPlayMode == ManosabaFxPlayMode.OncePerRun)
            {
                _sfxPlayedThisSession = true;
            }
        }

        await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<SecondSwordPower>(Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Strength.UpgradeValueBy(2m);
    }
}

