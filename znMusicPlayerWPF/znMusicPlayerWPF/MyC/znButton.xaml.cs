using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace znMusicPlayerWPF.MyC
{
    /// <summary>
    /// znButton.xaml 的交互逻辑
    /// </summary>
    public partial class znButton : Button
    {
        Grid parents;
        Path CR;
        EllipseGeometry CR1;
        Grid MainGrid;
        Border Back;
        Border EnterColorBack;
        TextBlock ContentText;
        TextBlock EnterContentText;
        ContentPresenter TheContent;

        public static readonly DependencyProperty AnimationColorProperty = DependencyProperty.Register(
            "AnimationColor",
            typeof(Brush), typeof(znButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(100, 0, 0, 0))
                )
            );

        public static readonly DependencyProperty EnterColorProperty = DependencyProperty.Register(
            "EnterColor",
            typeof(Brush), typeof(znButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(35, 0, 0, 0))
                )
            );

        public static readonly DependencyProperty EnterForegroundColorProperty = DependencyProperty.Register(
            "EnterForegroundColor",
            typeof(Brush), typeof(znButton),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(0, 0, 0, 0))
                )
            );

        public static readonly DependencyProperty CRadiusProperty = DependencyProperty.Register(
            "CRadius",
            typeof(CornerRadius), typeof(znButton),
            new PropertyMetadata(new CornerRadius(5))
            );

        private bool _ShowContent = false;
        private object _PopupContent = null;
        private bool _IsPressLast = false;

        public bool IsAnimateCR { get; set; } = true;

        public bool ClipCRToBounds { get; set; } = true;

        public Brush AnimationColor
        {
            get
            {
                return (Brush)GetValue(AnimationColorProperty);
            }
            set
            {
                if (CR != null)
                {
                    CR.Fill = value;
                }
                SetValue(AnimationColorProperty, value);
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
                if (EnterColorBack != null)
                {
                    EnterColorBack.Background = value;
                    EnterColorBack.BorderBrush = value;
                }
                try
                {
                    SetValue(EnterColorProperty, value);
                }
                catch { }
            }
        }

        public Brush EnterForegroundColor
        {
            get
            {
                return (Brush)GetValue(EnterForegroundColorProperty);
            }
            set
            {
                if (EnterContentText != null)
                {
                    EnterContentText.Foreground = value;
                }
                SetValue(EnterForegroundColorProperty, value);
            }
        }

        public bool ShowContent
        {
            get { return _ShowContent; }
            set
            {
                _ShowContent = value;
                if (_ShowContent)
                {
                    if (TheContent != null)
                    {
                        try { TheContent.Visibility = Visibility.Visible; }
                        catch { }
                    }
                }
            }
        }

        public CornerRadius CRadius
        {
            get { return (CornerRadius)GetValue(CRadiusProperty); }
            set
            {
                SetValue(CRadiusProperty, value);
                SetCRadius(value);
            }
        }

        public znButton()
        {
            InitializeComponent();
            this.AddHandler(Button.MouseUpEvent, new RoutedEventHandler(bt_MouseUp), true);
        }

        private void SetCRadius(CornerRadius value)
        {
            try
            {
                Back.CornerRadius = value;
                EnterColorBack.CornerRadius = value;
                CR.Clip = new RectangleGeometry(
                    new Rect(0, 0, this.ActualWidth - 2, this.ActualHeight - 2),
                    this.Back.CornerRadius.TopLeft - 1,
                    this.Back.CornerRadius.TopLeft - 1);
            }
            catch { }
        }

        private void Back_Loaded(object sender, RoutedEventArgs e)
        {
            Back = sender as Border;
            SetCRadius(CRadius);
        }

        private void EnterColorBack_Loaded(object sender, RoutedEventArgs e)
        {
            EnterColorBack = sender as Border;
            SetCRadius(CRadius);

            EnterColorBack.Opacity = 0;
            EnterColorBack.Background = EnterColor;
            EnterColorBack.BorderBrush = EnterColor;
        }

        private void ContentText_Loaded(object sender, RoutedEventArgs e)
        {
            ContentText = sender as TextBlock;
        }

        private void EnterContentText_Loaded(object sender, RoutedEventArgs e)
        {
            EnterContentText = sender as TextBlock;

            EnterContentText.Opacity = 0;
            EnterContentText.Foreground = EnterForegroundColor;
        }

        private void CR_Loaded(object sender, RoutedEventArgs e)
        {
            this.CR = sender as Path;
            this.CR1 = this.CR.Data as EllipseGeometry;

            this.CR.Fill = AnimationColor;
            if (ClipCRToBounds)
            {
                this.CR.SizeChanged += CR_SizeChanged;
                this.SizeChanged += CR_SizeChanged;
                CR_SizeChanged(null, null);
            }
            else
            {
                this.CR.Clip = null;
            }
        }

        private void CR_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                this.CR.Clip = new RectangleGeometry(new Rect(0, 0, this.ActualWidth - 2, this.ActualHeight - 2), this.Back.CornerRadius.TopLeft - 1, this.Back.CornerRadius.TopLeft - 1);
            }
            catch { }
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            this.MainGrid = sender as Grid;
        }

        private void ContentPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            TheContent = sender as ContentPresenter;

            if (ShowContent) ShowContent = true;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _IsPressLast = true;

            EllipseGeometry ellipse = Template.FindName("CR1", this) as EllipseGeometry;
            Path path = Template.FindName("CR", this) as Path;

            if (IsAnimateCR && GetAnimateData())
            {
                ellipse.Center = Mouse.GetPosition(this);

                DoubleAnimation doubleAnimation = new DoubleAnimation()
                {
                    From = 0,
                    To = this.ActualWidth * 1.5,
                    Duration = new Duration(TimeSpan.FromSeconds(2.6)),
                    EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
                };

                ellipse.BeginAnimation(EllipseGeometry.RadiusXProperty, doubleAnimation);
                zilongcn.Animations.animateOpacity(path, 1, 0, 10).Begin();
            }
            else
            {
                ellipse.RadiusX = this.ActualWidth * 1.5;
                zilongcn.Animations.animateOpacity(path, path.Opacity, 1, 0.2, 0, 0, IsAnimate: GetAnimateData()).Begin();
            }
        }

        private void bt_MouseUp(object sender, RoutedEventArgs e)
        {
            if (!_IsPressLast) return;

            EllipseGeometry ellipse = Template.FindName("CR1", this) as EllipseGeometry;
            Path path = Template.FindName("CR", this) as Path;

            if (IsAnimateCR && GetAnimateData())
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation()
                {
                    From = ellipse.RadiusX,
                    To = this.ActualWidth * 1.5,
                    Duration = new Duration(TimeSpan.FromSeconds(0.7))
                };

                ellipse.BeginAnimation(EllipseGeometry.RadiusXProperty, doubleAnimation);
                zilongcn.Animations.animateOpacity(path, path.Opacity, 0.0, 0.7, 0, 0, easingMode: EasingMode.EaseIn).Begin();
            }
            else
            {
                zilongcn.Animations.animateOpacity(path, path.Opacity, 0.0, 0.25, 0, 0, IsAnimate: GetAnimateData()).Begin();
            }

            _IsPressLast = false;
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!IsEnabled) return;

            zilongcn.Animations.animateOpacity(EnterColorBack, EnterColorBack.Opacity, 1, 0.25, IsAnimate: GetAnimateData()).Begin();
            zilongcn.Animations.animateOpacity(ContentText, ContentText.Opacity, 1, 0.3, IsAnimate: GetAnimateData()).Begin();
            zilongcn.Animations.animateOpacity(EnterContentText, EnterContentText.Opacity, 1, 0.3, IsAnimate: GetAnimateData()).Begin();
        }

        private void MainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            zilongcn.Animations.animateOpacity(EnterColorBack, EnterColorBack.Opacity, 0, 0.25, IsAnimate: GetAnimateData()).Begin();
            zilongcn.Animations.animateOpacity(ContentText, ContentText.Opacity, 1, 0.3, IsAnimate: GetAnimateData()).Begin();
            zilongcn.Animations.animateOpacity(EnterContentText, EnterContentText.Opacity, 0, 0.3, IsAnimate: GetAnimateData()).Begin();
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

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            parents = sender as Grid;
        }
    }
}
