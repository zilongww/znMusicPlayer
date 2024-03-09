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
using znMusicPlayerWUI.Media;
using static Vanara.PInvoke.User32;
using System.Diagnostics.Eventing.Reader;

namespace znMusicPlayerWUI.Windowed
{
    public enum LyricTextPosition { Default, Left, Right, Center }
    public enum LyricTranslateTextPosition { Center, Left, Right }
    public enum LyricTextBehavior { Exchange, MainLyric, NextLyric, OnlyMainLyric }
    public enum LyricTranslateTextBehavior { MainLyric, TranslateLyric, OnlyMainLyric, OnlyTranslate }
    public sealed partial class DesktopLyricWindow : Window
    {
        public OverlappedPresenter overlappedPresenter { get; private set; }
        private SUBCLASSPROC subClassProc;
        bool transparent = true;
        public static double LyricOpacity { get; set; } = 1.0;
        public static bool PauseButtonVisible { get; set; } = true;
        public static bool ProgressUIVisible { get; set; } = true;
        public static bool ProgressUIPercentageVisible { get; set; } = true;
        public static bool MusicChangeUIVisible { get; set; } = true;
        public static LyricTextPosition LyricTextPosition { get; set; } = LyricTextPosition.Default;
        public static LyricTranslateTextPosition LyricTranslateTextPosition { get; set; } = LyricTranslateTextPosition.Center;
        public static LyricTextBehavior LyricTextBehavior { get; set; } = LyricTextBehavior.Exchange;
        public static LyricTranslateTextBehavior LyricTranslateTextBehavior { get; set; } = LyricTranslateTextBehavior.MainLyric;

