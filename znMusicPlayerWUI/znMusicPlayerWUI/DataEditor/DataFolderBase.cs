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
            { SettingParams.NeteaseMusicCookie.ToString(), "_ntes_nnid=41c754489047fd9ab73dad6977523940,1661445480833; _ntes_nuid=41c754489047fd9ab73dad6977523940; NMTID=00OWpRrMlrN04TlNkAln4-5ctxFpi8AAAGC1d4fdw; WEVNSM=1.0.0; WNMCID=gkblta.1661445481168.01.0; WM_TID=WWP0p1yu9k9ABUAUAVKATPM286wIDpTN; __snaker__id=vkDfQnv7pzaH8kzg; _9755xjdesxxd_=32; YD00000558929251:WM_TID=U/DdtctJj1JBRQRFQVOFHPbu6OiAER+K; ntes_kaola_ad=1; __remember_me=true; timing_user_id=time_1gocMA82as; WM_NI=iYJQn/j9/zr/dzcbEFWOrUQg5ZhaUlxDXuAEthtECPjUwgEJP+fuA+y8Cefa/iIcjIwRxuWoBlZtozLWRXZ9XtWICIkXPKFI2kHcko4WddwRuZy/qKPFBBZVRS9rCeQqNEg=; WM_NIKE=9ca17ae2e6ffcda170e2e6eea3d64df7ab9ca2b56789b48ba3c55e868f9fadc544ba978b92e768ae87a5b1e62af0fea7c3b92aab8786b2ed25aae7e19bf125a8a6ad83c84e9591beb6d46df786a882d868bc9c9896c44fb8b8bca6c25caa8bb988c84294f5aa96c421bbe88d86e56bedb78784e943b58eb896d66582b49695ee7290899eb9d9339cac99aac243a988a0acc543f2b58884ea65f889ac85f65ab786b7d0f441949a8ea3d95e928df793c26f93af82a6e637e2a3; JSESSIONID-WYYY=BksAjFcGrklRzJ9tDf1B0+BkBKBlGXiGZ0aYEeuf/3I+Yu17gcxUWvdZfKl8TSu8YRp0E5KGd8bFE8dsUzfvAmJlq/6exCAOK6d/ziY1J/fhOzQn7h2+lxHgdWS+xlc69ORg8j\\5oY8p5MIk0u+arBXAEuH\\qiyUe5OYMFjbYfMFXeR0:1663511882776; _iuqxldmzr_=33; gdxidpyhxdE=9zrsh5XOP+tkTodJS+Cao6qBUkTagctoiH0K56kxRrph270SgjwjtDzkLqOvBCA\\sMBulpiCtT1WT2oMD954LfYsBCkr7w2VJDpHS/mLJOvO57GIMJiC7xXpk3JBXMSVL3AJ\\SkBSPaZIV\\9jm1EUPw2bB7NlB40d3mgXIxuznGUQYsv:1663510983478; YD00000558929251:WM_NI=PPsPadn1wQGmpfKwy+gXwNfJXpjsDznH2gKWNJ2pXt2L1qxL92iJGvAhpZTwb/DRKMxzerZFkOuqpz2rmkF1Daeoe11G/GbYx0a07ZyOSwLZYQxlN02XD8B7nh/xaxvrSEE=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6eea9c639b6b3aa85c463fbb08ea6d54a869a9aacd44dba969ecce15c97ee8cd6bc2af0fea7c3b92a9490bdb1e470aeefb9b2c667b4ba8598fc21a588b9afe5429cb0a694c772ae98fe88ca5d8e96fe91db80f294ff92fb48fbeea7abc67e83b8c0bbd77da2f18cb3ca47bca6ab93d94ea991bb92f372f2aea38cc53b88bfab90e83bf399e1a8e153bcf0afd1b652bb919d98cf529b8fb88dcd3cf2888cd9c13daeb8e199e474b6ae9da6c837e2a3; __csrf=09d2b7d04b802786ab0412225d6f10d8; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a172afddb4a5428bd9fb33801418f8c31e993166e004087dd389c273c5654780bd773a5d05af07374d45ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684" }
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
