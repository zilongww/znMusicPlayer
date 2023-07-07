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
using Windows.Graphics.Imaging;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Media.Devices;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Controls
{
    public partial class Imagezn : Grid, IDisposable
    {
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
                UpdataSource();
            }
        }

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
                UpdataDatas();
            }
        }


        private bool _isMassImage = true;
        public bool IsMassImage
        {
            get => _isMassImage;
            set
            {
                _isMassImage = value;
                UpdataTheme();
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
            UpdataTheme();
        }

        public void Dispose()
        {
            Source = null;
            ImageSource.Source = null;
            ImageSourceBefore.Source = null;
            firstLoad = true;
        }

        private async void UpdataDatas()
        {
            var imageSource = (await ImageManage.GetImageSource(DataSource, (int)(50 * dataDPI), (int)(50 * dataDPI), true)).Item1;
            if (imageSource != null)
            {

                System.Diagnostics.Debug.WriteLine(musicData.Title);
                Source = imageSource;
            }
        }

        private void UpdataTheme()
        {
            if (IsMassImage)
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
            else
            {
                ImageMass.Visibility = Visibility.Collapsed;
            }
        }

        bool firstLoad = true;
        public void UpdataSource()
        {
            UpdataTheme();
            if (!firstLoad)
            {
                ImageSourceBefore.Source = ImageSource.Source;
                AnimateHelper.AnimateScalar(
                    ImageSource, 1, 2,
                    0.2f, 1, 0.22f, 1f,
                    out var visual, out var compositor, out var scalarKeyFrameAnimation);
                visual.Opacity = 0;
                visual.StartAnimation("Opacity", scalarKeyFrameAnimation);
                ImageSource.Source = Source;

                AnimateHelper.AnimateScalar(
                    ImageSourceBefore, 0, 2,
                    0.2f, 1, 0.22f, 1f,
                    out var visual1, out var compositor1, out var scalarKeyFrameAnimation1);
                visual1.Opacity = 1;
                compositor1.GetCommitBatch(CompositionBatchTypes.Animation).Completed += (_, __) =>
                {
                    try
                    {
                        ImageSourceBefore.Source = null;
                        //3System.Diagnostics.Debug.WriteLine("www");
                    }
                    catch(Exception err) { System.Diagnostics.Debug.WriteLine(err); }
                };
                visual1.StartAnimation("Opacity", scalarKeyFrameAnimation1);
            }
            else
            {
                AnimateHelper.AnimateScalar(
                    ImageSource, 1, 2,
                    0.2f, 1, 0.22f, 1f,
                    out var visual, out var compositor, out var scalarKeyFrameAnimation);
                visual.Opacity = 0;
                visual.StartAnimation("Opacity", scalarKeyFrameAnimation);
                ImageSource.Source = Source;
                firstLoad = false;
            }
            RefreshImageSource();
        }

        private async void RefreshImageSource()
        {
            await Task.Delay(1100);
            if (ImageSource.Source == null)
            {
                ImageSourceBefore.Source = null;
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (ShowMenuBehavior == ShowMenuBehaviors.None || ShowMenuBehavior == ShowMenuBehaviors.OnlyLightUp) return;
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
            var result = await MainWindow.ShowDialog("查看图片", grid, "确定", "保存到文件...");
            if (result == ContentDialogResult.Primary)
            {
                StorageFile f = null;
                
                if (Source.GetType() == typeof(WriteableBitmap))
                {
                    f = await FileHelper.UserSaveFile("一张图片", Windows.Storage.Pickers.PickerLocationId.PicturesLibrary, new[] { ".png" }, "图片");
                }
                else
                {
                    await MainWindow.ShowDialog("无法保存图片", "不支持保存此类型的图片。");
                }

                if (f != null)
                {
                    try
                    {
                        using (IRandomAccessStream stream = await f.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            WriteableBitmap wb = Source as WriteableBitmap;
                            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                            Stream pixelStream = wb.PixelBuffer.AsStream();
                            byte[] pixels = new byte[pixelStream.Length];
                            await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                                (uint)wb.PixelWidth,
                                                (uint)wb.PixelHeight,
                                                96.0, 96.0,
                                                pixels);
                            await encoder.FlushAsync();
                        }
                    }
                    catch (Exception err)
                    {
                        await MainWindow.ShowDialog("保存图片失败", $"保存图片时出现错误：\n{err.Message}");
                    }
                }
            }
        }

        private void Grid_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
        }

        bool isPointEnter = false;
        bool isFirstAnimate = true;
        private async void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (ShowMenuBehavior == ShowMenuBehaviors.None) return;
            isPointEnter = true;

            ImageMassAlpha.Visibility = Visibility.Visible;
            AnimateHelper.AnimateScalar(
                ImageMassAlpha, 1f, 0.2,
                0.2f, 1, 0.22f, 1f,
                out var visual, out var compositor, out var scalarKeyFrameAnimation);
            visual.StartAnimation(nameof(ImageMassAlpha.Opacity), scalarKeyFrameAnimation);

            AnimateHelper.AnimateScalar(
                ImageSource, 1.07f, 0.2,
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
                ImageMassAlpha, 0, 1.2,
                0, 0, 0, 0,
                out var visual, out var compositor, out var scalarKeyFrameAnimation);
            visual.StartAnimation(nameof(ImageMassAlpha.Opacity), scalarKeyFrameAnimation);
            compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed += (_, __) =>
            {
                if (!isPointEnter)
                    ImageMassAlpha.Visibility = Visibility.Collapsed;
            };

            AnimateHelper.AnimateScalar(
                ImageSource, 1f, 1.2,
                0.2f, 1, 0.22f, 1f,
                out var scaleVisual, out var compositor1, out var animation);
            scaleVisual.CenterPoint = new(scaleVisual.Size.X / 2, scaleVisual.Size.Y / 2, 1);
            scaleVisual.StartAnimation("Scale.X", animation);
            scaleVisual.StartAnimation("Scale.Y", animation);
        }

        bool IsMouse4Click = false;
        private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsXButton1Pressed)
            {
                IsMouse4Click = true;
            }
        }

        private void Grid_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
        }

        private void Grid_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!IsMouse4Click)
            {
                MenuFlyoutItem_Click(null, null);
            }
            IsMouse4Click = false;
        }

        private void RootGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //ImageSource.CenterPoint = new(5, 5, 1);
            RGClip.Rect = new(0, 0, ActualWidth, ActualHeight);
        }
    }
}
