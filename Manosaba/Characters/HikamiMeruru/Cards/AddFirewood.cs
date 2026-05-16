using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using manosaba.Characters.HikamiMeruru.Relics;
using Manosaba.Characters.HikamiMeruru.Potions;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HikamiMeruru.Cards;

[Pool(typeof(HikamiMeruruCardPool))]
public class AddFirewood : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Firepower", 10m)];

    public AddFirewood() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = choiceContext;
        _ = cardPlay;

        WitchesCauldron? cauldron = Owner.GetRelic<WitchesCauldron>();
        if (cauldron == null)
        {
            return;
        }

        if (cauldron.Firepower >= WitchesCauldron.MaxFirepower)
        {
            await PotionCmd.TryToProcure<Catalyst>(Owner);
            return;
        }

        cauldron.AddFirepower(DynamicVars["Firepower"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Firepower"].UpgradeValueBy(5m);
    }
}
