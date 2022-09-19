using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// MusicFromSelect.xaml 的交互逻辑
    /// </summary>
    public partial class MusicFromSelect : UserControl, IznComboBox
    {
        public event ComboBoxResultDelegate result;

        public MusicFromSelect()
        {
            InitializeComponent();
        }

        public void UpdataThemeDelay()
        {
        }

        private void znButton_Loaded(object sender, RoutedEventArgs e)
        {
            znButton z = sender as znButton;
            z.BorderBrush = null;
            z.Background = App.window.NowBlurThemeData.ButtonBackColor;
            z.EnterColor = App.window.NowBlurThemeData.ButtonEnterColor;
            z.Foreground = App.window.NowBlurThemeData.TextColor;
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            if (result != null) result(znMusicPlayerWPF.MusicPlay.TheMusicDatas.MusicFromFromString((sender as znButton).Content as string));
        }
    }
}
