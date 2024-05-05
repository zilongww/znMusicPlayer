using System.IO;
using System.Collections;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace znMusicPlayerWUI.Pages.ListViewPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayListPage : Page
    {
        ScrollViewer scrollViewer;
        MusicListData musicListData = null;
        ObservableCollection<SongItemBindBase> musicListBind = new();
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

            ItemsList_Header_Image.Margin = new(0);
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

        private void InitBindings()
        {
            if (isUnloaded) return;
            int count = 1;
            foreach (var item in musicListData.Songs)
            {
                item.Count = count;
                musicListBind.Add(new() { MusicData = item });
                count++;
            }
        }

        private void InitInfo()
        {
            if (isUnloaded) return;
            ItemsList_Header_Info_TitleTextBlock.Text = musicListData.ListShowName;
            ItemsList_Header_Info_OtherTextBlock.Text = $"共 {musicListData.Songs.Count} 首歌曲";
        }

        private async void InitImage()
        {
            if (isUnloaded) return;
            ItemsList_Header_ImageInfo_Root.BorderThickness = new(0);
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
            ItemsList_Header_ImageInfo_Root.BorderThickness = new(1);
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

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            InitShyHeader(true);
        }

        public ArrayList arrayList { get; set; }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (musicListData == null) return;
            Init();
            ItemsList.ItemsSource = musicListBind;
            //arrayList = new ArrayList(10000000);
        }

        bool isUnloaded = false;
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            isUnloaded = true;
            ItemsList_Header_Image.Source = null;
            musicListBind.Clear();
            ItemsList.ItemsSource = null; 
            musicListBind = null;
            if (scrollViewer != null)
                scrollViewer.ViewChanging -= ScrollViewer_ViewChanging;
        }

        private void ItemsList_Header_Image_Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitShyHeader();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitShyHeader();
        }
    }
}
