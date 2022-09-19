using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// SearchPage.xaml 的交互逻辑
    /// </summary>
    public partial class SearchPage : UserControl
    {
        private TheMusicDatas.MusicFrom _MusicFrom = TheMusicDatas.MusicFrom.kwMusic;
        private int _PageCount = 1;

        public int PageCount
        {
            get { return _PageCount; }
            set
            {
                if (value <= 0)
                {
                    value = 1;
                }
                else
                {
                    TheParent.StartSearch(SearchTextBox.Text, value.ToString(), PageSize.ToString(), MusicFrom);
                }

                _PageCount = value;
                PageCountText.Text = $"第{value}页";
            }
        }

        public int PageSize { get; set; } = 30;

        public TheMusicDatas.MusicFrom MusicFrom
        {
            get { return _MusicFrom; }
            set
            {
                _MusicFrom = value;
                SoftButton.Text = " 搜索平台：" + value.ToString();
            }
        }

        private MainWindow TheParent = null;
        private Source TheSource = null;

        public SearchPage(MainWindow Parent)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = Parent.TheSource;
            SearchPageChangerGrid.Visibility = Visibility.Collapsed;
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Button_Click(null, null);
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            PageCount = 1;
        }

        private void znButton_Click_3(object sender, RoutedEventArgs e)
        {
            PageCount--;
        }

        private void znButton_Click_4(object sender, RoutedEventArgs e)
        {
            PageCount++;
        }

        private void znButton_Click_5(object sender, RoutedEventArgs e)
        {
            PageCount = 1;
        }

        private void SoftButton_ContentClick(MyC.znComboBox znComboBox, object data)
        {
            MusicFrom = (TheMusicDatas.MusicFrom)data;
        }
    }
}
