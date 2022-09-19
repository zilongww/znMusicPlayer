using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace znMusicPlayerWPF.MusicPlay
{
    public class MusicPlayCache
    {
        MainWindow TheParent;
        TheMusicDatas.MusicData MusicData = new TheMusicDatas.MusicData();

        public MusicPlayCache(MainWindow TheParent) => this.TheParent = TheParent;

        public async Task<Tuple<string, TheMusicDatas.MusicData>> GetMusicCache(TheMusicDatas.MusicData musicData)
        {
            MusicData = musicData;
            Tuple<string, TheMusicDatas.MusicData> CachePath = new Tuple<string, TheMusicDatas.MusicData>(await IsCacheExists(musicData), musicData);
            bool cantPlay = false;

            if (CachePath.Item1 == null)
            {
                if (!Source.InternetConnect() && musicData.From != TheMusicDatas.MusicFrom.localMusic)
                {
                    CachePath = new Tuple<string, TheMusicDatas.MusicData>("internetError", musicData);
                }
                else
                {
                    TheParent.SetAlbumImageState(MainWindow.AlbumImageState.Load);

                    switch (musicData.From)
                    {
                        case TheMusicDatas.MusicFrom.kwMusic:
                            CachePath = await GetKwMusicCache(musicData);
                            break;
                        case TheMusicDatas.MusicFrom.kgMusic:
                            CachePath = await GetKgMusicCache(musicData);
                            break;
                        case TheMusicDatas.MusicFrom.neteaseMusic:
                            CachePath = await GetNeteaseMusicCache(musicData);
                            break;
                        case TheMusicDatas.MusicFrom.qqMusic:
                            CachePath = new Tuple<string, TheMusicDatas.MusicData>(await GetQqMusicCache(musicData), musicData);
                            break;
                        case TheMusicDatas.MusicFrom.miguMusic:
                            CachePath = new Tuple<string, TheMusicDatas.MusicData>(await GetMiguMusicCache(musicData), musicData);
                            break;
                        case TheMusicDatas.MusicFrom.localMusic:
                            CachePath = new Tuple<string, TheMusicDatas.MusicData>(musicData.IsDownload, musicData);
                            break;
                        default:
                            CachePath = new Tuple<string, TheMusicDatas.MusicData>("noSupportType", musicData);
                            break;
                    }
                }
            }

            if (TheParent.NowPlaySong.MD5 == musicData.MD5)
            {
                switch (CachePath.Item1)
                {
                    case "pay":
                        TheParent.ShowBox("无法播放此歌曲", "此歌曲资源不存在或为付费歌曲，无法播放。\n请切换播放源试试。");
                        cantPlay = true;
                        break;
                    case "error":
                        TheParent.ShowBox("歌曲加载失败", "无法加载此歌曲，将自动切换到下一首。");
                        cantPlay = true;
                        NextSongAsync(musicData);
                        break;
                    case "internetError":
                        TheParent.ShowBox("加载缓存失败", "网络未连接，请连接网络后再试。");
                        cantPlay = true;
                        break;
                    case "noSupportType":
                        TheParent.ShowBox("加载缓存失败", "不支持加载此平台的歌曲。");
                        cantPlay = true;
                        break;
                    default:
                        break;
                }

                TheParent.SetAlbumImageState(MainWindow.AlbumImageState.None);

                if (cantPlay)
                {
                    TheParent.SetAlbumImageState(MainWindow.AlbumImageState.Error);
                    CachePath = new Tuple<string, TheMusicDatas.MusicData>("errorCache", musicData);
                }
            }

            return CachePath;
        }

        private async void NextSongAsync(TheMusicDatas.MusicData musicData)
        {
            await Task.Delay(3000);

            if (TheParent.NowPlaySong.MD5 == musicData.MD5)
                TheParent.SetNextSong();
        }

        private async Task<Tuple<string, TheMusicDatas.MusicData>> GetKwMusicCache(TheMusicDatas.MusicData musicData)
        {
            Tuple<string, string> tuple = await MusicAddressGet.kwMusicAddressGet(musicData);
            string MusicAddress = BaseFoldsPath.SongsFolderPath + musicData.From + musicData.SongRid + tuple.Item2;

            switch (tuple.Item1)
            {
                case "error":
                    MusicAddress = "error";
                    break;
                case "pay":
                    MusicAddress = "pay";
                    break;
                default:
                    WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 };
                    webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                    await webClient.DownloadFileTaskAsync(new Uri(tuple.Item1), MusicAddress);
                    break;
            }

            return new Tuple<string, TheMusicDatas.MusicData>(MusicAddress, musicData);
        }

        private async Task<Tuple<string, TheMusicDatas.MusicData>> GetKgMusicCache(TheMusicDatas.MusicData musicData)
        {
            Tuple<string, JObject> tuple = await MusicAddressGet.kgMusicAddressGet(musicData);
            string MusicAddress = $"{BaseFoldsPath.SongsFolderPath}{musicData.From}{musicData.SongRid}.mp3";

            if (tuple.Item1 == "")
            {
                MusicAddress = "pay";
            }
            else
            {
                musicData.PicUri = tuple.Item2["data"]["img"].ToString();
                musicData.ThekgMusicLrcs = tuple.Item2["data"]["lyrics"].ToString();

                WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 };
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                await webClient.DownloadFileTaskAsync(new Uri(tuple.Item1), MusicAddress);
            }

            return new Tuple<string, TheMusicDatas.MusicData>(MusicAddress, musicData);
        }

        private async Task<Tuple<string, TheMusicDatas.MusicData>> GetNeteaseMusicCache(TheMusicDatas.MusicData musicData)
        {
            Tuple<string, TheMusicDatas.MusicData> tuple = await MusicAddressGet.neteaseMusicAddressGet(musicData);
            string MusicUrl = tuple.Item1;
            musicData = tuple.Item2;

            if (MusicUrl == "https://music.163.com/404" || MusicUrl == null)
            {
                return new Tuple<string, TheMusicDatas.MusicData>("pay", musicData);
            }
            else if (MusicUrl.Split('|')[0] == "ERROR ")
            {
                return new Tuple<string, TheMusicDatas.MusicData>("error", musicData);
            }
            else
            {
                string MusicAddress = $"{BaseFoldsPath.SongsFolderPath}{musicData.From}{musicData.SongRid}.mp3";

                WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 };
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                await webClient.DownloadFileTaskAsync(new Uri(tuple.Item1), MusicAddress);

                return new Tuple<string, TheMusicDatas.MusicData>(MusicAddress, musicData);
            }
        }

        private async Task<string> GetQqMusicCache(TheMusicDatas.MusicData musicData)
        {
            string web = await MusicAddressGet.qqMusicAddressGet(musicData);

            if (true)
            {
                string musicAddress = $"{BaseFoldsPath.SongsFolderPath}{musicData.From}{musicData.SongRid}.mp3";

                WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 };
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                await webClient.DownloadFileTaskAsync(web, musicAddress);

                return musicAddress;
            }
        }

        private async Task<string> GetMiguMusicCache(TheMusicDatas.MusicData musicData)
        {
            string web = await MusicAddressGet.miguMusicAddressGet(musicData);

            if (true)
            {
                string musicAddress = $"{BaseFoldsPath.SongsFolderPath}{musicData.From}{musicData.SongRid}.flac";

                WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 };
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

                try
                {
                    await webClient.DownloadFileTaskAsync(web, musicAddress);
                }
                catch
                {
                    if (musicData.miguSongPath != null)
                    {
                        await Task.Run(() =>
                        {
                            zilongcn.Others.FtpFileDownload(musicAddress, musicData.miguSongPath, "", "");
                        });
                    }
                    else
                    {
                        musicAddress = "pay";
                    }
                }

                return musicAddress;
            }
        }


        private async Task<string> IsCacheExists(TheMusicDatas.MusicData musicData)
        {
            return await Task.Run(() =>
            {
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(BaseFoldsPath.SongsFolderPath);
                System.IO.FileInfo[] fileInfo = directory.GetFiles();
                foreach (System.IO.FileInfo file in fileInfo)
                {
                    string name = file.Name.Replace(".mp3", "").Replace(".wma", "").Replace(".aac", "").Replace(".m4a", "").Replace(".flac", "");
                    if (name == musicData.From + musicData.SongRid)
                    {
                        return file.FullName;
                    }
                }
                return null;
            });
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (TheParent.NowPlaySong.MD5 == MusicData.MD5) TheParent.IsMusicLoadingText.Text = e.ProgressPercentage.ToString() + "%";
        }
    }
}
