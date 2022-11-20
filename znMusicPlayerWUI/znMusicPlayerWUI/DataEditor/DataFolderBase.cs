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
            { SettingParams.NeteaseMusicCookie.ToString(), "_ntes_nnid=41c754489047fd9ab73dad6977523940,1661445480833; _ntes_nuid=41c754489047fd9ab73dad6977523940; NMTID=00OWpRrMlrN04TlNkAln4-5ctxFpi8AAAGC1d4fdw; WEVNSM=1.0.0; WNMCID=gkblta.1661445481168.01.0; WM_TID=WWP0p1yu9k9ABUAUAVKATPM286wIDpTN; __snaker__id=vkDfQnv7pzaH8kzg; _9755xjdesxxd_=32; YD00000558929251:WM_TID=U/DdtctJj1JBRQRFQVOFHPbu6OiAER+K; ntes_kaola_ad=1; _fbp=fb.1.1664534924476.1928515079; __remember_me=true; WM_NI=YFvJxue1kDB7d3dmPo+RDZFOEMAfVrvDWgHdyaBGrw+vDuqvl3EzEubFJmz6nNPfRnpAmaHSp0wObGNiwJ4X8nkeHJreI6DNfhak7mvdAMdBKfT3lxWUJV9ps9kzTlwiTlM=; WM_NIKE=9ca17ae2e6ffcda170e2e6ee83aa409bbea586b65b8db08ba3c85e829e9ab1c5469b98c088d762edb399d5f12af0fea7c3b92a9b9abaabfb7fa7efa5a5c13da8edac8ed365e98e8cbbc54587938f87d57ab69c9988cb70bbbf8ea8f17de9aa8c96f47eb0ed8b9bc87fa29a868ef93eae92a087d166b49fa382cb59b4f0b9b0e14d8eb7b9abb754fcb2a586cb42b2bb87d7c746a6b8af8cb667b6bc8faee74a89ba9cd4ef74958b86b7bc7cb1e7a59bd768b5b79c8dd037e2a3; JSESSIONID-WYYY=x8OpTf5liS9K0KFlOoAEK0dBVDCJdKrkNjK140jHS0XChZ/NXW1uQKYZGZ5ZEshz4lQW4yUtJaN8Z652EcRGloyD\\epz0wqYoi64mae1cpyI\\wVBW7YO\\4Cuc1ITTd0fNk4nU7Eki/4BkZIBIZjTrX64TyX7+4Zux4y1Qd5dxbqj3dxe:1668841000937; _iuqxldmzr_=33; gdxidpyhxdE=IzxbUIRjwdfRfG3acX4Y59PVlza5b5QhMtWUONo+4w0SvH19YkLmivL68fuWHt+EWA/fJ+Xl17\\wii9KXO4RJBdqGRGRfXEYDr5b6UH0sfSWWyUx/REMacwgW30X5YA+11k2ccg5hYE+05tf9OQtZTK\\NBHoaMdSzgdIDHjPrbPpgg8E:1668840101349; YD00000558929251:WM_NI=WS/71RAw9SmrDuBYozrXEljPz5H24N6L6Sgf1uJ/dhQ15HRgTeBh74mV1TBxq+FjeOBo3VUPlPWqgrI1N7JLskuMt2eyyCxwevrCU8BJnj5CzqRg1VwXF+TMwty3Od9Bc2E=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6eea8c154a5e98489c9598e968fb7d84f929f8eadd5439b89fbd2fb52fb9fa8ade82af0fea7c3b92a958de1d8b45a83a9f7b4f07a88ab8ebaee72b2888b8de95cbaeb88d3b746f694fab4e66ef3ab86d0e76493effba9d16d98bb8e89b640af8affb5c174b3f58a8be84582b1fdb7d85cb0909992e67a8bef82d8c948b1b5bbaef73aa7a7b7a4fb7da7aabea4d8348296a28bb849baaf9e88b45eb5888accd747b5acb7d4ce70fb9eadd3c837e2a3; __csrf=6f357c85318362675d0be05fd50afae3; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a1efe156bf8c21e4695f52de40597e1e4c993166e004087dd392c5acbd0124b4cc3eb2a57e3dd993e545ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684" }
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
