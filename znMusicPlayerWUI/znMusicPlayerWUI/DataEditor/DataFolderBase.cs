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
            { SettingParams.NeteaseMusicCookie.ToString(), "_ntes_nnid=0590a8d6b6be9cad6bc23b97c07f7cdc,1672820325011; _ntes_nuid=0590a8d6b6be9cad6bc23b97c07f7cdc; NMTID=00OjlEbhTNEzgg00U1LuFYI4ZujWywAAAGFe9x7WA; WM_NI=UvEVNICG8Tuo2SLkVTOVEVD6KhJLG3R2jKJMrK4vydLfnK0o8IPwIXGPNcCwmtLxWQ419zJ9wxb1n/ThuX7NgfJOTfDmwjM/sdRT1iLVBd/T4pkce3+Ix+bQY2lxxhMNaTE=; WM_NIKE=9ca17ae2e6ffcda170e2e6ee9afc4ba794bd94d03cf5ac8ea3d54a979e8ab0c547f1afaf90f55ab3e781daf22af0fea7c3b92a97aa8bb7cd438199f8b5f865f28d848cd5608d8a8bb7c73e89b886adc63992eea6acc947b295fc97fc7090baa2acb365fc97a098cc5485908284c27dafac8499d65bf8eff79bc933b5a8aed2ed2598b8bbb9b646a5ba86b0d76e88b3b98bd025b1bc9eb5b463908a8e85ae60f8b0a584e839e997b8cce15b9beea499fb648f92afb5c837e2a3; WM_TID=D1p/ipOchYdFQRFFBALAJyqesrVvtbz+; __snaker__id=piPDdtsHQOZxmZu2; gdxidpyhxdE=i4lfd0iKs9PaiQG4PIdz\\JzV1K4xSHA67hq\\fnIM0KJN\\M2NwOJfBZ/RM6ZKAyA4\\Bk8mLOGa2LDooNqZDD/IiQuNUTPsVEHsu4KCUhCDtLkpNlsY6LMzjBE0xQE5+69kGNmq+vhRMPpUCsVw1xYiW6wB/YB0I3PRbQYu4Y+8HpqRGpi:1672821234351; YD00000558929251:WM_TID=K5mT5w0FhgtEUQQUQVaQdn6btuTcRF4w; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6eeb0ae39abab8284c76592928ea6c85e879b8bb1d542f1aaa095aa50a19b8e94ef2af0fea7c3b92aa9b9a0abd442bbaca195c2528dbd8282d87f9aa8ffd9b367f8b99b8cb73bb297abd9d86187f18384aa64898d85d4b6668aada8d0c2498cb1acd9dc79bcb19aa3c261a7e9fedaf57fb396fdb2b266ac89a0ccb66490eafba6e66b9bb38daee14bbaeb8dabf87ea688ffd8eb6495a6aeaef1808de7ad8afb6e95b8bc90d921b59e9ab9cc37e2a3; YD00000558929251:WM_NI=kgPJQJytotCZOcNaUuZx180m4gUk41rIe8UkE/Mc+hKSK37nJsh/CCHod8YsxoVBCke729vuqJ3Q3nhWP+7hL65EgaYmCMbAz5CJuvfX17hhWzbMrtO9cixfWhtoZ+wBVXI=; __remember_me=true; __csrf=65af5ef29e98c4dd2623233be86fde56; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a1b259013c90dad64d1720a3d9c60e3b8a993166e004087dd368176e9d9a423736e47c408db9c3642845ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684; ntes_kaola_ad=1; JSESSIONID-WYYY=ZpzdoQsCzraK1Q2IVMX4rH72rv/6YStrIlTp8XQVgXFtckN9dlTeTwJ2Wj+zveGKeDvpoXBc+HGa7aPlwKy\\fKA\\hksbQPU/c3IQAoV7M6O+WkZddhahzb\\0YyqccfR\\bRsHF\\tYVIxkYqbAG+fA6UccDsDgvowHAZhIhBE2IPGtBvW8:1672829137064; _iuqxldmzr_=32" }
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
