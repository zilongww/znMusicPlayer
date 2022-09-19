using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using znMusicPlayerWPF.WindowBlurEffect;

namespace znMusicPlayerWPF
{
    /// <summary>
    /// DesktopLrcWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DesktopLrcWindow : Window
    {
        private MainWindow TheParent = null;
        private Source TheSource = null;
        private bool _IsShow = false;
        private bool _IsCenter = false;

        private WindowAccentCompositor compositor = null;

        public bool IsShowGUI = false;

        public bool IsTranslate { get; set; } = false;

        public bool IsShow
        {
            get { return _IsShow; }
            set
            {
                if (value == false)
                {
                    this.Left = 0 - this.ActualWidth - 500;
                    this.Visibility = Visibility.Hidden;
                }
                else
                {
                    this.Visibility = Visibility.Visible;
                    this.Left = Source.ScreenManager.GetScreenActualWidth() / 2 - this.ActualWidth / 2;
                    this.Top = Source.ScreenManager.GetScreenActualHeight() / 2 - this.ActualHeight / 2;

                    var preference = zilongcn.Others.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                    zilongcn.Others.SetWindowRound(this, zilongcn.Others.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(uint));
                }
                _IsShow = value;
            }
        }

        private bool _IsLock = false;
        public bool IsLock
        {
            get { return _IsLock; }
            set
            {
                if (value == false)
                {
                    LockPath.Data = this.FindResource("开锁") as Geometry;
                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    SetWindowLong(hwnd, -20, 0x80);
                }
                else
                {
                    LockPath.Data = this.FindResource("锁") as Geometry;
                    this.Background = null;
                    bkb.Background = new SolidColorBrush(Colors.Transparent);

                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    SetWindowLong(hwnd, -20, 0x20 + 0x80);

                    TheParent.ShowMessage("桌面歌词已锁定", "使用 热键Ctrl + Shift + L 或 在应用程序中 将桌面歌词关闭即可解除锁定。", 4, System.Windows.Forms.ToolTipIcon.Info);
                }
                _IsLock = value;
            }
        }

        public bool IsCenter
        {
            get { return SettingDataEdit.ToBool(SettingDataEdit.GetParam("LrcCenter")); }
            set
            {
                _IsCenter = value;

                MakeCenter(value);

                SettingDataEdit.SetParam("LrcCenter", value.ToString());

                if (TheParent.openedsetting) TheParent.SettingPages.DesktopLrcCenterOCBtn.IsChecked = value;
            }
        }

        public TextBlock OtherLrc = null;

        private TextBlock _NowChoiceLrc = null;
        public TextBlock NowChoiceLrc
        {
            get { return _NowChoiceLrc; }
            set
            {
                value.Foreground = TheParent.LrcColor;
                if (value == Lrc1) { Lrc2.Foreground = TheParent.OtherColor; OtherLrc = Lrc2; }
                else { Lrc1.Foreground = TheParent.OtherColor; OtherLrc = Lrc1; }
                _NowChoiceLrc = value;
            }
        }

        private int _LrcSize = 28;
        public int LrcSize
        {
            get
            {
                return _LrcSize;
            }
            set
            {
                if (value < 4 || value > 58) return;
                SetLrcSize(value);
            }
        }

        [DllImport("user32", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, int NewLong);

        public static void SetWindowTran(IntPtr hwnd, int nIndex, int NewLong)
        {
            SetWindowLong(hwnd, nIndex, NewLong);
        }

        public void SetLrcSize(int value)
        {
            _LrcSize = value;
            SettingDataEdit.SetParam("DesktopLrcSize", value.ToString());

            Lrc1.FontSize = value;
            Lrc2.FontSize = value;

            //await Task.Delay(500);

            MinHeight = Buttons.ActualHeight + LrcBack.ActualHeight;
            MaxHeight = MinHeight;
            Height = MinHeight;
        }

        public DesktopLrcWindow(MainWindow theParent)
        {
            this.FontFamily = this.FindResource("znNormal") as FontFamily;
            TheParent = theParent;
            InitializeComponent();
            compositor = new WindowAccentCompositor(this);

            MyC.PopupWindow.SetPopupShow(VolumeGrid, "滑动鼠标滚轮以调整音量");

            Loaded += (s, e) =>
            {
                zilongcn.Others.HideAltTab(this);
            };

            StateChanged += (s, e) =>
            {
                if (WindowState == WindowState.Maximized) ExitMaxSize.Visibility = Visibility.Visible;
                else ExitMaxSize.Visibility = Visibility.Collapsed;
            };

            TheParent.MusicPages.LrcChangeEvent += (nowLrcData, nextLrcData) =>
            {
                UpdataLrc(nowLrcData, nextLrcData);
            };
        }

        public void MakeCenter(bool value)
        {
            if (value)
            {
                Lrc1.TextAlignment = TextAlignment.Center;
                Lrc2.TextAlignment = TextAlignment.Center;
            }
            else
            {
                Lrc1.TextAlignment = TextAlignment.Left;
                Lrc2.TextAlignment = TextAlignment.Right;
            }
        }

        public void UpdataLrc(Pages.MusicPage.LrcData NowLrc, Pages.MusicPage.LrcData NextLrc)
        {
            if (IsShow)
            {
                if (IsTranslate)
                {
                    if (bool.Parse(SettingDataEdit.GetParam("ShowTranslateOnly")))
                    {
                        Lrc1.Text = NextLrc.LrcText;
                        Lrc2.Text = "";
                    }
                    else
                    {
                        Lrc1.Text = NowLrc.LrcText;
                        Lrc2.Text = NextLrc.LrcText;
                    }

                    NowChoiceLrc = Lrc1;
                    MakeCenter(true);
                }
                else
                {
                    Pages.MusicPage.LrcData lrcData = Lrc2.Tag as Pages.MusicPage.LrcData;
                    bool lrcDataIsEmpty = false;

                    if (lrcData == null) lrcDataIsEmpty = true;
                    else
                    {
                        if ((Lrc2.Tag as Pages.MusicPage.LrcData).MD5 == NowLrc.MD5)
                        {
                            Lrc1.Text = NextLrc.LrcText;
                            Lrc2.Text = NowLrc.LrcText;

                            Lrc1.Tag = NextLrc;
                            Lrc2.Tag = NowLrc;

                            NowChoiceLrc = Lrc2;
                        }
                        else lrcDataIsEmpty = true;
                    }

                    if (lrcDataIsEmpty)
                    {
                        Lrc1.Text = NowLrc.LrcText;
                        Lrc2.Text = NextLrc.LrcText;

                        Lrc1.Tag = NowLrc;
                        Lrc2.Tag = NextLrc;

                        NowChoiceLrc = Lrc1;
                    }

                    MakeCenter(IsCenter);
                }

            }
        }

        public void UserShow(MainWindow Parent)
        {
            this.Top = -1000;
            this.Left = -1000;
            IsLock = false;
            TheParent = Parent;
            TheSource = Parent.TheSource;
            this.Show();
            TheParent.lrcDelegate += SetLrc;
            Lrc1.Foreground = TheParent.LrcColor;
        }

        public void SetLrc(string Lrc1Text, string Lrc2Text)
        {
            Lrc1.Text = Lrc1Text;
            Lrc2.Text = Lrc2Text;
        }

        private bool IsMoveWindow = false;
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !IsLock)
            {
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    IsMoveWindow = true;
                    DragMove();
                    IsMoveWindow = false;
                }
            }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsMoveWindow = false;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsLock) return;

            var preference = zilongcn.Others.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;

            Buttons.Visibility = Visibility;

            zilongcn.Animations animations = new zilongcn.Animations(TheParent.Animation);

            animations.storyboard.Completed += (s, args) =>
            {
                Buttons.Visibility = Visibility.Visible;

                if (Buttons.Visibility != Visibility.Collapsed) return;
                // win11使用圆角
                zilongcn.Others.SetWindowRound(this,
                                               zilongcn.Others.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE,
                                               ref preference,
                                               sizeof(uint)
                );
            };

            animations.animateOpacity(Buttons, Buttons.Opacity, 1, 0.2);

            if (MainWindow.SystemBlur)
            {
                compositor.Composite(WindowAccentCompositor.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, Color.FromArgb(1, 0, 0, 0));
                animations.animateColor(bkb, (bkb.Background as SolidColorBrush).Color, Color.FromArgb(80, 90, 90, 90), 0.2);
            }
            else
            {
                animations.animateColor(bkb, (bkb.Background as SolidColorBrush).Color, Color.FromArgb(130, 0, 0, 0), 0.2);
            }

            animations.Begin();
            IsMoveWindow = false;

            zilongcn.Others.SetWindowRound(this, zilongcn.Others.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(uint));

            IsShowGUI = true;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsMoveWindow)
            {
                var preference = zilongcn.Others.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND;
                zilongcn.Others.SetWindowRound(this, zilongcn.Others.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(uint));

                zilongcn.Animations animations = new zilongcn.Animations(TheParent.Animation);
                animations.animateColor(bkb, (bkb.Background as SolidColorBrush).Color, Color.FromArgb(1, 0, 0, 0), 0.11);
                animations.animateOpacity(Buttons, Buttons.Opacity, 0, 0.15);
                animations.storyboard.Completed += (s, args) =>
                {
                    Buttons.Visibility = Visibility.Collapsed;

                };
                animations.Begin();

                if (MainWindow.SystemBlur)
                {
                    compositor.Composite(WindowAccentCompositor.AccentState.ACCENT_DISABLED, Color.FromArgb(0, 0, 0, 0));
                }

                IsShowGUI = false;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.PauseMusicButton_Click(null, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TheParent.SetNextSong();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TheParent.SetBeforeSong();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            IsShow = IsShow == false;
        }

        private void PauseButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            TheParent.MenuOpen_Click(null, null);
        }

        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.NowVolume = -1;
        }

        private void VolumeButton_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TheParent.VolumeSp_MouseWheel(sender, e);
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            IsLock = IsLock == false;
        }

        private bool AltKeyDown = false;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);
            if (key == Key.LeftAlt || key == Key.RightAlt)
            {
                AltKeyDown = true;
            }

            if (AltKeyDown)
            {
                switch (key)
                {
                    case Key.F4:
                        break;
                    case Key.Space:
                        break;
                    default:
                        break;
                }
                e.Handled = true;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightAlt || e.Key == Key.LeftAlt)
            {
                AltKeyDown = false;
                e.Handled = true;
            }
        }

        private void ExitMaxSize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            LrcSize++;
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            LrcSize--;
        }
    }
}
