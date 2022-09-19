using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace znMusicPlayerWPF.MyC
{
    /// <summary>
    /// OCButton.xaml 的交互逻辑
    /// </summary>
    public partial class OCButton : Grid
    {
        Rectangle rectangle;
        Rectangle rectangle1;
        Canvas style2animateBack;
        Ellipse ellipse;
        MaterialDesignThemes.Wpf.PackIcon icon;
        Viewbox viewbox;

        Grid Style1;
        Grid Style2;

        public delegate void CheckedDelegate(OCButton sender);
        public event CheckedDelegate Checked;
        public delegate void LockedClickDelegate(OCButton sender);
        public event LockedClickDelegate LockedClick;

        public static readonly DependencyProperty EAOffestProperty = DependencyProperty.Register(
            "EAOffest",
            typeof(double), typeof(OCButton),
            new PropertyMetadata(8.0)
            );

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness",
            typeof(double), typeof(OCButton),
            new PropertyMetadata(1.0)
            );

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked",
            typeof(bool), typeof(OCButton),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty IsLockedProperty = DependencyProperty.Register(
            "IsLocked",
            typeof(bool), typeof(OCButton),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty IsRadioButtonStyleProperty = DependencyProperty.Register(
            "IsRadioButtonStyle",
            typeof(bool), typeof(OCButton),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty ContentHorizontalAlignmentProperty = DependencyProperty.Register(
            "ContentHorizontalAlignment",
            typeof(HorizontalAlignment), typeof(OCButton),
            new PropertyMetadata(HorizontalAlignment.Left)
            );

        public new static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.Transparent)
                )
            );

        public static readonly DependencyProperty OCBackgroundProperty = DependencyProperty.Register(
            "OCBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(255, 51, 51, 51))
                )
            );

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(255, 51, 51, 51))
                )
            );

        public static readonly DependencyProperty MouseEnterBackgroundProperty = DependencyProperty.Register(
            "MouseEnterBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.Transparent)
                )
            );

        public static readonly DependencyProperty MouseEnterStrokeProperty = DependencyProperty.Register(
            "MouseEnterStroke",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
                )
            );

        public static readonly DependencyProperty MouseEnterOCBackgroundProperty = DependencyProperty.Register(
            "MouseEnterOCBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(255, 0, 0, 0))
                )
            );

        public static readonly DependencyProperty IsCheckedBackgroundProperty = DependencyProperty.Register(
            "IsCheckedBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(255, 255, 133, 133))
                )
            );

        public static readonly DependencyProperty IsCheckedStrokeProperty = DependencyProperty.Register(
            "IsCheckedStroke",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(255, 255, 133, 133))
                )
            );

        public static readonly DependencyProperty IsCheckedOCBackgroundProperty = DependencyProperty.Register(
            "IsCheckedOCBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.White)
                )
            );

        public static readonly DependencyProperty IsCMouseEnterBackgroundProperty = DependencyProperty.Register(
            "IsCMouseEnterBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(255, 255, 133, 133))
                )
            );

        public static readonly DependencyProperty IsCMouseEnterStrokeProperty = DependencyProperty.Register(
            "IsCMouseEnterStroke",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(255, 255, 133, 133))
                )
            );

        public static readonly DependencyProperty IsCMouseEnterOCBackgroundProperty = DependencyProperty.Register(
            "IsCMouseEnterOCBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.White)
                )
            );

        public static readonly DependencyProperty IsMouseDownBackgroundProperty = DependencyProperty.Register(
            "IsMouseDownBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.Silver)
                )
            );

        public static readonly DependencyProperty IsMouseDownStrokeProperty = DependencyProperty.Register(
            "IsMouseDownStroke",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.Silver)
                )
            );

        public static readonly DependencyProperty IsMouseDownOCBackgroundProperty = DependencyProperty.Register(
            "IsMouseDownOCBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.WhiteSmoke)
                )
            );

        public static readonly DependencyProperty IsLockedBackgroundProperty = DependencyProperty.Register(
            "IsLockedBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.Silver)
                )
            );

        public static readonly DependencyProperty IsLockedStrokeProperty = DependencyProperty.Register(
            "IsLockedStroke",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.Silver)
                )
            );

        public static readonly DependencyProperty IsLockedOCBackgroundProperty = DependencyProperty.Register(
            "IsLockedOCBackground",
            typeof(Brush), typeof(OCButton),
            new PropertyMetadata(
                new SolidColorBrush(Colors.WhiteSmoke)
                )
            );

        private bool _IsPress = false;

        public double EAOffest
        {
            get { return (double)GetValue(EAOffestProperty); }
            set
            {
                SetValue(EAOffestProperty, value);
            }
        }

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set
            {
                SetValue(StrokeThicknessProperty, value);

                if (viewbox != null)
                {
                    rectangle.StrokeThickness = value;
                    rectangle1.StrokeThickness = value;
                }
            }
        }

        public HorizontalAlignment ContentHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(ContentHorizontalAlignmentProperty); }
            set
            {
                SetValue(HorizontalAlignmentProperty, value);

                if (viewbox != null) viewbox.HorizontalAlignment = value;
            }
        }

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set
            {
                SetValue(IsCheckedProperty, value);

                if (Checked != null)
                {
                    try { Checked(this); }
                    catch { }
                }

                try
                {
                    zilongcn.Animations animations = new zilongcn.Animations(GetAnimateData());

                    if (value)
                    {
                        animations.animateOpacity(icon, icon.Opacity, 1, 0.1);
                        animations.animatePosition(ellipse, ellipse.Margin, new Thickness(style1.ActualWidth - EAOffest - ellipse.ActualWidth, 0, 0, 0), 0.3, 1, 0);
                        animations.animatePosition(style2animateBack, style2animateBack.Margin, new Thickness(0, 0, 1, 0), 0.3, 1, 0);
                        icon.Visibility = Visibility.Visible;
                        rectangle.Fill = IsCheckedBackground;
                        rectangle.Stroke = IsCheckedStroke;
                        style2animateBack.Background = IsCheckedBackground;
                        rectangle1.Stroke = IsCheckedStroke;
                        ellipse.Fill = IsCheckedOCBackground;
                    }
                    else
                    {
                        animations.animatePosition(ellipse, ellipse.Margin, new Thickness(EAOffest, 0, 0, 0), 0.3, 1, 0);
                        animations.animatePosition(style2animateBack, style2animateBack.Margin, new Thickness(0, 0, 50, 0), 0.3, 1, 0);
                        icon.Visibility = Visibility.Collapsed;
                        rectangle.Fill = Background;
                        rectangle.Stroke = Stroke;
                        style2animateBack.Background = IsCheckedBackground;
                        rectangle1.Stroke = Stroke;
                        ellipse.Fill = OCBackground;
                        animations.storyboard.Completed += (s, e) =>
                        {
                            if (!IsChecked)
                                zilongcn.Animations.animateOpacity(icon, icon.Opacity, 0, 0.22, IsAnimate: GetAnimateData()).Begin();
                        };
                    }
                    animations.Begin();
                }
                catch { }
            }
        }

        public bool IsLocked
        {
            get { return (bool)GetValue(IsLockedProperty); ; }
            set
            {
                SetValue(IsLockedProperty, value);

                try
                {
                    if (value)
                    {
                        rectangle.Fill = IsLockedBackground;
                        rectangle.Stroke = IsLockedStroke;
                        rectangle1.Fill = IsLockedBackground;
                        ellipse.Fill = IsLockedOCBackground;
                    }
                    else
                    {
                        if (IsChecked)
                        {
                            rectangle.Fill = IsCheckedBackground;
                            rectangle.Stroke = IsCheckedStroke;
                            ellipse.Fill = IsCheckedOCBackground;
                        }
                        else
                        {
                            rectangle.Fill = Background;
                            rectangle.Stroke = Stroke;
                            ellipse.Fill = OCBackground;
                        }
                        rectangle1.Fill = null;
                    }

                }
                catch { }
            }
        }

        public bool IsRadioButtonStyle
        {
            get { return (bool)GetValue(IsRadioButtonStyleProperty); }
            set
            {
                SetValue(IsRadioButtonStyleProperty, value);
                if (value)
                {
                    style1.Visibility = Visibility.Collapsed;
                    style2.Visibility = Visibility.Visible;

                }
                else
                {
                    style1.Visibility = Visibility.Visible;
                    style2.Visibility = Visibility.Collapsed;
                }
            }
        }

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set
            {
                SetValue(BackgroundProperty, value);
                if (rectangle != null)
                {
                    rectangle.Fill = value;
                }
            }
        }

        public Brush OCBackground
        {
            get { return (Brush)GetValue(OCBackgroundProperty); }
            set
            {
                SetValue(OCBackgroundProperty, value);
                if (ellipse != null) ellipse.Fill = value;
            }
        }

        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set
            {
                SetValue(StrokeProperty, value);
                if (rectangle != null)
                {
                    rectangle.Stroke = value;
                    rectangle1.Stroke = value;
                }
            }
        }

        public Brush MouseEnterBackground
        {
            get { return (Brush)GetValue(MouseEnterBackgroundProperty); }
            set { SetValue(MouseEnterBackgroundProperty, value); }
        }

        public Brush MouseEnterStroke
        {
            get { return (Brush)GetValue(MouseEnterStrokeProperty); }
            set { SetValue(MouseEnterStrokeProperty, value); }
        }

        public Brush MouseEnterOCBackground
        {
            get { return (Brush)GetValue(MouseEnterOCBackgroundProperty); }
            set { SetValue(MouseEnterOCBackgroundProperty, value); }
        }

        public Brush IsCheckedBackground
        {
            get { return (Brush)GetValue(IsCheckedBackgroundProperty); }
            set { SetValue(IsCheckedBackgroundProperty, value); }
        }

        public Brush IsCheckedStroke
        {
            get { return (Brush)GetValue(IsCheckedStrokeProperty); }
            set { SetValue(IsCheckedStrokeProperty, value); }
        }

        public Brush IsCheckedOCBackground
        {
            get { return (Brush)GetValue(IsCheckedOCBackgroundProperty); }
            set { SetValue(IsCheckedOCBackgroundProperty, value); }
        }

        public Brush IsCMouseEnterBackground
        {
            get { return (Brush)GetValue(IsCMouseEnterBackgroundProperty); }
            set { SetValue(IsCMouseEnterBackgroundProperty, value); }
        }

        public Brush IsCMouseEnterStroke
        {
            get { return (Brush)GetValue(IsCMouseEnterStrokeProperty); }
            set { SetValue(IsCMouseEnterStrokeProperty, value); }
        }

        public Brush IsCMouseEnterOCBackground
        {
            get { return (Brush)GetValue(IsCMouseEnterOCBackgroundProperty); }
            set { SetValue(IsCMouseEnterOCBackgroundProperty, value); }
        }

        public Brush IsMouseDownBackground
        {
            get { return (Brush)GetValue(IsMouseDownBackgroundProperty); }
            set { SetValue(IsMouseDownBackgroundProperty, value); }
        }

        public Brush IsMouseDownStroke
        {
            get { return (Brush)GetValue(IsMouseDownStrokeProperty); }
            set { SetValue(IsMouseDownStrokeProperty, value); }
        }

        public Brush IsMouseDownOCBackground
        {
            get { return (Brush)GetValue(IsMouseDownOCBackgroundProperty); }
            set { SetValue(IsMouseDownOCBackgroundProperty, value); }
        }

        public Brush IsLockedBackground
        {
            get { return (Brush)GetValue(IsLockedBackgroundProperty); }
            set { SetValue(IsLockedBackgroundProperty, value); }
        }

        public Brush IsLockedStroke
        {
            get { return (Brush)GetValue(IsLockedStrokeProperty); }
            set { SetValue(IsLockedStrokeProperty, value); }
        }

        public Brush IsLockedOCBackground
        {
            get { return (Brush)GetValue(IsLockedOCBackgroundProperty); }
            set { SetValue(IsLockedOCBackgroundProperty, value); }
        }

        public OCButton()
        {
            InitializeComponent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsLocked) return;

            if (IsChecked)
            {
                if (rectangle != null)
                {
                    rectangle.Fill = IsCMouseEnterBackground;
                    rectangle.Stroke = IsCMouseEnterStroke;
                    style2animateBack.Background = IsCMouseEnterBackground;
                    rectangle1.Stroke = IsCMouseEnterStroke;
                }
                if (ellipse != null) ellipse.Fill = IsCMouseEnterOCBackground;
            }
            else
            {
                if (rectangle != null)
                {
                    rectangle.Fill = MouseEnterBackground;
                    rectangle.Stroke = MouseEnterStroke;
                    style2animateBack.Background = MouseEnterBackground;
                    rectangle1.Stroke = MouseEnterStroke;
                }
                if (ellipse != null) ellipse.Fill = MouseEnterOCBackground;

            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsLocked) return;

            if (IsChecked)
            {
                if (rectangle != null)
                {
                    rectangle.Fill = IsCheckedBackground;
                    rectangle.Stroke = IsCheckedStroke;
                    style2animateBack.Background = IsCheckedBackground;
                    rectangle1.Stroke = IsCheckedStroke;
                }
                if (ellipse != null) ellipse.Fill = IsCheckedOCBackground;
            }
            else
            {
                if (rectangle != null)
                {
                    rectangle.Fill = Background;
                    rectangle.Stroke = Stroke;
                    style2animateBack.Background = IsCheckedBackground;
                    rectangle1.Stroke = Stroke;
                }
                if (ellipse != null) ellipse.Fill = OCBackground;
            }
        }

        private void Rectangle_Loaded(object sender, RoutedEventArgs e)
        {
            rectangle = sender as Rectangle;
            rectangle.Fill = Background;
            rectangle.Stroke = Stroke;
            rectangle.StrokeThickness = StrokeThickness;
            if (IsChecked) IsChecked = true;
            if (IsLocked) IsLocked = true;
        }

        private void Rectangle_Loaded_1(object sender, RoutedEventArgs e)
        {

            rectangle1 = sender as Rectangle;
            rectangle1.Stroke = Stroke;
            rectangle1.StrokeThickness = StrokeThickness;
            if (IsChecked) IsChecked = true;
            if (IsLocked) IsLocked = true;
        }

        private void Ellipse_Loaded(object sender, RoutedEventArgs e)
        {
            ellipse = sender as Ellipse;
            ellipse.Fill = OCBackground;
            if (IsChecked) IsChecked = true;
            if (IsLocked) IsLocked = true;
        }

        private void Viewbox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_IsPress) return;

            if (IsLocked)
            {
                try { LockedClick(this); }
                catch { }
                return;
            }


            IsChecked = !IsChecked;
            _IsPress = false;
        }

        private void Viewbox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsPress = true;

            if (IsLocked) return;

            if (rectangle != null)
            {
                rectangle.Fill = IsMouseDownBackground;
                rectangle.Stroke = IsMouseDownStroke;
            }
            if (ellipse != null) ellipse.Fill = IsMouseDownOCBackground;
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

        private void style2animateBack_Loaded(object sender, RoutedEventArgs e)
        {
            style2animateBack = sender as Canvas;
            style2animateBack.Background = IsCheckedBackground;

            IsChecked = IsChecked;
        }

        private void style1_Loaded(object sender, RoutedEventArgs e)
        {
            Style1 = sender as Grid;

            if (IsRadioButtonStyle) IsRadioButtonStyle = true;
        }
        private void style2_Loaded(object sender, RoutedEventArgs e)
        {
            Style2 = sender as Grid;

            if (IsRadioButtonStyle) IsRadioButtonStyle = true;
        }

        private void PackIcon_Loaded(object sender, RoutedEventArgs e)
        {
            icon = sender as MaterialDesignThemes.Wpf.PackIcon;
            icon.Foreground = IsCheckedOCBackground;

            IsChecked = IsChecked;
        }

        private void Viewbox_Loaded(object sender, RoutedEventArgs e)
        {
            viewbox = sender as Viewbox;

            viewbox.HorizontalAlignment = ContentHorizontalAlignment;
        }
    }
}
