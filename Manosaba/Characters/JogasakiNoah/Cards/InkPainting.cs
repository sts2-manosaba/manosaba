using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;
using Manosaba.Characters.Common.Powers;
using Manosaba.Characters.JogasakiNoa.Orbs;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Manosaba.Characters.JogasakiNoah.Cards;

[Pool(typeof(JogasakiNoahCardPool))]
public class InkPainting : PathCustomCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromCard<InkPaintingBlackPaintOrbToken>(IsUpgraded),
        HoverTipFactory.FromCard<InkPaintingWhitePaintOrbToken>(IsUpgraded)
    ];

    public InkPainting() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Creature.CombatState == null)
        {
            return;
        }

        List<CardModel> options =
        [
            CreateToken<InkPaintingBlackPaintOrbToken>(),
            CreateToken<InkPaintingWhitePaintOrbToken>()
        ];

        CardModel? selectedToken = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner);
        if (selectedToken == null)
        {
            return;
        }

        await CardCmd.AutoPlay(choiceContext, selectedToken, null);

        T CreateToken<T>() where T : CardModel
        {
            T token = Owner.Creature.CombatState.CreateCard<T>(Owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(token);
            }

            return token;
        }
    }

    protected override void OnUpgrade()
    {
    }
}

[Pool(typeof(TokenCardPool))]
public class InkPaintingBlackPaintOrbToken : PathCustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<BlackPaintOrb>(), HoverTipFactory.FromPower<MajokaPower>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MajokaPower>(10m)];

    public InkPaintingBlackPaintOrbToken() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await OrbCmd.Channel(choiceContext, ModelDb.Orb<BlackPaintOrb>().ToMutable(), Owner);
        await PowerCmd.Apply<MajokaPower>(Owner.Creature, DynamicVars["MajokaPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["MajokaPower"].UpgradeValueBy(5m);
    }
}

[Pool(typeof(TokenCardPool))]
public class InkPaintingWhitePaintOrbToken : PathCustomCardModel
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = false;
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<WhitePaintOrb>()];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    public InkPaintingWhitePaintOrbToken() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await OrbCmd.Channel(choiceContext, ModelDb.Orb<WhitePaintOrb>().ToMutable(), Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
