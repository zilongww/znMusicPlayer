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
            { SettingParams.NeteaseMusicCookie.ToString(), "_ntes_nnid=41c754489047fd9ab73dad6977523940,1661445480833; _ntes_nuid=41c754489047fd9ab73dad6977523940; NMTID=00OWpRrMlrN04TlNkAln4-5ctxFpi8AAAGC1d4fdw; WM_TID=WWP0p1yu9k9ABUAUAVKATPM286wIDpTN; __snaker__id=vkDfQnv7pzaH8kzg; _9755xjdesxxd_=32; YD00000558929251:WM_TID=U/DdtctJj1JBRQRFQVOFHPbu6OiAER+K; ntes_kaola_ad=1; _fbp=fb.1.1664534924476.1928515079; __remember_me=true; WM_NI=mPbRioBux8MpZxp3inSyFLkMGftHeuLWilCsST5i5KprMJo1a2qv+6++G/sHbAm8nKjicWAytikPyuswP3g0tHssxHQRSdLJ6EHUFO24JYYwrKXih88HFuy3Txr72RNnQnk=; WM_NIKE=9ca17ae2e6ffcda170e2e6eedaaa448b888b92b13e88e78ab7d85e869a9ab1c842f3aea197f1459cbbe58de62af0fea7c3b92aaf90aeb3ec6582abb8d9d07c9aa8c0d4ec6691a78aadea4785babca9e87f9089a78dc279918cfb8ab841b4ae83abee3ba3eebf97aa3ee9f589d0f244a29fa3d9ed41aab7adb8c473b8b7a5b1fc7fb1a9a0d4e63cb898bd94fb44938e9d85cf42f69b88b6c545f2ec86badc7db295988aeb34fc988a96fc3998a8bed8b15a8eb29f8fea37e2a3; JSESSIONID-WYYY=5KhSSlvG9xrBlHz8mEecQv39IdPT0TK84VuZGh1Dp1o1G+G2XzKYY1/qXI9qIjcogMgDC0vTAa+3S7S+kuDAj06rxdMvizUJ4thNYMkxi7yRyDDwE/\\2E4lvxChVdT7b0GX8tBHg1WDJ\\nwdKMKuqE5rgfDKqcxtAs69iApJqzmY\\9pi:1671800894537; _iuqxldmzr_=33; gdxidpyhxdE=XWR42QdbzlNT68RhzeNZgiThiwpSfc9iMsgdVyhniBBABb4SGKaTssYQ1IxezRMYM0CPXjIJuO2T8QMgMo20b97qm1yCPRrsNLamKT/Hgi59D\\sBlKCo6ucfcETs\\UE/NO8JKZsvR7t+4r8V1BN1pNYbf3bT71QKSeWHXYAIk8ockIBH:1671799997475; YD00000558929251:WM_NI=CKcvj/SaD8OadiKXR4YKSppkl18snM9WVP/KYBfkm9DJdXTjqLBwDbdTzqnTdU4RvZhrQb8IlQxQZyzZjT6qQblhEtPVmAt1ez81e15/CcbmVyif5EkPhLc8ASL+jlAgYkI=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6eeb9f64396adfe88d2338e868ab3c55a829a8fb1d542f3affb97ae48acebaa90b32af0fea7c3b92a8195ad97e92591bf8cd9ce6ba8b785b9d1409b959d91f361b0eff894ed47fb899ab1ae419b9eaa8cf0338896acb9d762b3948e98c76aa88cb692ed60a88bfcb3f552acae9f83bb43b08fb8b2d973ba86a6b5b57b93bea489c880948aa3a2f73ba7a6f8d2e83bf7f18d84e16796a7a787b84fa990a8ade234838d84cce96883b9978ccc37e2a3; __csrf=29be1324a1985b5342e08284bbd09045; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a13acb4150caa6eccfde598ee8a3f55797993166e004087dd3a334420b2d86f6a9d1f9be323b65af8f45ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684" }
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
