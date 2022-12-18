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

namespace znMusicPlayerWUI.Controls
{
    public partial class ItemListView : Page
    {
        private ScrollViewer scrollViewer { get; set; }
        public object NavToObj { get; set; }
        public DataType NowShowMode { get; set; }
        public SearchDataType NowSearchMode { get; set; } = SearchDataType.歌曲;
        public MusicFrom NowMusicFrom { get; set; } = MusicFrom.neteaseMusic;

        public ItemListView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var a = (List<object>)e.Parameter;
            NowShowMode = (DataType)a[0];
            NavToObj = a[1];
            if (a.Count >= 3)
                NowMusicFrom = (MusicFrom)a[2];
            if (a.Count >= 4)
                NowSearchMode = (SearchDataType)a[3];
            InitData();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            foreach (SongItem item in Children.Items)
            {
                item.Dispose();
            }
            Children.Items.Clear();
            dropShadow.Dispose();
            await Task.Delay(500);
            PlayList_Image.Dispose();
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
            SearchHomeButton.Visibility = Visibility.Collapsed;
            SearchPageSelectorSeparator.Visibility = Visibility.Collapsed;
            AddLocalFilesButton.Visibility = Visibility.Collapsed;

            SearchPageSelector.Visibility = Visibility.Collapsed;
            SearchPageSelectorCustom.Visibility = Visibility.Collapsed;

            SearchResult_BaseGrid.Visibility = Visibility.Collapsed;
            PlayList_BaseGrid.Visibility = Visibility.Collapsed;
            switch (NowShowMode)
            {
                case DataType.歌曲:
                    SearchResult_BaseGrid.Visibility = Visibility.Visible;

                    SearchPageSelector.Visibility = Visibility.Visible;
                    SearchPageSelectorCustom.Visibility = Visibility.Visible;
                    SearchHomeButton.Visibility = Visibility.Visible;
                    var searchData = NavToObj as string;
                    Result_Search_Header.Text = $"\"{searchData}\"的搜索结果";
                    NowPage.Text = pageNumber.ToString();
                    
                    ToolsCommandBar.Margin = new Thickness(0);
                    Children.Items.Clear();
                    try
                    {
                        musicListData = await WebHelper.SearchData(searchData, pageNumber, pageSize, NowMusicFrom, NowSearchMode);
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        await MainWindow.ShowDialog("不支持的平台", "当前不支持此平台搜索。");
                    }
                    catch (Exception e)
                    {
                        await MainWindow.ShowDialog("搜索失败", e.Message);
                        musicListData = null;
                    }
                    break;

                case DataType.歌单:
                    ToolsCommandBar.Margin = new Thickness(126, 0, 0, 0);
                    PlayList_BaseGrid.Visibility = Visibility.Visible;
                    AddLocalFilesButton.Visibility = Visibility.Visible;
                    musicListData = NavToObj as MusicListData;
                    PlayList_TitleTextBlock.Text = musicListData.ListShowName;
                    PlayList_OtherTextBlock.Text = $"共{musicListData.Songs.Count}首歌曲";
                    break;
            }

            if (musicListData != null)
            {
                Children.Items.Clear();
                foreach (var i in musicListData.Songs)
                {
                    var a = new SongItem(i, musicListData);
                    Children.Items.Add(a);
                }

                if (musicListData.ListDataType == DataType.本地歌单)
                {
                    PlayList_Image.Source = await FileHelper.GetImageSource(musicListData.PicturePath);
                }
                else if (musicListData.ListDataType == DataType.歌单)
                {
                    PlayList_Image.Source = await FileHelper.GetImageSource(await ImageManage.GetImageSource(musicListData));
                }
            }

            if (firstInit)
            {
                firstInit = false;
                await Task.Delay(1000);
                Button_Click_2(null, null);
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

            double anotherHeight = HeaderBaseGrid.ActualHeight;
            if (NowShowMode == DataType.歌单) anotherHeight = 130;
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

            if (NowShowMode == DataType.歌单)
            {
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
                (sender as Button).Background = App.Current.Resources["AccentAAFillColorTertiaryBrush"] as Brush;
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
                (sender as Button).Background = new SolidColorBrush(Colors.Transparent);
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

            ToolsCommandBar.Width = 0;
            await Task.Delay(1);
            ToolsCommandBar.Width = double.NaN;
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

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            if (PageNumberTextBox.Text != String.Empty)
                pageNumber = int.Parse(PageNumberTextBox.Text);
            else pageNumber = 1;

            if (PageSizeTextBox.Text != String.Empty)
                pageSize = int.Parse(PageSizeTextBox.Text);
            else pageSize = 30;

            InitData();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            SearchPageSelectorCustomFlyout.Hide();
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

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                if (NowShowMode == DataType.歌单)
                {

                }
                else
                {
                    List<SongItem> a = new List<SongItem>();
                    foreach (SongItem item in Children.SelectedItems) a.Add(item);
                    foreach (var b in a)
                    {
                        Children.Items.Remove(b);
                    }
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
                    if (a!=null)
                        a.IsSelected = !a.IsSelected;
                }
                catch { }
            }
        }

        private async void MenuFlyout_Opening(object sender, object e)
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
            foreach (SongItem item in Children.SelectedItems)
            {
                await PlayListHelper.AddMusicDataToPlayList(
                    ((sender as MenuFlyoutItem).Tag as MusicListData).ListName,
                    item.MusicData);
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
            var bb = new Button() { Content = "单个文件夹", HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new(0,4,0,0) };

            ab.Click += async (_, __) =>
            {
                var files = await FileHelper.UserSelectFiles(
                    PickerViewMode.List, PickerLocationId.MusicLibrary,
                    App.SupportedMediaFormats);
                if (files.Any())
                {
                    foreach (var i in files)
                    {
                        await PlayListHelper.AddLocalMusicDataToPlayList(musicListData.ListName, new FileInfo(i.Path));
                    }
                }
            };
            bb.Click += async (_, __) =>
            {
                Windows.Storage.StorageFolder folder = await FileHelper.UserSelectFolder(PickerLocationId.MusicLibrary);
                if (folder != null)
                {
                    DirectoryInfo directory = Directory.CreateDirectory(folder.Path);
                    foreach (var i in directory.GetFiles())
                    {
                        if (App.SupportedMediaFormats.Contains(i.Extension))
                        {
                            await PlayListHelper.AddLocalMusicDataToPlayList(musicListData.ListName, i);
                        }
                    }
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
                foreach (SongItem songItem in Children.Items)
                {
                    App.downloadManager.Add(songItem.MusicData);
                }
            }
        }
    }
}
