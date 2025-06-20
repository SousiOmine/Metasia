using NUnit.Framework;
using Metasia.Editor.ViewModels;

namespace Metasia.Editor.Tests.ViewModels
{
    [TestFixture]
    public class MainWindowViewModelTests
    {
        private MainWindowViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _viewModel = new MainWindowViewModel();
        }

        [Test]
        public void Constructor_InitializesViewModelsAndCommands()
        {
            // Assert that ViewModel properties are initialized
            Assert.IsNotNull(_viewModel.PlayerParentVM, "PlayerParentVM should be initialized.");
            Assert.IsNotNull(_viewModel.inspectorViewModel, "inspectorViewModel should be initialized.");
            Assert.IsNotNull(_viewModel.TimelineParentVM, "TimelineParentVM should be initialized.");
            Assert.IsNotNull(_viewModel.ToolsVM, "ToolsVM should be initialized.");

            // Assert that Command properties are initialized
            Assert.IsNotNull(_viewModel.SaveEditingProject, "SaveEditingProject command should be initialized.");
            Assert.IsNotNull(_viewModel.LoadEditingProject, "LoadEditingProject command should be initialized.");
            Assert.IsNotNull(_viewModel.CreateNewProject, "CreateNewProject command should be initialized.");
            Assert.IsNotNull(_viewModel.Undo, "Undo command should be initialized.");
            Assert.IsNotNull(_viewModel.Redo, "Redo command should be initialized.");
        }
    }
}
