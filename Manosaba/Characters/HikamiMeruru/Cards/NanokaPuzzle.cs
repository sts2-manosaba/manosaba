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
        private const int EnergyCost = 3;
        private const CardType CardTypeValue = CardType.Skill;
        private const CardRarity Rarity = CardRarity.Rare;
        private const TargetType TargetTypeValue = TargetType.Self;
        private const bool ShouldShowInCardLibrary = true;

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("NanokaPieces", 35)];

        protected override IEnumerable<IHoverTip> ExtraHoverTips => [
            HoverTipFactory.FromCard<NanokaPiece>(),
            HoverTipFactory.FromCard<NanokaHead>(),
            HoverTipFactory.FromCard<NanokaRightArm>(),
            HoverTipFactory.FromCard<NanokaRightLeg>(),
            HoverTipFactory.FromCard<NanokaLeftArm>(),
            HoverTipFactory.FromCard<NanokaLeftLeg>()
        ];

        public NanokaPuzzle() : base(EnergyCost, CardTypeValue, Rarity, TargetTypeValue, ShouldShowInCardLibrary)
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
