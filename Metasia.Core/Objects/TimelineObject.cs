using Metasia.Core.Render;
using Metasia.Core.Sounds;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using SkiaSharp;

namespace Metasia.Core.Objects
{
    [Serializable]
    /// <summary>
    /// タイムライン専用のオブジェクト
    /// </summary>
    public class TimelineObject : IMetasiaObject, IRenderable, IAudible
    {
        /// <summary>
        /// タイムラインの最大長
        /// </summary>
        public static readonly int MAX_LENGTH = int.MaxValue;

        /// <summary>
        /// オブジェクト固有のID
        /// </summary>
        public string Id { get; set; } = string.Empty;

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
        public int SelectionEnd { get; set; } = MAX_LENGTH;

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

        public async Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
        {
            return await RenderExecuteAsync(context, cancellationToken);
        }

        public async Task<IAudioChunk> GetAudioChunkAsync(GetAudioContext context)
        {
            double framerate = context.ProjectFrameRate;

            // 要求範囲の開始フレームを計算
            int startFrame = (int)(context.StartSamplePosition * framerate / context.Format.SampleRate);
            var activeGroupControls = GetActiveGroupControlsAtFrame(startFrame);

            IAudioChunk resultChunk = new AudioChunk(context.Format, context.RequiredLength);

            for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
            {
                var layer = Layers[layerIndex];
                if (!layer.IsActive) continue;

                // このレイヤーに適用されるグループ制御を取得
                var applicableControls = activeGroupControls
                    .Where(c => IsControlApplicableToLayer(c.control, c.layerIndex, layerIndex))
                    .ToList();

                var chunk = await layer.GetAudioChunkAsync(context);

                // グループ制御の音量ゲインを計算
                double groupGain = 1.0;
                foreach (var (control, _) in applicableControls)
                {
                    groupGain *= control.Volume.Value / 100;
                }

                // グループ制御のエフェクトを適用
                IAudioChunk processedChunk = chunk;
                if (applicableControls.Count > 0)
                {
                    // ゲインを適用
                    if (groupGain != 1.0)
                    {
                        for (long i = 0; i < context.RequiredLength; i++)
                        {
                            for (int ch = 0; ch < context.Format.ChannelCount; ch++)
                            {
                                long index = i * context.Format.ChannelCount + ch;
                                processedChunk.Samples[index] *= groupGain;
                            }
                        }
                    }

                    // 各グループ制御のエフェクトを順次適用
                    foreach (var (control, _) in applicableControls)
                    {
                        if (control.AudioEffects.Count > 0)
                        {
                            // グループ制御の長さを計算（制御オブジェクトの期間）
                            double controlDuration = (control.EndFrame - control.StartFrame) / framerate;
                            var controlContext = new GetAudioContext(context.Format, context.StartSamplePosition, context.RequiredLength, context.ProjectFrameRate, controlDuration, context.AudioFileAccessor, context.ProjectPath);
                            AudioEffectContext effectContext = new AudioEffectContext(control, controlContext);

                            foreach (var effect in control.AudioEffects)
                            {
                                processedChunk = effect.Apply(processedChunk, effectContext);
                            }
                        }
                    }
                }

                // 処理後のチャンクを結果に加算
                for (long i = 0; i < context.RequiredLength; i++)
                {
                    for (int ch = 0; ch < context.Format.ChannelCount; ch++)
                    {
                        long sourceIndex = i * context.Format.ChannelCount + ch;
                        long resultIndex = i * context.Format.ChannelCount + ch;
                        resultChunk.Samples[resultIndex] += processedChunk.Samples[sourceIndex];
                        resultChunk.Samples[resultIndex] = Math.Max(-1.0, Math.Min(1.0, resultChunk.Samples[resultIndex]));
                    }
                }
            }

            // TimelineObject全体の長さを計算（実際のクリップの範囲）
            int lastFrame = GetLastFrameOfClips();
            double timelineDuration = Math.Max(1, lastFrame) / framerate;

            var timelineContext = new GetAudioContext(context.Format, context.StartSamplePosition, context.RequiredLength, context.ProjectFrameRate, timelineDuration, context.AudioFileAccessor, context.ProjectPath);
            AudioEffectContext timelineEffectContext = new AudioEffectContext(this, timelineContext);

            foreach (var effect in AudioEffects)
            {
                resultChunk = effect.Apply(resultChunk, timelineEffectContext);
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

        private class GroupControlLife(GroupControlRenderNode node)
        {
            public GroupControlRenderNode Node { get; init; } = node;
            public int Life { get; set; } = node.ScopeLayerTarget.ToScopeCount();
        }

        private class CameraControlLife(CameraControlRenderNode node)
        {
            public CameraControlRenderNode Node { get; init; } = node;
            public int Life { get; set; } = node.ScopeLayerTarget.ToScopeCount();
        }

        /// <summary>
        /// レイヤー範囲を指定してレンダリングを行う
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="layerStartIndex">開始レイヤーインデックス</param>
        /// <param name="layerEndIndex">終了レイヤーインデックス（-1の場合は最後のレイヤーまで）</param>
        /// <returns></returns>
        private async Task<NormalRenderNode> RenderExecuteAsync(
            RenderContext context,
            CancellationToken cancellationToken = default,
            int layerStartIndex = 0,
            int layerEndIndex = -1)
        {
            layerStartIndex = Math.Max(0, layerStartIndex);
            if (layerEndIndex < 0)
            {
                layerEndIndex = Layers.Count;
            }
            layerEndIndex = Math.Min(Layers.Count, layerEndIndex);

            cancellationToken.ThrowIfCancellationRequested();

            var nodes = new List<IRenderNode>();
            var targetLayers = Layers.GetRange(layerStartIndex, layerEndIndex - layerStartIndex);

            List<GroupControlLife> groupControlLifes = [];
            List<CameraControlLife> cameraControlLifes = [];

            for (int i = 0; i < targetLayers.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // グループ制御の寿命管理
                for (int j = groupControlLifes.Count - 1; j >= 0; j--)
                {
                    groupControlLifes[j].Life--;
                    if (groupControlLifes[j].Life < 0)
                    {
                        groupControlLifes.RemoveAt(j);
                    }
                }

                // カメラ制御の寿命管理
                for (int j = cameraControlLifes.Count - 1; j >= 0; j--)
                {
                    cameraControlLifes[j].Life--;
                    if (cameraControlLifes[j].Life < 0)
                    {
                        cameraControlLifes.RemoveAt(j);
                    }
                }

                var layer = targetLayers[i];
                if (!layer.IsActive || cameraControlLifes.Count > 0) continue;

                IRenderNode node = await layer.RenderAsync(context, cancellationToken);
                if (node is NormalRenderNode)
                {
                    foreach (var group in groupControlLifes)
                    {
                        node = await ApplyGroupControl(node, group.Node);
                    }
                    nodes.Add(node);
                }
                else if (node is GroupControlRenderNode)
                {
                    GroupControlRenderNode? groupNode = node as GroupControlRenderNode;
                    if (groupNode is not null)
                    {
                        foreach (var group in groupControlLifes)
                        {
                            node = await ApplyGroupControl(node, group.Node);
                        }
                        nodes.Add(node);
                        groupControlLifes.Add(new GroupControlLife(groupNode));
                    }
                }
                else if (node is CameraControlRenderNode)
                {
                    CameraControlRenderNode? cameraNode = node as CameraControlRenderNode;
                    if (cameraNode is not null)
                    {
                        cameraControlLifes.Add(new CameraControlLife(cameraNode));
                        int targetLayerCount = cameraNode.ScopeLayerTarget.ToScopeCount();
                        var targetLayersNode = await RenderExecuteAsync(context, cancellationToken, layerStartIndex + i + 1, layerStartIndex + i + targetLayerCount + 1);

                        var compositor = new Compositor();
                        var info = new SKImageInfo((int)context.RenderResolution.Width, (int)context.RenderResolution.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
                        using var surface = SKSurface.Create(info) ?? throw new InvalidOperationException($"Failed to create SKSurface with dimensions {info.Width}x{info.Height}");
                        var canvas = surface.Canvas;
                        await compositor.ProcessNodeAsync(canvas, targetLayersNode, context.ProjectResolution, context.RenderResolution, cancellationToken);
                        IRenderNode renderedNode = new NormalRenderNode()
                        {
                            Image = surface.Snapshot(),
                            LogicalSize = new SKSize(context.ProjectResolution.Width, context.ProjectResolution.Height),
                            Transform = cameraNode.Transform,
                        };

                        foreach (var group in groupControlLifes)
                        {
                            renderedNode = await ApplyGroupControl(renderedNode, group.Node);
                        }
                        nodes.Add(renderedNode);
                    }
                }


            }

            return new NormalRenderNode()
            {
                Children = nodes,
            };
        }

        private static Task<IRenderNode> ApplyGroupControl(IRenderNode node, GroupControlRenderNode groupNode)
        {
            node.Transform = node.Transform.Add(groupNode.Transform);
            foreach (var child in node.Children)
            {
                child.Transform = child.Transform.Add(groupNode.Transform);
            }
            return Task.FromResult(node);
        }

        /// <summary>
        /// 指定したフレームでアクティブなGroupControlObjectを収集する
        /// </summary>
        /// <param name="frame">フレーム位置</param>
        /// <returns>(GroupControlObject, 配置レイヤーインデックス)のリスト</returns>
        private List<(GroupControlObject control, int layerIndex)> GetActiveGroupControlsAtFrame(int frame)
        {
            var activeControls = new List<(GroupControlObject, int)>();

            for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
            {
                var layer = Layers[layerIndex];
                if (!layer.IsActive) continue;

                foreach (var obj in layer.Objects.OfType<GroupControlObject>())
                {
                    if (obj.IsActive && obj.IsExistFromFrame(frame))
                    {
                        activeControls.Add((obj, layerIndex));
                    }
                }
            }

            return activeControls;
        }

        /// <summary>
        /// グループ制御が指定したレイヤーに適用されるかどうかを判定する
        /// </summary>
        /// <param name="control">グループ制御オブジェクト</param>
        /// <param name="controlLayerIndex">制御が配置されたレイヤーインデックス</param>
        /// <param name="targetLayerIndex">判定対象のレイヤーインデックス</param>
        /// <returns>適用される場合はtrue</returns>
        private bool IsControlApplicableToLayer(GroupControlObject control, int controlLayerIndex, int targetLayerIndex)
        {
            // 制御は下位レイヤーにのみ適用される
            if (targetLayerIndex <= controlLayerIndex)
                return false;

            if (control.TargetLayers.IsInfinite)
                return true;

            // 対象レイヤー数が制限されている場合
            int layerDistance = targetLayerIndex - controlLayerIndex;
            return layerDistance <= control.TargetLayers.LayerCount;
        }
    }
}
