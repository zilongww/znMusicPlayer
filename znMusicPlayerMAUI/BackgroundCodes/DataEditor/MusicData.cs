﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace znMusicPlayerMAUI.BackgroundCodes.DataEditor
{
    public enum MusicFrom { kwMusic, kgMusic, qqMusic, neteaseMusic, miguMusic, localMusic, otherMusic }

    public enum DataType { Search, PlayList, Album, User, Artist }
    public enum MusicKbps { aac, wma, Kbps128, Kbps192, Kbps320, Kbps1000 }

    public struct Artist
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public string PicturePath { get; set; }

        public Artist(string name, string ID, string picturePath) : this()
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
        public string MD5 { get; set; }

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
            Title = title;
            this.ID = ID;
            Artists = artists == null ? new List<Artist>() { new Artist() { Name = "Unknow" } } : artists;
            Album = album;
            AlbumID = albumID;
            PicturePath = picturePath;
            RelaseTime = relaseTime;
            From = from;
            this.Kbps = Kbps;
            InLocal = inLocal;

            for (int i = 0; i < Artists.Count; i++)
            {
                ButtonName += $"{Artists[i].ToString()}{(i < Artists.Count - 1 ? i < Artists.Count - 2 ? ", " : " & " : " · ")}";
            }

            ButtonName += Album;

            MD5 = Helpers.CodeHelper.ToMD5($"{Title}{Artists[0]}{Artists.Count}{Album}{this.ID}{AlbumID}{From}{InLocal}");
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

        public bool Equals(MusicData other)
        {
            return string.Equals(MD5, other.MD5, StringComparison.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return MD5 != null ? StringComparer.InvariantCulture.GetHashCode(MD5) : 0;
        }
    }

    public class MusicListData
    {
        public string ListName { get; set; }
        public string ListShowName { get; set; }
        public string PicturePath { get; set; }
        public MusicFrom ListFrom { get; set; }
        public string ID { get; set; }
        public int ListCount { get; set; }
        public string MD5 { get; set; }
        public List<MusicData> Songs { get; set; }

        public MusicListData(string listName = null, string listShowName = null, string picturePath = null, MusicFrom? listFrom = null, string ID = null, List<MusicData> songs = null)
        {
            ListName = listName;
            ListShowName = listShowName;
            PicturePath = picturePath;
            ListFrom = listFrom == null ? new MusicFrom() : (MusicFrom)listFrom;
            this.ID = ID;
            Songs = songs == null ? new() : songs;
            ListCount = songs == null ? 0 : songs.Count;
            MD5 = Helpers.CodeHelper.ToMD5($"{listName}{picturePath}{listFrom}{ID}{ListCount}");
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
