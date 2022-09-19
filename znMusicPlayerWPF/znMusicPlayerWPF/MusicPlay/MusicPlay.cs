using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace znMusicPlayerWPF.MusicPlay
{
    public class TheMusicDatas
    {
        public enum MusicFrom { kwMusic, kgMusic, qqMusic, neteaseMusic, miguMusic, localMusic, otherMusic }

        public enum MusicKbps { aac, wma, Kbps128, Kbps192, Kbps320, Kbps1000 }

        public struct MusicData
        {
            public string Title { get; set; }
            public string SongRid { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public string AlbumID { get; set; }
            public string PicUri { get; set; }
            public string Time { get; set; }
            public object OtherData { get; set; }
            public MusicFrom From { get; set; }
            public MusicKbps Kbps { get; set; }
            public string IsDownload { get; set; }
            public JObject NeteaseAlbumData { get; set; }
            public string ThekgMusicBackupHash { get; set; }
            public string ThekgMusicLrcs { get; set; }
            public string MD5 { get; set; }
            public int Played { get; set; }
            public string miguLrc { get; set; }
            public string miguSongPath { get; set; }
            public string miguSongCopyrightId { get; set; }

            public MusicData(string Thetitle = "", string TheSongRid = "", string TheArtist = "", string TheAlbum = null, string TheAlbumID = null, string ThePicUri = null, string TheTime = null, object TheOtherData = null, MusicFrom TheFrom = MusicFrom.kwMusic, MusicKbps TheKbps = MusicKbps.Kbps192, string TheDownload = null, JObject TheNeteaseAlbumData = null, string kgBackupHash = null, string KgMusicLrcs = null, bool IsPlaying = false) : this()
            {
                this.Title = Thetitle;
                this.SongRid = TheSongRid;
                this.Artist = TheArtist;
                this.Album = TheAlbum;
                this.AlbumID = TheAlbumID;
                this.PicUri = ThePicUri;
                this.Time = TheTime;
                TheOtherData = null;
                //this.OtherData = TheOtherData;
                this.From = TheFrom;
                this.Kbps = TheKbps;
                this.IsDownload = TheDownload;
                TheNeteaseAlbumData = null;
                //this.NeteaseAlbumData = TheNeteaseAlbumData;
                this.ThekgMusicBackupHash = kgBackupHash;
                this.ThekgMusicLrcs = KgMusicLrcs;
                this.MD5 = zilongcn.Others.ToMD5($"{this.Title}{this.Artist}{this.Album}{this.SongRid}{this.AlbumID}{this.From}{this.IsDownload}");
                this.Played = 0;
            }
        }

        public class MusicListData
        {
            public string listName { get; set; }
            public string listShowName { get; set; }
            public string picPath { get; set; }
            public MusicFrom listFrom { get; set; }
            public string listId { get; set; }
            public List<MusicData> songs { get; set; }
            public int listCount { get; set; }
            public string MD5 { get; set; }

            public MusicListData(string listName = null, string listShowName = null, string picPath = null, MusicFrom? listFrom = null, string listId = null, List<MusicData> songs = null)
            {
                this.listName = listName;
                this.listShowName = listShowName;
                this.picPath = picPath;
                this.listFrom = listFrom == null ? new MusicFrom() : (MusicFrom)listFrom;
                this.listId = listId;
                this.songs = songs;
                this.listCount = songs == null ? 0 : songs.Count;
                MD5 = zilongcn.Others.ToMD5($"{listName}{picPath}{listFrom}{listId}{this.listCount}");
            }
        }

        public static MusicFrom MusicFromFromString(string text)
        {
            TheMusicDatas.MusicFrom musicFrom = TheMusicDatas.MusicFrom.localMusic;

            try
            {
                switch (text)
                {
                    case "kwMusic":
                        musicFrom = TheMusicDatas.MusicFrom.kwMusic;
                        break;
                    case "neteaseMusic":
                        musicFrom = TheMusicDatas.MusicFrom.neteaseMusic;
                        break;
                    case "kgMusic":
                        musicFrom = TheMusicDatas.MusicFrom.kgMusic;
                        break;
                    case "qqMusic":
                        musicFrom = TheMusicDatas.MusicFrom.qqMusic;
                        break;
                    case "miguMusic":
                        musicFrom = TheMusicDatas.MusicFrom.miguMusic;
                        break;
                    case "localMusic":
                        musicFrom = TheMusicDatas.MusicFrom.localMusic;
                        break;
                    default:
                        musicFrom = TheMusicDatas.MusicFrom.otherMusic;
                        break;
                }
            }
            catch
            {
                musicFrom = TheMusicDatas.MusicFrom.kwMusic;
            }

            return musicFrom;
        }

        public static async Task<BitmapImage> GetNetImageAsync(string address)
        {
            string md5 = zilongcn.Others.ToMD5(address).Replace("\\", "##").Replace("/", "#");
            string imagePath = BaseFoldsPath.ImageFolderPath + md5;
            BitmapImage bitmapImage = null;

            bool imageExists = await Task.Run(() =>
            {
                return File.Exists(imagePath);
            });

            if (imageExists)
            {
                // TODO
                bitmapImage = new BitmapImage(await Task.Run(() =>
                {
                    return new Uri(imagePath);
                }));
            }
            else
            {
                // ??系统没连接网络时也返回true
                if (Source.InternetConnect())
                {
                    try
                    {
                        WebClient webClient = new WebClient();
                        await Task.Run(() => { webClient.DownloadFile(address, imagePath); });
                        webClient.Dispose();

                        bitmapImage = new BitmapImage(await Task.Run(() =>
                        {
                            return new Uri(imagePath);
                        }));
                    }
                    catch { bitmapImage = null; }
                }
                else
                {
                    bitmapImage = null;
                }
            }

            await Task.Run(() =>
            {
                try
                {
                    if (File.ReadAllBytes(imagePath).Length == 0) File.Delete(imagePath);
                }
                catch { }
            });

            //bitmapImage.EndInit();
            //bitmapImage.Freeze();
            return bitmapImage;
        }
    }

    public class MusicPlayMod
    {
        public enum PlayMod { Seq, Random, Loop };

        public delegate void PlayModChangeDelegate(PlayMod playMod);
        public static event PlayModChangeDelegate PlayModChange;

        public static string PlayModToString(PlayMod playMod)
        {
            switch (playMod)
            {
                case PlayMod.Seq:
                    return "顺序播放";
                case PlayMod.Random:
                    return "随机播放";
                case PlayMod.Loop:
                    return "单曲循环";
                default:
                    return "顺序播放";
            }
        }

        public static PlayMod NowPlayMod
        {
            get
            {
                return SettingDataEdit.ToPlayMod(SettingDataEdit.GetParam("PlayMod"));
            }
            set
            {
                SettingDataEdit.SetParam("PlayMod", value.ToString());

                if (PlayModChange != null)
                {
                    PlayModChange(value);
                }
            }
        }
    }
}