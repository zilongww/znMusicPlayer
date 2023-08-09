﻿using Microsoft.UI;
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
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using znMusicPlayerWUI.Pages;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Storage.Pickers;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Media;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using znMusicPlayerWUI.Pages.ListViewPages;
using System.Diagnostics;
using CommunityToolkit.WinUI.UI;
using znMusicPlayerWUI.Controls;

namespace znMusicPlayerWUI.Pages
{
    public partial class ItemListViewAlbum : Page
    {
        private ScrollViewer scrollViewer { get; set; }
        public Album NavToObj { get; set; }
        public MusicFrom NowMusicFrom { get; set; } = MusicFrom.neteaseMusic;

        public ItemListViewAlbum()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlayAllButton.Foreground = new SolidColorBrush(CodeHelper.IsAccentColorDark() ? Colors.White : Colors.Black);
            Album a = (Album)e.Parameter;
            NavToObj = a;
            musicListData = new() { ListDataType = DataType.专辑 };
            InitData();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            await Task.Delay(500);
            scrollViewer?.ScrollToVerticalOffset(0);

            MusicDataList?.Clear();
            Children.ItemsSource = null;
            Children.Items.Clear();
            Album_Image.Dispose();
            musicListData = null;
            NavToObj = null;
            UnloadObject(this);
        }
/*
        private void CreatShadow()
        {
            var visual = ElementCompositionPreview.GetElementVisual(Album_Image);
            compositor = visual.Compositor;

            var basicRectVisual = compositor.CreateSpriteVisual();
            basicRectVisual.Size = Album_Image.RenderSize.ToVector2();

            dropShadow = compositor.CreateDropShadow();
            dropShadow.BlurRadius = 45f;
            dropShadow.Color = Colors.Black;
            dropShadow.Opacity = 0.3f;
            dropShadow.Offset = new Vector3(0, 4, 0);

            basicRectVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(Album_Image_DropShadowBase, basicRectVisual);
        }
*/
        public ObservableCollection<SongItemBindBase> MusicDataList = new();
        MusicListData musicListData = null;
        static bool firstInit = false;
        int pageNumber = 1;
        int pageSize = 30;
        public async void InitData()
        {
            SelectorSeparator.Visibility = Visibility.Collapsed;
            AddSelectedToPlayingListButton.Visibility = Visibility.Collapsed;
            AddSelectedToPlayListButton.Visibility = Visibility.Collapsed;
            DeleteSelectedButton.Visibility = Visibility.Collapsed;
            DownloadSelectedButton.Visibility = Visibility.Collapsed;
            SelectReverseButton.Visibility = Visibility.Collapsed;
            SelectAllButton.Visibility = Visibility.Collapsed;
            LoadingRing.Visibility = Visibility.Visible;
            LoadingRing.IsIndeterminate = true;
            var obj = await App.metingServices.NeteaseServices.GetAlbum(NavToObj.ID);
            if (obj == null)
            {
                await MainWindow.ShowDialog("加载专辑信息时出现错误", "无法加载专辑信息，请重试。");
                return;
            }
            NavToObj = obj;
            musicListData = NavToObj.Songs;
            Title2_Text.Text = obj.Title2;

            if (musicListData != null)
            {
                LoadImage();
                DescribeeText.Text = obj.Describee;
                LoadingRing.Visibility = Visibility.Visible;
                LoadingRing.IsIndeterminate = true;
                await Task.Delay(100);
                var dpi = CodeHelper.GetScaleAdjustment(App.WindowLocal);

                MusicDataList.Clear();
                int count = 0;
                foreach (var i in musicListData.Songs)
                {
                    count++;
                    i.Count = count;
                    MusicDataList.Add(new() { MusicData = i, ImageScaleDPI = dpi, MusicListData = musicListData });
                }
            }
            LoadingRing.IsIndeterminate = false;
            LoadingRing.Visibility = Visibility.Collapsed;
        }

