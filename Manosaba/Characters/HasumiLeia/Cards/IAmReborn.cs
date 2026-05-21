using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Config;
using Manosaba.Characters.HasumiLeia.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class IAmReborn : PathCustomCardModel
{
    private static bool _sfxPlayedThisSession;
    private const string BgmEventPath = "event:/Manosaba/audio/bgm/i_am_reborn.mp3";

    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
        HoverTipFactory.FromPower<IAmRebornPower>(),
        HoverTipFactory.FromPower<DoubleDamagePower>(),
        HoverTipFactory.FromPower<DrawCardsNextTurnPower>(),
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public IAmReborn()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public static void ResetSfxForNewRun()
    {
        _sfxPlayedThisSession = false;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        ManosabaFxPlayMode sfxPlayMode = ManosabaConfig.IAmRebornEffectFrequency;
        if (sfxPlayMode != ManosabaFxPlayMode.Never &&
            (sfxPlayMode != ManosabaFxPlayMode.OncePerRun || !_sfxPlayedThisSession))
        {
            SfxCmd.Play(BgmEventPath, 0.8f);
            if (sfxPlayMode == ManosabaFxPlayMode.OncePerRun)
            {
                _sfxPlayedThisSession = true;
            }
        }

        await CardCmd.Discard(choiceContext, PileType.Hand.GetPile(Owner).Cards);
        await CommonActions.Apply<IAmRebornPower>(choiceContext, ownerCreature, this, 1m, silent: true);
        await CommonActions.Apply<DrawCardsNextTurnPower>(choiceContext, ownerCreature, this, 5m, silent: true);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
