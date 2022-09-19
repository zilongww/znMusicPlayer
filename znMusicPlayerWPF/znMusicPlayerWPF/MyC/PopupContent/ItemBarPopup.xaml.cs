using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// ItemBarPopup.xaml 的交互逻辑
    /// </summary>
    public partial class ItemBarPopup : Border
    {
        public MusicPlay.TheMusicDatas.MusicData MusicData = new MusicPlay.TheMusicDatas.MusicData();

        public ItemBarPopup(MusicPlay.TheMusicDatas.MusicData musicData)
        {
            InitializeComponent();
            MusicData = musicData;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            App.window.DownloadMusic(MusicData);
            App.window.Activate();
        }

        private async void AddListButton_Click(object sender, RoutedEventArgs e)
        {
            if (MusicData.From == MusicPlay.TheMusicDatas.MusicFrom.neteaseMusic)
            {
                if (MusicData.AlbumID != null)
                {
                    try
                    {
                        MusicData.PicUri = await MusicPlay.MusicAddressGet.GetNeteasePic(MusicData.SongRid);
                    }
                    catch { }
                }
            }
            if (MusicData.From == MusicPlay.TheMusicDatas.MusicFrom.kgMusic)
            {
                await Task.Run(() =>
                {
                    string GetMusicUrl = $"https://www.kugou.com/yy/index.php?r=play/getdata&hash={MusicData.SongRid}&album_id={MusicData.AlbumID}";
                    System.Net.WebClient webClient = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 };
                    webClient.Headers.Add("Cookie", "kg_mid=c1470a3e02dbaa42f2d0f9ee79320cf0; kg_dfid=4WPBwo1Rm8lo3XLx8X0i4I0b; kg_dfid_collect=d41d8cd98f00b204e9800998ecf8427e; ACK_SERVER_10015={\"list\":[[\"bjlogin - user.kugou.com\"]]}; KuGooRandom=66501625663676046; Hm_lvt_aedee6983d4cfc62f509129360d6bb3d=1624546354,1625661111,1625665264; Hm_lpvt_aedee6983d4cfc62f509129360d6bb3d=1625665312");

                    Newtonsoft.Json.Linq.JObject jdata = Newtonsoft.Json.Linq.JObject.Parse(webClient.DownloadString(GetMusicUrl));

                    MusicData.PicUri = jdata["data"]["img"].ToString();
                });
            }

            MyC.PopupContent.AddAlbumSongContent addAlbumSongContent = new MyC.PopupContent.AddAlbumSongContent();
            MyC.PopupWindow popupWindow = new MyC.PopupWindow()
            {
                Content = addAlbumSongContent,
                ForceAcrylicBlur = true,
                IsDeActivityClose = true,
                IsShowActivated = true,
                isWindowSmallRound = false
            };

            popupWindow.UserShow();

            addAlbumSongContent.ResultEvent += async (s) =>
            {
                await SongDataEdit.AddUserSong(MusicData, s);
            };

            App.window.IsLoadML = false;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            App.window.ShowBox(MusicData);
            App.window.Activate();
        }

        private void networkbtn_Click(object sender, RoutedEventArgs e)
        {
            if (MusicData.From == MusicPlay.TheMusicDatas.MusicFrom.kwMusic) App.window.TheSource.OpenWeb("http://www.kuwo.cn/play_detail/" + MusicData.SongRid);
            else if (MusicData.From == MusicPlay.TheMusicDatas.MusicFrom.neteaseMusic) App.window.TheSource.OpenWeb("https://music.163.com/#/song?id=" + MusicData.SongRid);
            else if (MusicData.From == MusicPlay.TheMusicDatas.MusicFrom.kgMusic) App.window.TheSource.OpenWeb($"https://www.kugou.com/song/#hash={MusicData.SongRid}&album_id={MusicData.AlbumID}");
            else App.window.ShowBox("错误", "此文件无打开网址。");
            App.window.Activate();
        }

        private async void deletebtn_Click(object sender, RoutedEventArgs e)
        {
            if (!App.window.InUserList)
            {
                App.window.NowPlayList.Remove(MusicData);
                App.window.ListPages.Lists.Children.Remove(this);
            }
            else
            {
                try
                {
                    await SongDataEdit.RemoveUserSong(MusicData, App.window.MusicPlayListContent.MusicListPage.MusicListData);
                }
                catch { }

                await MusicPlay.MusicUpdata.UpdataUserMusicList(App.window.MusicPlayListContent.MusicListPage);
            }
            App.window.Activate();
        }

        private void nextplaybtn_Click(object sender, RoutedEventArgs e)
        {
            App.window.NowPlayList.SetNextPlay(MusicData);
            App.window.Activate();
        }

        private void Path_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as System.Windows.Shapes.Path).Stroke = App.window.NowBlurThemeData.InColorTextColor;
        }

        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as System.Windows.Controls.TextBlock).Foreground = App.window.NowBlurThemeData.TextColor;
        }

        private void znButton_Loaded(object sender, RoutedEventArgs e)
        {
            znButton z = sender as znButton;
            z.BorderBrush = null;
            z.Background = App.window.NowBlurThemeData.ButtonBackColor;
            z.EnterColor = App.window.NowBlurThemeData.ButtonEnterColor;
            z.Foreground = App.window.NowBlurThemeData.TextColor;
        }

        private void PackIcon_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as MaterialDesignThemes.Wpf.PackIcon).Foreground = App.window.NowBlurThemeData.InColorTextColor;
        }
    }
}
