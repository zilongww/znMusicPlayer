using Meting4Net.Core.Models.Tencent;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Helpers;

namespace znMusicPlayerWUI.Background
{
    public class LyricManager
    {
        public delegate void PlayingLyricDelegate(ObservableCollection<LyricData> nowPlayingLyrics);
        public delegate void PlayingLyricData(LyricData nowLyricsData);
        public event PlayingLyricDelegate PlayingLyricSourceChange;
        public event PlayingLyricData PlayingLyricSelectedChange;
        MusicData MusicData;
        DispatcherTimer timer;

        public ObservableCollection<LyricData> NowPlayingLyrics = new();
        private LyricData _nowLyricsData = null;
        public LyricData NowLyricsData
        {
            get => _nowLyricsData;
            set
            {
                //if (_nowLyricsData == null || value == null) return;
                if (value == null)
                {
                    _nowLyricsData = value;
                    PlayingLyricSelectedChange?.Invoke(value);
                }
                else if (_nowLyricsData != value)
                {
                    _nowLyricsData = value;
                    PlayingLyricSelectedChange?.Invoke(value);
                }
            }
        }

        public LyricManager()
        {
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(5) };
            timer.Tick += (_, __) => ReCallUpdata();

            //MainWindow.WindowViewStateChanged += MainWindow_WindowViewStateChanged;
            App.audioPlayer.SourceChanged += AudioPlayer_SourceChanged;
            App.audioPlayer.PlayStateChanged += AudioPlayer_PlayStateChanged;
        }

        private void MainWindow_WindowViewStateChanged(bool isView)
        {
            if (isView)
            {
                timer.Start();
            }
            else
            {
                timer.Stop();
            }
        }

        private void AudioPlayer_PlayStateChanged(Media.AudioPlayer audioPlayer)
        {
            if (App.audioPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                timer.Start();
            }
        }

        public async Task InitLyricList(MusicData musicData)
        {
            string cachePath = await FileHelper.GetLyricCache(musicData);
            string resultPath = null;

            if (cachePath != null)
            {
                resultPath = cachePath;
            }
            else
            {
                Tuple<string, string> lyricTuple;
                if (musicData.From == MusicFrom.neteaseMusic)
                {
                    lyricTuple = await WebHelper.GetLyricStringAsync(musicData);
                }
                else
                {
                    lyricTuple = null;
                }

                if (lyricTuple == null)
                {
                    resultPath = null;
                }
                else
                {
                    string path = $@"{DataFolderBase.LyricCacheFolder}\{musicData.From}{musicData.ID}";
                    await Task.Run(() =>
                    {
                        if (!System.IO.File.Exists(path))
                        {
                            System.IO.File.Create(path).Close();
                        }
                        System.IO.File.WriteAllText(path, $"{lyricTuple.Item1}\n{lyricTuple.Item2}");
                    });
                    resultPath = path;
                }
            }

            await InitLyricList(resultPath);
        }

        public async Task InitLyricList(string lyricPath)
        {
            if (lyricPath == null)
            {
                NowPlayingLyrics.Clear();
                NowLyricsData = null;
                return;
            }

            string f = null;
            var lrcEncode = FileHelper.GetEncodeingType(lyricPath);
            if (lrcEncode == Encoding.Default)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                f = await System.IO.File.ReadAllTextAsync(lyricPath, Encoding.GetEncoding("GB2312"));
            }
            else
            {
                f = await System.IO.File.ReadAllTextAsync(lyricPath, lrcEncode);
            }

            NowPlayingLyrics.Clear();

            if (f.Length < 10)
            {
                System.IO.File.Delete(lyricPath);
                return;
            }

            var lyricDatas = await LyricHelper.LyricToLrcData(f);
            if (lyricDatas.Length > 1)
            {
                foreach (var i in lyricDatas)
                {
                    NowPlayingLyrics.Add(i);
                }

                NowLyricsData = NowPlayingLyrics.First();
            }
            else
            {
                NowLyricsData = null;
            }
        }

        LyricData lastLyricData = null;
        public void ReCallUpdata()
        {
            //timer.Interval = TimeSpan.FromMilliseconds(5);
            if (App.audioPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing && NowPlayingLyrics.Any()
                && (MainWindow.IsDesktopLyricWindowOpen || !MainWindow.isMinSize))
            {
                if (!timer.IsEnabled) timer.Start();
                //System.Diagnostics.Debug.WriteLine(App.audioPlayer.CurrentTime);
                
                foreach (var data in NowPlayingLyrics)
                {
                    if (data.LyricTimeSpan < App.audioPlayer.CurrentTime)
                    {
                        lastLyricData = data;
                    }
                    else
                    {
                        NowLyricsData = lastLyricData;
                        break;
                    }
                }/*
                for (int i = 0; i < NowPlayingLyrics.Count; i++)
                {
                    var npl = NowPlayingLyrics[i];
                    if (npl.LyricTimeSpan < App.audioPlayer.CurrentTime)
                    {
                        lastLyricData = npl;
                    }
                    else
                    {
                        NowLyricsData = lastLyricData;
                        break;
                    }
                }*/
            }
            else
            {
                timer.Stop();
            }
        }

        private async void AudioPlayer_SourceChanged(Media.AudioPlayer audioPlayer)
        {
            if (MusicData != audioPlayer.MusicData)
            {
                MusicData = audioPlayer.MusicData;
                await InitLyricList(audioPlayer.MusicData);
                PlayingLyricSourceChange?.Invoke(NowPlayingLyrics);
                timer.Start();
            }
        }
    }
}
