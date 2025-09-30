using Avalonia.Controls;

namespace Metasia.Editor.Views.Inspector.Properties;

public partial class DoublePropertyView : UserControl
{
    public DoublePropertyView()
    {
        InitializeComponent();

        // スライダーのプレビュー機能
        if (this.FindControl<Slider>("ValueSlider") is { } slider)
        {
            slider.PointerPressed += (_, _) =>
            {
                if (DataContext is ViewModels.Inspector.Properties.DoublePropertyViewModel vm)
                {
                    vm.StartSliderPreview();
                }
            };

            slider.PointerReleased += (_, _) =>
            {
                if (DataContext is ViewModels.Inspector.Properties.DoublePropertyViewModel vm)
                {
                    vm.EndSliderPreview();
                }
            };
        }
    }
}