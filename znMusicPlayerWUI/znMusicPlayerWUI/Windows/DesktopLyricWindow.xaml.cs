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
using Windows.Devices.Input;
using znMusicPlayerWUI.DataEditor;
using PInvoke;

namespace znMusicPlayerWUI.Windowed
{
    public sealed partial class DesktopLyricWindow : Window
    {
        public AppWindow AppWindow { get; private set; }
        public OverlappedPresenter overlappedPresenter { get; private set; }
        private SUBCLASSPROC subClassProc;
        bool transparent = true;

        public DesktopLyricWindow()
        {
            InitializeComponent();
            AppWindow = WindowHelperzn.WindowHelper.GetAppWindowForCurrentWindow(this);
            /*
            WindowHelper.Window.hWnd = WindowHelper.Window.GetHWnd(this);
            WindowHelper.Window.MakeTransparent();*/

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                overlappedPresenter = OverlappedPresenter.Create();
                overlappedPresenter.IsMaximizable = false;
                overlappedPresenter.IsMinimizable = false;
                overlappedPresenter.IsAlwaysOnTop = true;
                //AppWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
                AppWindow.SetPresenter(overlappedPresenter);
                AppWindow.IsShownInSwitchers = false;
                AppWindow.Title = "Desktop Lyric Window";
                AppWindow.SetIcon(null);
                AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.InactiveForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonInactiveForegroundColor = Colors.Transparent;

                if (IsMoved)
                {
                    AppWindow.Move(lastWindowPosition);
                    AppWindow.Resize(lastWindowSize);
                }
                else
                {
                    AppWindow.Resize(new SizeInt32() { Width = 450, Height = 100 });
                    if (!MainWindow.isMinSize)
                    {
                        PointInt32 pointInt32 = new(
                            App.AppWindowLocal.Position.X + App.AppWindowLocal.Size.Width - AppWindow.Size.Width,
                            App.AppWindowLocal.Position.Y + App.AppWindowLocal.Size.Height - AppWindow.Size.Height);
                        AppWindow.Move(pointInt32);
                    }
                }
            }
            TrySetAcrylicBackdrop();

            App.lyricManager.PlayingLyricSourceChange += LyricManager_PlayingLyricSourceChange;
            App.lyricManager.PlayingLyricSelectedChange += LyricManager_PlayingLyricSelectedChange;
            App.audioPlayer.SourceChanged += AudioPlayer_SourceChanged;
            App.audioPlayer.PlayStateChanged += AudioPlayer_PlayStateChanged;
            LyricManager_PlayingLyricSelectedChange(App.lyricManager.NowLyricsData);
        }

        int showBorderCount = 0;
        private async void AudioPlayer_PlayStateChanged(Media.AudioPlayer audioPlayer)
        {
            PlayStateElement.PlaybackState = audioPlayer.PlaybackState;
            if (audioPlayer.PlaybackState == PlaybackState.Playing)
            {
                InfoBorder.Opacity = 0;
            }
            else
            {
                InfoBorder.Opacity = 1;
            }
        }

        int showCount = 0;
        private async void AudioPlayer_SourceChanged(Media.AudioPlayer audioPlayer)
        {
            InfoTB.Opacity = 1;
            InfoTB.Text = $"正在播放：{audioPlayer.MusicData.Title}";

            showCount++;
            await Task.Delay(5000);
            showCount--;

            if (showCount <= 0) InfoTB.Opacity = 0;
        }

        private void LyricManager_PlayingLyricSourceChange(ObservableCollection<LyricData> nowPlayingLyrics)
        {
            if (nowPlayingLyrics.Any())
                LyricManager_PlayingLyricSelectedChange(nowPlayingLyrics[0]);
        }

