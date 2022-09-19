using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using zilongcn;
using znMusicPlayerWPF.Pages;

namespace znMusicPlayerWPF.MusicPlay
{
    class MusicUpdata
    {
        public static async void UpdataInterface(MainWindow TheParent, TheMusicDatas.MusicData NowMusicData)
        {
            TheParent.MusicBorder.BorderBrush = TheParent.GetCardColor(NowMusicData.From);

            if (Source.InternetConnect())
            {
                switch (NowMusicData.From)
                {
                    case TheMusicDatas.MusicFrom.kgMusic:
                        if (NowMusicData.PicUri == null)
                        {
                            try
                            {
                                Tuple<string, Newtonsoft.Json.Linq.JObject> Datas = await MusicAddressGet.kgMusicAddressGet(NowMusicData);

                                if (Datas.Item2["status"].ToString() == "1")
                                {
                                    NowMusicData.PicUri = Datas.Item2["data"]["img"].ToString();
                                    NowMusicData.ThekgMusicLrcs = Datas.Item2["data"]["lyrics"].ToString();
                                }
                            }
                            catch { }
                        }
                        break;

                    case TheMusicDatas.MusicFrom.neteaseMusic:
                        if (NowMusicData.PicUri == null)
                        {
                            try
                            {
                                NowMusicData.PicUri = await MusicAddressGet.GetNeteasePic(NowMusicData.SongRid);
                            }
                            catch { }
                        }
                        break;
                }
            }

            if (TheParent.NowPlaySong.MD5 == NowMusicData.MD5)
            {
                TheParent.MusicPages.Set(NowMusicData, TheParent.Animation);
            }
        }

        public static async Task<int> UpdataUserMusicList(MusicListPage PlayList)
        {
            App.window.IsLoadML = true;
            PlayList.NoMusicInListTb.Visibility = Visibility.Collapsed;
            PlayList.BarTop.Visibility = Visibility.Hidden;
            PlayList.TheList.Visibility = Visibility.Hidden;
            PlayList.LoadingTb.Visibility = Visibility.Visible;
            PlayList.LoadingEffect.Pause = false;

            bool IsSliver = false;

            PlayList.MusicListData.songs = null;
            for (int i = 0; i < PlayList.TheList.Children.Count; i++)
            {
                (PlayList.TheList.Children[i] as ItemBar).Delete();
                PlayList.TheList.Children.RemoveAt(i);
                i--;
                await Task.Delay(1);
            }

            int anl = 0;

            PlayList.LoadingTb.Visibility = Visibility.Collapsed;
            PlayList.LoadingEffect.Pause = true;
            PlayList.TheList.Visibility = Visibility.Visible;
            PlayList.BarTop.Visibility = Visibility.Visible;

            var a = await SongDataEdit.GetMusicListData(PlayList.MusicListData.listName);
            var b = a.songs;
            if (PlayList.MusicListData.listFrom == TheMusicDatas.MusicFrom.localMusic) b.Reverse();
            PlayList.MusicListData = a;
            foreach (TheMusicDatas.MusicData TheDatas in b)
            {
                anl++;
                ItemBar Bar = null;
                Bar = new ItemBar() { IsInMusicList = true };

                Bar.Set(anl.ToString(), TheDatas, TheDatas.PicUri, false);

                if (IsSliver) Bar.BackAnimateGrid.Fill = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
                //IsSliver = IsSliver == false;

                PlayList.TheList.Children.Add(Bar);

                Animations animations = new Animations(App.window.Animation);
                animations.animatePosition(Bar, new Thickness(40, 2, 0, 0), new Thickness(0, 2, 0, 0), 0.4, 1, 0);
                animations.animateOpacity(Bar, 0, 1, 0.4, 1, 0);
                animations.Begin();

                PlayList.MusicListPage_SizeChanged(null, null);
                await Task.Delay(1);
            }

            if (anl == 0)
            {
                PlayList.BarTop.Visibility = Visibility.Hidden;
                PlayList.TheList.Visibility = Visibility.Hidden;
                PlayList.NoMusicInListTb.Visibility = Visibility.Visible;
            }

            //PlayList.TitleTb.Text = PlayList.MusicListData.listName + $"（共 {anl} 首歌曲）";
            return anl;
            // TODO: 优化性能
            //TheParent.MusicPlayListContent.AlbumListRefresh();
        }
    }
}
