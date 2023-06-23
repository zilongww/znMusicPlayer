using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using znMusicPlayerWUI.Helpers;

namespace znMusicPlayerWUI.Controls
{
    public enum ViewMode { Horizontal, Vertical, None };
    public partial class AutoScrollViewer : Grid
    {
        DispatcherTimer _timer;
        public ViewMode ViewMode { get; set; } = ViewMode.Horizontal;
        public bool isHorizontalContentOutOfBounds { get; private set; } = false;
        public bool isVerticalContentOutOfBounds { get; private set; } = false;

        public AutoScrollViewer()
        {
            InitializeComponent();
            _timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(64) };
            _timer.Tick += (_, __) => UpdataContentInterface();
        }

        FrameworkElement content = null;
        private async void sizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (isStop)
            {
                _timer.Stop();
                return;
            }
            RGClip.Rect = new(0, 0, ActualWidth, ActualHeight);
            if (!Children.Any()) return;
            content = Children.First() as FrameworkElement;
            if (content == null) return;

            if (content.ActualWidth > ActualWidth) isHorizontalContentOutOfBounds = true;
            else isHorizontalContentOutOfBounds = false;
            if (content.ActualHeight > ActualHeight) isVerticalContentOutOfBounds = true;
            else isVerticalContentOutOfBounds = false;

            Debug.WriteLine("animating");
            _timer.Start();
        }

        bool allowAnimate = true;
        bool isHorizontalAnimating = false;
        bool isVerticalAnimating = false;
        public async void UpdataContentInterface()
        {
            if (!isHorizontalContentOutOfBounds && !isVerticalContentOutOfBounds)
            {
                _timer.Stop();
                return;
            }

            if (ViewMode == ViewMode.Horizontal && isHorizontalContentOutOfBounds)
            {
                content.Margin = new(content.Margin.Left - 5, 0, 0, 0);
                Debug.WriteLine($"{content.ActualWidth} / {Math.Abs(content.Margin.Left) + ActualWidth + 3}");
                if (content.ActualWidth < Math.Abs(content.Margin.Left) + ActualWidth + 3)
                {
                    Debug.WriteLine("animated");
                    _timer.Stop();
                    await Task.Delay(1000);
                    content.Margin = new(0, 0, 0, 0);
                }

            }
            else if (ViewMode == ViewMode.Vertical && isVerticalContentOutOfBounds)
            {

            }
            else
            {
                _timer.Stop();
            }
        }

        bool isStop = false;
        public void StopScrolling()
        {
            isStop = true;
            _timer.Stop();
            isHorizontalAnimating = false;
            isVerticalAnimating = false;
        }

        private async void unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("unloaded");
            StopScrolling();
        }
    }
}
