using System.Windows.Controls;

namespace Schementi.Controls.Extensions.Silverlight {
    public static class ScrollViewerExtensions {
        public static void ScrollToRightEnd(this ScrollViewer scrollViewer) {
            scrollViewer.Dispatcher.BeginInvoke(() => scrollViewer.ScrollToHorizontalOffset(double.PositiveInfinity));
        }
    }
}
