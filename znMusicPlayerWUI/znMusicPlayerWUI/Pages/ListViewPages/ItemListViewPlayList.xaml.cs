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
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using znMusicPlayerWUI.Pages;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Storage.Pickers;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Media;
using Newtonsoft.Json.Linq;
using CommunityToolkit.WinUI.UI;
using znMusicPlayerWUI.Controls;
using System.Collections.ObjectModel;
using System.Data;
using Vanara.Extensions;

namespace znMusicPlayerWUI.Pages
{
    public partial class ItemListViewPlayList : Page
    {
        private ScrollViewer scrollViewer { get; set; }
        public MusicListData NavToObj { get; set; }

        public ItemListViewPlayList()
        {
            InitializeComponent();
            DataContext = this;
            var _enumval = Enum.GetValues(typeof(PlaySort)).Cast<PlaySort>();
            SortComboBox.ItemsSource = _enumval.ToList();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ConnectedAnimation animation =
                ConnectedAnimationService.GetForCurrentView().GetAnimation("forwardAnimation");
            if (animation != null)
            {
                animation.TryStart(PlayList_ImageBaseBorder);
            }

            PlayAllButton.Foreground = new SolidColorBrush(CodeHelper.IsAccentColorDark() ? Colors.White : Colors.Black);
            App.playListReader.Updataed += PlayListReader_Updataed;
            App.audioPlayer.SourceChanged += AudioPlayer_SourceChanged;

            NavToObj = (MusicListData)e.Parameter;
            InitData();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.audioPlayer.SourceChanged -= AudioPlayer_SourceChanged;
            App.playListReader.Updataed -= PlayListReader_Updataed;

            if (Children.SelectionMode != ListViewSelectionMode.None)
            {
                Button_Click_2(null, null);
            }
            await Task.Delay(500);

            foreach (var i in MusicDataList)
            {
                i.Dispose();
            }
            foreach (var i in searchMusicDatas)
            {
                i.Dispose();
            }
            MusicDataList.Clear();

            Children.ItemsSource = null; //😡GC2代频繁回收的罪魁祸首😡😡
            Children.Items.Clear();

            searchMusicDatas.Clear();
            dropShadow?.Dispose();
            PlayList_Image.Dispose();
            PlayList_Image.Dispose();
            NavToObj = null;
            UnloadObject(this);
            //GC.SuppressFinalize(this);
            //System.Diagnostics.Debug.WriteLine("Clear");
        }

        private void CreatShadow()
        {
            if (logoVisual == null) return;
            compositor = logoVisual.Compositor;

            var basicRectVisual = compositor.CreateSpriteVisual();
            basicRectVisual.Size = PlayList_Image.RenderSize.ToVector2();

            dropShadow = compositor.CreateDropShadow();
            dropShadow.BlurRadius = 30f;
            dropShadow.Opacity = 0.3f;
            dropShadow.Offset = new Vector3(0, 4, 0);

            basicRectVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(PlayList_Image_DropShadowBase, basicRectVisual);
        }

