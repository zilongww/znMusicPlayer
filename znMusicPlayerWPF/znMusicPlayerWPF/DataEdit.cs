using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF
{
    public class BaseFoldsPath
    {
        /// <summary>
        /// 程序数据文件夹
        /// </summary>
        public static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + $"\\{App.BaseName}Datas\\";

        /// <summary>
        /// 数据文件夹
        /// </summary>
        public static string LastPath { get; } = BasePath + "Datas\\";

        /// <summary>
        /// 歌曲缓存文件夹
        /// </summary>
        public static string SongsFolderPath { get; set; } = BasePath + "Songs\\";

        /// <summary>
        /// 图片缓存文件夹
        /// </summary>
        public static string ImageFolderPath { get; set; } = BasePath + "Images\\";

        /// <summary>
        /// 默认下载位置
        /// </summary>
        public static string DownloadFolderPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\";

        /// <summary>
        /// 歌单数据文件
        /// </summary>
        public static string SongDataPath { get; set; } = LastPath + ".usersongdata.txt";

        /// <summary>
        /// 设置数据文件
        /// </summary>
        public static string SettingDataPath { get; set; } = LastPath + ".usersettingdata.txt";

        /// <summary>
        /// 背景文件
        /// </summary>
        public static string BackgroundPath { get; set; } = BasePath + ".Background.image";

        /// <summary>
        /// 程序版本文件
        /// </summary>
        public static string SoftwareDataPath { get; set; } = LastPath + ".data";

        public static JObject TheUserSongData
        {
            get
            {
                string UserSongData;

                try { UserSongData = File.ReadAllText(SongDataPath); }
                catch { CreatSongDataFile(); UserSongData = "{}"; }

                JObject UserSongData1 = JObject.Parse(UserSongData);
                return UserSongData1;
            }
            set
            {
                try { File.WriteAllText(SongDataPath, value.ToString()); }
                catch { CreatSongDataFile(); }
            }
        }
        public static string NowSettingData
        {
            get
            {
                string SettingData;

                try { SettingData = File.ReadAllText(SettingDataPath); }
                catch { CreatSettingDataFile(); SettingData = NormalSettingText; }

                return SettingData;
            }
            set
            {
                try { File.WriteAllText(SettingDataPath, value); }
                catch { CreatSettingDataFile(); }
            }
        }
        public static string NormalSettingText
        {
            get
            {
                int BlurRadius = 45;
                bool Animate = true;
                bool LrcBlurBackground = true;
                bool Snp = true;
                TextHintingMode textHintingMode = TextHintingMode.Fixed;

                zilongcn.Others.TestResult testResult = zilongcn.Others.CheckIsFast();
                if (testResult == zilongcn.Others.TestResult.低)
                {
                    Animate = false;
                    BlurRadius = 0;
                    LrcBlurBackground = false;
                    Snp = false;
                    textHintingMode = TextHintingMode.Animated;

                }
                else if (testResult == zilongcn.Others.TestResult.中)
                {
                    Animate = false;
                    BlurRadius = 14;
                    LrcBlurBackground = true;
                    Snp = false;
                    textHintingMode = TextHintingMode.Animated;
                }
                else if (testResult == zilongcn.Others.TestResult.高)
                {
                    Animate = true;
                    BlurRadius = 100;
                    LrcBlurBackground = true;
                    Snp = true;
                    textHintingMode = TextHintingMode.Fixed;
                }
                TestFastResult = testResult;

                return
                    $"Volume = 0.5\n" +
                    $"LoadPath = {BasePath}Songs\\\n" +
                    $"DownloadPath = {Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)}\\\n" +
                    $"LrcColor = 255,255,128,128\n" +
                    $"OtherLrcColor = 145,255,255,255\n" +
                    $"LrcCenter = {false}\n" +
                    $"BlurRadius = {BlurRadius}\n" +
                    $"BlurMod = 1\n" +
                    $"Animate = {Animate}\n" +
                    $"WindowTopmost = {false}\n" +
                    $"Background = {false}\n" +
                    $"BackgroundBlurRadius = 13\n" +
                    $"BackgroundShadow = 0.3\n" +
                    $"BackgroundShadowColor = 255,255,255,255\n" +
                    $"StartUp = {false}\n" +
                    $"LrcBlurBackground = {LrcBlurBackground}\n" +
                    $"AudioApi = Wasapi\n" +
                    $"WasapiNotShare = {false}\n" +
                    $"ShowTranslateOnly = {false}\n" +
                    $"PlayMod = {MusicPlayMod.PlayMod.Seq}\n" +
                    $"UseLayoutRounding = {Snp}\n" +
                    $"SnapsToDevicePixels = {false}\n" +
                    $"TextHintingMode = {textHintingMode}\n" +
                    $"DesktopLrcSize = 23\n" +
                    $"LittleMusicPage = {false}\n" +
                    $"VolumeDataShow = {false}\n" +
                    $"VolumeDataSystem = {false}\n" +
                    $"DarkTheme = {false}\n" +
                    $"SoftwareVisual = {false}\n" +
                    $"LrcWindowTaskbarShow = {false}\n" +
                    $"CacheMaximumSpace = 2048000000\n";
            }
        }

        public static zilongcn.Others.TestResult TestFastResult = zilongcn.Others.TestResult.中;

        public static void FirstLoad()
        {
            if (!Directory.Exists(LastPath)) Directory.CreateDirectory(LastPath);
            if (!Directory.Exists(SongsFolderPath)) Directory.CreateDirectory(SongsFolderPath);
            if (!Directory.Exists(ImageFolderPath)) Directory.CreateDirectory(ImageFolderPath);
            if (!File.Exists(SongDataPath)) CreatSongDataFile();
            if (!File.Exists(SettingDataPath)) CreatSettingDataFile();
            if (!File.Exists(SoftwareDataPath)) CreatSoftwareDataFile();
        }

        public async static void CreatSongDataFile()
        {
            FileStream Creater = new FileStream(SongDataPath, FileMode.Create, FileAccess.Write);
            StreamWriter Whiter = new StreamWriter(Creater);
            Whiter.WriteLine("{}");
            Whiter.Close();
            Creater.Close();
            await SongDataEdit.AddNewList();
        }

        public static void CreatSettingDataFile()
        {
            FileStream Creater = new FileStream(SettingDataPath, FileMode.Create, FileAccess.Write);
            StreamWriter Whiter = new StreamWriter(Creater);
            Whiter.WriteLine(NormalSettingText);
            Whiter.Close();
            Creater.Close();
        }

        public static void CreatSoftwareDataFile()
        {
            FileStream Creater = new FileStream(SoftwareDataPath, FileMode.Create, FileAccess.Write);
            StreamWriter Whiter = new StreamWriter(Creater);
            Whiter.WriteLine(System.Convert.ToBase64String(System.Text.Encoding.Default.GetBytes($"{App.BaseName}\n{App.BaseVersion}")));
            Whiter.Close();
            Creater.Close();
        }
    }

    public class SongDataEdit
    {
        public static async Task<List<TheMusicDatas.MusicData>> GetSongDataMusicDatas(string listName = "default")
        {
            return await Task.Run(async () =>
            {
                List<TheMusicDatas.MusicData> TheDatas = new List<TheMusicDatas.MusicData>();
                JObject data = BaseFoldsPath.TheUserSongData;

                if (!data.ContainsKey(listName))
                {
                    await AddNewList(listName);
                    data = BaseFoldsPath.TheUserSongData;
                }

                foreach (var songdata in data[listName]["songs"] as JObject)
                {
                    JToken NowData = data[listName]["songs"][songdata.Key];

                    TheMusicDatas.MusicFrom musicFrom = TheMusicDatas.MusicFrom.kwMusic;

                    try
                    {
                        musicFrom = TheMusicDatas.MusicFromFromString(NowData["From"].ToString());
                    }
                    catch { musicFrom = TheMusicDatas.MusicFrom.kwMusic; }

                    TheMusicDatas.MusicData musicData = new TheMusicDatas.MusicData(NowData["Title"].ToString(), NowData["Rid"].ToString(), NowData["Artist"].ToString(), NowData["Album"].ToString(), NowData["AlbumID"].ToString(), NowData["PicUri"].ToString(), NowData["Time"].ToString(), null, musicFrom, TheMusicDatas.MusicKbps.Kbps320, (string)NowData["IsDownload"]);
                    if (musicData.From == TheMusicDatas.MusicFrom.miguMusic && NowData["MiguLrcUrl"] != null)
                    {
                        musicData.miguLrc = NowData["MiguLrcUrl"].ToString();
                    }

                    if (musicFrom == TheMusicDatas.MusicFrom.kgMusic) musicData.ThekgMusicBackupHash = NowData["Hash"].ToString();

                    TheDatas.Add(musicData);
                }

                return TheDatas;
            });
        }

        public static async Task<List<TheMusicDatas.MusicListData>> GetAllMusicListData()
        {
            List<TheMusicDatas.MusicListData> TheDatas = new List<TheMusicDatas.MusicListData>();
            JObject data = BaseFoldsPath.TheUserSongData;

            foreach (var listData in data)
            {
                TheDatas.Add(await GetMusicListData(listData.Key));
            }

            return TheDatas;
        }

        public static async Task<TheMusicDatas.MusicListData> GetMusicListData(string listName = "default")
        {
            JObject data = BaseFoldsPath.TheUserSongData;

            if (!data.ContainsKey(listName))
            {
                await AddNewList(listName);
                data = BaseFoldsPath.TheUserSongData;
            }

            TheMusicDatas.MusicListData musicListData = new TheMusicDatas.MusicListData(
                listName,
                data[listName]["name"].ToString(),
                data[listName]["picPath"].ToString() == "null" ? null : data[listName]["picPath"].ToString(),
                TheMusicDatas.MusicFromFromString(data[listName]["from"].ToString()),
                data[listName]["id"].ToString(),
                await GetSongDataMusicDatas(listName)
                );

            return musicListData;
        }

        public static async Task<TheMusicDatas.MusicListData> GetMusicListData(TheMusicDatas.MusicListData musicListData)
        {
            return await GetMusicListData(musicListData.listName);
        }

        public static async Task AddNewList(string listName = "default", string picPath = null, TheMusicDatas.MusicFrom listFrom = TheMusicDatas.MusicFrom.localMusic, string id = null)
        {
            await Task.Run(() =>
            {
                JObject data = BaseFoldsPath.TheUserSongData;

                JObject info = new JObject() { { "name", listName }, { "picPath", picPath }, { "from", listFrom.ToString() }, { "id", id }, { "songs", new JObject() } };
                data.Add(listName, info);

                BaseFoldsPath.TheUserSongData = data;
            });
        }

        public static async Task AddNewList(TheMusicDatas.MusicListData musicListData)
        {
            await AddNewList(musicListData.listShowName, musicListData.picPath, musicListData.listFrom, musicListData.listId);

            if (musicListData.songs != null)
            {
                foreach (var song in musicListData.songs)
                {
                    await AddUserSong(song, musicListData.listShowName);
                }
            }
        }

        public static async Task RemoveList(string listName = "deufult")
        {
            await Task.Run(async () =>
            {
                JObject data = BaseFoldsPath.TheUserSongData;

                if (!data.ContainsKey(listName))
                {
                    await AddNewList(listName);
                    data = BaseFoldsPath.TheUserSongData;
                }
                data.Remove(listName);

                BaseFoldsPath.TheUserSongData = data;
            });
        }

        public static async Task RemoveList(TheMusicDatas.MusicListData musicListData)
        {
            await RemoveList(musicListData.listName);
        }

        public static async Task AddUserSong(TheMusicDatas.MusicData TheDatas, string addListName = "default")
        {
            await Task.Run(async () =>
            {
                JObject data = BaseFoldsPath.TheUserSongData;

                if (!data.ContainsKey(addListName))
                {
                    await AddNewList(addListName);
                    data = BaseFoldsPath.TheUserSongData;
                }

                JObject TheSongDatas = new JObject() { { "Title", TheDatas.Title }, { "Artist", TheDatas.Artist }, { "Album", TheDatas.Album }, { "Rid", TheDatas.SongRid }, { "AlbumID", TheDatas.AlbumID }, { "PicUri", TheDatas.PicUri }, { "Time", TheDatas.Time }, { "IsDownload", TheDatas.IsDownload }, { "From", TheDatas.From.ToString() } };

                if (TheDatas.From == TheMusicDatas.MusicFrom.kgMusic) TheSongDatas.Add("Hash", TheDatas.ThekgMusicBackupHash);
                else if (TheDatas.From == TheMusicDatas.MusicFrom.miguMusic) TheSongDatas.Add("MiguLrcUrl", TheDatas.miguLrc);

                try
                {
                    if (TheDatas.From != TheMusicDatas.MusicFrom.localMusic) (data[addListName]["songs"] as JObject).Add(new JProperty(TheDatas.From + TheDatas.SongRid, TheSongDatas));
                    else
                    {
                        (data[addListName]["songs"] as JObject).Add(new JProperty(TheDatas.From + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(TheDatas.Title)), TheSongDatas));
                    }
                }
                catch (ArgumentException) { }

                BaseFoldsPath.TheUserSongData = data;
            });
        }

        public static async Task AddUserSong(TheMusicDatas.MusicData TheDatas, TheMusicDatas.MusicListData musicListData)
        {
            await AddUserSong(TheDatas, musicListData.listName);
        }

        public static async Task RemoveUserSong(TheMusicDatas.MusicData musicData, string removeListName = "default")
        {
            await Task.Run(async () =>
            {
                JObject data = BaseFoldsPath.TheUserSongData;
                bool IsFound = false;

                if (!data.ContainsKey(removeListName))
                {
                    await AddNewList(removeListName);
                    data = BaseFoldsPath.TheUserSongData;
                }

                foreach (var songdata in data[removeListName]["songs"] as JObject)
                {
                    JToken nowdata = data[removeListName]["songs"][songdata.Key];

                    if (musicData.From != TheMusicDatas.MusicFrom.localMusic)
                    {
                        if (nowdata["Title"].ToString() == musicData.Title && nowdata["From"].ToString() == musicData.From.ToString() && nowdata["Album"].ToString() == musicData.Album)
                        {
                            (data[removeListName]["songs"] as JObject).Remove(songdata.Key);
                            IsFound = true;
                            break;
                        }
                    }
                    else
                    {
                        if (musicData.IsDownload == nowdata["IsDownload"].ToString())
                        {
                            (data[removeListName]["songs"] as JObject).Remove(songdata.Key);
                            IsFound = true;
                            break;
                        }
                    }
                }

                if (!IsFound) return;

                BaseFoldsPath.TheUserSongData = data;
            });
        }

        public static async Task RemoveUserSong(TheMusicDatas.MusicData musicData, TheMusicDatas.MusicListData musicListData)
        {
            await RemoveUserSong(musicData, musicListData.listName);
        }
    }

    public class SettingDataEdit
    {
        public static string GetParam(string Key)
        {
            try
            {
                foreach (string i in BaseFoldsPath.NowSettingData.Split('\n'))
                {
                    if (!i.Contains(" = ")) continue;

                    string[] StringList = i.Split('=');
                    string TheKey = StringList[0].Replace(" ", "");
                    string SParam = StringList[1].Replace(" ", "");

                    if (Key == TheKey)
                    {
                        return SParam;
                    }
                }
            }
            catch
            {
                foreach (string i in BaseFoldsPath.NormalSettingText.Split('\n'))
                {
                    if (!i.Contains(" = ")) continue;

                    string[] StringList = i.Split('=');
                    string TheKey = StringList[0].Replace(" ", "");
                    string SParam = StringList[1].Replace(" ", "");

                    if (Key == TheKey)
                    {
                        return SParam;
                    }
                }
            }
            return "False";
        }

        public static void SetParam(string Key, string Param)
        {
            string data = BaseFoldsPath.NowSettingData;

            try
            {
                foreach (string i in data.Split('\n'))
                {
                    if (!i.Contains(" = ")) continue;

                    string[] StringList = i.Split('=');
                    string TheKey = StringList[0].Replace(" ", "");
                    string SParam = StringList[1].Replace(" ", "");

                    if (Key == TheKey)
                    {
                        data = data.Replace(i, $"{Key} = {Param}");
                        break;
                    }
                }

                BaseFoldsPath.NowSettingData = data;
            }
            catch
            {
                data += $"{Key} = {Param}\n";

                foreach (string i in data.Split('\n'))
                {
                    if (!i.Contains(" = ")) continue;

                    string[] StringList = i.Split('=');
                    string TheKey = StringList[0].Replace(" ", "");
                    string SParam = StringList[1].Replace(" ", "");

                    if (Key == TheKey)
                    {
                        data = data.Replace(i, $"{Key} = {Param}");
                        break;
                    }
                }

                BaseFoldsPath.NowSettingData = data;
            }
        }

        public static bool ToBool(string value)
        {
            return bool.Parse(value);
        }

        public static Color ToColor(string value)
        {
            string[] ColorList = value.Split(',');
            return Color.FromArgb(byte.Parse(ColorList[0]), byte.Parse(ColorList[1]), byte.Parse(ColorList[2]), byte.Parse(ColorList[3]));
        }

        public static System.Windows.Media.Effects.RenderingBias ToRenderingBias(string value)
        {
            if (value == "1") return System.Windows.Media.Effects.RenderingBias.Performance;
            else if (value == "2") return System.Windows.Media.Effects.RenderingBias.Quality;
            return System.Windows.Media.Effects.RenderingBias.Performance;
        }

        public static AudioPlayer.AudioOutApiEnum ToAudioOutApi(string value)
        {
            switch (value)
            {
                case "WaveOut":
                    return AudioPlayer.AudioOutApiEnum.WaveOut;
                case "DirectSound":
                    return AudioPlayer.AudioOutApiEnum.DirectSound;
                case "Wasapi":
                    return AudioPlayer.AudioOutApiEnum.Wasapi;
                case "Asio":
                    return AudioPlayer.AudioOutApiEnum.Asio;
            }
            return AudioPlayer.AudioOutApiEnum.DirectSound;
        }

        public static MusicPlayMod.PlayMod ToPlayMod(string value)
        {
            switch (value)
            {
                case "Seq":
                    return MusicPlayMod.PlayMod.Seq;
                case "Random":
                    return MusicPlayMod.PlayMod.Random;
                case "Loop":
                    return MusicPlayMod.PlayMod.Loop;
                default:
                    return MusicPlayMod.PlayMod.Seq;
            }
        }

        public static TextHintingMode ToTextHintingMode(string value)
        {
            switch (value)
            {
                case "Animated":
                    return TextHintingMode.Animated;
                case "Fixed":
                    return TextHintingMode.Fixed;
                default:
                    return TextHintingMode.Auto;
            }
        }
    }
}
