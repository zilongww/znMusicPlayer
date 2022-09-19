using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using zilongcn;
using znMusicPlayerWPF.MusicPlay;
using znMusicPlayerWPF.Pages;

namespace znMusicPlayerWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// write by zilongcn
    /// github: zilongcn23
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string BaseName = App.BaseName;
        public static string BaseVersion = App.BaseVersion;//System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static bool Animate = true;
        public static bool SystemBlur = true;

        public enum SleepTaskEndDos { PauseMusic, ExitSoftware, Logout, ShutDown }
        public struct SleepTask
        {
            public TimeSpan SleepTaskTime { get; set; }
            public SleepTaskEndDos SleepTaskEndDo { get; set; }
            public bool PlayEndToDo { get; set; }
            public bool SleepTaskOpen { get; set; }
            public SleepTask(TimeSpan sleepTaskTime, SleepTaskEndDos sleepTaskEndDo, bool playEndToDo, bool sleepTaskOpen) : this()
            {
                SleepTaskTime = sleepTaskTime;
                SleepTaskEndDo = sleepTaskEndDo;
                PlayEndToDo = playEndToDo;
                SleepTaskOpen = sleepTaskOpen;
            }
        }

        public bool SleepTaskEndPauseMusic = false;
        public bool IsInSleepTask = false;
        public SleepTask NowSleepTask = new SleepTask();
        public async void SleepTaskStart(SleepTask sleepTask)
        {
            NowSleepTask = sleepTask;
            if (IsInSleepTask) return;
            IsInSleepTask = true;
            while (NowSleepTask.SleepTaskTime.TotalMinutes > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                if (!NowSleepTask.SleepTaskOpen)
                {
                    SettingPages.SleepBar.Describe = "无定时任务";
                    IsInSleepTask = false;
                    return;
                }
                NowSleepTask.SleepTaskTime = TimeSpan.FromSeconds(NowSleepTask.SleepTaskTime.TotalSeconds - 1);
                SettingPages.SleepBar.Describe = $"剩余 {NowSleepTask.SleepTaskTime.ToString()}";
            }
            IsInSleepTask = false;
            SettingPages.SleepBar.Describe = "定时结束";
            if (NowSleepTask.PlayEndToDo)
            {
                audioPlayer.PlayEndEvent += () =>
                {
                    SleepTaskEndDo();
                };
            }
            else
            {
                SleepTaskEndDo();
            }
        }

        public void SleepTaskEndDo()
        {
            SleepTaskEndPauseMusic = false;
            switch (NowSleepTask.SleepTaskEndDo)
            {
                case SleepTaskEndDos.PauseMusic:
                    if (NowSleepTask.PlayEndToDo) SleepTaskEndPauseMusic = true;
                    audioPlayer.Stop();
                    break;

                case SleepTaskEndDos.ExitSoftware:
                    MenuExit_Click(null, null);
                    break;

                case SleepTaskEndDos.Logout:
                    Exec("shutdown -l");
                    break;

                case SleepTaskEndDos.ShutDown:
                    Exec("shutdown -s");
                    break;
            }
        }
        public void Exec(string str)
        {
            try
            {
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = "cmd.exe";//调用cmd.exe程序
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardInput = true;//重定向标准输入
                    process.StartInfo.RedirectStandardOutput = true;//重定向标准输出
                    process.StartInfo.RedirectStandardError = true;//重定向标准出错
                    process.StartInfo.CreateNoWindow = true;//不显示黑窗口
                    process.Start();//开始调用执行
                    process.StandardInput.WriteLine(str + "&exit");//标准输入str + "&exit"，相等于在cmd黑窗口输入str + "&exit"
                    process.StandardInput.AutoFlush = true;//刷新缓冲流，执行缓冲区的命令，相当于输入命令之后回车执行
                    process.WaitForExit();//等待退出
                    process.Close();//关闭进程
                }
            }
            catch (Exception err)
            {
                ShowBox("err", err.ToString());
            }
        }

        public AudioPlayer audioPlayer = null;

        List<TheMusicDatas.MusicData> MusicList = new List<TheMusicDatas.MusicData>();

        public delegate void WindowStateChangedDelegate();
        public event WindowStateChangedDelegate WindowStateChangedEvent;

        object LastData = null;
        public JObject NowPlayData = null;
        object NowListData = null;
        public string AlbumRid = null;
        public int PSINT = 0;

        public MusicPlayList NowPlayList { get; set; } = new MusicPlayList();

        private bool _DebugsMod = false;
        public object NowPage = null;
        public ThumbButtonInfo TheTaskbarPlayButton = new ThumbButtonInfo();
        public IntPtr Handle = IntPtr.Zero;
        public MainPage MainPages = null;
        public ListPage ListPages = null;
        public LoadPage LoadingPage = null;
        public MusicPage MusicPages = null;
        public AlbumPage AlbumPages = null;
        public Setting SettingPages = null;
        public SetMusicTagPage SetMusicPage = null;
        public DownloadPage TheDownload = null;
        public AboutPage TheAbout = null;
        public SearchPage TheSearchPage = null;
        public MusicPlayListContent MusicPlayListContent = null;

        public DesktopLrcWindow LrcWindow = null;

        public MyC.PopupWindow MainMenuPopup = new MyC.PopupWindow() { isWindowSmallRound = false, IsShowActivated = true, IsDeActivityClose = true, CloseExit = false, ForceAcrylicBlur = true };
        public MyC.PopupContent.MainMenu Popups = null;
        public MyC.PopupWindow VolumePopup = new MyC.PopupWindow() { isWindowSmallRound = false, IsShowActivated = true, IsDeActivityClose = true, CloseExit = false, ForceAcrylicBlur = true };
        public MyC.PopupContent.VolumeContent VolumeContent = null;

        public Source TheSource = null;
        public SongDataEdit TheUserDataEdit = null;
        public MusicPlayMod MusicPlayMod = null;
        public int Blurs
        {
            get { return int.Parse(SettingDataEdit.GetParam("BlurRadius")); }
            set
            {
                SettingDataEdit.SetParam("BlurRadius", value.ToString());
            }
        }
        public string FileAddress = BaseFoldsPath.DownloadFolderPath;
        public RenderingBias quality = RenderingBias.Performance;

        public bool SoftwareVisual
        {
            get { return bool.Parse(SettingDataEdit.GetParam("SoftwareVisual")); }
            set
            {
                SettingDataEdit.SetParam("SoftwareVisual", value.ToString());
                if (value) RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
                else RenderOptions.ProcessRenderMode = RenderMode.Default;
            }
        }

        public bool LrcWindowTaskBarShow
        {
            get { return bool.Parse(SettingDataEdit.GetParam("LrcWindowTaskbarShow")); }
            set
            {
                SettingDataEdit.SetParam("LrcWindowTaskbarShow", value.ToString());
                LrcWindow.ShowInTaskbar = value;
            }
        }

        public bool Animation
        {
            get { return Animate; }
            set { Animate = value; }
        }

        public bool IsBarKeyDown = false;
        private PlayState _NowPlayState = PlayState.Pause;
        public enum PlayState { Play, Pause };

        private System.Windows.Forms.NotifyIcon MessageIcon = new System.Windows.Forms.NotifyIcon();

        private bool _IsFullScreen = false;
        private bool _IsMaxScreen = false;
        private TheMusicDatas.MusicData _NowPlaySong = new TheMusicDatas.MusicData();
        public MusicPlayCache musicCache = null;

        public delegate void ThemeChangeDelegate(ThemeData themeData);
        public event ThemeChangeDelegate ThemeChangeEvent;
        public delegate void BlurThemeChangeDelegate(BlurThemeData BlurThemeData);
        public event BlurThemeChangeDelegate BlurThemeChangeEvent;

        public double AnimateOpacityNormalTime = 0.23;

        public static readonly ThemeData DefaultThemeData = new ThemeData(
            new SolidColorBrush(Color.FromArgb(255, 255, 133, 133)),
            new SolidColorBrush(Color.FromArgb(255, 255, 77, 77)),
            new SolidColorBrush(Color.FromArgb(255, 249, 249, 249)),
            new SolidColorBrush(Color.FromArgb(255, 66, 66, 66)),
            new SolidColorBrush(Color.FromArgb(255, 141, 141, 141)),
            new SolidColorBrush(Colors.White),
            new SolidColorBrush(Color.FromArgb(45, 0, 0, 0))
            );

        public static readonly BlurThemeData DefaultBlurThemeData = new BlurThemeData(
            new SolidColorBrush(Color.FromArgb(210, 254, 254, 254)),
            new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)),
            new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)),
            new SolidColorBrush(Color.FromArgb(255, 70, 70, 70)),
            new SolidColorBrush(Color.FromArgb(255, 90, 90, 90))
            );

        public static readonly ThemeData DarkThemeData = new ThemeData(
            new SolidColorBrush(Color.FromArgb(70, 90, 90, 90)),
            new SolidColorBrush(Color.FromArgb(255, 67, 67, 67)),
            new SolidColorBrush(Color.FromArgb(255, 39, 39, 39)),
            new SolidColorBrush(Color.FromArgb(255, 170, 170, 170)),
            new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)),
            new SolidColorBrush(Color.FromArgb(255, 200, 200, 200)),
            new SolidColorBrush(Color.FromArgb(45, 255, 255, 255))
            );

        public static readonly BlurThemeData DarkBlurThemeData = new BlurThemeData(
            new SolidColorBrush(Color.FromArgb(150, 60, 60, 60)),
            new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)),
            new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)),
            new SolidColorBrush(Color.FromArgb(255, 230, 230, 230)),
            new SolidColorBrush(Color.FromArgb(255, 210, 210, 210))
            );

        public struct ThemeData
        {
            public Brush ButtonBackColor { get; set; }
            public Brush ButtonEnterColor { get; set; }
            public Brush BackColor { get; set; }
            public Brush TextColor { get; set; }
            public Brush LittleTextColor { get; set; }
            public Brush InColorTextColor { get; set; }
            public Brush ALineColor { get; set; }
            public string MD5 { get; set; }

            public ThemeData(Brush ButtonBackColor,
                Brush ButtonEnterColor,
                Brush BackColor,
                Brush TextColor,
                Brush LittleTextColor,
                Brush InColorTextColor,
                Brush ALineColor
                ) : this()
            {
                this.ButtonBackColor = ButtonBackColor;
                this.ButtonEnterColor = ButtonEnterColor;
                this.BackColor = BackColor;
                this.TextColor = TextColor;
                this.LittleTextColor = LittleTextColor;
                this.InColorTextColor = InColorTextColor;
                this.ALineColor = ALineColor;
                MD5 = zilongcn.Others.ToMD5($"{ButtonBackColor}{ButtonEnterColor}{BackColor}{TextColor}{LittleTextColor}{InColorTextColor}");
            }
        }

        public struct BlurThemeData
        {
            public Brush BackColor { get; set; }
            public Brush ButtonBackColor { get; set; }
            public Brush ButtonEnterColor { get; set; }
            public Brush TextColor { get; set; }
            public Brush InColorTextColor { get; set; }
            public string MD5 { get; set; }

            public BlurThemeData(Brush BackColor,
                Brush ButtonBackColor,
                Brush ButtonEnterColor,
                Brush TextColor,
                Brush InColorTextColor
                ) : this()
            {
                this.BackColor = BackColor;
                this.ButtonBackColor = ButtonBackColor;
                this.ButtonEnterColor = ButtonEnterColor;
                this.TextColor = TextColor;
                this.InColorTextColor = InColorTextColor;
                MD5 = zilongcn.Others.ToMD5($"{ButtonBackColor}{ButtonEnterColor}{BackColor}{TextColor}{InColorTextColor}");
            }
        }

        public ThemeData NowThemeData
        {
            get
            {
                return new ThemeData(
                    FindResource("ButtonPAMP") as SolidColorBrush,
                    FindResource("ButtonPAMM") as SolidColorBrush,
                    FindResource("BackColor") as SolidColorBrush,
                    FindResource("ATextColor") as SolidColorBrush,
                    FindResource("ALittleTextColor") as SolidColorBrush,
                    FindResource("ATextColor_InColor") as SolidColorBrush,
                    FindResource("ALineColor") as SolidColorBrush
                    );
            }
            set
            {
                Resources["ButtonPAMP"] = value.ButtonBackColor;
                Resources["ButtonPAMM"] = value.ButtonEnterColor;
                Resources["BackColor"] = value.BackColor;
                Resources["ATextColor"] = value.TextColor;
                Resources["ALittleTextColor"] = value.LittleTextColor;
                Resources["ATextColor_InColor"] = value.InColorTextColor;
                Resources["ALineColor"] = value.ALineColor;
                if (ThemeChangeEvent != null) ThemeChangeEvent(value);
                ThemeChangeDo();
            }
        }

        public BlurThemeData NowBlurThemeData
        {
            get
            {
                return new BlurThemeData(
                    FindResource("BlurBackColor") as SolidColorBrush,
                    FindResource("ButtonBlurPAMP") as SolidColorBrush,
                    FindResource("ButtonBlurPAMM") as SolidColorBrush,
                    FindResource("ATextColor_InBlur") as SolidColorBrush,
                    FindResource("ATextColor_InColorBlur") as SolidColorBrush
                    );
            }
            set
            {
                Resources["BlurBackColor"] = value.BackColor;
                Resources["ButtonBlurPAMP"] = value.ButtonBackColor;
                Resources["ButtonBlurPAMM"] = value.ButtonEnterColor;
                Resources["ATextColor_InBlur"] = value.TextColor;
                Resources["ATextColor_InColorBlur"] = value.InColorTextColor;
                if (BlurThemeChangeEvent != null) BlurThemeChangeEvent(value);

                /*
                var a = (value.BackColor as SolidColorBrush).Color;
                var b = (FindResource("BlurBackColor") as SolidColorBrush).Color;
                MessageBox.Show($"{a.A},{a.R},{a.G},{a.B}\n{b.A},{b.R},{b.G},{b.B}");*/
            }
        }

        public string DownloadPath
        {
            get { return BaseFoldsPath.DownloadFolderPath; }
            set
            {
                BaseFoldsPath.DownloadFolderPath = value;
                SettingPages.UriDownloadAddress.Describe = value;
                SettingDataEdit.SetParam("DownloadPath", value);
            }
        }

        public string LoadPath
        {
            get { return BaseFoldsPath.SongsFolderPath; }
            set
            {
                BaseFoldsPath.SongsFolderPath = value;
                SettingPages.UriAddress.Describe = value;
                SettingDataEdit.SetParam("LoadPath", value);
            }
        }

        public bool DebugsMod
        {
            get { return _DebugsMod; }
            set
            {
                _DebugsMod = value;
                if (value)
                {
                    win11debugstext.Visibility = Visibility.Collapsed;
                }
                else
                {
                    win11debugstext.Visibility = Visibility.Collapsed;
                }
                //SettingPages.DebugsOCBtn.IsChecked = value;
            }
        }

        double top = 0;
        double left = 0;
        double width = 0;
        double height = 0;
        public bool IsFullScreen
        {
            get { return _IsFullScreen; }
            set
            {
                if (value)
                {
                    if (IsMaxScreen) IsMaxScreen = false;

                    _IsFullScreen = true;

                    this.WindowState = WindowState.Normal;
                    this.ResizeMode = ResizeMode.CanMinimize;
                    windowChrome.ResizeBorderThickness = new Thickness(0);

                    top = Top;
                    left = Left;
                    width = Width;
                    height = Height;

                    Left = 0.0;
                    Top = 0.0;
                    Width = SystemParameters.PrimaryScreenWidth;
                    Height = SystemParameters.PrimaryScreenHeight;

                    FullScreenPath.Data = FindResource("关闭全屏") as Geometry;
                    FullScreenText = "取消全屏";

                    this.WindowStyle = WindowStyle.None;
                }
                else
                {
                    _IsFullScreen = false;

                    this.WindowState = WindowState.Normal;
                    this.ResizeMode = ResizeMode.CanResize;
                    windowChrome.ResizeBorderThickness = new Thickness(5);

                    Width = width;
                    Height = height;
                    Left = left;
                    Top = top;

                    FullScreenPath.Data = FindResource("打开全屏") as Geometry;
                    FullScreenText = "全屏";

                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                }
            }
        }

        public bool IsMaxScreen
        {
            get { return _IsMaxScreen; }
            set
            {
                if (value)
                {
                    if (IsFullScreen) IsFullScreen = false;

                    MainBack.Margin = new Thickness(6);
                    windowChrome.ResizeBorderThickness = new Thickness(0);

                    WindowState = WindowState.Maximized;
                    WindowButton_Max_Path.Data = this.FindResource("窗口化") as Geometry;
                    _IsMaxScreen = true;
                }
                else
                {
                    MainBack.Margin = new Thickness(0);
                    windowChrome.ResizeBorderThickness = new Thickness(5);

                    WindowState = WindowState.Normal;
                    WindowButton_Max_Path.Data = this.FindResource("最大化") as Geometry;
                    _IsMaxScreen = false;
                }
            }
        }

        public delegate void VolumeChangedDelegate(float value);
        public event VolumeChangedDelegate VolumeChangedEvent;

        private float _NowVolume = 0.5f;
        public float NowVolume
        {
            get { return _NowVolume; }
            set
            {
                if (value > 1.0)
                {
                    return;
                }
                if (value < -0.001)
                {
                    return;
                }

                _NowVolume = value;
                if (VolumeChangedEvent != null) VolumeChangedEvent(value);
                if (IsLoadSettings) SettingDataEdit.SetParam("Volume", value.ToString());

                if (value <= 0)
                {
                    audioPlayer.Volume = 0;
                    VolumeTe.Text = "静音";
                    LrcWindow.VolumeTextG.Text = "0";
                    VolumeContent.texts.Text = "音量：静音";
                    VolumeContent.texts.Text = "音量：静音";
                    VolumeContent.icons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
                    VolumeIcons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
                    LrcWindow.VolumeIcons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMute;
                }
                else
                {
                    try
                    {
                        if (audioPlayer == null) return;

                        audioPlayer.Volume = value;

                        VolumeTe.Text = (value * 100).ToString().Split('.')[0];
                        LrcWindow.VolumeTextG.Text = VolumeTe.Text;
                        VolumeSp.Value = value * 100;
                        VolumeContent.texts.Text = $"音量：{VolumeTe.Text}";

                        if (value < 0.34)
                        {
                            VolumeContent.icons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeLow;
                            VolumeIcons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeLow;
                            LrcWindow.VolumeIcons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeLow;
                        }
                        else if (value < 0.67)
                        {
                            VolumeContent.icons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMedium;
                            VolumeIcons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMedium;
                            LrcWindow.VolumeIcons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeMedium;
                        }
                        else if (value < 1.1)
                        {
                            VolumeContent.icons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
                            VolumeIcons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
                            LrcWindow.VolumeIcons.Kind = MaterialDesignThemes.Wpf.PackIconKind.VolumeHigh;
                        }
                    }
                    catch (Exception wee) { ShowBox("ERR", wee.ToString()); }
                }
            }
        }

        public bool WasapiNotShare
        {
            get
            {
                return SettingDataEdit.ToBool(SettingDataEdit.GetParam("WasapiNotShare"));
            }
            set
            {
                SettingDataEdit.SetParam("WasapiNotShare", value.ToString());
                SettingPages.WasapiNotShareOCBtn.IsChecked = value;
                audioPlayer.WasapiNotShare = value;
            }
        }

        private bool _Debug = false;
        public bool Debug
        {
            get { return _Debug; }
            set
            {
                _Debug = value;
            }
        }

        public Brush LrcColor
        {
            get { return SettingPages.eColor.Fill; }
            set
            {
                MusicPages.NowLrcColor = value;
                SettingPages.eColor.Fill = value;
                try { LrcWindow.NowChoiceLrc.Foreground = value; }
                catch { LrcWindow.Lrc1.Foreground = value; }
            }
        }

        public Brush OtherColor
        {
            get { return SettingPages.bColor.Fill; }
            set
            {
                MusicPages.OtherLrcColor = value;
                MusicPages.CleanLrcColor();
                SettingPages.bColor.Fill = value;
                try { LrcWindow.OtherLrc.Foreground = value; }
                catch { LrcWindow.Lrc2.Foreground = value; }
            }
        }

        private bool _IsOpenMenu = false;
        public bool IsOpenMenu
        {
            get
            {
                return _IsOpenMenu;
            }
            set
            {
                if (MainUI.Visibility == Visibility.Collapsed) return;
                _IsOpenMenu = value;

                Animations animations = new Animations(Animation);
                animations.storyboard.Completed += (sender, args) => Menu.IsEnabled = true;
                Menu.IsEnabled = false;

                if (value)
                {
                    animations.animatePosition(TopBar, TopBar.Margin, new Thickness(202, 0, 0, 0), 0.35, 1, 0);
                    animations.animatePosition(CenterPage, CenterPage.Margin, new Thickness(202, 46, 0, 116), 0.35, 1, 0);
                    animations.animateWidth(LeftBar, LeftBar.ActualWidth, 202, 0.35, 1, 0);
                }
                else
                {
                    animations.animatePosition(TopBar, TopBar.Margin, new Thickness(52, 0, 0, 0), 0.35, 1, 0);
                    animations.animatePosition(CenterPage, CenterPage.Margin, new Thickness(52, 46, 0, 116), 0.35, 1, 0);
                    animations.animateWidth(LeftBar, LeftBar.ActualWidth, 52, 0.35, 1, 0);
                }
                animations.Begin();
            }
        }

        public delegate void PlayStateChangeDelegate(PlayState nowPlayState);
        public event PlayStateChangeDelegate PlayStateChangeEvent;

        public PlayState NowPlayState
        {
            get { return _NowPlayState; }
            set
            {
                _NowPlayState = value;
                if (PlayStateChangeEvent != null) PlayStateChangeEvent(value);
            }
        }

        public List<string> 垃圾音乐黑名单 = new List<string>() { "我害怕鬼(其他)", "学猫叫" };

        public TheMusicDatas.MusicData NowPlaySong
        {
            get { return _NowPlaySong; }
            set
            {
                _NowPlaySong = value;
                NowPlayState = PlayState.Pause;
                PlaySet(value, true);
            }
        }

        public string OSVersion = App.OSVersion;
        public string[] TheArgs = null;
        public string FullScreenText = "全屏";

        private Geometry PlayPath = null;
        private Geometry PausePath = null;
        private BitmapImage TaskBarPauseImage = new BitmapImage(new Uri(@"pack://application:,,,/icons/任务栏暂停.png"));
        private BitmapImage TaskBarPlayImage = new BitmapImage(new Uri(@"pack://application:,,,/icons/任务栏播放.png"));

        public MainWindow(string[] Args)
        {
            TheArgs = Args;

            BaseFoldsPath.FirstLoad();

            FontFamily = FindResource("znNormal") as FontFamily;
            PlayPath = FindResource("播放") as Geometry;
            PausePath = FindResource("暂停") as Geometry;

            InitializeComponent();

            IsOpenMenu = false;

            audioPlayer = new AudioPlayer(this);
            audioPlayer.PlayEndEvent += PlayEnd;
            audioPlayer.NowOutDevice = MusicPlay.AudioPlayer.GetOutDevices().First();

            this.Handle = new WindowInteropHelper(this).Handle;
            this.TaskbarItemInfo = new TaskbarItemInfo();
            this.ShowInTaskbar = true;

            TheSource = new Source().Set(this);

            MainPages = new MainPage(this, TheSource);
            ListPages = new ListPage(this, TheSource);
            LoadingPage = new LoadPage(this, TheSource);
            MusicPages = new MusicPage(this, TheSource);
            AlbumPages = new AlbumPage(this, TheSource);
            SettingPages = new Setting(this, TheSource);
            SetMusicPage = new SetMusicTagPage(this, TheSource);
            TheDownload = new DownloadPage(this);
            TheAbout = new AboutPage(this);
            TheSearchPage = new SearchPage(this);
            LrcWindow = new DesktopLrcWindow(this);
            Popups = new MyC.PopupContent.MainMenu(this);
            MusicPlayListContent = new MusicPlayListContent(this);
            VolumeContent = new MyC.PopupContent.VolumeContent(this);
            MainMenuPopup.Content = Popups;
            VolumePopup.Content = VolumeContent;

            this.Title = BaseName;
            Title1.Text = this.Title;
            SetPage("Main");

            PlayMusicSlider.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(Bar_MouseLeftButtonDown), true);
            PlayMusicSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(Bar_MouseLeftButtonUp), true);

            MusicPage2.Content = MusicPages;

            SizeChanged += SizeChangedDo;
            this.Closing += MainWindow_Closing;
            this.Closed += MainWindow_Closed;

            VolumeSp.Maximum = 100;
            VolumeSp.Value = audioPlayer.Volume * 100;
            VolumeTe.Text = (audioPlayer.Volume * 100).ToString().Split('.')[0];

            Uri IconPath = new Uri(TheSource.GetSoftwareAddress() + @"icon.ico");

            if (!File.Exists(IconPath.LocalPath))
                zilongcn.Others.ExtractResFile("znMusicPlayerWPF.icon.ico", IconPath.LocalPath);

            try
            {
                this.Icon = new BitmapImage(IconPath);
                MessageIcon.Icon = new System.Drawing.Icon(IconPath.LocalPath);
                MessageIcon.Visible = true;
                MessageIcon.Text = BaseName;
                MessageIcon.MouseClick += MessageIcon_MouseClick;
                MessageIcon.MouseUp += MessageIcon_MouseUp;
            }
            catch (Exception err)
            {
                TheSource.ShowBox("错误", "icon.ico文件格式不正确或不存在。\n\n错误代码:\n" + err.ToString());
                MenuExit_Click(null, null);
            }

            TheTaskbarPlayButton.ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/icons/任务栏播放.png"));
            TheTaskbarPlayButton.Description = "播放";
            TheTaskbarPlayButton.Click += TheTaskbarPlayButton_Click;

            ThumbButtonInfo TheTaskbarLastButton = new ThumbButtonInfo();
            TheTaskbarLastButton.ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/icons/上一首.png"));
            TheTaskbarLastButton.Description = "上一首";
            TheTaskbarLastButton.Click += TheTaskbarLastButton_Click; ;

            ThumbButtonInfo TheTaskbarNextButton = new ThumbButtonInfo();
            TheTaskbarNextButton.ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/icons/下一首.png"));
            TheTaskbarNextButton.Description = "下一首";
            TheTaskbarNextButton.Click += TheTaskbarNextButton_Click; ;

            this.TaskbarItemInfo.ThumbButtonInfos = new ThumbButtonInfoCollection();
            this.TaskbarItemInfo.ThumbButtonInfos.Add(TheTaskbarLastButton);
            this.TaskbarItemInfo.ThumbButtonInfos.Add(TheTaskbarPlayButton);
            this.TaskbarItemInfo.ThumbButtonInfos.Add(TheTaskbarNextButton);

            LrcWindow.UserShow(this);
            LrcWindow.IsShow = false;
            MainMenuPopup.UserShow(-1000, -1000);
            MainMenuPopup.UserClose();
            VolumePopup.UserShow(-1000, -1000);
            VolumePopup.UserClose();

            TheSource.MemoryClean();

            RenderOptions.SetBitmapScalingMode(PlayMusicAlbumPic, BitmapScalingMode.LowQuality);

            if (OSVersion == "11") DebugsMod = true;

            MyC.PopupWindow.SetPopupShow(MainBtn, "主页(H)");
            MyC.PopupWindow.SetPopupShow(SearchBtn, "搜索(S)");
            MyC.PopupWindow.SetPopupShow(DownloadBtn, "下载列表(D)");
            MyC.PopupWindow.SetPopupShow(PlayListBtn, "播放列表(L)");
            MyC.PopupWindow.SetPopupShow(SettingBtn, "设置(T)");
            MyC.PopupWindow.SetPopupShow(AboutBtn, "关于(A)");
            MyC.PopupWindow.SetPopupShow(FullScreenButton, "全屏/取消全屏(F)");
            MyC.PopupWindow.SetPopupShow(LrcWindowOpenButton, "打开/关闭桌面歌词");
            MyC.PopupWindow.SetPopupShow(ValueButton, "音量");
            MyC.PopupWindow.SetPopupShow(ListButton, "播放列表(Tab)");
            MyC.PopupWindow.SetPopupShow(Menu, "展开菜单");
            MyC.PopupWindow.SetPopupShow(WindowButton_Min, "最小化");
            MyC.PopupWindow.SetPopupShow(WindowButton_Close, "关闭窗口");
            MyC.PopupWindow.SetPopupShow(LastMusicButton, "上一首");
            MyC.PopupWindow.SetPopupShow(NextMusicButton, "下一首");
            MyC.PopupWindow.SetPopupShow(PauseMusicButton, "播放/暂停");
            MyC.PopupWindow.SetPopupShow(LList_CloseBtn, "隐藏播放列表界面");
            MyC.PopupWindow.SetPopupShow(LList_DeleteBtn, "清空播放列表");

            if (OSVersion != "11")
                MyC.PopupWindow.SetPopupShow(WindowButton_Max, "最大化");

            // 订阅亚克力背景悬浮控件改变主题事件
            BlurThemeChangeEvent += (data) =>
            {
                MainMenuPopup.Background = data.BackColor;
                VolumePopup.Background = data.BackColor;
            };

            // 订阅播放暂停事件
            PlayStateChangeEvent += async (value) =>
            {
                switch (value)
                {
                    case PlayState.Play:
                        if (await audioPlayer.Play())
                        {
                            ReCallPlayTimerBool = false;
                            PPPath.Data = PausePath;
                            Popups.PopupPPPath.Data = PausePath;
                            LrcWindow.PPPath.Data = PausePath;
                            TheTaskbarPlayButton.ImageSource = TaskBarPauseImage;
                            MusicPages.IsReCall = true;
                            SetTaskbarState(TaskbarItemProgressState.Normal);
                            ReCallPlayTimer();
                        }
                        break;

                    case PlayState.Pause:
                        if (await audioPlayer.Pause())
                        {
                            PPPath.Data = PlayPath;
                            Popups.PopupPPPath.Data = PlayPath;
                            LrcWindow.PPPath.Data = PlayPath;
                            TheTaskbarPlayButton.ImageSource = TaskBarPlayImage;
                            MusicPages.IsReCall = false;
                            SetTaskbarState(TaskbarItemProgressState.Paused);
                            Title1.Text = BaseName;
                            ReCallPlayTimerBool = false;
                        }
                        else
                        {
                            ShowBox("暂停失败", "暂停时出现错误，请重试。");
                        }
                        break;
                }
                LrcWindow.PauseButton.Content = PauseMusicButton.Content;
            };

            // 注册歌词改变事件以更新标题栏歌词显示
            MusicPages.LrcChangeEvent += (nowLrcData, nextLrcData) =>
            {
                Title1.Text = $"{MainWindow.BaseName}  |  " + nowLrcData.LrcText;
            };

            ModernWpf.ThemeManager.Current.ActualApplicationThemeChanged += (s, ev) => UpdataTheme();

            WaitSettings();
            StartLoadList();

            if (this.ActualWidth >= SystemParameters.WorkArea.Width && this.ActualHeight >= SystemParameters.WorkArea.Height && IsMaxScreen == false)
            {
                if (IsFullScreen == false) IsMaxScreen = true;
            }

        }

        public void UpdataTheme()
        {
            if (zilongcn.Others.AppsUseLightTheme())
            {
                if (NowThemeData.MD5 != DefaultThemeData.MD5)
                {
                    NowThemeData = DefaultThemeData;
                    NowBlurThemeData = DefaultBlurThemeData;
                    zilongcn.Others.EnableMica(this, false);
                }
            }
            else
            {
                if (NowThemeData.MD5 != DarkThemeData.MD5)
                {
                    NowThemeData = DarkThemeData;
                    NowBlurThemeData = DarkBlurThemeData;
                    zilongcn.Others.EnableMica(this, true);
                }
            }

            SystemBlur = zilongcn.Others.SystemEnableBlurBehind();
        }

        // 快捷方式参数逻辑
        public async void StartLoadList()
        {
            if (TheArgs.Count() > 0)
            {
                bool isHasUnknowArg = false;
                string unknowArg = "未知快捷方式参数：";

                foreach (string Arg in TheArgs)
                {
                    if (Arg.Contains("NormalMod"))
                    {
                        try
                        {
                            if (Arg.Contains("-"))
                            {
                                if (Arg.Split('-')[1] == "SUM")
                                {
                                    UserClose();
                                }
                            }

                            // TODO: 启动播放歌单
                            //await MusicUpdata.UpdataUserMusicList(this);

                            await PlaySet(NowPlayList[0]);
                            await Task.Delay(10);
                            NowPlayState = PlayState.Pause;
                        }
                        catch { }
                    }
                    else if (Arg == "DebugMod") Debug = true;
                    else if (Arg.Contains("ShowBoxSent"))
                    {
                        await Task.Delay(10);

                        try
                        {
                            string title = Arg.Split('-')[1];
                            string text = Arg.Split('-')[2];

                            ShowBox(title, text);
                        }
                        catch { }
                    }
                    else if (Arg == "GreenOpenMod")
                    {
                        File.Delete(BaseFoldsPath.SoftwareDataPath);
                        ShowBox("绿色模式", "正在以绿色模式运行。");
                    }
                    else
                    {
                        if (isHasUnknowArg) unknowArg += "，";

                        unknowArg += $"\"{Arg}\"";

                        isHasUnknowArg = true;
                    }
                }

                if (isHasUnknowArg)
                {
                    ShowBox("快捷方式参数错误", unknowArg + "。");
                }
            }
        }

        public bool IsLoadSettings = false;
        public async void WaitSettings()
        {
            await Task.Delay(5);
            try
            {
                SoftwareVisual = SoftwareVisual;
                LrcWindowTaskBarShow = LrcWindowTaskBarShow;
                BaseFoldsPath.DownloadFolderPath = SettingDataEdit.GetParam("DownloadPath");
                BaseFoldsPath.SongsFolderPath = SettingDataEdit.GetParam("LoadPath");
                SystemBlur = SettingDataEdit.ToBool(SettingDataEdit.GetParam("LrcBlurBackground"));
                quality = SettingDataEdit.ToRenderingBias(SettingDataEdit.GetParam("BlurMod"));
                Blurs = int.Parse(SettingDataEdit.GetParam("BlurRadius"));
                LrcColor = new SolidColorBrush(SettingDataEdit.ToColor(SettingDataEdit.GetParam("LrcColor")));
                OtherColor = new SolidColorBrush(SettingDataEdit.ToColor(SettingDataEdit.GetParam("OtherLrcColor")));
                NowVolume = float.Parse(SettingDataEdit.GetParam("Volume"));
                //SettingPages.DesktopLrcCenterOCBtn.IsChecked = LrcWindow.IsCenter;
                //if (quality == RenderingBias.Performance) SettingPages.PerOCBtn.IsChecked = false;
                //else SettingPages.PerOCBtn.IsChecked = true;
                LrcWindow.LrcSize = int.Parse(SettingDataEdit.GetParam("DesktopLrcSize"));
                //SettingPages.AnimateOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("Animate"));
                Animate = SettingDataEdit.ToBool(SettingDataEdit.GetParam("Animate"));
                //SettingPages.TopMostOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("WindowTopmost"));
                //SettingPages.StartUpOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("StartUp"));
                //SettingPages.WasapiNotShareOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("WasapiNotShare"));
                //SettingPages.ShowTranslateOnlyOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("ShowTranslateOnly"));

                //SettingPages.UseLayoutRoundingOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("UseLayoutRounding"));
                //SettingPages.SnapsToDevicePixelsOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("SnapsToDevicePixels"));
                //SettingPages.TextHintingModeOCBtn.IsChecked = SettingDataEdit.ToTextHintingMode(SettingDataEdit.GetParam("TextHintingMode")) == TextHintingMode.Fixed ? true : false;

                UseLayoutRounding = bool.Parse(SettingDataEdit.GetParam("UseLayoutRounding"));
                SnapsToDevicePixels = bool.Parse(SettingDataEdit.GetParam("SnapsToDevicePixels"));

                if (SettingDataEdit.ToTextHintingMode(SettingDataEdit.GetParam("TextHintingMode")) == TextHintingMode.Fixed)
                {
                    TextOptions.SetTextHintingMode(this, TextHintingMode.Fixed);
                    TextOptions.SetTextHintingMode(LrcWindow, TextHintingMode.Fixed);
                }
                else
                {
                    TextOptions.SetTextHintingMode(this, TextHintingMode.Animated);
                    TextOptions.SetTextHintingMode(LrcWindow, TextHintingMode.Animated);
                }

                //SettingPages.LittleMusicPageOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("LittleMusicPage"));
                //SettingPages.VolumeDataSystemOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("VolumeDataSystem"));
                //SettingPages.DarkThemeOCBtn.IsChecked = SettingDataEdit.ToBool(SettingDataEdit.GetParam("DarkTheme"));
                MusicPages.VolumeDataShow = SettingDataEdit.ToBool(SettingDataEdit.GetParam("VolumeDataShow"));
                MusicPlayMod.NowPlayMod = MusicPlayMod.NowPlayMod;

                /*
                if (OSVersion == "7" || OSVersion == "8" || OSVersion == "8.1")
                {
                    SettingPages.BlurLrcOCBtn.IsChecked = false;
                    SettingPages.BlurLrcOCBtn.IsLocked = true;
                }
                else
                {
                    SettingPages.BlurLrcOCBtn.IsChecked = IsBlurDesktopLrc;
                }*/

                if (SettingDataEdit.ToBool(SettingDataEdit.GetParam("Background")) == true)
                {
                    SettingPages.SetBG(BaseFoldsPath.BackgroundPath);
                }
            }
            catch (Exception err)
            {
                err.ToString(); //??? TODO: Debug 删除此句会导致设置初始化没动画

                //throw err;
                //MessageBox.Show(err.ToString());

                File.WriteAllText(BaseFoldsPath.SettingDataPath, BaseFoldsPath.NormalSettingText);
                WaitSettings();
                return;
            }
            IsLoadSettings = true;

            Show();
        }

        public delegate void LrcDelegate(string LrcText1, string LrcText2);
        public LrcDelegate lrcDelegate;

        public enum ShowBoxStyle { Normal, UserDesktopTimeChanger, Loading, MusicDatas, AltF4UI, WindowMenuHelperUI }

        public void ShowBox(string Title, string Text, ShowBoxStyle showBoxStyle = ShowBoxStyle.Normal, bool Animate = true)
        {
            try
            {
                // 将ShowBox_Grid的所有子类设为不可见
                foreach (Grid grid in ShowBox_Grid.Children)
                {
                    grid.Visibility = Visibility.Collapsed;
                }
                ShowBox_Loading_SimpleLoading.Pause = true;

                ShowBoxUI.Visibility = Visibility.Visible;
                switch (showBoxStyle)
                {
                    case ShowBoxStyle.Normal:
                        ShowBox_Normal_Title.Text = Title;
                        ShowBox_Normal_Text.Text = Text;
                        ShowBox_Normal.Visibility = Visibility.Visible;
                        ShowBox_Normal_Button.Focus();
                        break;
                    case ShowBoxStyle.UserDesktopTimeChanger:
                        ShowBox_UserDesktopChanger_Title.Text = Title;
                        ShowBox_UserDesktopChanger_Text.Text = Text;
                        ShowBox_UserDesktopChanger.Visibility = Visibility.Visible;
                        ShowBox_Loading_Button.Focus();
                        break;
                    case ShowBoxStyle.Loading:
                        ShowBox_Loading_Title.Text = Title;
                        ShowBox_Loading_Text.Text = Text;
                        ShowBox_Loading_SimpleLoading.Pause = false;
                        ShowBox_Loading.Visibility = Visibility.Visible;
                        break;
                    case ShowBoxStyle.MusicDatas:
                        ShowBox_MusicDatas_Title.Text = Title;
                        ShowBox_MusicDatas.Visibility = Visibility.Visible;
                        break;
                    case ShowBoxStyle.AltF4UI:
                        ShowBox_AltF4.Visibility = Visibility.Visible;
                        break;
                    case ShowBoxStyle.WindowMenuHelperUI:
                        ShowBox_WindowMenuHelper.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }

                Animations.animateOpacity(ShowBoxUI, 0, 1, AnimateOpacityNormalTime - 0.03, IsAnimate: Animation && Animate).Begin();
                Animations.animateScale(ShowBox_Scale, 0.94, 1, AnimateOpacityNormalTime - 0.03, IsAnimate: Animation && Animate);
            }
            catch { TheSource.ShowBox(Title, Text); }
        }

        public void ShowBox(TheMusicDatas.MusicData data, bool Animate = true)
        {
            ShowBox_MusicDatas_MusicData = data;

            ShowBox_MusicDatas_TitleText.Text = data.Title;
            ShowBox_MusicDatas_ArtistText.Text = data.Artist;
            ShowBox_MusicDatas_AlbumText.Text = data.Album;
            if (data.From != TheMusicDatas.MusicFrom.localMusic)
            {
                ShowBox_MusicDatas_IDText_s.Visibility = Visibility.Visible;
                ShowBox_MusicDatas_PicUrlText_s.Visibility = Visibility.Visible;
                ShowBox_MusicDatas_AlbumIDText_s.Visibility = Visibility.Visible;
                ShowBox_MusicDatas_IDText.Text = data.SongRid;
                ShowBox_MusicDatas_PicUrlText.Text =
                    !IsOpenBigPage && (data.From == TheMusicDatas.MusicFrom.kgMusic || data.From == TheMusicDatas.MusicFrom.neteaseMusic) ?
                    data.PicUri == "" || data.PicUri == null ? "请播放歌曲后点击音乐界面的更多按钮获取。" : data.PicUri
                    : data.PicUri;
                ShowBox_MusicDatas_AlbumIDText.Text = data.AlbumID;
                if (data.From == TheMusicDatas.MusicFrom.kgMusic) ShowBox_MusicDatas_IDText_Button.Content = "Hash";
                else ShowBox_MusicDatas_IDText_Button.Content = "歌曲id";
            }
            else
            {
                ShowBox_MusicDatas_IDText_s.Visibility = Visibility.Collapsed;
                ShowBox_MusicDatas_PicUrlText_s.Visibility = Visibility.Collapsed;
                ShowBox_MusicDatas_AlbumIDText_s.Visibility = Visibility.Collapsed;
            }

            if (IsOpenBigPage)
            {
                ShowBox_MusicDatas_InMusicPage_s.Visibility = Visibility.Visible;
            }
            else
            {
                ShowBox_MusicDatas_InMusicPage_s.Visibility = Visibility.Collapsed;
            }

            ShowBox($"\"{data.Title}\" 的详细信息", null, ShowBoxStyle.MusicDatas, Animate);
        }

        TheMusicDatas.MusicData ShowBox_MusicDatas_MusicData = new TheMusicDatas.MusicData();

        private void ShowBox_MusicDatas_ButtonsClick(object sender, RoutedEventArgs e)
        {
            MyC.znButton znButton = sender as MyC.znButton;
            if (znButton.Name == "ShowBox_MusicDatas_TitleText_Button") Clipboard.SetDataObject(ShowBox_MusicDatas_MusicData.Title);
            else if (znButton.Name == "ShowBox_MusicDatas_ArtistText_Button") Clipboard.SetDataObject(ShowBox_MusicDatas_MusicData.Artist);
            else if (znButton.Name == "ShowBox_MusicDatas_AlbumText_Button") Clipboard.SetDataObject(ShowBox_MusicDatas_MusicData.Album);
            else if (znButton.Name == "ShowBox_MusicDatas_IDText_Button") Clipboard.SetDataObject(ShowBox_MusicDatas_MusicData.SongRid);
            else if (znButton.Name == "ShowBox_MusicDatas_PicUrlText_Button") Clipboard.SetDataObject(ShowBox_MusicDatas_MusicData.PicUri);
            else if (znButton.Name == "ShowBox_MusicDatas_AlbumIDText_Button") Clipboard.SetDataObject(ShowBox_MusicDatas_MusicData.AlbumID);
            else if (znButton.Name == "ShowBox_MusicDatas_GetAlbumPicButton")
            {
                string SavePath = TheSource.SavingWindow("", $"{MusicPages.MusicData.Title}-{MusicPages.MusicData.Artist}", ".jpg", "图片文件 (*.jpg)|*.jpg");

                if (SavePath != null)
                {
                    try
                    {
                        TheSource.SaveBitmapImage(MusicPages.TheImage.Source as BitmapImage, SavePath);
                        ShowBox("保存成功", "文件已保存到'" + SavePath);
                    }
                    catch (Exception err) { ShowBox("错误", "无法保存图片!\n\n错误代码:\n" + err.ToString()); }
                }
                else ShowBox("已取消", "操作已取消");
            }
        }

        public void ShowBox_ButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                MyC.znButton znButton = sender as MyC.znButton;

                switch (znButton.Name)
                {
                    case "ShowBox_AltF4_ExitAppButton":
                        MenuExit_Click(null, null);
                        break;
                    case "ShowBox_AltF4_MinSizeButton":
                        WindowButton_Min_Click(null, null);
                        break;
                    case "ShowBox_AltF4_CloseWindowButton":
                        WindowButton_Close_Click(null, null);
                        break;
                    case "ShowBox_WindowMenuHelper_MinSizeButton":
                        WindowButton_Min_Click(null, null);
                        break;
                    case "ShowBox_WindowMenuHelper_MaxSizeButton":
                        WindowButton_Max_Click(null, null);
                        break;
                    case "ShowBox_WindowMenuHelper_ExitAppButton":
                        MenuExit_Click(null, null);
                        break;
                    default:
                        break;
                }
            }
            catch { }

            Storyboard TheBorad = Animations.animateOpacity(ShowBoxUI, 1, 0, 0.2, IsAnimate: Animation);
            TheBorad.Completed += TheBorad_Completed;
            TheBorad.Begin();
            Animations.animateScale(ShowBox_Scale, 1, 0.975, 0.2, IsAnimate: Animation);
        }

        public delegate void NowPlaySongChangeDelegate(TheMusicDatas.MusicData musicData);
        public event NowPlaySongChangeDelegate NowPlaySongChangeEvent;

        bool IsLoadingFile = false;
        string TheMusicCachePath = null;
        public async Task PlaySet(TheMusicDatas.MusicData value, bool IsNowPlaySongChange = false)
        {
            if (SleepTaskEndPauseMusic)
            {
                SleepTaskEndPauseMusic = false;
                return;
            }
            if (IsLoadingFile) return;

            try { audioPlayer.NowMusicPosition = new TimeSpan(0); }
            catch { }
            NowPlayState = PlayState.Pause;

            if (value.SongRid == null)
            {
                return;
            }

            if (!IsNowPlaySongChange)
            {
                NowPlaySong = value;
                return;
            }

            audioPlayer.DisposeAll();

            if (MainMenuPopup.IsShow) Popups.nowPlayingName.Text = $"{value.Title} - {value.Artist}";
            NowPlay_TitleTb.Text = value.Title;
            NowPlay_ArtistTb.Text = value.Artist + " - 加载中...";
            SetTaskbarState(TaskbarItemProgressState.Indeterminate);
            LrcWindow.SetLrc(value.Title + " - " + value.Artist, "加载中");

            // 更新界面
            MusicUpdata.UpdataInterface(this, value);

            // 获取歌曲缓存
            musicCache = new MusicPlayCache(this);
            Tuple<string, TheMusicDatas.MusicData> MusicCachePath = await musicCache.GetMusicCache(value);
            if (MusicCachePath.Item1 == "errorCache" || NowPlaySong.MD5 != value.MD5 || NowPlaySong.MD5 == null) return;

            TheMusicCachePath = MusicCachePath.Item1;
            bool audioState;

            IsLoadingFile = true;
            NowPlay_ArtistTb.Text = value.Artist + " - 正在加载本地文件到内存中...";

            if (TheMusicCachePath != null && NowPlaySong.MD5 == value.MD5)
                audioState = await audioPlayer.Set(
                    TheMusicCachePath,
                    audioPlayer.NowOutDevice,
                    value.From != TheMusicDatas.MusicFrom.localMusic
                    );
            else
                audioState = false;

            IsLoadingFile = false;

            NowPlay_ArtistTb.Text = value.Artist;

            if (NowPlaySongChangeEvent != null) NowPlaySongChangeEvent(value);

            if (audioState)
            {
                NowPlayState = PlayState.Play;
            }
            else
            {
                SetAlbumImageState(AlbumImageState.Error);
            }

            SetTaskbarState(TaskbarItemProgressState.None);
            LrcWindow.SetLrc($"{value.Title} - {value.Artist}", "");
        }

        public enum AlbumImageState { Error, Load, None };
        public void SetAlbumImageState(AlbumImageState state)
        {
            switch (state)
            {
                case AlbumImageState.None:
                    IsMusicLoadingBack.Visibility = Visibility.Collapsed;
                    IsMusicLoadingAnime.Pause = true;
                    break;
                case AlbumImageState.Load:
                    IsMusicLoadingBack.Background = new SolidColorBrush(Color.FromArgb(153, 100, 100, 100));
                    IsMusicLoadingBack.Visibility = Visibility.Visible;
                    IsMusicLoadingAnime.Pause = false;
                    IsMusicLoadingText.Text = "0%";
                    break;
                case AlbumImageState.Error:
                    IsMusicLoadingBack.Visibility = Visibility.Visible;
                    IsMusicLoadingBack.Background = new SolidColorBrush(Color.FromArgb(153, 255, 0, 0));
                    IsMusicLoadingText.Text = "播放失败";
                    break;
            }
        }

        private void MainActivity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.Tab || e.Key == Key.Space)
            {
                e.Handled = true;
            }

            if (e.KeyStates == System.Windows.Input.Keyboard.GetKeyStates(Key.F4) && System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (IsOpenBigPage) IsOpenBigPage = false;
                else ShowBox("", "", ShowBoxStyle.AltF4UI);
                e.Handled = true;
            }
            else if (e.KeyStates == System.Windows.Input.Keyboard.GetKeyStates(Key.Space) && System.Windows.Input.Keyboard.Modifiers == ModifierKeys.Alt)
            {
                ShowBox("", "", ShowBoxStyle.WindowMenuHelperUI);
                e.Handled = true;
            }
            /*
            else if (e.Key == Key.Up)
            {
                ChangeVolume();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                ChangeVolume(false);
                e.Handled = true;
            }*/
        }

        private void MainActivity_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (MusicPage2.Visibility == Visibility.Visible) SetBigPage("Music");
                    break;

                case Key.Space:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) NowPlayState = NowPlayState == PlayState.Play ? PlayState.Pause : PlayState.Play;
                    break;

                case Key.Tab:
                    e.Handled = true;
                    if (ListButton.IsEnabled) { ListButton_Click(null, null); ListButton.Focus(); }
                    break;

                case Key.Right:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) audioPlayer.NowMusicPosition += new TimeSpan(0, 0, 5);
                    break;

                case Key.Left:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) audioPlayer.NowMusicPosition -= new TimeSpan(0, 0, 5);
                    break;

                case Key.M:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused && NowPlaySong.SongRid != null) SetBigPage("Music");
                    break;

                case Key.N:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) SetBigPage("Set");
                    break;

                case Key.H:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) SetPage("Main");
                    break;

                case Key.S:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) SetPage("List");
                    break;

                case Key.D:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) SetPage("Download");
                    break;

                case Key.L:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) SetPage("PlayList");
                    break;

                case Key.T:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) SetPage("Setting");
                    break;

                case Key.A:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) SetPage("About");
                    break;

                case Key.F:
                    e.Handled = true;
                    if (!TheSearchPage.SearchTextBox.IsFocused) FullScreenButton_Click(null, null);
                    break;

                default:
                    break;
            }
        }

        public void ThemeChangeDo()
        {
            object a = NowPage;
            InPage.Content = null;
            InPage.Content = NowPage;
            if (OSVersion != "11") MainBack.Background = NowThemeData.BackColor;

            SolidColorBrush solidColorBrush = NowThemeData.ButtonBackColor as SolidColorBrush;
            if (NowThemeData.MD5 == DarkThemeData.MD5)
            {
                solidColorBrush = new SolidColorBrush(Color.FromArgb(255, 52, 52, 52));
            }
            ListLPage.Background = solidColorBrush;
            MusicVolumePage.Background = solidColorBrush;
            TopBar.Background = solidColorBrush;
            LeftBarMin.Background = solidColorBrush;
        }

        private void TheBorad_Completed(object sender, EventArgs e)
        {
            ShowBox_Loading_SimpleLoading.Pause = true;
            ShowBoxUI.Visibility = Visibility.Collapsed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            UserClose();
        }

        private void TheTaskbarNextButton_Click(object sender, EventArgs e)
        {
            NextMusicButton_Click_1(null, null);
        }

        private void TheTaskbarLastButton_Click(object sender, EventArgs e)
        {
            LastMusicButton_Click_1(null, null);
        }

        private void TheTaskbarPlayButton_Click(object sender, EventArgs e)
        {
            PauseMusicButton_Click(null, null);
        }

        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "FlashWindow")]
        private static extern void FlashWindow(IntPtr hwnd, bool bInvert);

        public void SetTaskbarState(TaskbarItemProgressState TheState)
        {
            //this.TaskbarItemInfo.ProgressState = TheState;
        }

        public void SetTaskbarValue(double Value)
        {
            //this.TaskbarItemInfo.ProgressValue = Value;
        }

        public void Flash()
        {
            FlashWindow(GetWindowHandle(), true);
        }

        public IntPtr GetWindowHandle()
        {
            try
            {
                return new WindowInteropHelper(this).Handle;
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private void MessageIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    UserShow();
                }
            }
            catch { }
        }

        private void MessageIcon_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                double px = System.Windows.Forms.Control.MousePosition.X;
                double py = System.Windows.Forms.SystemInformation.WorkingArea.Height + 14;

                Popups.nowPlayingName.Text = NowPlaySong.SongRid != null ? $"{NowPlaySong.Title} - {NowPlaySong.Artist}" : "无播放任务";
                MainMenuPopup.UserShow(px, py);
            }
        }

        private void MessageIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            SetBigPage("Music");
        }

        private async void PlayEnd()
        {
            PlayMusicSlider.Value = PlayMusicSlider.Maximum;
            NowPlayState = PlayState.Pause;
            audioPlayer.Stop();

            if (MusicPlayMod.NowPlayMod == MusicPlayMod.PlayMod.Loop)
            {
                await PlaySet(NowPlaySong);
            }
            else
            {
                SetNextSong();
            }

            TheSource.MemoryClean();
        }

        private void Bar_MouseLeftButtonDown(object sender, object args)
        {
            IsBarKeyDown = true;
            ReCallPlayTimerTime = 1;
        }

        private void Bar_MouseLeftButtonUp(object sender, object args)
        {
            audioPlayer.NowMusicPosition = new TimeSpan(long.Parse(PlayMusicSlider.Value.ToString().Split('.')[0]));
            ReCallPlayTimerTime = 200;

            IsBarKeyDown = false;

            MusicPages.CleanLrcColor();
        }

        public async void SetNextSong()
        {
            if (IsLoadingFile) return;
            try
            {
                NowPlayList.First();
            }
            catch { return; }

            try
            {
                int TheCount = 0;

                if (NowPlayList.Count() != 0)
                {
                    if (MusicPlayMod.NowPlayMod != MusicPlayMod.PlayMod.Random)
                    {
                        foreach (TheMusicDatas.MusicData Datas in NowPlayList.musicDatasList)
                        {
                            if (Datas.MD5 == NowPlaySong.MD5)
                            {
                                try
                                {
                                    await PlaySet(NowPlayList[TheCount + 1]);
                                }
                                catch
                                {
                                    await PlaySet(NowPlayList[0]);
                                }
                                break;
                            }
                            TheCount += 1;
                        }
                    }
                    else
                    {
                        await PlaySet(NowPlayList[new Random().Next(0, NowPlayList.Count())]);
                    }
                }
            }
            catch
            {
                await PlaySet(NowPlayList[0]);
            }
        }

        public async void SetBeforeSong()
        {
            if (IsLoadingFile) return;
            try
            {
                NowPlayList.First();
            }
            catch { return; }

            try
            {
                int TheCount = 0;

                if (NowPlayList.Count() != 0)
                {
                    if (MusicPlayMod.NowPlayMod != MusicPlayMod.PlayMod.Random)
                    {
                        foreach (TheMusicDatas.MusicData Datas in NowPlayList.musicDatasList)
                        {
                            if (Datas.MD5 == NowPlaySong.MD5)
                            {
                                try
                                {
                                    await PlaySet(NowPlayList[TheCount - 1]);
                                }
                                catch
                                {
                                    await PlaySet(NowPlayList.Last());
                                }
                                break;
                            }
                            TheCount += 1;
                        }
                    }
                    else
                    {
                        await PlaySet(NowPlayList[new Random().Next(0, NowPlayList.Count())]);
                    }
                }
            }
            catch
            {
                await PlaySet(NowPlayList[0]);
            }
        }

        private bool _IsReallyMin = false;
        private bool _IsShortBar = false;
        public void SizeChangedDo(object sender, object TheEvent)
        {
            //LrcWindow.Lrc1.Text = $"{ActualWidth}x{ActualHeight}";
            if (ActualHeight < 115)
            {
                if (!_IsReallyMin)
                {
                    (WindowButton_Close.Parent as FrameworkElement).Visibility = Visibility.Collapsed;
                    Title1.Visibility = Visibility.Collapsed;
                    Menu.Visibility = Visibility.Collapsed;
                    MusicPages.MoreButton.Visibility = Visibility.Collapsed;
                    MusicPages.TimeButton.Visibility = Visibility.Collapsed;
                    if (!DebugsMod) TopBar.Visibility = Visibility.Collapsed;

                    _IsReallyMin = true;
                }
            }
            else
            {
                if (_IsReallyMin)
                {
                    (WindowButton_Close.Parent as FrameworkElement).Visibility = Visibility.Visible;
                    Title1.Visibility = Visibility.Visible;
                    Menu.Visibility = Visibility.Visible;
                    MusicPages.MoreButton.Visibility = Visibility.Visible;
                    MusicPages.TimeButton.Visibility = Visibility.Visible;
                    if (!DebugsMod) TopBar.Visibility = Visibility.Visible;

                    _IsReallyMin = false;
                }
            }

            if (this.ActualWidth <= 850)
            {
                if (!_IsShortBar)
                {
                    //PlayMusicBit.Visibility = Visibility.Collapsed;

                    _IsShortBar = true;
                }
            }
            else
            {
                if (_IsShortBar)
                {
                    LrcWindowOpenButton.Visibility = Visibility.Visible;
                    PlayMusicBit.Visibility = Visibility.Visible;

                    _IsShortBar = false;
                }
            }

            //(popupWindow.Content as TextBlock).Text = $"{this.ActualWidth}x{this.ActualHeight}\n{SystemParameters.WorkArea.Width}x{SystemParameters.WorkArea.Height}";
            //popupWindow.SetSize();
        }


        private int ReCallPlayTimerTime = 200;
        private bool ReCallPlayTimerBool = false;
        private async void ReCallPlayTimer()
        {
            if (ReCallPlayTimerBool) return;

            ReCallPlayTimerBool = true;
            while (ReCallPlayTimerBool)
            {
                if (NowPlayState == PlayState.Pause)
                {
                    await Task.Delay(ReCallPlayTimerTime);
                    continue;
                }

                if (!WindowIsOpen())
                {
                    await Task.Delay(ReCallPlayTimerTime);
                    continue;
                }

                string b = audioPlayer.NowMusicPosition.ToString().Split('.')[0];
                string c = audioPlayer.NowMusicTotalTime.ToString().Split('.')[0];
                string d = (audioPlayer.NowMusicTotalTime - audioPlayer.NowMusicPosition).ToString().Split('.')[0];
                string NowTimer = "";
                string NowOtherTime = d.Split(':')[1] + ":" + d.Split(':')[2];

                try
                {
                    NowTimer = b.Split(':')[1] + ":" + b.Split(':')[2] + "/" + c.Split(':')[1] + ":" + c.Split(':')[2];
                }
                catch { }

                if (!IsBarKeyDown)
                {
                    if (PlayMusicTime.Text != NowTimer)
                    {
                        try
                        {
                            SetTaskbarState(TaskbarItemProgressState.Normal);
                            SetTaskbarValue(double.Parse(audioPlayer.NowMusicPosition.Ticks.ToString()) / double.Parse(audioPlayer.NowMusicTotalTime.Ticks.ToString()));
                        }
                        catch { }

                        PlayMusicTime.Text = NowTimer;
                        PlayMusicOtherTime.Text = NowOtherTime;
                        if (IsBarKeyDown == false) MediaChangedDo();
                        if (IsOpenBigPage) MusicPages.TimeUpdata(true);
                    }
                }
                else
                {
                    string a = (new TimeSpan(long.Parse(PlayMusicSlider.Value.ToString().Split('.')[0]))).ToString().Split('.')[0];
                    try
                    {
                        PlayMusicTime.Text = a.Split(':')[1] + ":" + a.Split(':')[2];
                        PlayMusicOtherTime.Text = NowOtherTime;
                    }
                    catch { }
                }
                await Task.Delay(ReCallPlayTimerTime);
            }

            ReCallPlayTimerBool = false;
        }

        private void MediaChangedDo()
        {
            if (audioPlayer.NowMusicTotalTime == null) return;

            PlayMusicSlider.Maximum = audioPlayer.NowMusicTotalTime.Ticks;
            PlayMusicSlider.Value = audioPlayer.NowMusicPosition.Ticks;
        }

        public void SetIconMessageToolTip(string Text)
        {
            try
            {
                MessageIcon.Text = $"{BaseName}\n" + Text;
            }
            catch (ArgumentOutOfRangeException)
            {
                ShowMessage("注意", "文本必须小于64字符。\n" + Text, TipIcon: System.Windows.Forms.ToolTipIcon.Warning);
            }
        }

        public bool InUserList = false;
        public bool IsLoadML = false;
        public List<FrameworkElement> LeftButtons
        {
            get
            {
                return new List<FrameworkElement>() { MainBtnBack, SearchBtnBack, DownloadBtnBack, PlayListBtnBack, SettingBtnBack, AboutBtnBack };
            }
        }

        public bool openedsetting = false;
        public void SetPage(string ThePages)
        {
            object pages = null;
            string title = "";
            bool IsSearchPage = false;
            InUserList = false;

            foreach (var element in LeftButtons)
            {
                zilongcn.Animations.animateOpacity(element, element.Opacity, 0, 0.25, IsAnimate: Animation).Begin();
            }

            switch (ThePages)
            {
                case "Main":
                    pages = MainPages;
                    title = "主页";
                    zilongcn.Animations.animateOpacity(MainBtnBack, MainBtnBack.Opacity, 1, 0.25, IsAnimate: Animation).Begin();
                    break;
                case "List":
                    title = "搜索";
                    pages = TheSearchPage;
                    IsSearchPage = true;
                    zilongcn.Animations.animateOpacity(SearchBtnBack, SearchBtnBack.Opacity, 1, 0.25, IsAnimate: Animation).Begin();
                    break;
                case "Loading":
                    title = "";
                    pages = LoadingPage;
                    break;
                case "Music":
                    title = "";
                    pages = MusicPages;
                    MusicPages.Set(NowPlaySong, Animation);
                    break;
                case "Album":
                    pages = TheSearchPage;
                    TheSearchPage.InPage.Content = AlbumPages;
                    IsSearchPage = true;
                    title = "搜索";
                    break;
                case "PlayList":
                    pages = MusicPlayListContent;
                    title = "播放列表";
                    InUserList = true;
                    // TODO
                    if (!IsLoadML) { }
                    //MusicUpdata.UpdataUserMusicList(this);
                    zilongcn.Animations.animateOpacity(PlayListBtnBack, PlayListBtnBack.Opacity, 1, 0.25, IsAnimate: Animation).Begin();
                    break;
                case "Setting":
                    pages = SettingPages;
                    title = "设置";

                    if (!openedsetting)
                    {
                        SettingPages.OpenAndCloseSettingBar();
                        openedsetting = true;
                    }

                    SettingPages.OpenSettingDo();

                    try
                    {
                        SettingPages.UpdataCacheLength();
                    }
                    catch { }

                    zilongcn.Animations.animateOpacity(SettingBtnBack, SettingBtnBack.Opacity, 1, 0.25, IsAnimate: Animation).Begin();
                    break;
                case "Download":
                    pages = TheDownload;
                    title = "下载列表";
                    zilongcn.Animations.animateOpacity(DownloadBtnBack, DownloadBtnBack.Opacity, 1, 0.25, IsAnimate: Animation).Begin();
                    break;
                case "About":
                    pages = TheAbout;
                    title = "关于";
                    zilongcn.Animations.animateOpacity(AboutBtnBack, AboutBtnBack.Opacity, 1, 0.25, IsAnimate: Animation).Begin();
                    break;
            }

            if (ListPages != null) ListPages.Visibility = Visibility.Collapsed;

            PageTitle.Text = title;
            NowPage = pages;
            InPage.Content = pages;

            Animations animations = new Animations(Animation);
            animations.animatePosition(PageTitle, new Thickness(33, 10, 0, 0), new Thickness(5, 10, 0, 0), 0.5, 1, 0);
            animations.animateOpacity(PageTitle, 0, 1, 0.40, 0.8);
            animations.animatePosition(InPage, new Thickness(44, 0, -44, 0), new Thickness(0, 0, 0, 0), 0.5, 1, 0);
            animations.Begin();

            Storyboard storyboard = Animations.animateOpacity(InPage, 0, 1, 0.45, 0.8, IsAnimate: Animation);

            storyboard.Completed += (obj, obj1) =>
            {
                if (ListPages != null && IsSearchPage)
                {
                    ListPages.Visibility = Visibility.Visible;
                    Animations.animateOpacity(ListPages, 0, 1, AnimateOpacityNormalTime, IsAnimate: Animation).Begin();
                }
            };
            storyboard.Begin();

            TheDownload.SizeChangedDo(null, null);
            SettingPages.Width = double.NaN;
            SettingPages.Parent_SizeChanged(null, null);
        }

        public void TestAnyMainBtnIsPressed(string elementName)
        {
            if (elementName == "MainBtn")
            {
                SetPage("Main");
            }
            if (elementName == "SearchBtn")
            {
                SetPage("List");
            }
            if (elementName == "DownloadBtn")
            {
                SetPage("Download");
            }
            if (elementName == "PlayListBtn")
            {
                SetPage("PlayList");
            }
            if (elementName == "SettingBtn")
            {
                SetPage("Setting");
            }
            if (elementName == "AboutBtn")
            {
                SetPage("About");
            }
        }

        List<string> UserAgent = new List<string>()
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.132 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.77 Safari/537.36 Edg/91.0.864.37"
        };

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //TheTimer.Stop();
            //PlayMusicName.Text = PlayMusicSlider.Value.ToString();
            //media.Position = new TimeSpan(double.Parse(PlayMusicSlider.Value.ToString()));
            //TheTimer.Start();
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public System.Windows.Forms.NotifyIcon ShowMessage(string Title, string Text, int TimeOut = 4, System.Windows.Forms.ToolTipIcon TipIcon = System.Windows.Forms.ToolTipIcon.None)
        {
            MessageIcon.BalloonTipTitle = Title;
            MessageIcon.BalloonTipText = Text;
            MessageIcon.BalloonTipIcon = TipIcon;
            MessageIcon.ShowBalloonTip(TimeOut);

            return this.MessageIcon;
        }

        public void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            StartSearch(TheSearchPage.SearchTextBox.Text, TheSearchPage.PageCount.ToString(), TheSearchPage.PageSize.ToString(), TheSearchPage.MusicFrom);
        }

        MusicSearch musicSearch = null;
        public async void StartSearch(string text, string pageCount, string pageSize, TheMusicDatas.MusicFrom from)
        {
            musicSearch = new MusicSearch(this);
            try
            {
                await musicSearch.StartSearch(text, pageCount, pageSize, from);
            }
            catch (Exception err)
            {
                ShowBox("搜索失败", "搜索时出现问题，请重试。\n" + err.ToString());
                TheSearchPage.SearchBtn.IsEnabled = true;
            }
        }

        public void AddACard(string TheNumber, TheMusicDatas.MusicData TheDatas, string pic = null, bool AEN = true, ListPage TheObj = null, bool Silvers = false)
        {
            ItemBar Bar = new ItemBar();

            Bar.Set(TheNumber.ToString(), TheDatas, pic, AEN);

            if (Silvers)
            {
                Bar.BackAnimateGrid.Fill = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
            }

            ListPages = TheObj;
            TheObj.Lists.Children.Add(Bar);
        }

        public void AddAlbumCards(JObject jsons, bool isLast = false, ListPage TheObj = null)
        {
            LastData = NowListData;
            NowListData = jsons;
            bool IsSilver = false;
            int anl = 1;

            debug.Text += jsons.ToString();

            try
            {
                foreach (var data in jsons["data"]["list"])
                {
                    string albumpic = data["pic"].ToString();

                    //if (data["songTimeMinutes"].ToString() == "") data["songTimeMinutes"] = "无详细日期";

                    TheMusicDatas.MusicData TheStruct = new TheMusicDatas.MusicData(data["name"].ToString(), data["id"].ToString(), data["artist_name"].ToString(), data["album_name"].ToString(), null, albumpic, data["releaseDate"].ToString(), data);

                    AddACard(anl.ToString(), TheStruct, albumpic, false, TheObj, IsSilver);
                    IsSilver = IsSilver == false;

                    if (data["album_name"].ToString() == "Arcaea Sound Collection: Memories of Light")
                    {
                    }

                    anl++;
                }
            }
            catch (NullReferenceException) { }
            catch (Exception err)
            { TheSource.ShowBox("错误", err.ToString()); }
        }

        public async void ListPlayMusicButton_Click(object sender, RoutedEventArgs e)
        {
            TheMusicDatas.MusicData TheDatas = (TheMusicDatas.MusicData)(sender as Button).Tag;

            await PlaySet(TheDatas);
        }

        public void PauseMusicButton_Click(object sender, RoutedEventArgs e)
        {
            switch (NowPlayState)
            {
                case PlayState.Pause:
                    NowPlayState = PlayState.Play;
                    break;
                case PlayState.Play:
                    NowPlayState = PlayState.Pause;
                    break;
            }
        }

        public delegate void StateChangeDelegate(object state);
        public event StateChangeDelegate BigPageStateChangeEvent;

        private bool _IsOpenBigPage = false;
        public bool IsOpenBigPage
        {
            get
            {
                return _IsOpenBigPage;
            }
            set
            {
                _IsOpenBigPage = value;
                ImageButton.IsEnabled = false;
                if (value)
                {
                    MusicPage2.Visibility = Visibility.Visible;
                    Storyboard storyboard = Animations.animateOpacity(MusicPage2, 0, 1, AnimateOpacityNormalTime, IsAnimate: Animation);
                    storyboard.Completed += Storyboard_Completed;
                    storyboard.Begin();
                    MusicPages.ReCallTimer();
                    MusicPages.VolumeDataShow = MusicPages.VolumeDataShow;
                }
                else if (!value)
                {
                    MainUI.Visibility = Visibility.Visible;
                    Storyboard story = Animations.animateOpacity(MusicPage2, 1, 0, AnimateOpacityNormalTime, IsAnimate: Animation);
                    story.Completed += MusicPage2OpacityAnimateOver;
                    story.Begin();
                }
                if (BigPageStateChangeEvent != null) BigPageStateChangeEvent(value);
                IsShowAlbumImg = !value;
            }
        }

        private bool _IsShowAlbumImg = true;
        public bool IsShowAlbumImg
        {
            get { return _IsShowAlbumImg; }
            set
            {
                _IsShowAlbumImg = value;
                zilongcn.Animations animations = new Animations(Animation);
                if (value)
                {
                    animations.animateOpacity(PlayMusicBack_Grid, PlayMusicBack_Grid.Opacity, 1, 0.4, 1, 0);
                    animations.animatePosition(PlayBar_NowPlayTb, PlayBar_NowPlayTb.Margin, new Thickness(84, 0, 0, 0), 0.4, 1, 0);
                }
                else
                {
                    animations.animateOpacity(PlayMusicBack_Grid, PlayMusicAlbumPic.Opacity, 0, 0.4, 1, 0);
                    animations.animatePosition(PlayBar_NowPlayTb, PlayBar_NowPlayTb.Margin, new Thickness(8, 0, 0, 0), 0.4, 1, 0);
                }
                animations.Begin();
            }
        }

        public void SetBigPage(string PageType)
        {
            object ThePages = null;

            switch (PageType)
            {
                case "Music":
                    ThePages = MusicPages;
                    if (MusicPage2.Visibility == Visibility.Collapsed)
                    {
                        if (MusicPages.BackImage != null) MusicPages.BackImage.Margin = new Thickness(-Blurs);
                        MusicPages.CleanLrcColor();
                    }
                    break;

                case "Set":
                    ThePages = SetMusicPage;
                    SetMusicPage.BackImage.Margin = new Thickness(-Blurs);
                    break;

                case "LrcContent":
                    ThePages = new Pages.LrcContentPage();
                    break;
                default:
                    break;
            }

            if (MusicPage2.Visibility == Visibility.Visible)
            {
                IsOpenBigPage = false;
                return;
            }

            MusicPage2.Content = ThePages;
            IsOpenBigPage = true;
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            MainUI.Visibility = Visibility.Collapsed;
            ImageButton.IsEnabled = true;
        }

        public void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (NowPlaySong.SongRid == null)
            {
                if (SetMusicPage.NowFilePaths != null)
                {
                    SetBigPage("Set");
                    return;
                }
                else
                {
                    return;
                }
            }

            if (IsOpenBigPage && NowPlaySong.SongRid != MusicPages.MusicData.SongRid)
            {
                if (NowPlaySong.From != TheMusicDatas.MusicFrom.localMusic)
                {
                    MusicPages.Set(NowPlaySong, false);
                }
                else
                {
                    if (NowPlaySong.IsDownload != MusicPages.MusicData.IsDownload)
                    {
                        MusicPages.Set(NowPlaySong, false);
                    }
                }
            }
            MusicPages.SetBlur(Blurs);
            SetBigPage("Music");
        }

        private void MusicPage2OpacityAnimateOver(object sender, EventArgs args)
        {
            MusicPage2.Visibility = Visibility.Collapsed;
            ImageButton.IsEnabled = true;
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    SearchButton_Click(null, null);
                    break;
            }
        }

        private void NextMusicButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (NowPlayList == null) return;
            SetNextSong();
        }

        private void LastMusicButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (NowPlayList == null) return;
            SetBeforeSong();
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsFullScreen)
            {
                TheSource.tExitFullScreen();
            }
            else
            {
                TheSource.tFullScreen();
            }
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            NowPlayList.CleanList();
        }

        private Thickness ListLInThickness = new Thickness(0, 51, 5, 118);
        private Thickness ListLOutThickness = new Thickness(0, 51, -426, 118);
        private bool IsListLIn = false;

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsValueIn)
            {
                ValueButton_Click(null, null);
            }

            ListButton.IsEnabled = false;

            if (!IsListLIn)
            {
                ListLPage.Visibility = Visibility.Visible;

                Storyboard storyboard = Animations.animatePosition(ListLPage, ListLPage.Margin, ListLInThickness, 0.34, 1, 0, IsAnimate: Animation);
                storyboard.Completed += (s, args) => ListButton.IsEnabled = true;
                storyboard.Begin();

                IsListLIn = true;
                ClickBackShow = true;

                foreach (var i in TheListLList.Items)
                {
                    if ((i as ListLItem).MusicData.MD5 == NowPlaySong.MD5)
                    {
                        TheListLList.ScrollIntoView(i);
                        break;
                    }
                }
            }
            else
            {
                Storyboard storyboard = Animations.animatePosition(ListLPage, ListLPage.Margin, ListLOutThickness, 0.28, 0, 1, IsAnimate: Animation, easingMode: EasingMode.EaseIn);
                storyboard.Completed += (s, args) => { ListLPage.Visibility = Visibility.Collapsed; ListButton.IsEnabled = true; };
                storyboard.Begin();

                IsListLIn = false;
                ClickBackShow = false;
            }

            ListLPage.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private Thickness ValueInThickness = new Thickness(0, 0, 5, 118);
        private Thickness ValueOutThickness = new Thickness(0, 0, -446, 118);
        private bool IsValueIn = false;

        private void ValueButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsListLIn)
            {
                ListButton_Click(null, null);
            }

            if (IsValueIn)
            {
                Animations.animatePosition(MusicVolumePage, MusicVolumePage.Margin, ValueOutThickness, 0.28, 0, 1, IsAnimate: Animation, easingMode: EasingMode.EaseIn).Begin();
                IsValueIn = false;
                ClickBackShow = false;
            }
            else
            {
                Animations.animatePosition(MusicVolumePage, MusicVolumePage.Margin, ValueInThickness, 0.34, 1, 0, IsAnimate: Animation).Begin();
                IsValueIn = true;
                ClickBackShow = true;

                VolumeDataActivity();
            }
        }

        double[] d = null;
        private async void VolumeDataActivity()
        {
            while (IsValueIn)
            {
                d = audioPlayer.GetNowVolumeData().Take(30).ToArray();
                
                if (d != null && d.Length != 0)
                {
                    VolumeDataPb.Margin = new Thickness(10, 12, 426 - d.Average() * 2.7, 39);
                    VolumeDataPb.Opacity = 1;
                }

                await Task.Delay(10);
            }
        }

        private void MainActivity_Deactivated(object sender, EventArgs e)
        {
            Title1.Foreground = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            Menu.Foreground = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            FullScreenPath.Fill = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            FullScreenPath.Stroke = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            WindowButton_Close.Foreground = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            WindowButton_Min.Foreground = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            WindowButton_Max_Path.Fill = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            WindowButton_Max_Path.Stroke = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
            MusicPages.MoreButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 230, 230, 230));
        }

        private void MainActivity_Activated(object sender, EventArgs e)
        {
            Title1.Foreground = new SolidColorBrush(Colors.White);
            Menu.Foreground = new SolidColorBrush(Colors.White);
            FullScreenPath.Fill = new SolidColorBrush(Colors.White);
            FullScreenPath.Stroke = new SolidColorBrush(Colors.White);
            WindowButton_Close.Foreground = new SolidColorBrush(Colors.White);
            WindowButton_Min.Foreground = new SolidColorBrush(Colors.White);
            WindowButton_Max_Path.Fill = new SolidColorBrush(Colors.White);
            WindowButton_Max_Path.Stroke = new SolidColorBrush(Colors.White);
            MusicPages.MoreButton.Foreground = new SolidColorBrush(Colors.White);

            /*
            if (ProcessOperation.Anti.ReCallAnti())
            {
                if (SettingPages.AnimationOCBtn != null)
                {
                    SettingPages.AnimationOCBtn.IsChecked = false;
                    SettingPages.AnimationOCBtn.IsLocked = true;
                }
            }
            else
            {
                if (SettingPages.AnimationOCBtn != null)
                    SettingPages.AnimationOCBtn.IsLocked = false;
            }*/

            UpdataTheme();

            //SettingPages.DarkThemeOCBtn.IsChecked =
            /*if (SettingPages.AnimationOCBtn != null)
                SettingPages.AnimationOCBtn.IsChecked = zilongcn.Others.SystemEnableAnimation();*/
            /*
            var a = NowThemeData;
            a.ButtonBackColor = new SolidColorBrush(zilongcn.Others.GetSystemAccentColor());
            NowThemeData = a;*/
        }

        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsFullScreen)
            {
                base.OnMouseLeftButtonDown(e);
                this.DragMove();
            }
        }

        private void WindowButton_Close_Click(object sender, RoutedEventArgs e)
        {
            if (IsOpenBigPage)
            {
                IsOpenBigPage = false;
                return;
            }
            this.Close();
        }

        private void WindowButton_Min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void WindowButton_Max_Click(object sender, RoutedEventArgs e)
        {
            IsMaxScreen = !IsMaxScreen;
        }

        private MyC.PopupContent.ItemBarPopup ItemBarPopup = null;
        private MyC.PopupWindow Popups1 = null;
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            ItemBarPopup = new MyC.PopupContent.ItemBarPopup(NowPlaySong);
            Popups1 = new MyC.PopupWindow() { Content = ItemBarPopup, CloseExit = true, IsShowActivated = true, IsDeActivityClose = true, ForceAcrylicBlur = true, isWindowSmallRound = false };

            Popups1.Closed += (s, args) =>
            {
                ItemBarPopup = null;
                Popups1 = null;
            };

            Popups1.UserShow();
        }

        public List<DownloadCard> NowDownloadList = new List<DownloadCard>();

        public async void DownloadMusic(TheMusicDatas.MusicData TheDatas)
        {
            MusicDownload musicDownload = new MusicDownload(this);
            await musicDownload.StartDownload(TheDatas);
        }

        private void Msg_BalloonTipClicked(object sender, EventArgs e)
        {
            this.MessageIcon_MouseClick(null, null);
            SetPage("Download");
        }

        public void Msg_BalloonTipClicked1(object sender, EventArgs e)
        {
            this.MessageIcon_MouseClick(null, null);
            SetPage("Download");
        }

        public void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            NowVolume = (float)(VolumeSp.Value / 100);
        }

        public void ChangeVolume(bool up = true)
        {
            if (up)
            {
                VolumeSp.Value += 1;
            }
            else
            {
                VolumeSp.Value -= 1;
            }
            Slider_ValueChanged_1(null, null);

            if (!VolumePopup.IsShow)
            {
                if (!IsValueIn)
                {
                    if (!MainMenuPopup.IsShow)
                    {
                        if (!LrcWindow.IsShowGUI)
                        {
                            Point point = ValueButton.PointToScreen(new Point());
                            VolumePopup.UserShow(point.X - 66, point.Y - 70);
                        }
                    }
                }
            }
        }

        public void VolumeSp_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                ChangeVolume(false);
            }
            else
            {
                ChangeVolume();
            }
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            IsOpenMenu = !IsOpenMenu;
        }

        private void MainBtn_Checked(object sender, RoutedEventArgs e)
        {
            TestAnyMainBtnIsPressed((sender as MyC.znButton).Name);
        }

        public void LrcWindowOpenButton_Click(object sender, RoutedEventArgs e)
        {
            LrcWindow.IsShow = LrcWindow.IsShow == false;
            LrcWindow.IsLock = false;

            if (MusicPages.NowLrcData != null && MusicPages.NextLrcData != null)
            {
                LrcWindow.SetLrc(MusicPages.NowLrcData.LrcText, MusicPages.NextLrcData.LrcText);
            }

            MainMenuPopup.UserClose();
        }

        public void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            UserShow();
            MainMenuPopup.UserClose();
        }

        public async void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            MainMenuPopup.UserClose();
            audioPlayer.DisposeAll();
            LrcWindow.IsShow = false;
            this.WindowState = WindowState.Minimized;
            this.MessageIcon.Visible = false;
            await Task.Delay(500);
            Environment.Exit(0);
        }

        public SolidColorBrush GetCardColor(TheMusicDatas.MusicFrom from)
        {
            SolidColorBrush CardColor = null;
            Color kwColor = Color.FromArgb(255, 255, 228, 118);
            Color neteaseColor = Color.FromArgb(255, 221, 0, 27);
            Color kgColor = Color.FromArgb(255, 30, 132, 228);
            Color qqColor = Color.FromArgb(255, 18, 189, 116);
            Color miguColor = Color.FromArgb(255, 229, 60, 120);
            Color localColor = Color.FromArgb(255, 255, 133, 133);
            Color otherColor = Color.FromArgb(255, 133, 133, 133);

            switch (from)
            {
                case TheMusicDatas.MusicFrom.kwMusic:
                    CardColor = new SolidColorBrush(kwColor);
                    break;
                case TheMusicDatas.MusicFrom.neteaseMusic:
                    CardColor = new SolidColorBrush(neteaseColor);
                    break;
                case TheMusicDatas.MusicFrom.kgMusic:
                    CardColor = new SolidColorBrush(kgColor);
                    break;
                case TheMusicDatas.MusicFrom.qqMusic:
                    CardColor = new SolidColorBrush(qqColor);
                    break;
                case TheMusicDatas.MusicFrom.miguMusic:
                    CardColor = new SolidColorBrush(miguColor);
                    break;
                case TheMusicDatas.MusicFrom.localMusic:
                    CardColor = new SolidColorBrush(localColor);
                    break;
                default:
                    CardColor = new SolidColorBrush(otherColor);
                    break;
            }

            return CardColor;
        }

        private const Int32 MY_HOTKEYID = 0x9999;

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle)
        {
            if (wParam.ToInt32() == MY_HOTKEYID)
            {
                string Code = lParam.ToString();

                if (Code == "2424834" || Code == "11599872")
                {
                    SetBeforeSong();
                }
                else if (Code == "2555906" || Code == "11534336")
                {
                    SetNextSong();
                }
                else if (Code == "11665408" || Code == "2490370")
                {
                    NowPlayState = PlayState.Pause;
                    audioPlayer.Stop();
                }
                else if (Code == "11730944" || Code == "2621442")
                {
                    if (NowPlayState == PlayState.Play)
                    {
                        NowPlayState = PlayState.Pause;
                    }
                    else
                    {
                        NowPlayState = PlayState.Play;
                    }
                }
                else if (Code == "5177350")
                {
                    MenuOpen_Click(null, null);
                }
                else if (Code == "12255234")
                {
                    NowVolume += 0.01f;
                }
                else if (Code == "12386306")
                {
                    NowVolume -= 0.01f;
                }
                else if (Code == "4980742")
                {
                    LrcWindowOpenButton_Click(null, null);
                }
                else if (Code == "5373958")
                {
                    MusicPlayMod.NowPlayMod = MusicPlayMod.NowPlayMod == MusicPlayMod.PlayMod.Random ? MusicPlayMod.PlayMod.Seq : MusicPlayMod.PlayMod.Random;

                    if (MusicPlayMod.NowPlayMod == MusicPlayMod.PlayMod.Random) ShowMessage("随机播放", "随机播放已开启。");
                    else ShowMessage("随机播放", "随机播放已关闭。");
                }
                else
                {
                    ShowBox("WndProc", "无逻辑的IParam代码：" + Code);
                }
            }
            return IntPtr.Zero;
        }

        private IntPtr handle = IntPtr.Zero;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //var compositor = new WindowAccentCompositor(this);
            //compositor.Composite(Color.FromRgb(0x18, 0xa0, 0x5e));

            // 需要注册热键的列表
            var WillRegisterHotKeysList = new List<Tuple<uint, uint>>()
            {
                Tuple.Create((uint)0x0002, (uint)0x25),
                Tuple.Create((uint)0x0002, (uint)0x27),
                Tuple.Create((uint)0x0002, (uint)0x28),
                Tuple.Create((uint)0x0002, (uint)0x26),
                Tuple.Create((uint)0x0002, (uint)0xBD),
                Tuple.Create((uint)0x0002, (uint)0xBB),
                Tuple.Create((uint)0x4000, (uint)0xB0),
                Tuple.Create((uint)0x4000, (uint)0xB1),
                Tuple.Create((uint)0x4000, (uint)0xB2),
                Tuple.Create((uint)0x4000, (uint)0xB3),
                Tuple.Create(0x0002 + (uint)0x0004, (uint)0x4F),
                Tuple.Create(0x0002 + (uint)0x0004, (uint)0x4C),
                Tuple.Create(0x0002 + (uint)0x0004, (uint)0x52)
            };

            var ErrorCantCreatHotKeysList = new List<uint>();

            handle = new WindowInteropHelper(this).Handle;
            Handle = handle;

            // 循环列表注册热键
            foreach (Tuple<uint, uint> tuple in WillRegisterHotKeysList)
            {
                bool IsRegister = Source.RegisterHotKey(handle, MY_HOTKEYID, tuple.Item1, tuple.Item2);
                if (!IsRegister) ErrorCantCreatHotKeysList.Add(tuple.Item2);
            }

            if (ErrorCantCreatHotKeysList.Count > 0)
            {
                string Txt = "部分热键已被其他应用程序注册，因此此应用程序无法注册。如果想重新使用热键，请尝试关闭可能已注册和此应用程序相同热键的其他应用程序，并重启此应用程序以重新注册。\n\n已被其他应用程序注册的热键：\n";

                foreach (uint i in ErrorCantCreatHotKeysList)
                {
                    string HotKey = "未知热键: " + i;

                    if (i == 37) HotKey = "Ctrl + ← （播放上一首歌曲）";
                    if (i == 39) HotKey = "Ctrl + → （播放下一首歌曲）";
                    if (i == 40) HotKey = "Ctrl + ↓ （改变歌曲播放状态）";
                    if (i == 38) HotKey = "Ctrl + ↑ （停止歌曲播放）";
                    if (i == 189) HotKey = "Ctrl + - （减小播放歌曲音量）";
                    if (i == 187) HotKey = "Ctrl + + （增加播放歌曲音量）";
                    if (i == 176) HotKey = "媒体键下一首 （播放下一首歌曲）";
                    if (i == 177) HotKey = "媒体键上一首 （播放上一首歌曲）";
                    if (i == 178) HotKey = "媒体键播放/暂停 （改变歌曲播放状态）";
                    if (i == 179) HotKey = "媒体键停止（停止歌曲播放）";
                    if (i == 79) HotKey = "Shift + Ctrl + O （唤醒应用程序）";
                    if (i == 76) HotKey = "Shift + Ctrl + L （打开/关闭桌面歌词）";
                    if (i == 82) HotKey = "Shift + Ctrl + R （打开/关闭随机播放）";

                    Txt += HotKey + "\n";
                }

                ShowBox("热键注册失败", Txt + "\n未被其他应用程序注册的热键仍可以使用。");
            }

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        private void Popups_LostFocus(object sender, RoutedEventArgs e)
        {
            MainMenuPopup.UserClose();
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenuPopup.UserClose();
        }

        private void Popups_Opened(object sender, EventArgs e)
        {
        }

        private void ImageButton_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayMusicAlbumPic.Effect = new BlurEffect() { Radius = 7 };
            IsMusicLoadingAnime.Effect = new BlurEffect() { Radius = 10 };
            IsMusicLoadingText.Effect = new BlurEffect() { Radius = 10 };
        }

        private void ImageButton_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayMusicAlbumPic.Effect = null;
            IsMusicLoadingAnime.Effect = null;
            IsMusicLoadingText.Effect = null;
        }

        ~MainWindow()
        {

        }

        private void OCButton_Checked(MyC.OCButton sender)
        {

        }

        public void PopupMenu_LrcOCBtn_Checked(MyC.OCButton sender)
        {
            if (LrcWindow.IsShow == sender.IsChecked) return;

            LrcWindow.IsShow = sender.IsChecked;
        }

        public void PopupMenu_LrcLockOCBtn_Checked(MyC.OCButton sender)
        {
            if (LrcWindow.IsLock == sender.IsChecked) return;

            LrcWindow.IsLock = sender.IsChecked;
        }

        private Thickness WarningTextThickness = new Thickness(85, 16, 0, -3);
        private bool _isTime = false;

        public void PopupMenu_LrcLockOCBtn_LockedClick(MyC.OCButton sender)
        {
            if (_isTime) return;

            _isTime = true;
            _isTime = false;
        }

        private void MainActivity_DpiChanged(object sender, DpiChangedEventArgs e)
        {
            //ShowBox("注意", "系统DPI已修改，可能导致程序显示模糊。重启程序可使程序重新适配新DPI。");
        }

        private bool _WindowIsClose = false;
        public bool WindowIsClose
        {
            get
            {
                return _WindowIsClose;
            }
            set
            {
                _WindowIsClose = value;
            }
        }

        public void UserClose()
        {
            this.ShowInTaskbar = false;

            zilongcn.Others.HideAltTab(this);

            TheSource.MemoryClean();

            this.StateChanged += MainWindow_StateChanged;

            this.Visibility = Visibility.Hidden;

            WindowIsClose = true;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.UserShow();
            }

            if (this.WindowState == WindowState.Minimized)
            {
                WindowIsClose = true;
                SetTaskbarState(TaskbarItemProgressState.None);
            }
        }

        public void UserShow()
        {
            WindowIsClose = false;

            Others.SetWindowLong(GetWindowHandle(), -20, IntPtr.Zero);
            this.StateChanged -= MainWindow_StateChanged;
            this.ShowInTaskbar = true;
            Flash();

            this.Visibility = Visibility.Visible;
            this.WindowState = WindowState.Normal;
            this.Activate();

            if (WindowStateChangedEvent != null) WindowStateChangedEvent();
        }

        public bool WindowIsOpen()
        {
            if (this.Visibility == Visibility.Hidden) return false;
            if (this.WindowState == WindowState.Minimized) return false;

            return true;
        }

        private void PlayDeviceText_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (audioPlayer.AudioOutApi == AudioPlayer.AudioOutApiEnum.WaveOut)
            {
                audioPlayer.AudioOutApi = AudioPlayer.AudioOutApiEnum.DirectSound;
            }
            else if (audioPlayer.AudioOutApi == AudioPlayer.AudioOutApiEnum.DirectSound)
            {
                audioPlayer.AudioOutApi = AudioPlayer.AudioOutApiEnum.Wasapi;
            }
            else if (audioPlayer.AudioOutApi == AudioPlayer.AudioOutApiEnum.Wasapi)
            {
                audioPlayer.AudioOutApi = AudioPlayer.AudioOutApiEnum.Asio;
            }
            else if (audioPlayer.AudioOutApi == AudioPlayer.AudioOutApiEnum.Asio)
            {
                audioPlayer.AudioOutApi = AudioPlayer.AudioOutApiEnum.WaveOut;
            }

            audioPlayer.Reload();
            PlayDeviceText.Content = $"{audioPlayer.AudioOutApi}";
            */
        }

        private void MainActivity_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                IsMaxScreen = false;
            }
            else if (WindowState == WindowState.Maximized)
            {
                IsMaxScreen = true;
            }

            if (WindowStateChangedEvent != null) WindowStateChangedEvent();
        }

        private bool _ClickBackShow = false;
        public bool ClickBackShow
        {
            get { return _ClickBackShow; }
            set
            {
                _ClickBackShow = value;
                if (value)
                {
                    ClickBack.Visibility = Visibility.Visible;
                }
                else
                {
                    ClickBack.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void ClickBack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsValueIn) ValueButton_Click(null, null);
            if (IsListLIn) ListButton_Click(null, null);
            ClickBackShow = false;
        }

        private void ValueButton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!VolumePopup.IsShow)
            {
                Point point = ValueButton.PointToScreen(new Point());
                VolumePopup.UserShow(point.X - 66, point.Y - 70);
            }
        }

        [DllImport("user32.dll", EntryPoint = "keybd_event", SetLastError = true)]
        public static extern void keybd_event(System.Windows.Forms.Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        public const int KEYEVENTF_KEYUP = 2;
        public bool IsMouseEnterWindowBtn_Max = false;

        private async void WindowButton_Max_MouseEnter(object sender, MouseEventArgs e)
        {
            if (OSVersion != "11") return;

            IsMouseEnterWindowBtn_Max = true;

            await Task.Delay(990);

            if (IsMouseEnterWindowBtn_Max)
            {
                IsMouseEnterWindowBtn_Max = false;

                Activate();

                await Task.Delay(10);

                keybd_event(System.Windows.Forms.Keys.LWin, 0, 0, 0);
                keybd_event(System.Windows.Forms.Keys.Z, 0, 0, 0);
                keybd_event(System.Windows.Forms.Keys.LWin, 0, KEYEVENTF_KEYUP, 0);
            }
        }

        private void WindowButton_Max_MouseLeave(object sender, MouseEventArgs e)
        {
            IsMouseEnterWindowBtn_Max = false;
        }

        private void MainActivity_TouchDown(object sender, TouchEventArgs e)
        {

        }

        private void MainActivity_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowBox("", "");
        }

        private void MainActivity_MouseWheel(object sender, MouseWheelEventArgs e)
        {

        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            IsMaxScreen = !IsMaxScreen;
        }

        private async void PlayDeviceText_ContentClick(MyC.znComboBox znComboBox, object data)
        {
            audioPlayer.NowOutDevice = (MusicPlay.AudioPlayer.OutDevice)data;
            await audioPlayer.Reload();
        }

        private void PlayDeviceText_ButtonClick(object sender, EventArgs e)
        {
            (PlayDeviceText.ClickShowContent as MyC.PopupContent.SelectSoundOutContent).UpdataList();
        }
    }
}