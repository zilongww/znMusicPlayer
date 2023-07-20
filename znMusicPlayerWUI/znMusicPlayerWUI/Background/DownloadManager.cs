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
        public int DownloadingMaxium { get; set; } = 3;

        public DownloadManager()
        {
            OnDownloaded += (_) =>
            {
                UpdataDownload();
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
                    UpdataDownload();
                }
                catch (Exception err)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(err.Message);
#endif
                }
            }
        }

        public void UpdataDownload()
        {
            while (DownloadingData.Count < DownloadingMaxium && WaitingDownloadData.Any())
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

        public int br { get; set; } = 960;
        public async void StartDownload(DownloadData dm)
        {
            System.Diagnostics.Debug.WriteLine($"d:{dm.MusicData.Title}");
            string downloadPath1 =
                $"{DataFolderBase.DownloadFolder}\\{CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.Title)} - {CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.ButtonName)}";
            string addressPath = null;
            bool isErr = false;
            try
            {
                addressPath = await App.metingServices.NeteaseServices.GetUrl(dm.MusicData.ID, br);
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

            string lastName = new FileInfo(addressPath).Extension;
            string downloadPath = downloadPath1 + lastName;
            string lyricPath = downloadPath1 + ".lrc";
            string picPath = downloadPath1 + ".imagez";
            /*
            System.Diagnostics.Debug.WriteLine(addressPath);
            System.Diagnostics.Debug.WriteLine(lastName);
            System.Diagnostics.Debug.WriteLine(downloadPath);*//*
            var downloader = new DownloadService();
            await downloader.DownloadFileTaskAsync(addressPath, downloadPath);
            downloader.Dispose();*/
            await WebHelper.DownloadFileAsync(addressPath, downloadPath);
/*
            System.Net.WebClient TheDownloader = new System.Net.WebClient();
            await TheDownloader.DownloadFileTaskAsync(new Uri(addressPath), downloadPath);*/
            /*TheDownloader.DownloadProgressChanged += (s, e) =>
            {
                dm.DownloadPercent = e.ProgressPercentage;
                dm.FileSize = e.TotalBytesToReceive;
                dm.DownloadedSize = e.BytesReceived;
                OnDownloading(dm);
                System.Diagnostics.Debug.WriteLine(e.ProgressPercentage);
                //Set1(e.ProgressPercentage, (Convert.ToDouble(e.BytesReceived) / Convert.ToDouble(e.TotalBytesToReceive) * 100).ToString("0.0") + "%", zilongcn.Others.GetAutoSizeString(e.BytesReceived, 2) + "/" + zilongcn.Others.GetAutoSizeString(e.TotalBytesToReceive, 2));
            };
            TheDownloader.DownloadFileCompleted += (s, e) =>
            {
                TheDownloader.Dispose();
            };*/

            OnDownloadedPreview?.Invoke(dm);
            byte[] picDatas = null;

            try
            {
                picDatas = await WebHelper.Client.GetByteArrayAsync(await WebHelper.GetPicturePathAsync(dm.MusicData));
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.ToString());
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
                tag.Pictures = new[] { new TagLib.Picture(new TagLib.ByteVector(picDatas)) };

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
                        File.Create(lyricPath).Dispose();

                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        File.WriteAllText(lyricPath, $"{lyric.Item1}\n{lyric.Item2}", Encoding.GetEncoding("GB2312"));
                    });

                    tag.Lyrics = $"{lyric.Item1}\n{lyric.Item2}";
                }
                else
                {

                }
            }

            // 在上面的异步操作完成后，似乎不会很快地将下载的数据从内存中写入到磁盘中，
            // 因此导致文件被占用。在此每间隔1秒尝试保存。
            int retryCount = 0;
            while (retryCount <= 12)
            {
                try
                {
                    tagFile.Save();
                    break;
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(err.ToString());
                    retryCount++;
                }
            }

            tagFile.Dispose();
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
