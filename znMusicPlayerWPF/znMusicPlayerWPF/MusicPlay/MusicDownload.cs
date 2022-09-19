using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using znMusicPlayerWPF.Network;

namespace znMusicPlayerWPF.MusicPlay
{
    public class MusicDownload
    {
        private MainWindow TheParent;
        public Pages.DownloadCard DownloadCard;

        public MusicDownload(MainWindow TheParent)
        {
            this.TheParent = TheParent;
        }

        public async Task StartDownload(TheMusicDatas.MusicData musicData)
        {
            try
            {
                switch (musicData.From)
                {
                    case TheMusicDatas.MusicFrom.kwMusic:
                        await kwMusicDownload(musicData);
                        break;
                    case TheMusicDatas.MusicFrom.kgMusic:
                        await kgMusicDownload(musicData);
                        break;
                    case TheMusicDatas.MusicFrom.neteaseMusic:
                        await NeteaseMusicDownload(musicData);
                        break;
                    case TheMusicDatas.MusicFrom.qqMusic:
                        await QqMusicDownload(musicData);
                        break;
                    case TheMusicDatas.MusicFrom.miguMusic:
                        await MiguMusicDownload(musicData);
                        break;
                    default:
                        TheParent.ShowBox("注意", "无法下载此平台的歌曲，请切换其他平台搜索试试。");
                        break;
                }
            }
            catch (Exception e)
            {
                TheParent.ShowBox("下载失败", "下载时出现错误，请重试。\n错误信息：" + e.ToString());
            }
        }

        private async void SetDownload(TheMusicDatas.MusicData musicData, string downloadUrl, string downloadPath, string picPath, string lrcPath, string lrcData)
        {
            DownloadCard = TheParent.TheDownload.AddACard(musicData.Title, musicData.Artist + " - " + musicData.Album, 3, "0.0% - 0MB", musicData.SongRid, downloadUrl, downloadPath, BaseFoldsPath.DownloadFolderPath, musicData, picPath, lrcPath, TheParent);
            zilongcn.Animations.animateOpacity(DownloadCard, 0, 1, TheParent.AnimateOpacityNormalTime, IsAnimate: TheParent.Animation).Begin();
            TheParent.TheDownload.SizeChangedDo(null, null);

            if (lrcData != null)
                await Task.Run(() =>
                {
                    File.WriteAllText(lrcPath, lrcData, Encoding.GetEncoding("GB2312"));
                });
        }

        private async Task kwMusicDownload(TheMusicDatas.MusicData musicData)
        {
            TheMusicDatas.MusicKbps Kbps = TheMusicDatas.MusicKbps.Kbps1000;
            string FileName = zilongcn.Others.ReplaceBadCharOfFileName(musicData.Title + " - " + musicData.Artist);

            Tuple<string, string> MData = await MusicAddressGet.kwMusicAddressGet(musicData);
            string TheWeb = MData.Item1;
            string LastNames = MData.Item2;

            string DownloadPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + LastNames;
            string PicPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + "Pic.jpg";
            string LrcPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + ".lrc";

            string LrcData = await Task.Run(() =>
            {
                string LyricData = null;
                try
                {
                    LyricData = new znWebClient().GetString("http://iecoxe.top:5000/v1/kuwo/lyric?rid=" + musicData.SongRid);
                }
                catch { return null; }
                if (LyricData == "") return null;

                try
                {
                    JObject Lyrics = JObject.Parse(LyricData);
                    string l = Lyrics["lyric_str"].ToString();
                    if (l == "[00:00:00]此歌曲为没有填词的纯音乐，请您欣赏\n" || l == "") return null;
                    return l;
                }
                catch { return null; }
            });

            SetDownload(musicData, TheWeb, DownloadPath, PicPath, LrcPath, LrcData);
        }

        private async Task kgMusicDownload(TheMusicDatas.MusicData musicData)
        {
            Tuple<string, JObject> tuple = await MusicAddressGet.kgMusicAddressGet(musicData);
            string MusicUri = tuple.Item1;
            string MusicPic = tuple.Item2["data"]["img"].ToString();

            string FileName = zilongcn.Others.ReplaceBadCharOfFileName(musicData.Title + " - " + musicData.Artist);
            string DownloadPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + ".mp3";
            string PicPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + "Pic.jpg";
            string LrcPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + ".lrc";

            musicData.PicUri = MusicPic;

            string LrcData = await Task.Run(() =>
            {
                string l = tuple.Item2["data"]["lyrics"].ToString();
                if (l == "[00:00:00]此歌曲为没有填词的纯音乐，请您欣赏\n" || l == "") return null;
                return l;
            });

            SetDownload(musicData, MusicUri, DownloadPath, PicPath, LrcPath, LrcData);
        }

        private async Task NeteaseMusicDownload(TheMusicDatas.MusicData musicData)
        {
            Tuple<string, TheMusicDatas.MusicData> tuple = await MusicAddressGet.neteaseMusicAddressGet(musicData);
            string MusicUri = tuple.Item1;
            musicData = tuple.Item2;

            string FileName = zilongcn.Others.ReplaceBadCharOfFileName(musicData.Title + " - " + musicData.Artist);
            string DownloadPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + ".mp3";
            string PicPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + "Pic.jpg";
            string LrcPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + ".lrc";

            string LrcData = await Task.Run(() =>
            {
                string Lrc = MusicAddressGet.GetNeteaseMusicLrc(musicData.SongRid);
                if (Lrc == "") return null;

                return Lrc;
            });

            SetDownload(musicData, MusicUri, DownloadPath, PicPath, LrcPath, LrcData);
        }

        private async Task QqMusicDownload(TheMusicDatas.MusicData musicData)
        {
            string MusicUri = await MusicAddressGet.qqMusicAddressGet(musicData);

            string FileName = zilongcn.Others.ReplaceBadCharOfFileName(musicData.Title + " - " + musicData.Artist);
            string DownloadPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + ".mp3";
            string PicPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + "Pic.jpg";
            string LrcPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + ".lrc";

            string LrcData = await Task.Run(() =>
            {
                string Lrc = MusicAddressGet.GetQqMusicLrc(musicData.SongRid);
                if (Lrc == "") return null;

                return Lrc;
            });

            SetDownload(musicData, MusicUri, DownloadPath, PicPath, LrcPath, LrcData);
        }

        private async Task MiguMusicDownload(TheMusicDatas.MusicData musicData)
        {
            string MusicUri = await MusicAddressGet.miguMusicAddressGet(musicData);

            string FileName = zilongcn.Others.ReplaceBadCharOfFileName(musicData.Title + " - " + musicData.Artist);
            string DownloadPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + ".flac";
            string PicPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + "Pic.jpg";
            string LrcPath = BaseFoldsPath.DownloadFolderPath + @"\" + FileName + ".lrc";

            string LrcData = await Task.Run(() =>
            {
                string Lrc = MusicAddressGet.GetMiguMusicLrc(musicData);
                if (Lrc == "") return null;

                return Lrc;
            });

            SetDownload(musicData, MusicUri, DownloadPath, PicPath, LrcPath, LrcData);
        }
    }
}
