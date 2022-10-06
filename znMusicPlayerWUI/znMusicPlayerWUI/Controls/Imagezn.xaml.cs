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

namespace znMusicPlayerWUI.Controls
{
    public partial class Imagezn : Grid, IDisposable
    {
        public enum ShowMenuBehaviors { PointEnter, RightTaped, Tapped, None }

        private ShowMenuBehaviors _showMenuBehavior = default;
        public ShowMenuBehaviors ShowMenuBehavior
        {
            get => _showMenuBehavior;
            set
            {
                _showMenuBehavior = value;
                if (ShowMenuBehavior == ShowMenuBehaviors.None)
                {
                    MenuBtn.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MenuBtn.Visibility = Visibility.Visible;
                }
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

        public Imagezn()
        {
            InitializeComponent();
            UpdataTheme();
        }

        public void Dispose()
        {
            Source = null;
            Source = null;
            firstLoad = true;
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
                    ImageSource, 1, 1,
                    0.2f, 1, 0.22f, 1f,
                    out var visual, out var compositor, out var scalarKeyFrameAnimation);
                visual.Opacity = 0;
                visual.StartAnimation("Opacity", scalarKeyFrameAnimation);
                compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed += (_, __) =>
                {
                    ImageSourceBefore.Source = null;
                };
                ImageSource.Source = Source;
            }
            else
            {
                AnimateHelper.AnimateScalar(
                    ImageSource, 1, 1,
                    0.2f, 1, 0.22f, 1f,
                    out var visual, out var compositor, out var scalarKeyFrameAnimation);
                visual.Opacity = 0;
                visual.StartAnimation("Opacity", scalarKeyFrameAnimation);
                ImageSource.Source = Source;
                firstLoad = false;
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (ShowMenuBehavior == ShowMenuBehaviors.None) return;
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
    }
}
