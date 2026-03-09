using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.Views.Dialogs;
using Metasia.Editor.Views.Settings;

namespace Metasia.Editor.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel => DataContext as MainWindowViewModel;
        private IDisposable? _newProjectHandlerDisposable;
        private IDisposable? _outputHandlerDisposable;
        private OutputWindow? _outputWindow;
        private IDisposable? _openSettingsHandlerDisposable;
        private readonly Grid? _mainLayoutGrid;
        private readonly Grid? _topPaneGrid;
        private bool _layoutRestored;
        private bool _isSavingLayout;
        private Size? _lastNormalWindowSize;

        public MainWindow()
        {
            InitializeComponent();

            _mainLayoutGrid = this.FindControl<Grid>("MainLayoutGrid");
            _topPaneGrid = this.FindControl<Grid>("TopPaneGrid");

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
            // Dispose previous handler if it exists
            _newProjectHandlerDisposable?.Dispose();
            _openSettingsHandlerDisposable?.Dispose();
            _newProjectHandlerDisposable = null;
            _outputHandlerDisposable?.Dispose();
            _outputHandlerDisposable = null;
            _outputWindow?.Close();
            _outputWindow = null;
            _openSettingsHandlerDisposable = null;

            if (_viewModel is not { } viewModel)
            {
                return;
            }

            // ViewModelが設定された後にキーバインディングを適用
            var keyBindingService = App.Current?.Services?.GetService<IKeyBindingService>();
            keyBindingService?.ApplyKeyBindings(this);

            // NewProjectInteractionのハンドラーを登録
            _newProjectHandlerDisposable = viewModel.NewProjectInteraction.RegisterHandler(async interaction =>
            {
                try
                {
                    var dialog = new NewProjectDialog()
                    {
                        DataContext = interaction.Input
                    };
                    var result = await dialog.ShowDialog<(bool, string, Metasia.Core.Project.ProjectInfo, Metasia.Core.Project.MetasiaProject?)>(this);
                    interaction.SetOutput(result);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in NewProjectInteraction handler: {ex.Message}");
                    interaction.SetOutput((false, ex.Message, null, null));
                }
            });

            // OutputInteractionのハンドラーを登録
            _outputHandlerDisposable = viewModel.OutputInteraction.RegisterHandler(interaction =>
            {
                try
                {
                    if (_outputWindow is null)
                    {
                        _outputWindow = new OutputWindow()
                        {
                            DataContext = interaction.Input
                        };
                        _outputWindow.Show(this);
                        _outputWindow.Closed += (s, e) =>
                        {
                            _outputWindow = null;
                        };
                    }
                    else
                    {
                        _outputWindow.Activate();
                    }
                    interaction.SetOutput(null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OutputInteraction handler: {ex.Message}");
                    interaction.SetOutput(null);
                }
            });
            // OpenSettingsInteractionのハンドラーを登録
            _openSettingsHandlerDisposable = viewModel.OpenSettingsInteraction.RegisterHandler(async interaction =>
            {
                try
                {
                    var serviceProvider = App.Current?.Services;
                    if (serviceProvider is not null)
                    {
                        var settingsWindow = new SettingsWindow()
                        {
                            DataContext = serviceProvider.GetRequiredService<SettingsWindowViewModel>()
                        };
                        await settingsWindow.ShowDialog(this);
                    }
                    interaction.SetOutput(Unit.Default);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in OpenSettingsInteraction handler: {ex.Message}");
                    interaction.SetOutput(Unit.Default);
                }
            });
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
            PersistLayout();
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
            _newProjectHandlerDisposable?.Dispose();
            _openSettingsHandlerDisposable?.Dispose();
            _newProjectHandlerDisposable = null;
            _outputHandlerDisposable?.Dispose();
            _outputHandlerDisposable = null;
            _outputWindow?.Close();
            _outputWindow = null;
            _openSettingsHandlerDisposable = null;

            DataContextChanged -= OnDataContextChanged;
            Opened -= OnOpened;
            Closing -= OnClosing;
            Resized -= OnResized;

            base.OnUnloaded(e);
        }
    }
}
