using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Manosaba.Characters.HikamiMeruru.Cards
{
    [Pool(typeof(HikamiMeruruCardPool))]
    public class HikamiMeruruExaid : PathCustomCardModel
    {
        private const int energyCost = 3;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MajokaPower>(70)];

        public HikamiMeruruExaid() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<MajokaPower>(base.Owner.Creature, base.DynamicVars["MajokaPower"].BaseValue, base.Owner.Creature, this);
            PlayerCmd.EndTurn(base.Owner, canBackOut: false);
            SfxCmd.Play("event:/Manosaba/audio/bgm/hikami_meruru_exaid.mp3", 0.2f);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["MajokaPower"].UpgradeValueBy(30);
        }
    }
}
