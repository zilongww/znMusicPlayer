using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using Microsoft.UI.Xaml.Navigation;
using znMusicPlayerWUI.Controls;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using znMusicPlayerWUI.DataEditor;
using Newtonsoft.Json.Linq;

namespace znMusicPlayerWUI.Pages
{
    public partial class HistoryPage : Page
    {
        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            await Task.Delay(500);
            HistoryHelper.HistoryDataChanged -= HistoryHelper_HistoryDataChanged;
            songHistories.Clear();
            ListViewBase.ItemsSource = null;
            ListViewBase.Items.Clear();
        }

        ObservableCollection<SongHistoryData> songHistories = new();
        public HistoryPage()
        {
            InitializeComponent();
            ListViewBase.ItemsSource = songHistories;
            HistoryHelper.HistoryDataChanged += HistoryHelper_HistoryDataChanged;
        }

        private void HistoryHelper_HistoryDataChanged()
        {
            Init();
        }

        private async void Init()
        {
            var scrollOffset = scrollViewer.VerticalOffset;
            songHistories.Clear();
            var datas = await SongHistoryHelper.GetHistories();
            List<SongHistoryData> d = new();
            foreach (var data in datas)
            {
                d.Add(data);
            }
            d = d.OrderByDescending(m => m.Time).ToList();
            foreach (var data in d)
            {
                songHistories.Add(data);
            }
            await Task.Delay(10);
            scrollViewer.ScrollToVerticalOffset(scrollOffset);
        }

        Visual headerVisual;
        public void UpdataShyHeader()
        {
            if (scrollViewer == null) return;
            CompositionPropertySet scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
            Compositor compositor = scrollerPropertySet.Compositor;

            var padingSize = 50;
            // Get the visual that represents our HeaderTextBlock 
            // And define the progress animation string
            String progress = $"Clamp(-scroller.Translation.Y / {padingSize}, 0, 1.0)";

            // Shift the header by 50 pixels when scrolling down
            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {padingSize}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            /*
            Visual textVisual = ElementCompositionPreview.GetElementVisual(HeaderBaseTextBlock);
            Vector3 finalOffset = new Vector3(0, 10, 0);
            var headerOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0,0,0), finalOffset, {progress})");
            headerOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            headerOffsetAnimation.SetVector3Parameter("finalOffset", finalOffset);
            textVisual.StartAnimation(nameof(Visual.Offset), headerOffsetAnimation);
            */

            // Logo scale and transform                                          from               to
            var logoHeaderScaleAnimation = compositor.CreateExpressionAnimation("Lerp(Vector2(1,1), Vector2(0.7, 0.7), " + progress + ")");
            logoHeaderScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);

            var logoVisual = ElementCompositionPreview.GetElementVisual(HeaderBaseTextBlock);
            logoVisual.StartAnimation("Scale.xy", logoHeaderScaleAnimation);

            var logoVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 34, {progress})");
            logoVisualOffsetYAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Offset.Y", logoVisualOffsetYAnimation);

            var logoVisualOffsetXAnimation = compositor.CreateExpressionAnimation($"Lerp(0, -12, {progress})");
            logoVisualOffsetXAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Offset.X", logoVisualOffsetXAnimation);

            var backgroundVisual = ElementCompositionPreview.GetElementVisual(HeaderBaseRectangle);
            var backgroundVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            backgroundVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            backgroundVisual.StartAnimation("Opacity", backgroundVisualOpacityAnimation);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdataShyHeader();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await App.playingList.Play(((sender as Button).DataContext as SongHistoryData).MusicData);
        }

        ScrollViewer scrollViewer;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // 设置header为顶层
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)ListViewBase.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex(headerContainer, 1);

            headerVisual = ElementCompositionPreview.GetElementVisual(HeaderBaseGrid);
            scrollViewer = (VisualTreeHelper.GetChild(ListViewBase, 0) as Border).Child as ScrollViewer;
            scrollViewer.CanContentRenderOutsideBounds = true;
            scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            UpdataShyHeader();
            Init();
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            headerVisual.IsPixelSnappingEnabled = true;
        }
    }
}
