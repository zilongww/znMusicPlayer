using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
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
using Windows.Media.Playback;
using Windows.Media;
using Windows.Storage.Streams;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace znMusicPlayerWUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static readonly MediaPlayer BMP = BackgroundMediaPlayer.Current;
        public static readonly Windows.Media.SystemMediaTransportControls SMTC = BackgroundMediaPlayer.Current?.SystemMediaTransportControls;
        public static readonly MetingServices metingServices = new();
        public static readonly AudioPlayer audioPlayer = new();
        public static readonly PlayingList playingList = new();
        public static readonly LyricManager lyricManager = new();
        public static readonly DownloadManager downloadManager = new();
        public static readonly PlayListReader playListReader = new();

        public static Window WindowLocal;
        public static AppWindow AppWindowLocal;
        public static AppWindow AppDesktopLyricWindow;
        public static OverlappedPresenter AppWindowLocalOverlappedPresenter;
        public static FullScreenPresenter AppWindowLocalFullScreenPresenter;
        public static IntPtr AppWindowLocalHandle;
        public static IntPtr AppDesktopLyricWindowHandle;

        public static readonly string AppName = "znMusicPlayer";
        public static readonly string AppVersion = "0.1.98 Preview";

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            DataFolderBase.InitFiles();
            this.InitializeComponent();
            UnhandledException += App_UnhandledException;

            SMTC.IsPlayEnabled = true;
            SMTC.IsPauseEnabled = true;
            SMTC.IsNextEnabled = true;
            SMTC.IsPreviousEnabled = true;
            SMTC.IsStopEnabled = true;
            SMTC.DisplayUpdater.Type = MediaPlaybackType.Music;
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
                }
                else
                {
                    SMTC.DisplayUpdater.MusicProperties.Title = _.MusicData.Title;
                    SMTC.DisplayUpdater.MusicProperties.Artist = _.MusicData.ButtonName;
                }
                    SMTC.DisplayUpdater.Update();
            };
            playingList.NowPlayingImageLoading += (_, __) =>
            {
                SMTC.DisplayUpdater.Thumbnail = null;
                SMTC.DisplayUpdater.Update();
            };
            playingList.NowPlayingImageLoaded += async (_, __) =>
            {
                if (__ == null) return;
                Debug.WriteLine(__);
                SMTC.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromFile(await StorageFile.GetFileFromPathAsync(__));
                SMTC.DisplayUpdater.Update();
            };

            TaskScheduler.UnobservedTaskException +=
            (object sender, UnobservedTaskExceptionEventArgs excArgs) =>
            {
#if DEBUG
                Debug.WriteLine(excArgs.Exception.ToString());
#endif
            };
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
#if DEBUG
            Debug.WriteLine(e.ToString());
#endif
        }

        public static Microsoft.UI.Xaml.LaunchActivatedEventArgs LAE = null;
        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            WindowLocal = m_window;
            AppWindowLocalHandle = WindowHelperzn.WindowHelper.GetWindowHandle(m_window);
            AppWindowLocalOverlappedPresenter = OverlappedPresenter.Create();
            AppWindowLocalFullScreenPresenter = FullScreenPresenter.Create();
            App.AppWindowLocal.SetPresenter(AppWindowLocalOverlappedPresenter);
            LAE = args;
            /*
                        var l_window = new DesktopLyricWindow();
                        AppDesktopLyricWindowHandle = WindowHelper.GetWindowHandle(l_window);
                        l_window.Activate();
            */
            
            var displayArea = CodeHelper.GetDisplayArea(m_window);
            var dpi = CodeHelper.GetScaleAdjustment(m_window);
            double a = 2;

            CodeHelper.MoveWindow(
                AppWindowLocalHandle,
                (int)((displayArea.WorkArea.Width / a - 1140 / a) * dpi),
                (int)((displayArea.WorkArea.Height / a - 630 / a) * dpi),
                1140 * (int)dpi, 640 * (int)dpi,
                false
                );

            m_window.Activate();
            m_window.Closed += M_window_Closed;
            //AppWindowLocal.SetPresenter(AppWindowLocalPresenter);
        }

        private void M_window_Closed(object sender, WindowEventArgs args)
        {
            audioPlayer.DisposeAll();
            SaveSettings();
        }

        public static void LoadSettings()
        {
            var b = JSettingData;

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
            metingServices.InitMeting();
        }

        public static void SaveSettings()
        {
            var a = JSettingData;
            a[SettingParams.Volume.ToString()] = audioPlayer.Volume == 0 ? MainWindow.NoVolumeValue : audioPlayer.Volume;
            a[SettingParams.EqualizerEnable.ToString()] = audioPlayer.EqEnabled;
            a[SettingParams.EqualizerString.ToString()] = AudioEqualizerBands.GetNameFromBands(audioPlayer.EqualizerBand);
            a[SettingParams.WasapiOnly.ToString()] = audioPlayer.WasapiOnly;
            a[SettingParams.AudioLatency.ToString()] = audioPlayer.Latency < 50 ? 50 : audioPlayer.Latency;
            a[SettingParams.MusicPageShowLyricPage.ToString()] = MainWindow.SMusicPage.ShowLrcPage;

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
            ".m4a", ".m4v", ".mov", ".mp4",
            // SAMI
            ".sami", ".smi",
            // other
            ".wav", ".avi",".ogg", ".flac", ".aiff", ".aif", ".mid"
        };
    }
}