        bool IsT1Focus = true;
        private void LyricManager_PlayingLyricSelectedChange(LyricData nowLyricsData)
        {
            if (nowLyricsData == null)
            {
                T1.Text = App.AppName;
                T2.Text = "无歌词";
                V1.HorizontalAlignment = HorizontalAlignment.Center;
                V2.HorizontalAlignment = HorizontalAlignment.Center;
                return;
            }
            if (nowLyricsData.Lyric == null)
            {
                T1.Text = App.AppName;
                T2.Text = "无歌词";
                V1.HorizontalAlignment = HorizontalAlignment.Center;
                V2.HorizontalAlignment = HorizontalAlignment.Center;
                return;
            }

            if (nowLyricsData?.Lyric.FirstOrDefault() == LyricHelper.NoneLyricString) return;
            if (nowLyricsData?.Lyric.Count > 1)
            {
                IsT1Focus = true;

                int tcount = 1;
                int num = App.lyricManager.NowPlayingLyrics.IndexOf(nowLyricsData);
                while (nowLyricsData?.Lyric?.FirstOrDefault() == App.lyricManager.NowPlayingLyrics[num + tcount]?.Lyric?.FirstOrDefault())
                {
                    tcount++;
                }

                string t1text = tcount == 1
                    ? nowLyricsData?.Lyric?.FirstOrDefault()
                    : $"{nowLyricsData?.Lyric?.FirstOrDefault()} (x{tcount})";

                V1.HorizontalAlignment = HorizontalAlignment.Center;
                V2.HorizontalAlignment = HorizontalAlignment.Center;
                T1.Text = t1text;
                T2.Text = nowLyricsData?.Lyric[1];
                T1.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                T2.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
            }
            else
            {
                V1.HorizontalAlignment = HorizontalAlignment.Left;
                V2.HorizontalAlignment = HorizontalAlignment.Right;
                LyricData nextData = new(null, null, TimeSpan.Zero);
                try
                {
                    int num = App.lyricManager.NowPlayingLyrics.IndexOf(nowLyricsData);
                    do
                    {
                        num++;
                        nextData = App.lyricManager.NowPlayingLyrics[num];
                    }
                    while (nextData.Lyric.FirstOrDefault() == LyricHelper.NoneLyricString);
                }
                catch { }

                int tcount = 1;
                int num1 = App.lyricManager.NowPlayingLyrics.IndexOf(nowLyricsData);
                while (nowLyricsData?.Lyric?.FirstOrDefault() == App.lyricManager.NowPlayingLyrics[num1 + tcount]?.Lyric?.FirstOrDefault())
                {
                    tcount++;
                }
                string t1text = tcount == 1
                    ? nowLyricsData?.Lyric?.FirstOrDefault()
                    : $"{nowLyricsData?.Lyric?.FirstOrDefault()} (x{tcount})";
                string t2text = tcount == 1
                    ? nextData?.Lyric?.FirstOrDefault()
                    : $"{nextData?.Lyric?.FirstOrDefault()}{(tcount - 1 == 1 ? "" : $" (x{tcount - 1})")}";

                if (IsT1Focus)
                {
                    IsT1Focus = false;
                    T1.Text = t1text;
                    T1.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                    T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;

                    if (nextData.Lyric != null) T2.Text = t2text;
                    else T2.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                }
                else
                {
                    IsT1Focus = true;
                    T2.Text = t1text;

                    T1.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                    T2.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;

                    if (nextData.Lyric != null) T1.Text = t2text;
                    else T1.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                }
            }
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            UpdataDragSize();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UpdataDragSize();
        }

        static bool IsMoved = false;
        static PointInt32 lastWindowPosition = default;
        static SizeInt32 lastWindowSize = default;