        public DesktopLyricWindow()
        {
            InitializeComponent();
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
                AppWindow.Title = "DesktopLyric Window";
                AppWindow.SetIcon("icon.ico");
                AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.InactiveForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonInactiveForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;

                var dpi = CodeHelper.GetScaleAdjustment(this);
                if (IsMoved)
                {
                    AppWindow.Move(lastWindowPosition);
                    AppWindow.Resize(lastWindowSize);
                }
                else
                {
                    AppWindow.Resize(new SizeInt32() { Width = (int)(450 * dpi), Height = (int)(100 * dpi) });
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
            //SystemBackdrop = new DesktopAcrylicBackdrop();
            AddEvents();
            AppWindow.Closing += AppWindow_Closing;
        }

        private void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
        {
            RemoveEvents();
        }

        public void AddEvents()
        {
            System.Diagnostics.Debug.WriteLine("[DesktopLyricWindow]: Add Events.");
            App.audioPlayer.SourceChanged += AudioPlayer_SourceChanged;
            App.audioPlayer.PlayStateChanged += AudioPlayer_PlayStateChanged;
            App.audioPlayer.VolumeChanged += AudioPlayer_VolumeChanged;
            App.audioPlayer.TimingChanged += AudioPlayer_TimingChanged;
            AudioPlayer_PlayStateChanged(App.audioPlayer);
            App.audioPlayer.ReCallTiming();
            SetLyricOpacity(LyricOpacity);
        }

        public void RemoveEvents()
        {
            System.Diagnostics.Debug.WriteLine("[DesktopLyricWindow]: Removed Events.");
            App.audioPlayer.SourceChanged -= AudioPlayer_SourceChanged;
            App.audioPlayer.PlayStateChanged -= AudioPlayer_PlayStateChanged;
            App.audioPlayer.VolumeChanged -= AudioPlayer_VolumeChanged;
            App.audioPlayer.TimingChanged -= AudioPlayer_TimingChanged;
        }

        public void SetLyricOpacity(double value)
        {
            root.Opacity = value;
        }

        int showBorderCount = 0;
        bool isAddedEvent = false;
        private async void AudioPlayer_PlayStateChanged(Media.AudioPlayer audioPlayer)
        {
            PlayStateElement.PlaybackState = audioPlayer.PlaybackState;
            if (audioPlayer.PlaybackState == PlaybackState.Playing)
            {
                InfoBorder.Opacity = 0;

                if (!isAddedEvent)
                {
                    isAddedEvent = true;
                    App.lyricManager.PlayingLyricSourceChange += LyricManager_PlayingLyricSourceChange;
                    App.lyricManager.PlayingLyricSelectedChange += LyricManager_PlayingLyricSelectedChange;
                    App.lyricManager.StartTimer();
                    LyricManager_PlayingLyricSelectedChange(App.lyricManager.NowLyricsData);
                }
            }
            else
            {
                if (PauseButtonVisible) InfoBorder.Opacity = 1;
                isAddedEvent = false;
                App.lyricManager.PlayingLyricSourceChange -= LyricManager_PlayingLyricSourceChange;
                App.lyricManager.PlayingLyricSelectedChange -= LyricManager_PlayingLyricSelectedChange;
            }
        }

        private void AudioPlayer_VolumeChanged(Media.AudioPlayer audioPlayer, object data)
        {

            ShowInfo($"音量：{Math.Round(audioPlayer.Volume)}");
        }

        private void AudioPlayer_SourceChanged(Media.AudioPlayer audioPlayer)
        {
            if (!MusicChangeUIVisible) return;
            ShowInfo($"正在播放：{audioPlayer.MusicData.Title}");
        }

        int showCount = 0;
        private async void ShowInfo(string text)
        {
            InfoTBBorder.Opacity = 1;
            InfoTB.Text = text;

            showCount++;
            await Task.Delay(5000);
            showCount--;

            if (showCount <= 0) InfoTBBorder.Opacity = 0;
        }

        private void SetLyric(LyricData nowLyricsData, bool isNext = false)
        {
            T11.Text = null;
            T21.Text = null;

            // nice coding
            if (nowLyricsData == null)
            {
                if (App.audioPlayer.MusicData != null)
                {
                    T1.Text = App.audioPlayer.MusicData.Title;
                    T2.Text = App.audioPlayer.MusicData.ButtonName;
                }
                T1.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;

                if (LyricTranslateTextPosition == LyricTranslateTextPosition.Left)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Left;
                    V2.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else if (LyricTranslateTextPosition == LyricTranslateTextPosition.Right)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Right;
                    V2.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Center;
                    V2.HorizontalAlignment = HorizontalAlignment.Center;
                }
                return;
            }
            if (nowLyricsData.Lyric == null)
            {
                T1.Text = App.audioPlayer.MusicData.Title;
                T2.Text = App.audioPlayer.MusicData.ButtonName;
                T1.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;

                if (LyricTranslateTextPosition == LyricTranslateTextPosition.Left)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Left;
                    V2.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else if (LyricTranslateTextPosition == LyricTranslateTextPosition.Right)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Right;
                    V2.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Center;
                    V2.HorizontalAlignment = HorizontalAlignment.Center;
                }
                return;
            }
            if (nowLyricsData.Lyric.First() == LyricHelper.NoneLyricString)
            {
                if (App.lyricManager.NowPlayingLyrics.Any())
                {
                    var index = App.lyricManager.NowPlayingLyrics.IndexOf(nowLyricsData) + 1;
                    if (index > App.lyricManager.NowPlayingLyrics.Count - 1) return;
                    SetLyric(App.lyricManager.NowPlayingLyrics[index], true);
                }
                return;
            }

