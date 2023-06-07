using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Helpers;
using WinRT;
using znMusicPlayerWUI.Media;

namespace znMusicPlayerWUI.Background
{
    public enum PlayBehaviour { 顺序播放, 随机播放, 单曲循环, 单曲播放完成后停止 }
    public class PlayingList
    {
        public delegate void PlayingListItemChangeDelegate(ObservableCollection<MusicData> nowPlayingList);
        public event PlayingListItemChangeDelegate PlayingListItemChange;

        public delegate void NowPlayingImageChangeDelegate(ImageSource imageSource, string path);
        public event NowPlayingImageChangeDelegate NowPlayingImageLoading;
        public event NowPlayingImageChangeDelegate NowPlayingImageLoaded;

        public ObservableCollection<MusicData> NowPlayingList = new();
        public PlayBehaviour PlayBehaviour { get; set; } = PlayBehaviour.顺序播放;
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
            App.audioPlayer.PlayEnd += AudioPlayer_PlayEnd;
        }

        private async void AudioPlayer_PlayEnd(Media.AudioPlayer audioPlayer)
        {
            //System.Diagnostics.Debug.WriteLine(App.playingList.PlayBehaviour);
            switch (PlayBehaviour)
            {
                case PlayBehaviour.顺序播放:
                    await App.playingList.PlayNext(true);
                    break;
                case PlayBehaviour.随机播放:
                    await Play(NowPlayingList[new Random().Next(NowPlayingList.Count - 1)], true);
                    break;
                case PlayBehaviour.单曲循环:
                    await Play(App.audioPlayer.MusicData, true);
                    break;
                case PlayBehaviour.单曲播放完成后停止:
                    App.audioPlayer.CurrentTime = TimeSpan.Zero;
                    App.audioPlayer.SetStop();
                    break;
            }
        }

        MusicData lastMusicData = null;
        private async void AudioPlayer_SourceChanged(AudioPlayer audioPlayer)
        {
            if (audioPlayer.MusicData == null) return;
            if (audioPlayer.MusicData.InLocal != null)
            {
                if (audioPlayer.MusicData.Album == lastMusicData?.Album)
                {
                    return;
                }
            }
            else
            {
                if (audioPlayer.MusicData.AlbumID == lastMusicData?.AlbumID) return;
            }
            lastMusicData = audioPlayer.MusicData;

            NowPlayingImageLoading?.Invoke(null, null);
            string path;
            ImageSource a = null;

            if (audioPlayer.MusicData?.InLocal != null)
            {
                try
                {
                    a = await CodeHelper.GetCover(audioPlayer.MusicData.InLocal);
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine("2------" + err.Message);
                }
                path = audioPlayer.MusicData.InLocal;
            }
            else
            {
                var _ = await Media.ImageManage.GetImageSource(audioPlayer.MusicData);
                a = await FileHelper.GetImageSource(_);
                path = _;
            }

            NowPlayingImage = a;
            NowPlayingImageLoaded?.Invoke(NowPlayingImage, path);
        }


        public void Add(MusicData musicData, bool invoke = true)
        {
            bool isFind = Find(musicData);
            if (!isFind)
                NowPlayingList.Add(musicData);

            if (invoke)
                PlayingListItemChange?.Invoke(NowPlayingList);
        }

        bool isLoadingPlay = false;
        public async Task<bool> Play(MusicData musicData, bool isAutoPlay = false)
        {
            if (isLoadingPlay) return false;
            isLoadingPlay = true;
            FreezePlayTime();
            AddHistory(musicData);

            Add(musicData);
            bool a = true;

            NAudio.Wave.PlaybackState playState;
            if (App.audioPlayer.NowOutObj != null)
            {
                playState = App.audioPlayer.NowOutObj.PlaybackState;
            }
            else
            {
                playState = NAudio.Wave.PlaybackState.Playing;
            }

            if (isAutoPlay)
            {
                playState = NAudio.Wave.PlaybackState.Playing;
            }

            try
            {
                await App.audioPlayer.SetSource(musicData);
                if (playState == NAudio.Wave.PlaybackState.Playing)
                    App.audioPlayer.SetPlay();
            }
            catch (DivideByZeroException)
            {
                a = false;
                var data = DataFolderBase.JSettingData;
                data[DataFolderBase.SettingParams.AudioLatency.ToString()] = 
                    DataFolderBase.SettingDefault[DataFolderBase.SettingParams.AudioLatency.ToString()];
                App.audioPlayer.Latency = (int)data[DataFolderBase.SettingParams.AudioLatency.ToString()];
                DataFolderBase.JSettingData = data;

                var retryPlay = await MainWindow.ShowDialog("播放失败",
                    $"播放音频时出现错误，可能是播放延迟设置不正确导致的。\n" +
                    $"已将播放延迟设置到默认值，请尝试重新播放.");
                if (retryPlay == Microsoft.UI.Xaml.Controls.ContentDialogResult.Secondary)
                {
                    await Play(musicData, isAutoPlay);
                }
            }
            catch (Exception e)
            {
                a = false;
#if DEBUG
                await MainWindow.ShowDialog("播放音频时出现错误", e.ToString());
#else
                await MainWindow.ShowDialog("播放音频时出现错误", e.Message);
#endif
            }

            return a;
        }

        private async void FreezePlayTime()
        {
            await Task.Delay(200);
            isLoadingPlay = false;
        }

        private async void AddHistory(MusicData musicData)
        {
            await SongHistoryHelper.AddHistory(new() { MusicData = musicData, Time = DateTime.Now });
        }

        public async Task<bool> PlayNext(bool isAutoPlay = false)
        {
            if (NowPlayingList.Any())
            {
                var a = NowPlayingList.IndexOf(App.audioPlayer.MusicData) + 1;
                if (a > NowPlayingList.Count - 1)
                {
                    a = 0;
                }

                return await Play(NowPlayingList[a], isAutoPlay);
            }

            return true;
        }

        public async Task<bool> PlayPrevious()
        {
            if (NowPlayingList.Any())
            {
                var a = NowPlayingList.IndexOf(App.audioPlayer.MusicData) - 1;
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