        public bool IsLock = false;
        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsLock)
            {
                IsLock = !IsLock;
                MainWindow.OpenDesktopLyricWindow(false);
                MainWindow.OpenDesktopLyricWindow();
            }
            else
            {
                IsLock = !IsLock;
                overlappedPresenter.IsResizable = false;
                overlappedPresenter.SetBorderAndTitleBar(false, false);
                DisposeAcrylicBackdrop();
                TryTransparentWindow();
                LockButton.Visibility = Visibility.Collapsed;

                UpdataDragSize();
                SizeInt32 sizeInt32 = new(AppWindow.Size.Width - 1, AppWindow.Size.Height);
                SizeInt32 sizeInt32_1 = new(AppWindow.Size.Width + 1, AppWindow.Size.Height);
                AppWindow.Resize(sizeInt32);
                AppWindow.Resize(sizeInt32_1);
            }
        }

        public void UpdataDragSize()
        {
            double dpi = CodeHelper.GetScaleAdjustment(this);
            int windowWidth = (int)(AppWindow.Size.Width * dpi);
            int windowHeight = (int)(AppWindow.Size.Height * dpi);
            int lockButtonWidth = (int)((LockButton.ActualWidth - 1) * dpi);
            int lockButtonHeight = (int)(LockButton.ActualHeight * dpi);

            RectInt32[] rectInt32s = default;
            if (IsLock)
            {
                return;
                rectInt32s = new RectInt32[] {
                    new(0, 0, windowWidth, windowHeight)
                };
            }
            else
            {
                rectInt32s = new RectInt32[] {
                    new(lockButtonWidth, 0, windowWidth - lockButtonWidth, windowHeight),
                    new(0, lockButtonHeight, lockButtonWidth, windowHeight - lockButtonHeight)
                };
            }
            
            AppWindow.TitleBar.SetDragRectangles(rectInt32s);
        }

        #region Enable Window Backdrop
        SystemBackdropConfiguration m_configurationSource = new SystemBackdropConfiguration();
        DesktopAcrylicController m_acrylicController = null;

        bool TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
                this.Activated += DesktopLyricWindow_Activated; ;
                this.Closed += DesktopLyricWindow_Closed;
                
                m_acrylicController = new DesktopAcrylicController()
                {
                    TintColor = Color.FromArgb(255, 35, 35, 35),
                    LuminosityOpacity = 0.8f,
                    TintOpacity = 0f,
                    FallbackColor = Color.FromArgb(255, 40, 40, 40)
                };

                m_configurationSource.IsInputActive = true;
                m_acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                return true;
            }

            return false;
        }

        private void DesktopLyricWindow_Closed(object sender, WindowEventArgs args)
        {
            IsMoved = true;
            lastWindowPosition = AppWindow.Position;
            lastWindowSize = AppWindow.Size;
            DisposeAcrylicBackdrop();
            App.lyricManager.PlayingLyricSourceChange -= LyricManager_PlayingLyricSourceChange;
            App.lyricManager.PlayingLyricSelectedChange -= LyricManager_PlayingLyricSelectedChange;
        }

        private void DisposeAcrylicBackdrop()
        {
            m_acrylicController?.Dispose();
            m_acrylicController = null;
        }

        private void DesktopLyricWindow_Activated(object sender, WindowActivatedEventArgs args)
        {

        }
        #endregion

        #region Enable Transparent Window
        public void TryTransparentWindow()
        {
            subClassProc = new SUBCLASSPROC(SubClassWndProc);
            var windowHandle = new IntPtr((long)this.AppWindow.Id.Value);
            SetWindowSubclass(windowHandle, subClassProc, 0, 0);

            var exStyle = Vanara.PInvoke.User32.GetWindowLongAuto(windowHandle, Vanara.PInvoke.User32.WindowLongFlags.GWL_EXSTYLE).ToInt32();
            if ((exStyle & (int)Vanara.PInvoke.User32.WindowStylesEx.WS_EX_LAYERED) == 0)
            {
                exStyle |= (int)Vanara.PInvoke.User32.WindowStylesEx.WS_EX_LAYERED;
                exStyle |= (int)Vanara.PInvoke.User32.WindowStylesEx.WS_EX_TRANSPARENT;
                Vanara.PInvoke.User32.SetWindowLong(windowHandle, Vanara.PInvoke.User32.WindowLongFlags.GWL_EXSTYLE, exStyle);
                Vanara.PInvoke.User32.SetLayeredWindowAttributes(
                    windowHandle,
                    (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(255, 99, 99, 99)), 255,
                    Vanara.PInvoke.User32.LayeredWindowAttributes.LWA_COLORKEY);
            }
            Helpers.TransparentWindowHelper.TransparentHelper.SetTransparent(this, true);
        }

        private IntPtr SubClassWndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData)
        {
            if (uMsg == (uint)Vanara.PInvoke.User32.WindowMessage.WM_ERASEBKGND)
            {
                if (Vanara.PInvoke.User32.GetClientRect(hWnd, out var rect))
                {
                    using var brush = Vanara.PInvoke.Gdi32.CreateSolidBrush((uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(255, 99, 99, 99)));
                    Vanara.PInvoke.User32.FillRect(wParam, rect, brush);
                    return new IntPtr(1);
                }
            }

            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }

        private delegate IntPtr SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData);

        [DllImport("Comctl32.dll", SetLastError = true)]
        private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("Comctl32.dll", SetLastError = true)]
        private static extern bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, uint dwRefData);
        #endregion
    }
}
