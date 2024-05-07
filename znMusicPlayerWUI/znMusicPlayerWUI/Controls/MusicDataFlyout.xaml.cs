using System;
using System.IO;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.System;
using Windows.Storage;
using Windows.Foundation;
using znMusicPlayerWUI.Pages;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Controls
{
    public sealed partial class MusicDataFlyout : UserControl
    {
        public ArrayList arrayList { get; set; }
        SongItemBindBase songItemBind = null;
        public SongItemBindBase SongItemBind
        {
            get => songItemBind;
            set
            {
                songItemBind = value;
            }
        }

        public MusicDataFlyout()
        {
            InitializeComponent();
            //arrayList = new ArrayList(10000000);
        }

        void Init()
        {
            TitleTextblock.Text = songItemBind.MusicData.Title;
            AlbumItem.Text = $"ר����{songItemBind.MusicData.Album.Title}";
        }

        void InitFlyout()
        {
            if (songItemBind.MusicListData?.ListDataType == DataType.�赥 || songItemBind.MusicListData?.ListDataType == DataType.���ظ赥)
            {
                DeleteFromPlaylistItem.Visibility = Visibility.Visible;
            }
            else
            {
                DeleteFromPlaylistItem.Visibility = Visibility.Collapsed;
            }

            if (songItemBind.MusicData.From == MusicFrom.localMusic)
            {
                LinkItem.Visibility = Visibility.Collapsed;
                ExploreLocalFileItem.Visibility = Visibility.Visible;
            }
            else
            {
                LinkItem.Visibility = Visibility.Visible;
                ExploreLocalFileItem.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowAt(FrameworkElement element)
        {
            root.ShowAt(element);
        }
        
        public void ShowAt(UIElement element, Point point)
        {
            root.ShowAt(element, point);
        }
        
        public void ShowAt(DependencyObject element, FlyoutShowOptions flyoutShowOptions)
        {
            root.ShowAt(element, flyoutShowOptions);
        }

        private void root_Opened(object sender, object e)
        {
            Init();
            InitFlyout();
        }

        private void root_Closed(object sender, object e)
        {

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            songItemBind = null;
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            switch (menuFlyoutItem.Tag as string)
            {
                case "play":
                    await App.playingList.Play(songItemBind.MusicData, true);
                    break;
                case "addToPlayingList":
                    App.playingList.Add(songItemBind.MusicData);
                    break;
                case "setToNextPlay":
                    App.playingList.SetNextPlay(App.audioPlayer.MusicData, songItemBind.MusicData);
                    break;
                case "deleteFromPlaylist":
                    if (songItemBind.MusicListData.ListDataType == DataType.���ظ赥 || songItemBind.MusicListData.ListDataType == DataType.�赥)
                    {
                        await PlayListHelper.DeleteMusicDataFromPlayList(songItemBind.MusicListData.ListName, songItemBind.MusicData);
                        await App.playListReader.Refresh();
                    }
                    break;
                case "deleteFile":
                    if (songItemBind.MusicListData.ListDataType == DataType.���ظ赥 || songItemBind.MusicListData.ListDataType == DataType.�赥)
                    {
                        string deletePath = songItemBind.MusicData.InLocal;
                        await Task.Run(() => File.Delete(deletePath));
                        await PlayListHelper.DeleteMusicDataFromPlayList(songItemBind.MusicListData.ListName, songItemBind.MusicData);
                        await App.playListReader.Refresh();
                    }
                    break;
                case "album":
                    Pages.ListViewPages.ListViewPage.SetPageToListViewPage(songItemBind.MusicData.Album);
                    break;
                case "download":
                    App.downloadManager.Add(songItemBind.MusicData);
                    break;
                case "search_software":
                    MainWindow.SetNavViewContent(typeof(SearchPage), songItemBind.MusicData.Title);
                    break;
                case "search_websiteSearch":
                    await Launcher.LaunchUriAsync(new Uri($"https://www.bing.com/search?q={songItemBind.MusicData.Title}-{songItemBind.MusicData.Album}"));
                    break;
                case "search_website":
                    Uri uri = null;
                    switch (songItemBind.MusicData.From)
                    {
                        case MusicFrom.neteaseMusic:
                            uri = new($"https://music.163.com/#/song?id={songItemBind.MusicData.ID}");
                            break;
                    }

                    if (uri != null)
                    {
                        var success = await Launcher.LaunchUriAsync(uri);
                    }
                    break;
                case "search_copy":
                    var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
                    dp.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                    dp.SetText(songItemBind.MusicData.Title);
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
                    break;
                case "link":
                    var link = await App.metingServices.NeteaseServices.GetUrl(songItemBind.MusicData.ID, (int)DataFolderBase.DownloadQuality.lossless);
                    MainWindow.HideDialog();
                    await MainWindow.ShowDialog("��ȡ���������ǣ�", link);
                    break;
                case "exploreLocalFile":
                    var seletFile = new FolderLauncherOptions();
                    seletFile.ItemsToSelect.Add(await StorageFile.GetFileFromPathAsync(songItemBind.MusicData.InLocal));
                    await Launcher.LaunchFolderPathAsync(new FileInfo(songItemBind.MusicData.InLocal).DirectoryName, seletFile);
                    break;
                case "openWithOtherSoftware":
                    await Launcher.LaunchUriAsync(new Uri(songItemBind.MusicData.InLocal), new() { DisplayApplicationPicker = true });
                    break;
                case "cache":
                    if (await App.cacheManager.GetCachePath(songItemBind.MusicData) is not null)
                    {
                        MainWindow.AddNotify($"�˸����ѻ��棡", null, NotifySeverity.Warning);
                        return;
                    }
                    item = MainWindow.AddNotify($"���ڻ��棺{songItemBind.MusicData.Title}", "������...", NotifySeverity.Loading, TimeSpan.MaxValue);
                    App.cacheManager.CachingStateChangeMusicData -= CacheManager_CachingStateChangeMusicData;
                    App.cacheManager.CachingStateChangeMusicData += CacheManager_CachingStateChangeMusicData;
                    App.cacheManager.CachedMusicData -= CacheManager_CachedMusicData;
                    App.cacheManager.CachedMusicData += CacheManager_CachedMusicData;
                    await App.cacheManager.StartCacheMusic(songItemBind.MusicData);
                    break;
                case "cacheDelete":
                    var path = await App.cacheManager.GetCachePath(songItemBind.MusicData);
                    if (string.IsNullOrEmpty(path))
                    {
                        MainWindow.AddNotify("�˸����Ļ����ļ������ڡ�", null, NotifySeverity.Error);
                        return;
                    }

                    var itema = MainWindow.AddNotify($"����ɾ����{songItemBind.MusicData.Title}", null, NotifySeverity.Loading, TimeSpan.MaxValue);
                    Exception err = null;
                    try
                    {
                        await Task.Run(() => File.Delete(path));
                    }
                    catch (Exception ex)
                    {
                        err = ex;
                        itema.SetNotifyItemData("ɾ��ʧ�ܡ�", null, NotifySeverity.Error);
                    }
                    if (err == null)
                    {
                        itema.SetNotifyItemData("ɾ���ɹ���", null, NotifySeverity.Complete);
                    }
                    MainWindow.NotifyCountDown(itema);
                    break;
                case "info":
                    await MainWindow.ShowDialog($"{songItemBind.MusicData.Title} ����ϸ��Ϣ��", $"���⣺{songItemBind.MusicData.Title}\n������&ר����{songItemBind.MusicData.ButtonName}\nID��{songItemBind.MusicData.ID}\n��Դ��{songItemBind.MusicData.From}\nͼƬ��ַ��{songItemBind.MusicData.Album.PicturePath}");
                    break;
            }
        }

        NotifyItem item = null;
        private void CacheManager_CachingStateChangeMusicData(MusicData musicData, object value)
        {
            if (musicData != songItemBind.MusicData) return;
            item.SetProcess(100, (int)value);
            item.SetNotifyItemData(item.GetNotifyItemData().Title, $"{value}%", NotifySeverity.Loading);
        }

        private void CacheManager_CachedMusicData(MusicData musicData, object value)
        {
            if (musicData != songItemBind.MusicData) return;
            App.cacheManager.CachingStateChangeMusicData -= CacheManager_CachingStateChangeMusicData;
            App.cacheManager.CachedMusicData -= CacheManager_CachedMusicData;
            item.SetNotifyItemData(item.GetNotifyItemData().Title, "������ɡ�", NotifySeverity.Complete);
            MainWindow.NotifyCountDown(item);
            item = null;
        }

        private async void AddToPlayListSubItems_Loaded(object sender, RoutedEventArgs e)
        {
            return;
            AddToPlayListSubItems.Items.Clear();
            var mls = await PlayListHelper.ReadAllPlayList();
            foreach (var item in mls)
            {
                var menuItem = new MenuFlyoutItem()
                {
                    Text = item.ListShowName,
                    Tag = item
                };
                menuItem.Click += MenuItem_Click;
                AddToPlayListSubItems.Items.Add(menuItem);
            }
        }

        private void AddToPlayListSubItems_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (MenuFlyoutItem item in AddToPlayListSubItems.Items)
            {
                item.Tag = null;
                item.Click -= MenuItem_Click;
            }
            AddToPlayListSubItems.Items.Clear();
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            await PlayListHelper.AddMusicDataToPlayList(((sender as FrameworkElement).Tag as MusicListData).ListName, songItemBind.MusicData);
        }

        private void ArtistItem_Loaded(object sender, RoutedEventArgs e)
        {
            return;
            ArtistItem.Items.Clear();
            foreach (var artist in songItemBind.MusicData.Artists)
            {
                var mfi = new MenuFlyoutItem()
                {
                    Text = artist.Name,
                    Tag = artist
                };
                mfi.Click += Mfi_Click;
                ArtistItem.Items.Add(mfi);
            }

        }

        private void ArtistItem_Unloaded(object sender, RoutedEventArgs e)
        {
            foreach (MenuFlyoutItem item in ArtistItem.Items)
            {
                item.Tag = null;
                item.Click -= Mfi_Click;
            }
            ArtistItem.Items.Clear();
        }

        private void Mfi_Click(object sender, RoutedEventArgs e)
        {
            Pages.ListViewPages.ListViewPage.SetPageToListViewPage((sender as FrameworkElement).Tag as Artist);
        }
    }
}
