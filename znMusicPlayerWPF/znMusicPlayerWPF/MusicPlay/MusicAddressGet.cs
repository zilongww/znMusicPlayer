using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using znMusicPlayerWPF.Network;

namespace znMusicPlayerWPF.MusicPlay
{
    static class MusicAddressGet
    {
        #region 歌曲地址获取
        public static async Task<Tuple<string, string>> kwMusicAddressGet(TheMusicDatas.MusicData musicData)
        {
            string Names = "1000kape";
            string LastName = ".mp3";
            TheMusicDatas.MusicKbps Kbps = TheMusicDatas.MusicKbps.Kbps1000;

            if (musicData.Kbps == TheMusicDatas.MusicKbps.Kbps320) { Names = "320kmp3"; Kbps = TheMusicDatas.MusicKbps.Kbps320; }
            if (musicData.Kbps == TheMusicDatas.MusicKbps.Kbps192) { Names = "192kmp3"; Kbps = TheMusicDatas.MusicKbps.Kbps192; }
            if (musicData.Kbps == TheMusicDatas.MusicKbps.Kbps128) { Names = "128kmp3"; Kbps = TheMusicDatas.MusicKbps.Kbps128; }
            if (musicData.Kbps == TheMusicDatas.MusicKbps.wma) { Names = "96kwma"; Kbps = TheMusicDatas.MusicKbps.wma; }
            if (musicData.Kbps == TheMusicDatas.MusicKbps.aac) { Names = "48kaac"; Kbps = TheMusicDatas.MusicKbps.aac; }

            string TheWeb = "";

            await Task.Run(() =>
            {
                //TheWeb = new znWebClient().GetString("http://www.kuwo.cn/url?type=convert_url&br=" + Names + "&rid=" + musicData.SongRid);

                try
                {
                    TheWeb = new znWebClient().GetString($"http://www.kuwo.cn/api/v1/www/music/playUrl?mid={musicData.SongRid}&type=music&httpsStatus=1&reqId=23f96a61-38bf-11ec-a92b-3f68ddb00bd8");
                    if (JObject.Parse(TheWeb)["msg"].ToString().Contains("付费"))
                    {
                        TheWeb = "pay";
                    }
                    else
                    {
                        TheWeb = JObject.Parse(TheWeb)["data"]["url"].ToString();
                    }
                }
                catch
                {
                    TheWeb = "error";
                }
                /*
                if (TheWeb.Split('.').Last() == "res not found")
                {
                    TheWeb = new znWebClient().GetString("http://www.kuwo.cn/url?type=convert_url&br=320kmp3&rid=" + musicData.SongRid);
                    Kbps = TheMusicDatas.MusicKbps.Kbps320;
                }
                if (TheWeb.Split('.').Last() == "res not found")
                {
                    TheWeb = new znWebClient().GetString("http://www.kuwo.cn/url?type=convert_url&br=192kmp3&rid=" + musicData.SongRid);
                    Kbps = TheMusicDatas.MusicKbps.Kbps192;
                }
                if (TheWeb.Split('.').Last() == "res not found")
                {
                    TheWeb = new znWebClient().GetString("http://www.kuwo.cn/url?type=convert_url&br=128kmp3&rid=" + musicData.SongRid);
                    Kbps = TheMusicDatas.MusicKbps.Kbps128;
                }
                if (TheWeb.Split('.').Last() == "res not found")
                {
                    TheWeb = new znWebClient().GetString("http://www.kuwo.cn/url?format=wma&type=convert_url&br=128kwma&rid=" + musicData.SongRid);
                    Kbps = TheMusicDatas.MusicKbps.wma;
                    LastName = ".wma";
                }
                if (TheWeb.Split('.').Last() == "res not found")
                {
                    TheWeb = new znWebClient().GetString("http://www.kuwo.cn/url?format=aac&type=convert_url&br=48kaac&rid=" + musicData.SongRid);
                    Kbps = TheMusicDatas.MusicKbps.aac;
                    LastName = ".aac";
                }*/
            });

            return new Tuple<string, string>(TheWeb, LastName);
        }

