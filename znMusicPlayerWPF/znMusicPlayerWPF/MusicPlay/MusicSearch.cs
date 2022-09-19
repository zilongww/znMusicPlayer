using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using zilongcn;
using znMusicPlayerWPF.Network;
using znMusicPlayerWPF.Pages;

namespace znMusicPlayerWPF.MusicPlay
{
    class MusicSearch
    {
        MainWindow TheParent;
        public static ListPage TheObj;
        string UserAgent;
        string Name, pn, rn;

        public MusicSearch(MainWindow TheParent) => this.TheParent = TheParent;

        public async Task StartSearch(string Name, string pn, string rn, TheMusicDatas.MusicFrom musicFrom = TheMusicDatas.MusicFrom.kwMusic)
        {
            this.Name = Name;
            this.pn = pn;
            this.rn = rn;

            UserAgent = UserAgents.UserAgent[new Random().Next(0, 3)];

            if (!Source.InternetConnect())
            {
                TheParent.ShowBox("网络未连接", "网络未连接，请连接网络后重试。");
                TheParent.TheSearchPage.SearchBtn.IsEnabled = true;
                return;
            }

            TheParent.LoadingPage.loadingContent.Pause = false;
            TheParent.TheSearchPage.InPage.Content = TheParent.LoadingPage;
            TheParent.TheSearchPage.SearchBtn.IsEnabled = false;
            TheParent.TheSearchPage.BeforeBtn.IsEnabled = false;
            TheParent.TheSearchPage.NextBtn.IsEnabled = false;
            TheParent.TheSearchPage.HomeBtn.IsEnabled = false;
            TheParent.TheSearchPage.SearchPageChangerGrid.Visibility = Visibility.Visible;
            TheParent.TheSearchPage.InPage.Margin = new Thickness(0, 75, 0, 50);

            if (TheObj != null)
            {
                foreach (ItemBar itemBar in TheObj.Lists.Children)
                {
                    itemBar.Delete();
                }

                TheObj.Lists.Children.Clear();
                TheObj.Lists = null;
                TheObj = null;
            }

            switch (musicFrom)
            {
                case TheMusicDatas.MusicFrom.kwMusic:
                    await kwMusicSearch();
                    break;
                case TheMusicDatas.MusicFrom.kgMusic:
                    await kgMusicSearch();
                    break;
                case TheMusicDatas.MusicFrom.neteaseMusic:
                    await neteaseMusicSearch();
                    break;
                case TheMusicDatas.MusicFrom.qqMusic:
                    await qqMusicSearch();
                    break;
                case TheMusicDatas.MusicFrom.miguMusic:
                    await miguMusicSearch();
                    break;
                default:
                    TheParent.ShowBox("不支持此平台的搜索功能", "暂不支持此平台的搜索功能，请切换其他平台搜索。");
                    break;
            }
            TheParent.LoadingPage.loadingContent.Pause = true;
            TheParent.TheSearchPage.SearchBtn.IsEnabled = true;
            TheParent.TheSearchPage.BeforeBtn.IsEnabled = true;
            TheParent.TheSearchPage.NextBtn.IsEnabled = true;
            TheParent.TheSearchPage.HomeBtn.IsEnabled = true;
            if (TheObj != null) Animations.animateOpacity(TheObj.Lists, 0, 1, TheParent.AnimateOpacityNormalTime, IsAnimate: TheParent.Animation).Begin();
        }

        private async Task kwMusicSearch()
        {
            string TheWeb = $"http://www.kuwo.cn/api/www/search/searchMusicBykeyWord?key={Name}&pn={pn}&rn={rn}&httpsStatus=1&reqId=b41b4da0-fd3e-11eb-976e-d7c06bc07d45";
            //&httpsStatus=1&reqId=f8e4b390-6e32-11eb-a3fe-7f3e8900f88c";

            WebClient wb = new WebClient() { Encoding = Encoding.UTF8 };
            wb.Headers.Add("User-Agent", UserAgent);
            wb.Headers.Add("Referer", "http://www.kuwo.cn");
            wb.Headers.Add("csrf", "1QXNT69Z9L9");
            wb.Headers.Add("Cookie", "_ga=GA1.2.1922351370.1620396930; _gid=GA1.2.1234824139.1622823429; Hm_lvt_cdb524f42f0ce19b169a8071123a4797=1622217255,1622643315,1622823429,1622824188; Hm_lpvt_cdb524f42f0ce19b169a8071123a4797=1622826053; kw_token=1QXNT69Z9L9");

            string Result = await Task.Run(() =>
            {
                try
                {
                    return wb.DownloadString(TheWeb);
                }
                catch
                {
                    return "err";
                }
            });

            wb.Dispose();

            if (Result != null && Result != "" && Result != "err")
            {
                JObject Results = JObject.Parse(Result);

                if (Results["code"].ToString() == "200")
                {
                    await kwMusicAddCards(Results);
                    Results = null;
                    return;
                }
            }

            TheParent.ShowBox("搜索失败", "搜索无结果，请换个关键词重试。");
            TheParent.TheSearchPage.InPage.Content = null;
        }

