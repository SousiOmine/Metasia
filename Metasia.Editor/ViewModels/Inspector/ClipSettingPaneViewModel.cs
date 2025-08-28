using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Inspector;

public class ClipSettingPaneViewModel : ViewModelBase
{
    private ClipObject targetObject;
    public ClipSettingPaneViewModel(ClipObject target)
    {
        targetObject = target;
    }
}
