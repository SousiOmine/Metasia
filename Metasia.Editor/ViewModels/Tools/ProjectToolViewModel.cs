using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using DynamicData;
using Metasia.Editor.Models.Tools.ProjectTool;

namespace Metasia.Editor.ViewModels.Tools
{
    public class ProjectToolViewModel : ViewModelBase
    {
        public string ProjectDir_Path { get; private set; } = String.Empty;
        
        public ObservableCollection<FileTreeNode>? Nodes { get; }
        
        public ProjectToolViewModel(string? ProjectDir_Path)
        {
            this.ProjectDir_Path = ProjectDir_Path;
            //プロジェクトディレクトリなしで作成された時はファイルを開くとかの案内を表示したい
            if (String.IsNullOrEmpty(ProjectDir_Path))
            {
                
            }

            Nodes = new ObservableCollection<FileTreeNode>()
            {
                new FileTreeNode("Timelines", new ObservableCollection<FileTreeNode>
                {
                    new FileTreeNode("RootTimeline.mtl"),
                    new FileTreeNode("Timeline2.mtl"),
                    new FileTreeNode("Timeline3.mtl"),
                }),
                new FileTreeNode("packages", new ObservableCollection<FileTreeNode>
                {
                    new FileTreeNode("freimg"),
                }),
                new FileTreeNode("karimovie.mtpj"),
                
            };
        }
    }
}