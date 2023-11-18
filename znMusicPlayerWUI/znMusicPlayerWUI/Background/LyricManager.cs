﻿using Meting4Net.Core.Models.Tencent;
using Microsoft.UI.Xaml;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Helpers;
using static znMusicPlayerWUI.Helpers.MetingService.KgMeting;

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
                    InovkeLyricChangeEvent(value);
                }
                else if (_nowLyricsData != value)
                {
                    _nowLyricsData = value;
                    InovkeLyricChangeEvent(value);
                }
            }
        }

        private void InovkeLyricChangeEvent(LyricData lyricData)
        {
            PlayingLyricSelectedChange?.Invoke(lyricData);
            Debug.WriteLine($"[LyricManager]: 当前歌词已设置为：\"{lyricData?.Lyric?.FirstOrDefault()}\"");
        }

        public LyricManager()
        {
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (_, __) => ReCallUpdate();

            //MainWindow.WindowViewStateChanged += MainWindow_WindowViewStateChanged;
            App.audioPlayer.SourceChanged += AudioPlayer_SourceChanged;
            App.audioPlayer.PlayStateChanged += AudioPlayer_PlayStateChanged;
        }

        private void AudioPlayer_PlayStateChanged(Media.AudioPlayer audioPlayer)
        {
            if (App.audioPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
            {
                StartTimer();
            }
        }

        public async Task InitLyricList(MusicData musicData)
        {
            Debug.WriteLine($"[LyricManager]: 初始化歌词：\"{musicData.Title}\"");
            if (musicData == null) return;
            NowPlayingLyrics.Clear();

            string cachePath = await FileHelper.GetLyricCache(musicData);
            string resultPath = null;

            if (cachePath != null)
            {
                resultPath = cachePath;
                Debug.WriteLine($"[LyricManager]: 找到歌词缓存：\"{cachePath}\"");
            }
            else
            {
                if (musicData.From == MusicFrom.localMusic)
                {
                    try
                    {
                        var tagfile = await Task.Run(() => TagLib.File.Create(musicData.InLocal));
                        await InitLyricList(tagfile);
                    }
                    catch { }
                    return;
                }

                Debug.WriteLine($"[LyricManager]: 从网络中下载歌词");
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
                    Debug.WriteLine($"[LyricManager]: 下载网络歌词完成");
                }
            }

            await InitLyricList(resultPath);
            Debug.WriteLine($"[LyricManager]: 初始化歌词成功： \"{musicData.Title}\"");
        }

        public async Task InitLyricList(TagLib.File file)
        {
            if (string.IsNullOrEmpty(file.Tag.Lyrics)) return;
            Debug.WriteLine($"[LyricManager]: 从 IDv3 标签中获取歌词");
            InitLyricList(await LyricHelper.LyricToLrcData(file.Tag.Lyrics));
        }

        public async Task InitLyricList(string lyricPath)
        {
            if (lyricPath == null)
            {
                NowPlayingLyrics.Clear();
                NowLyricsData = null;
                Debug.WriteLine($"[LyricManager]: 无法获取有效");
                return;
            }

            Debug.WriteLine($"[LyricManager]: 读取歌词文件：\"{lyricPath}\"");
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

            if (f.Length < 10)
            {
                Debug.WriteLine($"[LyricManager]: 歌词文件大小未超过 10 字节，不会使用此歌词文件");
                //System.IO.File.Delete(lyricPath);
                return;
            }

            InitLyricList(await LyricHelper.LyricToLrcData(f));
        }

        public void InitLyricList(LyricData[] lyricDatas)
        {
            if (lyricDatas.Length > 1)
            {
                foreach (var i in lyricDatas)
                {
                    NowPlayingLyrics.Add(i);
                }
            }
            NowLyricsData = null;
        }

        public void StartTimer()
        {
            //Debug.WriteLine($"[LyricManager]: 歌词循环已开始");
            ReCallUpdate();
        }
        
        private void StopTimer()
        {
            //Debug.WriteLine($"[LyricManager]: 歌词循环已停止");
            timer.Stop();
        }

        LyricData lastLyricData = null;
        public void ReCallUpdate()
        {
            //System.Diagnostics.Debug.WriteLine($"Lyric Lasted Count {PlayingLyricSelectedChange?.GetInvocationList().Length}");
            timer.Start();
            if (PlayingLyricSelectedChange == null) StopTimer();
            if (!NowPlayingLyrics.Any()) StopTimer();
            if (NowPlayingLyrics.Count <= 3) StopTimer();

            //System.Diagnostics.Debug.WriteLine($"Lyric Timing Changed: {App.audioPlayer.FileReader?.Position}.");
            //System.Diagnostics.Debug.WriteLine($"Lyric Timing Changed: {App.audioPlayer.FileReader?.WaveFormat.AverageBytesPerSecond}.");
            //System.Diagnostics.Debug.WriteLine($"Lyric Timing Changed: {(decimal)App.audioPlayer.FileReader?.Position / (decimal)App.audioPlayer.FileReader?.WaveFormat.AverageBytesPerSecond}.");

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
            }
            /*
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
        

        private async void AudioPlayer_SourceChanged(Media.AudioPlayer audioPlayer)
        {
            if (MusicData != audioPlayer.MusicData)
            {
                MusicData = audioPlayer.MusicData;
                await InitLyricList(audioPlayer.MusicData);
                PlayingLyricSourceChange?.Invoke(NowPlayingLyrics);

                //if (audioPlayer.NowOutDevice.DeviceType == Media.AudioPlayer.OutApi.Wasapi) timer.Interval = TimeSpan.FromMilliseconds(audioPlayer.Latency);
                //else timer.Interval = TimeSpan.FromMilliseconds(100);
                StartTimer();
            }
        }
    }
}
