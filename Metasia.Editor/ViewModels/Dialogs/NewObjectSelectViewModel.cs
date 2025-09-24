using System.Reactive;
using Metasia.Core.Objects;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Dialogs;

public class NewObjectSelectViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, IMetasiaObject?> OkCommand { get; }
    public ReactiveCommand<Unit, IMetasiaObject?> CancelCommand { get; }

    public NewObjectSelectViewModel()
    {
        OkCommand = ReactiveCommand.Create(() => (IMetasiaObject?)null);
        CancelCommand = ReactiveCommand.Create(() => (IMetasiaObject?)null);
    }
}