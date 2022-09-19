using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace znMusicPlayerWPF.MyC
{
    /// <summary>
    /// PopupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PopupWindow : Window
    {
        private bool _FollowMouse = false;

        public bool CloseExit { get; set; } = true;

        public Color BlurBackgroundColor { get; set; } = Color.FromArgb(1, 0, 0, 0);

        public Color BackgroundColor
        {
            get
            {
                return (Background as SolidColorBrush).Color;
            }
            set
            {
                Background = new SolidColorBrush(value);
            }
        }

        public bool FollowMouse
        {
            get { return _FollowMouse; }
            set
            {
                _FollowMouse = value;
                if (value)
                {
                    FollowMouseAsync();
                }
            }
        }

        public bool AutoResize { get; set; } = true;

        public bool IsShow { get; set; } = false;

        public bool IsShowActivated { get; set; } = false;

        public bool IsDeActivityClose { get; set; } = false;

        public bool ForceAcrylicBlur { get; set; } = false;

        public bool isWindowSmallRound { get; set; } = true;

        System.Drawing.Graphics currentGraphics = null;
        private double DpiX = 1;
        private double DpiY = 1;
        private double TheLeft = 0;
        private double TheTop = 0;
        private WindowBlurEffect.WindowAccentCompositor compositor = null;
        public bool IsClosing = false;

        public PopupWindow()
        {
            InitializeComponent();
            compositor = new WindowBlurEffect.WindowAccentCompositor(this);
            this.ShowInTaskbar = false;
        }

        public void SetWindowRound(zilongcn.Others.DWM_WINDOW_CORNER_PREFERENCE type)
        {
            var preference = type;
            zilongcn.Others.SetWindowRound(this, zilongcn.Others.DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref preference, sizeof(uint));
        }

        public void UserShow(double? positionX = null, double? positionY = null, double? width = null, double? height = null, bool roundWindow = true, bool animation = true)
        {
            if (IsClosing) return;
            try
            {
                var c = BackgroundColor;
                var b = new SolidColorBrush(Colors.White).Color;
                if (c.A == b.A && c.R == b.R && c.G == b.G && c.B == b.B)
                    BackgroundColor = (App.window.FindResource("BlurBackColor") as SolidColorBrush).Color;
            }
            catch { }

            this.IsShow = true;
            this.Topmost = true;
            this.Width = 0;
            this.Height = 0;
            this.Opacity = 0;
            this.ShowActivated = false;
            this.Show();

            Reload(positionX, positionY, width, height, roundWindow);

            if (IsShowActivated) this.Activate();

            zilongcn.Animations.animateOpacity(this, 0, 1, 0.2, IsAnimate: GetAnimateData() && animation).Begin();
            zilongcn.Others.HideAltTab(this);
        }

        public void UserClose()
        {
            IsShow = false;
            IsClosing = true;
            _FollowMouse = false;

            try
            {
                compositor.Composite(WindowBlurEffect.WindowAccentCompositor.AccentState.ACCENT_DISABLED, BlurBackgroundColor);
                SetWindowRound(zilongcn.Others.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DONOTROUND);
            }
            catch { }

            var sb = zilongcn.Animations.animateOpacity(this, 1, 0, 0.2, IsAnimate: GetAnimateData());
            sb.Completed += (s, e) =>
            {
                if (CloseExit) this.Close();
                else
                {
                    this.Left = 0 - this.ActualWidth - 500;
                    IsClosing = false;
                }
            };
            sb.Begin();
        }

        public void Reload(double? positionX = null, double? positionY = null, double? width = null, double? height = null, bool roundWindow = true)
        {
            Title = Content.ToString();

            SetSize(width: width, height: height);
            SetPosition(positionX, positionY);

            if (GetBlurData())
            {
                try
                {
                    BackgroundColor = (App.window.NowBlurThemeData.BackColor as SolidColorBrush).Color;
                }
                catch { }
                compositor.Composite(WindowBlurEffect.WindowAccentCompositor.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, BlurBackgroundColor, ForceAcrylicBlur);
            }
            else
            {
                try
                {
                    var a = (App.window.NowBlurThemeData.BackColor as SolidColorBrush).Color;
                    BackgroundColor = a;
                    if (BackgroundColor == (MainWindow.DefaultBlurThemeData.BackColor as SolidColorBrush).Color)
                    {
                        a = Color.FromArgb(255, 240, 240, 240);
                    }
                    else
                    {
                        a = Color.FromArgb(255, 50, 50, 50);
                    }

                    a.A = 255;
                    BackgroundColor = a;
                }
                catch { }
            }

            if (App.OSVersion != "11")
            {
                BorderBrush = MainWindow.DefaultThemeData.ALineColor;
                BorderThickness = new Thickness(1);
            }
            else if (roundWindow)
            {
                var preference = isWindowSmallRound ? zilongcn.Others.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUNDSMALL : zilongcn.Others.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;
                SetWindowRound(preference);
            }
        }

        public void SetSize(double? width = null, double? height = null)
        {
            FrameworkElement _obj = Content as FrameworkElement;

            Width = width == null ? (AutoResize ? _obj.ActualWidth + 16 : _obj.ActualWidth) : (double)width;
            Height = height == null ? (AutoResize ? _obj.ActualHeight + 10 : _obj.ActualHeight) : (double)height;
        }

        public void SetPosition(double? positionX = null, double? positionY = null)
        {
            //currentGraphics = System.Drawing.Graphics.FromHwnd(new WindowInteropHelper(Application.Current.MainWindow).Handle);

            // 计算系统Dpi
            //DpiX = currentGraphics.DpiX / 96;
            //DpiY = currentGraphics.DpiY / 96;

            // 感知显示器Dpi
            DpiX = VisualTreeHelper.GetDpi(this).DpiScaleX;
            DpiY = VisualTreeHelper.GetDpi(this).DpiScaleY;

            if (positionX != null && positionY != null)
            {
                TheLeft = (double)positionX / DpiX;
                TheTop = (double)positionY / DpiY;
            }
            else
            {
                TheLeft = System.Windows.Forms.Control.MousePosition.X / DpiX + 2;
                TheTop = System.Windows.Forms.Control.MousePosition.Y / DpiY + 24;
            }

            // 计算系统分辨率并防止窗口溢出屏幕
            if (TheLeft + ActualWidth > Source.ScreenManager.GetScreenActualWidth())
                TheLeft = Source.ScreenManager.GetScreenActualWidth() - ActualWidth + 2;
            if (TheTop + ActualHeight > Source.ScreenManager.GetScreenActualHeight())
                TheTop = TheTop - ActualHeight - 24;

            Left = TheLeft;
            Top = TheTop;
        }

        public void SetPositionWindowCenter(Window window)
        {
            if (App.window.WindowState == WindowState.Maximized)
            {
                SetPosition(
                    Source.ScreenManager.GetScreenWidth() / 2 - ActualWidth * DpiX / 2,
                    Source.ScreenManager.GetScreenHeight() / 2 - ActualHeight * DpiY / 2
                );
            }
            else
            {
                SetPosition(
                    window.Left * DpiX + window.ActualWidth * DpiX / 2 - ActualWidth * DpiX / 2,
                    window.Top * DpiY + window.ActualHeight * DpiY / 2 - ActualHeight * DpiY / 2
                    );
            }
        }

        private async void FollowMouseAsync()
        {
            while (_FollowMouse)
            {
                SetPosition();
                await Task.Delay(1);
            }
        }

        private bool GetAnimateData()
        {
            try
            {
                return MainWindow.Animate;
            }
            catch
            {
                return false;
            }
        }

        private bool GetBlurData()
        {
            try
            {
                return MainWindow.SystemBlur;
            }
            catch
            {
                return false;
            }
        }

        private void PopupWindow_Deactivated(object sender, EventArgs e)
        {
            if (IsShowActivated && IsDeActivityClose) UserClose();
        }

        public static void SetPopupShow(FrameworkElement node, FrameworkElement content, SolidColorBrush background = null, bool followMouse = false, int endTime = 1500, Window popupOwner = null, bool ForceAcrylicBlur = true)
        {
            PopupWindow popupWindow = null;
            int ShowTime = 0;
            bool IsMouseIn = false;
            bool IsTouch = false;
            bool IsMouseDown = false;

            node.MouseEnter += async (s, e) =>
            {
                IsMouseIn = true;
                while (IsMouseIn)
                {
                    ShowTime += 500;

                    if (ShowTime >= endTime)
                    {
                        if (!IsMouseDown)
                        {
                            popupWindow = new PopupWindow() { Owner = popupOwner, ForceAcrylicBlur = ForceAcrylicBlur };
                            popupWindow.Content = content;
                            if (background != null) popupWindow.BackgroundColor = background.Color;
                            popupWindow.UserShow();
                            popupWindow.FollowMouse = followMouse;

                            ShowTime = 0;
                            IsMouseIn = false;

                            break;
                        }
                    }

                    await Task.Delay(500);
                }
            };

            node.TouchDown += (s, e) =>
            {
                IsTouch = true;
            };

            node.MouseLeave += (s, e) =>
            {
                IsMouseDown = false;
                ShowTime = 0;
                IsMouseIn = false;
                try { if (!popupWindow.IsClosing) popupWindow.UserClose(); }
                catch { }
            };

            /*
            Node.MouseMove += (s, e) =>
            {
                try
                {
                    ShowTime = 0;
                    if (!popupWindow.IsClosing && !popupWindow.FollowMouse) popupWindow.UserClose();
                }
                catch { }
            };*/

            node.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(
                (s, e) =>
                {
                    IsMouseDown = true;
                    ShowTime = 0;
                    try { if (!popupWindow.IsClosing && !IsTouch && !popupWindow.FollowMouse) popupWindow.UserClose(); }
                    catch { }
                }
            ), true);
        }

        public static void SetPopupShow(FrameworkElement node, string text, SolidColorBrush background = null, SolidColorBrush Foreground = null, bool followMouse = false, int endTime = 1500, Window popupOwner = null, bool ForceAcrylicBlur = true)
        {
            PopupWindow popupWindow = null;
            if (Foreground == null) Foreground = new SolidColorBrush(Color.FromArgb(255, 254, 254, 254));
            TextBlock textBlock = new TextBlock()
            {
                Text = text,
                FontSize = (double)App.Current.FindResource("提示字体大小"),
                Foreground = Foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            int ShowTime = 0;
            bool IsMouseIn = false;
            bool IsTouch = false;
            bool IsMouseDown = false;

            node.MouseEnter += async (s, e) =>
            {
                IsMouseIn = true;
                while (IsMouseIn)
                {
                    ShowTime += 500;

                    if (ShowTime >= endTime)
                    {
                        if (!IsMouseDown)
                        {
                            textBlock.Foreground = Foreground.Color == Color.FromArgb(255, 254, 254, 254) ? App.window.NowBlurThemeData.TextColor : Foreground;

                            popupWindow = new PopupWindow() { Owner = popupOwner, ForceAcrylicBlur = ForceAcrylicBlur };
                            popupWindow.Content = textBlock;
                            if (background != null) popupWindow.BackgroundColor = background.Color;
                            popupWindow.UserShow();
                            popupWindow.FollowMouse = followMouse;

                            ShowTime = 0;
                            IsMouseIn = false;

                            break;
                        }
                    }

                    await Task.Delay(500);
                }
            };

            node.TouchDown += (s, e) =>
            {
                IsTouch = true;
            };

            node.MouseLeave += (s, e) =>
            {
                IsMouseDown = false;
                ShowTime = 0;
                IsMouseIn = false;
                try { if (!popupWindow.IsClosing) popupWindow.UserClose(); }
                catch { }
            };

            /*
            Node.MouseMove += (s, e) =>
            {
                try
                {
                    ShowTime = 0;
                    if (!popupWindow.IsClosing && !popupWindow.FollowMouse) popupWindow.UserClose();
                }
                catch { }
            };*/

            node.AddHandler(FrameworkElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(
                (s, e) =>
                {
                    IsMouseDown = true;
                    ShowTime = 0;
                    try { if (!popupWindow.IsClosing && !IsTouch && !popupWindow.FollowMouse) popupWindow.UserClose(); }
                    catch { }
                }
            ), true);
            /*SetPopupShow(Node, new TextBlock()
            {
                Text = Text,
                FontSize = (double)App.Current.FindResource("提示字体大小"),
                Foreground = Foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }, Background, FollowMouse, EndTime, Owner, ForceAcrylicBlur, TheParent);*/
        }
    }
}
