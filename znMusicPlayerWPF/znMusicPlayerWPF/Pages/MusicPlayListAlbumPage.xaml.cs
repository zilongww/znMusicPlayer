using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// MusicPlayListAlbumPage.xaml 的交互逻辑
    /// </summary>
    public partial class MusicPlayListAlbumPage : UserControl
    {
        public MainWindow TheParent = null;

        public MusicPlayListAlbumPage(MainWindow mainWindow)
        {
            InitializeComponent();
            TheParent = mainWindow;
        }

        bool isrefreshcomplete = true;
        public async void Refresh()
        {
            if (!isrefreshcomplete) return;

            isrefreshcomplete = false;
            foreach (AlbumButton i in AlbumList.Items)
            {
                i.Delete();
            }
            AlbumList.Items.Clear();

            foreach (var listData in await SongDataEdit.GetAllMusicListData())
            {
                AlbumButton albumButton = new AlbumButton();
                albumButton.Margin = new Thickness(0, 8, 2, 0);
                albumButton.Set(listData);
                AlbumList.Items.Add(albumButton);

                await Task.Delay(1);
            }
            isrefreshcomplete = true;
        }

        private void ReCallButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.MusicPlayListContent.SetAddListPage();
        }
    }
}
