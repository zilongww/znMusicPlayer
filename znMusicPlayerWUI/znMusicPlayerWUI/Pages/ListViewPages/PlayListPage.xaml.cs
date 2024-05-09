using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Composition;
using znMusicPlayerWUI.Media;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.DataEditor;
using Windows.Graphics.Display;
using Newtonsoft.Json.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace znMusicPlayerWUI.Pages.ListViewPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayListPage : Page
    {
        MusicListData musicListData { get; set; } = null;
        ObservableCollection<SongItemBindBase> musicListBind { get; set; } = new();
        ScrollViewer scrollViewer;
        string md5;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            md5 = (string)e.Parameter;
            //InitShyHeader();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            musicListData = null;
            MainWindow.WindowDpiChanged -= MainWindow_WindowDpiChanged;
        }

        public PlayListPage()
        {
            InitializeComponent();
        }


        CompositionPropertySet scrollerPropertySet;
        Compositor compositor;
        Visual scrollVisual;
        Visual headerVisual;
        Visual backgroundVisual;
        Visual imageVisual;
        Visual infoVisual;
        Visual commandBarVisual;
        Visual headerFootRootVisual;
        Visual searchRootVisual;
        private void InitVisuals()
        {
            if (isUnloaded) return;

            // 设置 header 为顶层
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)ItemsList.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex(headerContainer, 1);

            scrollViewer = (VisualTreeHelper.GetChild(ItemsList, 0) as Border).Child as ScrollViewer;
            scrollViewer.CanContentRenderOutsideBounds = true;
            scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

            compositor = scrollerPropertySet.Compositor;
            scrollVisual = ElementCompositionPreview.GetElementVisual(scrollViewer);
            headerVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Root);
            backgroundVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_ImageInfo_BackgroundFill);
            imageVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Image_Root);
            infoVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Info_Root);
            commandBarVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Info_CommandBar);
            headerFootRootVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Foot_Root);
            searchRootVisual = ElementCompositionPreview.GetElementVisual(ItemList_Header_Search_Root);
        }

        ExpressionAnimation logoHeaderScaleAnimation;
        ExpressionAnimation offsetExpression;
        ExpressionAnimation backgroundVisualOpacityAnimation;
        ExpressionAnimation imageVisualOffsetAnimation;
        ExpressionAnimation infoVisualOffsetAnimation;
        ExpressionAnimation commandBarVisualOffsetAnimation;
        ExpressionAnimation headerFootRootVisualOffsetAnimation;
        ExpressionAnimation searchRootVisualOffsetAnimation;
        private async void InitShyHeader(bool imageSizeOnly = false, bool delay = false)
        {
            if (scrollViewer == null) return;
            if (compositor == null) return;
            if (isUnloaded) return;
            var anotherHeight = 154;
            double imageSizeEnd = 0.45;
            string progress = $"Clamp(-scroller.Translation.Y / {anotherHeight}, 0, 1.0)";

            logoHeaderScaleAnimation?.Dispose();
            logoHeaderScaleAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector2(1, 1), Vector2({imageSizeEnd}, {imageSizeEnd}), {progress})");
            logoHeaderScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            imageVisual.StartAnimation("Scale.xy", logoHeaderScaleAnimation);
            if (ItemsList_Header_Root.ActualWidth != 0)
                ItemsList_Header_Info_Root_SizeChanger.Width = ItemsList_Header_Root.ActualWidth - ItemsList_Header_Image_Root.ActualWidth * imageVisual.Scale.X - 32 - 16;
            if (imageSizeOnly) return;

            if (delay) await Task.Delay(10);

            offsetExpression?.Dispose();
            offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {anotherHeight}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            backgroundVisualOpacityAnimation?.Dispose();
            backgroundVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            backgroundVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            backgroundVisual.StartAnimation("Opacity", backgroundVisualOpacityAnimation);

            imageVisualOffsetAnimation?.Dispose();
            imageVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector2(0, 0), Vector2(0, {anotherHeight}), {progress})");
            imageVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            imageVisual.StartAnimation("Offset.xy", imageVisualOffsetAnimation);

            infoVisualOffsetAnimation?.Dispose();
            infoVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3({ItemsList_Header_Image_Root.ActualWidth + 16}, 0, 0), Vector3({(int)(ItemsList_Header_Image_Root.ActualWidth * imageSizeEnd) + 16}, {anotherHeight}, 0), {progress})");
            infoVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            infoVisual.StartAnimation(nameof(infoVisual.Offset), infoVisualOffsetAnimation);

            commandBarVisualOffsetAnimation?.Dispose();
            commandBarVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(-6, {imageVisual.Size.Y - commandBarVisual.Size.Y + 6}, 0), Vector3(-6, {imageVisual.Size.Y * imageSizeEnd - commandBarVisual.Size.Y + 6}, 0), {progress})");
            commandBarVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            commandBarVisual.StartAnimation(nameof(infoVisual.Offset), commandBarVisualOffsetAnimation);

            headerFootRootVisualOffsetAnimation?.Dispose();
            headerFootRootVisualOffsetAnimation = compositor.CreateExpressionAnimation(
                $"Lerp(" +
                    $"Vector3(" +
                        $"-16," +
                        $"{ActualHeight} - {headerFootRootVisual.Size.Y} - 8," +
                        $"0)," +
                    $"Vector3(" +
                        $"-16," +
                        $"{anotherHeight} + {ActualHeight} - {headerFootRootVisual.Size.Y} - 8," +
                        $"0)," +
                    $"{progress})");
            headerFootRootVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            headerFootRootVisual.StartAnimation("Offset", headerFootRootVisualOffsetAnimation);

            searchRootVisualOffsetAnimation?.Dispose();
            searchRootVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0, {ItemsList_Header_Root.ActualHeight + 4}, 0), Vector3(0, {ItemsList_Header_Root.ActualHeight + 4}, 0), {progress})");
            searchRootVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            searchRootVisual.StartAnimation(nameof(infoVisual.Offset), searchRootVisualOffsetAnimation);
        }
        void DisposeVisuals()
        {
            logoHeaderScaleAnimation?.Dispose();
            offsetExpression?.Dispose();
            backgroundVisualOpacityAnimation?.Dispose();
            imageVisualOffsetAnimation?.Dispose();
            infoVisualOffsetAnimation?.Dispose();
            commandBarVisualOffsetAnimation?.Dispose();
            headerFootRootVisualOffsetAnimation?.Dispose();

            scrollerPropertySet = null;
            compositor = null;
            headerVisual = null;
            backgroundVisual = null;
            imageVisual = null;
            infoVisual = null;
            commandBarVisual = null;
            headerFootRootVisual = null;
            logoHeaderScaleAnimation = null;
            offsetExpression = null;
            backgroundVisualOpacityAnimation = null;
            imageVisualOffsetAnimation = null;
            infoVisualOffsetAnimation = null;
            commandBarVisualOffsetAnimation = null;
            headerFootRootVisualOffsetAnimation = null;
        }

        bool isInInitBindings = false;
        List<PlaySort> listSortEnum = null;
        private async void InitBindings()
        {
            if (isUnloaded) return;
            if (isInInitBindings) return;
            isInInitBindings = true;
            var scs = musicListData.PlaySort;
            List<MusicData> array = [];
            await Task.Run(() =>
            {
                switch (scs)
                {
                    case PlaySort.默认升序:
                        array = musicListData.Songs;
                        break;
                    case PlaySort.默认降序:
                        MusicData[] list = new MusicData[musicListData.Songs.Count];
                        musicListData.Songs.CopyTo(list, 0);
                        var l = list.ToList();
                        l.Reverse();
                        array = l;
                        break;
                    case PlaySort.名称升序:
                        array = [.. musicListData.Songs.OrderBy(m => m.Title)];
                        break;
                    case PlaySort.名称降序:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.Title)];
                        break;
                    case PlaySort.艺术家升序:
                        array = [.. musicListData.Songs.OrderBy(m => m.Artists.Count != 0 ? m.Artists[0].Name : "未知")];
                        break;
                    case PlaySort.艺术家降序:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.Artists.Count != 0 ? m.Artists[0].Name : "未知")];
                        break;
                    case PlaySort.专辑升序:
                        array = [.. musicListData.Songs.OrderBy(m => m.Album.Title)];
                        break;
                    case PlaySort.专辑降序:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.Album.Title)];
                        break;
                    case PlaySort.时间升序:
                        array = [.. musicListData.Songs.OrderBy(m => m.RelaseTime)];
                        break;
                    case PlaySort.时间降序:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.RelaseTime)];
                        break;
                    case PlaySort.索引升序:
                        array = [.. musicListData.Songs.OrderBy(m => m.Index)];
                        break;
                    case PlaySort.索引降序:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.Index)];
                        break;
                }
            });

            musicListBind.Clear();
            if (isUnloaded || musicListData == null)
            {
                array = null;
                return;
            }
            int count = 1;
            var dpi = CodeHelper.GetScaleAdjustment(App.WindowLocal);
            foreach (var item in array)
            {
                item.Count = count;
                musicListBind.Add(new() { MusicData = item, MusicListData = musicListData, ImageScaleDPI = dpi });
                count++;
            }

            SortComboBox.SelectedIndex = (int)musicListData.PlaySort;
            isInInitBindings = false;
        }

        private void InitInfo()
        {
            if (isUnloaded) return;
            foreach (var mld in App.playListReader.NowMusicListData)
            {
                if (mld.MD5 == md5)
                {
                    musicListData = mld;
                    break;
                }
            }

            if (musicListData == null) return;
            listSortEnum = Enum.GetValues(typeof(PlaySort)).Cast<PlaySort>().ToList();
            SortComboBox.ItemsSource = listSortEnum;
            SortComboBox.SelectedIndex = (int)musicListData.PlaySort;

            MainWindow.WindowDpiChanged -= MainWindow_WindowDpiChanged;
            MainWindow.WindowDpiChanged += MainWindow_WindowDpiChanged;
            ItemsList_Header_Info_TitleTextBlock.Text = musicListData.ListShowName;
            ItemsList_Header_Info_OtherTextBlock.Text = $"共 {musicListData.Songs.Count} 首歌曲";
        }

        static Thickness thickness0 = new(0);
        static Thickness thickness1 = new(1);
        private async void InitImage()
        {
            if (isUnloaded) return;
            ItemsList_Header_Image.BorderThickness = thickness0;
            ImageSource imageSource = null;
            if (musicListData.ListDataType == DataType.本地歌单)
            {
                bool isExists = true;
                await Task.Run(() => { isExists = File.Exists(musicListData.PicturePath); });
                if (isExists) imageSource = await FileHelper.GetImageSource(musicListData.PicturePath);
            }
            else if (musicListData.ListDataType == DataType.歌单)
            {
                imageSource = (await ImageManage.GetImageSource(musicListData)).Item1;
            }
            if (imageSource == null)
            {
                imageSource = await FileHelper.GetImageSource("");
            }

            if (isUnloaded) return;
            ItemsList_Header_Image.BorderThickness = thickness1;
            ItemsList_Header_Image.Source = imageSource;
            InitShyHeader();

        }

        private void Init()
        {
            InitInfo();
            InitImage();
            InitVisuals();
            InitShyHeader();
            InitBindings();
        }

        public ArrayList arrayList { get; set; }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            ItemsList.ItemsSource = musicListBind;
            //arrayList = new ArrayList(100000000);
        }

        bool isUnloaded = false;
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            isUnloaded = true;
            MainWindow.WindowDpiChanged -= MainWindow_WindowDpiChanged;
            DisposeVisuals();
            if (ItemsList_Header_Image != null) ItemsList_Header_Image.Source = null;
            if (ItemsList != null) ItemsList.ItemsSource = null;
            if (SortComboBox != null) SortComboBox.ItemsSource = null;
            musicListBind?.Clear();
            musicListBind = null;
            listSortEnum?.Clear();
            listSortEnum = null;
            musicListData = null;
            if (scrollViewer != null)
                scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
            Bindings.StopTracking();
            UnloadObject(this);
        }

        bool isDelayInitShyHeaderWhenScroll = false;
        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (scrollViewer == null) return;
            //scrollViewer.ScrollToVerticalOffset(Math.Round(scrollViewer.VerticalOffset, 0));
            scrollVisual.IsPixelSnappingEnabled = true;
            if (scrollViewer.VerticalOffset < 300)
            {
                isDelayInitShyHeaderWhenScroll = true;
                InitShyHeader(true);
                await Task.Delay(200);
                InitShyHeader(true);
            }
            else
            {
                if (isDelayInitShyHeaderWhenScroll)
                {
                    isDelayInitShyHeaderWhenScroll = false;
                    await Task.Delay(500);
                    InitShyHeader(true);
                }
            }
        }

        private void ItemsList_Header_Image_Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitShyHeader();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitShyHeader();
        }

        private void MainWindow_WindowDpiChanged()
        {
            InitShyHeader();
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            switch (btn.Tag)
            {
                case "playAll":
                    if (musicListBind.Count == 0) return;
                    if (App.playingList.PlayBehavior == znMusicPlayerWUI.Background.PlayBehavior.随机播放)
                    {
                        App.playingList.ClearAll();
                    }
                    foreach (var songItem in musicListBind)
                    {
                        App.playingList.Add(songItem.MusicData, false);
                    }
                    await App.playingList.Play(musicListBind.First().MusicData, true);
                    App.playingList.SetRandomPlay(App.playingList.PlayBehavior);
                    break;
                case "refresh":
                    InitBindings();
                    break;
                case "addLocal":
                    break;
                case "search":
                    ItemList_Header_Search_Control.IsOpen = !ItemList_Header_Search_Control.IsOpen;
                    break;
            }
        }

        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        bool isInSave = false;
        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInSave) return;
            if (isInInitBindings) return;
            if (musicListData == null) return;
            if (SortComboBox == null) return;
            if (SortComboBox.SelectedIndex == -1) return;
            if (SortComboBox.SelectedIndex == (int)musicListData.PlaySort) return;
            isInSave = true;
            musicListData.PlaySort = (PlaySort)SortComboBox.SelectedIndex;
            var data = await PlayListHelper.ReadData();
            data[musicListData.ListName] = JObject.FromObject(musicListData);
            await PlayListHelper.SaveData(data);
            InitBindings();
            isInSave = false;
        }

        private void ItemList_Header_Search_Control_IsOpenChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ItemsList_Header_Root.Margin = new(0, 0, 0, ItemList_Header_Search_Control.ActualHeight + 7);
            }
            else
            {
                ItemsList_Header_Root.Margin = new(0, 0, 0, 3);
            }
        }
    }
}
