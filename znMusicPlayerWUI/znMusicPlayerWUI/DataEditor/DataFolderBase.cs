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
            { SettingParams.NeteaseMusicCookie.ToString(), "_ntes_nnid=0590a8d6b6be9cad6bc23b97c07f7cdc,1672820325011; _ntes_nuid=0590a8d6b6be9cad6bc23b97c07f7cdc; NMTID=00OjlEbhTNEzgg00U1LuFYI4ZujWywAAAGFe9x7WA; WEVNSM=1.0.0; WNMCID=qkftya.1672820325246.01.0; WM_TID=D1p/ipOchYdFQRFFBALAJyqesrVvtbz+; __snaker__id=piPDdtsHQOZxmZu2; YD00000558929251:WM_TID=K5mT5w0FhgtEUQQUQVaQdn6btuTcRF4w; ntes_kaola_ad=1; WM_NI=YVKFAsUcgOug/l9MEq8s6bVIYSf6F5uGWvHFOO14deJsB/ZlyVRFUECUY3TQtj3888cLisZgg9QDi3l3e8uSl45KZb3ZbtpXkGz4UNedtLiyZHYufkIJ4fuKYhLgM6wTV2I=; WM_NIKE=9ca17ae2e6ffcda170e2e6ee98d37298bba5b7e53ab3968ba3d85f969b9a86c147e999bfb7aa4892bc878cd32af0fea7c3b92a9b8a85a7c47997bda9b0f86dedb4f7aec87bfcadfa83d5439b8daad7c53fb7999997cb4e8d91ffd5e76f8aad8ed0d968bb8a9ea7d84f818b97d4d75bb8b6fdd9bb34a194a794d96da5e79fa5ec39b0edabd9f859b0ecfbacd96af186ae95f354a999b6d5d846a7bcbcadec739a989796e5618b96fc87f8419bb88488d03eb58c9ad3cc37e2a3; JSESSIONID-WYYY=CTN4KVhPU2F8rFU/6+cOpdcUXlHtX75RN4SIf48epAaYJmXjEruvDW19vl4Sr\\UyKdVae+NP6gRsHmWuEooRAJ7ha/0KZZlBvJyfvVKNmnC5JX86JJcjnH2v4D1asy2Aag3JPl0jAqUmAZ\\8XM07UBqq8atIXJo+T7cztFWz6XZwME5i:1674218901445; _iuqxldmzr_=33; gdxidpyhxdE=yhs9kLpX4M9gtikxtaUpdXZ8+1xnpDVs3o0saLENBk2diejeTM3dA0YgEIPelc\\sW77eoncg7w30EEyeLA/drrQlIVENga/k3vWA6EfkjOKtR3Isd2WrLV2m+lIL3LX0Cw71BrSxHb0jdsQYWYvUpihblOWjbWmpi3DsgVfxn1c2W/WL:1674218001760; YD00000558929251:WM_NI=7TsVQ4+Lz5pDKb88mCu4oybhQW85GMglStHJCO3C558llRntCi66NtxZcqOysDifo8hsGV2HsARCNOS5u9lWRzXlPmq11lAgVjmwqhiPjuyaNstOA3WIC2z6KYPuX65ESmE=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6eebbed679788ffd1ea5fb7ac8bb7c15e979e8aacc170b78ce5b7f16f88edbaaeef2af0fea7c3b92af58cbdb7d440e994b6d6f35089bef8d9f049b7eca19ae1649389f8d6c647a5b49d95cb428191fda4b83ffcb4a4b3ed8081b7fad7cd80bc86ad92ce73b19ca787ee34acad89b7b144b19f9ea4cd4591ebbbdaef5d92a6988dd367b3efff8dc46d96b6a398f464ab90a696fc6b8eadbcb0c43995978dd3f93e8987a096db3ef79b9d8ec837e2a3; __csrf=80414aeef1b507d02bb6aba6d1dac5e6; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a18b9900c5d165cbbc45de5b67132ab567993166e004087dd3ede110f015be8131d0afecafcb10e3e245ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684; __remember_me=true" }
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
