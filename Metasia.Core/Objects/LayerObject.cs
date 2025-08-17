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

namespace Metasia.Core.Objects
{
    public class LayerObject : MetasiaObject, IRenderable, IMetaAudiable
    {
        /// <summary>
        /// レイヤーに属するオブジェクト 原則同じフレームに2個以上オブジェクトがあってはならない
        /// </summary>
        [JsonInclude]
        public ObservableCollection<MetasiaObject> Objects { get; private set; }
        public double Volume { get; set; } = 100;

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

        public void AudioExpresser(ref AudioExpresserArgs e, int frame)
        {
            List<MetasiaObject> ApplicateObjects = new();
            foreach (var obj in Objects)
            {
                if (obj.IsExistFromFrame(frame) && obj is IMetaAudiable && obj.IsActive)
                {
                    ApplicateObjects.Add(obj);
                }
            }
            if (ApplicateObjects.Count == 0) return;

            if (e.Sound is null) e.Sound = new MetasiaSound(e.AudioChannel, e.SoundSampleRate, (ushort)e.FPS);

            foreach (var o in ApplicateObjects)
            {
                IMetaAudiable audiableObject = (IMetaAudiable)o;
                AudioExpresserArgs express = new()
                {
                    AudioChannel = e.AudioChannel,
                    SoundSampleRate = e.SoundSampleRate,
                    FPS = e.FPS
                };
                audiableObject.AudioExpresser(ref express, frame);

                if (express.Sound is not null)
                {

                    if (audiableObject.Volume != 100)
                    {
                        express.Sound = MetasiaSound.VolumeChange(express.Sound, audiableObject.Volume / 100);
                    }

                    e.Sound = MetasiaSound.SynthesisPulse(e.AudioChannel, e.Sound, express.Sound);
                }

                express.Dispose();
            }
        }

        /// <summary>
        /// 指定されたオブジェクトを指定された範囲で重複せず配置できるかどうかを判定する
        /// </summary>
        /// <param name="objectToCheck">配置を確認するオブジェクト</param>
        /// <param name="newStartFrame">新しい開始フレーム</param>
        /// <param name="newEndFrame">新しい終了フレーム</param>
        /// <returns>配置可能ならtrue, 不可能ならfalse</returns>

        public bool CanPlaceObjectAt(MetasiaObject objectToCheck, int newStartFrame, int newEndFrame)
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

        
    }
}
