using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace znMusicPlayerWPF.MyC
{
    public delegate void ComboBoxResultDelegate(object data);

    public interface IznComboBox
    {
        event ComboBoxResultDelegate result;
        void UpdataThemeDelay();
    }

    /// <summary>
    /// znComboBox.xaml 的交互逻辑
    /// </summary>
    public partial class znComboBox : UserControl
    {
        public delegate void ContentClickDelegate(znComboBox znComboBox, object data);
        public event ContentClickDelegate ContentClick;
        public event EventHandler ButtonClick;

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius), typeof(znComboBox),
            new PropertyMetadata(new CornerRadius(5))
            );

        public static readonly new DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
            "BorderThickness",
            typeof(Thickness), typeof(znComboBox),
            new PropertyMetadata(new Thickness(1))
            );

        public static readonly new DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground",
            typeof(Brush), typeof(znComboBox),
            new PropertyMetadata(new SolidColorBrush(Colors.Black))
            );

        public static readonly new DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background",
            typeof(Brush), typeof(znComboBox),
            new PropertyMetadata(new SolidColorBrush(Colors.White))
            );

        public static readonly new DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            "BorderBrush",
            typeof(Brush), typeof(znComboBox),
            new PropertyMetadata(null)
            );

        public static readonly DependencyProperty EnterColorProperty = DependencyProperty.Register(
            "EnterColor",
            typeof(Brush), typeof(znComboBox),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(35, 0, 0, 0))
                )
            );

        public static readonly DependencyProperty ShowWidthProperty = DependencyProperty.Register(
            "ShowWidth",
            typeof(double), typeof(znComboBox),
            new PropertyMetadata(0D)
            );

        public static readonly DependencyProperty LeftMarginProperty = DependencyProperty.Register(
            "LeftMargin",
            typeof(double), typeof(znComboBox),
            new PropertyMetadata(10D)
            );

        public enum ShowDirections { Up, Down }
        public static readonly DependencyProperty ShowDirectionProperty = DependencyProperty.Register(
            "ShowDirection",
            typeof(ShowDirections), typeof(znComboBox),
            new PropertyMetadata(ShowDirections.Down)
            );

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string), typeof(znComboBox),
            new PropertyMetadata("选择")
            );

        FrameworkElement _clickShowContent = null;
        public FrameworkElement ClickShowContent
        {
            get { return _clickShowContent; }
            set
            {
                _clickShowContent = value;
            }
        }

        public double LeftMargin
        {
            get { return (double)GetValue(LeftMarginProperty); }
            set
            {
                SetValue(LeftMarginProperty, value);
                if (Title != null)
                {
                    Title.Margin = new Thickness(value, 0, 0, 0);
                }
            }
        }

        public double ShowWidth
        {
            get { return (double)GetValue(ShowWidthProperty); }
            set
            {
                SetValue(ShowWidthProperty, value);
            }
        }

        public ShowDirections ShowDirection
        {
            get { return (ShowDirections)GetValue(ShowDirectionProperty); }
            set
            {
                SetValue(ShowDirectionProperty, value);
                if (path != null)
                {
                    path.RenderTransform = new RotateTransform(ShowDirection == ShowDirections.Down ? -90 : 90);
                }
            }
        }

        public new Thickness BorderThickness
        {
            get
            {
                return (Thickness)GetValue(BorderThicknessProperty);
            }
            set
            {
                SetValue(BorderThicknessProperty, value);
                if (button != null)
                {
                    button.BorderThickness = value;
                }
            }
        }

        public CornerRadius CornerRadius
        {
            get
            {
                return (CornerRadius)GetValue(CornerRadiusProperty);
            }
            set
            {
                SetValue(CornerRadiusProperty, value);
                if (button != null)
                {
                    button.CRadius = value;
                }
            }
        }

        public new Brush Foreground
        {
            get
            {
                return (Brush)GetValue(ForegroundProperty);
            }
            set
            {
                if (button != null)
                {
                    button.Foreground = value;
                    Title.Foreground = value;
                    path.Fill = value;
                }
                try
                {
                    SetValue(ForegroundProperty, value);
                }
                catch { }
            }
        }

        public new Brush Background
        {
            get
            {
                return (Brush)GetValue(BackgroundProperty);
            }
            set
            {
                if (button != null)
                {
                    button.Background = value;
                }
                try
                {
                    SetValue(BackgroundProperty, value);
                }
                catch { }
            }
        }

        public new Brush BorderBrush
        {
            get
            {
                return (Brush)GetValue(BorderBrushProperty);
            }
            set
            {
                if (button != null)
                {
                    button.BorderBrush = value;
                }
                try
                {
                    SetValue(BorderBrushProperty, value);
                }
                catch { }
            }
        }

        public Brush EnterColor
        {
            get
            {
                return (Brush)GetValue(EnterColorProperty);
            }
            set
            {
                if (button != null)
                {
                    button.EnterColor = value;
                }
                try
                {
                    SetValue(EnterColorProperty, value);
                }
                catch { }
            }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                if (Title != null) Title.Text = value;
                SetValue(TextProperty, value);
            }
        }

        public znComboBox()
        {
            InitializeComponent();
        }

        TextBlock Title;
        private void Title_Loaded(object sender, RoutedEventArgs e)
        {
            Title = sender as TextBlock;
            Title.Margin = new Thickness(LeftMargin, 0, 0, 0);
            Title.Text = Text;
            Title.Foreground = Foreground;
            Title.FontSize = FontSize;
        }

        Path path;
        private void path_Loaded(object sender, RoutedEventArgs e)
        {
            path = sender as Path;
            path.RenderTransform = new RotateTransform(ShowDirection == ShowDirections.Down ? -90 : 90);
            path.Fill = Foreground;
        }

        public bool AnimationEllipse { get; set; } = true;
        znButton button;
        private void znButton_Loaded(object sender, RoutedEventArgs e)
        {
            button = sender as znButton;
            button.Background = Background;
            button.EnterColor = EnterColor;
            button.BorderBrush = BorderBrush;
            button.BorderThickness = BorderThickness;
            button.CRadius = CornerRadius;
            button.IsAnimateCR = AnimationEllipse;
            //MainContent.Background = null;
        }

        PopupWindow popupWindow = null;
        private async void znButton_Click(object sender, RoutedEventArgs e)
        {
            if (ButtonClick != null) ButtonClick(sender, e);
            if (ClickShowContent == null) return;

            popupWindow = new PopupWindow() { Content = ClickShowContent, ForceAcrylicBlur = true, isWindowSmallRound = false, IsShowActivated = true, IsDeActivityClose = true, CloseExit = true };

            popupWindow.Closed += (s, args) =>
            {
                popupWindow = null;
            };

            if (popupWindow == null) return;

            Point point = this.PointToScreen(new Point());
            popupWindow.UserShow(point.X - 4, point.Y - 4, ShowWidth == 0 ? (this.ActualWidth + 8) : ShowWidth, 0, false, false);

            if (popupWindow == null) return;

            var st = zilongcn.Animations.animateHeight(popupWindow, 0, ClickShowContent.ActualHeight, 0.36, IsAnimate: MainWindow.Animate);
            if (ShowDirection == ShowDirections.Up)
            {
                popupWindow.SizeChanged += (s, args) =>
                {
                    try
                    {
                        popupWindow.Top = point.Y / VisualTreeHelper.GetDpi(this).DpiScaleY - popupWindow.ActualHeight + ActualHeight;
                    }
                    catch { }
                };
            }
            st.Begin();

            if (MainWindow.Animate)
                await Task.Delay(50);
            popupWindow.SetWindowRound(zilongcn.Others.DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND);

            try
            {
                (ClickShowContent as IznComboBox).result += (data) =>
                {
                    if (ContentClick != null) ContentClick(this, data);
                    if (popupWindow != null) popupWindow.UserClose();
                };
            }
            catch { }
        }
    }
}
