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
using Microsoft.VisualBasic.Devices;
using System.Collections.ObjectModel;

namespace znMusicPlayerWUI.Pages
{
    public partial class ItemListViewSearch : Page
    {
        private ScrollViewer scrollViewer { get; set; }
        public object NavToObj { get; set; }
        public SearchDataType NowSearchMode { get; set; } = SearchDataType.歌曲;
        public MusicFrom NowMusicFrom { get; set; } = MusicFrom.neteaseMusic;

        public ItemListViewSearch()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlayAllButton.Foreground = new SolidColorBrush(CodeHelper.IsAccentColorDark() ? Colors.White : Colors.Black);
            var a = (List<object>)e.Parameter;
            NavToObj = a[0];
            NowMusicFrom = (MusicFrom)a[1];
            NowSearchMode = (SearchDataType)a[2];
            InitData();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            await Task.Delay(500);
            scrollViewer?.ScrollToVerticalOffset(0);
            MusicDataList.Clear();
            UnloadObject(this);
        }

        public ObservableCollection<SongItemBindBase> MusicDataList = new();
        object searchDatas = null;
        static bool firstInit = false;
        int pageNumber = 1;
        int pageSize = 30;
        public async void InitData()
        {
            LoadingRing.Visibility = Visibility.Visible;
            LoadingRing.IsIndeterminate = true;

            SelectorSeparator.Visibility = Visibility.Collapsed;
            AddSelectedToPlayingListButton.Visibility = Visibility.Collapsed;
            AddSelectedToPlayListButton.Visibility = Visibility.Collapsed;
            DownloadSelectedButton.Visibility = Visibility.Collapsed;
            DeleteSelectedButton.Visibility = Visibility.Collapsed;
            SelectReverseButton.Visibility = Visibility.Collapsed;
            SelectAllButton.Visibility = Visibility.Collapsed;
            SearchHomeButton.Visibility = Visibility.Collapsed;
            SearchPageSelectorSeparator.Visibility = Visibility.Collapsed;

            SearchPageSelector.Visibility = Visibility.Collapsed;
            SearchPageSelectorCustom.Visibility = Visibility.Collapsed;

            SearchResult_BaseGrid.Visibility = Visibility.Visible;
            SearchPageSelector.Visibility = Visibility.Visible;
            SearchPageSelectorCustom.Visibility = Visibility.Visible;
            SearchHomeButton.Visibility = Visibility.Visible;
            var searchData = NavToObj as string;
            Result_Search_Header.Text = $"\"{searchData}\"的搜索结果";
            NowPage.Text = pageNumber.ToString();

            MusicDataList.Clear();

            bool isComplete = false;
            while (!isComplete)
            {
                try
                {
                    searchDatas = await WebHelper.SearchData(searchData, pageNumber, pageSize, NowMusicFrom, NowSearchMode);
                    isComplete = true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    await MainWindow.ShowDialog("不支持的平台", "当前不支持此平台搜索。");
                    searchDatas = null;
                    break;
                }
                catch (NullReferenceException)
                {
                    await MainWindow.ShowDialog("搜索失败", "无相关结果。");
                    searchDatas = null;
                    break;
                }
                catch (Exception ex)
                {
                    string errString = $"搜索时出现错误：\n{ex.Message}";
                    var d = await MainWindow.ShowDialog("搜索失败", errString, "重试", "确定");
                    if (d == ContentDialogResult.Primary)
                    {
                        searchDatas = null;
                        break;
                    }
                }
            }

            if (searchDatas != null)
            {
                MusicDataList.Clear();

                var dpi = CodeHelper.GetScaleAdjustment(App.WindowLocal);

                switch (NowSearchMode)
                {
                    case SearchDataType.歌曲:
                        MusicData[] array = (searchDatas as MusicListData).Songs.ToArray();
                        foreach (var i in array)
                        {
                            MusicDataList.Add(new() { MusicData = i, ImageScaleDPI = dpi });
                        }
                        ItemPresenterControlBridge.Margin = new(14, 0, 16, 0);
                        break;
                    case SearchDataType.艺术家:
                        /*foreach (var i in searchDatas as List<Artist>)
                        {
                            var a = new Controls.ArtistCard() { Artist = i, ImageScaleDPI = dpi };
                            Children.Items.Add(a);
                        }
                        ItemPresenterControlBridge.Margin = new(14, 14, 16, 14);*/
                        break;
                }
            }

            System.Diagnostics.Debug.WriteLine("加载完成。");
            LoadingRing.IsIndeterminate = false;
            LoadingRing.Visibility = Visibility.Collapsed;
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
            String progress = $"Clamp(-scroller.Translation.Y / {anotherHeight}, 0, 1.0)";

            if (scrollerPropertySet == null)
            {
                scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
                compositor = scrollerPropertySet.Compositor;
                headerVisual = ElementCompositionPreview.GetElementVisual(menu_border);
                backgroundVisual = ElementCompositionPreview.GetElementVisual(BackColorBaseRectangle);
            }

            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {anotherHeight}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            var backgroundVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            backgroundVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            backgroundVisual.StartAnimation("Opacity", backgroundVisualOpacityAnimation);
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
            UpdataCommandToolBarWidth();
        }

        private void Result_BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdataShyHeader();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!MusicDataList.Any()) return;
            foreach (SongItemBindBase songItem in MusicDataList)
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
            UpdataCommandToolBarWidth();
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
                foreach (SongItemBindBase songItem in Children.Items)
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
            foreach (SongItemBindBase item in Children.SelectedItems)
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
    }
}