        private async Task kwMusicAddCards(JObject TheDatas)
        {
            // 歌曲排序
            int anl = TheParent.TheSearchPage.PageCount * TheParent.TheSearchPage.PageSize - TheParent.TheSearchPage.PageSize + 1;

            // 歌曲卡片背景为灰色
            bool IsSilver = true;

            // 新建一个用于显示歌曲卡片的ListBox
            TheObj = new ListPage(TheParent, TheParent.TheSource);

            // 循环添加歌曲卡片到显示歌曲卡片的ListBox
            foreach (var data in TheDatas["data"]["list"])
            {
                string name = System.Web.HttpUtility.HtmlDecode(data["name"].ToString());
                string artist = System.Web.HttpUtility.HtmlDecode(data["artist"].ToString());
                string album = System.Web.HttpUtility.HtmlDecode(data["album"].ToString());
                string albumpic = "";
                string times = "";
                string releaseDate = "";
                try { albumpic = data["albumpic"].ToString(); }
                catch { }
                try { times = data["songTimeMinutes"].ToString(); }
                catch { }
                try { releaseDate = data["releaseDate"].ToString(); }
                catch { }

                TheMusicDatas.MusicData TheStruct = new TheMusicDatas.MusicData(name, data["rid"].ToString(), artist, album, data["albumid"].ToString(), albumpic, releaseDate, data);

                AddACard(anl.ToString(), TheStruct, albumpic, true, TheObj, IsSilver);
                IsSilver = !IsSilver;

                anl++;

                await Task.Delay(1);
            }

            TheDatas = null;

            TheParent.TheSearchPage.InPage.Content = TheObj;
            TheObj.ListPage_SizeChanged(null, null);
        }

        private async Task kgMusicSearch()
        {
            string Urls = $"http://mobilecdn.kugou.com/api/v3/search/song?format=json&keyword={Name}&page={pn}&pagesize={rn}";
            WebClient wb = new WebClient() { Encoding = Encoding.UTF8 };
            wb.Headers.Add("User-Agent", UserAgent);
            wb.Headers.Add("csrf", "1QXNT69Z9L9");
            wb.Headers.Add("Cookie", "_ga=GA1.2.1922351370.1620396930; _gid=GA1.2.1234824139.1622823429; Hm_lvt_cdb524f42f0ce19b169a8071123a4797=1622217255,1622643315,1622823429,1622824188; Hm_lpvt_cdb524f42f0ce19b169a8071123a4797=1622826053; kw_token=1QXNT69Z9L9");

            string TheHtml = null;

            await Task.Run(() =>
            {
                TheHtml = wb.DownloadString(new Uri(Urls));
            });

            wb.Dispose();

            if (TheHtml == null)
            {
                TheParent.ShowBox("搜索", "搜索失败，服务器无返回数据。");

                TheParent.TheSearchPage.InPage.Content = null;
                TheParent.TheSearchPage.SearchBtn.IsEnabled = true;
            }
            else
            {
                await kgMusicAddCards(JObject.Parse(TheHtml));
            }
        }

        private async Task kgMusicAddCards(JObject TheDatas)
        {
            var data = TheDatas["data"];
            var SearchInfo = data["info"];

            int num = TheParent.TheSearchPage.PageCount * TheParent.TheSearchPage.PageSize - TheParent.TheSearchPage.PageSize + 1;
            TheObj = new ListPage(TheParent, TheParent.TheSource);
            bool IsSilver = true;

            foreach (var TheSong in SearchInfo)
            {
                TheMusicDatas.MusicData musicData = new TheMusicDatas.MusicData(
                    TheSong["songname"].ToString(),
                    TheSong["320hash"].ToString(),
                    TheSong["singername"].ToString(),
                    TheSong["album_name"].ToString(),
                    TheSong["album_id"].ToString(),
                    TheFrom: TheMusicDatas.MusicFrom.kgMusic
                    );

                musicData.OtherData = TheSong;

                AddACard(num.ToString(), musicData, TheObj: TheObj, Silvers: IsSilver);
                IsSilver = !IsSilver;

                num++;

                await Task.Delay(1);
            }

            TheDatas = null;
            TheParent.TheSearchPage.InPage.Content = TheObj;
            TheObj.ListPage_SizeChanged(null, null);
        }

