using Godot;
using MegaCrit.Sts2.Core.Nodes;

namespace Manosaba.Audio;

public static class GodotSfxRouter
{
    private static readonly string[] AudioExtensions = [".ogg", ".wav", ".mp3"];
    private const string BgmEventPrefix = "event:/Manosaba/audio/bgm/";
    private static AudioStreamPlayer? _customBgmPlayer;
    private static string? _currentBgmEvent;

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
        if (_customBgmPlayer != null && GodotObject.IsInstanceValid(_customBgmPlayer))
        {
            if (_currentBgmEvent == eventPath && _customBgmPlayer.IsPlaying())
            {
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
            VolumeLinear = volume,
            Bus = "Master"
        };
        _customBgmPlayer.Finished += () =>
        {
            _customBgmPlayer?.QueueFree();
            _customBgmPlayer = null;
            _currentBgmEvent = null;
        };

        game.AddChild(_customBgmPlayer);
        _customBgmPlayer.Play();
        _currentBgmEvent = eventPath;
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
    }
}
