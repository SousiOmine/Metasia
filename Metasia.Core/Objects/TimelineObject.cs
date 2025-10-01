using Metasia.Core.Render;
using Metasia.Core.Xml;
using Metasia.Core.Sounds;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Attributes;

namespace Metasia.Core.Objects
{
	[Serializable]
	/// <summary>
	/// タイムライン専用のオブジェクト
	/// </summary>
	public class TimelineObject : ClipObject, IRenderable, IAudible
	{
		/// <summary>
		/// タイムラインに属するレイヤー 格納順に描画される
		/// </summary>
		public List<LayerObject> Layers { get; private set; }

		[EditableProperty("AudioVolume")]
		public double Volume { get; set; } = 100;

		public List<AudioEffectBase> AudioEffects { get; set; } = new();

        public TimelineObject(string id) : base(id)
		{
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

			long requestStartSample = context.StartSamplePosition;
            long requestEndSample = context.StartSamplePosition + context.RequiredLength;

            IAudioChunk resultChunk = new AudioChunk(context.Format, context.RequiredLength);

			foreach (var obj in Layers)
			{
				if (!obj.IsActive) continue;
				double samplesPerFrame = context.Format.SampleRate / framerate;
				long objStartSample = (long)Math.Round(obj.StartFrame * samplesPerFrame);
                long objEndSample = (long)Math.Round(obj.EndFrame * samplesPerFrame);

                long overlapStartSample = Math.Max(requestStartSample, objStartSample);
                long overlapEndSample = Math.Min(requestEndSample, objEndSample);

                if (overlapStartSample >= overlapEndSample)
                {
                    continue;
                }

				long layerStartPosition = overlapStartSample - objStartSample;
                long overlapLength = overlapEndSample - overlapStartSample;
                
                // レイヤーオブジェクトの長さを計算
                double layerDuration = (obj.EndFrame - obj.StartFrame) / framerate;
                var chunk = obj.GetAudioChunk(new GetAudioContext(context.Format, layerStartPosition, overlapLength, context.ProjectFrameRate, layerDuration));
                for (int i = 0; i < overlapLength; i++)
                {
                    for (int ch = 0; ch < context.Format.ChannelCount; ch++)
                    {
                        long sourceIndex = i * context.Format.ChannelCount + ch;
                        long resultIndex = (overlapStartSample - requestStartSample + i) * context.Format.ChannelCount + ch;
                        resultChunk.Samples[resultIndex] += chunk.Samples[sourceIndex];
                        resultChunk.Samples[resultIndex] = Math.Max(-1.0, Math.Min(1.0, resultChunk.Samples[resultIndex]));
                    }
                }
			}

			// TimelineObject全体の長さを計算
			double timelineDuration = (EndFrame - StartFrame) / framerate;

			var timelineContext = new GetAudioContext(context.Format, context.StartSamplePosition, context.RequiredLength, context.ProjectFrameRate, timelineDuration);
			AudioEffectContext effectContext = new AudioEffectContext(this, timelineContext);

			foreach (var effect in AudioEffects)
			{
				resultChunk = effect.Apply(resultChunk, effectContext);
			}

			return resultChunk;
        }

        /// <summary>
        /// 指定したフレームでタイムラインオブジェクトを分割する
        /// </summary>
        /// <param name="splitFrame">分割フレーム</param>
        /// <returns>分割後の2つのタイムラインオブジェクト（前半と後半）</returns>
        public override (ClipObject firstClip, ClipObject secondClip) SplitAtFrame(int splitFrame)
        {
            var result = base.SplitAtFrame(splitFrame);

            var firstTimeline = (TimelineObject)result.firstClip;
            var secondTimeline = (TimelineObject)result.secondClip;

            firstTimeline.Id = Id + "_part1";
            secondTimeline.Id = Id + "_part2";

            // レイヤーを適切に分割
            firstTimeline.Layers = new List<LayerObject>();
            secondTimeline.Layers = new List<LayerObject>();

            foreach (var layer in Layers)
            {
                // レイヤーが完全に前半に属する場合
                if (layer.EndFrame < splitFrame)
                {
                    // 前半タイムラインの範囲に合わせてレイヤーの範囲を調整
                    var xml = MetasiaObjectXmlSerializer.Serialize(layer);
                    var adjustedLayer = MetasiaObjectXmlSerializer.Deserialize<LayerObject>(xml);
                    adjustedLayer.StartFrame = Math.Max(layer.StartFrame, firstTimeline.StartFrame);
                    adjustedLayer.EndFrame = Math.Min(layer.EndFrame, firstTimeline.EndFrame);
                    firstTimeline.Layers.Add(adjustedLayer);
                }
                // レイヤーが完全に後半に属する場合
                else if (layer.StartFrame >= splitFrame)
                {
                    // 後半タイムラインの範囲に合わせてレイヤーの範囲を調整
                    var xml = MetasiaObjectXmlSerializer.Serialize(layer);
                    var adjustedLayer = MetasiaObjectXmlSerializer.Deserialize<LayerObject>(xml);
                    adjustedLayer.StartFrame = Math.Max(layer.StartFrame, secondTimeline.StartFrame);
                    adjustedLayer.EndFrame = Math.Min(layer.EndFrame, secondTimeline.EndFrame);
                    secondTimeline.Layers.Add(adjustedLayer);
                }
                // レイヤーが分割フレームをまたぐ場合、レイヤーを分割
                else
                {
                    var splitResult = layer.SplitAtFrame(splitFrame);
                    firstTimeline.Layers.Add((LayerObject)splitResult.firstClip);
                    secondTimeline.Layers.Add((LayerObject)splitResult.secondClip);
                }
            }
            return (firstTimeline, secondTimeline);
        }

        /// <summary>
        /// タイムラインオブジェクトのコピーを作成する
        /// </summary>
        /// <returns>コピーされたタイムラインオブジェクト</returns>
        protected override ClipObject CreateCopy()
        {
            var xml = MetasiaObjectXmlSerializer.Serialize(this);
            var copy = MetasiaObjectXmlSerializer.Deserialize<TimelineObject>(xml);
            copy.Id = Id + "_copy";
            return copy;
        }
    }
}
