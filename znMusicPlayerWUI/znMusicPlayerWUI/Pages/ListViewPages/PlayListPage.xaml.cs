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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            musicListData = e.Parameter as MusicListData;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            musicListData = null;
        }

        public PlayListPage()
        {
            InitializeComponent();
        }


        CompositionPropertySet scrollerPropertySet;
        Compositor compositor;
        Visual headerVisual;
        Visual backgroundVisual;
        Visual imageVisual;
        Visual infoVisual;
        Visual commandBarVisual;
        private void InitVisuals()
        {
            if (isUnloaded) return;

            // 设置 header 为顶层
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)ItemsList.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex(headerContainer, 1);

            scrollViewer = (VisualTreeHelper.GetChild(ItemsList, 0) as Border).Child as ScrollViewer;
            scrollViewer.CanContentRenderOutsideBounds = true;
            scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
            scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

            compositor = scrollerPropertySet.Compositor;
            headerVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_ImageInfo_Root);
            backgroundVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_ImageInfo_BackgroundFill);
            imageVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Image_Root);
            infoVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Info_Root);
            commandBarVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Info_CommandBar);
        }

        private void InitShyHeader(bool imageSizeOnly = false)
        {
            if (scrollViewer == null) return;
            if (compositor == null) return;
            if (isUnloaded) return;

            var anotherHeight = 154;
            double imageSizeEnd = 0.45;
            string progress = $"Clamp(-scroller.Translation.Y / {anotherHeight}, 0, 1.0)";

            var logoHeaderScaleAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector2(1, 1), Vector2({imageSizeEnd}, {imageSizeEnd}), {progress})");
            logoHeaderScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            imageVisual.StartAnimation("Scale.xy", logoHeaderScaleAnimation);
            if (ItemsList_Header_Root.ActualWidth != 0)
                ItemsList_Header_Info_Root_SizeChanger.Width = ItemsList_Header_Root.ActualWidth - ItemsList_Header_Image_Root.ActualWidth * imageVisual.Scale.X - 32 - 16;
            if (imageSizeOnly) return;

            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {anotherHeight}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            var backgroundVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            backgroundVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            backgroundVisual.StartAnimation("Opacity", backgroundVisualOpacityAnimation);

            var imageVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector2(0, 0), Vector2(0, {anotherHeight}), {progress})");
            imageVisualOffsetYAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            imageVisual.StartAnimation("Offset.xy", imageVisualOffsetYAnimation);

            var infoVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3({ItemsList_Header_Image_Root.ActualWidth + 16}, 0, 0), Vector3({(int)(ItemsList_Header_Image_Root.ActualWidth * imageSizeEnd) + 16}, {anotherHeight}, 0), {progress})");
            infoVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            infoVisual.StartAnimation(nameof(infoVisual.Offset), infoVisualOffsetAnimation);

            var commandBarVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(-6, {imageVisual.Size.Y - commandBarVisual.Size.Y + 6}, 0), Vector3(-6, {imageVisual.Size.Y * imageSizeEnd - commandBarVisual.Size.Y + 6}, 0), {progress})");
            commandBarVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            commandBarVisual.StartAnimation(nameof(infoVisual.Offset), commandBarVisualOffsetAnimation);
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
                        List<MusicData> list = musicListData.Songs;
                        list.Reverse();
                        array = list;
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
                musicListBind.Add(new() { MusicData = item, ImageScaleDPI = dpi });
                count++;
            }
            SortComboBox.SelectedIndex = (int)musicListData.PlaySort;
            isInInitBindings = false;
        }

        private void InitInfo()
        {
            if (isUnloaded) return;
            ItemsList_Header_Info_TitleTextBlock.Text = musicListData.ListShowName;
            ItemsList_Header_Info_OtherTextBlock.Text = $"共 {musicListData.Songs.Count} 首歌曲";
        }

        static Thickness thickness0 = new(0);
        static Thickness thickness1 = new(1);
        private async void InitImage()
        {
            if (isUnloaded) return;
            ItemsList_Header_ImageInfo_Root.BorderThickness = thickness0;
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

            if (isUnloaded) return;
            ItemsList_Header_ImageInfo_Root.BorderThickness = thickness1;
            ItemsList_Header_Image.Source = imageSource;
            InitShyHeader();

        }

        private void Init()
        {
            InitInfo();
            InitImage();
            InitBindings();
            InitVisuals();
            InitShyHeader();
        }

        public ArrayList arrayList { get; set; }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (musicListData == null) return;
            Init();
            ItemsList.ItemsSource = musicListBind;

            listSortEnum = Enum.GetValues(typeof(PlaySort)).Cast<PlaySort>().ToList();
            SortComboBox.ItemsSource = listSortEnum;
            SortComboBox.SelectedIndex = (int)musicListData.PlaySort;
            //arrayList = new ArrayList(100000000);
        }

        bool isUnloaded = false;
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            isUnloaded = true;
            if (ItemsList_Header_Image != null) ItemsList_Header_Image.Source = null;
            if (ItemsList != null) ItemsList.ItemsSource = null;
            if (SortComboBox != null) SortComboBox.ItemsSource = null;
            musicListBind?.Clear();
            musicListBind = null;
            listSortEnum?.Clear();
            listSortEnum = null;
            musicListData = null;
            if (scrollViewer != null)
                scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            InitShyHeader(true);
        }

        private void ItemsList_Header_Image_Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitShyHeader();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
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
                    break;
            }
        }

        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInInitBindings) return;
            if (musicListData == null) return;
            if (SortComboBox == null) return;
            if (SortComboBox.SelectedIndex == -1) return;
            if (SortComboBox.SelectedIndex == (int)musicListData.PlaySort) return;
            musicListData.PlaySort = (PlaySort)SortComboBox.SelectedIndex;
            InitBindings();
        }
    }
}
