using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Dialogs;

public class OutputViewModel : ViewModelBase
{

    public ObservableCollection<string> OutputMethodList { get; } = new ObservableCollection<string>();

    public ObservableCollection<string> TimelineList { get; } = new ObservableCollection<string>();

    public ICommand CancelCommand { get; }

    public Action? CancelAction { get; set; }

    public OutputViewModel()
    {
        CancelCommand = ReactiveCommand.Create(() => Cancel());

        OutputMethodList.Add("連番画像出力");
        OutputMethodList.Add("FFmpegPlugin");

        TimelineList.Add("RootTimeline");
        TimelineList.Add("Timeline1");
        TimelineList.Add("Timeline2");
    }

    private void Cancel()
    {
        CancelAction?.Invoke();
    }

}

