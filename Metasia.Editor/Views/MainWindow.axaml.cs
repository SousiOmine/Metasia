using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Models;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Services;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Editor.ViewModels;
using Metasia.Editor.Views.Dialogs;
using System.IO;

namespace Metasia.Editor.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel => DataContext as MainWindowViewModel;
        private readonly Grid? _mainLayoutGrid;
        private readonly Grid? _topPaneGrid;
        private bool _layoutRestored;
        private bool _isSavingLayout;
        private bool _isCloseConfirmed;
        private Size? _lastNormalWindowSize;
        private MenuView? _menuView;

        public MainWindow()
        {
            InitializeComponent();

            _mainLayoutGrid = this.FindControl<Grid>("MainLayoutGrid");
            _topPaneGrid = this.FindControl<Grid>("TopPaneGrid");
            _menuView = this.FindControl<MenuView>("MainMenuView");

            DataContextChanged += OnDataContextChanged;
            Opened += OnOpened;
            Closing += OnClosing;
            Resized += OnResized;

            Loaded += (s, e) =>
            {
                Focus();
            };
        }

        private void OnDataContextChanged(object? sender, EventArgs args)
        {
            if (_viewModel is not { } viewModel)
            {
                return;
            }

            var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
            keyBindingService?.ApplyKeyBindings(this);

            if (_menuView is not null && App.Current?.Services is not null)
            {
                _menuView.DataContext = App.Current.Services.GetRequiredService<MenuViewModel>();

                if (_menuView.DataContext is MenuViewModel menuViewModel)
                {
                    menuViewModel.ExitInteraction.RegisterHandler(async interaction =>
                    {
                        await TryCloseWithConfirmAsync();
                        interaction.SetOutput(Unit.Default);
                    });
                }
            }
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            if (_layoutRestored)
            {
                return;
            }

            RestoreLayoutFromSettings();
            _layoutRestored = true;
            Dispatcher.UIThread.Post(CaptureNormalWindowBounds, DispatcherPriority.Background);
        }

        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            if (_isCloseConfirmed)
            {
                PersistLayout();
                return;
            }

            var projectState = App.Current?.Services?.GetService<IProjectState>();
            if (projectState is not null && projectState.IsDirty && projectState.CurrentProject is not null)
            {
                e.Cancel = true;
                Dispatcher.UIThread.Post(async () => await TryCloseWithConfirmAsync());
            }
            else
            {
                PersistLayout();
            }
        }

        private async Task TryCloseWithConfirmAsync()
        {
            if (_isCloseConfirmed) return;

            var projectState = App.Current?.Services?.GetService<IProjectState>();
            if (projectState is null || !projectState.IsDirty || projectState.CurrentProject is null)
            {
                _isCloseConfirmed = true;
                Close();
                return;
            }

            var dialog = new ConfirmDialog(
                "Metasia",
                "プロジェクトに未保存の変更があります。\n保存しますか？",
                "保存",
                "保存しない",
                "キャンセル");

            var result = await dialog.ShowDialog<ConfirmDialogResult>(this);

            switch (result)
            {
                case ConfirmDialogResult.Save:
                    var saved = await SaveProjectAsync(projectState);
                    if (!saved) return;
                    _isCloseConfirmed = true;
                    Close();
                    break;
                case ConfirmDialogResult.DontSave:
                    _isCloseConfirmed = true;
                    Close();
                    break;
                case ConfirmDialogResult.Cancel:
                default:
                    break;
            }
        }

        private async Task<bool> SaveProjectAsync(IProjectState projectState)
        {
            try
            {
                if (projectState.CurrentProject is null) return false;

                string? targetFilePath = projectState.CurrentProject.ProjectFilePath;
                if (string.IsNullOrEmpty(targetFilePath))
                {
                    var fileDialogService = App.Current?.Services?.GetService<IFileDialogService>();
                    if (fileDialogService is null) return false;

                    var file = await fileDialogService.SaveFileDialogAsync(
                        "プロジェクトを保存",
                        ["*.mtpj"],
                        "mtpj");
                    if (file is null) return false;
                    targetFilePath = file.Path.LocalPath;
                }

                if (string.IsNullOrEmpty(targetFilePath)) return false;

                ProjectSaveLoadManager.Save(projectState.CurrentProject, targetFilePath);

                if (string.IsNullOrEmpty(projectState.CurrentProject.ProjectFilePath))
                {
                    projectState.CurrentProject.ProjectFilePath = targetFilePath;
                    projectState.CurrentProject.ProjectPath = new DirectoryEntity(
                        Path.GetDirectoryName(targetFilePath)!);
                }

                projectState.MarkProjectSaved();
                return true;
            }
            catch (Exception ex)
            {
                var notificationService = App.Current?.Services?.GetService<INotificationService>();
                notificationService?.ShowError(
                    "プロジェクト保存失敗",
                    $"プロジェクトの保存に失敗しました。\n{ex.Message}");
                return false;
            }
        }

        private void OnResized(object? sender, WindowResizedEventArgs e)
        {
            CaptureNormalWindowBounds();
        }

        private void RestoreLayoutFromSettings()
        {
            var layout = App.Current?.Services?.GetService<ISettingsService>()?.CurrentSettings.MainWindowLayout;

            if (layout is null)
            {
                ApplyPaneRatios(
                    MainWindowLayoutHelper.DefaultLeftPaneRatio,
                    MainWindowLayoutHelper.DefaultCenterPaneRatio,
                    MainWindowLayoutHelper.DefaultRightPaneRatio,
                    MainWindowLayoutHelper.DefaultTopPaneRatio);
                return;
            }

            ApplyPaneRatios(layout.LeftPaneRatio, layout.CenterPaneRatio, layout.RightPaneRatio, layout.TopPaneRatio);

            if (layout.NormalWidth is > 0 && layout.NormalHeight is > 0)
            {
                Width = layout.NormalWidth.Value;
                Height = layout.NormalHeight.Value;
                _lastNormalWindowSize = new Size(layout.NormalWidth.Value, layout.NormalHeight.Value);
            }

            if (layout.IsMaximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void ApplyPaneRatios(double left, double center, double right, double topRatio)
        {
            var horizontalRatios = MainWindowLayoutHelper.NormalizeThreePaneRatios(left, center, right);
            var normalizedTopRatio = MainWindowLayoutHelper.NormalizeTopPaneRatio(topRatio);
            var leftColumnDefinition = GetResizableColumnDefinition(0);
            var centerColumnDefinition = GetResizableColumnDefinition(2);
            var rightColumnDefinition = GetResizableColumnDefinition(4);
            var topRowDefinition = GetResizableRowDefinition(0);
            var bottomRowDefinition = GetResizableRowDefinition(2);

            if (leftColumnDefinition is not null)
            {
                leftColumnDefinition.Width = new GridLength(horizontalRatios.Left, GridUnitType.Star);
            }

            if (centerColumnDefinition is not null)
            {
                centerColumnDefinition.Width = new GridLength(horizontalRatios.Center, GridUnitType.Star);
            }

            if (rightColumnDefinition is not null)
            {
                rightColumnDefinition.Width = new GridLength(horizontalRatios.Right, GridUnitType.Star);
            }

            if (topRowDefinition is not null)
            {
                topRowDefinition.Height = new GridLength(normalizedTopRatio, GridUnitType.Star);
            }

            if (bottomRowDefinition is not null)
            {
                bottomRowDefinition.Height = new GridLength(1d - normalizedTopRatio, GridUnitType.Star);
            }
        }

        private void CaptureNormalWindowBounds()
        {
            if (WindowState != WindowState.Normal)
            {
                return;
            }

            _lastNormalWindowSize = Bounds.Size;
        }

        private void PersistLayout()
        {
            if (_isSavingLayout)
            {
                return;
            }

            var settingsService = App.Current?.Services?.GetService<ISettingsService>();
            if (settingsService is null)
            {
                return;
            }

            _isSavingLayout = true;
            try
            {
                CaptureNormalWindowBounds();

                var settings = CloneSettings(settingsService.CurrentSettings);
                settings.MainWindowLayout = BuildLayoutSettings(settings.MainWindowLayout);
                settingsService.UpdateSettings(settings);
            }
            finally
            {
                _isSavingLayout = false;
            }
        }

        private MainWindowLayoutSettings BuildLayoutSettings(MainWindowLayoutSettings? current)
        {
            var layout = current ?? new MainWindowLayoutSettings();
            var size = _lastNormalWindowSize ?? Bounds.Size;
            var horizontalRatios = GetHorizontalPaneRatios();

            layout.IsMaximized = WindowState == WindowState.Maximized;
            layout.NormalWidth = size.Width > 0 ? size.Width : null;
            layout.NormalHeight = size.Height > 0 ? size.Height : null;
            layout.LeftPaneRatio = horizontalRatios.Left;
            layout.CenterPaneRatio = horizontalRatios.Center;
            layout.RightPaneRatio = horizontalRatios.Right;
            layout.TopPaneRatio = GetTopPaneRatio();
            return layout;
        }

        private (double Left, double Center, double Right) GetHorizontalPaneRatios()
        {
            return MainWindowLayoutHelper.NormalizeThreePaneRatios(
                GetDefinitionPixelSize(GetResizableColumnDefinition(0)),
                GetDefinitionPixelSize(GetResizableColumnDefinition(2)),
                GetDefinitionPixelSize(GetResizableColumnDefinition(4)));
        }

        private double GetTopPaneRatio()
        {
            var top = GetDefinitionPixelSize(GetResizableRowDefinition(0));
            var bottom = GetDefinitionPixelSize(GetResizableRowDefinition(2));
            var total = top + bottom;
            if (double.IsNaN(total) || double.IsInfinity(total) || total <= 0)
            {
                return MainWindowLayoutHelper.DefaultTopPaneRatio;
            }

            return MainWindowLayoutHelper.NormalizeTopPaneRatio(top / total);
        }

        private static double GetDefinitionPixelSize(DefinitionBase? definition)
        {
            return definition switch
            {
                ColumnDefinition column => column.ActualWidth,
                RowDefinition row => row.ActualHeight,
                _ => 0d
            };
        }

        private static EditorSettings CloneSettings(EditorSettings settings)
        {
            var json = JsonSerializer.Serialize(settings);
            return JsonSerializer.Deserialize<EditorSettings>(json) ?? new EditorSettings();
        }

        private ColumnDefinition? GetResizableColumnDefinition(int index)
        {
            if (_topPaneGrid is null || _topPaneGrid.ColumnDefinitions.Count <= index)
            {
                return null;
            }

            return _topPaneGrid.ColumnDefinitions[index];
        }

        private RowDefinition? GetResizableRowDefinition(int index)
        {
            if (_mainLayoutGrid is null || _mainLayoutGrid.RowDefinitions.Count <= index)
            {
                return null;
            }

            return _mainLayoutGrid.RowDefinitions[index];
        }

        protected override void OnUnloaded(Avalonia.Interactivity.RoutedEventArgs e)
        {
            DataContextChanged -= OnDataContextChanged;
            Opened -= OnOpened;
            Closing -= OnClosing;
            Resized -= OnResized;

            base.OnUnloaded(e);
        }
    }
}