using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// PlayApiChoice.xaml 的交互逻辑
    /// </summary>
    public partial class PlayApiChoice : Border, MyC.IznComboBox
    {
        public event MyC.ComboBoxResultDelegate result;

        public PlayApiChoice()
        {
            InitializeComponent();
            UpdataThemeDelay();
        }

        public async void UpdataThemeDelay()
        {
            await Task.Delay(50);

            if (App.window == null) return;
            App.window.BlurThemeChangeEvent += (data) =>
            {
                WaveOutBtn.Foreground = data.TextColor;
                DirectSoundBtn.Foreground = data.TextColor;
                WasapiBtn.Foreground = data.TextColor;
                AsioBtn.Foreground = data.TextColor;
                WaveOutBtn.Background = data.ButtonBackColor;
                DirectSoundBtn.Background = data.ButtonBackColor;
                WasapiBtn.Background = data.ButtonBackColor;
                AsioBtn.Background = data.ButtonBackColor;
                WaveOutBtn.EnterColor = data.ButtonEnterColor;
                DirectSoundBtn.EnterColor = data.ButtonEnterColor;
                WasapiBtn.EnterColor = data.ButtonEnterColor;
                AsioBtn.EnterColor = data.ButtonEnterColor;
            };
        }

        private void WaveOutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (result != null) result((sender as znButton).Content);
        }
    }
}