            if (nowLyricsData?.Lyric.Count > 1)
            {
                IsT1Focus = true;

                int tcount = 1;
                int num = App.lyricManager.NowPlayingLyrics.IndexOf(nowLyricsData);
                try
                {
                    while (nowLyricsData?.Lyric?.FirstOrDefault() == App.lyricManager.NowPlayingLyrics[num + tcount]?.Lyric?.FirstOrDefault())
                    {
                        tcount++;
                    }
                }
                catch { }

                bool RomajiEnable = !string.IsNullOrEmpty(nowLyricsData.Romaji);
                TR1DSP.Visibility = Visibility.Visible;
                TR2DSP.Visibility = Visibility.Visible;
                TR1.Text = null;
                TR2.Text = null;

                string t1text = nowLyricsData?.Lyric?.FirstOrDefault();
                if (LyricTranslateTextPosition == LyricTranslateTextPosition.Center)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Center;
                    V2.HorizontalAlignment = HorizontalAlignment.Center;
                    TR1DSP.HorizontalAlignment = HorizontalAlignment.Center;
                    TR2DSP.HorizontalAlignment = HorizontalAlignment.Center;
                    //progressRoot.HorizontalAlignment = HorizontalAlignment.Center;
                }
                else if (LyricTranslateTextPosition == LyricTranslateTextPosition.Left)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Left;
                    V2.HorizontalAlignment = HorizontalAlignment.Left;
                    TR1DSP.HorizontalAlignment = HorizontalAlignment.Left;
                    TR2DSP.HorizontalAlignment = HorizontalAlignment.Left;
                    //progressRoot.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Right;
                    V2.HorizontalAlignment = HorizontalAlignment.Right;
                    TR1DSP.HorizontalAlignment = HorizontalAlignment.Right;
                    TR2DSP.HorizontalAlignment = HorizontalAlignment.Right;
                    //progressRoot.HorizontalAlignment = HorizontalAlignment.Right;
                }
                if (LyricTranslateTextBehavior == LyricTranslateTextBehavior.MainLyric)
                {
                    if (tcount == 1) T11.Text = null;
                    else T11.Text = $"x{tcount}";

                    if (RomajiEnable)
                    {
                        TR1DSP.Visibility = Visibility.Visible;
                        TR1.Text = nowLyricsData.Romaji;
                    }

                    T1.Text = t1text;
                    T2.Text = nowLyricsData?.Lyric[1];
                }
                else if (LyricTranslateTextBehavior == LyricTranslateTextBehavior.TranslateLyric)
                {
                    if (tcount == 1) T21.Text = null;
                    else T21.Text = $"x{tcount}";

                    if (RomajiEnable)
                    {
                        TR2DSP.Visibility = Visibility.Visible;
                        TR2.Text = nowLyricsData.Romaji;
                    }

                    T1.Text = nowLyricsData?.Lyric[1];
                    T2.Text = t1text;
                }
                else if (LyricTranslateTextBehavior == LyricTranslateTextBehavior.OnlyMainLyric)
                {
                    if (tcount == 1) T11.Text = null;
                    else T11.Text = $"x{tcount}";

                    if (RomajiEnable)
                    {
                        TR1DSP.Visibility = Visibility.Visible;
                        TR1.Text = nowLyricsData.Romaji;
                    }

                    T1.Text = t1text;
                    T2.Text = null;
                }
                else
                {
                    if (tcount == 1) T11.Text = null;
                    else T11.Text = $"x{tcount}";

                    T1.Text = nowLyricsData?.Lyric[1];
                    T2.Text = null;
                }

