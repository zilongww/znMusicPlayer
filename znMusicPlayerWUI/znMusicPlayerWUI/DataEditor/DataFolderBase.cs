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
            { SettingParams.NeteaseMusicCookie.ToString(), "_ntes_nnid=41c754489047fd9ab73dad6977523940,1661445480833; _ntes_nuid=41c754489047fd9ab73dad6977523940; NMTID=00OWpRrMlrN04TlNkAln4-5ctxFpi8AAAGC1d4fdw; WM_TID=WWP0p1yu9k9ABUAUAVKATPM286wIDpTN; __snaker__id=vkDfQnv7pzaH8kzg; _9755xjdesxxd_=32; YD00000558929251:WM_TID=U/DdtctJj1JBRQRFQVOFHPbu6OiAER+K; ntes_kaola_ad=1; _fbp=fb.1.1664534924476.1928515079; JSESSIONID-WYYY=\\Jizi\\o5anGGl7k5tbV/ByRVhfQDUJusdC0e+H9mtIotQ6XyX/fX3\\uI8a\\/K+9+vEABHDh5K5bQYng4uPyZB6U1XXJu9f\\lXOoc9vfCCugSv41O1MG\\CRZ2naINW2aVYUqI\\wXBQoYVaFoC85pEHNlqWzIIolsc6B05XbjwasA7bvvU:1671100459286; _iuqxldmzr_=33; WM_NI=17XBt/427p3Cjtlm/HDhmlAMDanIEls4ADapzkgGhhY58l4Z3ZygxfTUuU1XnxCrMiqnuKLvBdJBDqc1A4979lBnMhzJ7YC/buC1Y+Lwn94BTkIoU9qs0WN8fbJCJCCeaWE=; WM_NIKE=9ca17ae2e6ffcda170e2e6eeacf870af8d86a8ea40f3ac8fa7d44b938f8e87d569ac87a7b4f17d979d8c82f52af0fea7c3b92af3e998a3f725f8eef991b249aaaca48eae4488b8a38dc44788bfa2aac868b1ec8fa5e47cbab5a9a8eb649bebf88db752f186b788fb6e988bbbb6b454aea88d93d063b3b2bbaccf7e82bc86a3c77ba1ef8fd5bc3dfbb48e8fd064ba96f9bac225a2ab8dd2dc2190a9a2dab74a98b58790d833b3ad00b8cd34a6be86a4c94981bbafb8c837e2a3; gdxidpyhxdE=BR41kJO0/OL/c8uKfWiNhHQEEet0EQgAmzO/hfX9WTh1wvGLkdY8C8tYo6DvmMSThP+I/0pe/\\i8vw+bzxsMT8/3avrkJZ/Ye/w4owcHqrxTg4Th3TUuyOQLTfm2Wbr\\IpZdpjaw\\xrQ4RmKXiagW7JojPHmtmoP2eO43EDjj+E1cykj:1671099563195; YD00000558929251:WM_NI=Fmjc0GepI03iu95K5kuEY2EWgA4XOu6S3Q8NUQGUayvZVxaskNKSfiwhB2nvmr/bFzw1PY+4Ml1itPI1gza3INWCmxc+5DFLI8wBiLuzWF+SCw57EYcdtqgQxZ4mzOm0dDU=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6eea5d7808f9cb783f95ca6928eb6c55b969e9b87c546b0bca7a4f56d8688bba3ea2af0fea7c3b92a86b3a684b34da7b087d1b263b7e7fbacb861b79b97d3c85da59ffcb9ce7ff68dfdb2bb46978f89b6e473b6869a99e479a99285b4e563b5b88ed3ed7eafaee183c572b5efa0baaa408fb4ff8af75c8befa99be4398b9299a4f074a1f5fba5c5488be8b9a3ec48b7a699a7aa5981a9fbd8c853a1bcbc92e65bbc86fc8ef945aff0aca5d837e2a3; __csrf=c547d7f4653c8444b001648bf6064116; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a1d99ba4c6e2934ffae2b6315708e93397993166e004087dd3650e9e825ecb34044e14341572d3bafe45ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684; __remember_me=true" }
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
