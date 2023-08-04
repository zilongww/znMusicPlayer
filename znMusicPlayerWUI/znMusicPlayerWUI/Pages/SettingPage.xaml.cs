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
using znMusicPlayerWUI.Background.HotKeys;
using CommunityToolkit.WinUI.UI;
using CommunityToolkit.Labs.WinUI;

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
            SettingsExpander expander = null;
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
            AudioCachePlaceSize = "当前占用：" + CodeHelper.GetAutoSizeString(await CodeHelper.GetDirctoryLength(DataFolderBase.AudioCacheFolder), 2);
            AudioCachePlaceSizeBusy = false;
        }
        
        public async void ToImageCachePlaceSize()
        {
            ImageCachePlaceSizeBusy = true;
            ImageCachePlaceSize = "计算中...";
            ImageCachePlaceSize = "当前占用：" + CodeHelper.GetAutoSizeString(await CodeHelper.GetDirctoryLength(DataFolderBase.ImageCacheFolder), 2);
            ImageCachePlaceSizeBusy = false;
        }
        
        public async void ToLyricCachePlaceSize()
        {
            LyricCachePlaceSizeBusy = true;
            LyricCachePlaceSize = "计算中...";
            LyricCachePlaceSize = "当前占用：" + CodeHelper.GetAutoSizeString(await CodeHelper.GetDirctoryLength(DataFolderBase.LyricCacheFolder), 2);
            LyricCachePlaceSizeBusy = false;
        }

        public string CachePath { get; set; } = null;
        public string AudioCachePath { get; set; } = null;
        public string ImageCachePath { get; set; } = null;
        public string LyricCachePath { get; set; } = null;
        public string DownloadPath { get; set; } = null;
        public string AudioCachePlaceSize { get; set; } = null;
        public bool AudioCachePlaceSizeBusy { get; set; } = false;
        public string ImageCachePlaceSize { get; set; } = null;
        public bool ImageCachePlaceSizeBusy { get; set; } = false;
        public string LyricCachePlaceSize { get; set; } = null;
        public bool LyricCachePlaceSizeBusy { get; set; } = false;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ToAudioCachePlaceSize();
            ToImageCachePlaceSize();
            ToLyricCachePlaceSize();
            CachePath = DataFolderBase.CacheFolder;
            AudioCachePath = DataFolderBase.AudioCacheFolder;
            ImageCachePath = DataFolderBase.ImageCacheFolder;
            LyricCachePath = DataFolderBase.LyricCacheFolder;
            DownloadPath = DataFolderBase.DownloadFolder;

            //System.Diagnostics.Debug.WriteLine(App.downloadManager.br);
            /*
            switch (App.downloadManager.br)
            {
                case 128: DownloadFormatCb.SelectedIndex = 0; break;
                case 192: DownloadFormatCb.SelectedIndex = 1; break;
                case 320: DownloadFormatCb.SelectedIndex = 2; break;
                case 960: DownloadFormatCb.SelectedIndex = 3; break;
            }
            DownloadMaximumNb.Value = App.downloadManager.DownloadingMaxium;
            */
        }

        Visual headerVisual;
        ScrollViewer scrollViewer;
        public void UpdateShyHeader()
        {
            // 设置header为顶层
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)ListViewBase.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex(headerContainer, 1);

            if (scrollViewer == null)
            {
                scrollViewer = (VisualTreeHelper.GetChild(ListViewBase, 0) as Border).Child as ScrollViewer;
                scrollViewer.CanContentRenderOutsideBounds = true;
                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;
            }

            CompositionPropertySet scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
            Compositor compositor = scrollerPropertySet.Compositor;

            var padingSize = 40;
            // Get the visual that represents our HeaderTextBlock 
            // And define the progress animation string
            headerVisual = ElementCompositionPreview.GetElementVisual(HeaderBaseGrid);
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
                        //DownloadPathTb.Text = newPath.Path;
                        DataFolderBase.DownloadFolder = newPath.Path;
                    }
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {/*
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
            }*/
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
        /*
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
            }*/
            isCodeChangedTheme = false;
        }

        private void ThemeCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isCodeChangedTheme) return;/*
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
            MainWindow.UpdateWindowBackdropTheme();*/
        }


        private void PlayBehaviourBaseeGrid_Loaded(object sender, RoutedEventArgs e)
        {/*
            PlayBehaviourCb.SelectedIndex = (int)App.playingList.PlayBehaviour;*/
        }

        private void PlayBehaviourCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //App.playingList.PlayBehaviour = (PlayBehaviour)Enum.Parse(typeof(PlayBehaviour), PlayBehaviourCb.SelectedItem as string);
            //System.Diagnostics.Debug.WriteLine(App.playingList.PlayBehaviour);
        }

        private void Page_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            var list = App.hotKeyManager.RegistedHotKeys.ToList();
            App.hotKeyManager.UnregisterHotKeys(list);
            await Task.Delay(200);
            App.hotKeyManager.RegisterHotKeys(list);
        }

        private async void Button_Click_6(object sender, RoutedEventArgs e)
        {
            var list = App.hotKeyManager.RegistedHotKeys.ToList();
            App.hotKeyManager.UnregisterHotKeys(list);
            await Task.Delay(200);
            App.hotKeyManager.RegisterHotKeys(HotKeyManager.DefaultRegisterHotKeysList);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            string tagObj = button.Tag as string;
            string folderPath = null;
            switch (tagObj)
            {
                case "0":
                    folderPath = CachePath;
                    break;
                case "1":
                    folderPath = AudioCachePath;
                    break;
                case "2":
                    folderPath = ImageCachePath;
                    break;
                case "3":
                    folderPath = LyricCachePath;
                    break;
                case "4":
                    folderPath = DownloadPath;
                    break;
            }

            await FileHelper.OpenFilePath(folderPath);
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            string tagObj = button.Tag as string;
            var folder = await FileHelper.UserSelectFolder();
            if (folder == null) return;
            var folderPath = folder.Path;
            switch (tagObj)
            {
                case "0":
                    DataFolderBase.CacheFolder = folderPath;
                    break;
                case "1":
                    DataFolderBase.AudioCacheFolder = folderPath;
                    break;
                case "2":
                    DataFolderBase.ImageCacheFolder = folderPath;
                    break;
                case "3":
                    DataFolderBase.LyricCacheFolder = folderPath;
                    break;
                case "4":
                    DataFolderBase.DownloadFolder = folderPath;
                    break;
            }
            (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(button)))) as SettingsCard).Description = folderPath;
        }

        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            string tagObj = button.Tag as string;
            string folderPath = null;
            switch (tagObj)
            {
                case "0":
                    folderPath = AudioCachePath;
                    break;
                case "1":
                    folderPath = ImageCachePath;
                    break;
                case "2":
                    folderPath = LyricCachePath;
                    break;
            }
            await Task.Run(() =>
            {
                var files = Directory.EnumerateFiles(folderPath);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
            });

            (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(button))) as SettingsCard).Description = "当前占用：0B";
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Download_NamedRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged_2(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ComboBox_SelectionChanged_3(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
