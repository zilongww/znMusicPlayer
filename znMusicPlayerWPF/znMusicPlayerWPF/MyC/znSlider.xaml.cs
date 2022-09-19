using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace znMusicPlayerWPF.MyC
{
    /// <summary>
    /// znSlider.xaml 的交互逻辑
    /// </summary>
    public partial class znSlider : Slider
    {

        public static readonly new DependencyProperty BorderBrushProperty = DependencyProperty.Register(
            "BorderBrush",
            typeof(Brush), typeof(znSlider),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Color.FromArgb(255, 255, 255, 255))
                )
            { BindsTwoWayByDefault = true }
            );

        public static readonly new DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground",
            typeof(Brush), typeof(znSlider),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Color.FromArgb(200, 0, 0, 0)),
                new PropertyChangedCallback((s, e) =>
                {
                    (s as znSlider).Foreground = (Brush)e.NewValue;
                }))
            { BindsTwoWayByDefault = true }
            );

        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set
            {
                SetValue(BorderBrushProperty, value);
                if (thumb != null) thumb.BorderBrush = value;
            }
        }

        public new Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set
            {
                SetValue(ForegroundProperty, value);
                if (thumb != null) thumb.Foreground = value;
                if (rectangle != null) rectangle.Fill = value;
            }
        }


        public znSlider()
        {
            InitializeComponent();
        }

        public znThumb thumb;
        private void Thumb_Loaded(object sender, RoutedEventArgs e)
        {
            thumb = sender as znThumb;
            thumb.BorderBrush = BorderBrush;
            thumb.Foreground = Foreground;
        }

        public Rectangle rectangle;
        private void rangel1_Loaded(object sender, RoutedEventArgs e)
        {
            rectangle = sender as Rectangle;
            rectangle.Fill = Foreground;
        }
    }
}
