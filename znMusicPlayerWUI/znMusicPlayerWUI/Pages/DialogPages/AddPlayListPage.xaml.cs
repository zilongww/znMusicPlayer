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

        NavigationView navigationView = null;
        public AddPlayListPage()
        {
            InitializeComponent();
            var a = (VisualTreeHelper.GetChild(this, 0) as Grid).Children[0] as NavigationView;
            navigationView = a;
            a.SelectedItem = a.MenuItems[0];

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
                MusicListData musicListData = null;

                if (navigationView.SelectedItem == navigationView.MenuItems[0])
                {
                    musicListData = new MusicListData(null, AddLocalPage_ListNameTB.Text, AddLocalPage_ListImageTB.Text, MusicFrom.localMusic);
                    musicListData.ListName = musicListData.MD5;
                    musicListData.ListFrom = MusicFrom.localMusic;
                    musicListData.ListDataType = DataType.LocalPlayList;
                    musicListData.ReMD5();
                }
                else
                {
                    try
                    {
                        var pl = await MetingService.GetPlayList(AddOutSidePage_IDTb.Text);
                        if (pl != null)
                        {
                            musicListData = pl;
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("error");
                    }
                }

                if (musicListData != null)
                {
                    try
                    {
                        await PlayListHelper.AddPlayList(musicListData);
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
            }
            ResultEvent -= AddPlayListPage_ResultEvent;
        }

        public static async Task ShowDialog()
        {
            var a = await MainWindow.ShowDialog("添加新的播放列表", new AddPlayListPage(), "取消", "创建");
            ResultEvent?.Invoke(a);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (navigationView.SelectedItem != navigationView.MenuItems[0])
            {
                AddLocalPage.Visibility = Visibility.Collapsed;
                AddOutSidePage.Visibility = Visibility.Visible;
            }
            else
            {
                AddLocalPage.Visibility = Visibility.Visible;
                AddOutSidePage.Visibility = Visibility.Collapsed;
            }
        }

        private async void AddLocalPage_ListImageSelectBtn_Click(object sender, RoutedEventArgs e)
        {
            var a = await FileHelper.UserSelectFile(Windows.Storage.Pickers.PickerViewMode.List, Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            if (a != null)
                AddLocalPage_ListImageTB.Text = a.Path;
        }
    }
}
