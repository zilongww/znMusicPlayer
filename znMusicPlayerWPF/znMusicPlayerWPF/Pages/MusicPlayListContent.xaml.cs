using System.Windows.Controls;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// MusicPlayListContent.xaml 的交互逻辑
    /// </summary>
    public partial class MusicPlayListContent : UserControl
    {
        MainWindow TheParent = null;
        public MusicPlayListAlbumPage MusicPlayListAlbumPage = null;
        public MusicListPage MusicListPage = null;
        public AlbumListAddPage AlbumListAddPage = null;

        public MusicPlayListContent(MainWindow mainWindow)
        {
            InitializeComponent();
            TheParent = mainWindow;
            MusicPlayListAlbumPage = new MusicPlayListAlbumPage(mainWindow);
            AlbumListAddPage = new AlbumListAddPage(mainWindow);
            InPage.Content = MusicPlayListAlbumPage;

            AlbumListRefresh();

            //MusicListPage = new MusicListPage(TheParent);
            //MusicListPage.Set(SongDataEdit.GetMusicListData());
        }

        public void AlbumListRefresh()
        {
            MusicPlayListAlbumPage.Refresh();
        }

        public void SetAlbumListPage()
        {
            TheParent.SetPage("PlayList");
            InPage.Content = MusicPlayListAlbumPage;
        }

        public void SetAddListPage()
        {
            TheParent.SetPage("PlayList");
            InPage.Content = AlbumListAddPage;
        }

        public void SetMusicListPage(TheMusicDatas.MusicListData musicListData)
        {
            TheParent.SetPage("PlayList");

            if (MusicListPage != null)
            {
                if (MusicListPage.MusicListData.MD5 == musicListData.MD5)
                {
                    InPage.Content = MusicListPage;
                    return;
                }

                foreach (var i in MusicListPage.TheList.Children)
                {
                    (i as ItemBar).Dispose();
                }
                MusicListPage.TheList.Children.Clear();
                MusicListPage.TheList = null;
                MusicListPage = null;
            }

            MusicListPage = new MusicListPage(TheParent);
            MusicListPage.Set(musicListData);
            InPage.Content = MusicListPage;
        }
    }
}
