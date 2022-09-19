using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// SelectSoundOutContent.xaml 的交互逻辑
    /// </summary>
    public partial class SelectSoundOutContent : UserControl, IznComboBox
    {
        public event ComboBoxResultDelegate result;

        public void UpdataThemeDelay()
        {

        }

        public SelectSoundOutContent()
        {
            InitializeComponent();
        }

        public void UpdataList()
        {
            listBox.Items.Clear();
            foreach (var d in MusicPlay.AudioPlayer.GetOutDevices())
            {
                znButton znButton = new znButton()
                {
                    Height = 34,
                    Width = 350,
                    FontSize = 14,
                    Background = App.window.NowBlurThemeData.ButtonBackColor,
                    BorderBrush = null,
                    Foreground = App.window.NowBlurThemeData.InColorTextColor,
                    EnterColor = App.window.NowBlurThemeData.ButtonEnterColor,
                    Content = $" {d.DeviceType}: {d.DeviceName}",
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Tag = d
                };
                znButton.Click += ZnButton_Click;

                listBox.Items.Add(znButton);
            }
        }

        private void ZnButton_Click(object sender, RoutedEventArgs e)
        {
            if (result != null) result((MusicPlay.AudioPlayer.OutDevice)(sender as znButton).Tag);
        }
    }
}
