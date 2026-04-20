using Manosaba.Config;
using MegaCrit.Sts2.Core.Commands;

namespace manosaba.Characters.NatsumeAnan.Cards;

public static class NovelSettingNoahFriendsSfx
{
    private static bool _sfxPlayedThisSession;
    private const string NoahFriendsSfx = "event:/Manosaba/audio/bgm/noah_friends.ogg";

    public static void ResetSfxForNewRun()
    {
        _sfxPlayedThisSession = false;
    }

    public static void TryPlay()
    {
        ManosabaFxPlayMode playMode = ManosabaConfig.NoahFriendsEffectFrequency;
        if (playMode == ManosabaFxPlayMode.Never)
        {
            return;
        }

        if (playMode == ManosabaFxPlayMode.OncePerRun && _sfxPlayedThisSession)
        {
            return;
        }

        SfxCmd.Play(NoahFriendsSfx, 0.8f);

        if (playMode == ManosabaFxPlayMode.OncePerRun)
        {
            _sfxPlayedThisSession = true;
        }
    }
}
