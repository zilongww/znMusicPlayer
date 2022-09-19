using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// DownloadLineContent.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadLineContent : UserControl, IznComboBox
    {
        public event ComboBoxResultDelegate result;

        public DownloadLineContent()
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
            if (result != null) result((sender as znButton).Content);
        }
    }
}
