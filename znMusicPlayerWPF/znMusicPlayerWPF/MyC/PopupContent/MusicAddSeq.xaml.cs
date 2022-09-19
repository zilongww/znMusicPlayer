using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// MusicAddSeq.xaml 的交互逻辑
    /// </summary>
    public partial class MusicAddSeq : UserControl
    {
        public delegate void ResultDelegate(string result);
        public event ResultDelegate ResultEvent;

        public MusicAddSeq()
        {
            InitializeComponent();
            tba.Foreground = App.window.NowBlurThemeData.TextColor;
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            ResultEvent("name-1");
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            ResultEvent("name-2");
        }

        private void znButton_Click_2(object sender, RoutedEventArgs e)
        {
            ResultEvent("time-1");
        }

        private void znButton_Click_3(object sender, RoutedEventArgs e)
        {
            ResultEvent("time-2");
        }

        private void znButton_Loaded(object sender, RoutedEventArgs e)
        {
            znButton zb = sender as znButton;
            zb.Background = App.window.NowBlurThemeData.ButtonBackColor;
            zb.EnterColor = App.window.NowBlurThemeData.ButtonEnterColor;
            zb.Foreground = App.window.NowBlurThemeData.InColorTextColor;
        }
    }
}
