using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// SettingBar.xaml 的交互逻辑
    /// </summary>
    public partial class SettingBar : UserControl
    {
        public delegate void ClickDelegate(SettingBar settingBar);
        public event ClickDelegate Click;

        public FrameworkElement IconContent
        {
            get { return iconVisual.Content as FrameworkElement; }
            set
            {
                iconVisual.Content = value;
            }
        }

        public string Title
        {
            get { return TitleTb.Text; }
            set
            {
                TitleTb.Text = value;
                if (value.Length == 0) TitleTb.Visibility = Visibility.Collapsed;
                else TitleTb.Visibility = Visibility.Visible;
            }
        }

        public string Describe
        {
            get { return DescribeTb.Text; }
            set
            {
                DescribeTb.Text = value;
                if (value.Length == 0) DescribeTb.Visibility = Visibility.Collapsed;
                else DescribeTb.Visibility = Visibility.Visible;
            }
        }

        bool _styleOpen = false;
        public bool StyleOpen
        {
            get { return _styleOpen; }
            set
            {
                _styleOpen = value;
                if (_styleOpen)
                {
                    if (OpenStyleList.Content != null)
                    {
                        CornerRadius = new CornerRadius(5, 5, 0, 0);
                        BarStyle2_RTF.Angle = 90;
                        OpenStyleList.Visibility = Visibility.Visible;
                        Height = maincontent.Height + 2 + OpenStyleList.Height;
                        zilongcn.Animations.animatePosition(OpenStyleList.Content as FrameworkElement, new Thickness(0, -(OpenStyleList.Content as FrameworkElement).ActualHeight, 0, 0), new Thickness(0, 0, 0, 0), 0.35, 1, 0, System.Windows.Media.Animation.EasingMode.EaseOut, MainWindow.Animate).Begin();
                    }
                }
                else
                {
                    BarStyle2_RTF.Angle = -90;
                    //Height = maincontent.Height + 2;
                    //OpenStyleList.Visibility = Visibility.Collapsed;
                    if (OpenStyleList.Content != null)
                    {
                        var a = zilongcn.Animations.animatePosition(OpenStyleList.Content as FrameworkElement, (OpenStyleList.Content as FrameworkElement).Margin, new Thickness(0, -(OpenStyleList.Content as FrameworkElement).ActualHeight, 0, 0), 0.35, 1, 0, System.Windows.Media.Animation.EasingMode.EaseOut, MainWindow.Animate);
                        a.Completed += (s, e) =>
                        {
                            if (!_styleOpen)
                            {
                                OpenStyleList.Visibility = Visibility.Collapsed;
                                CornerRadius = new CornerRadius(5);
                            }
                        };
                        a.Begin();

                    }
                }
            }
        }

        public FrameworkElement OpenVisual
        {
            get { return OpenStyleList.Content as FrameworkElement; }
            set { OpenStyleList.Content = value; }
        }

        FrameworkElement _QuestionContent = null;
        public FrameworkElement QuestionContent
        {
            get { return _QuestionContent; }
            set
            {
                _QuestionContent = value;
                if (value != null) QuestVisual.Visibility = Visibility.Visible;
                else QuestVisual.Visibility = Visibility.Collapsed;
            }
        }

        public CornerRadius CornerRadius
        {
            get { return mainback.CornerRadius; }
            set
            {
                mainback.CornerRadius = value;
            }
        }

        public enum SettingBarStyles { Default, Open, Show, Content }
        private SettingBarStyles _SettingBarStyle = SettingBarStyles.Default;
        public SettingBarStyles SettingBarStyle
        {
            get { return _SettingBarStyle; }
            set
            {
                _SettingBarStyle = value;
                maincontent.Height = 66;
                maincontent_back.Height = 66;
                Height = maincontent.Height + 2;
                UserVisual.Margin = new Thickness(0);
                switch (value)
                {
                    case SettingBarStyles.Default:
                        BarStyle1.Visibility = Visibility.Visible;
                        BarStyle2.Visibility = Visibility.Collapsed;
                        break;
                    case SettingBarStyles.Open:
                        BarStyle1.Visibility = Visibility.Collapsed;
                        BarStyle2.Visibility = Visibility.Visible;
                        break;
                    case SettingBarStyles.Show:
                        BarStyle1.Visibility = Visibility.Collapsed;
                        BarStyle2.Visibility = Visibility.Collapsed;
                        break;
                    case SettingBarStyles.Content:
                        maincontent.Height = 56;
                        maincontent_back.Height = 56;
                        Height = 56 + 2;
                        UserVisual.Margin = new Thickness(0, 0, 46, 0);
                        BarStyle1.Visibility = Visibility.Collapsed;
                        BarStyle2.Visibility = Visibility.Collapsed;
                        break;
                }
                UserVisual.Visibility = Visibility.Visible;
            }
        }

        public FrameworkElement ButtonContent
        {
            get { return UserVisual.Content as FrameworkElement; }
            set
            {
                UserVisual.Content = value;
            }
        }

        public SettingBar()
        {
            InitializeComponent();
            SizeChanged += (s, e) =>
            {
                if (OpenStyleList.Content != null)
                {
                    FrameworkElement a = OpenStyleList.Content as FrameworkElement;
                    a.Width = ActualWidth;
                    OpenContentClip.Rect = new Rect(0, 0, ActualWidth, ActualHeight);
                    //a.Margin = new Thickness(a.Margin.Left, 4, a.Margin.Right, a.Margin.Bottom);
                }
            };
            Awaiter();
        }

        public async void Awaiter()
        {
            await Task.Delay(1);
            try
            {
                App.window.ThemeChangeEvent += (data) => updataTheme();
                App.window.BlurThemeChangeEvent += (data) => updataTheme();
            }
            catch { }
            if (SettingBarStyle == SettingBarStyles.Open)
            {
                if (ButtonContent == null)
                    StyleOpen = _styleOpen;
            }
        }

        public void updataTheme()
        {
            if (App.OSVersion == "11" && zilongcn.Others.SystemEnableBlurBehind() && !zilongcn.Others.AppsUseLightTheme())
            {
                maincontent_back.Background = MainWindow.DarkThemeData.ButtonBackColor;

                mainbackEnterAnimation.Background = App.window.NowBlurThemeData.ButtonEnterColor;
                Foreground = App.window.NowBlurThemeData.InColorTextColor;
                UserVisual.Foreground = App.window.NowBlurThemeData.InColorTextColor;

                BarStyle1.Fill = App.window.NowBlurThemeData.InColorTextColor;
                BarStyle2.Fill = App.window.NowBlurThemeData.InColorTextColor;
            }
            else
            {
                maincontent_back.Background = App.window.NowThemeData.ButtonBackColor;
                mainbackEnterAnimation.Background = App.window.NowThemeData.ButtonEnterColor;
                Foreground = App.window.NowThemeData.InColorTextColor;
                UserVisual.Foreground = App.window.NowThemeData.InColorTextColor;

                BarStyle1.Fill = App.window.NowThemeData.InColorTextColor;
                BarStyle2.Fill = App.window.NowThemeData.InColorTextColor;
            }

            mainbackMouseDownAnimation.Background = App.window.NowBlurThemeData.ButtonEnterColor;

            if (iconVisual.Content != null)
            {
                if (iconVisual.Content.GetType() == typeof(Path))
                {
                    (iconVisual.Content as Path).Fill = Foreground;
                    (iconVisual.Content as Path).Stroke = Foreground;
                }
                else if (iconVisual.Content.GetType() == typeof(TextBlock))
                {
                    (iconVisual.Content as TextBlock).Foreground = Foreground;
                }
                else if (iconVisual.Content.GetType() == typeof(MaterialDesignThemes.Wpf.PackIcon))
                {
                    (iconVisual.Content as MaterialDesignThemes.Wpf.PackIcon).Foreground = Foreground;
                }
            }
            /*
            if (App.window.NowThemeData.MD5 == App.window.DefaultThemeData.MD5)
            {
                mainback.BorderBrush = new SolidColorBrush(Color.FromArgb(50, 155, 155, 155));
            }
            else
            {
                mainback.BorderBrush = new SolidColorBrush(Color.FromArgb(50, 155, 155, 155));
            }
            */

            if (SettingBarStyle == SettingBarStyles.Content || true)
            {
                mainback.BorderBrush = null;
            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (SettingBarStyle == SettingBarStyles.Content) return;

            zilongcn.Animations.animateOpacity(mainbackEnterAnimation, mainbackEnterAnimation.Opacity, 1, 0.3, IsAnimate: MainWindow.Animate).Begin();
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SettingBarStyle == SettingBarStyles.Content) return;

            zilongcn.Animations.animateOpacity(mainbackEnterAnimation, mainbackEnterAnimation.Opacity, 0, 0.3, IsAnimate: MainWindow.Animate).Begin();
            zilongcn.Animations.animateOpacity(mainbackMouseDownAnimation, mainbackMouseDownAnimation.Opacity, 0, 0.3, IsAnimate: MainWindow.Animate).Begin();
        }

        bool mouseDown = false;
        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SettingBarStyle == SettingBarStyles.Content) return;

            mouseDown = true;
            zilongcn.Animations.animateOpacity(mainbackMouseDownAnimation, mainbackMouseDownAnimation.Opacity, 1, 0.3, IsAnimate: MainWindow.Animate).Begin();
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SettingBarStyle == SettingBarStyles.Content) return;

            zilongcn.Animations.animateOpacity(mainbackMouseDownAnimation, mainbackMouseDownAnimation.Opacity, 0, 0.3, IsAnimate: MainWindow.Animate).Begin();
            if (mouseDown)
            {
                mouseDown = false;
                if (Click != null) Click(this);
            }
        }

        MyC.PopupWindow popupWindow = null;
        private void QuestVisual_MouseEnter(object sender, MouseEventArgs e)
        {
            if (QuestionContent != null)
            {
                try
                {
                    TextBlock tb = (QuestionContent as TextBlock);
                    tb.Foreground = App.window.NowBlurThemeData.TextColor;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                }
                catch { }

                popupWindow = new MyC.PopupWindow() { Content = QuestionContent, ForceAcrylicBlur = true, IsShowActivated = false, CloseExit = true, FollowMouse = true };
                popupWindow.Closed += (s, args) =>
                {
                    GC.Collect();
                };
                popupWindow.UserShow();
            }
        }

        private void QuestVisual_MouseLeave(object sender, MouseEventArgs e)
        {
            if (popupWindow != null)
            {
                popupWindow.UserClose();
                popupWindow = null;
            }
        }
    }
}
