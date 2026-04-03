using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;

namespace Metasia.Editor.Views
{
    public static class MainWindowLayoutHelper
    {
        public const double DefaultLeftPaneRatio = 1d / 6d;
        public const double DefaultCenterPaneRatio = 3d / 6d;
        public const double DefaultRightPaneRatio = 2d / 6d;
        public const double DefaultTopPaneRatio = 0.5d;

        public static (double Left, double Center, double Right) NormalizeThreePaneRatios(double left, double center, double right)
        {
            if (!IsPositiveFinite(left) || !IsPositiveFinite(center) || !IsPositiveFinite(right))
            {
                return (DefaultLeftPaneRatio, DefaultCenterPaneRatio, DefaultRightPaneRatio);
            }

            var total = left + center + right;
            if (!IsPositiveFinite(total))
            {
                return (DefaultLeftPaneRatio, DefaultCenterPaneRatio, DefaultRightPaneRatio);
            }

            return (left / total, center / total, right / total);
        }

        public static double NormalizeTopPaneRatio(double topRatio)
        {
            if (double.IsNaN(topRatio) || double.IsInfinity(topRatio) || topRatio <= 0 || topRatio >= 1)
            {
                return DefaultTopPaneRatio;
            }

            return topRatio;
        }

        private static bool IsPositiveFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
        }
    }
}
