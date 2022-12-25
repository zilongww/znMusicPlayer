using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.DataEditor;
using Windows.Networking.Connectivity;
using znMusicPlayerWUI.Windowed;
using znMusicPlayerWUI.Background;

namespace znMusicPlayerWUI.Pages
{
    public partial class SettingPage : Page
    {
        public SettingPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var a = (string)e.Parameter;
            if (!string.IsNullOrEmpty(a))
            {
                DelaySetParameter(a);
            }
        }

        private void DelaySetParameter(string value)
        {
            Expander expander = null;
            switch (value)
            {
                case "open download":
                    expander = DownloadEpd;
                    break;
            }
            expander.IsExpanded = true;
            ListViewBase.ScrollIntoView(expander);
        }

        public async void ToAudioCachePlaceSize()
        {
            AudioCachePlaceSizeBusy = true;
            AudioCachePlaceSize = "计算中...";
            AudioCachePlaceSize = "当前占用：" + CodeHelper.GetAutoSizeString(await CodeHelper.GetDirctoryLength(DataEditor.DataFolderBase.AudioCacheFolder), 2);
            AudioCachePlaceSizeBusy = false;
        }

        public string CachePath { get; set; } = null;
        public string AudioCachePath { get; set; } = null;
        public string ImageCachePath { get; set; } = null;
        public string LyricCachePath { get; set; } = null;
        public string AudioCachePlaceSize { get; set; } = null;
        public bool AudioCachePlaceSizeBusy { get; set; } = false;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ToAudioCachePlaceSize();
            CachePath = DataFolderBase.CacheFolder;
            AudioCachePath = DataFolderBase.AudioCacheFolder;
            ImageCachePath = DataFolderBase.ImageCacheFolder;
            LyricCachePath = DataFolderBase.LyricCacheFolder;
            DownloadPathTb.Text = DataFolderBase.DownloadFolder;

            //System.Diagnostics.Debug.WriteLine(App.downloadManager.br);
            switch (App.downloadManager.br)
            {
                case 128: DownloadFormatCb.SelectedIndex = 0; break;
                case 192: DownloadFormatCb.SelectedIndex = 1; break;
                case 320: DownloadFormatCb.SelectedIndex = 2; break;
                case 960: DownloadFormatCb.SelectedIndex = 3; break;
            }
            DownloadMaximumNb.Value = App.downloadManager.DownloadingMaxium;
        }

        public void UpdataShyHeader()
        {
            // 设置header为顶层
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)ListViewBase.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex(headerContainer, 1);

            var scrollViewer = (VisualTreeHelper.GetChild(ListViewBase, 0) as Border).Child as ScrollViewer;
            scrollViewer.CanContentRenderOutsideBounds = true;

            CompositionPropertySet scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
            Compositor compositor = scrollerPropertySet.Compositor;

            var padingSize = 40;
            // Get the visual that represents our HeaderTextBlock 
            // And define the progress animation string
            var headerVisual = ElementCompositionPreview.GetElementVisual(HeaderBaseGrid);
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

        private void CachePathBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void CacheDeleteBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void AudioCachePathBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void ImageCachePathBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void LyricCachePathBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdataShyHeader();
        }

        private void DownloadPathBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                if (button.Content as string == "打开目标文件夹")
                {
                    await FileHelper.OpenFilePath(DataFolderBase.DownloadFolder);
                }
                else
                {
                    var newPath = await FileHelper.UserSelectFolder(Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
                    if (newPath != null)
                    {
                        DownloadPathTb.Text = newPath.Path;
                        DataFolderBase.DownloadFolder = newPath.Path;
                    }
                }
            }
        }

        private void DownloadFormatBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var a = DownloadFormatCb.SelectedIndex;
            switch (a)
            {
                case 0:
                    App.downloadManager.br = 128;
                    break;
                case 1:
                    App.downloadManager.br = 192;
                    break;
                case 2:
                    App.downloadManager.br = 320;
                    break;
                case 3:
                    App.downloadManager.br = 960;
                    break;
            }
        }

        private void DownloadMaximumBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void NumberBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            App.downloadManager.DownloadingMaxium = (int)sender.Value;
        }

        private void OpenMediaBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Content as string == "打开文件")
            {
                var res = await FileHelper.UserSelectFile(Windows.Storage.Pickers.PickerViewMode.List, Windows.Storage.Pickers.PickerLocationId.VideosLibrary, new[] { ".mp4", "*" });
                if (res != null)
                {
                    new MediaPlayerWindow(res.Path);
                }
            }
            else
            {
                var tbox = new TextBox() { PlaceholderText = "请输入媒体文件地址" };
                var res = await MainWindow.ShowDialog("输入地址", tbox, "取消", "确定");
                if (res == ContentDialogResult.Primary)
                {
                    new MediaPlayerWindow(tbox.Text);
                }
            }
        }

        bool isCodeChangedTheme = false;
        private void ThemeBaseGrid_Loaded(object sender, RoutedEventArgs e)
        {
            isCodeChangedTheme = true;
            switch (MainWindow.SWindowGridBaseTop.RequestedTheme)
            {
                case ElementTheme.Default:
                    ThemeCb.SelectedIndex = 0;
                    break;
                case ElementTheme.Light:
                    ThemeCb.SelectedIndex = 1;
                    break;
                case ElementTheme.Dark:
                    ThemeCb.SelectedIndex = 2;
                    break;
            }
            isCodeChangedTheme = false;
        }

        private void ThemeCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCodeChangedTheme) return;
            switch (ThemeCb.SelectedItem as string)
            {
                case "跟随系统":
                    MainWindow.SWindowGridBaseTop.RequestedTheme = ElementTheme.Default;
                    break;
                case "浅色":
                    MainWindow.SWindowGridBaseTop.RequestedTheme = ElementTheme.Light;
                    break;
                case "深色":
                    MainWindow.SWindowGridBaseTop.RequestedTheme = ElementTheme.Dark;
                    break;
            }
            MainWindow.UpdataWindowBackdropTheme();
        }


        private void PlayBehaviourBaseeGrid_Loaded(object sender, RoutedEventArgs e)
        {
            PlayBehaviourCb.SelectedIndex = (int)App.playingList.PlayBehaviour;
        }

        private void PlayBehaviourCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.playingList.PlayBehaviour = (PlayBehaviour)Enum.Parse(typeof(PlayBehaviour), PlayBehaviourCb.SelectedItem as string);
            //System.Diagnostics.Debug.WriteLine(App.playingList.PlayBehaviour);
        }

        private void Page_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
        }
    }
}