        bool isLoading = false;
        public ObservableCollection<SongItemBindBase> MusicDataList = new();
        public async void InitData()
        {
            if (isLoading) return;
            isLoading = true;

            #region Collecter
            SelectorSeparator.Visibility = Visibility.Collapsed;
            AddSelectedToPlayingListButton.Visibility = Visibility.Collapsed;
            AddSelectedToPlayListButton.Visibility = Visibility.Collapsed;
            DeleteSelectedButton.Visibility = Visibility.Collapsed;
            DownloadSelectedButton.Visibility = Visibility.Collapsed;
            SelectReverseButton.Visibility = Visibility.Collapsed;
            SelectAllButton.Visibility = Visibility.Collapsed;
            AddLocalFilesButton.Visibility = Visibility.Collapsed;

            PlayList_BaseGrid.Visibility = Visibility.Visible;
            AddLocalFilesButton.Visibility = Visibility.Visible;
            #endregion
            PlayList_TitleTextBlock.Text = NavToObj.ListShowName;
            PlayList_OtherTextBlock.Text = $"共{NavToObj.Songs.Count}首歌曲";

            LoadingRing.Visibility = Visibility.Visible;
            LoadingRing.IsIndeterminate = true;

            if (NavToObj != null)
            {
                LoadImage();

                MusicDataList.Clear();
                var dpi = CodeHelper.GetScaleAdjustment(App.WindowLocal);
                MusicData[] array = null;

                SortComboBox.SelectedItem = NavToObj.PlaySort;
                switch ((PlaySort)SortComboBox.SelectedItem)
                {
                    case PlaySort.默认升序:
                        array = NavToObj.Songs.ToArray();
                        break;
                    case PlaySort.默认降序:
                        List<MusicData> list = new();
                        foreach (var d in NavToObj.Songs) list.Add(d);
                        list.Reverse();
                        array = list.ToArray();
                        break;
                    case PlaySort.名称升序:
                        array = NavToObj.Songs.OrderBy(m => m.Title).ToArray();
                        break;
                    case PlaySort.名称降序:
                        array = NavToObj.Songs.OrderByDescending(m => m.Title).ToArray();
                        break;
                    case PlaySort.艺术家升序:
                        array = NavToObj.Songs.OrderBy(m => m.Artists[0].Name).ToArray();
                        break;
                    case PlaySort.艺术家降序:
                        array = NavToObj.Songs.OrderByDescending(m => m.Artists[0].Name).ToArray();
                        break;
                    case PlaySort.专辑升序:
                        array = NavToObj.Songs.OrderBy(m => m.Album).ToArray();
                        break;
                    case PlaySort.专辑降序:
                        array = NavToObj.Songs.OrderByDescending(m => m.Album).ToArray();
                        break;
                    case PlaySort.时间升序:
                        array = NavToObj.Songs.OrderBy(m => m.RelaseTime).ToArray();
                        break;
                    case PlaySort.时间降序:
                        array = NavToObj.Songs.OrderByDescending(m => m.RelaseTime).ToArray();
                        break;
                    case PlaySort.索引升序:
                        array = NavToObj.Songs.OrderBy(m => m.Index).ToArray();
                        break;
                    case PlaySort.索引降序:
                        array = NavToObj.Songs.OrderByDescending(m => m.Index).ToArray();
                        break;
                }

                int count = 0;
                foreach (var i in array)
                {
                    if (i == null) continue;
                    count++;
                    i.Count = count;
                    MusicDataList.Add(new() { MusicData = i, MusicListData = NavToObj, ImageScaleDPI = dpi });
                }
                array = null;
                System.Diagnostics.Debug.WriteLine("加载完成。");
            }
            LoadingRing.IsIndeterminate = false; isLoading = false;
            LoadingRing.Visibility = Visibility.Collapsed;
        }

        private void PlayListReader_Updataed()
        {
            foreach (var data in App.playListReader.NowMusicListDatas)
            {
                if (data == NavToObj)
                {
                    NavToObj = data;
                    InitData();
                    break;
                }
            }
        }

        private void AudioPlayer_SourceChanged(AudioPlayer audioPlayer)
        {

        }

        private async void LoadImage()
        {
            if (NavToObj.ListDataType == DataType.本地歌单)
            {
                bool isExists = true;
                await Task.Run(() => { isExists = File.Exists(NavToObj.PicturePath); });
                if (isExists) PlayList_Image.Source = await FileHelper.GetImageSource(NavToObj.PicturePath);
            }
            else if (NavToObj.ListDataType == DataType.歌单)
            {
                PlayList_Image.Source = await FileHelper.GetImageSource(await ImageManage.GetImageSource(NavToObj));
            }
            System.Diagnostics.Debug.WriteLine("图片加载完成。");
        }

