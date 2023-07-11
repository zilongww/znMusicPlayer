﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Meting4Net.Core.Models.Tencent;
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
    public enum PlaySort { 默认升序, 默认降序, 名称升序, 名称降序, 艺术家升序, 艺术家降序, 专辑升序, 专辑降序, 时间升序, 时间降序, 索引升序, 索引降序 }

    public abstract class OnlyClass
    {
        string md5;
        public string MD5
        {
            get
            {
                if (md5 == null)
                    md5 = GetMD5();
                return md5;
            }
        }

        public abstract string GetMD5();

        public static bool operator ==(OnlyClass left, OnlyClass right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.MD5 == right.MD5;
        }

        public static bool operator !=(OnlyClass left, OnlyClass right)
        {
            if (left is null && right is null) return false;
            return !(left == right);
        }

        public override bool Equals(object other)
        {
            if (!(other is OnlyClass)) return false;
            return string.Equals(MD5, (other as OnlyClass).MD5, StringComparison.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return (MD5 != null ? StringComparer.InvariantCulture.GetHashCode(MD5) : 0);
        }
    }

    public class CUETrackData
    {
        public string Path { get; set; } = null;
        public int Index { get; set; } = 0;
        public TimeSpan Duration
        {
            get => EndDuration - StartDuration;
        }
        public TimeSpan StartDuration { get; set; } = default;
        public TimeSpan EndDuration { get; set; } = default;
    }

    public class Artist
    {
        public string Name { get; set; }
        public string Name2 { get; set; }
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

    public class Album
    {
        public string Title { get; set; }
        public string ID { get; set; }
        public string PicturePath { get; set; }
        public string Describee { get; set; }
        public string RelaseTime { get; set; }
        public List<Artist> Artists { get; set; }
        public MusicListData Songs { get; set; }
        public Album(string title = null, string ID = null, string picturePath = null, string describee = null, MusicListData songs = null)
        {
            Title = title;
            this.ID = ID == "0" || string.IsNullOrEmpty(ID) ? null : ID;
            PicturePath = picturePath;
            Describee = describee;
            Songs = songs;
        }

        public bool IsNull()
        {
            return string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(ID);
        }

        public override string ToString()
        {
            return Title;
        }
    }

    public class MusicData : OnlyClass
    {
        public string Title { get; set; }
        public string Title2 { get; set; }
        public string ID { get; set; }
        public List<Artist> Artists { get; set; }
        public Album Album { get; set; }
        public string RelaseTime { get; set; }
        public MusicFrom From { get; set; }
        public string InLocal { get; set; }
        public CUETrackData CUETrackData { get; set; } = null;
        public int Index { get; set; } = 0;

        string _artistName = null;
        public string ArtistName
        {
            get
            {
                if (_artistName == null)
                    SetABName();
                return _artistName;
            }
        }

        string _buttonName = null;
        public string ButtonName
        {
            get
            {
                if (_buttonName == null)
                {
                    SetABName();
                }
                return _buttonName;
            }
        }

        public MusicData(string title = "",
                         string ID = "",
                         List<Artist> artists = null,
                         Album album = null,
                         string relaseTime = null,
                         MusicFrom from = MusicFrom.kwMusic,
                         string inLocal = null)
        {
            this.Title = title;
            this.ID = ID;
            this.Artists = artists == null ? new List<Artist>() { new Artist() { Name = "未知" } } : artists;
            this.Album = album;
            this.RelaseTime = relaseTime;
            this.From = from;
            this.InLocal = inLocal;

        }

        private void SetABName()
        {
            for (int i = 0; i < (Artists.Count); i++)
            {
                _artistName += $"{Artists[i].ToString()}{(i < (Artists.Count - 1) ? (i < Artists.Count - 2 ? ", " : " & ") : "")}";
            }

            _buttonName = $"{ArtistName} · {Album}";
        }

        public override string GetMD5()
        {
            return CodeHelper.ToMD5($"{Title}{Artists[0].Name}{Artists[0].ID}{Artists.Count}{Album?.Title}{ID}{Album?.ID}{From}{InLocal}{(CUETrackData != null ? $"{CUETrackData.StartDuration}{CUETrackData.EndDuration}" : "")}");
        }

        public override string ToString()
        {
            return $"{Title} - {ButtonName}";
        }
    }

    public class MusicListData : OnlyClass
    {
        public string ListName { get; set; }
        public string ListShowName { get; set; }
        public string PicturePath { get; set; }
        public MusicFrom ListFrom { get; set; }
        public DataType ListDataType { get; set; }
        public string ID { get; set; }
        public int ListCount { get; set; }
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
            ListDataType = listDataType;
        }

        public override string GetMD5()
        {
            return CodeHelper.ToMD5($"{ListShowName}{ListName}{PicturePath}{ListFrom}{ListDataType}{ID}{ListCount}");
        }
    }

    public class LyricData : OnlyClass
    {
        public List<string> Lyric { get; set; }
        public TimeSpan LyricTimeSpan { get; set; }
        string lyricAllString = null;
        public string LyricAllString
        {
            get
            {
                if (lyricAllString == null && Lyric != null)
                {
                    lyricAllString = string.Join("\n", Lyric);
                }
                return lyricAllString;
            }
        }
        public LyricData(List<string> lyric, string translate, TimeSpan timeSpan)
        {
            Lyric = lyric;
            LyricTimeSpan = timeSpan;
        }

        public override string GetMD5()
        {
            if (Lyric == null) return null;
            return CodeHelper.ToMD5($"{string.Join<string>(' ', Lyric)}{Lyric.Count}{LyricTimeSpan.Ticks}");
        }
    }

    public static class Music
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

        public static MusicFrom MusicFromFromString(this string text)
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
