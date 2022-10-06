using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Helpers;

namespace znMusicPlayerWUI.Background
{
    public class DownloadManager
    {
        public class DownloadData
        {
            public MusicData MusicData;
            public decimal DownloadPercent;
            //public DownloadStates DownloadState;
        }

        //public enum DownloadStates { Waiting, Downloading, DownloadedPreview, Downloaded, Error  }
        public List<DownloadData> AllDownloadData { get; set; } = new();
        List<DownloadData> WaitingDownloadData { get; set; } = new();
        List<DownloadData> DownloadingData { get; set; } = new();
        List<DownloadData> DownloadedData { get; set; } = new();
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
            if (!DownloadingData.Contains(dm) && !DownloadedData.Contains(dm) && !WaitingDownloadData.Contains(dm))
            {
                AllDownloadData.Add(dm);
                WaitingDownloadData.Add(dm);
                try
                {
                    UpdataDownload();
                    AddDownload?.Invoke(dm);
                }
                catch (Exception err)
                {
                    System.Diagnostics.Debug.WriteLine(err.Message);
                }
            }
        }

        public void UpdataDownload()
        {
            while (DownloadingData.Count < DownloadingMaxium)
            {
                if (WaitingDownloadData.Any())
                {
                    var m = WaitingDownloadData[0];
                    WaitingDownloadData.Remove(m);
                    DownloadingData.Add(m);
                    try
                    {
                        StartDownload(m);
                    }
                    catch (Exception err)
                    {
                        System.Diagnostics.Debug.WriteLine(err.Message);
                    }
                }
                else break;
            }
        }

        public int br { get; set; } = 960;
        public async void StartDownload(DownloadData dm)
        {
            string downloadPath1 =
                $"{DataFolderBase.DownloadFolder}\\{CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.Title)} - {CodeHelper.ReplaceBadCharOfFileName(dm.MusicData.ButtonName)}";
            string addressPath = null;
            try
            {
                addressPath = await App.metingServices.NeteaseServices.GetUrl(dm.MusicData.ID, br);
            }
            catch
            {
                OnDownloadError?.Invoke(dm);
                return;
            }
            if (string.IsNullOrEmpty(addressPath))
            {
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
            System.Diagnostics.Debug.WriteLine(downloadPath);*/

            var response = await WebHelper.Client.GetAsync(addressPath);
            var file = new FileInfo(downloadPath);

            var n = response.Content.Headers.ContentLength;
            var stream = await response.Content.ReadAsStreamAsync();
            using (var fileStream = file.Create())
            using (stream)
            {
                byte[] buffer = new byte[1024];
                var readLength = 0;
                int length;
                int dcount = 0;
                while ((length = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    readLength += length;

                    dcount += 1;
                    if (dcount == 3550)
                    {
                        dcount = 0;
                        var a = Math.Round(readLength / (decimal)n * 100, 1);
                        dm.DownloadPercent = a;
                        OnDownloading?.Invoke(dm);
                        //DownloadingData[DownloadingData.IndexOf(dm)].DownloadPercent = a;
                        //System.Diagnostics.Debug.WriteLine(a);
                    }

                    // 写入到文件
                    await fileStream.WriteAsync(buffer, 0, length);
                }
            }

            OnDownloadedPreview?.Invoke(dm);

            bool picDownloaded = false;
            try
            {
                await Task.Run(() => File.Create(picPath).Dispose());
                await WebHelper.DownloadFileAsync(await WebHelper.GetPicturePathAsync(dm.MusicData), picPath);
                picDownloaded = true;
            }
            catch (Exception err)
            {
                System.Diagnostics.Debug.WriteLine(err.ToString());
            }

            await Task.Run(() =>
            {
                TagLib.File tagFile = TagLib.File.Create(downloadPath);
                TagLib.Tag tag = tagFile.Tag;
                tag.Title = dm.MusicData.Title;
                tag.Album = dm.MusicData.Album;
                tag.Comment = $"Download with {App.AppName}";

                if (picDownloaded)
                {
                    try
                    {
                        tag.Pictures = new[] { new TagLib.Picture(picPath) };
                    }
                    finally
                    {
                        if (File.Exists(picPath))
                        {
                            File.Delete(picPath);
                        }
                    }
                }

                List<string> artists = new();
                foreach (var a in dm.MusicData.Artists)
                {
                    artists.Add(a.Name);
                }
                tag.Performers = artists.ToArray();
                tag.AlbumArtists = tag.Performers;
                tagFile.Save();
                tagFile.Dispose();
            });

            var lyric = await App.metingServices.NeteaseServices.GetLyric(dm.MusicData.ID);
            if (!lyric.Item1.Contains("纯音乐，请欣赏"))
            {
                await Task.Run(() =>
                {
                    File.Create(lyricPath).Dispose();

                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    File.WriteAllText(lyricPath, $"{lyric.Item1}\n{lyric.Item2}", Encoding.GetEncoding("GB2312"));
                });
            }

            DownloadingData.Remove(dm);
            DownloadedData.Add(dm);
            OnDownloaded?.Invoke(dm);
            UpdataDownload();
        }
    }
}
