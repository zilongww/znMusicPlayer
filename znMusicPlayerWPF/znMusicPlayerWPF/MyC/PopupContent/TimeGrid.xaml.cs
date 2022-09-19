using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// TimeGrid.xaml 的交互逻辑
    /// </summary>
    public partial class TimeGrid : Border
    {
        public TimeGrid()
        {
            InitializeComponent();
        }

        private void Time_HourMin_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            App.window.BlurThemeChangeEvent += (data) =>
            {
                Time_HourMin.Foreground = data.TextColor;
                Time_Sec.Foreground = data.TextColor;
                Time_YearMouth.Foreground = data.TextColor;
            };
        }
    }
}
