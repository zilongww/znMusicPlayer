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
        public static readonly CacheManager cacheManager = new();
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
        public static readonly string AppVersion = "0.3.11 Preview";

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
                if (string.IsNullOrEmpty(__))
                {
                    SMTC.DisplayUpdater.Thumbnail = null;
                }
                else
                {
                    try
                    {
                        SMTC.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromFile(await StorageFile.GetFileFromPathAsync(__));
                    }
                    catch { }
                }

                SMTC.DisplayUpdater.Update();
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
            StartingSettings = DataFolderBase.JSettingData;
            var accentcolor = StartingSettings[DataFolderBase.SettingParams.ThemeAccentColor.ToString()];
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
            var width = (int)(1140 * dpi);
            var height = (int)(640 * dpi);
            if (displayArea.WorkArea.Width * dpi <= width ||
                displayArea.WorkArea.Height * dpi <= height)
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

            NotifyIconWindow.HideIcon();
            SaveNowPlaying();
            WindowLocal.Close();
            SMTC.DisplayUpdater.ClearAll();
            SMTC.DisplayUpdater.Update();
            audioPlayer.DisposeAll();
            SaveSettings();
            Exit();
        }

        public static async void ShowErrorDialog()
        {
            MessageDialog messageDialog = new("设置文件出现了一些错误，且程序尝试 5 次后也无法恢复默认配置。\n" +
                "请尝试删除 文档->znMusicPlayerDatas->UserData 里的 Setting 文件。\n" +
                "如果仍然出现问题，请到 GitHub 里向项目提出 Issues。", "znMusicPlayer - 程序无法启动");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(WindowLocal);
            WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, hwnd);
            await messageDialog.ShowAsync();
            App.Current.Exit();
        }

        static bool loadFailed = false;
        static int retryCount = 0;
        public static void LoadSettings(JObject b)
        {
            try
            {
                DataFolderBase.DownloadFolder = (string)b[DataFolderBase.SettingParams.DownloadFolderPath.ToString()];
                DataFolderBase.AudioCacheFolder = (string)b[DataFolderBase.SettingParams.AudioCacheFolderPath.ToString()];
                DataFolderBase.ImageCacheFolder = (string)b[DataFolderBase.SettingParams.ImageCacheFolderPath.ToString()];
                DataFolderBase.LyricCacheFolder = (string)b[DataFolderBase.SettingParams.LyricCacheFolderPath.ToString()];

                var bdata = ((string)b[DataFolderBase.SettingParams.EqualizerCustomData.ToString()]).Split(',');
                for (int i = 0; i < 10; i++)
                {
                    AudioEqualizerBands.CustomBands[i][2] = float.Parse(bdata[i]);
                }

                audioPlayer.Volume = (float)b[DataFolderBase.SettingParams.Volume.ToString()];
                audioPlayer.EqEnabled = (bool)b[DataFolderBase.SettingParams.EqualizerEnable.ToString()];
                audioPlayer.EqualizerBand = AudioEqualizerBands.GetBandFromString((string)b[DataFolderBase.SettingParams.EqualizerString.ToString()]);
                audioPlayer.WasapiOnly = (bool)b[DataFolderBase.SettingParams.WasapiOnly.ToString()];
                audioPlayer.Latency = (int)b[DataFolderBase.SettingParams.AudioLatency.ToString()];
                MainWindow.SMusicPage.ShowLrcPage = (bool)b[DataFolderBase.SettingParams.MusicPageShowLyricPage.ToString()];
                string nmc = "NeteaseMusicCookie";
                if (b.ContainsKey(nmc))
                {
                    metingServices.NeteaseCookie = (string)b[nmc];
                }

                downloadManager.DownloadQuality = (DataFolderBase.DownloadQuality)(int)b[DataFolderBase.SettingParams.DownloadQuality.ToString()];
                downloadManager.DownloadingMaximum = (int)b[DataFolderBase.SettingParams.DownloadMaximum.ToString()];
                downloadManager.DownloadNamedMethod = (DataFolderBase.DownloadNamedMethod)(int)b[DataFolderBase.SettingParams.DownloadNamedMethod.ToString()];
                downloadManager.IDv3WriteImage = (bool)b[DataFolderBase.SettingParams.DownloadOptions.ToString()][0];
                downloadManager.IDv3WriteArtistImage = (bool)b[DataFolderBase.SettingParams.DownloadOptions.ToString()][1];
                downloadManager.IDv3WriteLyric = (bool)b[DataFolderBase.SettingParams.DownloadOptions.ToString()][2];
                downloadManager.SaveLyricToLrcFile = (bool)b[DataFolderBase.SettingParams.DownloadOptions.ToString()][3];

                playingList.PlayBehavior = (PlayBehavior)(int)b[DataFolderBase.SettingParams.PlayBehavior.ToString()];
                playingList.PauseWhenPreviousPause = (bool)b[DataFolderBase.SettingParams.PlayPauseWhenPreviousPause.ToString()];
                playingList.NextWhenPlayError = (bool)b[DataFolderBase.SettingParams.PlayNextWhenPlayError.ToString()];

                MainWindow.SWindowGridBaseTop.RequestedTheme = (ElementTheme)(int)b[DataFolderBase.SettingParams.ThemeColorMode.ToString()];
                MainWindow.SMusicPage.RequestedTheme = (ElementTheme)(int)b[DataFolderBase.SettingParams.ThemeMusicPageColorMode.ToString()];
                MainWindow.m_currentBackdrop = (MainWindow.BackdropType)(int)b[DataFolderBase.SettingParams.ThemeBackdropEffect.ToString()];
                MainWindow.ImagePath = (string)b[DataFolderBase.SettingParams.ThemeBackdropImagePath.ToString()];
                MainWindow.SBackgroundMass.Opacity = (double)b[DataFolderBase.SettingParams.ThemeBackdropImageMassOpacity.ToString()];
                //Accent Color
                
                DesktopLyricWindow.PauseButtonVisible = (bool)b[DataFolderBase.SettingParams.DesktopLyricOptions.ToString()][0];
                DesktopLyricWindow.ProgressUIVisible = (bool)b[DataFolderBase.SettingParams.DesktopLyricOptions.ToString()][1];
                DesktopLyricWindow.ProgressUIPercentageVisible = (bool)b[DataFolderBase.SettingParams.DesktopLyricOptions.ToString()][2];
                DesktopLyricWindow.MusicChangeUIVisible = (bool)b[DataFolderBase.SettingParams.DesktopLyricOptions.ToString()][3];
                DesktopLyricWindow.LyricTextBehavior = (LyricTextBehavior)(int)b[DataFolderBase.SettingParams.DesktopLyricText.ToString()][0];
                DesktopLyricWindow.LyricTextPosition = (LyricTextPosition)(int)b[DataFolderBase.SettingParams.DesktopLyricText.ToString()][1];
                DesktopLyricWindow.LyricTranslateTextBehavior = (LyricTranslateTextBehavior)(int)b[DataFolderBase.SettingParams.DesktopLyricTranslateText.ToString()][0];
                DesktopLyricWindow.LyricTranslateTextPosition = (LyricTranslateTextPosition)(int)b[DataFolderBase.SettingParams.DesktopLyricTranslateText.ToString()][1];

                NotifyIconWindow.IsVisible = (bool)b[DataFolderBase.SettingParams.TaskbarShowIcon.ToString()];
                MainWindow.RunInBackground = (bool)b[DataFolderBase.SettingParams.BackgroundRun.ToString()];
                Controls.Imagezn.ImageDarkMass = (bool)b[DataFolderBase.SettingParams.ImageDarkMass.ToString()];
                LoadLastExitPlayingSongAndSongList = (bool)b[DataFolderBase.SettingParams.LoadLastExitPlayingSongAndSongList.ToString()];

                JArray hkd = (JArray)b[DataFolderBase.SettingParams.HotKeySettings.ToString()];
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
                DataFolderBase.JSettingData = DataFolderBase.SettingDefault;
                LoadSettings(DataFolderBase.JSettingData);
            }
        }

        public static Windows.UI.Color AccentColor = Windows.UI.Color.FromArgb(0, 0, 0, 0);
        public static void SaveSettings()
        {
            var a = DataFolderBase.JSettingData;
            a[DataFolderBase.SettingParams.Volume.ToString()] = audioPlayer.Volume == 0 ? MainWindow.NoVolumeValue : audioPlayer.Volume;
            a[DataFolderBase.SettingParams.DownloadFolderPath.ToString()] = DataFolderBase.DownloadFolder;
            a[DataFolderBase.SettingParams.AudioCacheFolderPath.ToString()] = DataFolderBase.AudioCacheFolder;
            a[DataFolderBase.SettingParams.ImageCacheFolderPath.ToString()] = DataFolderBase.ImageCacheFolder;
            a[DataFolderBase.SettingParams.LyricCacheFolderPath.ToString()] = DataFolderBase.LyricCacheFolder;
            a[DataFolderBase.SettingParams.DownloadOptions.ToString()][0] = downloadManager.IDv3WriteImage;
            a[DataFolderBase.SettingParams.DownloadOptions.ToString()][1] = downloadManager.IDv3WriteArtistImage;
            a[DataFolderBase.SettingParams.DownloadOptions.ToString()][2] = downloadManager.IDv3WriteLyric;
            a[DataFolderBase.SettingParams.DownloadOptions.ToString()][3] = downloadManager.SaveLyricToLrcFile;
            a[DataFolderBase.SettingParams.DownloadNamedMethod.ToString()] = (int)downloadManager.DownloadNamedMethod;
            a[DataFolderBase.SettingParams.DownloadQuality.ToString()] = (int)downloadManager.DownloadQuality;
            a[DataFolderBase.SettingParams.DownloadMaximum.ToString()] = downloadManager.DownloadingMaximum;
            a[DataFolderBase.SettingParams.PlayBehavior.ToString()] = (int)playingList.PlayBehavior;
            a[DataFolderBase.SettingParams.PlayPauseWhenPreviousPause.ToString()] = playingList.PauseWhenPreviousPause;
            a[DataFolderBase.SettingParams.PlayNextWhenPlayError.ToString()] = playingList.NextWhenPlayError;
            a[DataFolderBase.SettingParams.DownloadMaximum.ToString()] = downloadManager.DownloadingMaximum;
            a[DataFolderBase.SettingParams.EqualizerEnable.ToString()] = audioPlayer.EqEnabled;
            a[DataFolderBase.SettingParams.EqualizerString.ToString()] = AudioEqualizerBands.GetNameFromBands(audioPlayer.EqualizerBand);
            a[DataFolderBase.SettingParams.WasapiOnly.ToString()] = audioPlayer.WasapiOnly;
            a[DataFolderBase.SettingParams.AudioLatency.ToString()] = audioPlayer.Latency < 50 ? 50 : audioPlayer.Latency;
            a[DataFolderBase.SettingParams.MusicPageShowLyricPage.ToString()] = MainWindow.SMusicPage.ShowLrcPage;
            a[DataFolderBase.SettingParams.ThemeColorMode.ToString()] = (int)MainWindow.SWindowGridBaseTop.RequestedTheme;
            a[DataFolderBase.SettingParams.ThemeMusicPageColorMode.ToString()] = (int)MainWindow.SMusicPage.pageRoot.RequestedTheme;
            a[DataFolderBase.SettingParams.ThemeBackdropEffect.ToString()] = (int)MainWindow.m_currentBackdrop;
            a[DataFolderBase.SettingParams.ThemeBackdropImagePath.ToString()] = MainWindow.ImagePath;
            a[DataFolderBase.SettingParams.ThemeBackdropImageMassOpacity.ToString()] = MainWindow.SBackgroundMass.Opacity;
            a[DataFolderBase.SettingParams.ThemeAccentColor.ToString()] = AccentColor == Windows.UI.Color.FromArgb(0,0,0,0) ? null : AccentColor.ToString();
            a[DataFolderBase.SettingParams.DesktopLyricOptions.ToString()] = new JArray()
            {
                DesktopLyricWindow.PauseButtonVisible, DesktopLyricWindow.ProgressUIVisible,
                DesktopLyricWindow.ProgressUIPercentageVisible, DesktopLyricWindow.MusicChangeUIVisible
            };
            a[DataFolderBase.SettingParams.DesktopLyricText.ToString()] = new JArray()
            {
                DesktopLyricWindow.LyricTextBehavior,
                DesktopLyricWindow.LyricTextPosition
            };
            a[DataFolderBase.SettingParams.DesktopLyricTranslateText.ToString()] = new JArray()
            {
                DesktopLyricWindow.LyricTranslateTextBehavior,
                DesktopLyricWindow.LyricTranslateTextPosition
            };

            a[DataFolderBase.SettingParams.TaskbarShowIcon.ToString()] = NotifyIconWindow.IsVisible;
            a[DataFolderBase.SettingParams.BackgroundRun.ToString()] = MainWindow.RunInBackground;
            a[DataFolderBase.SettingParams.ImageDarkMass.ToString()] = Controls.Imagezn.ImageDarkMass;
            a[DataFolderBase.SettingParams.LoadLastExitPlayingSongAndSongList.ToString()] = LoadLastExitPlayingSongAndSongList;

            a[DataFolderBase.SettingParams.HotKeySettings.ToString()] = JArray.FromObject(App.hotKeyManager.RegistedHotKeys);

            List<float> c = new();
            foreach (var d in AudioEqualizerBands.CustomBands)
            {
                c.Add(d[2]);
            }
            string b = string.Join(",", c.ToArray());
            a[DataFolderBase.SettingParams.EqualizerCustomData.ToString()] = b;
            DataFolderBase.JSettingData = a;
#if DEBUG
            Debug.WriteLine("[SaveSettingData]: 设置配置已存储！");
#endif
        }

        public static bool LoadLastExitPlayingSongAndSongList = true;
        public static void SaveNowPlaying()
        {
            if (audioPlayer.MusicData is null) return;

            var path = Path.Combine(DataFolderBase.UserDataFolder, "LastPlaying");
            if (!LoadLastExitPlayingSongAndSongList)
            {
                File.Delete(path);
                return;
            }
            
            if (!File.Exists(path)) File.Create(path).Close();

            JArray array = new JArray();
            foreach (var a in (playingList.PlayBehavior == PlayBehavior.随机播放 ? playingList.RandomSavePlayingList : playingList.NowPlayingList))
                array.Add(JObject.FromObject(a));
            JObject jobject = new JObject() {
                { "music", JObject.FromObject(audioPlayer.MusicData) },
                { "list", array }
            };
            File.WriteAllText(path, jobject.ToString());
        }

        public static async void LoadLastPlaying()
        {
            if (!LoadLastExitPlayingSongAndSongList) return;

            var path = Path.Combine(DataFolderBase.UserDataFolder, "LastPlaying");
            if (!File.Exists(path)) return;

            MusicData musicData = null;
            await Task.Run(() =>
            {
                var texts = File.ReadAllText(path);
                JObject jobject = JObject.Parse(texts);
                musicData = JsonNewtonsoft.FromJSON<MusicData>(jobject["music"].ToString());

                foreach(var m in jobject["list"])
                {
                    var md = JsonNewtonsoft.FromJSON<MusicData>(m.ToString());
                    playingList.NowPlayingList.Add(md);
                }
            });

            if (musicData is null) return;
            if (playingList.PlayBehavior == PlayBehavior.随机播放)
            {
                playingList.SetRandomPlay(PlayBehavior.随机播放);
            }
            await playingList.Play(musicData, false);
            audioPlayer.SetPause();
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
