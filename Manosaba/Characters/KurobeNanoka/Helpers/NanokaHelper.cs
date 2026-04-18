using BaseLib.Utils;
using manosaba.Characters.KurobeNanoka;
using manosaba.Characters.KurobeNanoka.Relics;
using Manosaba.Characters.Common.Overrides;
using Manosaba.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.ValueProps;
namespace Manosaba.Characters.KurobeNanoka.Helpers
{
    internal class NanokaHelper
    {
        public static string GUN_SHOT_SFX = "event:/Manosaba/audio/SFX/gun_shot.ogg";

        public static void PlayGunFireSfx(float volume = 5)
        {
            SfxCmd.Play(GUN_SHOT_SFX, volume);
        }

        public static void PlayRewardSfx(float volume = 1) {
            SfxCmd.Play("event:/Manosaba/audio/SFX/reward.mp3", volume);
        }

        public static void PlayRollFailSfx(float volume = 10)
        {
            SfxCmd.Play("event:/Manosaba/audio/SFX/glass_shatter.ogg", volume);
        }

        public static void PlayAWPSfx(float volume = 3)
        {
            SfxCmd.Play("event:/Manosaba/audio/SFX/AWP.mp3", volume);
        }
    }
}
