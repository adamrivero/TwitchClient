using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TwitchClient.Helpers
{
    public class ScrollHelper : Behavior<ScrollViewer>
    {
        private double height = 0.0d;
        private ScrollViewer scrollViewer = null;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.scrollViewer = this.AssociatedObject;
            this.scrollViewer.LayoutUpdated += ScrollViewer_LayoutUpdated;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.scrollViewer != null)
            {
                this.scrollViewer.LayoutUpdated -= ScrollViewer_LayoutUpdated;
            }
        }

        private void ScrollViewer_LayoutUpdated(object sender, object e)
        {
            if (Math.Abs(this.scrollViewer.ExtentHeight - height) > 1)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
                this.height = this.scrollViewer.ExtentHeight;
            }
        }
    }
}
