using Avalonia.Controls;

namespace Metasia.Editor.Views.Timeline;

public partial class ClipView : UserControl
{
    public ClipView()
    {
        InitializeComponent();
    }

    // ClipViewBehaviorからのドラッグ開始通知用メソッド
    public void NotifyDragStarted()
    {
    }
}
