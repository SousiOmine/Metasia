using System.Collections.ObjectModel;
using Metasia.Editor.Models.FileSystem;

namespace Metasia.Editor.Models.Tools.ProjectTool
{
    public class FileTreeNode
    {
        public string? Title { get; }
        
        public IResourceEntity? ResourceEntity { get; }
        public ObservableCollection<FileTreeNode>? SubNodes { get; }
        
        public FileTreeNode(string? Title)
        {
            this.Title = Title;
        }

        public FileTreeNode(string? Title, ObservableCollection<FileTreeNode> subNodes)
        {
            this.Title = Title;
            SubNodes = subNodes;
        }
        
        public FileTreeNode(IResourceEntity resourceEntity)
        {
            this.ResourceEntity = resourceEntity;
            Title = ResourceEntity.Name;

            if (ResourceEntity is IDirectoryEntity directoryEntity)
            {
                SubNodes = new ObservableCollection<FileTreeNode>();
                foreach (var entity in directoryEntity.GetSubordinates())
                {
                    SubNodes.Add(new FileTreeNode(entity));
                }
            }

        }
    }
}