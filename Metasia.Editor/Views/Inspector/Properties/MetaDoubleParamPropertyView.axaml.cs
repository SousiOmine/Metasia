using Avalonia.Controls;

namespace Metasia.Editor.Views.Inspector.Properties;

public partial class MetaDoubleParamPropertyView : UserControl
{
    public MetaDoubleParamPropertyView()
    {
        InitializeComponent();

        if (this.FindControl<Slider>("ValueSlider") is { } slider)
        {
            slider.PointerPressed += (_, _) =>
            {
                if (DataContext is ViewModels.Inspector.Properties.MetaDoubleParamPropertyViewModel vm)
                {
                    vm.StartSliderPreview();
                }
            };

            slider.PointerReleased += (_, _) =>
            {
                if (DataContext is ViewModels.Inspector.Properties.MetaDoubleParamPropertyViewModel vm)
                {
                    vm.EndSliderPreview();
                }
            };
        }
    }
}