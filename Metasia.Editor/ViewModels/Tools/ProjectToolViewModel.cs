using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Windows.Input;
using DynamicData;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Tools.ProjectTool;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Tools
{
    public class ProjectToolViewModel : ViewModelBase
    {
        public string ProjectDir_Path { get; private set; } = String.Empty;

        public ObservableCollection<FileTreeNode> Nodes { get; private set; } = new ObservableCollection<FileTreeNode>();

        /// <summary>
        /// 選択中のノード（複数選択可能）
        /// </summary>
        public ObservableCollection<FileTreeNode> SelectedNodes { get; } = new ObservableCollection<FileTreeNode>();



        public bool IsFileSelecting
        {
            get => _isFileSelected;
            set => this.RaiseAndSetIfChanged(ref _isFileSelected, value);
        }

        public ICommand OpenFileByExternalApp { get; }

        private bool _isFileSelected;

        private PlayerParentViewModel _playerParentViewModel;

        public ProjectToolViewModel(PlayerParentViewModel playerParentViewModel)
        {
            _playerParentViewModel = playerParentViewModel;
            ProjectDir_Path = playerParentViewModel.CurrentEditorProject?.ProjectPath.Path ?? String.Empty;

            _playerParentViewModel.ProjectInstanceChanged += (sender, args) =>
            {
                ProjectDir_Path = playerParentViewModel.CurrentEditorProject?.ProjectPath.Path ?? String.Empty;
                LoadDirectory();
            };

            //プロジェクトディレクトリなしで作成された時はファイルを開くとかの案内を表示したい
            if (String.IsNullOrEmpty(ProjectDir_Path))
            {

            }

            OpenFileByExternalApp = ReactiveCommand.Create(() =>
            {
                //ファイルやディレクトリを外部アプリで開く処理を書く
            });

            //選択中のノードが変更された時にコンテキストメニューの表示を変更する
            SelectedNodes.CollectionChanged += (sender, args) =>
            {
                if (SelectedNodes.Count <= 0)
                {
                    IsFileSelecting = false;
                    return;
                }
                if (SelectedNodes[0].ResourceEntity is FileEntity)
                {
                    IsFileSelecting = true;
                }
                else
                {
                    IsFileSelecting = false;
                }
            };

            if (String.IsNullOrEmpty(ProjectDir_Path))
            {
                var kari = new DirectoryEntity("./../");
                foreach (var entity in kari.GetSubordinates())
                {
                    Nodes.Add(new FileTreeNode(entity));
                }
            }
            else
            {
                LoadDirectory();
            }
        }

        public void LoadDirectory()
        {
            var projectDir = new DirectoryEntity(ProjectDir_Path);
            Nodes.Clear();
            foreach (var entity in projectDir.GetSubordinates())
            {
                Nodes.Add(new FileTreeNode(entity));
            }
        }
    }
}