using System.Collections.ObjectModel;

namespace Metasia.Editor.Models.Tools.ProjectTool
{
    public class FileTreeNode
    {
        public string Title { get; }
        public ObservableCollection<FileTreeNode> SubNodes { get; }
        
        public FileTreeNode(string Title)
        {
            this.Title = Title;
        }

        public FileTreeNode(string Title, ObservableCollection<FileTreeNode> subNodes)
        {
            this.Title = Title;
            SubNodes = subNodes;
        }
    }
}