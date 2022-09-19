using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace znMusicPlayerWPF.MyC
{
    class znScrollViewer : ScrollViewer
    {
        #region 判断是否滑到底部
        public bool IsVerticalScrollBarAtButtom
        {
            get
            {
                bool isAtButtom = false;

                // get the vertical scroll position
                double dVer = this.VerticalOffset;

                //get the vertical size of the scrollable content area
                double dViewport = this.ViewportHeight;

                //get the vertical size of the visible content area
                double dExtent = this.ExtentHeight;

                if (dVer != 0)
                {
                    if (dVer + dViewport == dExtent)
                    {
                        isAtButtom = true;
                    }
                    else
                    {
                        isAtButtom = false;
                    }
                }
                else
                {
                    isAtButtom = false;
                }

                if (this.VerticalScrollBarVisibility == ScrollBarVisibility.Disabled
                    || this.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden)
                {
                    isAtButtom = true;
                }

                return isAtButtom;
            }
        }
        #endregion

        #region 垂直
        /// <summary>
        /// 垂直偏移值
        /// </summary>
        public double VerticalScrollRatio
        {
            get { return (double)GetValue(VerticalScrollRatioProperty); }
            set { SetValue(VerticalScrollRatioProperty, value); }
        }

        /// <summary>
        /// 垂直偏移值DP
        /// </summary>
        public static readonly DependencyProperty VerticalScrollRatioProperty =
            DependencyProperty.Register("VerticalScrollRatio", typeof(double), typeof(znScrollViewer), new PropertyMetadata(0.0, new PropertyChangedCallback(V_ScrollRatioChangedCallBack)));

        private static void V_ScrollRatioChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)(d);
            scrollViewer.ScrollToVerticalOffset((double)(e.NewValue));// * scrollViewer.ScrollableHeight);
        }

        /// <summary>
        /// 垂直移动量
        /// </summary>
        public double VerticalScrollValue
        {
            get { return (double)GetValue(VerticalScrollValueProperty); }
            set { SetValue(VerticalScrollValueProperty, value); }
        }

        /// <summary>
        /// 垂直移动量DP
        /// </summary>
        public static readonly DependencyProperty VerticalScrollValueProperty =
            DependencyProperty.Register("VerticalScrollValue", typeof(double), typeof(znScrollViewer), new PropertyMetadata(0.0, new PropertyChangedCallback(V_ScrollValueChangedCallBack)));

        private static void V_ScrollValueChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = (znScrollViewer)(d);
            ScrollViewerScroll(scrollViewer, e);
        }
        #endregion

        #region 水平
        /// <summary>
        /// 水平偏移值
        /// </summary>
        public double HorizontalScrollRatio
        {
            get { return (double)GetValue(HorizontalScrollRatioProperty); }
            set { SetValue(HorizontalScrollRatioProperty, value); }
        }

        /// <summary>
        /// 水平偏移值DP
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollRatioProperty =
            DependencyProperty.Register("HorizontalScrollRatio", typeof(double), typeof(znScrollViewer), new PropertyMetadata(0.0, new PropertyChangedCallback(H_ScrollRatioChangedCallBack)));

        private static void H_ScrollRatioChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)(d);
            scrollViewer.ScrollToHorizontalOffset((double)(e.NewValue));// * scrollViewer.ScrollableWidth);
        }
        #endregion

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            //MessageBox.Show(PanningDeceleration.ToString());
            double tovalue = VerticalOffset + -((MainWindow.Animate ? e.Delta * 1 : e.Delta * 0.4) * PanningRatio * (ViewportHeight <= 130 ? ViewportHeight * 0.007 : 1));
            if (tovalue + ViewportHeight >= ExtentHeight)
            {
                tovalue = ExtentHeight - ViewportHeight;
            }
            else if (tovalue <= 0)
            {
                tovalue = 0;
            }
            VerticalScrollValue = tovalue;
        }

        public double AnimationTime { get; set; } = 0.4;
        public double AnimationDecelerationRatio { get; set; } = 1;
        public double AnimationAccelerationRatio { get; set; } = 0;

        //public static bool isScroll = false;
        public static int scrollCount = 0;
        /// <summary>
        /// 滚动动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void ScrollViewerScroll(znScrollViewer sender, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Media.Animation.DoubleAnimation doubleAnimation = new System.Windows.Media.Animation.DoubleAnimation();
            doubleAnimation.From = (double)sender.VerticalOffset;//(double)e.OldValue - (double)e.NewValue > 0 ? sender.VerticalOffset - 3 : sender.VerticalOffset + 3;
            doubleAnimation.To = (double)e.NewValue;
            doubleAnimation.EasingFunction = new System.Windows.Media.Animation.SineEase() { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut };
            doubleAnimation.Duration = MainWindow.Animate ? new Duration(TimeSpan.FromSeconds(sender.AnimationTime)) : new Duration(new TimeSpan(0));
            doubleAnimation.DecelerationRatio = sender.AnimationDecelerationRatio;
            doubleAnimation.AccelerationRatio = sender.AnimationAccelerationRatio;
            sender.BeginAnimation(MyC.znScrollViewer.VerticalScrollRatioProperty, doubleAnimation);

            // 此流畅动画依靠bug运行
            App.window.wftb.Text = $"{sender.VerticalScrollValue}";

            //PanningRatioChangerAsync(sender);
        }

        public static async void PanningRatioChangerAsync(znScrollViewer sender)
        {
            scrollCount += 1;

            double bPanningRatio = sender.PanningRatio;

            sender.PanningRatio *= 0.0002 * sender.ExtentHeight + 1.8;

            await Task.Delay(450);
            sender.PanningRatio = bPanningRatio;
        }
    }
}
