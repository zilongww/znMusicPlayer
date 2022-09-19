using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using znMusicPlayerWPF.Network;

namespace znMusicPlayerWPF.MusicPlay
{
    class MusicUserListGet
    {
        /// <summary>
        /// 获取歌单数据
        /// </summary>
        public static async Task<Tuple<string, TheMusicDatas.MusicListData, string>> UserListGet(TheMusicDatas.MusicFrom musicFrom, string listId)
        {
            string searchState = "200";

            string imagePath = null;
            string name = null;
            string userName = null;
            List<TheMusicDatas.MusicData> musicDatas = new List<TheMusicDatas.MusicData>();

            JObject datas = null;

            switch (musicFrom)
            {
                case TheMusicDatas.MusicFrom.kwMusic:
                    // 开始搜索
                    string searchResult = await kwMusicUserListDataGet(listId);

                    try
                    {
                        datas = searchResult == null ? null : JObject.Parse(searchResult);
                    }
                    catch
                    {
                        datas = null;
                    }

                    // 搜索失败 code = -1，成功 code = 200
                    // 搜索失败检查
                    if (datas == null)
                    {
                        searchState = "搜索失败，请重试。";
                        break;
                    }

                    // 搜索无结果检查
                    string codeState = datas["code"].ToString();
                    if (codeState == "-1")
                    {
                        searchState = "搜索无结果，请检查输入id是否正确后重试。";
                        break;
                    }

                    imagePath = datas["data"]["img"].ToString();
                    name = datas["data"]["name"].ToString();
                    userName = datas["data"]["userName"].ToString();

                    //searchState = datas["data"].ToString();

                    foreach (var song in datas["data"]["musicList"])
                    {
                        string pic = null;

                        try
                        {
                            pic = song["pic"].ToString();
                        }
                        catch
                        {
                            pic = null;
                        }

                        musicDatas.Add(
                            new TheMusicDatas.MusicData(
                                song["name"].ToString(), song["rid"].ToString(), song["artist"].ToString(), song["album"].ToString(),
                                song["albumid"].ToString(), pic, song["releaseDate"].ToString(), TheFrom: musicFrom
                                )
                            );
                    }

                    break;
                case TheMusicDatas.MusicFrom.kgMusic:
                    break;
                case TheMusicDatas.MusicFrom.neteaseMusic:


                    var tuple = await neteaseMusicUserListDataGet(listId);

                    searchResult = tuple.Item1;

                    try
                    {
                        JObject searchData = JObject.Parse(searchResult);

                        if (searchData["code"].ToString() != "200")
                        {
                            searchState = searchData["msg"].ToString();
                            if (searchState == "no resource") searchState = "搜索无结果，请检查输入id是否正确后重试。";

                            searchState = $"{searchData["code"]}：{searchState}";
                            break;
                        }

                        imagePath = searchData["result"]["coverImgUrl"].ToString();
                        name = searchData["result"]["name"].ToString();
                        userName = searchData["result"]["description"].ToString();

                        List<JObject> songList = new List<JObject>();
                        foreach (var i in tuple.Item2.Replace("[", "").Replace("]", "").Split("}".ToCharArray()))
                        {
                            string jname = i.Replace(",{", "{") + "}";

                            if (jname != "}")
                                songList.Add(
                                    JObject.Parse(jname)
                                    );
                        }

                        foreach (var song in songList)
                        {
                            JObject songData = null;
                            JObject songCode = null;

                            string id = zilongcn.Others.StringBetween(song["url"].ToString(), "&id=", "&auth=");

                            while (songData == null)
                            {
                                await Task.Delay(740);

                                try
                                {
                                    songCode = JObject.Parse(
                                        await Task.Run(() =>
                                        {
                                            znWebClient znWebClient = new znWebClient();
                                            return znWebClient.GetString($"http://music.163.com/api/song/detail/?id={id}&ids=%5B{id}%5D");
                                        })
                                    ) as JObject;

                                    if (songCode["code"].ToString() != "200")
                                    {
                                        await Task.Delay(21000);
                                        continue;
                                    }

                                    songData = songCode["songs"][0] as JObject;
                                }
                                catch { songData = null; }
                            }

                            if (songData != null)
                            {
                                musicDatas.Add(
                                    new TheMusicDatas.MusicData(
                                        song["title"].ToString(),
                                        id,
                                        song["author"].ToString(),
                                        songData["album"]["name"].ToString(),
                                        songData["album"]["id"].ToString(),
                                        songData["album"]["picUrl"].ToString(),
                                        TheFrom: TheMusicDatas.MusicFrom.neteaseMusic
                                        )
                                    );
                                await Task.Delay(5);
                            }
                        }
                    }
                    catch
                    {
                        searchState = "搜索失败，请重试。";
                    }

                    break;
            }

            return
                new Tuple<string, TheMusicDatas.MusicListData, string>(
                    searchState,
                    new TheMusicDatas.MusicListData(listId, name, imagePath, musicFrom, listId, musicDatas),
                    userName
                    );
        }

        public static async Task<string> kwMusicUserListDataGet(string listId)
        {
            return await Task.Run(() =>
            {
                string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.81 Safari/537.36 Edg/94.0.992.50";

                WebClient wb = new WebClient() { Encoding = Encoding.UTF8 };
                wb.Headers.Add("User-Agent", UserAgent);
                wb.Headers.Add("Referer", "http://www.kuwo.cn");
                wb.Headers.Add("csrf", "NNJM2N97ZSO");
                wb.Headers.Add("Cookie", "_ga=GA1.2.1353631698.1609254654; Hm_lvt_cdb524f42f0ce19b169a8071123a4797=1631341067,1632124174,1633520231,1633847365; kw_token=NNJM2N97ZSO");

                return new znWebClient().GetString($"http://www.kuwo.cn/api/www/playlist/playListInfo?pid={listId}&pn=1&rn=300&httpsStatus=1&reqId=b3dfee30-34a8-11ec-ba02-cf765c77e634");
            });
        }

        public static async Task<Tuple<string, string>> neteaseMusicUserListDataGet(string listId)
        {
            return await Task.Run(() =>
            {
                znWebClient webClient = new znWebClient();
                string result1 = webClient.GetString($"https://music.163.com/api/playlist/detail?id={listId}");
                string result2 = webClient.GetString($"https://api.i-meto.com/meting/api?server=netease&type=playlist&id={listId}");
                if (result1 == null) result1 = "";

                return new Tuple<string, string>(result1, result2);
            });
        }
    }
}
