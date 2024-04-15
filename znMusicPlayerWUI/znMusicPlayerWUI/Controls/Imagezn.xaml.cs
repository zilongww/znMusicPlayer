using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Composition;
using znMusicPlayerWUI.Media;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Controls
{
    public partial class Imagezn : Grid, IDisposable
    {
        public static bool ImageDarkMass { get; set; } = true;

        public enum ShowMenuBehaviors { Tapped, OnlyLightUp, None }

        private ShowMenuBehaviors _showMenuBehavior = default;
        public ShowMenuBehaviors ShowMenuBehavior
        {
            get => _showMenuBehavior;
            set
            {
                _showMenuBehavior = value;
            }
        }

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
                UpdateSource();
            }
        }

        public string SaveName { get; set; }

        double dataDPI = 1.0;
        public double DataDPI
        {
            get => dataDPI; set => dataDPI = value;
        }

        MusicData musicData = null;
        public MusicData DataSource
        {
            get => musicData;
            set
            {
                musicData = value;
                UpdateDatas();
            }
        }


        private bool _isMassImage = true;
        public bool IsMassImage
        {
            get => _isMassImage;
            set
            {
                _isMassImage = value;
                UpdateTheme();
            }
        }

        public Stretch Stretch
        {
            get => ImageSource.Stretch;
            set
            {
                ImageSource.Stretch = value;
            }
        }

        public Imagezn()
        {
            InitializeComponent();
            UpdateTheme();
        }

        public void Dispose()
        {
            Source = null;
            ImageSource.Source = null;
        }

        private async void UpdateDatas()
        {
            var imageSource = (await ImageManage.GetImageSource(DataSource, (int)(50 * dataDPI), (int)(50 * dataDPI), true)).Item1;
            if (imageSource != null)
            {

                System.Diagnostics.Debug.WriteLine(musicData.Title);
                Source = imageSource;
            }
        }

        private void UpdateTheme()
        {
            if (IsMassImage)
            {
                if (ActualTheme == ElementTheme.Dark)
                {
                    if (ImageDarkMass)
                        ImageMass.Visibility = Visibility.Visible;
                    else
                        ImageMass.Visibility = Visibility.Collapsed;
                }
                else
                    ImageMass.Visibility = Visibility.Collapsed;
            }
            else
            {
                ImageMass.Visibility = Visibility.Collapsed;
            }
        }

        public void UpdateSource()
        {
            UpdateTheme();/*
            AnimateHelper.AnimateScalar(
                ImageSource, 1f, 0.4,
                0.2f, 1, 0.22f, 1f,
                out var scaleVisual, out var compositor1, out var animation);
            scaleVisual.CenterPoint = new(scaleVisual.Size.X / 2, scaleVisual.Size.Y / 2, 1);
            scaleVisual.Scale = new(1.25f);
            scaleVisual.StartAnimation("Scale.X", animation);
            scaleVisual.StartAnimation("Scale.Y", animation);*/

            AnimateHelper.AnimateScalar(
                ImageSource, 1, 02,
                0.2f, 1, 0.22f, 1f,
                out var visual, out var compositor, out var scalarKeyFrameAnimation);
            visual.Opacity = 0;
            visual.StartAnimation("Opacity", scalarKeyFrameAnimation);
            ImageSource.Source = Source;
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (ShowMenuBehavior == ShowMenuBehaviors.None || ShowMenuBehavior == ShowMenuBehaviors.OnlyLightUp) return;
            isEnterDialog = true;
            /*
            ScrollViewer scrollViewer = new()
            {
                ZoomMode = ZoomMode.Enabled,
                HorizontalScrollMode = ScrollMode.Enabled,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Visible
            };
            TextBlock textBlock = new() { Text = "Ctrl键滑动滚轮或双指滑动可进行缩放 100%", Margin = new(84, -32, 0, 0), IsHitTestVisible = false };
            scrollViewer.ViewChanged += (_, __) =>
            {
                try
                {
                    textBlock.Text = $"Ctrl键滑动滚轮或双指滑动可进行缩放 {Math.Round(scrollViewer.ZoomFactor * 100, 0)}%";
                }
                catch { }
            };
            scrollViewer.Content = new Border() { Child = new Image() { Source = Source }, CornerRadius = new(54) };
            Border border = new() { CornerRadius = new(4), Child = scrollViewer };
            Grid grid = new();
            grid.Children.Add(border);
            grid.Children.Add(textBlock);
            var result = await MainWindow.ShowDialog("查看图片", grid, "确定", "保存到文件...", defaultButton: ContentDialogButton.Close, fullSizeDesired: true);
            if (result == ContentDialogResult.Primary)
            {
            }*/
            var window = Windowed.ImageViewerWindow.ShowWindow(Source, SaveName);
            isEnterDialog = false;
        }

        private void Grid_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
        }

        bool isPointEnter = false;
        bool isFirstAnimate = true;
        private async void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isEnterDialog) return;
            if (ShowMenuBehavior == ShowMenuBehaviors.None) return;
            isPointEnter = true;

            ImageMassAlpha.Visibility = Visibility.Visible;
            AnimateHelper.AnimateScalar(
                ImageMassAlpha, 1f, 0.2,
                0.2f, 1, 0.22f, 1f,
                out var visual, out var compositor, out var scalarKeyFrameAnimation);
            visual.StartAnimation(nameof(ImageMassAlpha.Opacity), scalarKeyFrameAnimation);

            AnimateHelper.AnimateScalar(
                ImageSourceRoot, 1.07f, 0.2,
                0.2f, 1, 0.22f, 1f,
                out var scaleVisual, out var compositor1, out var animation);
            scaleVisual.CenterPoint = new(scaleVisual.Size.X / 2, scaleVisual.Size.Y / 2, 1);
            scaleVisual.StartAnimation("Scale.X", animation);
            scaleVisual.StartAnimation("Scale.Y", animation);

            if (isFirstAnimate)
            {
                isFirstAnimate = false;
                await Task.Delay(6);
                Grid_PointerEntered(null, null);
            }
        }

        private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (ShowMenuBehavior == ShowMenuBehaviors.None) return;
            isPointEnter = false;
            AnimateHelper.AnimateScalar(
                ImageMassAlpha, 0, 1.3,
                0, 0, 0, 0,
                out var visual, out var compositor, out var scalarKeyFrameAnimation);
            visual.StartAnimation(nameof(ImageMassAlpha.Opacity), scalarKeyFrameAnimation);
            compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed += (_, __) =>
            {
                if (!isPointEnter)
                    ImageMassAlpha.Visibility = Visibility.Collapsed;
            };

            AnimateHelper.AnimateScalar(
                ImageSourceRoot, 1f, 1.5,
                0.2f, 1, 0.22f, 1f,
                out var scaleVisual, out var compositor1, out var animation);
            scaleVisual.CenterPoint = new(scaleVisual.Size.X / 2, scaleVisual.Size.Y / 2, 1);
            scaleVisual.StartAnimation("Scale.X", animation);
            scaleVisual.StartAnimation("Scale.Y", animation);
        }

        bool IsMouse4Click = false;
        private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsXButton1Pressed ||
                e.GetCurrentPoint(this).Properties.IsXButton2Pressed)
            {
                IsMouse4Click = true;
            }

            AnimateHelper.AnimateScalar(
                ImageSourceRoot, 0.93f, 0.5,
                0.2f, 1, 0.22f, 1f,
                out var scaleVisual, out var compositor1, out var animation);
            scaleVisual.CenterPoint = new(scaleVisual.Size.X / 2, scaleVisual.Size.Y / 2, 1);
            scaleVisual.StartAnimation("Scale.X", animation);
            scaleVisual.StartAnimation("Scale.Y", animation);
        }

        private void Grid_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            AnimateHelper.AnimateScalar(
                ImageSourceRoot, 1.07f, 1.5,
                0.2f, 1, 0.22f, 1f,
                out var scaleVisual, out var compositor1, out var animation);
            scaleVisual.CenterPoint = new(scaleVisual.Size.X / 2, scaleVisual.Size.Y / 2, 1);
            scaleVisual.StartAnimation("Scale.X", animation);
            scaleVisual.StartAnimation("Scale.Y", animation);
        }

        bool isEnterDialog = false;
        private void Grid_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!IsMouse4Click)
            {
                isEnterDialog = true;
                MenuFlyoutItem_Click(null, null);
            }
            IsMouse4Click = false;
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ImageSource.CenterPoint = new(5, 5, 1);
            RGClip.Rect = new(0, 0, ActualWidth, ActualHeight);
        }

        private void RootGrid_ActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateTheme();
        }
    }
}
