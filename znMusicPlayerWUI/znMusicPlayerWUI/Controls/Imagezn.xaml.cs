using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Composition;
using znMusicPlayerWUI.Media;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.DataEditor;
using Microsoft.UI.Xaml.Hosting;

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
            CreateVisualsAnimation();
        }

        Visual animationVisual = null;
        Visual animationVisualMass = null;
        ScalarKeyFrameAnimation animationOpacity_SoureChanged = null;
        ScalarKeyFrameAnimation animationMassOpacity_MouseIn = null;
        ScalarKeyFrameAnimation animationMassOpacity_MouseExited = null;
        ScalarKeyFrameAnimation animationSize_MouseIn = null;
        ScalarKeyFrameAnimation animationSize_MouseExited = null;
        ScalarKeyFrameAnimation animationSize_MousePressed = null;
        ScalarKeyFrameAnimation animationSize_MouseReleased = null;
        private void CreateVisualsAnimation()
        {
            animationVisual = ElementCompositionPreview.GetElementVisual(ImageSourceRoot);
            animationVisualMass = ElementCompositionPreview.GetElementVisual(ImageMassAlpha);

            // 图片源切换动画
            AnimateHelper.AnimateScalar(
                animationVisual, 1, 02,
                0.2f, 1, 0.22f, 1f,
                out animationOpacity_SoureChanged);
            // 鼠标移入遮罩动画
            AnimateHelper.AnimateScalar(
                animationVisualMass, 1f, 0.2,
                0.2f, 1, 0.22f, 1f,
                out animationMassOpacity_MouseIn);
            // 鼠标移入 Size 动画
            AnimateHelper.AnimateScalar(
                animationVisual, 1.07f, 0.2,
                0.2f, 1, 0.22f, 1f,
                out animationSize_MouseIn);
            // 鼠标移出遮罩动画
            AnimateHelper.AnimateScalar(
                animationVisualMass, 0, 1.3,
                0, 0, 0, 0,
                out animationMassOpacity_MouseExited);
            // 鼠标移出 Size 动画
            AnimateHelper.AnimateScalar(
                animationVisual, 1f, 1.5,
                0.2f, 1, 0.22f, 1f,
                out animationSize_MouseExited);
            // 鼠标按下 Size 动画
            AnimateHelper.AnimateScalar(
                animationVisual, 0.93f, 0.5,
                0.2f, 1, 0.22f, 1f,
                out animationSize_MousePressed);
            // 鼠标松起 Size 动画
            AnimateHelper.AnimateScalar(
                animationVisual, 1.07f, 1.5,
                0.2f, 1, 0.22f, 1f,
                out animationSize_MouseReleased);
        }

        public void DisposeVisualsAnimation()
        {
            animationVisual?.Dispose();
            animationVisualMass?.Dispose();
            animationOpacity_SoureChanged?.Dispose();
            animationMassOpacity_MouseIn?.Dispose();
            animationMassOpacity_MouseExited?.Dispose();
            animationSize_MouseIn?.Dispose();
            animationSize_MouseExited?.Dispose();
            animationSize_MousePressed?.Dispose();
            animationSize_MouseReleased?.Dispose();
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

            if (Source == null) return;
            if (animationVisual == null) return;

            animationVisual.Opacity = 0;
            animationVisual.StartAnimation(nameof(animationVisual.Opacity), animationOpacity_SoureChanged);
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
        private async void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (animationVisual == null) return;
            if (animationVisualMass == null) return;
            if (isEnterDialog) return;
            if (ShowMenuBehavior == ShowMenuBehaviors.None) return;
            isPointEnter = true;

            animationVisualMass.StartAnimation(nameof(ImageMassAlpha.Opacity), animationMassOpacity_MouseIn);
            animationVisual.StartAnimation("Scale.X", animationSize_MouseIn);
            animationVisual.StartAnimation("Scale.Y", animationSize_MouseIn);
        }

        private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (animationVisual == null) return;
            if (animationVisualMass == null) return;
            if (ShowMenuBehavior == ShowMenuBehaviors.None) return;
            isPointEnter = false;

            animationVisualMass.StartAnimation(nameof(ImageMassAlpha.Opacity), animationMassOpacity_MouseExited);
            animationVisual.StartAnimation("Scale.X", animationSize_MouseExited);
            animationVisual.StartAnimation("Scale.Y", animationSize_MouseExited);
        }

        bool IsMouse4Click = false;
        private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsXButton1Pressed ||
                e.GetCurrentPoint(this).Properties.IsXButton2Pressed)
            {
                IsMouse4Click = true;
            }

            if (animationVisual == null) return;
            animationVisual.StartAnimation("Scale.X", animationSize_MousePressed);
            animationVisual.StartAnimation("Scale.Y", animationSize_MousePressed);
        }

        private void Grid_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (animationVisual == null) return;
            animationVisual.StartAnimation("Scale.X", animationSize_MouseReleased);
            animationVisual.StartAnimation("Scale.Y", animationSize_MouseReleased);
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
            if (animationVisual == null) return;
            animationVisual.CenterPoint = new((float)ActualWidth / 2, (float)ActualHeight / 2, 1);
        }

        private void RootGrid_ActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdateTheme();
        }

        private void ImageSource_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }
}
