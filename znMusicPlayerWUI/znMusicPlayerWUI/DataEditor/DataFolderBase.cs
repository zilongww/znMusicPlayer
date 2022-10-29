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
            { SettingParams.NeteaseMusicCookie.ToString(), "_ntes_nnid=41c754489047fd9ab73dad6977523940,1661445480833; _ntes_nuid=41c754489047fd9ab73dad6977523940; NMTID=00OWpRrMlrN04TlNkAln4-5ctxFpi8AAAGC1d4fdw; WEVNSM=1.0.0; WNMCID=gkblta.1661445481168.01.0; WM_TID=WWP0p1yu9k9ABUAUAVKATPM286wIDpTN; __snaker__id=vkDfQnv7pzaH8kzg; _9755xjdesxxd_=32; YD00000558929251:WM_TID=U/DdtctJj1JBRQRFQVOFHPbu6OiAER+K; ntes_kaola_ad=1; timing_user_id=time_qjeeY3gil1; _fbp=fb.1.1664534924476.1928515079; __remember_me=true; gdxidpyhxdE=p+\\+rXHSPJov7S87S6zmS5\\ex\\eXB5V/IgdK4ozLLAOCZvBOuI2A+VhCGt73MuEuTf9Uzdv\\4urOX1d71Tw9/P4yEosrC2nS+MU/3ZhclQ2v/2Oa6KPw8p5m58HpI6s10z/J3LuwV/O31nDxje2iMVB2aL7QZaIj0dYetR45bEnBp0Dz:1666521172876; YD00000558929251:WM_NI=W+Fn3rRkCpur8XRPx0YKkMcbgyYkk6O+Yt/lO+flUwHzW7oETGt5rGMdpe13rxaBgZZhYDBegxXnrpnzG0Si/YTuhKZtLVIVCj1Vid2PrDyeTbkSP5M+vxwMVrgUTwGGZ2o=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6eed7c45fbaa7b6d5ed60f7b88aa3d15e869b9bb0d469829ab9bbee42a7f5878ecc2af0fea7c3b92a95f58a8fb27a92b58d91f87afc889eb1fb3c9b95a5aee26aa5a7978cea3e8df59795ae688df5aa8dd87d8ca699d8ee4f9899bcd6f14d8fbcc086b439b2a8afa3e6529ab897a5c16fa5a8988ff17caea689d1d263ed879c96eb419aac84b7cc5e81b6ffb7ec70f290bea5fc6f98bea5b4d33f8ff5ba99f64796aea9b6d77d859996d3ee37e2a3; __csrf=63ffc1532f8d585a754b063f87b6f3a0; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a15f78183878bee4c437137dd64cc0d893993166e004087dd3155d5291b151506103d25c4818c96beb45ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684; WM_NI=DHsKVtiAqDiCLp6acgOldzt1ZxFPk/dA3kCspyWLxT0QuuyWCtvRxxKheOOg6NLEH9TtE8Z9xsqR+66SJt6YKX5w7Pd24i1vWUGHu3S//kv1rVxSxAlyLBww1z4YYD2rZEQ=; WM_NIKE=9ca17ae2e6ffcda170e2e6ee96dc4393f0b7a3c961919a8ea7c84a939f9eacd55297befc8faa41fcefb682d12af0fea7c3b92a8898bdacd580ab9fbfa5ec4990b0fa82e26d8db4ac9bf73b9aa88ab1ea25a89ffd8cc279b4a799adfb60f48fbb96fc5d81acbab3fb7489b8abb0ce6df69284a6cb3398ac8bd9d933bcadbfb3aa3ef68d8695b5538988fb98b65ca8eefc8ab47e958b89a9f83991f1e18cf53bb28ab8b4fb4bb0a784a3f67df3a6fcbadc50f2ae96a6d437e2a3; JSESSIONID-WYYY=BbM5DMwprZuYHc/AKfWa\\0Sv8ThiWhhq/RFyYiiO9FqXA0bH8Eb2fQgowbMb3YTGXh\\M2QpvA9jNrdzquAAd6kgV5AR1xgHmYodO14QYehupV\\8eE1jO1hmkC9+UC0MGaJJj/vuQc2lnnhK8ys5fBP9\\BMF23p4wO64FSyEk+R8+\\8rw:1666964879468; _iuqxldmzr_=33" }
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
