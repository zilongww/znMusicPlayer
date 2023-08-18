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
using Newtonsoft.Json.Linq;
using Windows.UI;
using znMusicPlayerWUI.Pages.ListViewPages;
using znMusicPlayerWUI.Pages;

namespace znMusicPlayerWUI.Controls
{
    public partial class PlayListCard : Grid, IDisposable
    {
        private MusicListData MusicListData { get; set; }
        public double ImageScaleDPI { get; set; } = 1.0;
        public string ID { get; set; }
        public Imagezn ConnectAnimationElement { get; set; }
        public TextBlock ConnectAnimationElement1 { get; set; }

        public PlayListCard()
        {
            InitializeComponent();
            ConnectAnimationElement = PlayListImage;
            ConnectAnimationElement1 = TextBaseTb;
        }

        public async Task Init(MusicFrom musicFrom, string id)
        {
            MusicListData = await App.metingServices.NeteaseServices.GetPlayList(id);
            Init(MusicListData);
            UILoaded(null, null);
        }

        public void Init(MusicListData musicListData)
        {
            MusicListData = musicListData;
            if (MusicListData.ListDataType != DataType.歌单)
            {
                RefreshPlayListButton.Visibility = Visibility.Collapsed;
            }
            if (musicListData.ListDataType == DataType.本地歌单)
            {
                EditPlayListButton.Visibility = Visibility.Visible;
            }
            else
            {
                EditPlayListButton.Visibility = Visibility.Collapsed;
            }
            DataContext = musicListData;
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
            dropShadow.BlurRadius = 50f;
            dropShadow.Opacity = 0f;
            dropShadow.Color = Color.FromArgb(255, 0, 0, 0);
            dropShadow.Offset = new Vector3(0, 2, 0);

            basicRectVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(ShadowBaseRectangle, basicRectVisual);
        }

        private async void UILoaded(object sender, RoutedEventArgs e)
        {
            ExitMass();
            CreatShadow();
            if (MusicListData != null)
            {
                int size = (int)(200 * ImageScaleDPI);
                if (MusicListData.ListDataType == DataType.本地歌单)
                {
                    PlayListImage.Source = await FileHelper.GetImageSource(MusicListData.PicturePath, size, size, true);
                }
                else if (MusicListData.ListDataType == DataType.歌单)
                {
                    var path = await ImageManage.GetImageSource(MusicListData);
                    PlayListImage.Source = await FileHelper.GetImageSource(path, size, size, true);
                }
                else
                {
                    PlayListImage.Source = null;
                }
            }
            //System.Diagnostics.Debug.WriteLine(MusicListData.PicturePath);
        }

        private void UIUnloaded(object sender, RoutedEventArgs e)
        {
            PlayListImage.Dispose();
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
                    0, -2, 0,
                    0.2,
                    0.2f, 1f, 0.22f, 1f,
                    out Visual visual, out Compositor compositor, out Vector3KeyFrameAnimation animation);
                visual.StartAnimation(nameof(visual.Offset), animation);

                AnimateHelper.AnimateScalar(
                    ABackColorBaseRectAngle,
                    1f, 0.2,
                    0.2f, 1, 0.22f, 1,
                    out Visual visual2, out Compositor compositor2, out ScalarKeyFrameAnimation animation2);
                visual2.StartAnimation(nameof(visual2.Opacity), animation2);

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
                    0, 0, 0,
                    0.2,
                    0.2f, 1f, 0.22f, 1f,
                    out Visual visual, out Compositor compositor, out Vector3KeyFrameAnimation animation);
                visual.StartAnimation(nameof(visual.Offset), animation);

                ExitMass();

                if (dropShadow != null)
                {
                    ScalarKeyFrameAnimation blurAnimation = compositor.CreateScalarKeyFrameAnimation();
                    blurAnimation.InsertKeyFrame(1, 0f);
                    blurAnimation.Duration = TimeSpan.FromSeconds(0.5);
                    dropShadow.StartAnimation("Opacity", blurAnimation);
                }
            }
        }

        private void ExitMass()
        {
            AnimateHelper.AnimateScalar(
                ABackColorBaseRectAngle,
                0, 0.2,
                0.2f, 1, 0.22f, 1,
                out Visual visual2, out Compositor compositor2, out ScalarKeyFrameAnimation animation2);
            visual2.StartAnimation(nameof(visual2.Opacity), animation2);

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
            if (isPressed && MusicListData != null)
            {
                if (isRightPressed)
                {
                    FlyoutMenu.ShowAt(sender as FrameworkElement);
                    isRightPressed = false;
                }
                else
                {
                    ListViewPage.SetPageToListViewPage<ItemListViewPlayList>(MusicListData);
                }
            }
            isPressed = false;
        }

        private void Grid_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FlyoutMenu.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private void Grid_Holding(object sender, Microsoft.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            FlyoutMenu.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var isDelete = await MainWindow.ShowDialog("确认删除列表", $"真的要删除列表 \"{MusicListData.ListShowName}\" 吗？\n此操作不可逆。", "取消", "确定");
            if (isDelete == ContentDialogResult.Primary)
            {
                MainWindow.ShowLoadingDialog("正在删除");
                await PlayListHelper.DeletePlayList(MusicListData);
                await App.playListReader.Refresh();
                MainWindow.HideDialog();
            }
        }

        private async void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoadingDialog("正在更新歌单...");

            try
            {
                var deletePath = await ImageManage.GetImageSource(MusicListData);
                await Task.Run(() =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(deletePath)) File.Delete(deletePath);
                    }
                    catch { }
                });


                var playlist = await App.metingServices.NeteaseServices.GetPlayList(MusicListData.ID);
                MusicListData = playlist;

                var data = await PlayListHelper.ReadData();
                data[MusicListData.ListName] = JObject.FromObject(playlist);
                await PlayListHelper.SaveData(data);

                await App.playListReader.Refresh();

                MainWindow.HideDialog();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("PlayingList Update Error", ex.ToString(), false);
                MainWindow.HideDialog();
                await MainWindow.ShowDialog("更新歌单失败", $"更新歌单时遇到错误，请重试。\n错误信息：{ex}");
            }
        }

        private void Grid_AccessKeyInvoked(UIElement sender, Microsoft.UI.Xaml.Input.AccessKeyInvokedEventArgs args)
        {
            if (MusicListData != null)
                ListViewPage.SetPageToListViewPage<ItemListViewPlayList>(MusicListData);
        }

        private async void EditPlayListButton_Click(object sender, RoutedEventArgs e)
        {
            await Pages.DialogPages.EditPlayListPage.ShowDialog(MusicListData);
        }
    }
}
