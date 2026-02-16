using Metasia.Core.Graphics;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Core.Objects
{
    [Serializable]
    public class LayerObject : IMetasiaObject, IRenderable, IAudible
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
        /// レイヤーに属するオブジェクト 原則同じフレームに2個以上オブジェクトがあってはならない
        /// </summary>
        [JsonInclude]
        public ObservableCollection<ClipObject> Objects { get; private set; }

        [EditableProperty("AudioVolume")]
        [ValueRange(0, 99999, 0, 200)]
        public MetaDoubleParam Volume { get; set; } = new MetaDoubleParam(100);

        public List<AudioEffectBase> AudioEffects { get; set; } = new();

        /// <summary>
        /// レイヤー名
        /// </summary>
        [EditableProperty("LayerName")]
        public string Name { get; set; }

        public LayerObject(string id, string LayerName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("ID cannot be null or whitespace", nameof(id));
            }
            Id = id;
            Name = LayerName;
            Objects = new();
        }

        public LayerObject()
        {
            Name = string.Empty;
            Objects = new();
        }

        public async Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            List<IRenderable> ApplicateObjects = new();
            foreach (var obj in Objects)
            {
                if (obj.IsExistFromFrame(context.Frame) && obj is IRenderable && obj.IsActive)
                {
                    ApplicateObjects.Add((IRenderable)obj);
                }
            }

            if (ApplicateObjects.Count == 0) return new NormalRenderNode();

            var nodes = new List<IRenderNode>();

            foreach (var obj in ApplicateObjects)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var node = await obj.RenderAsync(context, cancellationToken);
                if (node is NormalRenderNode)
                {
                    nodes.Add(node);
                }
                else if (node is GroupControlRenderNode)
                {
                    // GroupControlRenderNodeがフレームに含まれる場合はこれをそのまま返す
                    return node;
                }
                else if (node is CameraControlRenderNode)
                {
                    // CameraControlRenderNodeがフレームに含まれる場合はこれをそのまま返す
                    return node;
                }
            }

            return new NormalRenderNode()
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

        public async Task<IAudioChunk> GetAudioChunkAsync(GetAudioContext context)
        {
            double framerate = context.ProjectFrameRate;

            long requestStartSample = context.StartSamplePosition;
            long requestEndSample = context.StartSamplePosition + context.RequiredLength;

            IAudioChunk resultChunk = new AudioChunk(context.Format, context.RequiredLength);

            foreach (var obj in Objects.OfType<ClipObject>().OfType<IAudible>())
            {
                var clipObject = (ClipObject)obj;
                if (!clipObject.IsActive) continue;
                long objStartSample = (long)(clipObject.StartFrame * (context.Format.SampleRate / framerate));
                long objEndSample = (long)(clipObject.EndFrame * (context.Format.SampleRate / framerate));

                long overlapStartSample = Math.Max(requestStartSample, objStartSample);
                long overlapEndSample = Math.Min(requestEndSample, objEndSample);

                if (overlapStartSample >= overlapEndSample)
                {
                    continue;
                }

                long childStartPosition = overlapStartSample - objStartSample;
                long overlapLength = overlapEndSample - overlapStartSample;

                // 子オブジェクトの長さを計算
                double childDuration = (clipObject.EndFrame - clipObject.StartFrame) / framerate;
                var chunk = await obj.GetAudioChunkAsync(new GetAudioContext(context.Format, childStartPosition, overlapLength, context.ProjectFrameRate, childDuration, context.AudioFileAccessor, context.ProjectPath));
                double layerGain = obj.Volume?.Value / 100 ?? 1.0;
                for (int i = 0; i < overlapLength; i++)
                {
                    for (int ch = 0; ch < context.Format.ChannelCount; ch++)
                    {
                        long sourceIndex = i * context.Format.ChannelCount + ch;
                        long resultIndex = (overlapStartSample - requestStartSample + i) * context.Format.ChannelCount + ch;
                        resultChunk.Samples[resultIndex] += chunk.Samples[sourceIndex] * layerGain;
                        resultChunk.Samples[resultIndex] = Math.Max(-1.0, Math.Min(1.0, resultChunk.Samples[resultIndex]));
                    }
                }
            }

            // LayerObject全体の長さを計算（配下のクリップの範囲を考慮）
            int layerStartFrame = Objects.Count > 0 ? Objects.Min(o => o.StartFrame) : 0;
            int layerEndFrame = Objects.Count > 0 ? Objects.Max(o => o.EndFrame) : 0;
            double layerDuration = (layerEndFrame - layerStartFrame) / framerate;

            GetAudioContext layerContext = new(context.Format, context.StartSamplePosition, context.RequiredLength, context.ProjectFrameRate, layerDuration);
            AudioEffectContext effectContext = new(this, layerContext);

            foreach (var effect in AudioEffects)
            {
                resultChunk = effect.Apply(resultChunk, effectContext);
            }

            return resultChunk;
        }
    }
}
