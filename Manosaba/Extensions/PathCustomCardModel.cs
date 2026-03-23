using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manosaba.Extensions
{
    public abstract class PathCustomCardModel : CustomCardModel
    {
        public override string PortraitPath => $"res://Manosaba/images/cards/{Id.Entry.ToLowerInvariant()}.png";

        public PathCustomCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
        {
        }
    }
}
