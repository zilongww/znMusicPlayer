using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using Microsoft.UI.Xaml.Media;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Pages;
using System.Security.Cryptography;
using System.Diagnostics;
using Windows.System;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Hosting;
using Windows.Storage;
using znMusicPlayerWUI.Media;
using static Vanara.PInvoke.Kernel32;

namespace znMusicPlayerWUI.Controls
{
    public partial class SongItem : Grid, IDisposable
    {
        bool _ShowImage = true;

        public bool AutoLoadImage = false;
        public bool CanClickPlay { get; set; } = true;

        public static readonly DependencyProperty MusicDataProperty = DependencyProperty.Register(
            "MusicData",
            typeof(MusicData),
            typeof(SongItem),
            new PropertyMetadata(null, new(OnMusicDataChanged))
        );

        public MusicData MusicData
        {
            get => (MusicData)GetValue(MusicDataProperty);
            private set
            {
                SetValue(MusicDataProperty, value);
            }
        }
        MusicListData musicListData;
        public MusicListData MusicListData
        {
            get => musicListData;
            set
            {
                musicListData = value;
            }
        }

        SongItemBindBase musicItemBindBase;

        public double ImageScaleDPI { get; set; } = 1.0;
        public bool ShowImage
        {
            get { return _ShowImage; }
            set
            {
                _ShowImage = value;
                AlbumImage_BaseGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        bool _isMusicDataPlaying = false;
        public bool IsMusicDataPlaying
        {
            get
            {
                return _isMusicDataPlaying;
            }
            set
            {
                _isMusicDataPlaying = value;
                DelaySetIconRootVisibility(value);
            }
        }

        bool? lastValue = null;
        private async void DelaySetIconRootVisibility(bool value)
        {
            if (lastValue != null)
                if ((bool)lastValue == value) return;
            lastValue = value;

            if (MainWindow.DriveInType != Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                InfoButton.Visibility = Visibility.Collapsed;
                RightToolBar.Opacity = 1;
                RightToolBar.Visibility = Visibility.Visible;
                if (value) RightToolBar.Children[0].Visibility = Visibility.Visible;
                else RightToolBar.Children[0].Visibility = Visibility.Collapsed;
                if (!ShowImage)
                    BaseGrid.Padding = new(10, 16, 10, 16);
            }
            else
            {
                InfoButton.Visibility = Visibility.Visible;
                RightToolBar.Visibility = Visibility.Collapsed;
                RightToolBar.Children[0].Visibility = Visibility.Visible;
            }

            if (value)
            {
                SetPlayingIcon(App.audioPlayer.PlaybackState);
                App.audioPlayer.PlayStateChanged += AudioPlayer_PlayStateChanged;
                PlayingThemeRectangle.Opacity = 1;
                ShowRightToolBar();
                AnimatedMouseEnterBackground();
                RightToolBar.Visibility = Visibility.Visible;
            }
            else
            {
                SetPlayingIcon(NAudio.Wave.PlaybackState.Paused);
                App.audioPlayer.PlayStateChanged -= AudioPlayer_PlayStateChanged;
                PlayingThemeRectangle.Opacity = 0;
                backgroundBaseGridVisual.Opacity = 0;
                RightToolBar.Visibility = Visibility.Collapsed;
            }

            if (MainWindow.DriveInType != Microsoft.UI.Input.PointerDeviceType.Mouse)
                RightToolBar.Visibility = Visibility.Visible;

            /*
            if (value)
            {
                PlayingText.Text = "正在播放";
                //PlayingIconRoot.Visibility = Visibility.Visible;
                //AlbumImage.CornerRadius = new(8);
            }
            else
            {
                PlayingText.Text = null;
                //PlayingIconRoot.Visibility = Visibility.Collapsed;
                //AlbumImage.CornerRadius = new(0);
            }*/
        }

        private void AudioPlayer_PlayStateChanged(Media.AudioPlayer audioPlayer)
        {
            SetPlayingIcon(audioPlayer.PlaybackState);
        }

        private void SetPlayingIcon(NAudio.Wave.PlaybackState playbackState)
        {
            if (playbackState == NAudio.Wave.PlaybackState.Playing)
            {
                PlayingButtonIcon.Glyph = "\xE769";
            }
            else
            {
                PlayingButtonIcon.Glyph = "\xE768";
            }
        }

        Visual backgroundBaseGridVisual;
        Visual rightToolBarVisual;
        public SongItem()
        {
            InitializeComponent();
            backgroundBaseGridVisual = ElementCompositionPreview.GetElementVisual(BackgroundBaseGrid);
            rightToolBarVisual = ElementCompositionPreview.GetElementVisual(RightToolBar);

            AddUnloadedEvent();
            DataContextChanged += SongItem_DataContextChanged;
            //ShowImage = false;
        }

        private void SongItem_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                rmf.ShowAt(this);
            }
        }