        CompositionPropertySet scrollerPropertySet;
        Compositor compositor;
        Visual headerVisual;
        Visual backgroundVisual;
        Visual logoVisual;
        Visual stackVisual;
        Visual commandBarVisual;
        ExpressionAnimation offsetExpression;
        ExpressionAnimation backgroundVisualOpacityAnimation;
        ExpressionAnimation logoHeaderScaleAnimation;
        ExpressionAnimation logoVisualOffsetXAnimation;
        ExpressionAnimation logoVisualOffsetYAnimation;
        ExpressionAnimation stackVisualOffsetAnimation;
        ExpressionAnimation commandBarVisualOffsetAnimation;
        int logoSizeCount = 0;
        public void UpdataShyHeader(bool xOnly = false)
        {
            if (scrollViewer == null) return;

            int logoHeight = 280;
            double anotherHeight = 154;
            int anotherXEnd = 151;
            double logoSizeEnd = 0.45;
            int commandYEnd = 84;

            if (logoSizeCount == 1)
            {
                logoHeight = 160;
                logoSizeEnd = 0.66;
                anotherHeight = 54;
                anotherXEnd = 131;
                commandYEnd = 63;
            }
            int anotherX = 24 + logoHeight + 12;

            String progress = $"Clamp(-scroller.Translation.Y / {anotherHeight}, 0, 1.0)";

            if (scrollerPropertySet == null)
            {
                scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
                compositor = scrollerPropertySet.Compositor;
                headerVisual = ElementCompositionPreview.GetElementVisual(menu_border);
                backgroundVisual = ElementCompositionPreview.GetElementVisual(BackColorBaseRectangle);
                logoVisual = ElementCompositionPreview.GetElementVisual(PlayList_Image_BaseGrid);
                stackVisual = ElementCompositionPreview.GetElementVisual(InfosBaseStackPanel);
                commandBarVisual = ElementCompositionPreview.GetElementVisual(CommandBarWidthChanger);
            }
            else
            {
                if (!xOnly)
                {
                    offsetExpression.Dispose();
                    backgroundVisualOpacityAnimation.Dispose();
                    logoVisualOffsetXAnimation.Dispose();
                    logoVisualOffsetYAnimation.Dispose();
                    stackVisualOffsetAnimation.Dispose();
                    commandBarVisualOffsetAnimation.Dispose();
                    offsetExpression = null;
                    backgroundVisualOpacityAnimation = null;
                    logoVisualOffsetXAnimation = null;
                    logoVisualOffsetYAnimation = null;
                    logoVisualOffsetYAnimation = null;
                    stackVisualOffsetAnimation = null;
                    commandBarVisualOffsetAnimation = null;
                }
                logoHeaderScaleAnimation.Dispose();
                logoHeaderScaleAnimation = null;
            }

            if (!xOnly)
            {
                offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {anotherHeight}");
                offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
                headerVisual.StartAnimation("Offset.Y", offsetExpression);

                backgroundVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
                backgroundVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
                backgroundVisual.StartAnimation("Opacity", backgroundVisualOpacityAnimation);

                logoVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp(24, {anotherHeight} + 12, {progress})");
                logoVisualOffsetYAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
                logoVisual.StartAnimation("Offset.Y", logoVisualOffsetYAnimation);

                logoVisualOffsetXAnimation = compositor.CreateExpressionAnimation($"Lerp(24, 12, {progress})");
                logoVisualOffsetXAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
                logoVisual.StartAnimation("Offset.X", logoVisualOffsetXAnimation);

                stackVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3({anotherX},24,0), Vector3({anotherXEnd},{anotherHeight} + 12,0), {progress})");
                stackVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
                stackVisual.StartAnimation(nameof(stackVisual.Offset), stackVisualOffsetAnimation);
            }

            string sizelogo = null;
            if (logoSizeCount == 0) sizelogo = "1,1";
            // Logo scale and transform                                          from               to
            logoHeaderScaleAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector2(1,1), Vector2({logoSizeEnd}, {logoSizeEnd}), " + progress + ")");
            logoHeaderScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Scale.xy", logoHeaderScaleAnimation);

            commandBarVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(-6,{logoHeight - commandBarVisual.Size.Y + 6},0), Vector3(-6,{commandYEnd},0), {progress})");
            commandBarVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            commandBarVisual.StartAnimation(nameof(commandBarVisual.Offset), commandBarVisualOffsetAnimation);
            headerVisual.IsPixelSnappingEnabled = true;
            /*
            Visual textVisual = ElementCompositionPreview.GetElementVisual(Result_Search_Header);
            Vector3 finalOffset = new Vector3(0, (float)Result_Search_Header.ActualHeight, 0);
            var headerOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0,0,0), finalOffset, {progress})");
            headerOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            headerOffsetAnimation.SetVector3Parameter("finalOffset", finalOffset);
            textVisual.StartAnimation(nameof(Visual.Offset), headerOffsetAnimation);*/
        }

        private async void UpdataCommandToolBarWidth()
        {
            ToolsCommandBar.Width = 0;
            await Task.Delay(1);
            ToolsCommandBar.Width = double.NaN;
        }

        private void UpdataInfoWidth()
        {
            if (logoVisual == null) return;
            var width = HeaderBaseGrid.ActualWidth - 50 - (PlayList_ImageBaseBorder.ActualWidth * logoVisual.Scale.X);
            //System.Diagnostics.Debug.WriteLine(width);
            if (width > 0)
            {
                WidthChanger.Width = width;
            }
        }

        private async void menu_border_Loaded(object sender, RoutedEventArgs e)
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

            UpdataCommandToolBarWidth();

            UpdataShyHeader();
            await Task.Delay(1);
            UpdataShyHeader();
            CreatShadow();
        }

        bool isFirstScroll = true;
        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            if (!isFirstScroll) { isFirstScroll = false; return; }
            UpdataShyHeader(true);
            UpdataInfoWidth();
        }

        private async void Result_BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ActualWidth <= 660 || ActualHeight <= 348)
            {
                logoSizeCount = 1;
                PlayList_ImageBaseBorder.Width = 160;
                PlayList_ImageBaseBorder.Height = 160;
            }
            else
            {
                logoSizeCount = 0;
                PlayList_ImageBaseBorder.Width = 280;
                PlayList_ImageBaseBorder.Height = 280;
            }/*
            System.Diagnostics.Debug.WriteLine(ActualWidth);
            System.Diagnostics.Debug.WriteLine(ActualHeight);*/
            UpdataShyHeader();
            UpdataInfoWidth();
            await Task.Delay(1);
            CreatShadow();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var songItem in MusicDataList)
            {
                App.playingList.Add(songItem.MusicData, false);
            }
            await App.playingList.Play(MusicDataList.First().MusicData, true);
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
/*
                foreach (SongItem songItem in Children.Items)
                {
                    songItem.CanClickPlay = false;
                }*/
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
/*
                foreach (SongItem songItem in Children.Items)
                {
                    songItem.CanClickPlay = true;
                }*/
            }
            UpdataCommandToolBarWidth();
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

        private async void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                var result = await MainWindow.ShowDialog("移除歌曲", $"真的要从歌单中移除这{Children.SelectedItems.Count}首歌曲吗？", "取消", "确定");
                if (result == ContentDialogResult.Primary)
                {
                    var jdata = await PlayListHelper.ReadData();
                    MainWindow.ShowLoadingDialog("正在移除");
                    int num = 0;
                    foreach (SongItemBindBase item in Children.SelectedItems)
                    {
                        num++;
                        MainWindow.SetLoadingText($"正在移除：{item.MusicData.Title} - {item.MusicData.ButtonName}");
                        MainWindow.SetLoadingProgressRingValue(Children.SelectedItems.Count, num);
                        jdata = PlayListHelper.DeleteMusicDataFromPlayList(NavToObj.ListName, item.MusicData, jdata);
                    }
                    await PlayListHelper.SaveData(jdata);
                    await App.playListReader.Refresh();
                    foreach (var m in App.playListReader.NowMusicListDatas)
                    {
                        if (m.MD5 == NavToObj.MD5)
                        {
                            NavToObj = m;
                            break;
                        }
                    }
                    MainWindow.HideDialog();
                    InitData();
                }
            }
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            /*
            foreach (SongItem item in Children.Items)
            {
                (Children.ContainerFromIndex(Children.Items.IndexOf(item)) as ListViewItem).IsSelected = true;
            }*/
            Children.SelectAll();
        }

        private void SelectReverseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SongItemBindBase item in Children.Items)
            {
                try
                {
                    var a = Children.ContainerFromIndex(Children.Items.IndexOf(item)) as ListViewItem;
                    if (a != null)
                        a.IsSelected = !a.IsSelected;
                }
                catch { }
            }
        }

        private void AppBarButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void AddLocalFilesButton_Click(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanel = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
            var ab = new Button() { Content = "多个音频文件", HorizontalAlignment = HorizontalAlignment.Stretch };
            var bb = new Button() { Content = "单个文件夹", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new(0, 4, 0, 0) };

            ab.Click += async (_, __) =>
            {
                var files = await FileHelper.UserSelectFiles(
                    PickerViewMode.List, PickerLocationId.MusicLibrary,
                    App.SupportedMediaFormats);
                if (files.Any())
                {
                    var jdata = await PlayListHelper.ReadData();
                    MainWindow.HideDialog();
                    foreach (var i in files)
                    {
                        FileInfo fi = null;
                        await Task.Run(() => fi = new FileInfo(i.Path));
                        jdata = await PlayListHelper.AddLocalMusicDataToPlayList(NavToObj.ListName, fi, jdata);
                    }
                    await PlayListHelper.SaveData(jdata);
                    await App.playListReader.Refresh();
                    foreach (var m in App.playListReader.NowMusicListDatas)
                    {
                        if (m.MD5 == NavToObj.MD5)
                        {
                            NavToObj = m;
                            break;
                        }
                    }
                    InitData();
                    await MainWindow.ShowDialog("添加本地歌曲", "添加完成。");
                }
            };
            bb.Click += async (_, __) =>
            {
                Windows.Storage.StorageFolder folder = await FileHelper.UserSelectFolder(PickerLocationId.MusicLibrary);
                if (folder != null)
                {
                    var jdata = await PlayListHelper.ReadData();
                    DirectoryInfo directory = null;
                    await Task.Run(() => directory = Directory.CreateDirectory(folder.Path));
                    foreach (var i in directory.GetFiles())
                    {
                        if (App.SupportedMediaFormats.Contains(i.Extension))
                        {
                            jdata = await PlayListHelper.AddLocalMusicDataToPlayList(NavToObj.ListName, i, jdata);
                        }
                    }
                    await PlayListHelper.SaveData(jdata);
                    await App.playListReader.Refresh();
                    foreach (var m in App.playListReader.NowMusicListDatas)
                    {
                        if (m.MD5 == NavToObj.MD5)
                        {
                            NavToObj = m;
                            break;
                        }
                    }
                    InitData();
                    await MainWindow.ShowDialog("添加本地歌曲", "添加完成。");
                }
            };

            stackPanel.Children.Add(ab);
            stackPanel.Children.Add(bb);

            await MainWindow.ShowDialog("添加本地文件", stackPanel);
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

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isLoading)
                return;
            if (NavToObj.PlaySort == (PlaySort)SortComboBox.SelectedItem)
                return;

            NavToObj.PlaySort = (PlaySort)SortComboBox.SelectedItem;
            InitData();
            var data = await PlayListHelper.ReadData();
            data[NavToObj.ListName] = JObject.FromObject(NavToObj);
            await PlayListHelper.SaveData(data);
        }

        List<SongItemBindBase> searchMusicDatas = new();
        bool isQuery = false;
        int searchNum = -1;
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(SearchBox.Text)) return;
            if (!isQuery)
            {
                isQuery = true;
                searchNum = -1;
                searchMusicDatas.Clear();
                foreach (var i in MusicDataList)
                {
                    if (i.MusicData.Title.ToLower().Contains(SearchBox.Text.ToLower()))
                    {
                        searchMusicDatas.Add(i);
                    }
                }
            }
            if (searchMusicDatas.Any())
            {
                searchNum++;
                if (searchNum > searchMusicDatas.Count - 1) searchNum = 0;
                var item = searchMusicDatas[searchNum];
                await Children.SmoothScrollIntoViewWithItemAsync(item, ScrollItemPlacement.Center);
                System.Diagnostics.Debug.WriteLine(((VisualTreeHelper.GetChild(Children.ContainerFromItem(item) as UIElement, 0) as ListViewItemPresenter).Content as SongItem));
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            isQuery = false;
        }

        private void Children_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = Children.ContainerFromItem(Children.SelectedItem);
        }

        private void SearchBox_AccessKeyInvoked(UIElement sender, Microsoft.UI.Xaml.Input.AccessKeyInvokedEventArgs args)
        {
            (sender as FrameworkElement).Focus(FocusState.Programmatic);
        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
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
                    foreach(var i in MusicDataList)
                    {
                        if (i.MusicData == App.audioPlayer.MusicData)
                        {
                           await Children.SmoothScrollIntoViewWithItemAsync(i, ScrollItemPlacement.Center);
                        }
                    }
                    break;
            }
        }
    }
}