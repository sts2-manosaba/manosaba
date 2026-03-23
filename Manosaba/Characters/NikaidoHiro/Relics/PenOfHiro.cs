using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using manosaba.Characters.NikaidoHiro;
using manosaba.Extensions;
using Manosaba.Characters.Common.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace manosaba.Characters.NikaidoHiro.Relics
{

    [Pool(typeof(NikaidoHiroRelicPool))]
    public sealed class PenOfHiro : CustomRelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Starter;

        private bool _wasUsed;

        public override bool IsUsedUp => _wasUsed;

        [SavedProperty]
        public bool WasUsed
        {
            get
            {
                return _wasUsed;
            }
            set
            {
                AssertMutable();
                _wasUsed = value;
                if (IsUsedUp)
                {
                    base.Status = RelicStatus.Disabled;
                }
            }
        }
        public override bool ShouldDieLate(Creature creature)
        {
            if (creature != base.Owner.Creature)
            {
                return true;
            }

            if (WasUsed)
            {
                return true;
            }

            return false;
        }

        public override async Task AfterPreventingDeath(Creature creature)
        {
            Flash();
            WasUsed = true;
            decimal amount = Math.Max(1m, (decimal)creature.MaxHp * (50m / 100m));
            await CreatureCmd.Heal(creature, amount);
            await PowerCmd.Apply<MajokaPower>(creature, 50, creature, null);
        }

        public override string PackedIconPath
        {
            get
            {
                var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
                return ResourceLoader.Exists(path) ? path : "pen_of_hiro.png".RelicImagePath();
            }
        }

        protected override string PackedIconOutlinePath
        {
            get
            {
                var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();
                return ResourceLoader.Exists(path) ? path : "pen_of_hiro.png".RelicImagePath();
            }
        }

        protected override string BigIconPath
        {
            get
            {
                var path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
                return ResourceLoader.Exists(path) ? path : "pen_of_hiro.png".BigRelicImagePath();
            }
        }
    }
}
