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
using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using Microsoft.UI.Xaml.Markup;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace znMusicPlayerWUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static readonly AudioPlayer audioPlayer = new();
        public static readonly PlayingList playingList = new();
        public static readonly LyricManager lyricManager = new();
        public static readonly DownloadManager downloadManager = new();

        public static AppWindow AppWindowLocal;
        public static AppWindow AppDesktopLyricWindow;
        public static OverlappedPresenter AppWindowLocalPresenter;
        public static IntPtr AppWindowLocalHandle;
        public static IntPtr AppDesktopLyricWindowHandle;

        public static readonly string AppName = "znMusicPlayer";
        public static readonly string AppVersion = "1.0.0 Beta";

        public static readonly string PlayIconPathData = "M 744.727 551.564 L 325.818 795.927 c -30.2545 18.6182 -69.8182 -4.65454 -69.8182 -39.5636 v -488.727 c 0 -34.9091 39.5636 -58.1818 69.8182 -39.5636 l 418.909 244.364 c 30.2545 16.2909 30.2545 62.8364 0 79.1273 Z";
        public static readonly string PauseIconPathData = "M 442.182 709.818 c 0 37.2364 -30.2545 69.8182 -69.8182 69.8182 s -69.8182 -30.2545 -69.8182 -69.8182 v -395.636 c 0 -37.2364 30.2545 -69.8182 69.8182 -69.8182 s 69.8182 30.2545 69.8182 69.8182 v 395.636 Z m 279.273 0 c 0 37.2364 -30.2545 69.8182 -69.8182 69.8182 s -69.8182 -30.2545 -69.8182 -69.8182 v -395.636 c 0 -37.2364 30.2545 -69.8182 69.8182 -69.8182 s 69.8182 30.2545 69.8182 69.8182 v 395.636 Z";

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            DataFolderBase.InitFiles();
            this.InitializeComponent();
            UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Debug.WriteLine(e.ToString());
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            AppWindowLocalHandle = WindowHelper.GetWindowHandle(m_window);
            AppWindowLocalPresenter = OverlappedPresenter.Create();
            MetingService.InitMeting();

            //l_window = new DesktopLyricWindow();
            //AppDesktopLyricWindowHandle = WindowHelper.GetWindowHandle(l_window);
            //l_window.Activate();

            var displayArea = CodeHelper.GetDisplayArea(m_window);
            var dpi = CodeHelper.GetScaleAdjustment(m_window);
            double a = 2;

            CodeHelper.MoveWindow(
                AppWindowLocalHandle,
                (int)((displayArea.WorkArea.Width / 2 - 1140 / a) / dpi),
                (int)((displayArea.WorkArea.Height / 2 - 630 / a) / dpi),
                1140, 630,
                false
                );

            m_window.Activate();
            m_window.Closed += M_window_Closed;
            AppWindowLocal.SetPresenter(AppWindowLocalPresenter);
        }

        private void M_window_Closed(object sender, WindowEventArgs args)
        {
            audioPlayer.DisposeAll();
            SaveSettings();
        }

        public static void LoadSettings()
        {
            var b = DataFolderBase.JSettingData;

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
            MetingService.NeteaseCookie = (string)b[DataFolderBase.SettingParams.NeteaseMusicCookie.ToString()];
        }

        public static void SaveSettings()
        {
            var a = DataFolderBase.JSettingData;
            a[DataFolderBase.SettingParams.Volume.ToString()] = audioPlayer.Volume == 0 ? MainWindow.NoVolumeValue : audioPlayer.Volume;
            a[DataFolderBase.SettingParams.EqualizerEnable.ToString()] = audioPlayer.EqEnabled;
            a[DataFolderBase.SettingParams.EqualizerString.ToString()] = AudioEqualizerBands.GetNameFromBands(audioPlayer.EqualizerBand);
            a[DataFolderBase.SettingParams.WasapiOnly.ToString()] = audioPlayer.WasapiOnly;
            a[DataFolderBase.SettingParams.AudioLatency.ToString()] = audioPlayer.Latency;
            a[DataFolderBase.SettingParams.MusicPageShowLyricPage.ToString()] = MainWindow.SMusicPage.ShowLrcPage;

            List<float> c = new();
            foreach (var d in AudioEqualizerBands.CustomBands)
            {
                c.Add(d[2]);
            }
            string b = string.Join(",", c.ToArray());
            a[DataFolderBase.SettingParams.EqualizerCustomData.ToString()] = b;
            DataFolderBase.JSettingData = a;
        }

        private Window m_window;
        public static string[] SupportedMediaFormats = new string[] {
            // 3GP
            ".3g2", ".3gp", ".3gp2", ".3gpp",
            // ASF
            ".asf", ".wma", ".wmv",
            // ADTS
            ".aac", ".adts",
            // AVI
            ".avi",
            // MP3
            ".mp3",
            // MPEG-4
            ".m4a", ".m4v", ".mov", ".mp4",
            // SAMI
            ".sami", ".smi",
            // other
            ".wav", ".ogg", ".flac", ".aiff", ".aif"
        };
    }
}
