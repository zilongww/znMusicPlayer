using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task<string> GetImageSource(DataEditor.MusicData musicData)
        {
            while (LoadingImages.Contains(musicData))
            {
                await Task.Delay(500);
            }

            while (loadNum > maxLoadNum)
            {
                await Task.Delay(500);
            }

            string cachePath = await Helpers.FileHelper.GetImageCache(musicData);
            string resultPath = null;

            if (cachePath != null)
            {
                resultPath = cachePath;
            }
            else
            {
                loadNum++;
                LoadingImages.Add(musicData);

                if (Helpers.WebHelper.IsNetworkConnected)
                {
                    //System.Diagnostics.Debug.WriteLine(musicData.AlbumID);
                    string b = $@"{DataEditor.DataFolderBase.ImageCacheFolder}\{musicData.From}{(string.IsNullOrEmpty(musicData.AlbumID) ? musicData.MD5.Replace(@"/", "#") : musicData.AlbumID)}";
                    await Task.Run(() =>
                    {
                        if (!System.IO.File.Exists(b))
                        {
                            try
                            {
                                System.IO.File.Create(b).Close();
                            }
                            catch { }
                        }
                    });

                    string a;
                    if (musicData.PicturePath != null)
                    {
                        a = musicData.PicturePath;
                    }
                    else
                    {
                        a = await Helpers.WebHelper.GetPicturePathAsync(musicData);
                    }

                    bool error = await DownloadPic(a, b);
                    if (error) resultPath = resultPath = null;//await GetImageSource(musicData);
                    else resultPath = b;
                }
                else
                {
                    resultPath = "/Images/SugarAndSalt.jpg";
                }

                LoadingImages.Remove(musicData);
                loadNum--;
            }

            return resultPath;
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
