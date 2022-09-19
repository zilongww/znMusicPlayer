using System.Collections.Generic;
using System.Linq;

namespace znMusicPlayerWPF.MusicPlay
{
    public class MusicPlayList
    {
        public List<TheMusicDatas.MusicData> musicDatasList = new List<TheMusicDatas.MusicData>();

        public TheMusicDatas.MusicData this[int Index]
        {
            get
            {
                return musicDatasList[Index];
            }
            set
            {
                musicDatasList[Index] = value;
            }
        }

        public MusicPlayList() { }

        public bool FindCard(TheMusicDatas.MusicData musicData)
        {
            foreach (Pages.ListLItem i in App.window.TheListLList.Items)
            {
                if (i.MusicData.MD5 == musicData.MD5)
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdataLList()
        {
            App.window.ListL_Title.Text = $"播放列表{(App.window.TheListLList.Items.Count == 0 ? "" : $"({App.window.TheListLList.Items.Count})")}";
            if (App.window.TheListLList.Items.Count == 0)
            {
                App.window.ListL_NoneIconText.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                App.window.ListL_NoneIconText.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        public void Add(TheMusicDatas.MusicData musicData)
        {
            if (FindCard(musicData)) return;

            musicDatasList.Add(musicData);

            Pages.ListLItem Bar = new Pages.ListLItem().Set(musicData);
            App.window.TheListLList.Items.Add(Bar);
            zilongcn.Animations.animateOpacity(Bar, 0, 1, 0.3, IsAnimate: App.window.Animation).Begin();
            UpdataLList();
        }

        public void Remove(TheMusicDatas.MusicData musicData)
        {
            musicDatasList.Remove(musicData);

            foreach (Pages.ListLItem i in App.window.TheListLList.Items)
            {
                if (i.MusicData.MD5 == musicData.MD5)
                {
                    i.Delete();
                    App.window.TheListLList.Items.Remove(i);
                    break;
                }
            }
            UpdataLList();
        }

        public void SetNextPlay(TheMusicDatas.MusicData musicData)
        {
            if (musicDatasList.Count == 0) return;

            if (FindCard(musicData))
            {
                Remove(musicData);
            }

            int index = 0;
            int cindex = 0;
            foreach (var i in musicDatasList)
            {
                if (i.MD5 == App.window.NowPlaySong.MD5)
                {
                    index = musicDatasList.IndexOf(i);
                    break;
                }
            }
            foreach (Pages.ListLItem i in App.window.TheListLList.Items)
            {
                if (i.MusicData.MD5 == App.window.NowPlaySong.MD5)
                {
                    cindex = App.window.TheListLList.Items.IndexOf(i);
                    break;
                }
            }

            Pages.ListLItem Bar = new Pages.ListLItem().Set(musicData);

            musicDatasList.Insert(index + 1, musicData);
            App.window.TheListLList.Items.Insert(index + 1, Bar);
        }

        public int Count()
        {
            return musicDatasList.Count;
        }

        public TheMusicDatas.MusicData Last()
        {
            return musicDatasList.Last();
        }

        public TheMusicDatas.MusicData First()
        {
            return musicDatasList.First();
        }

        public void CleanList()
        {
            MusicPlay.TheMusicDatas.MusicData[] musicDatas = new TheMusicDatas.MusicData[musicDatasList.Count];
            musicDatasList.CopyTo(musicDatas);
            foreach (var i in musicDatas)
            {
                Remove(i);
            }
            System.GC.Collect();
            musicDatasList = new List<TheMusicDatas.MusicData>();
        }
    }
}
