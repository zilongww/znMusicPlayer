﻿using Downloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Media.Protection.PlayReady;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Helpers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace znMusicPlayerWUI.Background
{
    public enum DownloadStates { Waiting, Downloading, DownloadedPreview, Downloaded, Error }
    public class DownloadData
    {
        public MusicData MusicData;
        public long FileSize;
        public long DownloadedSize;
        public decimal DownloadPercent;
        public DownloadStates DownloadState;
    }

    public class DownloadManager
    {
        public List<DownloadData> AllDownloadData { get; set; } = new();
        public List<DownloadData> WaitingDownloadData { get; set; } = new();
        public List<DownloadData> DownloadingData { get; set; } = new();
        public List<DownloadData> DownloadedData { get; set; } = new();
        public List<DownloadData> DownloadErrorData { get; set; } = new();

        public delegate void DownloadHandler(DownloadData data);
        public event DownloadHandler AddDownload;
        public event DownloadHandler OnDownloading;
        public event DownloadHandler OnDownloadedPreview;
        public event DownloadHandler OnDownloaded;
        public event DownloadHandler OnDownloadError;

        public DataFolderBase.DownloadQuality DownloadQuality { get; set; } = DataFolderBase.DownloadQuality.lossless;
        public DataFolderBase.DownloadNamedMethod DownloadNamedMethod { get; set; } = DataFolderBase.DownloadNamedMethod.t_ar_al;
        public int DownloadingMaximum { get; set; } = 3;
        public bool IDv3WriteImage { get; set; } = true;
        public bool IDv3WriteArtistImage { get; set; } = true;
        public bool IDv3WriteLyric { get; set; } = true;
        public bool SaveLyricToLrcFile { get; set; } = true;

        public DownloadManager()
        {
            OnDownloaded += (_) =>
            {
                UpdateDownload();
            };
        }

        /// <summary>
        /// 添加下载歌曲
        /// </summary>
        /// <param name="musicData"></param>
        public void Add(MusicData musicData)
        {
            var dm = new DownloadData() { MusicData = musicData };
            if (!DownloadingData.Contains(dm) || !DownloadedData.Contains(dm) || !WaitingDownloadData.Contains(dm))
            {
                dm.DownloadState = DownloadStates.Waiting;
                AllDownloadData.Add(dm);
                WaitingDownloadData.Add(dm);
                AddDownload?.Invoke(dm);
                try
                {
                    UpdateDownload();
                }
                catch (Exception err)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(err.Message);
#endif
                }
            }
        }

        public void UpdateDownload()
        {
            while (DownloadingData.Count < DownloadingMaximum && WaitingDownloadData.Any())
            {
                var dm = WaitingDownloadData[0];
                WaitingDownloadData.Remove(dm);
                DownloadingData.Add(dm);
                try
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"下载中：{dm.MusicData.Title}");
#endif
                    StartDownload(dm);
                }
                catch (Exception err)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(err.Message);
#endif
                    DownloadingData.Remove(dm);
                    DownloadErrorData.Add(dm);
                    dm.DownloadState = DownloadStates.Error;
                    OnDownloadError?.Invoke(dm);
                }
            }
        }

        public async void StartDownload(DownloadData dm)
        {
            System.Diagnostics.Debug.WriteLine($"d:{dm.MusicData.Title}");
            dm.DownloadState = DownloadStates.Downloading;

            string downloadPath1 = null;
            switch (DownloadNamedMethod)
            {
                case DataFolderBase.DownloadNamedMethod.t_ar_al:
                    downloadPath1 =
                        $"{DataFolderBase.DownloadFolder}\\" +
                        $"{CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.Title)} - {CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.ButtonName)}";
                    break;
                case DataFolderBase.DownloadNamedMethod.t_ar:
                    downloadPath1 =
                        $"{DataFolderBase.DownloadFolder}\\" +
                        $"{CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.Title)} - {CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.ArtistName)}";
                    break;
                case DataFolderBase.DownloadNamedMethod.t_al_ar:
                    downloadPath1 =
                        $"{DataFolderBase.DownloadFolder}\\" +
                        $"{CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.Title)} - {CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.Album.Title)} · {CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.ArtistName)}";
                    break;
                case DataFolderBase.DownloadNamedMethod.t_al:
                    downloadPath1 =
                        $"{DataFolderBase.DownloadFolder}\\" +
                        $"{CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.Title)} - {CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.Album.Title)}";
                    break;
                case DataFolderBase.DownloadNamedMethod.t:
                    downloadPath1 =
                        $"{DataFolderBase.DownloadFolder}\\" +
                        $"{CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.Title)}";
                    break;
            }
                
            string addressPath = null;
            bool isErr = false;

            var downloadQuality = DownloadQuality;
            var writeImage = IDv3WriteImage;
            var writeArtistImage = IDv3WriteArtistImage;
            var writeLyric = IDv3WriteLyric;
            var saveLyric = SaveLyricToLrcFile;
            try
            {
                addressPath = await App.metingServices.NeteaseServices.GetUrl(dm.MusicData.ID, (int)downloadQuality);
            }
            catch
            {
                isErr = true;
            }
            if (string.IsNullOrEmpty(addressPath) || isErr)
            {
                dm.DownloadState = DownloadStates.Error;
                DownloadingData.Remove(dm);
                DownloadErrorData.Add(dm);
                OnDownloadError?.Invoke(dm);
                return;
            }

            string lastName = null;
            await Task.Run(() =>
            {
                lastName = new FileInfo(addressPath).Extension;
            });
            string downloadPath = downloadPath1 + lastName;
            string lyricPath = downloadPath1 + ".lrc";
            //await WebHelper.DownloadFileAsync(addressPath, downloadPath);

            System.Net.WebClient TheDownloader = new System.Net.WebClient(); 
            TheDownloader.DownloadProgressChanged += (s, e) =>
            {
                if (e == null) return;
                dm.DownloadPercent = e.ProgressPercentage;
                dm.FileSize = e.TotalBytesToReceive;
                dm.DownloadedSize = e.BytesReceived;
                dm.DownloadState = DownloadStates.Downloading;
                OnDownloading?.Invoke(dm);
                //System.Diagnostics.Debug.WriteLine(e.ProgressPercentage);
                //Set1(e.ProgressPercentage, (Convert.ToDouble(e.BytesReceived) / Convert.ToDouble(e.TotalBytesToReceive) * 100).ToString("0.0") + "%", zilongcn.Others.GetAutoSizeString(e.BytesReceived, 2) + "/" + zilongcn.Others.GetAutoSizeString(e.TotalBytesToReceive, 2));
            };
            TheDownloader.DownloadFileCompleted += (s, e) =>
            {
                TheDownloader?.Dispose();
            };
            await TheDownloader.DownloadFileTaskAsync(new Uri(addressPath), downloadPath);

            dm.DownloadState = DownloadStates.DownloadedPreview;
            OnDownloadedPreview?.Invoke(dm);

            List<Tuple<string, byte[]>> artistsPictureData = new();
            byte[] picDatas = null;
            if (writeImage)
            {
                try
                {
                    picDatas = await WebHelper.Client.GetByteArrayAsync(await WebHelper.GetPicturePathAsync(dm.MusicData));
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(err.ToString());
                }
                if (IDv3WriteArtistImage)
                {
                    try
                    {
                        foreach (var a in dm.MusicData.Artists)
                        {
                            string result = a.PicturePath;
                            if (a.PicturePath == null)
                            {
                                result =
                                    (await App.metingServices.NeteaseServices.GetArtist(a.ID)).PicturePath;
                            }
                            var data = await WebHelper.Client.GetByteArrayAsync(result);
                            artistsPictureData.Add(new(a.Name, data));
                        }
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Debug.WriteLine(err.ToString());
                    }
                }
            }

            TagLib.File tagFile = null;
            TagLib.Tag tag = null;
            await Task.Run(() =>
            {
                tagFile = TagLib.File.Create(downloadPath);
                tag = tagFile.Tag;

                List<TagLib.IPicture> pictures = new() { };
                if (picDatas != null)
                {
                    var cover = new TagLib.Id3v2.AttachmentFrame
                    {
                        Type = TagLib.PictureType.FrontCover,
                        Description = "Cover",
                        Data = new TagLib.ByteVector(picDatas),
                        TextEncoding = TagLib.StringType.UTF16
                    };
                    pictures.Add(cover);
                }

                if (artistsPictureData.Any())
                {
                    foreach (var a in artistsPictureData)
                    {
                        TagLib.Id3v2.AttachmentFrame artistImage = new()
                        {
                            Type = TagLib.PictureType.Artist,
                            Description = a.Item1,
                            Data = new TagLib.ByteVector(a.Item2),
                            TextEncoding = TagLib.StringType.UTF16
                        };
                        pictures.Add(artistImage);
                    }
                }

                tag.Pictures = pictures.ToArray();
                tag.Title = dm.MusicData.Title;
                tag.Album = dm.MusicData.Album.Title;
                tag.Comment = $"Download with {App.AppName}";
                tag.Description = tag.Comment;

                List<string> artists = new();
                foreach (var a in dm.MusicData.Artists)
                {
                    artists.Add(a.Name);
                }
                tag.Performers = artists.ToArray();
                tag.AlbumArtists = tag.Performers;
            });

            var lyric = await App.metingServices.NeteaseServices.GetLyric(dm.MusicData.ID);
            if (!lyric.Item1.Contains("纯音乐，请欣赏"))
            {
                if (lyric.Item1.Length >= 10)
                {
                    await Task.Run(() =>
                    {
                        if (saveLyric)
                        {
                            File.Create(lyricPath).Dispose();
                            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                            File.WriteAllText(lyricPath, $"{lyric.Item1}\n{lyric.Item2}", Encoding.GetEncoding("GB2312"));
                        }
                        
                        if (writeLyric) tag.Lyrics = $"{lyric.Item1}\n{lyric.Item2}";
                    });
                }
                else
                {

                }
            }

            await Task.Run(() =>
            {
                tagFile.Save();
                tagFile.Dispose();
            });
            dm.DownloadState = DownloadStates.Downloaded;
            DownloadingData.Remove(dm);
            DownloadedData.Add(dm);
            OnDownloaded?.Invoke(dm);
            System.Diagnostics.Debug.WriteLine($"下载完成：{dm.MusicData.Title}");
        }

        public void CallOnDownloadingEvent(DownloadData dm)
        {
            OnDownloading?.Invoke(dm);
        }
        public void CallOnDownloadedEvent(DownloadData dm)
        {
            OnDownloaded?.Invoke(dm);
        }
        public void CallOnDownloadErrorEvent(DownloadData dm)
        {
            OnDownloadError?.Invoke(dm);
        }
    }
}