        private async Task neteaseMusicSearch()
        {
            string Urls = $"http://music.163.com/api/search/get/web?s={Name}&type=1&offset={ int.Parse(pn) * int.Parse(rn) - int.Parse(rn) }&total=true&limit={rn}";
            WebClient wb = new WebClient() { Encoding = Encoding.UTF8 };
            wb.Headers.Add("User-Agent", UserAgent);
            wb.Headers.Add("csrf", "1QXNT69Z9L9");
            wb.Headers.Add("Cookie", "_ga=GA1.2.1922351370.1620396930; _gid=GA1.2.1234824139.1622823429; Hm_lvt_cdb524f42f0ce19b169a8071123a4797=1622217255,1622643315,1622823429,1622824188; Hm_lpvt_cdb524f42f0ce19b169a8071123a4797=1622826053; kw_token=1QXNT69Z9L9");

            string TheHtml = null;

            await Task.Run(() =>
            {
                TheHtml = wb.DownloadString(new Uri(Urls));
            });

            wb.Dispose();

            if (TheHtml == null)
            {
                TheParent.ShowBox("搜索", "搜索失败，服务器无返回数据。");

                TheParent.TheSearchPage.InPage.Content = null;
                TheParent.TheSearchPage.SearchBtn.IsEnabled = true;
            }
            else
            {
                await neteaseMusicAddCards(JObject.Parse(TheHtml));
            }
        }

        private async Task neteaseMusicAddCards(JObject TheDatas)
        {
            var TheResult = TheDatas["result"];
            var TheSongCount = TheResult["songCount"];
            var TheCode = TheDatas["code"];
            var TheSongs = TheResult["songs"];

            if (TheCode.ToString() != "200" || TheSongCount.ToString() == "0")
            {
                TheParent.ShowBox("搜索", "搜索无结果，请尝试使用其它关键词搜索。\n错误代码：" + TheCode.ToString());
                TheParent.TheSearchPage.SearchBtn.IsEnabled = true;
            }
            else
            {
                int num = TheParent.TheSearchPage.PageCount * TheParent.TheSearchPage.PageSize - TheParent.TheSearchPage.PageSize + 1;
                TheObj = new ListPage(TheParent, TheParent.TheSource);
                bool IsSilver = true;

                foreach (var TheSong in TheSongs)
                {
                    string AlbumID = null;

                    try { AlbumID = TheSong["album"]["id"].ToString(); }
                    catch { AlbumID = null; }

                    TheMusicDatas.MusicData musicData = new TheMusicDatas.MusicData(
                        TheSong["name"].ToString(),
                        TheSong["id"].ToString(),
                        TheSong["artists"][0]["name"].ToString(),
                        TheSong["album"]["name"].ToString(),
                        AlbumID,
                        TheFrom: TheMusicDatas.MusicFrom.neteaseMusic
                        );

                    musicData.OtherData = TheSong;

                    AddACard(num.ToString(), musicData, TheObj: TheObj, Silvers: IsSilver);
                    IsSilver = !IsSilver;
                    num++;
                    await Task.Delay(1);
                }

                TheDatas = null;
                TheParent.TheSearchPage.InPage.Content = TheObj;
                TheObj.ListPage_SizeChanged(null, null);
            }
        }

        private async Task qqMusicSearch()
        {
            string web = $"https://c.y.qq.com/soso/fcgi-bin/client_search_cp?aggr=1&cr=1&flag_qc=0&p={pn}&n={rn}&w={Name}";

            WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 };
            string result = await Task.Run(() =>
            {
                return webClient.DownloadString(web);
            });
            webClient.Dispose();

            await qqMusicAddCards(JObject.Parse(result.Remove(0, 9).Replace("})", "}")));
        }

