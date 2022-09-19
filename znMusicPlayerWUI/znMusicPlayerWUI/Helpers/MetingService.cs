using Meting4Net.Core.Models.Netease;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http.Diagnostics;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Helpers
{
    public static class MetingService
    {
        public static string NeteaseCookie = null;
        public static Meting4Net.Core.Meting NeteaseMeting = new(Meting4Net.Core.ServerProvider.Netease);

        public static void InitMeting()
        {
            NeteaseMeting.Cookie("os=pc; " + NeteaseCookie);
        }

        public static async Task<string> GetUrl(string id, int br)
        {
            return await Task.Run(() =>
            {
                var getAddressAction = string () =>
                {
                    string address = NeteaseMeting.FormatMethod().Url(id, br);
                    System.Diagnostics.Debug.WriteLine(address);

                    if (address != null)
                    {
                        var a = JObject.Parse(address);
                        if (a.ContainsKey("url"))
                        {
                            if (a["url"].ToString() != "")
                            {
                                address = a["url"].ToString();
                                return address;
                            }
                        }
                    }

                    return null;
                };

                for (int i = 0; i <= 15; i++)
                {
                    var a = getAddressAction();
                    if (a != null)
                    {
                        return a;
                    }
                }

                return null;
            });
        }

        /// <summary>
        /// 获取网易云音乐歌词
        /// </summary>
        /// <param name="id">歌曲id</param>
        /// <returns>
        /// item1歌词
        /// item2歌词翻译
        /// </returns>
        public static async Task<Tuple<string, string>> GetLyric(string id)
        {
            return await Task.Run(() =>
            {
                var getLyricAction = Tuple<string, string> () =>
                {
                    string lyric = NeteaseMeting.FormatMethod().Lyric(id);

                    if (lyric != null)
                    {
                        var a = JObject.Parse(lyric);
                        if (a.ContainsKey("lyric"))
                        {
                            var l = (string)a["lyric"];
                            var t = (string)a["tlyric"];
                            return new Tuple<string, string>(l, t);
                        }
                    }

                    return null;
                };

                for (int i = 0; i <= 15; i++)
                {
                    var a = getLyricAction();
                    if (a != null)
                    {
                        return a;
                    }
                }

                return null;
            });
        }
        
        public static async Task<Tuple<string, string>> GetPic(string id)
        {
            return await Task.Run(() =>
            {
                var getPicAction = Tuple<string, string> () =>
                {
                    string pic = NeteaseMeting.FormatMethod().Pic(id, 5000);

                    if (pic != null)
                    {
                        var a = JObject.Parse(pic);
                        if (a.ContainsKey("lyric"))
                        {
                            var l = (string)a["lyric"];
                            var t = (string)a["tlyric"];
                            return new Tuple<string, string>(l, t);
                        }
                    }

                    return null;
                };

                for (int i = 0; i <= 15; i++)
                {
                    var a = getPicAction();
                    if (a != null)
                    {
                        return a;
                    }
                }

                return null;
            });
        }

        public static async Task<MusicListData> GetPlayList(string id)
        {
            return await Task.Run(() =>
            {
                var getPlayListAction = MusicListData () =>
                {
                    JObject pls = JObject.Parse(NeteaseMeting.FormatMethod(false).Playlist(id));

                    if (pls["code"].ToString() == "200")
                    {
                        MusicListData musicListData = new();

                        var pld = pls["playlist"];
                        musicListData.ListShowName = (string)pld["name"];
                        musicListData.ID = (string)pld["id"];
                        musicListData.PicturePath = (string)pld["coverImgUrl"];
                        musicListData.ListFrom = MusicFrom.neteaseMusic;
                        musicListData.ListDataType = DataType.PlayList;

                        var plt = pld["tracks"];
                        foreach (var md in plt)
                        {
                            List<Artist> artists = new();
                            foreach (var artist in md["ar"])
                            {
                                artists.Add(new(
                                    (string)artist["name"],
                                    (string)artist["id"], null
                                    ));
                            }
                            musicListData.Songs.Add(new(
                                (string)md["name"],
                                (string)md["id"],
                                artists,
                                (string)md["al"]["name"],
                                (string)md["al"]["id"],
                                (string)md["al"]["picUrl"],
                                (string)md["publishTime"],
                                MusicFrom.neteaseMusic
                                ));
                        }

                        musicListData.ListName = $"{musicListData.ListShowName}{musicListData.ListFrom}{musicListData.PicturePath}";
                        musicListData.ReMD5();

                        System.Diagnostics.Debug.WriteLine(musicListData.MD5);
                        if (musicListData.Songs.Any()) return musicListData;
                    }

                    return null;
                };

                for (int i = 0; i <= 15; i++)
                {
                    var a = getPlayListAction();
                    if (a != null)
                    {
                        return a;
                    }
                }

                return null;
            });
        }
    }
}
