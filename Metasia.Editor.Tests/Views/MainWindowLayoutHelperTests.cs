using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System.Text.Json;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Views;

namespace Metasia.Editor.Tests.Views
{
    [TestFixture]
    public class MainWindowLayoutHelperTests
    {
        [Test]
        public void NormalizeThreePaneRatios_ReturnsDefaultForInvalidValues()
        {
            var result = MainWindowLayoutHelper.NormalizeThreePaneRatios(0, 2, 3);

            Assert.That(result.Left, Is.EqualTo(MainWindowLayoutHelper.DefaultLeftPaneRatio));
            Assert.That(result.Center, Is.EqualTo(MainWindowLayoutHelper.DefaultCenterPaneRatio));
            Assert.That(result.Right, Is.EqualTo(MainWindowLayoutHelper.DefaultRightPaneRatio));
        }

        [Test]
        public void NormalizeThreePaneRatios_NormalizesValidValues()
        {
            var result = MainWindowLayoutHelper.NormalizeThreePaneRatios(100, 300, 200);

            Assert.That(result.Left, Is.EqualTo(1d / 6d).Within(0.000001));
            Assert.That(result.Center, Is.EqualTo(3d / 6d).Within(0.000001));
            Assert.That(result.Right, Is.EqualTo(2d / 6d).Within(0.000001));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-0.1)]
        public void NormalizeTopPaneRatio_ReturnsDefaultForInvalidValues(double value)
        {
            var result = MainWindowLayoutHelper.NormalizeTopPaneRatio(value);

            Assert.That(result, Is.EqualTo(MainWindowLayoutHelper.DefaultTopPaneRatio));
        }

        [Test]
        public void EditorSettings_SerializesMainWindowLayout()
        {
            var settings = new EditorSettings
            {
                MainWindowLayout = new MainWindowLayoutSettings
                {
                    IsMaximized = true,
                    NormalWidth = 1600,
                    NormalHeight = 900,
                    LeftPaneRatio = 0.2,
                    CenterPaneRatio = 0.5,
                    RightPaneRatio = 0.3,
                    TopPaneRatio = 0.6
                }
            };

            var json = JsonSerializer.Serialize(settings);
            var restored = JsonSerializer.Deserialize<EditorSettings>(json);

            Assert.That(restored, Is.Not.Null);
            Assert.That(restored!.MainWindowLayout.IsMaximized, Is.True);
            Assert.That(restored.MainWindowLayout.NormalWidth, Is.EqualTo(1600));
            Assert.That(restored.MainWindowLayout.NormalHeight, Is.EqualTo(900));
            Assert.That(restored.MainWindowLayout.LeftPaneRatio, Is.EqualTo(0.2));
            Assert.That(restored.MainWindowLayout.CenterPaneRatio, Is.EqualTo(0.5));
            Assert.That(restored.MainWindowLayout.RightPaneRatio, Is.EqualTo(0.3));
            Assert.That(restored.MainWindowLayout.TopPaneRatio, Is.EqualTo(0.6));
        }
    }
}
