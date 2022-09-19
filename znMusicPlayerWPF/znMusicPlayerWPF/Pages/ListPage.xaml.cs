using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// ListPage.xaml 的交互逻辑
    /// </summary>
    public partial class ListPage : UserControl
    {
        private MainWindow TheParent = null;
        private Source TheSource = null;

        public ListPage(MainWindow Parent, Source Source1)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = Source1;
            SizeChanged += ListPage_SizeChanged;
        }

        public void ListPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                foreach (FrameworkElement items in Lists.Children)
                {
                    items.Width = Listb.ActualWidth - 33;
                }
            }
            catch { }
        }

        public void CleanCard()
        {
            for (int i = 0; i < Lists.Children.Count; i++)
            {
                (Lists.Children[i] as ItemBar).Delete();
                Lists.Children.RemoveAt(i);
                i--;
            }
        }

    }
}
