using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// PlayModChoice.xaml 的交互逻辑
    /// </summary>
    public partial class PlayModChoice : Border, IznComboBox
    {
        public event ComboBoxResultDelegate result;

        public PlayModChoice()
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
                ListPlayModBtn.Foreground = data.TextColor;
                LoopPlayModBtn.Foreground = data.TextColor;
                RandomPlayModBtn.Foreground = data.TextColor;
                ListPlayModBtn.Background = data.ButtonBackColor;
                LoopPlayModBtn.Background = data.ButtonBackColor;
                RandomPlayModBtn.Background = data.ButtonBackColor;
                ListPlayModBtn.EnterColor = data.ButtonEnterColor;
                LoopPlayModBtn.EnterColor = data.ButtonEnterColor;
                RandomPlayModBtn.EnterColor = data.ButtonEnterColor;
            };
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            if (result != null) result((sender as znButton).Content);
        }
    }
}
