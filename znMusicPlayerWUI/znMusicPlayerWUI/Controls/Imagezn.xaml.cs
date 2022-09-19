using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;
using znMusicPlayerWUI.Media;
using znMusicPlayerWUI.Pages;

namespace znMusicPlayerWUI.Controls
{
    public partial class Imagezn : Grid, IDisposable
    {
        public enum ShowMenuBehaviors { PointEnter, RightTaped, Tapped }

        public ShowMenuBehaviors ShowMenuBehavior { get; set; } = default;

        private ImageSource _source = null;
        public ImageSource Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
                UpdataSource();
            }
        }

        public Imagezn()
        {
            InitializeComponent();
            UpdataTheme();
        }

        public void Dispose()
        {
            ImageSource.Source = null;
        }

        private void UpdataTheme()
        {
            if (ActualTheme == ElementTheme.Dark)
            {
                ImageMass.Visibility = Visibility.Visible;
            }
            else
            {
                ImageMass.Visibility = Visibility.Collapsed;
            }
        }

        public void UpdataSource()
        {
            UpdataTheme();
            ImageSource.Source = Source;
        }

        private void OpacityAnimate()
        {
            AnimateHelper.AnimateScalar(
                MenuBtnBorder,
                1, 0.5, 1, 1, 1, 1,
                out var visual, out var compositor, out var animation);
            visual.Opacity = 0;
            visual.StartAnimation(nameof(visual.Opacity), animation);
        }

        private void RightTappedzn(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            if (ShowMenuBehavior == ShowMenuBehaviors.RightTaped)
            {
                OpacityAnimate();
                TimeBreak();
            }
        }

        private void Tappedzn(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ShowMenuBehavior == ShowMenuBehaviors.Tapped)
            {
                OpacityAnimate();
                TimeBreak();
            }
        }

        private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (ShowMenuBehavior == ShowMenuBehaviors.PointEnter)
            {
                OpacityAnimate();
                TimeBreak();
            }
        }

        private async void TimeBreak()
        {
            await Task.Delay(2000);
            AnimateHelper.AnimateScalar(
                MenuBtnBorder,
                0, 0.5, 1, 1, 1, 1,
                out var visual, out var compositor, out var animation);
            visual.Opacity = 1;
            visual.StartAnimation(nameof(visual.Opacity), animation);
        }

        private async void Clickzn(object sender, object e)
        {
            ScrollViewer scrollViewer = new();
            scrollViewer.Content = new Image() { Source = Source, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            await MainWindow.ShowDialog("查看图片", scrollViewer, "确定", "保存到文件...");
        }
    }
}