        private void SongItem_LostFocus(object sender, RoutedEventArgs e)
        {
            //RightToolBar.Visibility = Visibility.Collapsed;
        }

        private void SongItem_GotFocus(object sender, RoutedEventArgs e)
        {
            //RightToolBar.Visibility = Visibility.Visible;
        }

        private void SongItem_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext == null) return;
            var d = DataContext as SongItemBindBase;
            if (d == null) return;
            if (d.MusicData == null) return;
            if (musicItemBindBase?.MusicData == d.MusicData) return;
            musicItemBindBase = d;
            Init(musicItemBindBase);
        }

        public void Init(SongItemBindBase bindBase)
        {
            MusicData = bindBase.MusicData;
            musicListData = bindBase.MusicListData;
            ImageScaleDPI = bindBase.ImageScaleDPI;

            UpdateFlyoutMenuContext(bindBase.MusicData);
            if (MusicData.From == MusicFrom.localMusic)
            {
                TestFileExists();
            }
            else
            {
                UpdateImageInterface(bindBase.MusicData);
            }

            if (!bindBase.ShowAlbumName)
            {
                ButtonNameTextBlock.Text = bindBase.MusicData.ArtistName;
            }

            IsMusicDataPlaying = App.audioPlayer.MusicData == MusicData;
        }

        public async void TestFileExists()
        {
            string local = MusicData.InLocal;
            bool isExists = await Task.Run(() => File.Exists(local));
            if (!isExists)
            {
                FileNotExistsRoot.Visibility = Visibility.Visible;
                return;
            }

            FileNotExistsRoot.Visibility = Visibility.Collapsed;
            UpdateImageInterface(MusicData);
        }

        public void UpdateFlyoutMenuContext(MusicData musicData)
        {
            if (MusicListData?.ListDataType == DataType.歌单 || MusicListData?.ListDataType == DataType.本地歌单)
            {
                DeleteFlyoutBtn.Visibility = Visibility.Visible;
            }
            else
            {
                DeleteFlyoutBtn.Visibility = Visibility.Collapsed;
            }

            if (musicData.From != MusicFrom.localMusic)
            {
                Menuflyout_DownloadItem.Visibility = Visibility.Visible;
                MenuFlyout_BrowseSiteItem.Visibility = Visibility.Visible;
                MenuFlyout_GetUriItem.Visibility = Visibility.Visible;
                Menuflyout_CacheItem.Visibility = Visibility.Visible;
                MenuFlyout_BrowseFileItem.Visibility = Visibility.Collapsed;
                MenuFlyout_OpenFileItem.Visibility = Visibility.Collapsed;
            }
            else
            {
                Menuflyout_DownloadItem.Visibility = Visibility.Collapsed;
                MenuFlyout_BrowseSiteItem.Visibility = Visibility.Collapsed;
                MenuFlyout_GetUriItem.Visibility = Visibility.Collapsed;
                Menuflyout_CacheItem.Visibility = Visibility.Collapsed;
                MenuFlyout_BrowseFileItem.Visibility = Visibility.Visible;
                MenuFlyout_OpenFileItem.Visibility = Visibility.Visible;
            }
        }

        int showCount = 0;
        public async void UpdateImageInterface(MusicData musicData)
        {
            if (isDisposed) return;
            if (AlbumImage.Source != null)
            {
                AlbumImage.Dispose();
            }

            showCount++;
            await Task.Delay(200);
            showCount--;
            if (showCount != 0) return;

            if (MusicListData?.ListDataType == DataType.专辑)
            {
                ShowImage = false;
                MainWindow_DriveInTypeEvent(MainWindow.DriveInType);
                return;
            }

            MusicData data = musicData;
            ImageSource a = null;
            try
            {
                bool err = false;
                if (musicData.From == MusicFrom.localMusic)
                {
                    if (Path.GetExtension(musicData.InLocal) == ".mid")
                    {
                        a = null;
                        err = true;
                    }
                }
                /* 性能低下
                if (musicData.From == MusicFrom.localMusic)
                {
                    if (await FileHelper.FileTypeGetAsync(musicData.InLocal) == "7784")
                    {
                        a = null;
                        err = true;
                    }
                }*/

                if (!err)
                {
                    var b = await Media.ImageManage.GetImageSource(musicData, (int)(56 * ImageScaleDPI), (int)(56 * ImageScaleDPI), true);
                    a = b.Item1;
                }
            }
            catch { }
            
            if (isDisposed) return;
            if (musicData == MusicData)
            {
                if (a != null)
                {
                    ShowImage = true;
                    AlbumImage.Source = a;
                }
                else
                {
                    ShowImage = false;
                }
            }
            MainWindow_DriveInTypeEvent(MainWindow.DriveInType);
        }

        public static void OnMusicDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as SongItem;
            var md = e.NewValue as MusicData;
            if (element == null || md == null) return;
            
            element.mfi.Text = $"专辑：{md.Album}";
            element.rmfi.Text = $"专辑：{md.Album}";
        }

        #region Events
        private void A_Click(object sender, RoutedEventArgs e)
        {
            Pages.ListViewPages.ListViewPage.SetPageToListViewPage<ItemListViewArtist>((sender as MenuFlyoutItem).Tag);

            //var artist = await App.metingServices.NeteaseServices.GetArtist(((Artist)(sender as MenuFlyoutItem).Tag).ID);
            //await MainWindow.ShowDialog("result", $"{artist.Name}\n{artist.PicturePath}\n{artist.Describee}\n{artist.HotSongs.Songs.Count}");
        }

        bool isDisposed = false;
        public void Dispose()
        {
            try
            {
                App.audioPlayer.PlayStateChanged -= AudioPlayer_PlayStateChanged;
                if (!isDisposed) { return; }
                DataContext = null;
                AlbumImage?.Dispose();
                MusicData = null;
                UnloadObject(this);
                isDisposed = true;
            }
            catch (Exception err)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(err.ToString());
#endif
            }
        }

        // 当点击类型不是鼠标时为其添加间距以方便点击
        private void MainWindow_DriveInTypeEvent(Microsoft.UI.Input.PointerDeviceType deviceType)
        {
        }

        private void Button_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            rmf.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private void Grid_Holding(object sender, Microsoft.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            rmf.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        bool isShowRightToolBar = false;
        // 鼠标进入时改变颜色
        public void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                AnimatedMouseEnterBackground();
                ShowRightToolBar();
            }
        }

        public void ShowRightToolBar()
        {
            isShowRightToolBar = true;
            RightToolBar.Visibility = Visibility.Visible;
            AnimateHelper.AnimateScalar(rightToolBarVisual, 1, 0.1,
                0, 0, 0, 0,
                out var compositor, out var animation);
            rightToolBarVisual.StartAnimation(nameof(rightToolBarVisual.Opacity), animation);
        }

        public void HideRightToolBar()
        {
            if (IsMusicDataPlaying) return;

            isShowRightToolBar = false;

            AnimateHelper.AnimateScalar(rightToolBarVisual, 0, 0.1,
                0, 0, 0, 0,
                out var compositor, out var animation);
            compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed += (_, __) =>
            {
                if (!isShowRightToolBar)
                {
                    RightToolBar.Visibility = Visibility.Collapsed;
                }
            };
            rightToolBarVisual.StartAnimation(nameof(rightToolBarVisual.Opacity), animation);
        }

        // 鼠标离开时改变颜色
        private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                AnimateMouseLeavingBackground();
                HideRightToolBar();
            }
        }

        public void AnimatedMouseEnterBackground()
        {
            AnimateHelper.AnimateScalar(backgroundBaseGridVisual,
                                        1, 0.1,
                                        0, 0, 0, 0,
                                        out var compositor, out var animation);
            backgroundBaseGridVisual.StartAnimation(
                nameof(backgroundBaseGridVisual.Opacity), animation);
        }

        public void AnimateMouseLeavingBackground(bool opacityStartAtHeighest = false)
        {
            if (IsMusicDataPlaying) return;
            AnimateHelper.AnimateScalar(backgroundBaseGridVisual,
                0, opacityStartAtHeighest ? 3.5 : 0.1,
                0, 0, 0, 0,
                out var compositor, out var animation);
            if (opacityStartAtHeighest) backgroundBaseGridVisual.Opacity = 1;
            backgroundBaseGridVisual.StartAnimation(nameof(backgroundBaseGridVisual.Opacity), animation);
        }

        private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                //RightToolBar.Visibility = Visibility.Visible;
            }
        }

        private void Grid_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //Grid_PointerExited(null, null);
        }

        // 右键菜单
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            rmf.ShowAt(sender as FrameworkElement);
        }

        // 单击播放按钮
        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            if (!CanClickPlay) return;
            if (IsMusicDataPlaying)
            {
                if (App.audioPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                    App.audioPlayer.SetPause();
                else
                    App.audioPlayer.SetPlay();
            }
            else
                await App.playingList.Play(MusicData, true);
        }
        
        // 单击添加到播放中列表按钮
        private void AddPlay_Click(object sender, RoutedEventArgs e)
        {
            App.playingList.Add(MusicData);
        }
        
        // 单击下一首播放按钮
        private void NextPlay_Click(object sender, RoutedEventArgs e)
        {
            App.playingList.SetNextPlay(App.audioPlayer.MusicData, MusicData);
        }
        
        // 单击详细信息按钮
        private async void Info_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.ShowDialog($"{MusicData.Title} 的详细信息：", $"标题：{MusicData.Title}\n艺术家&专辑：{MusicData.ButtonName}\nID：{MusicData.ID}\n来源：{MusicData.From}\n图片地址：{MusicData.Album.PicturePath}");
        }

        // 双击元素 播放
        private void Grid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                Play_Click(null, null);
            }
        }

        // 除鼠标外的按下事件 单击播放
        private void Grid_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                Play_Click(null, null);
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            App.downloadManager.Add(MusicData);
        }

        private async void rmf_Opened(object sender, object e)
        {
        }

        private void rmf_Closed(object sender, object e)
        {
            AddToPlayListSubItems.Items.Clear();
        }

        private async void Add_Click(object sender, RoutedEventArgs e)
        {
            await PlayListHelper.AddMusicDataToPlayList(((sender as FrameworkElement).Tag as MusicListData).ListName, MusicData);
        }

        private async void DeleteFlyoutBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MusicListData.ListDataType == DataType.本地歌单 || MusicListData.ListDataType == DataType.歌单)
            {
                if ((sender as FrameworkElement).Tag as string == "1")
                {
                    string path = MusicData.InLocal;
                    await Task.Run(() => File.Delete(path));
                }
                await PlayListHelper.DeleteMusicDataFromPlayList(MusicListData.ListName, MusicData);
                await App.playListReader.Refresh();
            }
        }

        public static List<SongItem> StaticSongItems = new();
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            StaticSongItems.Add(this);
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            StaticSongItems.Remove(this);
            Dispose();
        }

        public void AddUnloadedEvent()
        {
            MoveSymbol.Visibility = Visibility.Collapsed;
            Unloaded += Grid_Unloaded;
        }

        public void RemoveUnloadedEvent()
        {
            MoveSymbol.Visibility = Visibility.Visible;
            Unloaded -= Grid_Unloaded;
        }


        private void ViewportBehavior_EnteringViewport(object sender, EventArgs e)
        {
            //Grid_Loaded(null, null);
        }

        private void ViewportBehavior_ExitedViewport(object sender, EventArgs e)
        {
            //Grid_Unloaded(null, null);
        }

        private void AddToPlayListSubItems_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void rmfs_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void mf_Opening(object sender, object e)
        {
            mfs.Items.Clear();
            foreach (var i in MusicData.Artists)
            {
                var a1 = new MenuFlyoutItem() { Text = i.Name, Tag = i };
                a1.Click += A_Click;
                mfs.Items.Add(a1);
            }
        }

        private async void rmf_Opening(object sender, object e)
        {
            rmfs.Items.Clear();
            foreach (var i in MusicData.Artists)
            {
                var a1 = new MenuFlyoutItem() { Text = i.Name, Tag = i };
                a1.Click += A_Click;
                rmfs.Items.Add(a1);
            }

            AddToPlayListSubItems.Items.Clear();
            var mls = await PlayListHelper.ReadAllPlayList();
            foreach (var item in mls)
            {
                var menuItem = new MenuFlyoutItem()
                {
                    Text = item.ListShowName,
                    Tag = item
                };
                menuItem.Click += Add_Click;
                AddToPlayListSubItems.Items.Add(menuItem);
            }
        }
        #endregion

        private void mfi_Click(object sender, RoutedEventArgs e)
        {
            Pages.ListViewPages.ListViewPage.SetPageToListViewPage<ItemListViewAlbum>(MusicData.Album);
        }

        private void root_AccessKeyInvoked(UIElement sender, Microsoft.UI.Xaml.Input.AccessKeyInvokedEventArgs args)
        {
            rmf.ShowAt(this);
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            var fe = sender as FrameworkElement;
            string tag = fe.Tag as string;
            switch (tag)
            {
                case "0":
                    MainWindow.SetNavViewContent(typeof(SearchPage), MusicData.Title);
                    break;
                case "1":
                    await Launcher.LaunchUriAsync(new System.Uri($"https://www.bing.com/search?q={MusicData.Title}-{MusicData.Album}"));
                    break;
                case "2":
                    Uri uri = null;
                    switch (MusicData.From)
                    {
                        case MusicFrom.neteaseMusic:
                            uri = new($"https://music.163.com/#/song?id={MusicData.ID}");
                            break;
                    }

                    if (uri != null)
                    {
                        var success = await Launcher.LaunchUriAsync(uri);
                    }
                    break;
                case "3":
                    var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
                    dp.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                    dp.SetText(MusicData.Title);
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
                    break;
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var uri = await App.metingServices.NeteaseServices.GetUrl(MusicData.ID, (int)DataFolderBase.DownloadQuality.lossless);
            MainWindow.HideDialog();
            await MainWindow.ShowDialog("获取到的链接是：", uri);
        }

        private async void MenuFlyoutItem_Click_1(object sender, RoutedEventArgs e)
        {
            var seletFile = new FolderLauncherOptions();
            seletFile.ItemsToSelect.Add(await StorageFile.GetFileFromPathAsync(MusicData.InLocal));
            await Launcher.LaunchFolderPathAsync(new FileInfo(MusicData.InLocal).DirectoryName, seletFile);
        }

        private async void MenuFlyout_OpenFileItem_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(MusicData.InLocal), new() { DisplayApplicationPicker = true });
        }

        private async void MenuFlyoutItem_Click_2(object sender, RoutedEventArgs e)
        {
            await App.playingList.Play(MusicData, true);
        }

        NotifyItem item = null;
        private async void Menuflyout_CacheItem_Click(object sender, RoutedEventArgs e)
        {
            if (await App.cacheManager.GetCachePath(MusicData) is not null)
            {
                MainWindow.AddNotify($"此歌曲已缓存！", null, NotifySeverity.Warning);
                return;
            }

            item = MainWindow.AddNotify($"正在缓存：{MusicData.Title}", "加载中...", NotifySeverity.Loading, TimeSpan.MaxValue);
            App.cacheManager.CachingStateChangeMusicData += CacheManager_CachingStateChangeMusicData;
            App.cacheManager.CachedMusicData += CacheManager_CachedMusicData;
            await App.cacheManager.StartCacheMusic(MusicData);
        }

        private void CacheManager_CachingStateChangeMusicData(MusicData musicData, object value)
        {
            if (musicData != MusicData) return;
            item.SetProcess(100, (int)value);
            item.SetNotifyItemData(item.GetNotifyItemData().Title, $"{value}%", NotifySeverity.Loading);
        }

        private void CacheManager_CachedMusicData(MusicData musicData, object value)
        {
            if (musicData != MusicData) return;
            App.cacheManager.CachingStateChangeMusicData -= CacheManager_CachingStateChangeMusicData;
            App.cacheManager.CachedMusicData -= CacheManager_CachedMusicData;
            item.SetNotifyItemData(item.GetNotifyItemData().Title, "缓存完成。", NotifySeverity.Complete);
            MainWindow.NotifyCountDown(item);
            item = null;
        }

        private async void Menuflyout_DeleteCacheItem_Click(object sender, RoutedEventArgs e)
        {
            var path = await App.cacheManager.GetCachePath(MusicData);
            if (string.IsNullOrEmpty(path))
            {
                MainWindow.AddNotify("此歌曲的缓存文件不存在。", null, NotifySeverity.Error);
                return;
            }

            var itema = MainWindow.AddNotify($"正在删除：{MusicData.Title}", null, NotifySeverity.Loading, TimeSpan.MaxValue);
            Exception err = null;
            try
            {
                await Task.Run(() => File.Delete(path));
            }
            catch (Exception ex)
            {
                err = ex;
                itema.SetNotifyItemData("删除失败。", null, NotifySeverity.Error);
            }
            if (err == null)
            {
                itema.SetNotifyItemData("删除成功。", null, NotifySeverity.Complete);
            }
            MainWindow.NotifyCountDown(itema);
        }
    }
}
