using Metasia.Core.Graphics;
using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Sounds;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Metasia.Core.Objects.AudioEffects;

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

        public RenderNode Render(RenderContext context)
		{
			var nodes = new List<RenderNode>();

            foreach (var layer in Layers)
			{
				if (!layer.IsActive) continue;
				nodes.Add(layer.Render(context));
			}

			return new RenderNode()
			{
				Children = nodes,
			};
		}

        public AudioChunk GetAudioChunk(GetAudioContext context)
        {
            double framerate = context.ProjectFrameRate;

			long requestStartSample = context.StartSamplePosition;
            long requestEndSample = context.StartSamplePosition + context.RequiredLength;

            var resultChunk = new AudioChunk(context.Format, context.RequiredLength);

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
    }
}
