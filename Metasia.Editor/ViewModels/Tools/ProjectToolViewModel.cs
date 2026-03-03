using System;
using System.Collections.ObjectModel;
using Metasia.Core.Objects;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.Tools.ProjectTool;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Tools
{
    public class ProjectToolViewModel : ViewModelBase
    {
        public ObservableCollection<ProjectObjectTreeNode> Nodes
        {
            get => _nodes;
            private set => this.RaiseAndSetIfChanged(ref _nodes, value);
        }

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

        private ObservableCollection<ProjectObjectTreeNode> _nodes = new();
        private ProjectObjectTreeNode? _selectedNode;
        private readonly IProjectState _projectState;
        private readonly ISelectionState _selectionState;

        public ProjectToolViewModel(PlayerParentViewModel playerParentViewModel, IProjectState projectState, ISelectionState selectionState)
        {
            _projectState = projectState;
            _selectionState = selectionState;

            _projectState.ProjectLoaded += BuildObjectTree;
            _projectState.ProjectClosed += OnProjectClosed;
            _projectState.TimelineChanged += BuildObjectTree;

            BuildObjectTree();
        }

        private void BuildObjectTree()
        {
            Nodes = ProjectObjectTreeNode.BuildFromProject(_projectState.CurrentProject);
        }

        private void OnProjectClosed()
        {
            Nodes.Clear();
        }

        private void OnNodeSelected(ProjectObjectTreeNode? node)
        {
            if (node is null) return;

            if (node.NodeType == ProjectObjectNodeType.Clip && node.SourceObject is ClipObject clip)
            {
                _selectionState.ClearSelectedClips();
                _selectionState.SelectClip(clip);
            }
        }
    }
}