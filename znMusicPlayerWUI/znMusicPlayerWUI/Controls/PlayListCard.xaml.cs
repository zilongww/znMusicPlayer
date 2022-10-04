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

namespace znMusicPlayerWUI.Controls
{
    public partial class PlayListCard : Grid, IDisposable
    {
        private DataEditor.MusicListData MusicListData { get; set; }

        public PlayListCard(DataEditor.MusicListData musicListData)
        {
            InitializeComponent();
            MusicListData = musicListData;
            DataContext = musicListData;
            UpdataTheme();
        }

        private void UpdataTheme()
        {
            if (App.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                BlackColorBaseRectangle.Visibility = Visibility.Visible;
            }
            else
            {
                BlackColorBaseRectangle.Visibility = Visibility.Collapsed;
            }
        }

        Compositor compositor;
        DropShadow dropShadow;
        private void CreatShadow()
        {
            var visual = ElementCompositionPreview.GetElementVisual(ShadowBaseRectangle);
            compositor = visual.Compositor;

            var basicRectVisual = compositor.CreateSpriteVisual();
            basicRectVisual.Size = new Vector2((float)(ActualWidth - 8), (float)ActualHeight);

            dropShadow = compositor.CreateDropShadow();
            dropShadow.BlurRadius = 30f;
            dropShadow.Opacity = 0f;
            dropShadow.Offset = new Vector3(0, 2, 0);

            basicRectVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(ShadowBaseRectangle, basicRectVisual);
        }

        private async void UILoaded(object sender, RoutedEventArgs e)
        {
            CreatShadow();
            PlayListImage.Opacity = 0.01;
            if (MusicListData.ListDataType == DataEditor.DataType.本地歌单)
            {
                PlayListImage.Source = await FileHelper.GetImageSource(MusicListData.PicturePath, 150, 150, true);
            }
            else if (MusicListData.ListDataType == DataEditor.DataType.歌单)
            {
                PlayListImage.Source = await FileHelper.GetImageSource(await ImageManage.GetImageSource(MusicListData), 150, 150, true);
            }
            else
            {
                PlayListImage.Source = await FileHelper.GetImageSource("", 150, 150, true);
            }
            AnimateHelper.AnimateOpacity(
                PlayListImage,
                PlayListImage.Opacity, 1,
                0.5,
                new CubicEase() { EasingMode = EasingMode.EaseOut }).Begin();
        }

        private void UIUnloaded(object sender, RoutedEventArgs e)
        {
            PlayListImage.Source = null;
        }

        public void Dispose()
        {
            PlayListImage.Source = null;
            MusicListData = null;
            DataContext = null;
        }

        private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as UIElement).PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                AnimateHelper.AnimateOffset(
                    Children[1] as Border,
                    0, -2, 0, 0.5,
                    0.2f, 1f, 0.22f, 1f,
                    out Visual visual, out Compositor compositor, out Vector3KeyFrameAnimation animation);
                visual.StartAnimation(nameof(visual.Offset), animation);

                if (dropShadow != null)
                {
                    ScalarKeyFrameAnimation blurAnimation = compositor.CreateScalarKeyFrameAnimation();
                    blurAnimation.InsertKeyFrame(0.5f, 0.15f);
                    blurAnimation.Duration = TimeSpan.FromSeconds(0.5);
                    dropShadow.StartAnimation("Opacity", blurAnimation);
                }
            }
        }

        private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as UIElement).PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                AnimateHelper.AnimateOffset(
                    Children[1] as Border,
                    0, 0, 0, 0.5,
                    0.2f, 1f, 0.22f, 1f,
                    out Visual visual, out Compositor compositor, out Vector3KeyFrameAnimation animation);

                visual.StartAnimation(nameof(visual.Offset), animation);

                if (dropShadow != null)
                {
                    ScalarKeyFrameAnimation blurAnimation = compositor.CreateScalarKeyFrameAnimation();
                    blurAnimation.InsertKeyFrame(0.5f, 0f);
                    blurAnimation.Duration = TimeSpan.FromSeconds(0.5);
                    dropShadow.StartAnimation("Opacity", blurAnimation);
                }
            }
        }

        bool isPressed = false;
        private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            isPressed = true;
        }

        private void Grid_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isPressed)
            {
                MainWindow.SetNavViewContent(
                    typeof(ItemListView),
                    new List<object> { DataEditor.DataType.歌单, MusicListData },
                    new DrillInNavigationTransitionInfo());
            }
            isPressed = false;
        }

        private void Grid_ActualThemeChanged(FrameworkElement sender, object args)
        {
            UpdataTheme();
        }
    }
}
