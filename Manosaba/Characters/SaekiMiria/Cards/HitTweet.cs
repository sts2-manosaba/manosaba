using System;
using BaseLib.Extensions;
using BaseLib.Utils;
using manosaba.Characters.SaekiMiria;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using HasumiLeiaCharacter = manosaba.Characters.HasumiLeia.HasumiLeia;
using SaekiMiriaCharacter = manosaba.Characters.SaekiMiria.SaekiMiria;

namespace Manosaba.Characters.SaekiMiria.Cards;

[Pool(typeof(SaekiMiriaCardPool))]
public sealed class HitTweet : PathCustomCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    private const string MajokaGainVar = "MajokaGain";
    private const string StrengthGainVar = "StrengthGain";

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<MajokaPower>(),
        HoverTipFactory.FromPower<StrengthPower>(),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(MajokaGainVar, 100m),
        new DynamicVar(StrengthGainVar, 5m),
    ];

    public HitTweet()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        string characterId = (Owner.Character?.Id.ToString() ?? string.Empty).RemovePrefix().ToLowerInvariant();
        if (characterId == SaekiMiriaCharacter.CharacterId)
        {
            decimal currentMajoka = Owner.Creature.GetPowerAmount<MajokaPower>();
            decimal toApply = Math.Max(0m, DynamicVars[MajokaGainVar].BaseValue - currentMajoka);
            if (toApply > 0m)
            {
                await CommonActions.Apply<MajokaPower>(choiceContext, Owner.Creature, this, toApply);
            }
        }
        else if (characterId == HasumiLeiaCharacter.CharacterId)
        {
            await CommonActions.Apply<StrengthPower>(choiceContext, Owner.Creature, this, DynamicVars[StrengthGainVar].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
