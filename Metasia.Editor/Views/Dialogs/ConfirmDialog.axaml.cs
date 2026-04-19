using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Metasia.Editor.Views.Dialogs;

public enum ConfirmDialogResult
{
    Cancel = 0,
    Save = 1,
    DontSave = 2,
}

public partial class ConfirmDialog : Window
{
    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public ConfirmDialog(
        string title,
        string message,
        string saveButtonText = "保存",
        string dontSaveButtonText = "保存しない",
        string cancelButtonText = "キャンセル")
        : this()
    {
        Title = title;
        MessageText.Text = message;
        SaveButton.Content = saveButtonText;
        DontSaveButton.Content = dontSaveButtonText;
        CancelButton.Content = cancelButtonText;
    }

    private void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        Close(ConfirmDialogResult.Save);
    }

    private void OnDontSaveClick(object? sender, RoutedEventArgs e)
    {
        Close(ConfirmDialogResult.DontSave);
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Close(ConfirmDialogResult.Cancel);
    }
}
