using Godot;
using System;
using System.Reflection;
using MegaCrit.Sts2.Core.Nodes;

namespace Manosaba.Characters.Common.Commands;

public static class ManosabaVideoCmd
{
    private static bool IsAudioPath(string path)
    {
        string ext = System.IO.Path.GetExtension(path)?.ToLowerInvariant() ?? "";
        return ext is ".ogg" or ".wav" or ".mp3";
    }

    private static VideoStream? TryLoadStream(string videoPath)
    {
        if (IsAudioPath(videoPath))
        {
            return null;
        }

        // First attempt: normal resource load (requires importer metadata).
        try
        {
            VideoStream? loaded = ResourceLoader.Load<VideoStream>(videoPath);
            if (loaded != null)
            {
                return loaded;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ManosabaVideoCmd] ResourceLoader.Load threw: " + ex);
        }

        // Fallback: try to instantiate a video stream type directly (works in some Godot builds even without .import).
        // We use reflection so the mod still builds if a particular stream type is not available in the game's Godot build.
        VideoStream? webm = TryCreateVideoStream("VideoStreamWebm", videoPath);
        if (webm != null)
        {
            Console.WriteLine("[ManosabaVideoCmd] Using reflection fallback: VideoStreamWebm");
            return webm;
        }

        VideoStream? theora = TryCreateVideoStream("VideoStreamTheora", videoPath);
        if (theora != null)
        {
            Console.WriteLine("[ManosabaVideoCmd] Using reflection fallback: VideoStreamTheora");
            return theora;
        }

        return null;
    }

    private static AudioStream? TryLoadAudio(string audioPath)
    {
        try
        {
            return ResourceLoader.Load<AudioStream>(audioPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ManosabaVideoCmd] ResourceLoader.Load<AudioStream> threw: " + ex);
            return null;
        }
    }