        public static async Task<Tuple<string, JObject>> kgMusicAddressGet(TheMusicDatas.MusicData musicData)
        {
            string MusicUrl = $"https://wwwapi.kugou.com/yy/index.php?r=play/getdata&hash={musicData.SongRid}&album_id={musicData.AlbumID}";
            JObject MusicDatas = null;

            await Task.Run(() =>
            {
                WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 };
                webClient.Headers.Add("cookie", "kg_mid=c1470a3e02dbaa42f2d0f9ee79320cf0; kg_dfid=4WPBwo1Rm8lo3XLx8X0i4I0b; kg_dfid_collect=d41d8cd98f00b204e9800998ecf8427e; Hm_lvt_aedee6983d4cfc62f509");
                string a = webClient.DownloadString(MusicUrl);

                JObject MusicData = JObject.Parse(a);
                var data = MusicData["data"];

                MusicUrl = new znWebClient().GetString($"https://api.i-meto.com/meting/api?server=kugou&type=song&id={musicData.SongRid}").Replace("[", "").Replace("]", "");

                try
                {
                    MusicUrl = JObject.Parse(MusicUrl)["url"].ToString();
                }
                catch (Exception err)
                {
                    MusicUrl = "ERROR | " + err.Message.ToString();
                }

                MusicDatas = MusicData;
                webClient.Dispose();
            });

            return new Tuple<string, JObject>(MusicUrl, MusicDatas);
        }

        public static async Task<Tuple<string, TheMusicDatas.MusicData>> neteaseMusicAddressGet(TheMusicDatas.MusicData musicData)
        {
            string MusicUrl = null;

            MusicUrl = $"https://api.injahow.cn/meting/?server=netease&type=song&id={musicData.SongRid}";

            if (musicData.AlbumID != null)
            {
                try
                {
                    var MusicPic = await GetNeteasePic(musicData.SongRid);
                    musicData.PicUri = MusicPic;
                }
                catch { }
            }

            await Task.Run(() =>
            {
                try
                {
                    MusicUrl = new znWebClient().GetString(MusicUrl);
                    MusicUrl = JObject.Parse(MusicUrl.Replace("[", "").Replace("]", ""))["url"].ToString();
                }
                catch (Exception err)
                {
                    MusicUrl = "ERROR | " + err.Message.ToString();
                }
            });

            return new Tuple<string, TheMusicDatas.MusicData>(MusicUrl, musicData);

        }

        public static async Task<string> qqMusicAddressGet(TheMusicDatas.MusicData musicData)
        {
            string web = $"https://api.injahow.cn/meting/?type=song&id={musicData.SongRid}&server=tencent";
            string result = await Task.Run(() =>
            {
                return new WebClient().DownloadString(web);
            });

            return JObject.Parse(result.Remove(0, 1).Replace("]", ""))["url"].ToString();
        }

        public static async Task<string> miguMusicAddressGet(TheMusicDatas.MusicData musicData)
        {
            //return musicData.miguSongPath;
            return $"http://app.pd.nf.migu.cn/MIGUM2.0/v1.0/content/sub/listenSong.do?toneFlag=SQ&netType=00&userId=15548614588710179085069&ua=Android_migu&version=5.1&copyrightId=0&contentId={musicData.SongRid}&resourceType=E&channel=0";
        }
        #endregion

        #region 其它
        public static async Task<string> GetNeteasePic(string MusicId)
        {
            string UseAgent = UserAgents.UserAgent[new Random().Next(0, 3)];

            string Urls = $"http://music.163.com/api/song/detail/?id={MusicId}&ids=%5B{MusicId}%5D";
            WebClient wb = new WebClient() { Encoding = Encoding.UTF8 };

            string result = null;

            try
            {
                await Task.Run(() => { result = wb.DownloadString(Urls); });
                result = JObject.Parse(result)["songs"][0]["album"]["picUrl"].ToString();
            }
            catch { result = null; }
            wb.Dispose();

            return result;
        }

        public static string GetNeteaseMusicLrc(string MusicId)
        {
            try
            {
                return JObject.Parse(new znWebClient().GetString($"http://music.163.com/api/song/lyric?id={MusicId}&lv=1"))["lrc"]["lyric"].ToString();
            }
            catch
            {
                return null;
            }
        }

        public static string GetQqMusicLrc(string MusicId)
        {
            try
            {
                System.Net.WebClient webClient = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 };
                webClient.Headers.Add("Referer", "https://y.qq.com/portal/player.html");
                string lrc = webClient.DownloadString($"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid={MusicId}&format=json&nobase64=1");
                webClient.Dispose();
                return JObject.Parse(lrc)["lyric"].ToString();
            }
            catch
            {
                return null;
            }
        }

        public static string GetMiguMusicLrc(TheMusicDatas.MusicData musicData)
        {
            string Result;
            WebClient webClient = new WebClient() { Encoding = System.Text.Encoding.UTF8 };
            webClient.Headers.Add("Referer", "http://music.migu.cn/");
            try
            {
                Result = webClient.DownloadString(musicData.miguLrc);
            }
            catch
            {
                Result = null;
            }
            webClient.Dispose();
            return Result;
        }
        #endregion
    }
}
