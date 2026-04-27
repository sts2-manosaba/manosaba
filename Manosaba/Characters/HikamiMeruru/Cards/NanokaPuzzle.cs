using BaseLib.Utils;
using manosaba.Characters.HikamiMeruru;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.HikamiMeruru.Cards
{
    [Pool(typeof(HikamiMeruruCardPool))]
    public class NanokaPuzzle : PathCustomCardModel
    {
        private const int energyCost = 3;
        private const CardType type = CardType.Skill;
        private const CardRarity rarity = CardRarity.Rare;
        private const TargetType targetType = TargetType.Self;
        private const bool shouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("NanokaPieces", 35)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromCard<NanokaComplete>()
        ];

        public NanokaPuzzle() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (CombatState == null)
                return;

            List<CardModel> drawPileCards = PileType.Draw.GetPile(Owner).Cards.ToList();
            foreach (CardModel card in drawPileCards)
            {
                await CardPileCmd.RemoveFromCombat(card);
            }

            List<CardModel> handPileCards = PileType.Hand.GetPile(Owner).Cards.ToList();
            foreach (CardModel card in handPileCards)
            {
                await CardPileCmd.RemoveFromCombat(card);
            }

            List<CardModel> puzzleCards = [];
            puzzleCards.AddRange(NanokaPiece.Create(Owner, DynamicVars["NanokaPieces"].IntValue, CombatState));
            puzzleCards.Add(CombatState.CreateCard<NanokaHead>(Owner));
            puzzleCards.Add(CombatState.CreateCard<NanokaRightArm>(Owner));
            puzzleCards.Add(CombatState.CreateCard<NanokaRightLeg>(Owner));
            puzzleCards.Add(CombatState.CreateCard<NanokaLeftArm>(Owner));
            puzzleCards.Add(CombatState.CreateCard<NanokaLeftLeg>(Owner));

            IReadOnlyList<CardPileAddResult> results = await CardPileCmd.AddGeneratedCardsToCombat(
                puzzleCards,
                PileType.Draw,
                addedByPlayer: true,
                CardPilePosition.Random
            );
            CardCmd.PreviewCardPileAdd(results);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["NanokaPieces"].UpgradeValueBy(-10);
        }
    }
}
