using Metasia.Core.Render;
using Metasia.Core.Sounds;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Core.Objects
{
    [Serializable]
    /// <summary>
    /// タイムライン専用のオブジェクト
    /// </summary>
    public class TimelineObject : IMetasiaObject, IRenderable, IAudible
    {
        /// <summary>
        /// オブジェクト固有のID
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// オブジェクトを有効にするか
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 選択範囲の開始フレーム
        /// </summary>
        public int SelectionStart { get; set; } = 0;

        /// <summary>
        /// 選択範囲の終了フレーム
        /// </summary>
        public int SelectionEnd { get; set; } = int.MaxValue;

        /// <summary>
        /// タイムラインに属するレイヤー 格納順に描画される
        /// </summary>
        public List<LayerObject> Layers { get; private set; }

        [EditableProperty("AudioVolume")]
        [ValueRange(0, 99999, 0, 200)]
        public MetaDoubleParam Volume { get; set; } = new MetaDoubleParam(100);

        public List<AudioEffectBase> AudioEffects { get; set; } = new();

        public TimelineObject(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace", nameof(id));
            }
            Id = id;
            Layers = new();
        }

        public TimelineObject()
        {
            Layers = new();
        }

        public async Task<RenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nodes = new List<RenderNode>();

            foreach (var layer in Layers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!layer.IsActive) continue;
                nodes.Add(await layer.RenderAsync(context, cancellationToken));
            }

            return new RenderNode()
            {
                Children = nodes,
            };
        }

        public IAudioChunk GetAudioChunk(GetAudioContext context)
        {
            double framerate = context.ProjectFrameRate;

            IAudioChunk resultChunk = new AudioChunk(context.Format, context.RequiredLength);

            foreach (var layer in Layers)
            {
                if (!layer.IsActive) continue;

                var chunk = layer.GetAudioChunk(context);

                for (long i = 0; i < context.RequiredLength; i++)
                {
                    for (int ch = 0; ch < context.Format.ChannelCount; ch++)
                    {
                        long sourceIndex = i * context.Format.ChannelCount + ch;
                        long resultIndex = i * context.Format.ChannelCount + ch;
                        resultChunk.Samples[resultIndex] += chunk.Samples[sourceIndex];
                        resultChunk.Samples[resultIndex] = Math.Max(-1.0, Math.Min(1.0, resultChunk.Samples[resultIndex]));
                    }
                }
            }

            // TimelineObject全体の長さを計算（実際のクリップの範囲）
            int lastFrame = GetLastFrameOfClips();
            double timelineDuration = Math.Max(1, lastFrame) / framerate;

            var timelineContext = new GetAudioContext(context.Format, context.StartSamplePosition, context.RequiredLength, context.ProjectFrameRate, timelineDuration);
            AudioEffectContext effectContext = new AudioEffectContext(this, timelineContext);

            foreach (var effect in AudioEffects)
            {
                resultChunk = effect.Apply(resultChunk, effectContext);
            }

            return resultChunk;
        }

        /// <summary>
        /// 配下のすべてのクリップの中で最も後ろのフレーム位置を取得する
        /// </summary>
        /// <returns>最も後ろのクリップのEndFrame。クリップが存在しない場合は0を返す。</returns>
        public int GetLastFrameOfClips()
        {
            int lastFrame = 0;

            foreach (var layer in Layers)
            {
                foreach (var clip in layer.Objects)
                {
                    if (clip.EndFrame > lastFrame)
                    {
                        lastFrame = clip.EndFrame;
                    }
                }
            }

            return lastFrame;
        }
    }
}
