using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using znMusicPlayerWUI.Helpers;
using Microsoft.UI;
using WinRT.Interop;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Media;
using znMusicPlayerWUI.Windowed;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using Microsoft.UI.Xaml.Markup;
using System.Threading.Tasks;
using znMusicPlayerWUI.Background;
using static znMusicPlayerWUI.DataEditor.DataFolderBase;
using Windows.Storage.Streams;
using Windows.Storage;
using znMusicPlayerWUI.Background.HotKeys;
using Newtonsoft.Json.Linq;
using Windows.UI.Popups;
using System.Runtime.InteropServices;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace znMusicPlayerWUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static readonly Windows.Media.Playback.MediaPlayer BMP = Windows.Media.Playback.BackgroundMediaPlayer.Current;
        public static Windows.Media.SystemMediaTransportControls SMTC { get; } = Windows.Media.Playback.BackgroundMediaPlayer.Current?.SystemMediaTransportControls;
        public static readonly MetingServices metingServices = new();
        public static readonly AudioPlayer audioPlayer = new();
        public static readonly PlayingList playingList = new();
        public static readonly LyricManager lyricManager = new();
        public static readonly DownloadManager downloadManager = new();
        public static readonly PlayListReader playListReader = new();
        public static readonly HotKeyManager hotKeyManager = new();

        public static Window WindowLocal;
        public static AppWindow AppWindowLocal;
        public static AppWindow AppDesktopLyricWindow;
        public static OverlappedPresenter AppWindowLocalOverlappedPresenter;
        public static FullScreenPresenter AppWindowLocalFullScreenPresenter;
        public static IntPtr AppWindowLocalHandle;
        public static IntPtr AppDesktopLyricWindowHandle;
        public static NotifyIconWindow NotifyIconWindow;
        public static TaskBarInfoWindow taskBarInfoWindow;

        public static readonly string AppName = "znMusicPlayer";
        public static readonly string AppVersion = "0.2.82 Preview";

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            DataFolderBase.InitFiles();
            //Media.Decoder.FFmpeg.FFmpegBinariesHelper.InitFFmpeg();
            InitializeComponent();
            UnhandledException += App_UnhandledException;

            SMTC.IsPlayEnabled = true;
            SMTC.IsPauseEnabled = true;
            SMTC.IsNextEnabled = true;
            SMTC.IsPreviousEnabled = true;
            SMTC.IsStopEnabled = true;
            SMTC.DisplayUpdater.Type = Windows.Media.MediaPlaybackType.Music;
            SMTC.DisplayUpdater.MusicProperties.Title = AppName;
            SMTC.DisplayUpdater.MusicProperties.Artist = "没有正在播放的歌曲";
            SMTC.DisplayUpdater.Update();

            audioPlayer.CacheLoadingChanged += (_, __) =>
            {
                SMTC.DisplayUpdater.MusicProperties.Title = _.MusicData?.Title;
                SMTC.DisplayUpdater.MusicProperties.Artist = "加载中...";
                SMTC.DisplayUpdater.Update();
            };
            audioPlayer.CacheLoadedChanged += (_) =>
            {
                if (_.MusicData == null)
                {
                    SMTC.DisplayUpdater.MusicProperties.Title = _.FileReader?.FileName;
                    AppWindowLocal.Title = AppName;
                }
                else
                {
                    SMTC.DisplayUpdater.MusicProperties.Title = _.MusicData.Title;
                    SMTC.DisplayUpdater.MusicProperties.Artist = _.MusicData.ButtonName;
                    AppWindowLocal.Title = $"{_.MusicData.Title} - {_.MusicData.ArtistName} · {AppName}";
                }
                SMTC.DisplayUpdater.Update();
            };
            audioPlayer.PlayStateChanged += (_) =>
            {
                if (_.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                {
                    SMTC.PlaybackStatus = Windows.Media.MediaPlaybackStatus.Playing;
                }
                else
                {
                    SMTC.PlaybackStatus = Windows.Media.MediaPlaybackStatus.Paused;
                }
            };
            playingList.NowPlayingImageLoading += (_, __) =>
            {
                SMTC.DisplayUpdater.Thumbnail = null;
                SMTC.DisplayUpdater.Update();
            };
            playingList.NowPlayingImageLoaded += async (_, __) =>
            {
                if (__ == null) return;
                try
                {
                    SMTC.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromFile(await StorageFile.GetFileFromPathAsync(__));
                    SMTC.DisplayUpdater.Update();
                }
                catch { }
            };

            TaskScheduler.UnobservedTaskException +=
                (object sender, UnobservedTaskExceptionEventArgs excArgs) =>
                {
                    LogHelper.WriteLog("UnobservedTaskError", excArgs.Exception.ToString(), false);
    #if DEBUG
                    Debug.WriteLine("UnobservedTaskError: " + excArgs.Exception.ToString());
    #endif
                };
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            LogHelper.WriteLog("UnhandledError", e.Exception.ToString(), false);
#if DEBUG
            Debug.WriteLine("UnhandledError: " + e.ToString());
#endif
        }

        public static Microsoft.UI.Xaml.LaunchActivatedEventArgs LAE = null;
        public static JObject StartingSettings = null;
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            StartingSettings = JSettingData;
            var accentcolor = StartingSettings[SettingParams.ThemeAccentColor.ToString()];
            if (accentcolor != null)
            {
                /*Current.Resources["SystemAccentColor"] = Windows.UI.Color.FromArgb(255, 2,255,2);
                Current.Resources["SystemAccentColorLight2"] = Windows.UI.Color.FromArgb(255, 2, 255, 2);
                Current.Resources["SystemAccentColorDark1"] = Windows.UI.Color.FromArgb(255, 2, 255, 2);*/


                Debug.WriteLine(Current.Resources["SystemAccentColorLight2"].GetType());
            }
            m_window = new MainWindow();
            WindowLocal = m_window;
            AppWindowLocalHandle = WindowHelperzn.WindowHelper.GetWindowHandle(m_window);
            AppWindowLocalOverlappedPresenter = OverlappedPresenter.Create();
            AppWindowLocalFullScreenPresenter = FullScreenPresenter.Create();
            App.AppWindowLocal.SetPresenter(AppWindowLocalOverlappedPresenter);
            LAE = args;
            /*
                        List<Microsoft.WindowsAPICodePack.Taskbar.ThumbnailToolBarButton> buttons = new()
                        {
                            new (null, "上一首"),
                            new(null, "播放/暂停"),
                            new(null, "下一首")
                        };
                        Microsoft.WindowsAPICodePack.Taskbar.TaskbarManager.Instance.ThumbnailToolBars.AddButtons(AppWindowLocalHandle, buttons.ToArray());
            */

            var displayArea = CodeHelper.GetDisplayArea(m_window);
            var dpi = CodeHelper.GetScaleAdjustment(m_window);
            var width = 1140;
            var height = 640;
            if (displayArea.WorkArea.Width <= width ||
                displayArea.WorkArea.Height <= height)
            {
                AppWindowLocalOverlappedPresenter.Maximize();
            }
            else
            {
                AppWindowLocal.MoveAndResize(new(
                    displayArea.WorkArea.Width / 2 - width / 2,
                    displayArea.WorkArea.Height / 2 - height / 2,
                    width, height));
            }

            if (loadFailed)
            {
                ShowErrorDialog();
                return;
            }

            m_window.Activate();
            m_window.Closed += M_window_Closed;
            //AppWindowLocal.SetPresenter(AppWindowLocalPresenter);
            hotKeyManager.Init(App.WindowLocal);
            DelayOpenWindows();
        }

        public async void DelayOpenWindows()
        {
#if DEBUG
            await Task.Delay(1000);
#endif
            await Task.Delay(500);
            taskBarInfoWindow = new();

            // 在 Windows App SDK 1.4 的版本一直闪退，1.3 则不会
            await Task.Delay(1000);
            NotifyIconWindow = new();
        }

        private void M_window_Closed(object sender, WindowEventArgs args)
        {
            if (MainWindow.DesktopLyricWindow != null)
            {
                MainWindow.DesktopLyricWindow.Close();
            }
            WindowLocal.Close();
            SMTC.DisplayUpdater.ClearAll();
            SMTC.DisplayUpdater.Update();
            audioPlayer.DisposeAll();
            SaveSettings();
            Exit();
        }

        static bool loadFailed = false;
        static int retryCount = 0;
        public static void LoadSettings(JObject b)
        {
            try
            {
                DataFolderBase.DownloadFolder = (string)b[SettingParams.DownloadFolderPath.ToString()];
                DataFolderBase.AudioCacheFolder = (string)b[SettingParams.AudioCacheFolderPath.ToString()];
                DataFolderBase.ImageCacheFolder = (string)b[SettingParams.ImageCacheFolderPath.ToString()];
                DataFolderBase.LyricCacheFolder = (string)b[SettingParams.LyricCacheFolderPath.ToString()];

                var bdata = ((string)b[SettingParams.EqualizerCustomData.ToString()]).Split(',');
                for (int i = 0; i < 10; i++)
                {
                    AudioEqualizerBands.CustomBands[i][2] = float.Parse(bdata[i]);
                }

                audioPlayer.Volume = (float)b[SettingParams.Volume.ToString()];
                audioPlayer.EqEnabled = (bool)b[SettingParams.EqualizerEnable.ToString()];
                audioPlayer.EqualizerBand = AudioEqualizerBands.GetBandFromString((string)b[SettingParams.EqualizerString.ToString()]);
                audioPlayer.WasapiOnly = (bool)b[SettingParams.WasapiOnly.ToString()];
                audioPlayer.Latency = (int)b[SettingParams.AudioLatency.ToString()];
                MainWindow.SMusicPage.ShowLrcPage = (bool)b[SettingParams.MusicPageShowLyricPage.ToString()];
                string nmc = "NeteaseMusicCookie";
                if (b.ContainsKey(nmc))
                {
                    metingServices.NeteaseCookie = (string)b[nmc];
                }

                downloadManager.DownloadQuality = (DownloadQuality)(int)b[SettingParams.DownloadQuality.ToString()];
                downloadManager.DownloadingMaximum = (int)b[SettingParams.DownloadMaximum.ToString()];
                downloadManager.DownloadNamedMethod = (DownloadNamedMethod)(int)b[SettingParams.DownloadNamedMethod.ToString()];
                downloadManager.IDv3WriteImage = (bool)b[SettingParams.DownloadOptions.ToString()][0];
                downloadManager.IDv3WriteArtistImage = (bool)b[SettingParams.DownloadOptions.ToString()][1];
                downloadManager.IDv3WriteLyric = (bool)b[SettingParams.DownloadOptions.ToString()][2];
                downloadManager.SaveLyricToLrcFile = (bool)b[SettingParams.DownloadOptions.ToString()][3];

                playingList.PlayBehavior = (PlayBehavior)(int)b[SettingParams.PlayBehavior.ToString()];
                playingList.PauseWhenPreviousPause = (bool)b[SettingParams.PlayPauseWhenPreviousPause.ToString()];
                playingList.NextWhenPlayError = (bool)b[SettingParams.PlayNextWhenPlayError.ToString()];

                MainWindow.SWindowGridBaseTop.RequestedTheme = (ElementTheme)(int)b[SettingParams.ThemeColorMode.ToString()];
                MainWindow.SMusicPage.RequestedTheme = (ElementTheme)(int)b[SettingParams.ThemeMusicPageColorMode.ToString()];
                MainWindow.m_currentBackdrop = (MainWindow.BackdropType)(int)b[SettingParams.ThemeBackdropEffect.ToString()];
                MainWindow.ImagePath = (string)b[SettingParams.ThemeBackdropImagePath.ToString()];
                MainWindow.SBackgroundMass.Opacity = (double)b[SettingParams.ThemeBackdropImageMassOpacity.ToString()];
                //Accent Color
                
                DesktopLyricWindow.PauseButtonVisible = (bool)b[SettingParams.DesktopLyricOptions.ToString()][0];
                DesktopLyricWindow.ProgressUIVisible = (bool)b[SettingParams.DesktopLyricOptions.ToString()][1];
                DesktopLyricWindow.ProgressUIPercentageVisible = (bool)b[SettingParams.DesktopLyricOptions.ToString()][2];
                DesktopLyricWindow.MusicChangeUIVisible = (bool)b[SettingParams.DesktopLyricOptions.ToString()][3];
                DesktopLyricWindow.LyricTextBehavior = (LyricTextBehavior)(int)b[SettingParams.DesktopLyricText.ToString()][0];
                DesktopLyricWindow.LyricTextPosition = (LyricTextPosition)(int)b[SettingParams.DesktopLyricText.ToString()][1];
                DesktopLyricWindow.LyricTranslateTextBehavior = (LyricTranslateTextBehavior)(int)b[SettingParams.DesktopLyricTranslateText.ToString()][0];
                DesktopLyricWindow.LyricTranslateTextPosition = (LyricTranslateTextPosition)(int)b[SettingParams.DesktopLyricTranslateText.ToString()][1];

                JArray hkd = (JArray)b[SettingParams.HotKeySettings.ToString()];
                HotKeyManager.WillRegisterHotKeysList = hkd.ToObject<List<HotKey>>();

                metingServices.InitMeting();
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("SettingError", e.ToString(), false);
                if (retryCount >= 5)
                {
                    loadFailed = true;
                    return;
                }
                retryCount++;
                JSettingData = DataFolderBase.SettingDefault;
                LoadSettings(JSettingData);
            }
        }

        public static async void ShowErrorDialog()
        {
            MessageDialog messageDialog = new("设置文件出现了一些错误，且程序尝试 5 次后也无法恢复默认配置。\n" +
                "请尝试删除 文档->znMusicPlayerDatas->UserData 里的 Setting 文件。\n" +
                "如果仍然出现问题，请到 GitHub 里向项目提出 Issues。", "znMusicPlayerWUI - 程序无法启动");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(WindowLocal);
            WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, hwnd);
            await messageDialog.ShowAsync();
            App.Current.Exit();
        }

        public static Windows.UI.Color AccentColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
        public static void SaveSettings()
        {
            var a = JSettingData;
            a[SettingParams.Volume.ToString()] = audioPlayer.Volume == 0 ? MainWindow.NoVolumeValue : audioPlayer.Volume;
            a[SettingParams.DownloadFolderPath.ToString()] = DataFolderBase.DownloadFolder;
            a[SettingParams.AudioCacheFolderPath.ToString()] = DataFolderBase.AudioCacheFolder;
            a[SettingParams.ImageCacheFolderPath.ToString()] = DataFolderBase.ImageCacheFolder;
            a[SettingParams.LyricCacheFolderPath.ToString()] = DataFolderBase.LyricCacheFolder;
            a[SettingParams.DownloadOptions.ToString()][0] = downloadManager.IDv3WriteImage;
            a[SettingParams.DownloadOptions.ToString()][1] = downloadManager.IDv3WriteArtistImage;
            a[SettingParams.DownloadOptions.ToString()][2] = downloadManager.IDv3WriteLyric;
            a[SettingParams.DownloadOptions.ToString()][3] = downloadManager.SaveLyricToLrcFile;
            a[SettingParams.DownloadNamedMethod.ToString()] = (int)downloadManager.DownloadNamedMethod;
            a[SettingParams.DownloadQuality.ToString()] = (int)downloadManager.DownloadQuality;
            a[SettingParams.DownloadMaximum.ToString()] = downloadManager.DownloadingMaximum;
            a[SettingParams.PlayBehavior.ToString()] = (int)playingList.PlayBehavior;
            a[SettingParams.PlayPauseWhenPreviousPause.ToString()] = playingList.PauseWhenPreviousPause;
            a[SettingParams.PlayNextWhenPlayError.ToString()] = playingList.NextWhenPlayError;
            a[SettingParams.DownloadMaximum.ToString()] = downloadManager.DownloadingMaximum;
            a[SettingParams.EqualizerEnable.ToString()] = audioPlayer.EqEnabled;
            a[SettingParams.EqualizerString.ToString()] = AudioEqualizerBands.GetNameFromBands(audioPlayer.EqualizerBand);
            a[SettingParams.WasapiOnly.ToString()] = audioPlayer.WasapiOnly;
            a[SettingParams.AudioLatency.ToString()] = audioPlayer.Latency < 50 ? 50 : audioPlayer.Latency;
            a[SettingParams.MusicPageShowLyricPage.ToString()] = MainWindow.SMusicPage.ShowLrcPage;
            a[SettingParams.ThemeColorMode.ToString()] = (int)MainWindow.SWindowGridBaseTop.RequestedTheme;
            a[SettingParams.ThemeMusicPageColorMode.ToString()] = (int)MainWindow.SMusicPage.pageRoot.RequestedTheme;
            a[SettingParams.ThemeBackdropEffect.ToString()] = (int)MainWindow.m_currentBackdrop;
            a[SettingParams.ThemeBackdropImagePath.ToString()] = MainWindow.ImagePath;
            a[SettingParams.ThemeBackdropImageMassOpacity.ToString()] = MainWindow.SBackgroundMass.Opacity;
            a[SettingParams.ThemeAccentColor.ToString()] = AccentColor == Windows.UI.Color.FromArgb(0,0,0,0) ? null : AccentColor.ToString();
            a[SettingParams.DesktopLyricOptions.ToString()] = new JArray()
            {
                DesktopLyricWindow.PauseButtonVisible, DesktopLyricWindow.ProgressUIVisible,
                DesktopLyricWindow.ProgressUIPercentageVisible, DesktopLyricWindow.MusicChangeUIVisible
            };
            a[SettingParams.DesktopLyricText.ToString()] = new JArray()
            {
                DesktopLyricWindow.LyricTextBehavior,
                DesktopLyricWindow.LyricTextPosition
            };
            a[SettingParams.DesktopLyricTranslateText.ToString()] = new JArray()
            {
                DesktopLyricWindow.LyricTranslateTextBehavior,
                DesktopLyricWindow.LyricTranslateTextPosition
            };
            a[SettingParams.HotKeySettings.ToString()] = JArray.FromObject(App.hotKeyManager.RegistedHotKeys);

            List<float> c = new();
            foreach (var d in AudioEqualizerBands.CustomBands)
            {
                c.Add(d[2]);
            }
            string b = string.Join(",", c.ToArray());
            a[SettingParams.EqualizerCustomData.ToString()] = b;
            JSettingData = a;
#if DEBUG
            Debug.WriteLine("设置配置已存储！");
#endif
        }

        private Window m_window;
        public static string[] SupportedMediaFormats = new string[] {
            // 3GP
            ".3g2", ".3gp", ".3gp2", ".3gpp",
            // ASF
            ".asf", ".wma", ".wmv",
            // ADTS
            ".aac", ".adts",
            // MP3
            ".mp3",
            // MPEG-4
            ".m4a", ".m4v", ".mov", ".mp4", ".mkv",
            // SAMI
            ".sami", ".smi",
            // other
            ".wav", ".ogg", ".flac", ".aiff", ".aif", ".mid", ".cue", ".dts"
        };
    }
}
