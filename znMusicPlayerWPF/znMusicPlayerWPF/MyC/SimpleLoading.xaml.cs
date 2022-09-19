using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace znMusicPlayerWPF.MyC
{
    /// <summary>
    /// SimpleLoading.xaml 的交互逻辑
    /// </summary>
    public partial class SimpleLoading : UserControl
    {
        private int _time = 135;
        public int time
        {
            get { return _time; }
            set { _time = value; }
        }

        public double StrokeThickness
        {
            get { return arc.StrokeThickness; }
            set { arc.StrokeThickness = value; }
        }

        private bool _Pause = false;
        public bool Pause
        {
            get { return _Pause; }
            set
            {
                _Pause = value;

                if (value)
                {
                    grid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grid.Visibility = Visibility.Visible;
                    //InitArcAngel();
                    Start();
                    AngelChange();
                }
            }
        }

        public SimpleLoading()
        {
            InitializeComponent();
            Pause = Pause;
        }

        bool startAnimateEnd = true;
        public void Start()
        {
            if (!startAnimateEnd) return;

            startAnimateEnd = false;
            var st = zilongcn.Animations.animateRenderTransformStatic(grid, 0, 360, 1, 0.5, 0.5, EasingMode.EaseInOut);
            st.Completed += St6_Completed;
            st.Begin();
        }

        private void St6_Completed(object sender, EventArgs e)
        {
            startAnimateEnd = true;
            if (Pause) return;
            Start();
        }

        private void InitArcAngel()
        {
            DoubleAnimation doubleAnimation1 = new DoubleAnimation(arc.StartAngle, 0, new TimeSpan(0));
            Storyboard.SetTarget(doubleAnimation1, arc);
            Storyboard.SetTargetProperty(arc, new PropertyPath("StartAngle"));
            arc.BeginAnimation(Microsoft.Expression.Shapes.Arc.StartAngleProperty, doubleAnimation1);

            DoubleAnimation doubleAnimation = new DoubleAnimation(18, new TimeSpan(0));
            Storyboard.SetTarget(doubleAnimation, arc);
            Storyboard.SetTargetProperty(arc, new PropertyPath("EndAngle"));
            arc.BeginAnimation(Microsoft.Expression.Shapes.Arc.EndAngleProperty, doubleAnimation);
        }

        bool angleAnimateEnd = true;
        private async void AngelChange()
        {
            if (!angleAnimateEnd) return;
            if (Pause) return;

            angleAnimateEnd = false;
            DoubleAnimation doubleAnimation = new DoubleAnimation(arc.EndAngle, arc.EndAngle + 230, new TimeSpan(0, 0, 0, 0, 940));
            Storyboard.SetTarget(doubleAnimation, arc);
            Storyboard.SetTargetProperty(arc, new PropertyPath("EndAngle"));
            arc.BeginAnimation(Microsoft.Expression.Shapes.Arc.EndAngleProperty, doubleAnimation);

            await Task.Delay(940);
            AngelChange_Step2();
        }

        private async void AngelChange_Step2()
        {
            if (!Pause)
            {
                DoubleAnimation doubleAnimation1 = new DoubleAnimation(arc.StartAngle, arc.EndAngle - 20, new TimeSpan(0, 0, 0, 0, 940));
                Storyboard.SetTarget(doubleAnimation1, arc);
                Storyboard.SetTargetProperty(arc, new PropertyPath("StartAngle"));
                arc.BeginAnimation(Microsoft.Expression.Shapes.Arc.StartAngleProperty, doubleAnimation1);

                await Task.Delay(940);
                angleAnimateEnd = true;
                AngelChange();
            }

            angleAnimateEnd = true;
        }
    }
}
