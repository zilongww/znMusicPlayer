using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;

namespace znMusicPlayerWUI.DataEditor
{
    public static class DataFolderBase
    {
        /// <summary>
        /// 程序数据文件夹路径
        /// </summary>
        public static string BaseFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @$"\{App.AppName}Datas";

        /// <summary>
        /// 数据文件夹路径
        /// </summary>
        public static string UserDataFolder { get; } = BaseFolder + @"\UserData";

        /// <summary>
        /// 歌单数据文件路径
        /// </summary>
        public static string PlayListDataPath { get; } = UserDataFolder + @"\PlayList";
        
        /// <summary>
        /// 设置数据文件路径
        /// </summary>
        public static string SettingDataPath { get; } = UserDataFolder + @"\Setting";

        /// <summary>
        /// 历史记录数据文件路径
        /// </summary>
        public static string HistoryDataPath { get; } = UserDataFolder + @"\History";

        /// <summary>
        /// 缓存文件夹路径
        /// </summary>
        public static string CacheFolder { get; set; } = BaseFolder + @"\Cache";

        /// <summary>
        /// 歌曲缓存文件夹路径
        /// </summary>
        public static string AudioCacheFolder { get; set; } = CacheFolder + @"\Audio";

        /// <summary>
        /// 图片缓存文件夹路径
        /// </summary>
        public static string ImageCacheFolder { get; set; } = CacheFolder + @"\Image";

        /// <summary>
        /// 歌词缓存文件夹路径
        /// </summary>
        public static string LyricCacheFolder { get; set; } = CacheFolder + @"\Lyric";

        /// <summary>
        /// 下载文件夹路径
        /// </summary>
        public static string DownloadFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

        /// <summary>
        /// 默认播放列表数据
        /// </summary>
        public static MusicListData PlayListDefault = new("default", "default", null, MusicFrom.localMusic, null);

        /// <summary>
        /// 默认设置数据
        /// </summary>
        public static JObject SettingDefault = new JObject()
        {
            { SettingParams.Volume.ToString(), 0.5 },
            { SettingParams.DownloadFolderPath.ToString(), DownloadFolder },
            { SettingParams.AudioCacheFolderPath.ToString(), AudioCacheFolder },
            { SettingParams.ImageCacheFolderPath.ToString(), ImageCacheFolder },
            { SettingParams.LyricCacheFolderPath.ToString(), LyricCacheFolder },
            { SettingParams.EqualizerEnable.ToString(), false },
            { SettingParams.EqualizerString.ToString(), nameof(Media.AudioEqualizerBands.CustomBands) },
            { SettingParams.EqualizerCustomData.ToString(), "0,0,0,0,0,0,0,0,0,0" },
            { SettingParams.WasapiOnly.ToString(), false },
            { SettingParams.AudioLatency.ToString(), 100 },
            { SettingParams.MusicPageShowLyricPage.ToString(), true },
            { SettingParams.NeteaseMusicCookie.ToString(), "_ntes_nnid=0590a8d6b6be9cad6bc23b97c07f7cdc,1672820325011; _ntes_nuid=0590a8d6b6be9cad6bc23b97c07f7cdc; NMTID=00OjlEbhTNEzgg00U1LuFYI4ZujWywAAAGFe9x7WA; WM_TID=D1p/ipOchYdFQRFFBALAJyqesrVvtbz+; __snaker__id=piPDdtsHQOZxmZu2; YD00000558929251:WM_TID=K5mT5w0FhgtEUQQUQVaQdn6btuTcRF4w; __remember_me=true; ntes_kaola_ad=1; WM_NI=w/Y4CB/PKwDKP7UvVxmIRkaVgc/X9wm11o/14Aadh5C5EiXVDs+LvyBt4PDtx7Hn5jlaLsl92cOlH2NRpnCRndyQuocBr1hsyxWJaWKwpWBCcgWrisy5xmq1je3f6zalMUw=; WM_NIKE=9ca17ae2e6ffcda170e2e6ee8fc23495b2bcd2ec47bb9e8ba7c14a968b8badd569fbb4a5d4b43398e7fd96c82af0fea7c3b92ab5f197d5c24aed908598c74194e99b97d574af979e8ce45ea5bde1b9bc7dafefff90ae3bf89faf85eb3f81eb8b8adb5e88ade5adf57382acfcb1c780bce9888fb862b0bf8494ef33f2bd818dcb3a8e8ec08fc25aaebcb7b2f865a19ebed2eb79bba899abe45d89a9c0b8c149a1b99993ec79bbebb88ef43baabbfd87b572a3b483b6f637e2a3; JSESSIONID-WYYY=OFUX7opj5uk\\BsaU/Yj/QWhkmC6aIo3jVboQ6r31laqSA5vTYjTp8H5VE3E/0XP9fxACs6jjjn4208K\\z6xs1BfWAixgnNIFhSIWOQZ5kiq4W7q0veEoJQ/GOq\\FP1XlqGr9nPpBxxSBQJwnFNYQoNNJ5/ix9daSWNUkgo1eqzJTeHXG:1673507287475; _iuqxldmzr_=33; gdxidpyhxdE=5bhhiPlyKjCSt1HQnzP+P7gETki+Kz7bOYWCP6y5cm3QOVxIpLZoibAZ/YA0TY3wDI3L1rdswjB6KaHy1B5awy1STKqa1Ba2IJqQdH\\q+M0qJtbT9tinwuH4DyPm23oS2EpjE+iJ\\+\\i\\mywUdvwbthqwYmeYY0AYcPMZmmX2gtVQ\\Ha:1673506387816; YD00000558929251:WM_NI=wRAhiP6EOuTF0j3qICZ36IpdWKTzkpCpsLiVWlXKiKces8ZypJ1sdoX5DK0pIdd5IcVH2dp+qNLGEqJ4aidCY0bPpi+8krReDjNS6ZrpnQJ1dqtjLAeVrMVG8W3nuqHBeGU=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6eea4b3429cec848eeb6698968ea6c54e868a9eb0d446b4b9a48bb465bc9fb798b62af0fea7c3b92ab58e8f89ec5cf69b8196d74ef4b6fd92cc499aedfaaaf37095959c9bea7c81b0bdadec5e95b498acec41a1bbbdd9d973b496ff94e7659ceb8cacb37c8bbcacd6cc699698fe85f321b39284a8c87b8aecaf8ae7499bf0aeb1f363e9e8a593d16f88b682b4b552b2b0a2b2c93ba8afbc8bcf4ba78abeaed54dfc89fd8ff87b8c9eaba8d837e2a3; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a1ab1bd8eb68a03fbc4cf92911af65c141993166e004087dd313ea9d3604d6400287a4a588fc38e2ff45ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684; __csrf=7344364ef273196d81dc0b512d27fe31" }
        };
        
