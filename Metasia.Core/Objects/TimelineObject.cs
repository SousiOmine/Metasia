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

namespace Metasia.Core.Objects
{
	/// <summary>
	/// タイムライン専用のオブジェクト
	/// </summary>
	public class TimelineObject : MetasiaObject, IRenderable, IMetaAudiable
	{
		/// <summary>
		/// タイムラインに属するレイヤー 格納順に描画される
		/// </summary>
		[JsonInclude]
		public List<LayerObject> Layers { get; private set; }

		public double Volume { get; set; } = 100;

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
		

		public void AudioExpresser(ref AudioExpresserArgs e, int frame)
		{
			//AudioExpresserArgsのMetasiaSoundのインスタンスがなかったら生成
			if (e.Sound is null) e.Sound = new MetasiaSound(e.AudioChannel, e.SoundSampleRate, (ushort)e.FPS);

			foreach (var layer in Layers)
			{
                if (!layer.IsActive) continue;
                AudioExpresserArgs express = new()
                {
                    AudioChannel = e.AudioChannel,
                    SoundSampleRate = e.SoundSampleRate,
                    FPS = e.FPS
                };
				layer.AudioExpresser(ref express, frame);

				if(express.Sound is null) continue;

                if (layer.Volume != 100)
                {
                    express.Sound = MetasiaSound.VolumeChange(express.Sound, layer.Volume / 100);
                }

                e.Sound = MetasiaSound.SynthesisPulse(e.AudioChannel, e.Sound, express.Sound);

				express.Dispose();
            }
			
		}
	}
}
