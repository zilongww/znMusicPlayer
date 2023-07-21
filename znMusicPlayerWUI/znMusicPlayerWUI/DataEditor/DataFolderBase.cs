using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using znMusicPlayerWUI.Background.HotKeys;

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
        /// 日志文件路径
        /// </summary>
        public static string LogDataPath { get; } = UserDataFolder + @"\Log";

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
            { SettingParams.AudioLatency.ToString(), 200 },
            { SettingParams.MusicPageShowLyricPage.ToString(), true },
            { SettingParams.HotKeySettings.ToString(), JArray.FromObject(HotKeyManager.DefaultRegisterHotKeysList) }
        };
        
        /// <summary>
        /// 默认历史记录数据
        /// </summary>
        public static JObject HistoryDefault = new JObject()
        {
            { "Songs", new JObject() },
            { "Search", new JObject() }
        };

        /// <summary>
        /// 设置标志
        /// </summary>
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
            HotKeySettings
        }

        /// <summary>
        /// 初始化所有文件夹和文件
        /// </summary>
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

            if (!File.Exists(LogDataPath))
            {
                File.Create(LogDataPath).Close();
            }
        }

        /// <summary>
        /// <list type="table">
        ///     <item>数据文件的抽象。</item>
        ///     <item>使用时会读取设置文件，设置时会写入数据文件</item>
        /// </list>
        /// </summary>
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
