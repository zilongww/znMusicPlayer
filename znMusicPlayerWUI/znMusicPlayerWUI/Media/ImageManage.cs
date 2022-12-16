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

        public static async Task<string> GetImageSource(DataEditor.MusicData musicData)
        {
            while (LoadingImages.Contains(musicData))
            {
                await Task.Delay(500);
            }

            LoadingImages.Add(musicData);
            string cachePath = await Helpers.FileHelper.GetImageCache(musicData);
            string resultPath = null;

            if (cachePath != null)
            {
                resultPath = cachePath;
            }
            else
            {
                if (Helpers.WebHelper.IsNetworkConnected)
                {
                    string b = $@"{DataEditor.DataFolderBase.ImageCacheFolder}\{musicData.From}{(string.IsNullOrEmpty(musicData.AlbumID) ? musicData.MD5 : musicData.AlbumID)}";
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

                    try
                    {
                        await Helpers.WebHelper.DownloadFileAsync(a, b);
                    }
                    catch { }

                    bool error = await Task.Run(() =>
                    {
                        if (System.IO.File.Exists(b))
                        {
                            if (System.IO.File.ReadAllBytes(b).Length == 0)
                            {
                                System.IO.File.Delete(b);
                                return true;
                            }
                        }
                        return false;
                    });

                    if (error) resultPath = resultPath = null;//await GetImageSource(musicData);
                    else resultPath = b;
                }
                else
                {
                    resultPath = "/Images/SugarAndSalt.jpg";
                }
            }

            LoadingImages.Remove(musicData);
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
