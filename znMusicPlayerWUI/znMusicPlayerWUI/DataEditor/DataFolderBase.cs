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
            { SettingParams.NeteaseMusicCookie.ToString(), "_ntes_nnid=41c754489047fd9ab73dad6977523940,1661445480833; _ntes_nuid=41c754489047fd9ab73dad6977523940; NMTID=00OWpRrMlrN04TlNkAln4-5ctxFpi8AAAGC1d4fdw; WM_TID=WWP0p1yu9k9ABUAUAVKATPM286wIDpTN; __snaker__id=vkDfQnv7pzaH8kzg; _9755xjdesxxd_=32; YD00000558929251:WM_TID=U/DdtctJj1JBRQRFQVOFHPbu6OiAER+K; ntes_kaola_ad=1; __remember_me=true; timing_user_id=time_qjeeY3gil1; _fbp=fb.1.1664534924476.1928515079; WM_NI=6o89g7dIkvy0/37vj+YaTiwGZoZsDjMA4tHZxFJPilpQM1q1owj9VWKOjzUlw1qsKV0r4ZZnXS1rMgrRPmu3MEY85ahfIVWLju/3SyGVunb+tKJS6vB8Hv15CYLXGECEVUY=; WM_NIKE=9ca17ae2e6ffcda170e2e6eeb9bc6998998899cc4f90868ab2d55a979f9a82c159fb8cfba5f96eaf8ca9a9d52af0fea7c3b92af6b1f8dae63da897a597fc3cededf997e9219bbf9c8af64d9ab19694c7628f9ffc95cb52bc9a86b1ec68b48f83d2f43bada9a6dad55d8991a69bd868b5efbf94ca5ef4aefcbbd9669c8dff93d06db28ea08ef8398f9b97d9b86bacba87b7d648aaabe1d4d273858abb8fe121b89586b4b57e82e88897b43f818784b9c64f819b9ab6dc37e2a3; gdxidpyhxdE=I3oAw+ROfb/RPPz7cvbtf9sG3Dv5Zqg7MAoGo9RDKe\\IC\\5JY6fvNGvK5uNngWjXi/x2pL9f\\jjYdZl0EhE2\\tEnW7grtkV1tahJiUBy/xMu147ndQLyAOp+pNOxeC3NBkKRN4oxIVTGbtqo4bkxYBhsufm2LOy/zOUQCJ6iXjTsDlI1:1664539123414; YD00000558929251:WM_NI=U7IR8ZvKQPzGWLQ8owvg5fgAixidt8J6ROdOyhBp+rauFqlpIW4Lt8IAItpOICcjeARhNh16zSVXlQfu+ZzhWVyVQBBI7Kuab+rdGLJ2kIjPabVEUZstPAeWdQfkvij3OUQ=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6ee89c53ff1b7f9acf86df4928ba7d55a978e9bb1c454fb88bcb8e96eabbd8782e92af0fea7c3b92a97e987b3bb52b6959fb1f94d95949fd9ee7db6b9fb87e64baba8a785f7348aea9eb0e745bbb88e91aa7aa3ab8a92ef7c8b89fcadf7348b9f8795f3458b9dad8be84b92b88289b43eba8d9ab9ef5ba6abe5bbf964958ab7b7d44a8297f9acf86ba2f5be85c6488aeea5aae95ca3be9aa6d852b1aca0a2e85da88faa8cf563aaed81b6d437e2a3; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a17d2589048fe672eeed1b165e237c5ae2993166e004087dd34f7c04dd80b0a6ed3ce6cbb61b53bb3f45ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684; __csrf=8151dc526edcbead19ac4069c671eb1c; JSESSIONID-WYYY=xX\\ohgAwZ4+/VHhePZ1tmzGKkh7W/RvsZKi1h4XNn1KJw9G2gv3o9Jp6iD5ZU3fMQ3hZh1/w30v4XWcE1ulEJcnP12+jDqZ6Gfyx90Xld7ol8cATscE1K1AU\\TR\\YKvkTkYouSoeiD\\TokqTOTHXjIG0KfnO\\+q0s2iMd6Ah5m+Qvmda:1664600313819; _iuqxldmzr_=32" }
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