                if (!isNext)
                {
                    T1.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                    T2.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                }
                else
                {
                    T1.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                    T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                }
            }
            else
            {
                if (LyricTextPosition == LyricTextPosition.Default)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Left;
                    V2.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else if (LyricTextPosition == LyricTextPosition.Left)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Left;
                    V2.HorizontalAlignment = HorizontalAlignment.Left;
                }
                else if (LyricTextPosition == LyricTextPosition.Right)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Right;
                    V2.HorizontalAlignment = HorizontalAlignment.Right;
                }
                else if (LyricTextPosition == LyricTextPosition.Center)
                {
                    V1.HorizontalAlignment = HorizontalAlignment.Center;
                    V2.HorizontalAlignment = HorizontalAlignment.Center;
                }

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
                try
                {
                    while (nowLyricsData?.Lyric?.FirstOrDefault() == App.lyricManager.NowPlayingLyrics[num1 + tcount]?.Lyric?.FirstOrDefault())
                    {
                        tcount++;
                    }
                }
                catch { }
                string t1text = nowLyricsData?.Lyric?.FirstOrDefault();
                string t2text = nextData?.Lyric?.FirstOrDefault();

                if (LyricTextBehavior == LyricTextBehavior.Exchange)
                {
                    if (IsT1Focus)
                    {
                        T1.Text = t1text;
                        if (isNext)
                        {
                            T1.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                            T2.Foreground = root.Resources["LrcSecondForeground"] as SolidColorBrush;
                        }
                        else
                        {
                            IsT1Focus = false;
                            T1.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                            T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                        }

                        if (nextData.Lyric != null) T2.Text = t2text;
                        else T2.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                    }
                    else
                    {
                        T2.Text = t1text;

                        T1.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                        if (isNext)
                        {
                            T1.Foreground = root.Resources["LrcSecondForeground"] as SolidColorBrush;
                            T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                        }
                        else
                        {
                            IsT1Focus = true;
                            T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                            T2.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                        }

                        if (nextData.Lyric != null) T1.Text = t2text;
                        else T1.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                    }
                }
                else if (LyricTextBehavior == LyricTextBehavior.MainLyric)
                {
                    T1.Text = t1text;
                    T2.Text = t2text;
                    if (isNext)
                        T1.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                    else
                        T1.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                    T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                }
                else if (LyricTextBehavior == LyricTextBehavior.NextLyric)
                {
                    T1.Text = t2text;
                    T2.Text = t1text;
                    if (isNext)
                        T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                    else
                        T2.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                    T1.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                }
                else if (LyricTextBehavior == LyricTextBehavior.OnlyMainLyric)
                {
                    T1.Text = t1text;
                    T2.Text = null;
                    if (isNext)
                        T1.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                    else
                        T1.Foreground = root.Resources["AccentLrcForeground"] as SolidColorBrush;
                    T2.Foreground = root.Resources["LrcForeground"] as SolidColorBrush;
                }
            }

            TR1DSP.Visibility = Visibility.Collapsed;
            TR2DSP.Visibility = Visibility.Collapsed;
        }

        private void AudioPlayer_TimingChanged(AudioPlayer audioPlayer)
        {
            if (!ProgressUIVisible)
            {
                App.audioPlayer.TimingChanged -= AudioPlayer_TimingChanged;
                progressRoot.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                progressRoot.Visibility = Visibility.Visible;
            }

            progressBase.Maximum = audioPlayer.TotalTime.Ticks;
            progressBase.Value = audioPlayer.CurrentTime.Ticks;
            if (ProgressUIPercentageVisible)
            {
                progressPresent.Visibility = Visibility.Visible;
                progressPresent.Text = $"{Math.Round(audioPlayer.CurrentTime / audioPlayer.TotalTime * 100)}%";
            }
            else
            {
                progressPresent.Visibility = Visibility.Collapsed;
            }
        }

        private void LyricManager_PlayingLyricSourceChange(ObservableCollection<LyricData> nowPlayingLyrics)
        {
            if (nowPlayingLyrics.Any())
                LyricManager_PlayingLyricSelectedChange(nowPlayingLyrics[0]);
        }

        bool IsT1Focus = true;
        private void LyricManager_PlayingLyricSelectedChange(LyricData nowLyricsData)
        {
            SetLyric(nowLyricsData);
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {

        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateDragSize();
        }

        static bool IsMoved = false;
        static PointInt32 lastWindowPosition = default;
        static SizeInt32 lastWindowSize = default;

        public bool IsLock = false;
        public void Lock()
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

                UpdateDragSize();
                SizeInt32 sizeInt32 = new(AppWindow.Size.Width - 1, AppWindow.Size.Height);
                SizeInt32 sizeInt32_1 = new(AppWindow.Size.Width + 1, AppWindow.Size.Height);
                AppWindow.Resize(sizeInt32);
                AppWindow.Resize(sizeInt32_1);

                ShowInfo("关闭桌面歌词后会解除锁定状态");
            }
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            Lock();
        }

        public void UpdateDragSize()
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
                this.Activated += DesktopLyricWindow_Activated;
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

        private void root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateDragSize();
            progressRoot.Width = root.ActualWidth / 4;
        }
    }
}
