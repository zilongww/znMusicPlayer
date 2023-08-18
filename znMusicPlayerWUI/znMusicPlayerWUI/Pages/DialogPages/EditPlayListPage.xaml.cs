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
    public partial class EditPlayListPage : Page
    {
        delegate void ResultDelegate(ContentDialogResult contentDialogResult);
        static event ResultDelegate ResultEvent;

        public MusicListData MusicListData { get; set; }
        public EditPlayListPage()
        {
            InitializeComponent();
            ResultEvent += AddPlayListPage_ResultEvent;
        }

        private async void AddPlayListPage_ResultEvent(ContentDialogResult contentDialogResult)
        {
            if (contentDialogResult == ContentDialogResult.Primary)
            {
                var data = await PlayListHelper.ReadData();
                data[MusicListData.ListName]["ListShowName"] = Name_TB.Text;
                data[MusicListData.ListName]["PicturePath"] = ImagePath_TB.Text;
                await PlayListHelper.SaveData(data);
                await App.playListReader.Refresh();
            }
            ResultEvent -= AddPlayListPage_ResultEvent;
        }

        public static async Task ShowDialog(MusicListData musicListData)
        {
            var a = await MainWindow.ShowDialog($"编辑 \"{musicListData.ListShowName}\"", new EditPlayListPage() { MusicListData = musicListData }, "取消", "更改", defaultButton: ContentDialogButton.Primary);
            ResultEvent?.Invoke(a);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var a = await FileHelper.UserSelectFile(Windows.Storage.Pickers.PickerViewMode.List, Windows.Storage.Pickers.PickerLocationId.PicturesLibrary);
            if (a != null)
                ImagePath_TB.Text = a.Path;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Name_TB.Text = MusicListData.ListShowName;
            ImagePath_TB.Text = MusicListData.PicturePath;
        }
    }
}
