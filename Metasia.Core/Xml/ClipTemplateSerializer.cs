using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Templates;

namespace Metasia.Core.Xml
{
    public static class ClipTemplateSerializer
    {
        private static readonly XmlSerializer _serializer = new(typeof(ClipTemplate));

        public static string Serialize(ClipTemplate template)
        {
            using var writer = new StringWriter();
            _serializer.Serialize(writer, template);
            return writer.ToString();
        }

        public static ClipTemplate Deserialize(string xml)
        {
            using var reader = new StringReader(xml);
            var result = _serializer.Deserialize(reader) as ClipTemplate;
            return result ?? throw new InvalidOperationException("Failed to deserialize ClipTemplate");
        }

        public static void SaveToFile(ClipTemplate template, string filePath)
        {
            var xml = Serialize(template);
            File.WriteAllText(filePath, xml);
        }

        public static ClipTemplate LoadFromFile(string filePath)
        {
            var xml = File.ReadAllText(filePath);
            return Deserialize(xml);
        }

        public static ClipTemplate CreateFromClips(IEnumerable<ClipObject> clips, TimelineObject timeline)
        {
            var clipList = clips.ToList();
            if (clipList.Count == 0)
            {
                throw new ArgumentException("No clips provided", nameof(clips));
            }

            var (baseClip, baseLayer) = FindBaseClipAndLayer(clipList, timeline);
            if (baseClip == null || baseLayer == null)
            {
                throw new InvalidOperationException("No base clip/layer found for provided clips/timeline");
            }
            int baseFrame = baseClip.StartFrame;

            var entries = new List<ClipTemplateEntry>();
            foreach (var clip in clipList)
            {
                var layer = FindLayerOfClip(clip, timeline);
                if (layer == null)
                {
                    Debug.WriteLine($"Skipping clip {clip.Id} - layer not found in timeline {timeline.Id}");
                    continue;
                }

                int layerIndex = timeline.Layers.IndexOf(layer);
                int frameOffset = clip.StartFrame - baseFrame;

                string clipXml = MetasiaObjectXmlSerializer.Serialize(clip);
                string clipTypeName = clip.GetType().AssemblyQualifiedName ?? clip.GetType().FullName ?? clip.GetType().Name;

                entries.Add(new ClipTemplateEntry(layerIndex, frameOffset, clipXml, clipTypeName));
            }

            return new ClipTemplate(entries);
        }

        public static List<(ClipObject clip, int layerIndex)> InstantiateClips(ClipTemplate template, int targetBaseFrame, int baseLayerIndex, TimelineObject timeline)
        {
            var result = new List<(ClipObject clip, int layerIndex)>();
            if (!template.ClipEntries.Any())
            {
                return result;
            }
            int minLayerIndex = template.ClipEntries.Min(e => e.LayerIndex);

            foreach (var entry in template.ClipEntries)
            {
                Type? clipType = Type.GetType(entry.ClipTypeName);
                if (clipType == null)
                {
                    clipType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a =>
                        {
                            try { return a.GetTypes(); }
                            catch { return Array.Empty<Type>(); }
                        })
                        .FirstOrDefault(t => t.Name == entry.ClipTypeName || t.FullName == entry.ClipTypeName);
                }

                if (clipType == null)
                {
                    throw new InvalidOperationException($"Could not find clip type: {entry.ClipTypeName}");
                }

                var clip = MetasiaObjectXmlSerializer.Deserialize(clipType, entry.ClipXml) as ClipObject;

                if (clip == null)
                {
                    throw new InvalidOperationException($"Failed to deserialize clip of type: {entry.ClipTypeName}");
                }

                clip.Id = Guid.NewGuid().ToString();
                AssignNewIdsToEffects(clip);

                int clipDuration = clip.EndFrame - clip.StartFrame;
                clip.StartFrame = targetBaseFrame + entry.FrameOffset;
                clip.EndFrame = clip.StartFrame + clipDuration;

                int targetLayerIndex = baseLayerIndex + (entry.LayerIndex - minLayerIndex);
                result.Add((clip, targetLayerIndex));
            }

            return result;
        }

        private static void AssignNewIdsToEffects(ClipObject clip)
        {
            if (clip is IRenderable renderable && renderable.VisualEffects != null)
            {
                foreach (var effect in renderable.VisualEffects)
                {
                    effect.Id = Guid.NewGuid().ToString();
                }
            }
            if (clip is IAudible audible && audible.AudioEffects != null)
            {
                foreach (var effect in audible.AudioEffects)
                {
                    effect.Id = Guid.NewGuid().ToString();
                }
            }
        }

        private static (ClipObject clip, LayerObject layer) FindBaseClipAndLayer(List<ClipObject> clips, TimelineObject timeline)
        {
            ClipObject? baseClip = null;
            LayerObject? baseLayer = null;
            int minFrame = int.MaxValue;
            int minLayerIndex = int.MaxValue;

            foreach (var clip in clips)
            {
                var layer = FindLayerOfClip(clip, timeline);
                if (layer == null) continue;

                int layerIndex = timeline.Layers.IndexOf(layer);

                bool shouldUpdate = false;
                if (clip.StartFrame < minFrame)
                {
                    shouldUpdate = true;
                }
                else if (clip.StartFrame == minFrame && layerIndex < minLayerIndex)
                {
                    shouldUpdate = true;
                }

                if (shouldUpdate)
                {
                    minFrame = clip.StartFrame;
                    minLayerIndex = layerIndex;
                    baseClip = clip;
                    baseLayer = layer;
                }
            }

            return (baseClip!, baseLayer!);
        }

        private static LayerObject? FindLayerOfClip(ClipObject clip, TimelineObject timeline)
        {
            foreach (var layer in timeline.Layers)
            {
                if (layer.Objects.Any(c => c.Id == clip.Id))
                {
                    return layer;
                }
            }
            return null;
        }
    }
}