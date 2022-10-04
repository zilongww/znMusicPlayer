using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Helpers;

namespace znMusicPlayerWUI.DataEditor
{
    public class PlayingList
    {
        public delegate void PlayingListItemChangeDelegate(ObservableCollection<MusicData> nowPlayingList);
        public event PlayingListItemChangeDelegate PlayingListItemChange;

        public delegate void NowPlayingImageChangeDelegate(ImageSource imageSource);
        public event NowPlayingImageChangeDelegate NowPlayingImageLoading;
        public event NowPlayingImageChangeDelegate NowPlayingImageLoaded;

        public ObservableCollection<MusicData> NowPlayingList = new();
        public MusicData NowPlayingMusicData { get; set; }
        //public BitmapImage DefaultPlayingImage { get; set; } = new BitmapImage("")

        ImageSource _nowPlayingImage;
        public ImageSource NowPlayingImage
        {
            get => _nowPlayingImage;
            set
            {
                _nowPlayingImage = value;
            }
        }

        public PlayingList()
        {
            App.audioPlayer.SourceChanged += AudioPlayer_SourceChanged;
        }

        private async void AudioPlayer_SourceChanged(Media.AudioPlayer audioPlayer)
        {
            bool sameAlbum = true;

            if (audioPlayer.MusicData.AlbumID != NowPlayingMusicData?.AlbumID) sameAlbum = false;
            else if (audioPlayer.MusicData.InLocal != null)
            {
                if (audioPlayer.MusicData.Album != NowPlayingMusicData?.Album)
                {
                    sameAlbum = false;
                }
            }

            if (!sameAlbum)
            {
                NowPlayingImageLoading?.Invoke(null);
                ImageSource a = null;

                if (audioPlayer.MusicData.InLocal != null)
                {
                    System.Diagnostics.Debug.WriteLine(audioPlayer.MusicData.InLocal);
                    a = await CodeHelper.GetCover(audioPlayer.MusicData.InLocal);
                }
                else
                {
                    a = await FileHelper.GetImageSource(await Media.ImageManage.GetImageSource(audioPlayer.MusicData));
                }

                NowPlayingImage = a;
                NowPlayingImageLoaded?.Invoke(_nowPlayingImage);
            }
        }
    

        public void Add(MusicData musicData, bool invoke = true)
        {
            bool isFind = Find(musicData);
            if (!isFind)
                NowPlayingList.Add(musicData);

            if (invoke)
                PlayingListItemChange?.Invoke(NowPlayingList);
        }

        public async Task<bool> Play(MusicData musicData)
        {
            Add(musicData);
            var a = await App.audioPlayer.SetSource(musicData);
            App.audioPlayer.SetPlay();
            return a;
        }

        public async Task<bool> PlayNext()
        {
            if (NowPlayingList.Any())
            {
                var a = NowPlayingList.IndexOf(NowPlayingMusicData) + 1;
                if (a > NowPlayingList.Count - 1)
                {
                    a = 0;
                }

                return await Play(NowPlayingList[a]);
            }

            return true;
        }

        public async Task<bool> PlayPrevious()
        {
            if (NowPlayingList.Any())
            {
                var a = NowPlayingList.IndexOf(NowPlayingMusicData) - 1;
                if (a < 0)
                {
                    a = NowPlayingList.Count - 1;
                }

                return await Play(NowPlayingList[a]);
            }

            return true;
        }

        public void SetNextPlay(MusicData currentData, MusicData insertData)
        {
            if (!NowPlayingList.Any()) return;
            if (Find(insertData)) NowPlayingList.Remove(insertData);

            NowPlayingList.Insert(NowPlayingList.IndexOf(currentData) + 1, insertData);
        }

        public bool Find(MusicData musicData)
        {
            foreach (var item in NowPlayingList)
            {
                if (item == musicData)
                {
                    return true;
                }
            }

            return false;
        }

        public void ClearAll()
        {
            NowPlayingList.Clear();
        }
    }
}
