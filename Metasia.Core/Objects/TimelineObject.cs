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

        public AudioChunk GetAudioChunk(AudioFormat format, long startSample, long length)
        {
			//ProjectInfoにフレームレートを含むようになったらそれを使う
            double framerate = 60;

			long requestStartSample = startSample;
            long requestEndSample = startSample + length;

            var resultChunk = new AudioChunk(format, length);

			foreach (var obj in Layers)
			{
				if (!obj.IsActive) continue;
				long objStartSample = (long)(obj.StartFrame * (format.SampleRate / framerate));
                long objEndSample = (long)(obj.EndFrame * (format.SampleRate / framerate));

                long overlapStartSample = Math.Max(requestStartSample, objStartSample);
                long overlapEndSample = Math.Min(requestEndSample, objEndSample);

                if (overlapStartSample >= overlapEndSample)
                {
                    continue;
                }

				long layerStartPosition = overlapStartSample - objStartSample;
                long overlapLength = overlapEndSample - overlapStartSample;
                var chunk = obj.GetAudioChunk(format, layerStartPosition, overlapLength);
                for (int i = 0; i < overlapLength; i++)
                {
                    for (int ch = 0; ch < format.ChannelCount; ch++)
                    {
                        long sourceIndex = i * format.ChannelCount + ch;
                        long resultIndex = (overlapStartSample - requestStartSample + i) * format.ChannelCount + ch;
                        resultChunk.Samples[resultIndex] += chunk.Samples[sourceIndex];
                    }
                }
			}

			AudioEffectContext effectContext = new AudioEffectContext(this, format, startSample);

			foreach (var effect in AudioEffects)
			{
				resultChunk = effect.Apply(resultChunk, effectContext);
			}

			return resultChunk;
        }
    }
}
