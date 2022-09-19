using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace znMusicPlayerWPF.MyC
{
    /// <summary>
    /// znThumb.xaml 的交互逻辑
    /// </summary>
    public partial class znThumb : System.Windows.Controls.Primitives.Thumb
    {
        public static readonly new DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            "BorderBrush",
            typeof(Brush), typeof(znThumb),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
                new PropertyChangedCallback((s, e) =>
                {
                    znThumb thumb = s as znThumb;
                    if (thumb.bigra != null) thumb.bigra.Fill = (Brush)e.NewValue;
                })
                )
            );

        public static readonly new DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground",
            typeof(Brush), typeof(znThumb),
            new PropertyMetadata(
                new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                new PropertyChangedCallback((s, e) =>
                {
                    znThumb thumb = s as znThumb;
                    if (thumb.inra != null) thumb.inra.Fill = (Brush)e.NewValue;
                })
                )
            );

        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set
            {
                SetValue(BorderBrushProperty, value);
                if (bigra != null) bigra.Fill = value;
            }
        }

        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set
            {
                SetValue(ForegroundProperty, value);
                if (inra != null) inra.Fill = value;
            }
        }

        public znThumb()
        {
            InitializeComponent();
        }

        public Rectangle bigra;
        public Rectangle inra;

        private void Rectangle_Loaded(object sender, RoutedEventArgs e)
        {
            bigra = sender as Rectangle;
            bigra.Fill = BorderBrush;
        }

        private void ar_Loaded(object sender, RoutedEventArgs e)
        {
            inra = sender as Rectangle;
            inra.Fill = Foreground;
        }

        double times = 0.28;
        private void Thumb_MouseEnter(object sender, MouseEventArgs e)
        {
            zilongcn.Animations.animatePosition(inra, inra.Margin, new Thickness(32), 0.28, 1, 0).Begin();
        }

        private void Thumb_MouseLeave(object sender, MouseEventArgs e)
        {
            zilongcn.Animations.animatePosition(inra, inra.Margin, new Thickness(48), 0.28, 1, 0).Begin();
        }

        bool isMouseDown = false;
        private void Thumb_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            zilongcn.Animations.animatePosition(inra, inra.Margin, new Thickness(58), 0.28, 1, 0).Begin();
        }
        private void Thumb_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            zilongcn.Animations.animatePosition(inra, inra.Margin, new Thickness(28), 0.28, 1, 0).Begin();
        }
    }
}
