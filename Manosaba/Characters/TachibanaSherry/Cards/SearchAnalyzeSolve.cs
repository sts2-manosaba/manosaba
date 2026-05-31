using BaseLib.Utils;
using manosaba.Characters.TachibanaSherry;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.TachibanaSherry.Powers;
using Manosaba.Characters.TachibanaSherry.Visuals;
using Manosaba.Config;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Manosaba.Characters.TachibanaSherry.Cards;

[Pool(typeof(TachibanaSherryCardPool))]
public sealed class SearchAnalyzeSolve : PathCustomCardModel
{
    private static bool _sfxPlayedThisSession;
    private const int energyCost = 1;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<CouldItBeThatSkillPower>(),
    ];

    public SearchAnalyzeSolve() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
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
        if (Owner?.Creature is not { } ownerCreature)
        {
            return;
        }

        CouldItBeThatSkillPower? existingPower = ownerCreature.GetPower<CouldItBeThatSkillPower>();
        if (existingPower != null)
        {
            existingPower.OnDuplicateCardPlayed();
        }
        else
        {
            await PowerCmd.Apply<CouldItBeThatSkillPower>(ownerCreature, 1m, ownerCreature, this);
        }

        CouldItBeThatSkillButtonUi.EnsureShown(Owner);

        ManosabaFxPlayMode sfxPlayMode = ManosabaConfig.SearchAnalyzeSolveEffectFrequency;
        if (sfxPlayMode == ManosabaFxPlayMode.Never)
        {
            return;
        }

        if (sfxPlayMode == ManosabaFxPlayMode.OncePerRun && _sfxPlayedThisSession)
        {
            return;
        }

        SfxCmd.Play("event:/Manosaba/audio/bgm/search_analyze_solve.mp3", 0.8f);

        if (sfxPlayMode == ManosabaFxPlayMode.OncePerRun)
        {
            _sfxPlayedThisSession = true;
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
