using Metasia.Core.Graphics;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Metasia.Core.Objects.AudioEffects;

namespace Metasia.Core.Objects
{
    [Serializable]
    public class LayerObject : ClipObject, IRenderable, IAudible
    {
        /// <summary>
        /// レイヤーに属するオブジェクト 原則同じフレームに2個以上オブジェクトがあってはならない
        /// </summary>
        [JsonInclude]
        public ObservableCollection<ClipObject> Objects { get; private set; }
        public double Volume { get; set; } = 100;

        public List<AudioEffectBase> AudioEffects { get; set; } = new();

        /// <summary>
        /// レイヤー名
        /// </summary>
        public string Name { get; set; }

        public LayerObject(string id, string LayerName) : base(id)
        {
            Name = LayerName;
            StartFrame = 0;
            EndFrame = int.MaxValue;
            Objects = new();
        }

        public LayerObject()
        {
            Name = string.Empty;
            StartFrame = 0;
            EndFrame = int.MaxValue;
            Objects = new();
        }

        public RenderNode Render(RenderContext context)
        {
            List<IRenderable> ApplicateObjects = new();
            foreach (var obj in Objects)
            {
                if (obj.IsExistFromFrame(context.Frame) && obj is IRenderable && obj.IsActive)
                {
                    ApplicateObjects.Add((IRenderable)obj);
                }
            }

            if (ApplicateObjects.Count == 0) return new RenderNode();

            var nodes = new List<RenderNode>();

            foreach (var obj in ApplicateObjects)
            {
                nodes.Add(obj.Render(context));
            }

            return new RenderNode()
            {
                Children = nodes,
            };
        }

        /// <summary>
        /// 指定されたオブジェクトを指定された範囲で重複せず配置できるかどうかを判定する
        /// </summary>
        /// <param name="objectToCheck">配置を確認するオブジェクト</param>
        /// <param name="newStartFrame">新しい開始フレーム</param>
        /// <param name="newEndFrame">新しい終了フレーム</param>
        /// <returns>配置可能ならtrue, 不可能ならfalse</returns>

        public bool CanPlaceObjectAt(ClipObject objectToCheck, int newStartFrame, int newEndFrame)
        {
            //新しい範囲がそもそも無効なら弾く
            if (newStartFrame > newEndFrame) return false;

            //範囲内に重複があるかどうか
            foreach (var existingObject in Objects)
            {
                //同じオブジェクトは除外
                if (existingObject.Id == objectToCheck.Id) continue;

                if (newStartFrame <= existingObject.EndFrame && newEndFrame >= existingObject.StartFrame)
                {
                    return false;
                }
            }

            return true;
        }

        public AudioChunk GetAudioChunk(AudioFormat format, long startSample, long length)
        {
            //ProjectInfoにフレームレートを含むようになったらそれを使う
            double framerate = 60;

            long requestStartSample = startSample;
            long requestEndSample = startSample + length;

            var resultChunk = new AudioChunk(format, length);

            foreach (var obj in Objects.OfType<ClipObject>().OfType<IAudible>())
            {
                var metasiaObject = (ClipObject)obj;
                if (!metasiaObject.IsActive) continue;
                long objStartSample = (long)(metasiaObject.StartFrame * (format.SampleRate / framerate));
                long objEndSample = (long)(metasiaObject.EndFrame * (format.SampleRate / framerate));

                long overlapStartSample = Math.Max(requestStartSample, objStartSample);
                long overlapEndSample = Math.Min(requestEndSample, objEndSample);

                if (overlapStartSample >= overlapEndSample)
                {
                    continue;
                }

                long childStartPosition = overlapStartSample - objStartSample;
                long overlapLength = overlapEndSample - overlapStartSample;
                var chunk = obj.GetAudioChunk(format, childStartPosition, overlapLength);
                double layerGain = obj.Volume / 100;
                for (int i = 0; i < overlapLength; i++)
                {
                    for (int ch = 0; ch < format.ChannelCount; ch++)
                    {
                        long sourceIndex = i * format.ChannelCount + ch;
                        long resultIndex = (overlapStartSample - requestStartSample + i) * format.ChannelCount + ch;
                        resultChunk.Samples[resultIndex] += chunk.Samples[sourceIndex] * layerGain;
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
