using BaseLib.Utils;
using manosaba.Characters.NikaidoHiro;
using Manosaba.Characters.Common.Commands;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Characters.NikaidoHiro.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Manosaba.Characters.NikaidoHiro.Cards
{
    [Pool(typeof(NikaidoHiroCardPool))]
    public class LaboursOfHiro : PathCustomCardModel
    {
        private static bool _vfxPlayedThisSession = false;
        private const string VfxScenePath = "res://Manosaba/scenes/nikaido_hiro/vfx/labours_of_hiro.tscn";
        private const int energyCost = 2;
        private const CardType type = CardType.Power;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<LaboursOfHiroPower>()];
        public override IEnumerable<CardKeyword> CanonicalKeywords => [ManosabaKeywords.Unique];
        public LaboursOfHiro() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        public static void ResetVfxForNewRun()
        {
            _vfxPlayedThisSession = false;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<LaboursOfHiroPower>(Owner.Creature, 1m, Owner.Creature, this);
            if (!_vfxPlayedThisSession)
            {
                ManosabaVfxCmd.PlaySceneAtCombatCenter(VfxScenePath, fitCoverViewport: true);
                SfxCmd.Play("event:/Manosaba/audio/bgm/ai_no_zanshi.mp3", 0.8f);
                _vfxPlayedThisSession = true;
            }
        }

        protected override void OnUpgrade()
        {
            AddKeyword(CardKeyword.Innate);
        }
    }
}
