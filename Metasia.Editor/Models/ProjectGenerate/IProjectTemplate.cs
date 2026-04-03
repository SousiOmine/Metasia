using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Project;

namespace Metasia.Editor.Models.ProjectGenerate;

public interface IProjectTemplate
{
    public string Name { get; }
    public MetasiaProject Template { get; }
}

