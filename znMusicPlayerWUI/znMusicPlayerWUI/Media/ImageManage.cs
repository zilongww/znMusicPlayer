using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Media;
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

        public static Dictionary<string, ImageSource> localImageCache = new();
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
            resultPath = await FileHelper.GetImageCache(musicData);

            if (musicData.From == DataEditor.MusicFrom.localMusic)
            {
                if (string.IsNullOrEmpty(resultPath))
                {
                    var imageByte = await CodeHelper.GetLocalImageByte(musicData);
                    if (imageByte != null)
                    {
                        string b = $@"{DataEditor.DataFolderBase.ImageCacheFolder}\{musicData.From}{musicData.MD5.Replace(@"/", "#")}";
                        await Task.Run(() =>
                        {
                            var f = File.Create(b);
                            f.Write(imageByte);
                            f.Close();
                            f.Dispose();
                        });
                        source = await FileHelper.GetImageSource(b, decodePixelWidth, decodePixelHeight, useBitmapImage);
                    }
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
                            source = await FileHelper.GetImageSource(coverPath, decodePixelWidth, decodePixelHeight, useBitmapImage);
                        }
                    }
                }
                else
                {
                    source = await FileHelper.GetImageSource(resultPath, decodePixelWidth, decodePixelHeight, useBitmapImage);
                }
            }
            else
            {
                while (LoadingImages.Contains(musicData))
                {
                    await Task.Delay(1000);
                }
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
                        string b = $@"{DataEditor.DataFolderBase.ImageCacheFolder}\{musicData.From}{(string.IsNullOrEmpty(musicData.Album?.ID) ? musicData.MD5.Replace(@"/", "#") : musicData.Album.ID)}";
                        string a;
                        if (musicData.Album?.PicturePath != null)
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
            if (musicListData == null) return null;
            string cachePath = await FileHelper.GetImageCache(musicListData);
            string resultPath = null;

            if (cachePath != null)
            {
                resultPath = cachePath;
            }
            else
            {
                if (WebHelper.IsNetworkConnected)
                {
                    string b = $@"{DataEditor.DataFolderBase.ImageCacheFolder}\{musicListData.ListFrom}{musicListData.ListDataType}{musicListData.ID}";
                    await Task.Run(() =>
                    {
                        if (!File.Exists(b))
                            File.Create(b).Close();
                    });
                    await WebHelper.DownloadFileAsync(musicListData.PicturePath, b);
                    resultPath = b;
                }
                else
                {
                    return "/Images/icon.png";
                }
            }

            return resultPath;
        }
    }
}
