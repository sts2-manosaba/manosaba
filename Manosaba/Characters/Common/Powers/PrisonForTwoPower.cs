using System;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Characters.Common.Powers;

public sealed class PrisonForTwoPower : PathCustomPowerModel
{
    private const string PartnerNetIdVar = "PartnerNetId";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;
    public override int DisplayAmount => 1;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new StringVar(PartnerNetIdVar)];

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _ = cardSource;

        if (applier?.Player?.NetId is { } partnerNetId &&
            DynamicVars[PartnerNetIdVar] is StringVar partnerVar)
        {
            partnerVar.StringValue = partnerNetId.ToString();
        }

        return Task.CompletedTask;
    }

    public static bool ShouldExcludeTarget(Creature owner, Creature candidate)
    {
        if (owner.GetPower<PrisonForTwoPower>() is not PrisonForTwoPower prisonForTwo)
        {
            return false;
        }

        if (prisonForTwo.DynamicVars[PartnerNetIdVar] is not StringVar partnerVar ||
            string.IsNullOrWhiteSpace(partnerVar.StringValue))
        {
            return false;
        }

        return string.Equals(candidate.Player?.NetId.ToString(), partnerVar.StringValue, StringComparison.Ordinal);
    }
}
