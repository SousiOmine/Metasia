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
        
        public ObservableCollection<FileTreeNode>? Nodes { get; }

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

        public ProjectToolViewModel(string? ProjectDir_Path)
        {
            this.ProjectDir_Path = ProjectDir_Path;
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


            Nodes = new ObservableCollection<FileTreeNode>()
            {
                /*new FileTreeNode("Timelines", new ObservableCollection<FileTreeNode>
                {
                    new FileTreeNode("RootTimeline.mtl"),
                    new FileTreeNode("Timeline2.mtl"),
                    new FileTreeNode("Timeline3.mtl"),
                }),
                new FileTreeNode("packages", new ObservableCollection<FileTreeNode>
                {
                    new FileTreeNode("freimg"),
                }),
                new FileTreeNode("karimovie.mtpj"),*/
                
            };

            var kari = new DirectoryEntity("./../");
            foreach (var entity in kari.GetSubordinates())
            {
                Nodes.Add(new FileTreeNode(entity));
            }
        }
    }
}