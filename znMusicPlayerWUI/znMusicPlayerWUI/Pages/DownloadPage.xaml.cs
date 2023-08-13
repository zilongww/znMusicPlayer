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
using znMusicPlayerWUI.Controls;
using Microsoft.UI.Xaml.Media.Animation;

namespace znMusicPlayerWUI.Pages
{
    public partial class DownloadPage : Page
    {
        public DownloadPage()
        {
            InitializeComponent();
            int allDownload = 0;
            int downloaded = 0;

            var UpdateTextTB = () => HeaderBaseTextBlock.Text = $"下载（{App.downloadManager.DownloadedData.Count}/{App.downloadManager.AllDownloadData.Count} - {App.downloadManager.DownloadErrorData.Count} 错误）";
            App.downloadManager.AddDownload += (dm) =>
            {
                bool noContains = true;
                foreach (DownloadCard downloadCard in ListViewBase.Items)
                {
                    if (downloadCard.downloadData == dm)
                    {
                        noContains = false;
                    }
                }

                if (noContains)
                {
                    ListViewBase.Items.Add(new DownloadCard(dm));
                    allDownload++;
                    UpdateTextTB();
                }
            };
            App.downloadManager.OnDownloading += (dm) =>
            {
                foreach (DownloadCard downloadCard in ListViewBase.Items)
                {
                    if (downloadCard != null)
                    {
                        if (downloadCard.downloadData == dm)
                        {
                            downloadCard.downloadData.DownloadPercent = dm.DownloadPercent;
                            downloadCard.SetProgressValue();
                            break;
                        }
                    }
                }
            };
            App.downloadManager.OnDownloadedPreview += (dm) =>
            {
                foreach (DownloadCard downloadCard in ListViewBase.Items)
                {
                    if (downloadCard != null)
                    {
                        if (downloadCard.downloadData == dm)
                        {
                            downloadCard.SetDownloadedPreview();
                            break;
                        }
                    }
                }
            };
            App.downloadManager.OnDownloaded += (dm) =>
            {
                foreach (DownloadCard downloadCard in ListViewBase.Items)
                {
                    if (downloadCard != null)
                    {
                        if (downloadCard.downloadData == dm)
                        {
                            downloadCard.SetDownloaded();
                            downloaded++;
                            UpdateTextTB();
                            break;
                        }
                    }
                }
            };
            App.downloadManager.OnDownloadError += (dm) =>
            {
                foreach (DownloadCard downloadCard in ListViewBase.Items)
                {
                    if (downloadCard != null)
                    {
                        if (downloadCard.downloadData == dm)
                        {
                            downloadCard.SetError();
                            UpdateTextTB();
                            break;
                        }
                    }
                }
            };

            // 当第一次初始化时加载
            foreach (var dm in App.downloadManager.AllDownloadData)
            {
                ListViewBase.Items.Add(new DownloadCard(dm));
                allDownload++;
            }
            foreach (var dm in App.downloadManager.DownloadingData)
            {
                App.downloadManager.CallOnDownloadingEvent(dm);
            }
            foreach (var dm in App.downloadManager.DownloadedData)
            {
                App.downloadManager.CallOnDownloadedEvent(dm);
                downloaded++;
            }
            foreach (var dm in App.downloadManager.DownloadErrorData)
            {
                App.downloadManager.CallOnDownloadErrorEvent(dm);
            }
        }

        ScrollViewer scrollViewer;
        Visual headerVisual;
        public void UpdateShyHeader()
        {
            // 设置header为顶层
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)ListViewBase.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex(headerContainer, 1);

            if (scrollViewer == null)
            {
                headerVisual = ElementCompositionPreview.GetElementVisual(HeaderBaseGrid);
                scrollViewer = (VisualTreeHelper.GetChild(ListViewBase, 0) as Border).Child as ScrollViewer;
                scrollViewer.CanContentRenderOutsideBounds = true;
                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            }

            CompositionPropertySet scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
            Compositor compositor = scrollerPropertySet.Compositor;

            var padingSize = 40;
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

            var logoVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 24, {progress})");
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

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            headerVisual.IsPixelSnappingEnabled = true;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateShyHeader();
        }

        private void ToSettingBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetNavViewContent(
                typeof(SettingPage),
                "open download");
        }
    }
}
