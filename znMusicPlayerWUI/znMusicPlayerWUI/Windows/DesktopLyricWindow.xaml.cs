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

namespace znMusicPlayerWUI.Windowed
{
    public sealed partial class DesktopLyricWindow : Window
    {
        public AppWindow AppWindow { get; private set; }

        public DesktopLyricWindow()
        {
            InitializeComponent();
            AppWindow = WindowHelper.GetAppWindowForCurrentWindow(this);

            var a = OverlappedPresenter.Create();
            a.IsMaximizable = false;
            a.IsMinimizable = false;
            a.IsAlwaysOnTop = true;
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                AppWindow.SetPresenter(AppWindowPresenterKind.CompactOverlay);
                AppWindow.SetPresenter(a);
                AppWindow.IsShownInSwitchers = false;
                AppWindow.Title = this.ToString();
                AppWindow.SetIcon(null);
                AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                AppWindow.TitleBar.ForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.InactiveForegroundColor = Colors.Transparent;
                AppWindow.TitleBar.ButtonInactiveForegroundColor = Colors.Transparent;
                AppWindow.Resize(new SizeInt32() { Width = 400, Height = 100 });
                PointInt32 pointInt32 = new(
                    App.AppWindowLocal.Position.X + App.AppWindowLocal.Size.Width - AppWindow.Size.Width,
                    App.AppWindowLocal.Position.Y + App.AppWindowLocal.Size.Height - AppWindow.Size.Height);
                AppWindow.Move(pointInt32);
            }
            TrySetAcrylicBackdrop();

            App.lyricManager.PlayingLyricSourceChange += LyricManager_PlayingLyricSourceChange;
            App.lyricManager.PlayingLyricSelectedChange += LyricManager_PlayingLyricSelectedChange;
            LyricManager_PlayingLyricSelectedChange(App.lyricManager.NowLyricsData);
        }

        private void LyricManager_PlayingLyricSourceChange(ObservableCollection<LyricData> nowPlayingLyrics)
        {
            LyricManager_PlayingLyricSelectedChange(nowPlayingLyrics[0]);
        }

        bool IsT1Focus = true;
        private void LyricManager_PlayingLyricSelectedChange(LyricData nowLyricsData)
        {
            if (nowLyricsData == null) return;
            if (nowLyricsData.Translate != null)
            {
                IsT1Focus = true;
                V1.HorizontalAlignment = HorizontalAlignment.Center;
                V2.HorizontalAlignment = HorizontalAlignment.Center;
                T1.Text = nowLyricsData.Lyric;
                T2.Text = nowLyricsData.Translate;
                T1.Foreground = App.Current.Resources["AccentAAFillColorTertiaryBrush"] as SolidColorBrush;
                T2.Foreground = App.Current.Resources["AccentAAFillColorTertiaryBrush"] as SolidColorBrush;
            }
            else
            {
                V1.HorizontalAlignment = HorizontalAlignment.Left;
                V2.HorizontalAlignment = HorizontalAlignment.Right;
                LyricData nextData = new(null, null, TimeSpan.Zero);
                try
                {
                    nextData = App.lyricManager.NowPlayingLyrics[App.lyricManager.NowPlayingLyrics.IndexOf(nowLyricsData) + 1];
                }
                catch { }
                if (IsT1Focus)
                {
                    IsT1Focus = false;
                    T1.Text = nowLyricsData.Lyric;
                    T2.Text = nextData.Lyric;
                    T1.Foreground = App.Current.Resources["AccentAAFillColorTertiaryBrush"] as SolidColorBrush;
                    T2.Foreground = App.Current.Resources["MusicPageLrcForeground"] as SolidColorBrush;
                }
                else
                {
                    IsT1Focus = true;
                    T1.Text = nextData.Lyric;
                    T2.Text = nowLyricsData.Lyric;
                    T1.Foreground = App.Current.Resources["MusicPageLrcForeground"] as SolidColorBrush;
                    T2.Foreground = App.Current.Resources["AccentAAFillColorTertiaryBrush"] as SolidColorBrush;
                }
            }
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            double dpi = CodeHelper.GetScaleAdjustment(this);
            RectInt32[] rectInt32s = { new(0, 0, (int)(AppWindow.Size.Width * dpi), (int)(AppWindow.Size.Height * dpi)) };
            AppWindow.TitleBar.SetDragRectangles(rectInt32s);
        }

        #region Enable Window Backdrop
        SystemBackdropConfiguration m_configurationSource = new SystemBackdropConfiguration();
        DesktopAcrylicController m_acrylicController = new DesktopAcrylicController()
        {
            TintColor = Color.FromArgb(255, 35, 35, 35),
            LuminosityOpacity = 0.8f,
            TintOpacity = 0f,
            FallbackColor = Color.FromArgb(255, 40, 40, 40)
        };

        bool TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
                
                this.Activated += DesktopLyricWindow_Activated; ;
                this.Closed += DesktopLyricWindow_Closed;

                m_configurationSource.IsInputActive = true;
                m_acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                return true;
            }

            return false;
        }

        private void DesktopLyricWindow_Closed(object sender, WindowEventArgs args)
        {
            m_acrylicController.Dispose();
            App.lyricManager.PlayingLyricSourceChange -= LyricManager_PlayingLyricSourceChange;
            App.lyricManager.PlayingLyricSelectedChange -= LyricManager_PlayingLyricSelectedChange;
        }

        private void DesktopLyricWindow_Activated(object sender, WindowActivatedEventArgs args)
        {

        }
        #endregion
    }
}