    private static VideoStream? TryCreateVideoStream(string godotTypeName, string videoPath)
    {
        try
        {
            Type? t =
                Type.GetType("Godot." + godotTypeName + ", GodotSharp") ??
                Type.GetType("Godot." + godotTypeName + ", Godot");

            if (t == null || !typeof(VideoStream).IsAssignableFrom(t))
            {
                return null;
            }

            object? instance = Activator.CreateInstance(t);
            if (instance is not VideoStream stream)
            {
                return null;
            }

            // Most built-in video stream resources expose a "File" property.
            PropertyInfo? fileProp =
                t.GetProperty("File", BindingFlags.Instance | BindingFlags.Public) ??
                t.GetProperty("file", BindingFlags.Instance | BindingFlags.Public);

            if (fileProp?.CanWrite == true)
            {
                fileProp.SetValue(stream, videoPath);
            }
            else
            {
                MethodInfo? setFile =
                    t.GetMethod("SetFile", BindingFlags.Instance | BindingFlags.Public) ??
                    t.GetMethod("set_file", BindingFlags.Instance | BindingFlags.Public);

                if (setFile != null)
                {
                    setFile.Invoke(stream, [videoPath]);
                }
            }

            return stream;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ManosabaVideoCmd] Reflection fallback failed for {godotTypeName}: {ex}");
            return null;
        }
    }

    public static void PlayFullscreenOneShot(string videoPath, bool chromaKeyGreen = false)
    {
        Console.WriteLine($"[ManosabaVideoCmd] PlayFullscreenOneShot path='{videoPath}' chromaKeyGreen={chromaKeyGreen}");

        if (string.IsNullOrWhiteSpace(videoPath))
        {
            Console.WriteLine("[ManosabaVideoCmd] Aborted: empty videoPath");
            return;
        }

        NGame? game = NGame.Instance;
        if (game == null)
        {
            Console.WriteLine("[ManosabaVideoCmd] Aborted: NGame.Instance is null");
            return;
        }

        try
        {
            Console.WriteLine("[ManosabaVideoCmd] Global path = " + ProjectSettings.GlobalizePath(videoPath));
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ManosabaVideoCmd] ProjectSettings.GlobalizePath threw: " + ex.Message);
        }

        if (IsAudioPath(videoPath))
        {
            Console.WriteLine("[ManosabaVideoCmd] Detected audio path; playing audio only");
            AudioStream? audio = TryLoadAudio(videoPath);
            if (audio == null)
            {
                GD.PushWarning("[Manosaba] Missing audio: " + videoPath);
                Console.WriteLine("[ManosabaVideoCmd] Aborted: could not load AudioStream");
                Console.WriteLine("[ManosabaVideoCmd] FileAccess.FileExists(res://) = " + Godot.FileAccess.FileExists(videoPath));
                return;
            }

            var audioPlayer = new AudioStreamPlayer
            {
                Name = "ManosabaMediaAudioPlayer",
                Stream = audio,
                Bus = "SFX"
            };

            game.AddChild(audioPlayer);
            audioPlayer.Finished += () =>
            {
                Console.WriteLine("[ManosabaVideoCmd] Audio finished; freeing AudioStreamPlayer");
                if (GodotObject.IsInstanceValid(audioPlayer))
                {
                    audioPlayer.QueueFree();
                }
            };

            Console.WriteLine("[ManosabaVideoCmd] Starting audio playback");
            audioPlayer.Play();
            return;
        }

        VideoStream? stream = TryLoadStream(videoPath);
        if (stream == null)
        {
            Console.WriteLine("[ManosabaVideoCmd] VideoStream load failed; attempting AudioStream fallback");
            AudioStream? audio = TryLoadAudio(videoPath);
            if (audio == null)
            {
                GD.PushWarning("[Manosaba] Missing video/audio: " + videoPath);
                Console.WriteLine("[ManosabaVideoCmd] Aborted: no VideoStream or AudioStream could be loaded");
                Console.WriteLine("[ManosabaVideoCmd] FileAccess.FileExists(res://) = " + Godot.FileAccess.FileExists(videoPath));
                return;
            }

            var audioPlayer = new AudioStreamPlayer
            {
                Name = "ManosabaMediaAudioPlayer",
                Stream = audio,
                Bus = "SFX"
            };

            game.AddChild(audioPlayer);
            audioPlayer.Finished += () =>
            {
                Console.WriteLine("[ManosabaVideoCmd] Audio finished; freeing AudioStreamPlayer");
                if (GodotObject.IsInstanceValid(audioPlayer))
                {
                    audioPlayer.QueueFree();
                }
            };

            Console.WriteLine("[ManosabaVideoCmd] Playing AudioStream fallback");
            audioPlayer.Play();
            return;
        }

        var overlay = new Control
        {
            Name = "ManosabaVideoOverlay",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            ZIndex = 1000
        };
        overlay.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        var videoPlayer = new VideoStreamPlayer
        {
            Name = "VideoStreamPlayer",
            Stream = stream,
            Autoplay = false,
            Loop = false,
            Expand = true,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        videoPlayer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

        if (chromaKeyGreen)
        {
            Shader? shader = ResourceLoader.Load<Shader>("res://Manosaba/shaders/chroma_key_green.gdshader");
            if (shader != null)
            {
                videoPlayer.Material = new ShaderMaterial { Shader = shader };
                Console.WriteLine("[ManosabaVideoCmd] Applied chroma-key shader");
            }
            else
            {
                Console.WriteLine("[ManosabaVideoCmd] Warning: chroma-key shader missing at res://Manosaba/shaders/chroma_key_green.gdshader");
            }
        }

        overlay.AddChild(videoPlayer);
        game.AddChild(overlay);
        Console.WriteLine("[ManosabaVideoCmd] Overlay added to NGame; starting playback");

        videoPlayer.Finished += () =>
        {
            Console.WriteLine("[ManosabaVideoCmd] Video finished; freeing overlay");
            if (GodotObject.IsInstanceValid(overlay))
            {
                overlay.QueueFree();
            }
        };

        videoPlayer.Play();
    }
}
