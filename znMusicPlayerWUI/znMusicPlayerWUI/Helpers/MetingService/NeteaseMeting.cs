using NAudio.Gui;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Helpers.MetingService
{
    public class NeteaseMeting: IMetingService
    {
        public Meting4Net.Core.Meting Services { get; set; }
        public NeteaseMeting(Meting4Net.Core.Meting meting)
        {
            Services = meting;
        }

        public async Task<string> GetUrl(string id, int br)
        {
            return await Task.Run(() =>
            {
                var getAddressAction = string () =>
                {
                    string address = Services.FormatMethod().Url(id, br);
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

                for (int i = 0; i <= App.metingServices.RetryCount; i++)
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
        public async Task<Tuple<string, string>> GetLyric(string id)
        {
            return await Task.Run(() =>
            {
                var getLyricAction = Tuple<string, string> () =>
                {
                    string lyric = Services.FormatMethod().Lyric(id);

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

                for (int i = 0; i <= App.metingServices.RetryCount; i++)
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

        public async Task<Tuple<string, string>> GetPic(string id)
        {
            return await Task.Run(() =>
            {
                var getPicAction = Tuple<string, string> () =>
                {
                    string pic = Services.FormatMethod().Pic(id, 5000);

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

                for (int i = 0; i <= App.metingServices.RetryCount; i++)
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
        
        public async Task<MusicListData> GetSearch(
            string keyword,
            int pageNumber = 1,
            int pageSize = 30,
            SearchDataType type = default)
        {
            return await Task.Run(() =>
            {
                var getSearchAction = MusicListData () =>
                {
                    string data = Services.FormatMethod(false).Search(keyword, new Meting4Net.Core.Models.Standard.Options() { page = pageNumber, limit = pageSize, type = 1/*(int)type*/ });

                    if (data != null)
                    {
                        var a = JObject.Parse(data);
                        if (a.ContainsKey("result"))
                        {
                            MusicListData ld = new(keyword, keyword, null, MusicFrom.neteaseMusic, null, null);
                            ld.Songs = new();

                            foreach (var song in a["result"]["songs"])
                            {
                                List<Artist> artists = null;
                                // 添加歌手
                                if (song["ar"] != null)
                                {
                                    artists = new();
                                    foreach (var artist in song["ar"])
                                    {
                                        artists.Add(new(
                                            (string)artist["name"],
                                            (string)artist["id"],
                                            null
                                            ));
                                    }
                                }

                                // 初始化歌曲信息
                                ld.Songs.Add(new(
                                    (string)song["name"], (string)song["id"],
                                    artists,
                                    (string)song["al"]["name"], (string)song["al"]["id"],
                                    (string)song["al"]["picUrl"],
                                    (string)song["publishTime"],
                                    MusicFrom.neteaseMusic
                                    ));
                            }

                            return ld;
                        }
                    }

                    return null;
                };

                for (int i = 0; i <= App.metingServices.RetryCount; i++)
                {
                    var a = getSearchAction();
                    if (a != null)
                    {
                        return a;
                    }
                }

                return null;
            });
        }

        public List<MusicData> UnpackMusicData(JToken token)
        {
            var datas = new List<MusicData>();
            foreach (var md in token)
            {
                List<Artist> artists = new();
                foreach (var artist in md["ar"])
                {
                    artists.Add(new(
                        (string)artist["name"],
                        (string)artist["id"], null
                        ));
                }
                datas.Add(new(
                    (string)md["name"],
                    (string)md["id"],
                    artists,
                    (string)md["al"]["name"],
                    (string)md["al"]["id"],
                    (string)md["al"]["picUrl"],
                    (string)md["publishTime"],
                    MusicFrom.neteaseMusic// from
                    ));
            }
            return datas;
        }

        public async Task<MusicListData> GetPlayList(string id)
        {
            return await Task.Run(() =>
            {
                var getPlayListAction = MusicListData () =>
                {
                    JObject pls = JObject.Parse(Services.FormatMethod(false).Playlist(id));
                    //System.Diagnostics.Debug.WriteLine(pls);

                    if (pls["code"].ToString() == "200")
                    {
                        MusicListData musicListData = new();

                        var pld = pls["playlist"];
                        musicListData.ListShowName = (string)pld["name"];
                        musicListData.ID = (string)pld["id"];
                        musicListData.PicturePath = (string)pld["coverImgUrl"];
                        musicListData.ListFrom = MusicFrom.neteaseMusic;
                        musicListData.ListDataType = DataType.歌单;

                        var plt = pld["tracks"];
                        musicListData.Songs = UnpackMusicData(plt);

                        musicListData.ListName = $"{musicListData.ListShowName}{musicListData.ListFrom}{musicListData.PicturePath}";
                        musicListData.ReMD5();

                        //System.Diagnostics.Debug.WriteLine(musicListData.MD5);
                        if (musicListData.Songs.Any()) return musicListData;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(pls["message"]);
                    }

                    return null;
                };

                for (int i = 0; i <= App.metingServices.RetryCount; i++)
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

        public async Task<Artist> GetArtist(string id)
        {
            return await Task.Run(() =>
            {
                var getArtistAction = Artist () =>
                {
                    var data = JObject.Parse(Services.FormatMethod(false).Artist(id));
                    //System.Diagnostics.Debug.WriteLine(data);
                    Artist artist = default;
                    if (data["code"].ToString() == "200")
                    {
                        var art = data["artist"];
                        artist.Name = (string)art["name"];
                        artist.ID = (string)art["id"];
                        artist.PicturePath = (string)art["img1v1Url"];
                        artist.Describee = (string)art["briefDesc"];

                        artist.HotSongs = new()
                        {
                            ListFrom = MusicFrom.neteaseMusic,
                            ListDataType = DataType.艺术家,
                            Songs = UnpackMusicData(data["hotSongs"]),
                            PicturePath = artist.PicturePath
                        };
                    }

                    return artist;
                };

                for (int i = 0; i <= App.metingServices.RetryCount; i++)
                {
                    var a = getArtistAction();
                    if (!string.IsNullOrEmpty(a.ID))
                        return a;
                }

                return default;
            });
        }
    }
}
