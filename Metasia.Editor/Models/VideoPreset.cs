using System.Collections.ObjectModel;
using System.Linq;

namespace Metasia.Editor.Models;

public class VideoPreset
{
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }
    public int FrameRate { get; }
    public int AudioSamplingRate { get; }
    public int AudioChannels { get; }
    public bool IsCustom { get; }

    private VideoPreset(string name, int width, int height, int frameRate, int audioSamplingRate, int audioChannels, bool isCustom = false)
    {
        Name = name;
        Width = width;
        Height = height;
        FrameRate = frameRate;
        AudioSamplingRate = audioSamplingRate;
        AudioChannels = audioChannels;
        IsCustom = isCustom;
    }

    private VideoPreset(string name, int width, int height, int frameRate)
        : this(name, width, height, frameRate, 44100, 2)
    {
    }

    public static VideoPreset Custom { get; } = new("カスタム", 1920, 1080, 30, 44100, 2, isCustom: true);

    public static ObservableCollection<VideoPreset> DefaultPresets { get; } =
    [
        new("HD (1280×720) 24fps", 1280, 720, 24),
        new("HD (1280×720) 30fps", 1280, 720, 30),
        new("HD (1280×720) 60fps", 1280, 720, 60),
        new("Full HD (1920×1080) 24fps", 1920, 1080, 24),
        new("Full HD (1920×1080) 30fps", 1920, 1080, 30),
        new("Full HD (1920×1080) 60fps", 1920, 1080, 60),
        new("4K (3840×2160) 24fps", 3840, 2160, 24),
        new("4K (3840×2160) 30fps", 3840, 2160, 30),
        new("4K (3840×2160) 60fps", 3840, 2160, 60),
        Custom,
    ];

    public static VideoPreset? FindMatch(int width, int height, int frameRate)
    {
        return DefaultPresets.FirstOrDefault(p =>
            !p.IsCustom &&
            p.Width == width &&
            p.Height == height &&
            p.FrameRate == frameRate);
    }
}
