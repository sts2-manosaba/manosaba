using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using Manosaba.Config;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class Pride : PathCustomCardModel
{
    private static bool _sfxPlayedThisSession;
    private const string BgmEventPath = "event:/Manosaba/audio/bgm/pride.mp3";

    public override bool GainsBlock => true;
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<Crowning>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(30m, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public Pride()
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

        ManosabaFxPlayMode sfxPlayMode = ManosabaConfig.PrideEffectFrequency;
        if (sfxPlayMode != ManosabaFxPlayMode.Never &&
            (sfxPlayMode != ManosabaFxPlayMode.OncePerRun || !_sfxPlayedThisSession))
        {
            SfxCmd.Play(BgmEventPath, 0.8f);
            if (sfxPlayMode == ManosabaFxPlayMode.OncePerRun)
            {
                _sfxPlayedThisSession = true;
            }
        }

        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);

        if (CombatState == null)
        {
            return;
        }

        var crowning = CombatState.CreateCard<Crowning>(Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(crowning, PileType.Discard, addedByPlayer: true));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(10m);
    }
}
