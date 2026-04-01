using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Editor.Plugin;
using Metasia.Editor.Services.PluginService;
using Metasia.Editor.ViewModels.Tools;
using ReactiveUI;

namespace Metasia.Editor.ViewModels
{
    public class ToolsViewModel : ViewModelBase
    {
        public ProjectToolViewModel ProjectToolVM { get; }

        public ObservableCollection<LeftPanePanelItemViewModel> Panels { get; } = [];

        public LeftPanePanelItemViewModel? SelectedPanel
        {
            get => _selectedPanel;
            set => this.RaiseAndSetIfChanged(ref _selectedPanel, value);
        }

        private LeftPanePanelItemViewModel? _selectedPanel;

        public ToolsViewModel(
            PlayerParentViewModel playerParentViewModel,
            IProjectState projectState,
            ISelectionState selectionState,
            IEditCommandManager editCommandManager,
            IPluginService pluginService)
        {
            ProjectToolVM = new ProjectToolViewModel(playerParentViewModel, projectState, selectionState, editCommandManager);

            Panels.Add(new LeftPanePanelItemViewModel(
                "builtin.project",
                "Project",
                "Project",
                null,
                () => new Views.Tools.ProjectTool
                {
                    DataContext = ProjectToolVM
                }));

            foreach (var panel in pluginService.GetLeftPanePanels())
            {
                Panels.Add(new LeftPanePanelItemViewModel(
                    panel.Id,
                    panel.Title,
                    panel.Tooltip,
                    panel.Icon,
                    panel.CreateView));
            }

            SelectedPanel = Panels.Count > 0 ? Panels[0] : null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var panel in Panels)
                {
                    panel.Dispose();
                }

                ProjectToolVM.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    public sealed class LeftPanePanelItemViewModel : ReactiveObject, IDisposable
    {
        private readonly Func<Control> _createView;
        private Control? _content;
        private bool _isDisposed;

        public LeftPanePanelItemViewModel(
            string id,
            string title,
            string tooltip,
            Geometry? icon,
            Func<Control> createView)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            ArgumentException.ThrowIfNullOrWhiteSpace(title);
            ArgumentException.ThrowIfNullOrWhiteSpace(tooltip);
            ArgumentNullException.ThrowIfNull(createView);

            Id = id;
            Title = title;
            Tooltip = tooltip;
            Icon = icon;
            ShortTitle = title[..1].ToUpperInvariant();
            ShowFallbackText = icon is null;
            _createView = createView;
        }

        public string Id { get; }

        public string Title { get; }

        public string Tooltip { get; }

        public Geometry? Icon { get; }

        public bool HasIcon => Icon is not null;

        public bool ShowFallbackText { get; }

        public string ShortTitle { get; }

        public Control Content => _content ??= CreateContent();

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (_content is IDisposable disposable)
            {
                disposable.Dispose();
            }

            _isDisposed = true;
        }

        private Control CreateContent()
        {
            try
            {
                return _createView() ?? CreateErrorView(new InvalidOperationException("Panel factory returned null."));
            }
            catch (Exception ex)
            {
                return CreateErrorView(ex);
            }
        }

        private static Control CreateErrorView(Exception ex)
        {
            return new Border
            {
                Padding = new Avalonia.Thickness(12),
                Child = new StackPanel
                {
                    Spacing = 8,
                    VerticalAlignment = VerticalAlignment.Top,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "パネルの読み込みに失敗しました。",
                            FontWeight = FontWeight.SemiBold,
                            TextWrapping = TextWrapping.Wrap
                        },
                        new TextBlock
                        {
                            Text = ex.Message,
                            TextWrapping = TextWrapping.Wrap
                        }
                    }
                }
            };
        }
    }
}

