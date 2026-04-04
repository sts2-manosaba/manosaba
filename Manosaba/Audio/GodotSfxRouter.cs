using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Saves;

namespace Manosaba.Audio;

public static class GodotSfxRouter
{
    private static readonly string[] AudioExtensions = [".ogg", ".wav", ".mp3"];
    private const string BgmEventPrefix = "event:/Manosaba/audio/bgm/";
    private static AudioStreamPlayer? _customBgmPlayer;
    private static string? _currentBgmEvent;
    private static float _currentBgmBaseVolume = 1f;
    private static bool _vanillaBgmDucked;

    public static bool TryPlay(string eventPath, float volume = 1f)
    {
        if (!TryResolveAudioPath(eventPath, out string? audioPath))
        {
            return false;
        }

        AudioStream? stream = ResourceLoader.Load<AudioStream>(audioPath);
        if (stream == null)
        {
            GD.PushWarning("[Manosaba] Missing Godot audio file for event: " + eventPath + " -> " + audioPath);
            return true;
        }

        NGame? game = NGame.Instance;
        if (game == null)
        {
            return true;
        }

        if (eventPath.StartsWith(BgmEventPrefix))
        {
            PlayCustomBgm(game, eventPath, stream, volume);
            return true;
        }

        var player = new AudioStreamPlayer
        {
            Name = "ManosabaSfxPlayer",
            Stream = stream,
            VolumeLinear = volume,
            Bus = "SFX"
        };

        game.AddChild(player);
        player.Finished += () => player.QueueFree();
        player.Play();
        return true;
    }

    private static bool TryResolveAudioPath(string eventPath, out string? audioPath)
    {
        const string eventPrefix = "event:/Manosaba";
        if (!eventPath.StartsWith(eventPrefix))
        {
            audioPath = null;
            return false;
        }

        string baseResPath = "res://" + eventPath["event:/".Length..];

        if (ResourceLoader.Exists(baseResPath))
        {
            audioPath = baseResPath;
            return true;
        }

        foreach (string ext in AudioExtensions)
        {
            string candidate = baseResPath + ext;
            if (ResourceLoader.Exists(candidate))
            {
                audioPath = candidate;
                return true;
            }
        }

        // Keep warning readable; loader will fail and report this candidate.
        audioPath = baseResPath + ".ogg";
        return true;
    }

    private static void PlayCustomBgm(NGame game, string eventPath, AudioStream stream, float volume)
    {
        PauseVanillaBgm();
        _currentBgmBaseVolume = volume;
        float effectiveVolume = volume * GetConfiguredBgmVolume();

        if (_customBgmPlayer != null && GodotObject.IsInstanceValid(_customBgmPlayer))
        {
            if (_currentBgmEvent == eventPath && _customBgmPlayer.IsPlaying())
            {
                _customBgmPlayer.VolumeLinear = effectiveVolume;
                return;
            }

            _customBgmPlayer.Stop();
            _customBgmPlayer.QueueFree();
            _customBgmPlayer = null;
        }


        _customBgmPlayer = new AudioStreamPlayer
        {
            Name = "ManosabaCustomBgmPlayer",
            Stream = stream,
            VolumeLinear = effectiveVolume,
            Bus = "Master"
        };
        _customBgmPlayer.Finished += () =>
        {
            _customBgmPlayer?.QueueFree();
            _customBgmPlayer = null;
            _currentBgmEvent = null;
            _currentBgmBaseVolume = 1f;
            ResumeVanillaBgm();
        };

        game.AddChild(_customBgmPlayer);
        _customBgmPlayer.Play();
        _currentBgmEvent = eventPath;
    }

    private static float GetConfiguredBgmVolume()
    {
        try
        {
            // Mirror NAudioManager.SetBgmVol's response curve.
            float bgm = SaveManager.Instance.SettingsSave.VolumeBgm;
            return Mathf.Pow(Mathf.Clamp(bgm, 0f, 1f), 2f);
        }
        catch
        {
            return 1f;
        }
    }

    public static void StopCustomBgmAndResumeVanilla()
    {
        if (_customBgmPlayer != null && GodotObject.IsInstanceValid(_customBgmPlayer))
        {
            _customBgmPlayer.Stop();
            _customBgmPlayer.QueueFree();
            _customBgmPlayer = null;
        }
        _currentBgmEvent = null;
        _currentBgmBaseVolume = 1f;
        ResumeVanillaBgm();
    }

    public static void RefreshCustomBgmVolumeFromSettings()
    {
        if (_customBgmPlayer != null && GodotObject.IsInstanceValid(_customBgmPlayer))
        {
            _customBgmPlayer.VolumeLinear = _currentBgmBaseVolume * GetConfiguredBgmVolume();
        }
    }

    private static void PauseVanillaBgm()
    {
        if (_vanillaBgmDucked)
        {
            return;
        }

        NAudioManager.Instance?.SetBgmVol(0f);
        _vanillaBgmDucked = true;
    }

    private static void ResumeVanillaBgm()
    {
        if (!_vanillaBgmDucked)
        {
            return;
        }

        NAudioManager.Instance?.SetBgmVol(GetConfiguredBgmVolumeRaw());
        _vanillaBgmDucked = false;
    }

    private static float GetConfiguredBgmVolumeRaw()
    {
        try
        {
            return Mathf.Clamp(SaveManager.Instance.SettingsSave.VolumeBgm, 0f, 1f);
        }
        catch
        {
            return 1f;
        }
    }
}