        private async void LoadImage()
        {
            if (musicListData.ListDataType == DataType.本地歌单)
            {
                Album_Image.Source = await FileHelper.GetImageSource(musicListData.PicturePath);
            }
            else if (musicListData.ListDataType == DataType.歌单)
            {
                Album_Image.Source = await FileHelper.GetImageSource(await ImageManage.GetImageSource(musicListData));
            }
            else if (musicListData.ListDataType == DataType.专辑)
            {
                var art = NavToObj;
                Album_Image.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(art.PicturePath));
                System.Diagnostics.Debug.WriteLine(art.PicturePath);
            }
            AlbumLogo.Source = Album_Image.Source;
        }

        CompositionPropertySet scrollerPropertySet;
        Compositor compositor;
        Visual headerVisual;
        Visual massAlbumRootVisual;
        Visual blurAlbumRootVisual;
        Visual ImageScrollVisual;
        Visual logoVisual;
        Visual infoTextsRootVisual;
        Visual commandbarVisual;
        Visual describeeRootVisual;
        public void UpdateShyHeader()
        {
            if (scrollViewer == null) return;

            double anotherHeight = 180;
            String progress = $"Clamp(-scroller.Translation.Y / {anotherHeight}, 0, 1.0)";
            String describeeProgress = $"Clamp(-scroller.Translation.Y / 80, 0, 1.0)";
            String blurProgress = $"Clamp((-scroller.Translation.Y - 20) / {anotherHeight}, 0, 1.0)";
            String massProgress = $"Clamp((-scroller.Translation.Y - 150) / {anotherHeight}, 0, 1.0)";

            if (scrollerPropertySet == null)
            {
                scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
                compositor = scrollerPropertySet.Compositor;
                headerVisual = ElementCompositionPreview.GetElementVisual(menu_border);
                massAlbumRootVisual = ElementCompositionPreview.GetElementVisual(MassAlbumRoot);
                blurAlbumRootVisual = ElementCompositionPreview.GetElementVisual(BlurAlbumRoot);
                ImageScrollVisual = ElementCompositionPreview.GetElementVisual(Album_ImageBaseBorder);
                infoTextsRootVisual = ElementCompositionPreview.GetElementVisual(InfoTextsRoot);
                logoVisual = ElementCompositionPreview.GetElementVisual(AlbumLogoRoot);
                commandbarVisual = ElementCompositionPreview.GetElementVisual(ToolsCommandBar);
                describeeRootVisual = ElementCompositionPreview.GetElementVisual(DescribeeTextRoot);
            }

            logoVisual.CenterPoint = new(0, logoVisual.Size.Y, 1);

            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {anotherHeight}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            var blurAlbumRootVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            blurAlbumRootVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            blurAlbumRootVisual.StartAnimation("Opacity", blurAlbumRootVisualOpacityAnimation);

            var massAlbumRootVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(1, 0, {progress})");
            massAlbumRootVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            massAlbumRootVisual.StartAnimation("Opacity", massAlbumRootVisualOpacityAnimation);
/*
            var backgroundVisualScaleAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(1, 1, 1), Vector3(1, 0.4, 1), {progress})");
            backgroundVisualScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            backgroundVisual.StartAnimation(nameof(backgroundVisual.Scale), backgroundVisualScaleAnimation);
*/
            var describeeVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(1, 0, {describeeProgress})");
            describeeVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            describeeRootVisual.StartAnimation("Opacity", describeeVisualOpacityAnimation);

            var ImageVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0,0,0), Vector3(0,{anotherHeight / 1.2},0), {progress})");
            ImageVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            ImageScrollVisual.StartAnimation(nameof(ImageScrollVisual.Offset), ImageVisualOffsetAnimation);
            
            var logoVisualScaleAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(1, 1, 1), Vector3(0.348, 0.348, 1), {progress})");
            logoVisualScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation(nameof(logoVisual.Scale), logoVisualScaleAnimation);

            var toolsCommandBarVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp({(282 - commandbarVisual.Size.Y)}, {102 - commandbarVisual.Size.Y}, {progress})");
            toolsCommandBarVisualOffsetYAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            commandbarVisual.StartAnimation("Offset.Y", toolsCommandBarVisualOffsetYAnimation);

            var infoTextsRootVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(288,0,0), Vector3(108,{anotherHeight},0), {progress})");
            infoTextsRootVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            infoTextsRootVisual.StartAnimation(nameof(infoTextsRootVisual.Offset), infoTextsRootVisualOffsetAnimation);
        }

        private async void UpdateCommandToolBarWidth()
        {
            ToolsCommandBar.Width = 0;
            await Task.Delay(1);
            ToolsCommandBar.Width = double.NaN;
        }

        Vector3 ATBOffset = default;
        private void menu_border_Loaded(object sender, RoutedEventArgs e)
        {
            if (scrollViewer == null)
            {
                scrollViewer = (VisualTreeHelper.GetChild(Children, 0) as Border).Child as ScrollViewer;
                scrollViewer.CanContentRenderOutsideBounds = true;
                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;

                // 设置header为顶层
                var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)Children.Header);
                var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
                Canvas.SetZIndex(headerContainer, 1);
            }

            UpdateCommandToolBarWidth();
            Result_BaseGrid_SizeChanged(null, null);
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            UpdateShyHeader();
            if (scrollViewer != null)
                AlbumLogoRoot.CornerRadius = new(Math.Min(Math.Max(scrollViewer.VerticalOffset / 8, 8), 18));
            if (logoVisual != null)
            {
                var a = ActualWidth - (logoVisual.Scale.X * AlbumLogoRoot.ActualWidth + 44);
                if (a > 0)
                {
                    InfoTextsRoot.Width = a;
                    ToolsCommandBar.MaxWidth = a;
                }
            }
            if (headerVisual != null) headerVisual.IsPixelSnappingEnabled = true;
            //BackColorBaseRectangle.Margin = new(0, Math.Min(scrollViewer.VerticalOffset, 180), 0, 0);
        }

        private void Result_BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //menu_border.MinHeight = LittleBarGrid.ActualHeight;
            //try { menu_border.Height = ActualHeight; }
            //catch { }
            //ImageClip.Rect = new(0, 0, ActualWidth, ActualHeight);
            ScrollViewer_ViewChanging(null, null);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Children.Items.Any()) return;
            if (App.playingList.PlayBehavior == znMusicPlayerWUI.Background.PlayBehavior.随机播放)
            {
                App.playingList.ClearAll();
            }
            foreach (var songItem in MusicDataList)
            {
                App.playingList.Add(songItem.MusicData, false);
            }
            await App.playingList.Play(MusicDataList.First().MusicData, true);
            App.playingList.SetRandomPlay(App.playingList.PlayBehavior);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InitData();
        }

        DropShadow dropShadow;
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (Children.SelectionMode == ListViewSelectionMode.None)
            {
                SelectItemButton.Background = App.Current.Resources["AccentAAFillColorTertiaryBrush"] as Brush;
                Children.SelectionMode = ListViewSelectionMode.Multiple;

                SelectorSeparator.Visibility = Visibility.Visible;
                AddSelectedToPlayingListButton.Visibility = Visibility.Visible;
                AddSelectedToPlayListButton.Visibility = Visibility.Visible;
                DeleteSelectedButton.Visibility = Visibility.Visible;
                DownloadSelectedButton.Visibility = Visibility.Visible;
                SelectReverseButton.Visibility = Visibility.Visible;
                SelectAllButton.Visibility = Visibility.Visible;

                Children.AllowDrop = true;
                Children.CanReorderItems = true;
            }
            else
            {
                SelectItemButton.Background = new SolidColorBrush(Colors.Transparent);
                Children.SelectionMode = ListViewSelectionMode.None;

                SelectorSeparator.Visibility = Visibility.Collapsed;
                AddSelectedToPlayingListButton.Visibility = Visibility.Collapsed;
                AddSelectedToPlayListButton.Visibility = Visibility.Collapsed;
                DeleteSelectedButton.Visibility = Visibility.Collapsed;
                DownloadSelectedButton.Visibility = Visibility.Collapsed;
                SelectReverseButton.Visibility = Visibility.Collapsed;
                SelectAllButton.Visibility = Visibility.Collapsed;

                Children.AllowDrop = false;
                Children.CanReorderItems = false;
            }
            UpdateCommandToolBarWidth();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            pageNumber = 1;
            InitData();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (pageNumber - 1 > 0)
            {
                pageNumber--;
                InitData();
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            pageNumber++;
            InitData();
        }

        private void AddSelectedToPlayingListButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                foreach (SongItemBindBase item in Children.SelectedItems)
                {
                    App.playingList.Add(item.MusicData);
                }
            }
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                foreach (SongItemBindBase item in Children.SelectedItems) 
                {
                    MusicDataList.Remove(item);
                }

            }
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            Children.SelectAll();
        }

        private void SelectReverseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SongItemBindBase item in MusicDataList)
            {
                try
                {
                    var a = Children.ContainerFromIndex(Children.Items.IndexOf(item)) as ListViewItem;
                    if (a!=null)
                        a.IsSelected = !a.IsSelected;
                }
                catch { }
            }
        }

        private void AppBarButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void DownloadSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                foreach (SongItemBindBase songItem in Children.SelectedItems)
                {
                    App.downloadManager.Add(songItem.MusicData);
                }
            }
        }

        private async void AddToPlayListFlyout_Opened(object sender, object e)
        {
            AddToPlayListFlyout.Items.Clear();
            foreach (var m in await PlayListHelper.ReadAllPlayList())
            {
                var a = new MenuFlyoutItem()
                {
                    Text = m.ListShowName,
                    Tag = m
                };
                a.Click += A_Click;

                AddToPlayListFlyout.Items.Add(a);
            }
        }

        private async void A_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoadingDialog();
            var text = await PlayListHelper.ReadData();
            foreach (SongItemBindBase item in Children.SelectedItems)
            {
                MainWindow.SetLoadingText($"正在添加：{item.MusicData.Title} - {item.MusicData.ButtonName}");
                
                text = PlayListHelper.AddMusicDataToPlayList(
                    ((sender as MenuFlyoutItem).Tag as MusicListData).ListName,
                    item.MusicData, text);
            }
            await PlayListHelper.SaveData(text);
            MainWindow.HideDialog();
        }

        private void AddToPlayListFlyout_Closed(object sender, object e)
        {
            //AddToPlayListFlyout.Items.Clear();
        }
        private async void Button_Click_6(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Tag)
            {
                case "0":
                    scrollViewer.ChangeView(null, 0, null);
                    break;
                case "1":
                    scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
                    break;
                case "2":
                    foreach (var i in MusicDataList)
                    {
                        if (i.MusicData == App.audioPlayer.MusicData)
                        {
                            await Children.SmoothScrollIntoViewWithItemAsync(i, ScrollItemPlacement.Center);
                            await Children.SmoothScrollIntoViewWithItemAsync(i, ScrollItemPlacement.Center, true);
                            foreach (var j in SongItem.StaticSongItems)
                            {
                                if (j != null)
                                    if (j.MusicData == App.audioPlayer.MusicData)
                                        j.AnimateMouseLeavingBackground(true);
                            }
                        }
                    }
                    break;
            }
        }
    }
}
