using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Composition;
using WinRT;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.Pages;
using znMusicPlayerWUI.Pages.MusicPages;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics;
using System.Runtime.InteropServices;
using WinRT.Interop;
using NAudio.Wave;
using Microsoft.UI.Xaml.Hosting;
using System.Collections.ObjectModel;
using znMusicPlayerWUI.Controls;
using NAudio.Gui;
using System.Runtime.Intrinsics.Arm;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace znMusicPlayerWUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static UIElement SContent;
        public static Window SWindow;
        public static MusicPage SMusicPage = new();
        public static Grid STopControlsBaseGrid;
        public static Grid SWindowGridBase;
        public static Grid SVolumeBaseGrid;
        public static Grid SMusicPageBaseGrid;
        public static ListView SPlayingListBaseView;
        public static Grid SPlayingListBaseGrid;
        public static Frame SMusicPageBaseFrame;
        public static Frame SPlayContent;
        public static TeachingTip teachingTipVolume;
        public static TeachingTip teachingTipPlayingList;
        static ContentDialog AsyncDialog = null;

        public MainWindow()
        {
            SWindow = this;
            this.InitializeComponent();
            WindowGridBase.DataContext = this;
            SContent = this.Content;
            SNavView = NavView;
            SContentFrame = ContentFrame;
            STopControlsBaseGrid = TopControlsBaseGrid;
            SWindowGridBase = GridBase;
            SVolumeBaseGrid = VolumeBaseGrid;
            SMusicPageBaseGrid = MusicPageBaseGrid;
            SPlayingListBaseGrid = PlayingListBaseGrid;
            SPlayingListBaseView = PlayingListBaseView;
            SMusicPageBaseFrame = MusicPageBaseFrame;
            teachingTipPlayingList = PlayingListBasePopup;
            teachingTipVolume = VolumeBasePopup;
            SPlayContent = PlayContent;
            AsyncDialog = new ContentDialog() { XamlRoot = SContent.XamlRoot, CloseButtonCommand = null };
            equalizerPage = new Pages.DialogPages.EqualizerPage();
            SubClassing();

            App.AppWindowLocal = WindowHelper.GetAppWindowForCurrentWindow(this);
            App.AppWindowLocal.Title = App.AppName;
            App.AppWindowLocal.SetIcon("icon.ico");

            InitializeTitleBar(App.Current.RequestedTheme);

            //RequestedTheme = App.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
            m_wsdqHelper = new WindowsSystemDispatcherQueueHelper();
            m_wsdqHelper.EnsureWindowsSystemDispatcherQueueController();
            SetBackdrop(BackdropType.Mica);
            SetDragRegionForCustomTitleBar(App.AppWindowLocal);

            NavView.SelectedItem = NavView.MenuItems[1];
            NavView.IsBackEnabled = false;

            Activated += MainWindow_Activated;
            WindowGridBase.ActualThemeChanged += WindowGridBase_ActualThemeChanged;
            App.audioPlayer.SourceChanged += AudioPlayer_SourceChanged;
            App.audioPlayer.PlayEnd += AudioPlayer_PlayEnd;
            App.audioPlayer.PlayStateChanged += AudioPlayer_PlayStateChanged;
            App.audioPlayer.TimingChanged += AudioPlayer_TimingChanged;
            App.audioPlayer.CacheLoadedChanged += AudioPlayer_CacheLoadedChanged;
            App.audioPlayer.CacheLoadingChanged += AudioPlayer_CacheLoadingChanged;
            App.audioPlayer.VolumeChanged += AudioPlayer_VolumeChanged;
            App.playingList.NowPlayingImageLoading += PlayingList_NowPlayingImageLoading;
            App.playingList.NowPlayingImageLoaded += PlayingList_NowPlayingImageLoaded;
            App.lyricManager.PlayingLyricSelectedChange += (_) =>
            {
                if (SWindowGridBase.Visibility == Visibility.Visible)
                    AppTitleTextBlock.Text = $"{App.AppName} - {_.Lyric}";
            };
            
            // 第一次点击不会响应动画。。。
            App.LoadSettings();
        }

        #region init TitleBar
        public void InitializeTitleBar(ApplicationTheme theme)
        {
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                InitializeTitleBara(App.AppWindowLocal.TitleBar, theme);
            }
            else
            {
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(AppTitleBar);
                AppTitleBar.Height = 28;
                AppTitleBar.Margin = new Thickness(0);
                NavView.Margin = new Thickness(0, 28, 0, 0);
                AppTitleTextBlock.Margin = new Thickness(18, 0, 0, 0);
                if (theme == ApplicationTheme.Dark)
                {
                    WindowGridBase.Background = new SolidColorBrush(Color.FromArgb(255, 32, 32, 32));
                    AppTitleBar.Background = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                }
                else
                {
                    //w10TitleBarColorReplaceBaseGrid.Visibility = Visibility.Visible;
                    WindowGridBase.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                    AppTitleBar.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
            }
        }

        public static void InitializeTitleBara(AppWindowTitleBar bar, ApplicationTheme theme)
        {
            bar.ExtendsContentIntoTitleBar = true;

            if (theme == ApplicationTheme.Light)
            {
                bar.ButtonBackgroundColor = Colors.Transparent;
                bar.ButtonForegroundColor = Colors.Black;
                bar.ButtonHoverBackgroundColor = Color.FromArgb(20, 0, 0, 0);
                bar.ButtonHoverForegroundColor = Colors.Black;
                bar.ButtonPressedBackgroundColor = Color.FromArgb(10, 0, 0, 0);
                bar.ButtonPressedForegroundColor = Color.FromArgb(100, 0, 0, 0);
                bar.ButtonInactiveBackgroundColor = Colors.Transparent;
                bar.ButtonInactiveForegroundColor = Color.FromArgb(50, 0, 0, 0);
            }
            else
            {
                bar.ButtonBackgroundColor = Colors.Transparent;
                bar.ButtonForegroundColor = Colors.White;
                bar.ButtonHoverBackgroundColor = Color.FromArgb(20, 255, 255, 255);
                bar.ButtonHoverForegroundColor = Colors.White;
                bar.ButtonPressedBackgroundColor = Color.FromArgb(10, 255, 255, 255);
                bar.ButtonPressedForegroundColor = Color.FromArgb(100, 255, 255, 255);
                bar.ButtonInactiveBackgroundColor = Colors.Transparent;
                bar.ButtonInactiveForegroundColor = Color.FromArgb(50, 255, 255, 255);
            }
        }
        #endregion

        #region Dialog
        static bool dialogShow = false;
        static List<object[]> dialogShowObjects = new();
        public static async Task<ContentDialogResult> ShowDialog(
            object title, object content,
            string closeButtonText = "确定", string primaryButtonText = null)
        {
            ContentDialogResult result = default;
            if (!dialogShow)
            {
                dialogShow = true;
                AsyncDialog.Title = title;
                AsyncDialog.Content = content;
                AsyncDialog.Background = App.Current.Resources["AcrylicInAppFillColorBaseBrush"] as AcrylicBrush;
                AsyncDialog.CloseButtonText = closeButtonText;
                AsyncDialog.PrimaryButtonText = primaryButtonText;
                AsyncDialog.CloseButtonCommand = null;
                AsyncDialog.XamlRoot = SContent.XamlRoot;
                result = await AsyncDialog.ShowAsync();
                dialogShow = false;

                if (dialogShowObjects.Any())
                {
                    var a = dialogShowObjects[0];
                    dialogShowObjects.Remove(a);
                    await ShowDialog(a[0], a[1], (string)a[2], (string)a[3]);
                }
            }
            else
            {
                dialogShowObjects.Add(new object[] { title, content, closeButtonText, primaryButtonText });
            }
            return result;
        }

        static Pages.DialogPages.EqualizerPage equalizerPage;
        public static async Task ShowEqualizerDialog()
        {
            await ShowDialog("音频设置", equalizerPage);
        }
        #endregion

        #region AudioPlayer Events
        private void AudioPlayer_VolumeChanged(Media.AudioPlayer audioPlayer, object data)
        {
            if (!isPEntered)
            {
                VolumeSlider.Value = (float)data * 100;
            }

            if ((float)data == 0)
            {
                VolumeIconBase.Symbol = Symbol.Mute;
                VolumeAppButton.Icon = new SymbolIcon(Symbol.Mute);
            }
            else
            {
                VolumeIconBase.Symbol = Symbol.Volume;
                VolumeAppButton.Icon = new SymbolIcon(Symbol.Volume);
            }
        }

        private async void AudioPlayer_PlayEnd(Media.AudioPlayer audioPlayer)
        {
            if (true)
            {
                AudioPlayer_PlayStateChanged(audioPlayer);
                await App.playingList.PlayNext();
            }
        }

        private void AudioPlayer_CacheLoadedChanged(Media.AudioPlayer audioPlayer)
        {
            PlayRing.Value = 0;
            PlayRing.IsIndeterminate = false;
            PlayingListBaseView.SelectedItem = audioPlayer.MusicData;
        }

        private void AudioPlayer_CacheLoadingChanged(Media.AudioPlayer audioPlayer, object data)
        {
            if (!CodeHelper.IsIconic(App.AppWindowLocalHandle))
            {
                PlayRing.IsIndeterminate = true;
                PlayRing.Foreground = App.Current.Resources["AccentAAFillColorDefaultBrush"] as SolidColorBrush;
            }
        }

        static bool isDeleteImage = true;
        private static void PlayingList_NowPlayingImageLoading(ImageSource imageSource)
        {
            if (SPlayContent.Content.GetType() != typeof(Imagezn))
            {
                SPlayContent.Content = new Imagezn();
            }
            /*
            if (!CodeHelper.IsIconic(App.AppWindowLocalHandle) && !InOpenMusicPage)
            {
                var image1 = SPlayContent.Content as Imagezn;
                if (image1 != null)
                {
                    image1.Dispose();
                }
                SPlayContent.Content = null;
                isDeleteImage = true;
            }
            else isDeleteImage = false;*/
        }

        public static void PlayingList_NowPlayingImageLoaded(ImageSource imageSource)
        {
            if (!CodeHelper.IsIconic(App.AppWindowLocalHandle) && !InOpenMusicPage)
            {
                if (imageSource != (SPlayContent.Content as Imagezn)?.Source)
                {
                    (SPlayContent.Content as Imagezn).Source = imageSource;
                    //if (!isDeleteImage) PlayingList_NowPlayingImageLoading(null);
                    //var image = new Imagezn() { Source = imageSource, Opacity = 1, ShowMenuBehavior = Imagezn.ShowMenuBehaviors.PointEnter };
                    //SPlayContent.Content = image;
                    /*AnimateHelper.AnimateScalar(
                        image, 1f, 1,
                        0.2f, 1f, 0.22f, 1f,
                        out var visual, out var compositor, out var animation);
                    visual.Opacity = 0;
                    visual.StartAnimation("Opacity", animation);*/
                }
            }
        }

        private void AudioPlayer_SourceChanged(Media.AudioPlayer audioPlayer)
        {
            bool b = false;
            if (audioPlayer.MusicData != App.playingList.NowPlayingMusicData)
            {
                b = true;
                PlayTitle.Text = audioPlayer.MusicData.Title;
                PlayArtist.Text = audioPlayer.MusicData.ButtonName;
            }

            if (b)
            {
                App.playingList.NowPlayingMusicData = audioPlayer.MusicData;
            }

            PlayingListBaseView.SelectedItem = audioPlayer.MusicData;
        }

        private void AudioPlayer_PlayStateChanged(Media.AudioPlayer audioPlayer)
        {
            if (audioPlayer.NowOutObj?.PlaybackState == PlaybackState.Playing)
            {
                PlayRing.Foreground = App.Current.Resources["AccentAAFillColorDefaultBrush"] as SolidColorBrush;
            }
            else
            {
                PlayRing.Foreground = new SolidColorBrush(Color.FromArgb(255, 225, 225, 0));
            }

            MediaPlayStateViewer.PlaybackState = audioPlayer.NowOutObj != null ? audioPlayer.NowOutObj.PlaybackState : PlaybackState.Paused;
        }

        private void AudioPlayer_TimingChanged(Media.AudioPlayer audioPlayer)
        {
            if (!CodeHelper.IsIconic(App.AppWindowLocalHandle) && !InOpenMusicPage)
            {
                if (audioPlayer.FileReader != null)
                {
                    PlayRing.Minimum = 0;
                    PlayRing.Maximum = audioPlayer.FileReader.TotalTime.Ticks;
                    PlayRing.Value = audioPlayer.CurrentTime.Ticks;
                }
            }
        }
        #endregion

        #region Enable Window Backdrop
        public static ApplicationTheme ActualTheme = ApplicationTheme.Dark;
        static ElementTheme requestedTheme = ElementTheme.Default;
        public static ElementTheme RequestedTheme
        {
            get => requestedTheme;
            set
            {
                requestedTheme = value;

                switch (value)
                {
                    case ElementTheme.Dark: ActualTheme = ApplicationTheme.Dark; break;
                    case ElementTheme.Light: ActualTheme = ApplicationTheme.Light; break;
                    case ElementTheme.Default:
                        var uiSettings = new Windows.UI.ViewManagement.UISettings();
                        var defaultthemecolor = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
                        ActualTheme = defaultthemecolor == Colors.Black ? ApplicationTheme.Dark : ApplicationTheme.Light;
                        break;
                }
            }
        }

        public enum BackdropType
        {
            Mica,
            DesktopAcrylic,
            DefaultColor,
        }

        static WindowsSystemDispatcherQueueHelper m_wsdqHelper;
        static BackdropType m_currentBackdrop;
        static MicaController m_micaController;
        static DesktopAcrylicController m_acrylicController;
        static SystemBackdropConfiguration m_configurationSource;

        public static void SetBackdrop(BackdropType type)
        {
            m_currentBackdrop = BackdropType.DefaultColor;
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
            }
            SWindow.Activated -= Window_Activated;
            SWindow.Closed -= Window_Closed;
            m_configurationSource = null;

            if (type == BackdropType.Mica)
            {
                if (TrySetMicaBackdrop())
                {
                    m_currentBackdrop = type;
                }
                else
                {
                    type = BackdropType.DesktopAcrylic;
                }
            }
            if (type == BackdropType.DesktopAcrylic)
            {
                if (TrySetAcrylicBackdrop())
                {
                    m_currentBackdrop = type;
                }
                else
                {
                }
            }
        }

        static bool TrySetMicaBackdrop()
        {
            if (MicaController.IsSupported())
            {
                m_configurationSource = new SystemBackdropConfiguration();
                SWindow.Activated += Window_Activated;
                SWindow.Closed += Window_Closed;

                m_configurationSource.IsInputActive = true;
                switch (RequestedTheme)
                {
                    case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
                    case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
                    case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
                }

                m_micaController = new MicaController();
                m_micaController.AddSystemBackdropTarget(SWindow.As<ICompositionSupportsSystemBackdrop>());
                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
                isAcrylicBackdrop = false;
                return true;
            }

            return false;
        }

        static bool isAcrylicBackdrop = false;
        static bool TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
                m_configurationSource = new SystemBackdropConfiguration();
                SWindow.Activated += Window_Activated;
                SWindow.Closed += Window_Closed;

                m_configurationSource.IsInputActive = true;
                RequestedTheme = App.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : default;
                switch (RequestedTheme)
                {
                    case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
                    case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
                    case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
                }

                m_acrylicController = new DesktopAcrylicController();
                if (RequestedTheme == ElementTheme.Dark)
                {
                    m_acrylicController.LuminosityOpacity = 1f;
                    m_acrylicController.TintOpacity = 0.5f;
                    m_acrylicController.TintColor = Color.FromArgb(255, 32, 32, 32);
                }
                else
                {
                    m_acrylicController.LuminosityOpacity = 1f;
                    m_acrylicController.TintOpacity = 0.5f;
                    m_acrylicController.TintColor = Color.FromArgb(255, 245, 245, 245);
                }

                m_acrylicController.AddSystemBackdropTarget(SWindow.As<ICompositionSupportsSystemBackdrop>());
                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                isAcrylicBackdrop = true;
                return true;
            }

            return false;
        }
        #endregion

        #region Window Events
        private void WindowGridBase_Loaded(object sender, RoutedEventArgs e)
        {
            PlayingListBaseView.ItemsSource = App.playingList.NowPlayingList;
        }

        private void WindowGridBase_ActualThemeChanged(FrameworkElement sender, object args)
        {
            InitializeTitleBar(App.Current.RequestedTheme);
            if (isAcrylicBackdrop)
            {
                SetBackdrop(BackdropType.DesktopAcrylic);
            }
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == WindowActivationState.PointerActivated ||
                args.WindowActivationState == WindowActivationState.CodeActivated)
            {
                if (!CodeHelper.IsIconic(App.AppWindowLocalHandle))
                {
                    if (!InOpenMusicPage)
                    {
                        PlayingList_NowPlayingImageLoaded(App.playingList.NowPlayingImage);
                    }
                    else
                    {
                        SMusicPage.MusicPageViewStateChange(MusicPageViewState.View);
                    }
                }
            }
            else
            {
                if (CodeHelper.IsIconic(App.AppWindowLocalHandle))
                    SMusicPage.MusicPageViewStateChange(MusicPageViewState.Hidden);
            }
        }

        private static void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (m_currentBackdrop != BackdropType.DesktopAcrylic)
            {
                m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            }

        }

        private static void Window_Closed(object sender, WindowEventArgs args)
        {
            /*
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
            }*/
            SWindow.Activated -= Window_Activated;
            m_configurationSource = null;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetDragRegionForCustomTitleBar(App.AppWindowLocal);

            try
            {
                PlayingListBaseGrid.Height = TopControlsBaseGrid.ActualHeight - 172;
                PlayingListBaseGrid.Width = 400;
                PlayingListBaseGrid.MaxHeight = 800;
            }
            catch { }
        }

        private void ContentFrame_Loaded(object sender, RoutedEventArgs e)
        {
            SContentFrame = ContentFrame;
        }
        #endregion

        #region Window MinSize Setter
        private delegate IntPtr WinProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);
        private WinProc newWndProc = null;
        private IntPtr oldWndProc = IntPtr.Zero;
        [DllImport("user32")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, PInvoke.User32.WindowLongIndexFlags nIndex, WinProc newProc);
        [DllImport("user32.dll")]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam);

        private void SubClassing()
        {
            //Get the Window's HWND
            var hwnd = this.As<IWindowNative>().WindowHandle;

            newWndProc = new WinProc(NewWindowProc);
            oldWndProc = SetWindowLong(hwnd, PInvoke.User32.WindowLongIndexFlags.GWL_WNDPROC, newWndProc);
        }

        int MinWidth = 460;
        int MinHeight = 460;

        [StructLayout(LayoutKind.Sequential)]
        struct MINMAXINFO
        {
            public PInvoke.POINT ptReserved;
            public PInvoke.POINT ptMaxSize;
            public PInvoke.POINT ptMaxPosition;
            public PInvoke.POINT ptMinTrackSize;
            public PInvoke.POINT ptMaxTrackSize;
        }

        private IntPtr NewWindowProc(IntPtr hWnd, PInvoke.User32.WindowMessage Msg, IntPtr wParam, IntPtr lParam)
        {
            switch (Msg)
            {
                case PInvoke.User32.WindowMessage.WM_GETMINMAXINFO:
                    var dpi = PInvoke.User32.GetDpiForWindow(hWnd);
                    float scalingFactor = (float)dpi / 96;

                    MINMAXINFO minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                    minMaxInfo.ptMinTrackSize.x = (int)(MinWidth * scalingFactor);
                    minMaxInfo.ptMinTrackSize.y = (int)(MinHeight * scalingFactor);
                    Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;

            }
            return CallWindowProc(oldWndProc, hWnd, Msg, wParam, lParam);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("EECDBF0E-BAE9-4CB6-A68E-9598E1CB57BB")]
        internal interface IWindowNative
        {
            IntPtr WindowHandle { get; }
        }
        #endregion

        #region Animate Icon Events
        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //AnimatedIcon.SetState(this.BackAnimatedIcon, "PointerOver");
        }

        private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //AnimatedIcon.SetState(this.BackAnimatedIcon, "Normal");
        }

        private void SetDragRegionForCustomTitleBar(AppWindow appWindow)
        {
            // Check to see if customization is supported.
            // Currently only supported on Windows 11.
            if (AppWindowTitleBar.IsCustomizationSupported()
                && appWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                double scaleAdjustment = CodeHelper.GetScaleAdjustment(this);
                double rpc = appWindow.TitleBar.RightInset / scaleAdjustment;
                double lpc = appWindow.TitleBar.LeftInset / scaleAdjustment;

                //RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);
                //LeftPaddingColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scaleAdjustment);

                List<RectInt32> dragRectsList = new();

                RectInt32 dragRectL;
                dragRectL.X = (int)(lpc * scaleAdjustment);
                dragRectL.Y = 0;
                dragRectL.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                dragRectL.Width = (int)((2 + lpc) * scaleAdjustment);
                dragRectsList.Add(dragRectL);

                RectInt32 dragRectR;
                // TOWAIT: when microsoft fix this winui3 bug
                dragRectR.X = (int)((lpc + 2 + (NavView.DisplayMode == NavigationViewDisplayMode.Minimal ? 84 * scaleAdjustment : 42 * scaleAdjustment)));
                dragRectR.Y = 0;
                dragRectR.Height = (int)(AppTitleBar.ActualHeight * scaleAdjustment);
                dragRectR.Width = (int)(rpc * scaleAdjustment * App.AppWindowLocal.Size.Width);
                dragRectsList.Add(dragRectR);

                RectInt32[] dragRects = dragRectsList.ToArray();

                appWindow.TitleBar.SetDragRectangles(dragRects);
            }
        }
        #endregion

        #region NavView Events
        public void UpdataNavViewContentBaseRGClip()
        {
            NavViewContentBase_RGClip.Rect = new Windows.Foundation.Rect(0, 0,
                NavViewContentBase.ActualWidth,
                App.AppWindowLocal.Size.Height - AppTitleBar.ActualHeight);
        }

        public static void SetNavViewContent(Type type, object param = null, NavigationTransitionInfo navigationTransitionInfo = null)
        {
            if (navigationTransitionInfo == null) navigationTransitionInfo = new DrillInNavigationTransitionInfo();
            SContentFrame.Navigate(type, param, navigationTransitionInfo);
            SNavView.IsBackEnabled = SContentFrame.CanGoBack;
            UpdataNavViewSelectedItem(true);
            /*if (type == typeof(ItemListView))
            {
                SNavView.SelectedItem = null;
            }*/
        }

        public static NavigationView SNavView;
        public static Frame SContentFrame;
        public static bool IsBackRequest = false;
        private async void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (IsBackRequest || sender.SelectedItem == null || IsJustUpdata)
            {
                return;
            }

            switch ((sender.SelectedItem as NavigationViewItem).Content)
            {
                case "搜索":
                    SetNavViewContent(typeof(SearchPage));
                    break;
                case "推荐":
                    SetNavViewContent(typeof(RecommendPage));
                    break;
                case "浏览":
                    SetNavViewContent(typeof(BrowsePage));
                    break;
                case "下载":
                    SetNavViewContent(typeof(DownloadPage));
                    break;
                case "播放列表":
                    SetNavViewContent(typeof(PlayListPage));
                    break;
                case "历史":
                    SetNavViewContent(typeof(HistoryPage));
                    break;
                case "设置":
                    SetNavViewContent(typeof(SettingPage));
                    break;
                default:
                    await ShowDialog("未添加此功能", $"未添加 \"{(sender.SelectedItem as NavigationViewItem).Content}\" 功能。");
                    break;
            }

            if (ContentFrame.CanGoBack) NavView.IsBackEnabled = true;
            else NavView.IsBackEnabled = false;
        }

        static bool IsJustUpdata = false;
        public static async void UpdataNavViewSelectedItem(bool justUpdata = false)
        {
            if (justUpdata) IsJustUpdata = true;
            switch ((SContentFrame.Content as Page).GetType().ToString().Split('.')[2])
            {
                case "SearchPage":
                    SNavView.SelectedItem = SNavView.MenuItems[1];
                    break;

                case "RecommendPage":
                    SNavView.SelectedItem = SNavView.MenuItems[2];
                    break;

                case "BrowsePage":
                    SNavView.SelectedItem = SNavView.MenuItems[3];
                    break;

                case "DownloadPage":
                    SNavView.SelectedItem = SNavView.MenuItems[5];
                    break;

                case "PlayListPage":
                    SNavView.SelectedItem = SNavView.MenuItems[6];
                    break;

                case "HistoryPage":
                    SNavView.SelectedItem = SNavView.MenuItems[7];
                    break;

                case "SettingPage":
                    SNavView.SelectedItem = SNavView.SettingsItem;
                    break;

                case "ItemListView":
                    SNavView.SelectedItem = null;
                    break;

                case "EmptyPage":
                    TryGoBack();
                    break;

                default:
                    await ShowDialog("未添加此功能", $"未添加 \"{"未知"}\" 功能。");
                    break;
            }
            IsJustUpdata = false;
        }

        public static bool TryGoBack()
        {
            if (!SContentFrame.CanGoBack)
                return false;

            IsBackRequest = true;
            SContentFrame.GoBack();
            UpdataNavViewSelectedItem();
            SNavView.IsBackEnabled = SContentFrame.CanGoBack;

            IsBackRequest = false;
            return true;
        }

        private void NavView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            TryGoBack();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            TryGoBack();
        }

        private void NavView_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            if (sender.DisplayMode == NavigationViewDisplayMode.Minimal)
            {
                AppTitleBar.Margin = new Thickness(90, 0, 0, 0);
            }
            else
            {
                AppTitleBar.Margin = new Thickness(50, 0, 0, 0);
            }
        }

        private void NavViewContentBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdataNavViewContentBaseRGClip();
        }

        private void NavView_PaneOpened(NavigationView sender, object args)
        {

        }
        #endregion

        #region Bottom Buttons Events
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.audioPlayer.NowOutObj?.PlaybackState == PlaybackState.Playing)
            {
                App.audioPlayer.SetPause();
            }
            else
            {
                App.audioPlayer.SetPlay();
            }
        }

        private async void PlayBeforeButton_Click(object sender, RoutedEventArgs e)
        {
            await App.playingList.PlayPrevious();
        }

        private async void PlayNextButton_Click(object sender, RoutedEventArgs e)
        {
            await App.playingList.PlayNext();
        }

        // VolumeButton
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenOrCloseVolume();
        }

        // PlayingList
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            OpenOrClosePlayingList();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenOrCloseMusicPage();
        }

        public static void OpenOrCloseVolume()
        {
            teachingTipVolume.IsOpen = !teachingTipVolume.IsOpen;
            teachingTipVolume.IsLightDismissEnabled = true;
        }

        public static void OpenOrClosePlayingList()
        {
            teachingTipPlayingList.IsOpen = !teachingTipPlayingList.IsOpen;
            teachingTipPlayingList.IsLightDismissEnabled = true;
        }

        public static bool InOpenMusicPage { get; set; } = false;
        public static void OpenOrCloseMusicPage()
        {
            if (App.audioPlayer.MusicData == null) return;

            if (!InOpenMusicPage)
            {
                InOpenMusicPage = true;
                SMusicPageBaseFrame.Content = SMusicPage;
                SMusicPageBaseFrame.Visibility = Visibility.Visible;
                AnimateHelper.AnimateOffset(
                    SMusicPageBaseFrame,
                    0, 0, 0, 0.76,
                    0.2f, 1f, 0.22f, 1f,
                    out Visual contentGridVisual, out Compositor compositor, out Vector3KeyFrameAnimation animation);
                contentGridVisual.StartAnimation(nameof(contentGridVisual.Offset), animation);
                compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed += (_, __) =>
                {
                    if (InOpenMusicPage)
                    {
                        SWindowGridBase.Visibility = Visibility.Collapsed;
                        System.Diagnostics.Debug.WriteLine("Collapsed");
                    }
                };

                SMusicPage.MusicPageViewStateChange(MusicPageViewState.View);
            }
            else
            {
                InOpenMusicPage = false;
                SWindowGridBase.Visibility = Visibility.Visible;
                System.Diagnostics.Debug.WriteLine("Visible");
                SMusicPageBaseFrame.Content = SMusicPage;
                AnimateHelper.AnimateOffset(
                    SMusicPageBaseFrame,
                    0, (float)SMusicPageBaseGrid.ActualHeight, 0, 0.42,
                    1f, 0f, 1f, 1f,
                    out Visual contentGridVisual, out Compositor compositor, out Vector3KeyFrameAnimation animation);
                contentGridVisual.StartAnimation(nameof(contentGridVisual.Offset), animation);
                compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed += (_, __) =>
                {
                    if (!InOpenMusicPage)
                    {
                        SMusicPageBaseFrame.Visibility = Visibility.Collapsed;
                    }
                };

                SMusicPage.MusicPageViewStateChange(MusicPageViewState.Hidden);
                PlayingList_NowPlayingImageLoaded(App.playingList.NowPlayingImage);
            }
        }
        #endregion

        #region Key Events
        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
        }

        private void Grid_KeyUp(object sender, KeyRoutedEventArgs e)
        {
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as UIElement).Properties.IsXButton1Pressed)
            {
                TryGoBack();
            }
            else if (e.GetCurrentPoint(sender as UIElement).Properties.IsXButton2Pressed)
            {
                if (ContentFrame.CanGoForward)
                {
                    IsBackRequest = true;
                    ContentFrame.GoForward();
                    UpdataNavViewSelectedItem();
                    SNavView.IsBackEnabled = SContentFrame.CanGoBack;
                    IsBackRequest = false;
                }
            }
        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

        }

        public delegate void DriveInTypeDelegate(Microsoft.UI.Input.PointerDeviceType deviceType);
        public static event DriveInTypeDelegate DriveInTypeEvent;
        public static Microsoft.UI.Input.PointerDeviceType DriveInType = Microsoft.UI.Input.PointerDeviceType.Mouse;
        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            bool a = false;

            if (e.PointerDeviceType != DriveInType)
                a = true;

            DriveInType = e.PointerDeviceType;

            if (a)
                DriveInTypeEvent?.Invoke(DriveInType);
        }
        #endregion

        #region Top Controls Events
        bool isPEntered;
        private void VolumeSlider_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            isPEntered = true;
        }

        private void VolumeSlider_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            isPEntered = false;
        }

        private void VolumeSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (isPEntered)
            {
                App.audioPlayer.Volume = (float)(sender as Slider).Value / (float)100.0;
            }
        }
        #endregion

        #region Volume Grid Button Events
        // 均衡器按钮点击事件
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await ShowEqualizerDialog();
        }

        public static float NoVolumeValue = 0;
        // 静音按钮点击事件
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (App.audioPlayer.Volume != 0)
            {
                NoVolumeValue = App.audioPlayer.Volume;
                App.audioPlayer.Volume = 0;
            }
            else
            {
                App.audioPlayer.Volume = NoVolumeValue;
            }
        }
        #endregion

        #region PlayingListView Events
        bool inSelectionChange = false;
        private async void PlayingListBaseView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (inSelectionChange) return;

            inSelectionChange = true;
            if (PlayingListBaseView.SelectedItem != null)
            {
                DataEditor.MusicData data = (DataEditor.MusicData)PlayingListBaseView.SelectedItem;
                if (App.playingList.NowPlayingMusicData != data)
                    await App.playingList.Play(data);
            }
            inSelectionChange = false;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            App.playingList.ClearAll();
        }

        public void UpdataPlayingListShyHeader()
        {
            /*
            // 设置header为顶层
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)PlayingListBaseView.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex(headerContainer, 1);

            var scrollViewer = (VisualTreeHelper.GetChild(PlayingListBaseView, 0) as Border).Child as ScrollViewer;

            CompositionPropertySet scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
            Compositor compositor = scrollerPropertySet.Compositor;

            var padingSize = 40;
            // Get the visual that represents our HeaderTextBlock 
            // And define the progress animation string
            var headerVisual = ElementCompositionPreview.GetElementVisual(PlayingListBaseGrid1);
            String progress = $"Clamp(-scroller.Translation.Y / {padingSize}, 0, 1.0)";

            // Shift the header by 50 pixels when scrolling down
            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {padingSize}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            
            Visual textVisual = ElementCompositionPreview.GetElementVisual(HeaderBaseTextBlock);
            Vector3 finalOffset = new Vector3(0, 10, 0);
            var headerOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0,0,0), finalOffset, {progress})");
            headerOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            headerOffsetAnimation.SetVector3Parameter("finalOffset", finalOffset);
            textVisual.StartAnimation(nameof(Visual.Offset), headerOffsetAnimation);
            

            // Logo scale and transform                                          from               to
            var logoHeaderScaleAnimation = compositor.CreateExpressionAnimation("Lerp(Vector2(1,1), Vector2(0.7, 0.7), " + progress + ")");
            logoHeaderScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);

            var logoVisual = ElementCompositionPreview.GetElementVisual(PlayingListBaseTextBlock);
            logoVisual.StartAnimation("Scale.xy", logoHeaderScaleAnimation);

            var logoVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 24, {progress})");
            logoVisualOffsetYAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Offset.Y", logoVisualOffsetYAnimation);

            var logoVisualOffsetXAnimation = compositor.CreateExpressionAnimation($"Lerp(0, -12, {progress})");
            logoVisualOffsetXAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Offset.X", logoVisualOffsetXAnimation);
            
            var stackVisual = ElementCompositionPreview.GetElementVisual(PlayingListBaseStackPanel);
            var stackVisualOffsetXAnimation = compositor.CreateExpressionAnimation($"Lerp(144, 330, {progress})");
            stackVisualOffsetXAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            stackVisual.StartAnimation("Offset.X", stackVisualOffsetXAnimation);

            var stackVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp(12, 20, {progress})");
            stackVisualOffsetYAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            stackVisual.StartAnimation("Offset.Y", stackVisualOffsetYAnimation);

            var backgroundVisual = ElementCompositionPreview.GetElementVisual(PlayingListBaseRectangle);
            var backgroundVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            backgroundVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            backgroundVisual.StartAnimation("Opacity", backgroundVisualOpacityAnimation);
            */
        }
        #endregion

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            //Meting4Net.Core.Meting meting = new(Meting4Net.Core.ServerProvider.Netease);
            //meting.Cookie("_ntes_nnid=e0720c34928112f57e4337b387e75a50,1654241864468; _ntes_nuid=e0720c34928112f57e4337b387e75a50; NMTID=00O84chCJ4UB0byzElkifyX9WkE3ZcAAAGBKH-xBw; WEVNSM=1.0.0; WNMCID=ujjiij.1654241864835.01.0; WM_TID=SWn5%2BrsBpSdEQBQERBeFA13BO%2BveMgTU; ntes_kaola_ad=1; _iuqxldmzr_=32; WM_NI=XQozOcYzcaCpDN93g4Dj0bPG2wp69WO6yTvymRLrerar51JwsGXjp3%2BTLqVGJk1UAav7LRkHXpt%2B2c4Cm2qxWgL1BX4I54KHEMgdDaPP9Qj%2Bh%2FkxAPZkpby32jBX5JoidnY%3D; WM_NIKE=9ca17ae2e6ffcda170e2e6eed9cc3b91b79f82cb80b2928aa6d85f839f8ab1d149919fa1b5c17b90a800a8db2af0fea7c3b92a9c8fa19bce699ba6ad82c27c8892f7d4e64088b60083d34df2a9c0d7bc5d8deab7b5f573af8e8493e87aa3aefbd2c97db199988bf339e98c8492d54d8ab5ffb6c46bb6e984b3ea449cb0bcccb169f89da3d3f47495b984d2c154f897fbd5ca448793a985c76b9490f7b2e921acf1a599c45c9ab5c083fc39f2b68eb9b842adb7ac8fdc37e2a3; JSESSIONID-WYYY=nbnnzUzR1Ap%2FRJpYqrIns7reQEmyC%2Bpu%2Bg9mQ8nDHajNFTsJkwrGyG5wXKQ1Xy4mkruApNERuC0V3vHukX%5Cmwx%5CEZ8%2FDvH2OjVwCGF1NP7acYy%5C5p%2F4%5CfRksIrli38UzUIsYGvMKeYc5PwmD3ZuqPqZMRw4EOUPX%2BRhd%5CHvhupvEAZrx%3A1659080893357");
            //string jstr = meting.FormatMethod().Lyric("1441758494");
            //System.Diagnostics.Debug.WriteLine(jstr);
        }

        #region PlayTimePopup
        private void PlayButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            InitPlayTimePopup();
        }

        private void PlayButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            InitPlayTimePopup();
        }

        private void InitPlayTimePopup()
        {
            PlayTimeSliderBasePopup.IsOpen = true;
            PlayTimeSliderBasePopup.IsLightDismissEnabled = true;
            PlayTimeSliderBasePopup.Closed += PlayTimeSliderBasePopup_Closed;
            App.audioPlayer.TimingChanged += AudioPlayer_TimingChanged1;
            PlayTimeSlider.ValueChanged += PlayTimeSlider_ValueChanged;
        }

        private void PlayTimeSliderBasePopup_Closed(TeachingTip sender, TeachingTipClosedEventArgs args)
        {
            PlayTimeSliderBasePopup.Closed -= PlayTimeSliderBasePopup_Closed;
            App.audioPlayer.TimingChanged -= AudioPlayer_TimingChanged1;
            PlayTimeSlider.ValueChanged -= PlayTimeSlider_ValueChanged;
        }

        private void AudioPlayer_TimingChanged1(Media.AudioPlayer audioPlayer)
        {
            if (audioPlayer.FileReader != null)
            {
                PlayTimeSlider.Minimum = 0;
                PlayTimeSlider.Maximum = audioPlayer.FileReader.TotalTime.Ticks;

                isCodeChangedSilderValue = true;
                PlayTimeSlider.Value = audioPlayer.CurrentTime.Ticks;
                isCodeChangedSilderValue = false;

                PlayTimeTextBlock.Text =
                        $"{audioPlayer.CurrentTime:mm\\:ss}/{audioPlayer.FileReader.TotalTime:mm\\:ss}";
            }
        }

        bool isCodeChangedSilderValue = false;
        private void PlayTimeSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!isCodeChangedSilderValue && App.audioPlayer.FileReader != null)
            {
                App.audioPlayer.CurrentTime = TimeSpan.FromTicks((long)PlayTimeSlider.Value);
            }
        }
        #endregion
    }
}
