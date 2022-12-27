using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using znMusicPlayerWUI.Background;
using znMusicPlayerWUI.Helpers;
using static System.Net.Mime.MediaTypeNames;

namespace znMusicPlayerWUI.DataEditor
{
    public enum MusicFrom { kwMusic, kgMusic, qqMusic, neteaseMusic, miguMusic, localMusic, otherMusic }
    public enum DataType { 歌曲, 歌单, 本地歌单, 专辑, 用户, 艺术家 }
    public enum SearchDataType { 歌曲 = 1, 歌单 = 1000, 专辑 = 10, 用户 = 1002, 艺术家 = 100 }
    public enum MusicKbps { aac, wma, Kbps128, Kbps192, Kbps320, Kbps1000 }
    public enum PlaySort { 默认排序, 名称升序, 名称降序, 艺术家升序, 艺术家降序, 专辑升序, 专辑降序, 时间升序, 时间降序 }

    public class Artist
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string PicturePath { get; set; }
        public string Describee { get; set; }
        public MusicListData HotSongs { get; set; }

        public Artist(string name = null, string ID = null, string picturePath = null)
        {
            Name = name;
            this.ID = ID;
            PicturePath = picturePath;
        }

        public new string ToString()
        {
            return Name;
        }
    }

    public class MusicData
    {
        public string Title { get; set; }
        public string ID { get; set; }
        public List<Artist> Artists { get; set; }
        public string Album { get; set; }
        public string AlbumID { get; set; }
        public string PicturePath { get; set; }
        public string RelaseTime { get; set; }
        public MusicFrom From { get; set; }
        public MusicKbps Kbps { get; set; }
        public string InLocal { get; set; }
        public string ButtonName { get; set; }

        private string md5 = null;
        public string MD5
        {
            get
            {
                if (md5 == null)
                    md5 = Helpers.CodeHelper.ToMD5($"{Title}{Artists[0]}{Artists.Count}{Album}{ID}{AlbumID}{From}{InLocal}");
                return md5;
            }
        }

        public MusicData(string title = "",
                         string ID = "",
                         List<Artist> artists = null,
                         string album = null,
                         string albumID = null,
                         string picturePath = null,
                         string relaseTime = null,
                         MusicFrom from = MusicFrom.kwMusic,
                         MusicKbps Kbps = MusicKbps.Kbps192,
                         string inLocal = null)
        {
            this.Title = title;
            this.ID = ID;
            this.Artists = artists == null ? new List<Artist>() { new Artist() { Name = "未知" } } : artists;
            this.Album = string.IsNullOrEmpty(album) ? "未知" : album;
            this.AlbumID = albumID == "0" || string.IsNullOrEmpty(albumID) ? null : albumID;
            this.PicturePath = picturePath;
            this.RelaseTime = relaseTime;
            this.From = from;
            this.Kbps = Kbps;
            this.InLocal = inLocal;

            for (int i = 0; i < (Artists.Count); i++)
            {
                ButtonName += $"{Artists[i].ToString()}{(i < (Artists.Count - 1) ? (i < Artists.Count - 2 ? ", " : " & ") : " · ")}";
            }

            ButtonName += Album;
        }

        public static bool operator ==(MusicData left, MusicData right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.MD5 == right.MD5;
        }

        public static bool operator !=(MusicData left, MusicData right)
        {
            if (left is null && right is null) return false;
            return !(left == right);
        }

        public override bool Equals(object other)
        {
            if (!(other is MusicData)) return false;
            return string.Equals(MD5, (other as MusicData).MD5, StringComparison.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return (MD5 != null ? StringComparer.InvariantCulture.GetHashCode(MD5) : 0);
        }
    }

    public class MusicListData
    {
        public string ListName { get; set; }
        public string ListShowName { get; set; }
        public string PicturePath { get; set; }
        public MusicFrom ListFrom { get; set; }
        public DataType ListDataType { get; set; }
        public string ID { get; set; }
        public int ListCount { get; set; }
        public string MD5 { get; set; }
        public PlaySort PlaySort { get; set; }
        public List<MusicData> Songs { get; set; }

        public MusicListData(string listName = null, string listShowName = null, string picturePath = null,
            MusicFrom listFrom = default, string ID = null, List<MusicData> songs = null, DataType listDataType = default)
        {
            this.ListName = listName;
            this.ListShowName = listShowName;
            this.PicturePath = picturePath;
            this.ListFrom = listFrom;
            this.ListDataType = listDataType;
            this.ID = ID;
            this.Songs = songs == null ? new() : songs;
            this.ListCount = songs == null ? 0 : songs.Count;
            MD5 = Helpers.CodeHelper.ToMD5($"{listShowName}{listName}{picturePath}{listFrom}{listDataType}{ID}{ListCount}");
            ListDataType = listDataType;
        }

        public void ReMD5()
        {
            MD5 = Helpers.CodeHelper.ToMD5($"{ListShowName}{ListName}{PicturePath}{ListFrom}{ListDataType}{ID}{ListCount}");
        }
    }

    public class LyricData
    {
        public string Lyric { get; set; }
        public string Translate { get; set; }
        public string LyricAndTranslate { get; set; }
        public TimeSpan LyricTimeSpan { get; set; }
        public bool InSelected { get; set; }
        public string MD5 { get; set; }
        public LyricData(string lyric, string translate, TimeSpan timeSpan)
        {
            Lyric = lyric;
            if (translate != null)
            {
                LyricAndTranslate = lyric + "\n" + translate;
            }
            else
            {
                LyricAndTranslate = lyric;
            }
            LyricTimeSpan = timeSpan;
            MD5 = Helpers.CodeHelper.ToMD5($"{Lyric}{Translate}{LyricTimeSpan.Ticks}");
        }
    }

    public class Music
    {
        public static List<string> GetArtistStrings(List<Artist> artists)
        {
            List<string> a = new();
            foreach (Artist artist in artists)
            {
                a.Add(artist.ToString());
            }
            return a;
        }

        public static MusicFrom MusicFromFromString(string text)
        {
            MusicFrom musicFrom = MusicFrom.localMusic;

            try
            {
                switch (text)
                {
                    case "kwMusic":
                        musicFrom = MusicFrom.kwMusic;
                        break;
                    case "neteaseMusic":
                        musicFrom = MusicFrom.neteaseMusic;
                        break;
                    case "kgMusic":
                        musicFrom = MusicFrom.kgMusic;
                        break;
                    case "qqMusic":
                        musicFrom = MusicFrom.qqMusic;
                        break;
                    case "miguMusic":
                        musicFrom = MusicFrom.miguMusic;
                        break;
                    case "localMusic":
                        musicFrom = MusicFrom.localMusic;
                        break;
                    default:
                        musicFrom = MusicFrom.otherMusic;
                        break;
                }
            }
            catch
            {
                musicFrom = MusicFrom.kwMusic;
            }

            return musicFrom;
        }
    }
}
