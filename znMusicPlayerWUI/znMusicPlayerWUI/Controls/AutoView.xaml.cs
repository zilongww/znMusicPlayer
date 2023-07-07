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
        public ViewMode ViewMode { get; set; } = ViewMode.Horizontal;
        public bool isHorizontalContentOutOfBounds { get; private set; } = false;
        public bool isVerticalContentOutOfBounds { get; private set; } = false;

        public AutoScrollViewer()
        {
            InitializeComponent();
        }

        FrameworkElement content = null;
        private async void sizeChanged(object sender, SizeChangedEventArgs e)
        {
            //RGClip.Rect = new(0, 0, ActualWidth, ActualHeight);
            if (!Children.Any()) return;
            content = Children.First() as FrameworkElement;
            if (content == null) return;

            if (content.ActualWidth > ActualWidth) isHorizontalContentOutOfBounds = true;
            else isHorizontalContentOutOfBounds = false;
            if (content.ActualHeight > ActualHeight) isVerticalContentOutOfBounds = true;
            else isVerticalContentOutOfBounds = false;

            UpdataContentInterface();
        }

        public async void UpdataContentInterface()
        {
            if (!isHorizontalContentOutOfBounds && !isVerticalContentOutOfBounds)
            {
                return;
            }

            if (ViewMode == ViewMode.Horizontal && isHorizontalContentOutOfBounds)
            {
                AnimateHelper.AnimateOffset(content,
                    -(float)content.ActualWidth, 0, 1,
                    5,
                    0, 0, 0, 0,
                    out var elementVisual, out var compositor, out var animation);
                elementVisual.StartAnimation(nameof(elementVisual.Offset), animation);
            }
            else if (ViewMode == ViewMode.Vertical && isVerticalContentOutOfBounds)
            {

            }
            else
            {
            }
        }

        private async void unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("unloaded");
        }
    }
}
