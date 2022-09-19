using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// MainPage.xaml 的交互逻辑
    /// </summary>
    public partial class MainPage : UserControl
    {
        private MainWindow TheParent = null;
        private Source TheSource = null;

        public MainPage(MainWindow Parent, Source Source1)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = Source1;
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            TheParent.SetPage("List");
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            TheParent.SetPage("Download");
        }

        private void Hyperlink_Click_2(object sender, RoutedEventArgs e)
        {
            TheParent.SetPage("Setting");
        }

        private void Hyperlink_Click_3(object sender, RoutedEventArgs e)
        {
            TheParent.SetPage("About");
        }

        private void Hyperlink_Click_4(object sender, RoutedEventArgs e)
        {
            TheParent.Button_Click(null, null);
        }

        private void Hyperlink_Click_5(object sender, RoutedEventArgs e)
        {
            TheParent.ImageButton_Click(null, null);
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            TheParent.SetPage("PlayList");
        }

        private void ListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            foreach (FrameworkElement i in listBox.Items)
            {
                try
                {
                    i.Width = listBox.ActualWidth - 30;
                }
                catch { }
            }
        }
    }
}
