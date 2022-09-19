using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// AlbumListAddPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlbumListAddPage : UserControl
    {
        public MainWindow TheParent = null;

        private TheMusicDatas.MusicFrom _MusicFrom = TheMusicDatas.MusicFrom.kwMusic;
        public TheMusicDatas.MusicFrom NetWorkMusicFrom
        {
            get { return _MusicFrom; }
            set
            {
                _MusicFrom = value;
                NetWork_PlatfromTb.Text = value.ToString();
            }
        }

        public AlbumListAddPage(MainWindow mainWindow)
        {
            TheParent = mainWindow;
            InitializeComponent();
            AddLocalBtn_Click(null, null);
            NetWork_Search_ResultSp.Visibility = Visibility.Collapsed;
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.MusicPlayListContent.SetAlbumListPage();
        }

        private void AddLocalBtn_Click(object sender, RoutedEventArgs e)
        {
            Local.Visibility = Visibility.Visible;
            NetWork.Visibility = Visibility.Collapsed;

            LocalBtn.Background = FindResource("ButtonPAMM") as SolidColorBrush;
            NetworkBtn.Background = FindResource("ButtonPAMP") as SolidColorBrush;
        }

        private void ImportNetworkBtn_Click(object sender, RoutedEventArgs e)
        {
            Local.Visibility = Visibility.Collapsed;
            NetWork.Visibility = Visibility.Visible;

            LocalBtn.Background = FindResource("ButtonPAMP") as SolidColorBrush;
            NetworkBtn.Background = FindResource("ButtonPAMM") as SolidColorBrush;
        }

        private void Local_ChoiceImgBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = TheParent.TheSource.OpenWindowChoiceFlie("图片文件|*.jpg;*.png|所有文件|*.*");
            Local_ImgPathTb.Text = path ?? "不选择";
        }

        private async void Local_CompleteBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isReName = false;
            string listName = Local_ListNameTBox.Text;
            string lastName = "0";

            if (listName == "") listName = "default";

            foreach (TheMusicDatas.MusicListData listData in await SongDataEdit.GetAllMusicListData())
            {
                if (listData.listName.Contains(listName))
                {
                    lastName = listData.listName != listName ? listData.listName.Replace(listName, "") : "1";

                    isReName = true;
                }
            }

            try
            {
                if (isReName) listName += (int.Parse(lastName) + 1).ToString();
            }
            catch { }

            await SongDataEdit.AddNewList(
                listName,
                Local_ImgPathTb.Text == "不选择" ? null : Local_ImgPathTb.Text
                );

            TheParent.MusicPlayListContent.SetAlbumListPage();
            TheParent.MusicPlayListContent.AlbumListRefresh();
        }

        private void NetWork_PlatfromBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (NetWorkMusicFrom)
            {
                case TheMusicDatas.MusicFrom.kwMusic: NetWorkMusicFrom = TheMusicDatas.MusicFrom.kgMusic; break;
                case TheMusicDatas.MusicFrom.kgMusic: NetWorkMusicFrom = TheMusicDatas.MusicFrom.neteaseMusic; break;
                case TheMusicDatas.MusicFrom.neteaseMusic: NetWorkMusicFrom = TheMusicDatas.MusicFrom.kwMusic; break;
            }
        }

        TheMusicDatas.MusicListData MusicListData = new TheMusicDatas.MusicListData();
        private async void NetWork_ListSearchBtn_Click(object sender, RoutedEventArgs e)
        {
            NetWork_ListSearchBtn.IsEnabled = false;
            NetWork_CompleteBtn.IsEnabled = false;
            NetWork_Search_SearchStateTb.Text = "搜索中...";
            NetWork_Search_SearchStateTb.Visibility = Visibility.Visible;
            NetWork_Search_ResultSp.Visibility = Visibility.Collapsed;

            string listId = NetWork_ListSearchTBox.Text;
            var datas = await MusicUserListGet.UserListGet(NetWorkMusicFrom, listId);
            if (datas.Item1 != "200")
            {
                NetWork_Search_SearchStateTb.Text = datas.Item1;
                NetWork_Search_SearchStateTb.Visibility = Visibility.Visible;
                NetWork_Search_ResultSp.Visibility = Visibility.Collapsed;
                NetWork_CompleteBtn.IsEnabled = true;
                NetWork_ListSearchBtn.IsEnabled = true;
                return;
            }

            NetWork_Search_SearchStateTb.Visibility = Visibility.Collapsed;
            NetWork_Search_ResultSp.Visibility = Visibility.Visible;

            var musicListData = datas.Item2;

            NetWork_Search_TitleTb.Text = musicListData.listShowName;
            NetWork_Search_UserTb.Text = datas.Item3;
            NetWork_Search_ListIdTb.Text = listId;
            NetWork_Search_Image.Source = await Source.GetImage(musicListData.picPath);

            NetWork_CompleteBtn.IsEnabled = true;
            NetWork_ListSearchBtn.IsEnabled = true;
            MusicListData = musicListData;
        }

        private async void NetWork_CompleteBtn_Click(object sender, RoutedEventArgs e)
        {
            await SongDataEdit.AddNewList(MusicListData);
            TheParent.MusicPlayListContent.SetAlbumListPage();
            TheParent.MusicPlayListContent.AlbumListRefresh();
        }
    }
}