        private async Task qqMusicAddCards(JObject TheDatas)
        {
            string code = TheDatas["code"].ToString();
            if (code != "0")
            {
                TheParent.ShowBox("搜索", "搜索失败，请重试。\n错误代码：" + code);
                TheParent.TheSearchPage.SearchBtn.IsEnabled = true;
                return;
            }

            int num = TheParent.TheSearchPage.PageCount * TheParent.TheSearchPage.PageSize - TheParent.TheSearchPage.PageSize + 1;
            TheObj = new ListPage(TheParent, TheParent.TheSource);
            bool IsSilver = true;

            var songList = TheDatas["data"]["song"]["list"];
            foreach (var song in songList)
            {
                string albumID = null;

                try { albumID = song["albummid"].ToString(); }
                catch { albumID = null; }

                TheMusicDatas.MusicData musicData = new TheMusicDatas.MusicData(
                    song["songname"].ToString(),
                    song["songmid"].ToString(),
                    song["singer"][0]["name"].ToString(),
                    song["albumname"].ToString(),
                    albumID,
                    $"http://imgcache.qq.com/music/photo/album/17/albumpic_{song["albumid"]}_0.jpg",
                    TheFrom: TheMusicDatas.MusicFrom.qqMusic
                    );

                musicData.OtherData = song;

                AddACard(num.ToString(), musicData, TheObj: TheObj, Silvers: IsSilver);
                IsSilver = !IsSilver;
                num++;
                await Task.Delay(1);
            }

            TheDatas = null;
            TheParent.TheSearchPage.InPage.Content = TheObj;
            TheObj.ListPage_SizeChanged(null, null);
        }

        private async Task miguMusicSearch()
        {
            string web = "http://pd.musicapp.migu.cn/MIGUM3.0/v1.0/content/search_all.do?&ua=Android_migu&version=5.0.1&text=" + Name + "&pageNo=" + pn + "&pageSize=" + rn + "&searchSwitch={\"song\":1,\"album\":0,\"singer\":0,\"tagSong\":0,\"mvSong\":0,\"songlist\":0,\"bestShow\":1}";

            string result = await Task.Run(() =>
            {
                WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 };
                string a = null;
                try
                {
                    a = webClient.DownloadString(web);
                }
                catch
                {

                }

                webClient.Dispose();
                return a;
            });

            if (result != null)
                miguMusicAddCards(JObject.Parse(result));
            else
            {
                TheParent.ShowBox("搜索", "搜索失败。");
                TheParent.TheSearchPage.SearchBtn.IsEnabled = true;
            }

        }

        private void miguMusicAddCards(JObject TheDatas)
        {
            int num = TheParent.TheSearchPage.PageCount * 20 - 20 + 1;
            TheObj = new ListPage(TheParent, TheParent.TheSource);
            bool IsSilver = true;

            var songList = TheDatas["songResultData"]["result"];
            foreach (var song in songList)
            {
                string albumName = null;
                string albumID = null;
                string lyricUrl = null;
                string ftpUrl = null;

                try { albumName = song["albums"][0]["name"].ToString(); }
                catch { albumName = null; }

                try { albumID = song["albums"][0]["id"].ToString(); }
                catch { albumID = null; }

                try { lyricUrl = song["lyricUrl"].ToString(); }
                catch { lyricUrl = null; }

                try { ftpUrl = song["newRateFormats"].Last["androidUrl"].ToString(); }
                catch
                {
                    try
                    {
                        ftpUrl = song["newRateFormats"][1]["url"].ToString();
                    }
                    catch
                    {
                        try
                        {
                            ftpUrl = song["newRateFormats"].First["url"].ToString();
                        }
                        catch
                        {
                            ftpUrl = null;
                        }
                    }
                }

                TheMusicDatas.MusicData musicData = new TheMusicDatas.MusicData();

                musicData = new TheMusicDatas.MusicData(
                    song["name"].ToString(),
                    song["contentId"].ToString(),
                    song["singers"][0]["name"].ToString(),
                    albumName,
                    albumID,
                    song["imgItems"][0]["img"].ToString(),
                    song["invalidateDate"].ToString(),
                    TheFrom: TheMusicDatas.MusicFrom.miguMusic
                    )
                {
                    miguLrc = lyricUrl,
                    miguSongPath = ftpUrl,
                    miguSongCopyrightId = song["copyrightId"].ToString()
                };

                musicData.OtherData = song;

                AddACard(num.ToString(), musicData, TheObj: TheObj, Silvers: IsSilver);
                IsSilver = !IsSilver;
                num++;

                //await Task.Delay(1);
            }

            TheDatas = null;
            TheParent.TheSearchPage.InPage.Content = TheObj;
            TheObj.ListPage_SizeChanged(null, null);
        }

        private void AddACard(string TheNumber, TheMusicDatas.MusicData TheDatas, string pic = null, bool AEN = true, ListPage TheObj = null, bool Silvers = false)
        {
            ItemBar Bar = new ItemBar();

            Bar.Set(TheNumber.ToString(), TheDatas, pic, AEN);

            if (Silvers)
            {
                //Bar.BackAnimateGrid.Fill = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
            }

            TheParent.ListPages = TheObj;
            TheObj.Lists.Children.Add(Bar);
        }
    }
}
