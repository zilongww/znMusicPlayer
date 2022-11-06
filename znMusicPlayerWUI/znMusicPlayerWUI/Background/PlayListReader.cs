using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace znMusicPlayerWUI.Background
{
    public sealed class PlayListReader
    {
        public delegate void PlayListChanged();
        public event PlayListChanged Updataed;

        DataEditor.MusicListData[] nowMusicListDatas;
        public DataEditor.MusicListData[] NowMusicListDatas
        {
            get => nowMusicListDatas;
            set
            {
                nowMusicListDatas = value;
                Updataed?.Invoke();
            }
        }

        public PlayListReader() { }

        bool inRefresh = false;
        public async Task Refresh()
        {
            if (inRefresh) return;
            inRefresh = true;
            NowMusicListDatas = await DataEditor.PlayListHelper.ReadAllPlayList();
            inRefresh = false;
        }
    }
}
