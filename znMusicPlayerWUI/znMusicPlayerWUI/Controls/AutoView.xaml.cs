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
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Helpers;

namespace znMusicPlayerWUI.Controls
{
    public enum ViewMode { Horizontal, Vertical, None };
    public partial class AutoScrollViewer : ScrollView
    {
        public ViewMode ViewMode { get; set; } = ViewMode.Horizontal;

        public static readonly DependencyProperty PauseProperty = DependencyProperty.Register(
            "Pause",
            typeof(bool),
            typeof(AutoScrollViewer),
            new PropertyMetadata(null, null)
        );
        public bool Pause
        {
            get => (bool)GetValue(PauseProperty);
            set
            {
                SetValue(PauseProperty, value);
                if (!value)
                {
                    RepeatChangeView();
                }
                else
                {
                    ScrollTo(0, 0, new(ScrollingAnimationMode.Disabled, ScrollingSnapPointsMode.Ignore));
                }
            }
        }
        public bool isHorizontalContentOutOfBounds { get; private set; } = false;
        public bool isVerticalContentOutOfBounds { get; private set; } = false;
        public int RepeatTime { get; set; } = 3000;

        public AutoScrollViewer()
        {
            InitializeComponent();
        }

        FrameworkElement content;
        private void GetSizeResult()
        {
            if (Content is null) return;
            content = (FrameworkElement)Content;
            if (content.ActualWidth > ActualWidth) isHorizontalContentOutOfBounds = true;
            else isHorizontalContentOutOfBounds = false;
            if (content.ActualHeight > ActualSize.Y) isVerticalContentOutOfBounds = true;
            else isVerticalContentOutOfBounds = false;
        }

        private void sizeChanged(object sender, SizeChangedEventArgs e)
        {
            //RGClip.Rect = new(0, 0, ActualWidth, ActualHeight);
            RepeatChangeView();
        }

        bool isAddedVelocity = false;
        bool pauseChanged = false;
        private async void RepeatChangeView()
        {
            //Debug.WriteLine("Repeating.");
            if (Content is null) return;
            if (Visibility == Visibility.Collapsed) return;
            if (isAddedVelocity) return;
            if (ActualSize.X <= 0 || ActualSize.Y <= 0) return;

            GetSizeResult();
            if (!isHorizontalContentOutOfBounds && !isVerticalContentOutOfBounds) return;

            isAddedVelocity = true;
            await Task.Delay(RepeatTime);
            isAddedVelocity = false;

            if (Pause) return;

            GetSizeResult();
            if (!isHorizontalContentOutOfBounds && !isVerticalContentOutOfBounds) return;

            Debug.WriteLine("[AutoView]: Repeated.");
            if (HorizontalOffset == 0 ? false : true)
            {
                ScrollTo(0, 0, new(ScrollingAnimationMode.Enabled));
                return;
            }

            if (isHorizontalContentOutOfBounds)
            {
                float velocity = (float)Math.Min(80f, Math.Max(4, ActualWidth / 4 + content.ActualWidth / 12));
                AddScrollVelocity(new(velocity, 0), new());
            }
            if (isVerticalContentOutOfBounds)
            {
                float velocity = Math.Min(80f, Math.Max(4f, ActualSize.Y / 4 + Content.ActualSize.Y / 12));
                AddScrollVelocity(new(0, velocity), new());
            }
        }

        private async void unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void ScrollView_ScrollCompleted(ScrollView sender, ScrollingScrollCompletedEventArgs args)
        {
            if (Pause) return;
            RepeatChangeView();
        }

        private void ScrollView_ViewChanged(ScrollView sender, object args)
        {

        }

        private void ScrollView_ExtentChanged(ScrollView sender, object args)
        {
            if (Content is not null)
            {
                (Content as FrameworkElement).Unloaded += AutoScrollViewer_Unloaded;
                ScrollTo(0, 0, new(ScrollingAnimationMode.Disabled, ScrollingSnapPointsMode.Ignore));
                Pause = false;
                //Debug.WriteLine("Content Changed.");
            }
        }

        private void AutoScrollViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            Pause = true;
            //Debug.WriteLine("Content Unloaded.");
            (sender as FrameworkElement).Unloaded -= AutoScrollViewer_Unloaded;
        }
    }
}