        /// <summary>
        /// 默认历史记录数据
        /// </summary>
        public static JObject HistoryDefault = new JObject()
        {
            { "Songs", new JObject() },
            { "Search", new JObject() }
        };

        public enum SettingParams { 
            Volume,
            AudioCacheFolderPath,
            DownloadFolderPath,
            ImageCacheFolderPath,
            LyricCacheFolderPath,
            EqualizerEnable,
            EqualizerString,
            EqualizerCustomData,
            WasapiOnly,
            AudioLatency,
            MusicPageShowLyricPage,
            NeteaseMusicCookie
        }

        public static void InitFiles()
        {
            Directory.CreateDirectory(BaseFolder);
            Directory.CreateDirectory(UserDataFolder);
            Directory.CreateDirectory(CacheFolder);
            Directory.CreateDirectory(AudioCacheFolder);
            Directory.CreateDirectory(ImageCacheFolder);
            Directory.CreateDirectory(LyricCacheFolder);

            if (!File.Exists(PlayListDataPath))
            {
                File.Create(PlayListDataPath).Close();
                File.WriteAllText(PlayListDataPath, "{}");
                PlayListHelper.AddPlayList(PlayListDefault);
            }
            
            if (!File.Exists(SettingDataPath))
            {
                File.Create(SettingDataPath).Close();
                File.WriteAllText(SettingDataPath, SettingDefault.ToString());
            }
            
            if (!File.Exists(HistoryDataPath))
            {
                File.Create(HistoryDataPath).Close();
                File.WriteAllText(HistoryDataPath, HistoryDefault.ToString());
            }
        }

        public static JObject JSettingData
        {
            get => JObject.Parse(File.ReadAllText(SettingDataPath));
            set
            {
                File.WriteAllText(SettingDataPath, value.ToString());
            }
        }
    }

    public static class JsonNewtonsoft
    {
        /// <summary>
        /// 把对象转换为JSON字符串
        /// </summary>
        /// <param name="o">对象</param>
        /// <returns>JSON字符串</returns>
        public static string ToJSON(this object o)
        {
            if (o == null)
            {
                return null;
            }
            return JsonConvert.SerializeObject(o);
        }
        /// <summary>
        /// 把Json文本转为实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        public static T FromJSON<T>(this string input)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(input);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }
    }
}
