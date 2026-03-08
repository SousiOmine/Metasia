using System.Collections.ObjectModel;
using Metasia.Core.Objects;
using Metasia.Editor.Models;

namespace Metasia.Editor.Models.Tools.ProjectTool
{
    public enum ProjectObjectNodeType
    {
        Timeline,
        Layer,
        Clip,
        VisualEffect,
        AudioEffect
    }

    public class ProjectObjectTreeNode
    {
        public string Title { get; }

        public ProjectObjectNodeType NodeType { get; }

        public object? SourceObject { get; }

        public ObservableCollection<ProjectObjectTreeNode>? SubNodes { get; }

        public ProjectObjectTreeNode(string title, ProjectObjectNodeType nodeType, object? sourceObject = null, ObservableCollection<ProjectObjectTreeNode>? subNodes = null)
        {
            Title = title;
            NodeType = nodeType;
            SourceObject = sourceObject;
            SubNodes = subNodes;
        }

        public static ObservableCollection<ProjectObjectTreeNode> BuildFromProject(
            Projects.MetasiaEditorProject? project)
        {
            var roots = new ObservableCollection<ProjectObjectTreeNode>();
            if (project is null) return roots;

            foreach (var timeline in project.Timelines)
            {
                roots.Add(BuildTimelineNode(timeline));
            }

            return roots;
        }

        private static ProjectObjectTreeNode BuildTimelineNode(TimelineObject timeline)
        {
            var layerNodes = new ObservableCollection<ProjectObjectTreeNode>();

            foreach (var layer in timeline.Layers)
            {
                layerNodes.Add(BuildLayerNode(layer));
            }

            string title = $"Timeline: {timeline.Id}";
            return new ProjectObjectTreeNode(title, ProjectObjectNodeType.Timeline, timeline, layerNodes);
        }

        private static ProjectObjectTreeNode BuildLayerNode(LayerObject layer)
        {
            var childNodes = new ObservableCollection<ProjectObjectTreeNode>();

            foreach (var clip in layer.Objects)
            {
                childNodes.Add(BuildClipNode(clip));
            }

            string layerName = string.IsNullOrEmpty(layer.Name) ? layer.Id : layer.Name;
            string title = $"Layer: {layerName}";
            return new ProjectObjectTreeNode(title, ProjectObjectNodeType.Layer, layer, childNodes);
        }

        private static ProjectObjectTreeNode BuildClipNode(ClipObject clip)
        {
            var effectNodes = new ObservableCollection<ProjectObjectTreeNode>();

            // VisualEffects
            if (clip is IRenderable renderable)
            {
                foreach (var effect in renderable.VisualEffects)
                {
                    string effectName = DisplayTextResolver.ResolveVisualEffectDisplayName(effect.GetType());
                    effectNodes.Add(new ProjectObjectTreeNode(
                        $"[Visual] {effectName}",
                        ProjectObjectNodeType.VisualEffect,
                        effect));
                }
            }

            // AudioEffects
            if (clip is IAudible audible)
            {
                foreach (var effect in audible.AudioEffects)
                {
                    string effectName = DisplayTextResolver.ResolveAudioEffectDisplayName(effect.GetType());
                    effectNodes.Add(new ProjectObjectTreeNode(
                        $"[Audio] {effectName}",
                        ProjectObjectNodeType.AudioEffect,
                        effect));
                }
            }

            string clipTypeName = DisplayTextResolver.ResolveClipDisplayName(clip.GetType());
            string title = $"{clipTypeName} ({clip.Id})";
            var subNodes = effectNodes.Count > 0 ? effectNodes : null;
            return new ProjectObjectTreeNode(title, ProjectObjectNodeType.Clip, clip, subNodes);
        }
    }
}
