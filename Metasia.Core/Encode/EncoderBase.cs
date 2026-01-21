
using System.Runtime.CompilerServices;
using Metasia.Core.Graphics;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using SkiaSharp;

namespace Metasia.Core.Encode;

/// <summary>
/// 動画エンコーダーの基底クラス 実装でタイムラインや選択範囲を気にしなくて済むようにする
/// </summary>
public abstract class EncoderBase : IEncoder, IDisposable
{
    public abstract double ProgressRate { get; }

    public virtual event EventHandler<EventArgs> StatusChanged = delegate { };
    public virtual event EventHandler<EventArgs> EncodeStarted = delegate { };
    public virtual event EventHandler<EventArgs> EncodeCompleted = delegate { };
    public virtual event EventHandler<EventArgs> EncodeFailed = delegate { };

    private MetasiaProject? _project;
    private TimelineObject? _targetTimeline;
    private IImageFileAccessor? _imageFileAccessor;
    private IVideoFileAccessor? _videoFileAccessor;
    private string? _projectPath;

    private int _startFrame;
    private int _endFrame;

    public virtual void Initialize(
        MetasiaProject project, 
        TimelineObject targetTimeline, 
        IImageFileAccessor imageFileAccessor, 
        IVideoFileAccessor videoFileAccessor, 
        string projectPath)
    {
        _project = project;
        _targetTimeline = targetTimeline;
        _imageFileAccessor = imageFileAccessor;
        _videoFileAccessor = videoFileAccessor;
        _projectPath = projectPath;

        _startFrame = _targetTimeline.SelectionStart;
        _endFrame = _targetTimeline.SelectionEnd;
    }

    public abstract void CancelRequest();

    public abstract void Start();

    protected async IAsyncEnumerable<SKBitmap> GetFramesAsync(int firstFrameIndex, int lastFrameIndex, [EnumeratorCancellation] CancellationToken ct)
    {
        if (_project is null || _targetTimeline is null || _imageFileAccessor is null || _videoFileAccessor is null || _projectPath is null)
        {
            throw new InvalidOperationException("プロジェクトまたはタイムラインが初期化されていません。");
        }
        if (firstFrameIndex > lastFrameIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(firstFrameIndex), "最初のフレームインデックスは最後のフレームインデックスより小さい必要があります。");
        }
        if (firstFrameIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(firstFrameIndex), "フレームインデックスは0以上である必要があります。");
        }
        if (lastFrameIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lastFrameIndex), "フレームインデックスは0以上である必要があります。");
        }

        var compositor = new Compositor();
        var projectResolution = _project.Info.Size;

        for (int frame = firstFrameIndex; frame <= lastFrameIndex; frame++)
        {
            ct.ThrowIfCancellationRequested();

            var skBitmap = await compositor.RenderFrameAsync(
                _targetTimeline,
                frame,
                projectResolution,
                projectResolution,
                _imageFileAccessor,
                _videoFileAccessor,
                _project.Info,
                _projectPath,
                ct);

            yield return skBitmap;
        }
    }

    protected async Task<IAudioChunk> GetAudioChunkAsync(long startSample, long sampleCount, int sampleRate, int channelCount, CancellationToken ct)
    {
        // TODO: GetAudioChunkが非同期処理に対応したらこのメソッドも非同期化する
        if (_project is null || _targetTimeline is null)
        {
            throw new InvalidOperationException("プロジェクトまたはタイムラインが初期化されていません。");
        }
        long startPosition = (long)((double)sampleRate / _project.Info.Framerate * _startFrame) + startSample;
        double lengthInSecond = (_endFrame - _startFrame) / _project.Info.Framerate;
        var chunk = _targetTimeline.GetAudioChunk(new GetAudioContext(new AudioFormat(sampleRate, channelCount), startPosition, sampleCount, _project.Info.Framerate, lengthInSecond));
        return chunk;
    }

    public void Dispose()
    {
        _project = null;
        _targetTimeline = null;
        _imageFileAccessor = null;
        _videoFileAccessor = null;
    }
}