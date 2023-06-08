using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using znMusicPlayerWUI.DataEditor;
using System.Net.Http.Json;

namespace znMusicPlayerWUI.Helpers
{
    public static class WebHelper
    {
        #region 属性
        public static string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/102.0.5005.124 Safari/537.36 Edg/102.0.1245.44";
        public static string NeteaseSearchAddress = "http://music.163.com/api/search/get/web?s={0}&type=1&offset={1}&limit={2}&total=true";
        public static HttpClient Client = new HttpClient();
        private static bool IsRequested = false;
        #endregion

        #region 联网检测
        [DllImport("wininet.dll", EntryPoint = "InternetGetConnectedState")]
        public extern static bool InternetGetConnectedState(out int conState, int reader);
        public static bool IsNetworkConnected
        {
            get
            {
                var n = 0;
                if (!InternetGetConnectedState(out n, 0)) return false;
                return true;
            }
        }
        #endregion/// <summary>

        /// <summary>
        /// 下载文件 
        /// </summary>
        /// <param name="address"></param>
        /// <param name="downloadPath"></param>
        /// <returns></returns>
        /// <exception cref="WebException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        public static async Task DownloadFileAsync(string address, string downloadPath)
        {
            if (!IsNetworkConnected) throw new WebException("网络未连接。");

            Uri uriResult;

            if (!Uri.TryCreate(address, UriKind.Absolute, out uriResult))
                throw new InvalidOperationException("无法定位到网络地址，请检查你的域名服务器是否正常工作或DNS配置是否正确。");

            if (!File.Exists(downloadPath))
                throw new FileNotFoundException("找不到目标文件。");

            try
            {
                var downloader = new Downloader.DownloadService();
                await downloader.DownloadFileTaskAsync(address, downloadPath);
                downloader.Dispose();
            }
            catch { }
        }

        /// <summary>
        /// 获取网址返回字符串
        /// </summary>
        /// <param name="address"></param>
        /// <param name="timeOutSec"></param>
        /// <returns></returns>
        /// <exception cref="System.Net.WebException"></exception>
        public static async Task<string> GetStringAsync(string address, int timeOutSec = 7)
        {
            if (!IsNetworkConnected) throw new WebException("网络未连接。");

            return await Client.GetStringAsync(address);
        }

        static List<MusicData> loadingImages = new();
        public static async Task<string> GetPicturePathAsync(MusicData musicData)
        {
            while (loadingImages.Count > 3)
            {
                await Task.Delay(250);
            }
            loadingImages.Add(musicData);

            string addressResult = null;

            try
            {
                switch (musicData.From)
                {
                    case MusicFrom.neteaseMusic:

                        //string address = MetingService.NeteaseMeting.PicObj(musicData.AlbumID).url;

                        string address = $"http://music.163.com/api/song/detail/?id={musicData.ID}&ids=%5B{musicData.ID}%5D";

                        string result = null;
                        result = await GetStringAsync(address);
                        await Task.Run(() =>
                        {
                            if (!string.IsNullOrEmpty(result))
                            {
                                if (!result.Contains("操作频繁"))
                                {
                                    result = JObject.Parse(result)["songs"][0]["album"]["picUrl"].ToString();
                                    addressResult = result;
                                }
                            }
                        });
                        break;
                    default:
                        addressResult = musicData.PicturePath;
                        break;
                }
            }
            catch { addressResult = null; }

            loadingImages.Remove(musicData);
            return addressResult;
        }

        /// <summary>
        /// 获取音频网络地址
        /// </summary>
        /// <param name="musicData"></param>
        /// <returns></returns>
        public static async Task<string> GetAudioAddressAsync(MusicData musicData)
        {
            string returns = null;
            switch (musicData.From)
            {
                case MusicFrom.kwMusic:
                    break;
                case MusicFrom.kgMusic:
                    break;
                case MusicFrom.neteaseMusic:
                    return await App.metingServices.NeteaseServices.GetUrl(musicData.ID, 960);
                case MusicFrom.qqMusic:
                    break;
                case MusicFrom.miguMusic:
                    break;
                default:
                    return null;
            }
            return returns;
        }

        public static async Task<Tuple<string, string>> GetLyricStringAsync(MusicData musicData)
        {
            switch (musicData.From)
            {
                case MusicFrom.neteaseMusic:
                    return await App.metingServices.NeteaseServices.GetLyric(musicData.ID);
                //string a = await GetStringAsync($"https://api.injahow.cn/meting/?server=netease&type=song&id={musicData.ID}");
                default: return null;
            }
        }

        /// <summary>
        /// 搜索平台数据
        /// </summary>
        /// <param name="searchData">搜索数据</param>
        /// <param name="pageNumber">搜索页</param>
        /// <param name="pageSize">页大小</param>
        /// <param name="searchFrom">搜索平台</param>
        /// <param name="dataType">搜索数据类型</param>
        /// <returns>Class数据</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="WebException">internal not conncetd or timeout</exception>
        public static async Task<object> SearchData(
            string keyword,
            int pageNumber = 1,
            int pageSize = 30,
            MusicFrom searchFrom = MusicFrom.neteaseMusic,
            SearchDataType searchDataType = SearchDataType.歌曲)
        {
            // TODO!!!:设置ListDataType
            object listData = null;
            switch (searchFrom)
            {
                case MusicFrom.kwMusic:
                    break;

                case MusicFrom.kgMusic:
                    break;

                case MusicFrom.neteaseMusic:
                    listData = await App.metingServices.NeteaseServices.GetSearch(keyword, pageNumber, pageSize, searchDataType);
                    break;

                case MusicFrom.qqMusic:
                    break;

                case MusicFrom.miguMusic:
                    break;

                default: throw new ArgumentOutOfRangeException(nameof(searchFrom));
            }

            return listData;
        }
    }
}
