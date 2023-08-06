using Downloader;
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
    public class DownloadManager
    {
        public class DownloadData
        {
            public MusicData MusicData;
            public long FileSize;
            public long DownloadedSize;
            public decimal DownloadPercent;
            //public DownloadStates DownloadState;
        }

        //public enum DownloadStates { Waiting, Downloading, DownloadedPreview, Downloaded, Error  }
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
                    OnDownloadError?.Invoke(dm);
                }
            }
        }

        public async void StartDownload(DownloadData dm)
        {
            System.Diagnostics.Debug.WriteLine($"d:{dm.MusicData.Title}");

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
            await WebHelper.DownloadFileAsync(addressPath, downloadPath);
            
            OnDownloadedPreview?.Invoke(dm);

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
            }

            TagLib.File tagFile = null;
            TagLib.Tag tag = null;
            await Task.Run(() =>
            {
                tagFile = TagLib.File.Create(downloadPath);
                tag = tagFile.Tag;
/*
                TagLib.Id3v2.AttachmentFrame cover = new()
                {
                    Type = TagLib.PictureType.FrontCover,
                    Description = "Cover",
                    Data = new TagLib.ByteVector(picDatas),
                    TextEncoding = TagLib.StringType.UTF16
                };*/
                if (picDatas != null) tag.Pictures = new[] { new TagLib.Picture(new TagLib.ByteVector(picDatas)) };

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
            DownloadingData.Remove(dm);
            DownloadedData.Add(dm);
            OnDownloaded?.Invoke(dm);
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
