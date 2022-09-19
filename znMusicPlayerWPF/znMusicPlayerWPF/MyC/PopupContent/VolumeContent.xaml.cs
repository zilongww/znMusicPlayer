using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// VolumeContent.xaml 的交互逻辑
    /// </summary>
    public partial class VolumeContent : UserControl
    {
        private MainWindow TheParent = null;
        public float lastVolume = 0.5f;

        public VolumeContent(MainWindow mainWindow)
        {
            InitializeComponent();
            TheParent = mainWindow;
            TheParent.BlurThemeChangeEvent += (data) =>
            {
                zb.Background = data.ButtonBackColor;
                zb.EnterColor = data.ButtonEnterColor;
                zb.Foreground = data.TextColor;
                texts.Foreground = data.TextColor;
                icons.Foreground = data.TextColor;
            };
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            if (TheParent.NowVolume > 0)
            {
                lastVolume = TheParent.NowVolume;
                TheParent.NowVolume = 0;
            }
            else
            {
                TheParent.NowVolume = lastVolume;
            }
        }

        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TheParent.VolumeSp_MouseWheel(sender, e);
        }
    }
}
