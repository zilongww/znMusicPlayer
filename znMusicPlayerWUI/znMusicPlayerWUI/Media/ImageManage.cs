using Microsoft.UI.Xaml.Media;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Scanners;
using znMusicPlayerWUI.Helpers;

namespace znMusicPlayerWUI.Media
{
    public static class ImageManage
    {
        public static List<DataEditor.MusicData> LoadingImages = new();
        static int loadNum = 0;
        static int maxLoadNum = 0;

        public static async Task<bool> DownloadPic(string a, string b)
        {
            try
            {
                await Helpers.WebHelper.DownloadFileAsync(a, b);
            }
            catch { }

            bool error = await Task.Run(() =>
            {
                if (System.IO.File.Exists(b))
                {
                    try
                    {
                        if (System.IO.File.ReadAllBytes(b).Length == 0)
                        {
                            System.IO.File.Delete(b);
                            return true;
                        }
                    }
                    catch { }
                }
                return false;
            });

            return error;
        }

        static Dictionary<string, ImageSource> localImageCache = new();
        /// <summary>
        /// 返回musicData对应的图像对象和图像所在的本地路径
        /// </summary>
        /// <param name="musicData"></param>
        /// <param name="decodePixelWidth"></param>
        /// <param name="decodePixelHeight"></param>
        /// <param name="useBitmapImage"></param>
        /// <returns>
        /// <list type="table">
        /// <item>T1: ImageSource，图像对象</item>
        /// <item>T2: string，图像在本地的路径</item>
        /// </list>
        /// </returns>
        public static async Task<Tuple<ImageSource, string>> GetImageSource(DataEditor.MusicData musicData, int decodePixelWidth = 0, int decodePixelHeight = 0, bool useBitmapImage = false)
        {
            ImageSource source = null;
            string resultPath = null;
            if (musicData.From == DataEditor.MusicFrom.localMusic)
            {
                string foundation = musicData.CUETrackData == null
                    ? string.IsNullOrEmpty(musicData.Album.Title) ? musicData.InLocal : musicData.Album.Title
                    : musicData.InLocal;
                if (localImageCache.ContainsKey(foundation))
                {
                    try
                    {
                        while (localImageCache[foundation] == null) await Task.Delay(400);
                        source = localImageCache[foundation];
                    }
                    catch { }
                }
                else
                {
                    localImageCache.Add(foundation, null);
                    source = await CodeHelper.GetCover(musicData.InLocal);
                    if (source != null) localImageCache[foundation] = source;
                    else
                    {
                        string coverPath = await Task.Run(() =>
                        {
                            FileInfo fileInfo = new FileInfo(musicData.InLocal);
                            string coverPath = $"{fileInfo.DirectoryName}\\Cover.jpg";
                            if (File.Exists(coverPath)) return coverPath;
                            else return null;
                        });
                        if (coverPath != null)
                        {
                            source = await FileHelper.GetImageSource(coverPath);//, decodePixelWidth, decodePixelHeight, useBitmapImage);
                            localImageCache[foundation] = source;
                        }
                        else
                            localImageCache.Remove(foundation);
                    }
                }
            }
            else
            {
                while (LoadingImages.Contains(musicData))
                {
                    await Task.Delay(1000);
                }
                resultPath = await FileHelper.GetImageCache(musicData);
                if (resultPath == null)
                {
                    while (loadNum > maxLoadNum)
                    {
                        await Task.Delay(400);
                    }
                    loadNum++;
                    LoadingImages.Add(musicData);

                    if (WebHelper.IsNetworkConnected)
                    {
                        string b = $@"{DataEditor.DataFolderBase.ImageCacheFolder}\{musicData.From}{(string.IsNullOrEmpty(musicData.Album.ID) ? musicData.MD5.Replace(@"/", "#") : musicData.Album.ID)}";
                        string a;
                        if (musicData.Album.PicturePath != null)
                        {
                            a = musicData.Album.PicturePath;
                        }
                        else
                        {
                            a = await WebHelper.GetPicturePathAsync(musicData);
                        }

                        bool error = await DownloadPic(a, b);
                        if (!error) resultPath = b;
                    }
                }

                try
                {
                    source = await FileHelper.GetImageSource(resultPath, decodePixelWidth, decodePixelHeight, useBitmapImage);
                }
                finally
                {
                    LoadingImages.Remove(musicData);
                    loadNum--;
                }
            }

            return new(source, resultPath);
        }

        public static async Task<string> GetImageSource(DataEditor.MusicListData musicListData)
        {
            string cachePath = await Helpers.FileHelper.GetImageCache(musicListData);
            string resultPath = null;

            if (cachePath != null)
            {
                resultPath = cachePath;
            }
            else
            {
                if (Helpers.WebHelper.IsNetworkConnected)
                {
                    string b = $@"{DataEditor.DataFolderBase.ImageCacheFolder}\{musicListData.ListFrom}{musicListData.ListDataType}{musicListData.ID}";
                    await Task.Run(() =>
                    {
                        if (!System.IO.File.Exists(b))
                            System.IO.File.Create(b).Close();
                    });
                    await Helpers.WebHelper.DownloadFileAsync(musicListData.PicturePath, b);
                    resultPath = b;
                }
                else
                {
                    return "/Images/SugarAndSalt.jpg";
                }
            }

            return resultPath;
        }
    }
}
