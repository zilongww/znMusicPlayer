using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// InteractContent.xaml 的交互逻辑
    /// </summary>
    public partial class InteractContent : UserControl
    {
        public delegate void ResultDelegate(bool e);
        public event ResultDelegate ResultEvent;

        public InteractContent(string Title = "确定？")
        {
            InitializeComponent();
            TitleTb.Text = Title;
            TitleTb.Foreground = App.window.NowBlurThemeData.TextColor;
        }

        private void YesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ResultEvent != null) ResultEvent(true);
        }

        private void NoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ResultEvent != null) ResultEvent(false);
        }
    }
}
