using System;
using BaseLib.Extensions;
using BaseLib.Utils;
using manosaba.Characters.HasumiLeia;
using manosaba.Extensions;
using Manosaba.Characters.Common.Powers;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using HasumiLeiaCharacter = manosaba.Characters.HasumiLeia.HasumiLeia;

namespace Manosaba.Characters.HasumiLeia.Cards;

[Pool(typeof(HasumiLeiaCardPool))]
public sealed class TheCenterOfTheWorldIs : PathCustomCardModel
{
    private const int energyCost = 3;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public TheCenterOfTheWorldIs()
        : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        _ = cardPlay;

        string characterId = (Owner.Character?.Id.ToString() ?? string.Empty).RemovePrefix().ToLowerInvariant();
        if (characterId != HasumiLeiaCharacter.CharacterId)
        {
            return;
        }

        if (CombatState?.Players == null)
        {
            return;
        }

        bool anyChoseLeia = false;
        foreach (Player player in CombatState.Players)
        {
            if (player == Owner)
            {
                continue;
            }

            if (player == null || player.Creature == null || !player.Creature.IsAlive)
            {
                continue;
            }

            List<CardModel> options =
            [
                CombatState.CreateCard<TheCenterOfTheWorldIs_LeiaChoice>(Owner),
                CombatState.CreateCard<TheCenterOfTheWorldIs_IgnoreChoice>(Owner),
            ];

            CardModel? selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, player, canSkip: true);
            if (selected is TheCenterOfTheWorldIs_LeiaChoice)
            {
                anyChoseLeia = true;
            }
        }

        if (Owner.Creature is not { } leiaCreature)
        {
            return;
        }

        if (anyChoseLeia)
        {
            await PowerCmd.Apply<StrengthPower>(leiaCreature, 4m, leiaCreature, this);
            return;
        }

        decimal currentMajoka = leiaCreature.GetPowerAmount<MajokaPower>();
        decimal toApply = Math.Max(0m, 100m - currentMajoka);
        if (toApply > 0m)
        {
            await PowerCmd.Apply<MajokaPower>(leiaCreature, toApply, leiaCreature, this);
        }
    }
}

[Pool(typeof(TokenCardPool))]
public sealed class TheCenterOfTheWorldIs_LeiaChoice : PathCustomCardModel
{
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override string PortraitPath => "the_center_of_the_world_is_leia_choice.png".CardsImagePath();

    public TheCenterOfTheWorldIs_LeiaChoice()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.None, shouldShowInCardLibrary: false)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
}

[Pool(typeof(TokenCardPool))]
public sealed class TheCenterOfTheWorldIs_IgnoreChoice : PathCustomCardModel
{
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override string PortraitPath => "the_center_of_the_world_is_ignore_choice.png".CardsImagePath();

    public TheCenterOfTheWorldIs_IgnoreChoice()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.None, shouldShowInCardLibrary: false)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;
}
