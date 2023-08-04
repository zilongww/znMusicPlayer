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
using Melanchall.DryWetMidi.Core;
using TagLib.Ape;
using System.Xml.Schema;
using znMusicPlayerWUI.Controls;

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

        public delegate void PlayBehaviourDelegate(PlayBehaviour playBehaviour);
        public event PlayBehaviourDelegate PlayBehaviourChanged;

        public ObservableCollection<MusicData> NowPlayingList = new();
        public ObservableCollection<MusicData> RandomSavePlayingList = new();

        bool lastIsRandomPlay = false;
        private PlayBehaviour _playBehaviour = PlayBehaviour.顺序播放;
        public PlayBehaviour PlayBehaviour
        {
            get => _playBehaviour;
            set
            {
                _playBehaviour = value;
                SetRandomPlay(value);
                PlayBehaviourChanged?.Invoke(value);
            }
        }

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

        private async void SetRandomPlay(PlayBehaviour value)
        {
            if (value == PlayBehaviour.随机播放)
            {
                lastIsRandomPlay = true;
                RandomSavePlayingList.Clear();
                foreach (var item in NowPlayingList) RandomSavePlayingList.Add(item);
                var arr = NowPlayingList.ToList();
                for (int i = 0; i < NowPlayingList.Count; i++)
                {
                    int index = new Random().Next(i, NowPlayingList.Count);
                    var temp = arr[i];
                    var random = arr[index];
                    arr[i] = random;
                    arr[index] = temp;
                }
                NowPlayingList.Clear();
                foreach (var item in arr) NowPlayingList.Add(item);
                PlayingListItemChange?.Invoke(NowPlayingList);
            }
            else
            {
                if (lastIsRandomPlay)
                {
                    NowPlayingList.Clear();
                    foreach (var item in RandomSavePlayingList) NowPlayingList.Add(item);
                    RandomSavePlayingList.Clear();
                    PlayingListItemChange?.Invoke(NowPlayingList);
                }
            }
            await Play(NowPlayingList.First());
        }

        private async void AudioPlayer_PlayEnd(Media.AudioPlayer audioPlayer)
        {
            AddHistory(audioPlayer.MusicData);
            switch (PlayBehaviour)
            {
                case PlayBehaviour.顺序播放:
                case PlayBehaviour.随机播放:
                    await App.playingList.PlayNext(true, false);
                    break;/*
                case PlayBehaviour.随机播放:
                    await Play(NowPlayingList[new Random().Next(NowPlayingList.Count - 1)], true);
                    break;*/
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
            //System.Diagnostics.Debug.WriteLine(NowPlayingImageLoaded.GetInvocationList().Length);
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
                if (audioPlayer.MusicData.Album.ID == lastMusicData?.Album.ID) return;
            }
            lastMusicData = audioPlayer.MusicData;

            NowPlayingImageLoading?.Invoke(null, null);
            string path;
            ImageSource a = null;

            var _ = await ImageManage.GetImageSource(audioPlayer.MusicData);
            a = _.Item1;
            path = _.Item2;

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

        public async Task<bool> Play(MusicData musicData, bool isAutoPlay = false, bool freezeTime = true)
        {
            Add(musicData);

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

            var a = true;
            //System.Diagnostics.Debug.WriteLine(musicData.Title);
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
            catch (NotEnoughBytesException err)
            {
                LogHelper.WriteLog("PlayingList Play Midi Error", err.ToString(), false);
                await MainWindow.ShowDialog("播放Midi音频时出现错误", $"似乎不支持此Midi音频文件。\n错误信息：{err.Message}");
            }
            catch (Exception e)
            {
                LogHelper.WriteLog("PlayingList Play Error", e.ToString(), false);
                a = false;
#if DEBUG
                await MainWindow.ShowDialog("播放音频时出现错误", e.ToString());
#else
                await MainWindow.ShowDialog("播放音频时出现错误", e.Message);
#endif
            }

            return a;
        }

        private async void AddHistory(MusicData musicData)
        {
            await SongHistoryHelper.AddHistory(new() { MusicData = musicData, Time = DateTime.Now });
        }

        public async Task<bool> PlayNext(bool isAutoPlay = false, bool freezeTime = true)
        {
            if (NowPlayingList.Any())
            {
                var a = NowPlayingList.IndexOf(App.audioPlayer.MusicData) + 1;
                if (a > NowPlayingList.Count - 1)
                {
                    a = 0;
                }

                return await Play(NowPlayingList[a], isAutoPlay, freezeTime);
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
