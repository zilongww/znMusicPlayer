using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// AlbumButtonRightMenu.xaml 的交互逻辑
    /// </summary>
    public partial class AlbumButtonRightMenu : UserControl
    {
        Pages.AlbumButton AlbumButton = null;

        public AlbumButtonRightMenu(Pages.AlbumButton albumButton)
        {
            InitializeComponent();
            AlbumButton = albumButton;
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as TextBlock).Foreground = App.window.NowBlurThemeData.TextColor;
        }

        private void PackIcon_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as MaterialDesignThemes.Wpf.PackIcon).Foreground = App.window.NowBlurThemeData.InColorTextColor;
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
            AlbumButton.znButton_Click(sender, e);
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            AlbumButton.znButton_Click_2(sender, e);
        }

        private void znButton_Click_2(object sender, RoutedEventArgs e)
        {
            AlbumButton.znButton_Click_1(sender, e);
        }
    }
}
