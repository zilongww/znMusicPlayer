using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Media;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Pages.DialogPages
{
    public partial class AddPlayListPage : Page
    {
        delegate void ResultDelegate(ContentDialogResult contentDialogResult);
        static event ResultDelegate ResultEvent;

        public AddPlayListPage()
        {
            InitializeComponent();

            var b = Enum.GetNames(typeof(MusicFrom)).ToList();
            AddOutSidePage_PlatfromCb.ItemsSource = b;
            b.RemoveAt(6);
            b.RemoveAt(5);
            AddOutSidePage_PlatfromCb.SelectedIndex = 3;

            ResultEvent += AddPlayListPage_ResultEvent;
        }

        private async void AddPlayListPage_ResultEvent(ContentDialogResult contentDialogResult)
        {
            if (contentDialogResult == ContentDialogResult.Primary)
            {
                MainWindow.ShowLoadingDialog();
                MusicListData musicListData = null;

                if (PivotList.SelectedItem == PivotList.Items[0])
                {
                    musicListData = new MusicListData(null, AddLocalPage_ListNameTB.Text, AddLocalPage_ListImageTB.Text, MusicFrom.localMusic);
                    musicListData.ListName = musicListData.MD5;
                    musicListData.ListFrom = MusicFrom.localMusic;
                    musicListData.ListDataType = DataType.本地歌单;
                    musicListData.ReMD5();
                }
                else
                {
                    try
                    {
                        var pl = await App.metingServices.NeteaseServices.GetPlayList(AddOutSidePage_IDTb.Text);
                        if (pl != null)
                        {
                            musicListData = pl;
                        }
                    }
                    catch
                    {
#if DEBUG
                        Debug.WriteLine("error");
#endif
                    }
                }

                if (musicListData != null)
                {
                    try
                    {
                        await PlayListHelper.AddPlayList(musicListData);
                        await App.playListReader.Refresh();
                    }
                    catch (Exception err)
                    {
                        //var b = await MainWindow.ShowDialog("添加播放列表时出现错误", err.Message, "确定", "重试");
                        //if (b == ContentDialogResult.Primary)
                        //{
                        //    AddPlayListPage_ResultEvent(contentDialogResult);
                        //}
                    }
                }

                MainWindow.HideDialog();
            }
            ResultEvent -= AddPlayListPage_ResultEvent;
        }

        public static async Task ShowDialog()
        {
            var a = await MainWindow.ShowDialog("", new AddPlayListPage(), "取消", "创建");
            ResultEvent?.Invoke(a);
        }

        private async void AddLocalPage_ListImageSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            var a = await FileHelper.UserSelectFile(Windows.Storage.Pickers.PickerViewMode.List, Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            if (a != null)
                AddLocalPage_ListImageTB.Text = a.Path;
        }
    }
}
