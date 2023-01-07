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
using znMusicPlayerWUI.DataEditor;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace znMusicPlayerWUI.Controls
{
    public partial class ArtistCard : Grid, IDisposable
    {
        public double ImageScaleDPI { get; set; } = 1.0;
        Artist _artist;
        public Artist Artist
        {
            get => _artist;
            set
            {
                _artist = value;
                DataContext = Artist;
            }
        }

        public ArtistCard()
        {
            InitializeComponent();
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
            dropShadow.BlurRadius = 40f;
            dropShadow.Opacity = 0f;
            dropShadow.Offset = new Vector3(0, 2, 0);

            basicRectVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(ShadowBaseRectangle, basicRectVisual);
        }

        private async void UILoaded(object sender, RoutedEventArgs e)
        {
            CreatShadow();
            PlayListImage.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(Artist.PicturePath));
        }

        private void UIUnloaded(object sender, RoutedEventArgs e)
        {
            PlayListImage.Dispose();
        }

        public void Dispose()
        {
            PlayListImage.Source = null;
            Artist = null;
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
        bool isRightPressed = false;
        private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            isPressed = true;
            isRightPressed = e.GetCurrentPoint(sender as UIElement).Properties.IsRightButtonPressed;
        }

        private void Grid_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isPressed)
            {
                if (isRightPressed)
                {
                    //FlyoutMenu.ShowAt(sender as FrameworkElement);
                    isRightPressed = false;
                }
                else
                {
                    MainWindow.SetNavViewContent(
                    typeof(Pages.ItemListViewArtist),
                    Artist,
                    new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                }
            }
            isPressed = false;
        }

        private void Grid_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {

        }

        private void Grid_Holding(object sender, Microsoft.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {

        }
    }
}
