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
            ImageSource.Source = null;
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
            scrollViewer.Content = new Border() { Child = new Image() { Source = Source }, CornerRadius = new(54) };
            Border border = new() { CornerRadius = new(4), Child = scrollViewer };
            Grid grid = new();
            grid.Children.Add(border);
            grid.Children.Add(new TextBlock() { Text = "按住Ctrl键滑动滚轮可缩放", Margin = new(84, -32, 0, 0), IsHitTestVisible = false });
            var result = await MainWindow.ShowDialog("查看图片", grid, "确定", "保存到文件...");
            if (result == ContentDialogResult.Primary)
            {
                var f = await FileHelper.UserSaveFile("一张图片", Windows.Storage.Pickers.PickerLocationId.PicturesLibrary, new[] { ".png" }, "图片");
                if (f != null)
                {
                    var _bitmap = new RenderTargetBitmap();
                    await _bitmap.RenderAsync(ImageSource);
                    var pixels = await _bitmap.GetPixelsAsync();
                    using (IRandomAccessStream stream = await f.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        var encoder = await
                        BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                        byte[] bytes = pixels.ToArray();
                        encoder.SetPixelData(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Ignore,
                            (uint)_bitmap.PixelWidth,
                            (uint)_bitmap.PixelHeight,
                            0, 0,
                            bytes);

                        await encoder.FlushAsync();
                    }
                }
            }
        }
    }
}
