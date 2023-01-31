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

namespace znMusicPlayerWUI.Pages
{
    public partial class ItemListViewPlayList : Page
    {
        private ScrollViewer scrollViewer { get; set; }
        public MusicListData NavToObj { get; set; }

        public ItemListViewPlayList()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlayAllButton.Foreground = new SolidColorBrush(CodeHelper.IsAccentColorDark() ? Colors.White : Colors.Black);
            var a = (MusicListData)e.Parameter;
            NavToObj = a;
            InitData();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Children.SelectionMode != ListViewSelectionMode.None)
            {
                Button_Click_2(null, null);
            }
            await Task.Delay(500);
            scrollViewer?.ScrollToVerticalOffset(0);
            foreach (SongItem item in Children.Items)
            {
                item.Dispose();
            }
            Children.Items.Clear();
            dropShadow?.Dispose();
            PlayList_Image.Dispose();
            searchMusicDatas.Clear();
            searchMusicDatas = null;
            //System.Diagnostics.Debug.WriteLine("Clear");
        }

        private void CreatShadow()
        {
            var visual = ElementCompositionPreview.GetElementVisual(PlayList_Image);
            compositor = visual.Compositor;

            var basicRectVisual = compositor.CreateSpriteVisual();
            basicRectVisual.Size = PlayList_Image.RenderSize.ToVector2();

            dropShadow = compositor.CreateDropShadow();
            dropShadow.BlurRadius = 30f;
            dropShadow.Opacity = 0.3f;
            dropShadow.Offset = new Vector3(0, 4, 0);

            basicRectVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(PlayList_Image_DropShadowBase, basicRectVisual);
        }

        static bool firstInit = false;
        public async void InitData()
        {
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
            PlayList_TitleTextBlock.Text = NavToObj.ListShowName;
            PlayList_OtherTextBlock.Text = $"共{NavToObj.Songs.Count}首歌曲";
            
            if (NavToObj != null)
            {
                LoadImage();
                Children.Items.Clear();
                LoadingRing.Visibility = Visibility.Visible;
                LoadingRing.IsIndeterminate = true;
                /*
                var h = (Children.Header as Border).ActualHeight;
                LoadingRing.Margin = new(0, h + (ActualHeight - h) / 2, 0, 0);
                */
                await Task.Delay(500);
                var dpi = CodeHelper.GetScaleAdjustment(App.WindowLocal);
                MusicData[] array = null;

                SortComboBox.SelectedIndex = (int)NavToObj.PlaySort;
                switch (SortComboBox.SelectedIndex)
                {
                    case 0: //默认
                        array = NavToObj.Songs.ToArray();
                        break;
                    case 1: //名称升序
                        array = NavToObj.Songs.OrderBy(m => m.Title).ToArray();
                        break;
                    case 2: //名称降序
                        array = NavToObj.Songs.OrderByDescending(m => m.Title).ToArray();
                        break;
                    case 3: //艺术家升序
                        array = NavToObj.Songs.OrderBy(m => m.Artists[0].Name).ToArray();
                        break;
                    case 4: //艺术家降序
                        array = NavToObj.Songs.OrderByDescending(m => m.Artists[0].Name).ToArray();
                        break;
                    case 5: //专辑升序
                        array = NavToObj.Songs.OrderBy(m => m.Album).ToArray();
                        break;
                    case 6: //专辑降序
                        array = NavToObj.Songs.OrderByDescending(m => m.Album).ToArray();
                        break;
                    case 7: //时间升序
                        array = NavToObj.Songs.OrderBy(m => m.RelaseTime).ToArray();
                        break;
                    case 8: //时间降序
                        array = NavToObj.Songs.OrderByDescending(m => m.RelaseTime).ToArray();
                        break;
                }

                foreach (var i in array)
                {
                    var a = new Controls.SongItem(i, NavToObj) { ImageScaleDPI = dpi };
                    Children.Items.Add(a);
                }
                System.Diagnostics.Debug.WriteLine("加载完成。");
                LoadingRing.IsIndeterminate = false;
                LoadingRing.Visibility = Visibility.Collapsed;
            }

            if (firstInit)
            {
                firstInit = false;
                await Task.Delay(1000);
                Button_Click_2(null, null);
            }
        }

        private async void LoadImage()
        {
            if (NavToObj.ListDataType == DataType.本地歌单)
            {
                PlayList_Image.Source = await FileHelper.GetImageSource(NavToObj.PicturePath);
            }
            else if (NavToObj.ListDataType == DataType.歌单)
            {
                PlayList_Image.Source = await FileHelper.GetImageSource(await ImageManage.GetImageSource(NavToObj));
            }
        }

        CompositionPropertySet scrollerPropertySet;
        Compositor compositor;
        Visual headerVisual;
        Visual backgroundVisual;
        Visual logoVisual;
        Visual stackVisual;
        public void UpdataShyHeader()
        {
            if (scrollViewer == null) return;

            double anotherHeight = 158;
            String progress = $"Clamp(-scroller.Translation.Y / {anotherHeight}, 0, 1.0)";

            if (scrollerPropertySet == null)
            {
                scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
                compositor = scrollerPropertySet.Compositor;
                headerVisual = ElementCompositionPreview.GetElementVisual(menu_border);
                backgroundVisual = ElementCompositionPreview.GetElementVisual(BackColorBaseRectangle);
                logoVisual = ElementCompositionPreview.GetElementVisual(PlayList_Image_BaseGrid);
                stackVisual = ElementCompositionPreview.GetElementVisual(InfosBaseStackPanel);
            }

            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {anotherHeight}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            var backgroundVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            backgroundVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            backgroundVisual.StartAnimation("Opacity", backgroundVisualOpacityAnimation);

            // Logo scale and transform                                          from               to
            var logoHeaderScaleAnimation = compositor.CreateExpressionAnimation("Lerp(Vector2(1,1), Vector2(0.5, 0.5), " + progress + ")");
            logoHeaderScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Scale.xy", logoHeaderScaleAnimation);

            var logoVisualOffsetXAnimation = compositor.CreateExpressionAnimation($"Lerp(24, 24, {progress})");
            logoVisualOffsetXAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Offset.X", logoVisualOffsetXAnimation);

            var logoVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp(24, {anotherHeight} + 8, {progress})");
            logoVisualOffsetYAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Offset.Y", logoVisualOffsetYAnimation);

            var stackVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(246,24,0), Vector3(140,{anotherHeight} + 8,0), {progress})");
            stackVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            stackVisual.StartAnimation(nameof(stackVisual.Offset), stackVisualOffsetAnimation);
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

        private void menu_border_Loaded(object sender, RoutedEventArgs e)
        {
            if (scrollViewer == null)
            {
                scrollViewer = (VisualTreeHelper.GetChild(Children, 0) as Border).Child as ScrollViewer;
                scrollViewer.CanContentRenderOutsideBounds = true;

                // 设置header为顶层
                var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)Children.Header);
                var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
                Canvas.SetZIndex(headerContainer, 1);
            }

            UpdataShyHeader();
            CreatShadow();
            UpdataCommandToolBarWidth();
        }

        private void Result_BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdataShyHeader();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Children.Items.Any()) return;
            foreach (SongItem songItem in Children.Items)
            {
                App.playingList.Add(songItem.MusicData, false);
            }
            await App.playingList.Play((Children.Items.First() as SongItem).MusicData);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InitData();
        }

        DropShadow dropShadow;
        private async void Button_Click_2(object sender, RoutedEventArgs e)
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

                foreach (SongItem songItem in Children.Items)
                {
                    songItem.CanClickPlay = false;
                }
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

                foreach (SongItem songItem in Children.Items)
                {
                    songItem.CanClickPlay = true;
                }
            }
            UpdataCommandToolBarWidth();
        }

        private void AddSelectedToPlayingListButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                foreach (SongItem item in Children.SelectedItems)
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
                    var jdata = JObject.Parse(await PlayListHelper.ReadData());
                    MainWindow.ShowLoadingDialog("正在移除");
                    int num = 0;
                    foreach (SongItem item in Children.SelectedItems)
                    {
                        num++;
                        MainWindow.SetLoadingText($"正在移除：{item.MusicData.Title} - {item.MusicData.ButtonName}");
                        MainWindow.SetLoadingProgressRingValue(Children.SelectedItems.Count, num);
                        jdata = PlayListHelper.DeleteMusicDataFromPlayList(NavToObj.ListName, item.MusicData, jdata);
                    }
                    await PlayListHelper.SaveData(jdata.ToString());
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
            foreach (SongItem item in Children.Items)
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
                    var jdata = JObject.Parse(await PlayListHelper.ReadData());
                    MainWindow.HideDialog();
                    MainWindow.ShowLoadingDialog("正在添加");
                    int num = 0;
                    foreach (var i in files)
                    {
                        num++;
                        MainWindow.SetLoadingProgressRingValue(files.Count, num);
                        MainWindow.SetLoadingText($"正在添加：{i.Name}");

                        FileInfo fi = null;
                        await Task.Run(() => fi = new FileInfo(i.Path));
                        jdata = await PlayListHelper.AddLocalMusicDataToPlayList(NavToObj.ListName, fi, jdata);
                    }
                    await PlayListHelper.SaveData(jdata.ToString());
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
            };
            bb.Click += async (_, __) =>
            {
                Windows.Storage.StorageFolder folder = await FileHelper.UserSelectFolder(PickerLocationId.MusicLibrary);
                if (folder != null)
                {
                    var jdata = JObject.Parse(await PlayListHelper.ReadData());
                    MainWindow.HideDialog();
                    MainWindow.ShowLoadingDialog("正在添加");
                    DirectoryInfo directory = null;
                    await Task.Run(() => directory = Directory.CreateDirectory(folder.Path));
                    int num = 0;
                    foreach (var i in directory.GetFiles())
                    {
                        if (App.SupportedMediaFormats.Contains(i.Extension))
                        {
                            num++;
                            MainWindow.SetLoadingProgressRingValue(directory.GetFiles().Length, num);
                            MainWindow.SetLoadingText($"正在添加：{i.Name}");
                            jdata = await PlayListHelper.AddLocalMusicDataToPlayList(NavToObj.ListName, i, jdata);
                        }
                    }
                    await PlayListHelper.SaveData(jdata.ToString());
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
            };

            stackPanel.Children.Add(ab);
            stackPanel.Children.Add(bb);

            await MainWindow.ShowDialog("添加本地文件", stackPanel);
        }

        private void DownloadSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                foreach (Controls.SongItem songItem in Children.Items)
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
            var text = JObject.Parse(await PlayListHelper.ReadData());
            foreach (Controls.SongItem item in Children.SelectedItems)
            {
                MainWindow.SetLoadingText($"正在添加：{item.MusicData.Title} - {item.MusicData.ButtonName}");

                text = PlayListHelper.AddMusicDataToPlayList(
                    ((sender as MenuFlyoutItem).Tag as MusicListData).ListName,
                    item.MusicData, text);
            }
            await PlayListHelper.SaveData(text.ToString());
            MainWindow.HideDialog();
        }

        private void AddToPlayListFlyout_Closed(object sender, object e)
        {
            //AddToPlayListFlyout.Items.Clear();
        }

        bool isfirst = true;
        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isfirst)
            {
                isfirst = false;
                return;
            }

            NavToObj.PlaySort = (PlaySort)Enum.Parse(typeof(PlaySort), SortComboBox.SelectedItem as string);
            InitData();
            var data = JObject.Parse(await PlayListHelper.ReadData());
            data[NavToObj.ListName] = JObject.FromObject(NavToObj);
            await PlayListHelper.SaveData(data.ToString());
        }

        List<SongItem> searchMusicDatas = new();
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
                foreach (SongItem i in Children.Items)
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
                item.AnimateMouseLeavingBackground(true);
                await Children.SmoothScrollIntoViewWithItemAsync(item, ScrollItemPlacement.Center);
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            isQuery = false;
        }
    }
}