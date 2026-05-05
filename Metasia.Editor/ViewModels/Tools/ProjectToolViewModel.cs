using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Models.Tools.ProjectTool;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Tools
{
    public class ProjectToolViewModel : ViewModelBase
    {
        private const int DefaultTimelineLayerCount = 100;

        public ObservableCollection<ProjectObjectTreeNode> Nodes
        {
            get => _nodes;
            private set => this.RaiseAndSetIfChanged(ref _nodes, value);
        }

        public bool IsProjectLoaded => _projectState.CurrentProject is not null;

        /// <summary>
        /// 選択中のノード
        /// </summary>
        public ProjectObjectTreeNode? SelectedNode
        {
            get => _selectedNode;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedNode, value);
                OnNodeSelected(value);
            }
        }

        public ICommand CreateTimelineCommand { get; }
        public ICommand DeleteTimelineCommand { get; }

        private ObservableCollection<ProjectObjectTreeNode> _nodes = new();
        private ProjectObjectTreeNode? _selectedNode;
        private readonly PlayerParentViewModel _playerParentViewModel;
        private readonly IProjectState _projectState;
        private readonly ISelectionState _selectionState;
        private readonly IEditCommandManager _editCommandManager;

        public ProjectToolViewModel(PlayerParentViewModel playerParentViewModel, IProjectState projectState, ISelectionState selectionState, IEditCommandManager editCommandManager)
        {
            _playerParentViewModel = playerParentViewModel;
            _projectState = projectState;
            _selectionState = selectionState;
            _editCommandManager = editCommandManager;
            CreateTimelineCommand = ReactiveCommand.Create(CreateTimeline);
            DeleteTimelineCommand = ReactiveCommand.Create(DeleteTimeline, CanDeleteTimeline());

            _projectState.ProjectLoaded += BuildObjectTree;
            _projectState.ProjectClosed += OnProjectClosed;
            _projectState.TimelineChanged += BuildObjectTree;

            BuildObjectTree();
        }

        private void BuildObjectTree()
        {
            Nodes = ProjectObjectTreeNode.BuildFromProject(_projectState.CurrentProject);
            this.RaisePropertyChanged(nameof(IsProjectLoaded));
        }

        private void OnProjectClosed()
        {
            SelectedNode = null;
            Nodes.Clear();
            this.RaisePropertyChanged(nameof(IsProjectLoaded));
        }

        private void OnNodeSelected(ProjectObjectTreeNode? node)
        {
            if (node is null) return;

            if (node.OwningTimeline is not null
                && _projectState.CurrentTimeline?.Id != node.OwningTimeline.Id)
            {
                _playerParentViewModel.SwitchToTimeline(node.OwningTimeline);
            }

            if (node.NodeType == ProjectObjectNodeType.Clip && node.SourceObject is ClipObject clip)
            {
                _selectionState.ClearSelectedClips();
                _selectionState.SelectClip(clip);
            }
        }

        private IObservable<bool> CanDeleteTimeline()
        {
            return this.WhenAnyValue(x => x.SelectedNode)
                .Select(node => CanDeleteNode(node));
        }

        private bool CanDeleteNode(ProjectObjectTreeNode? node)
        {
            if (node is null) return false;
            if (node.NodeType != ProjectObjectNodeType.Timeline) return false;
            if (node.SourceObject is not TimelineObject timeline) return false;
            if (_projectState.CurrentProject is null) return false;

            return timeline.Id != _projectState.CurrentProject.ProjectFile.RootTimelineId;
        }

        private void DeleteTimeline()
        {
            if (SelectedNode?.SourceObject is not TimelineObject timeline) return;
            if (_projectState.CurrentProject is null) return;

            var command = new TimelineRemoveCommand(_projectState.CurrentProject, timeline);
            _editCommandManager.Execute(command);

            _projectState.NotifyTimelineChanged();
            SelectedNode = null;
        }

        private void CreateTimeline()
        {
            if (_projectState.CurrentProject is null)
            {
                return;
            }

            var timeline = new TimelineObject(GenerateTimelineId());
            for (int i = 1; i <= DefaultTimelineLayerCount; i++)
            {
                timeline.Layers.Add(new LayerObject($"layer{i}", $"Layer {i}"));
            }

            var command = new TimelineAddCommand(_projectState.CurrentProject, timeline);
            _editCommandManager.Execute(command);

            _projectState.NotifyTimelineChanged();

            var createdNode = FindNodeByTimeline(Nodes, timeline);
            if (createdNode is not null)
            {
                SelectedNode = createdNode;
            }
        }

        private string GenerateTimelineId()
        {
            if (_projectState.CurrentProject is null)
            {
                return "Timeline1";
            }

            var existingIds = _projectState.CurrentProject.Timelines
                .Select(x => x.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            int index = 1;
            while (existingIds.Contains($"Timeline{index}"))
            {
                index++;
            }

            return $"Timeline{index}";
        }

        private static ProjectObjectTreeNode? FindNodeByTimeline(
            ObservableCollection<ProjectObjectTreeNode> nodes,
            TimelineObject timeline)
        {
            foreach (var node in nodes)
            {
                if (ReferenceEquals(node.SourceObject, timeline))
                {
                    return node;
                }

                if (node.SubNodes is null)
                {
                    continue;
                }

                var childResult = FindNodeByTimeline(node.SubNodes, timeline);
                if (childResult is not null)
                {
                    return childResult;
                }
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _projectState.ProjectLoaded -= BuildObjectTree;
                _projectState.ProjectClosed -= OnProjectClosed;
                _projectState.TimelineChanged -= BuildObjectTree;
            }

            base.Dispose(disposing);
        }
    }
}
