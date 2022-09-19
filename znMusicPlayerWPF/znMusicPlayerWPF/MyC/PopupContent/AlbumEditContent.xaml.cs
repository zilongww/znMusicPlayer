using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// AlbumEditContent.xaml 的交互逻辑
    /// </summary>
    public partial class AlbumEditContent : UserControl
    {
        public AlbumEditContent()
        {
            InitializeComponent();
            ListNameTbox.Foreground = App.window.NowBlurThemeData.InColorTextColor;
        }

        private void TextBlock_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            (sender as TextBlock).Foreground = App.window.NowBlurThemeData.TextColor;
        }
    }
}
