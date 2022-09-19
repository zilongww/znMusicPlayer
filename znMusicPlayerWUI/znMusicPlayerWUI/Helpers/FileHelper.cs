using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;

namespace znMusicPlayerWUI.Helpers
{
    public static class FileHelper
    {
        /// <summary>
        /// 查询或获取音频缓存文件路径
        /// </summary>
        /// <param name="musicData"></param>
        /// <returns>
        /// !=null - 音频缓存文件路径 /
        /// null - 未查询到音频缓存
        /// </returns>
        public static async Task<string> GetAudioCache(DataEditor.MusicData musicData)
        {
            return await Task.Run(() =>
            {
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(DataEditor.DataFolderBase.AudioCacheFolder);
                System.IO.FileInfo[] fileInfo = directory.GetFiles();
                foreach (System.IO.FileInfo file in fileInfo)
                {
                    string name = file.Name.Split('.')[0];
                    if (name == musicData.From + musicData.ID)
                    {
                        return file.FullName;
                    }
                }
                return null;
            });
        }
        
        /// <summary>
        /// 查询或获取图片缓存文件路径
        /// </summary>
        /// <param name="musicData"></param>
        /// <returns>
        /// !=null - 图片缓存文件路径 /
        /// null - 未查询到图片缓存
        /// </returns>
        public static async Task<string> GetImageCache(string fileName)
        {
            return await Task.Run(() =>
            {
                DirectoryInfo directory = new DirectoryInfo(DataEditor.DataFolderBase.ImageCacheFolder);
                FileInfo[] fileInfo = directory.GetFiles();
                foreach (FileInfo file in fileInfo)
                {
                    if (file.Name == fileName)
                    {
                        return file.FullName;
                    }
                }
                return null;
            });
        }
        
        /// <summary>
        /// 查询或获取图片缓存文件路径
        /// </summary>
        /// <param name="musicData"></param>
        /// <returns>
        /// !=null - 图片缓存文件路径 /
        /// null - 未查询到图片缓存
        /// </returns>
        public static async Task<string> GetImageCache(DataEditor.MusicData musicData)
        {
            return await GetImageCache(musicData.From + musicData.AlbumID);
        }
        
        /// <summary>
        /// 查询或获取图片缓存文件路径
        /// </summary>
        /// <param name="musicData"></param>
        /// <returns>
        /// !=null - 图片缓存文件路径 /
        /// null - 未查询到图片缓存
        /// </returns>
        public static async Task<string> GetImageCache(DataEditor.MusicListData musicListData)
        {
            return await GetImageCache(musicListData.ListFrom.ToString() + musicListData.ListDataType.ToString() + musicListData.ID);
        }
        
        /// <summary>
        /// 查询或获取歌词缓存文件路径
        /// </summary>
        /// <param name="musicData"></param>
        /// <returns>
        /// !=null - 歌词缓存文件路径 /
        /// null - 未查询到歌词缓存
        /// </returns>
        public static async Task<string> GetLyricCache(DataEditor.MusicData musicData)
        {
            return await Task.Run(() =>
            {
                System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(DataEditor.DataFolderBase.LyricCacheFolder);
                System.IO.FileInfo[] fileInfo = directory.GetFiles();
                foreach (System.IO.FileInfo file in fileInfo)
                {
                    if (file.Name == musicData.From + musicData.ID)
                    {
                        return file.FullName;
                    }
                }
                return null;
            });
        }

        public static BitmapImage GetImageFileBitmapImage(string filePath, int decodePixelWidth = 0, int decodePixelHeight = 0)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = @"ms-appx:///Images/SugarAndSalt.jpg";
            }
            return new BitmapImage(new Uri(filePath)) { DecodePixelWidth = decodePixelWidth, DecodePixelHeight = decodePixelHeight };
        }

        public static BitmapImage GetImageFileBitmapImage(Uri fileUri, int decodePixelWidth = 0, int decodePixelHeight = 0)
        {
            return new BitmapImage(fileUri) { DecodePixelWidth = decodePixelWidth, DecodePixelHeight = decodePixelHeight };
        }

        public static async Task<IReadOnlyList<Windows.Storage.StorageFile>> UserSelectFiles(
            PickerViewMode viewMode = default,
            PickerLocationId suggestedStartLocation = default,
            string[] fileTypeFilter = null)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = viewMode;
            picker.SuggestedStartLocation = suggestedStartLocation;

            if (fileTypeFilter == null) fileTypeFilter = new string[] { "*" };
            foreach (var i in fileTypeFilter)
            {
                picker.FileTypeFilter.Add(i);
            }

            WinRT.Interop.InitializeWithWindow.Initialize(picker, App.AppWindowLocalHandle);
            var files = await picker.PickMultipleFilesAsync();
            return files;
        }
        
        public static async Task<Windows.Storage.StorageFile> UserSelectFile(
            PickerViewMode viewMode = default,
            PickerLocationId suggestedStartLocation = default,
            string[] fileTypeFilter = null)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = viewMode;
            picker.SuggestedStartLocation = suggestedStartLocation;

            if (fileTypeFilter == null) fileTypeFilter = new string[] { "*" };
            foreach (var i in fileTypeFilter)
            {
                picker.FileTypeFilter.Add(i);
            }

            WinRT.Interop.InitializeWithWindow.Initialize(picker, App.AppWindowLocalHandle);
            var files = await picker.PickSingleFileAsync();
            return files;
        }

        public static async Task<Windows.Storage.StorageFolder> UserSelectFolder(PickerLocationId suggestedStartLocation = default)
        {
            var folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = suggestedStartLocation;

            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, App.AppWindowLocalHandle);
            Windows.Storage.StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            return folder;
        }
        public static async Task<string> FileTypeGet(string name)
        {
            return await Task.Run(() =>
            {
                FileStream fs = new FileStream(@name, FileMode.Open, FileAccess.Read);
                byte[] imagebytes = new byte[fs.Length];
                BinaryReader br = new BinaryReader(fs);
                imagebytes = br.ReadBytes(2);
                string ss = "";
                for (int i = 0; i < imagebytes.Length; i++)
                {
                    ss += imagebytes[i];
                }
                return ss;
            });
        }
    }
}
