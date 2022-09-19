using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// MusicListPage.xaml 的交互逻辑
    /// </summary>
    public partial class MusicListPage : UserControl
    {
        private MainWindow TheParent = null;
        private Source TheSource = null;
        public TheMusicDatas.MusicListData MusicListData = new TheMusicDatas.MusicListData();
        public bool IsLoadListComplete = true;

        private MyC.PopupWindow PlayModPopup = new MyC.PopupWindow() { CloseExit = false, IsShowActivated = true, IsDeActivityClose = true, ForceAcrylicBlur = true, Content = new MyC.PopupContent.PlayModChoice() };

        public MusicListPage(MainWindow Parent)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = Parent.TheSource;
            parents.SizeChanged += MusicListPage_SizeChanged;
            Buttons.SizeChanged += MusicListPage_SizeChanged;
            SizeChanged += MusicListPage_SizeChanged;
            IsSelection = false;

            MusicPlayMod.PlayModChange += (value) =>
            {
                switch (value)
                {
                    case MusicPlayMod.PlayMod.Seq:
                        PlayModTb.Text = "顺序播放";
                        PlayModIcon.Data = TheParent.FindResource("顺序播放") as Geometry;
                        PlayModIcon.Width = 15;
                        PlayModIcon.Height = 14;
                        break;

                    case MusicPlayMod.PlayMod.Random:
                        PlayModTb.Text = "随机播放";
                        PlayModIcon.Data = TheParent.FindResource("随机") as Geometry;
                        PlayModIcon.Width = 14;
                        PlayModIcon.Height = 14;
                        break;

                    case MusicPlayMod.PlayMod.Loop:
                        PlayModTb.Text = "单曲循环";
                        PlayModIcon.Data = TheParent.FindResource("单曲循环") as Geometry;
                        PlayModIcon.Width = 14;
                        PlayModIcon.Height = 15;
                        break;
                    default:
                        break;
                }
            };
            //MyC.PopupWindow.SetPopupShow(RandomPlayGrid, "打开此选项后播放结束时将会随机选择播放列表内的一首歌曲。\n(注：可能会选择到同一首歌)", FollowMouse: true, EndTime: 0);
            //MyC.PopupWindow.SetPopupShow(LoopPlayGrid, "打开此选项后播放结束时将会把下一首歌曲设定为当前歌曲。", FollowMouse: true, EndTime: 0);
        }

        public void Set(TheMusicDatas.MusicListData musicListData)
        {
            MusicListData = musicListData;

            if (musicListData.listShowName == "default")
                TitleTb.Text = "默认播放列表";
            else
                TitleTb.Text = musicListData.listShowName.Replace("default", "播放列表");

            ReCallButton_Click(null, null);
        }

        public void MusicListPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                Listb.Height = ActualHeight - Buttons.ActualHeight - Lines.ActualHeight;
            }
            catch { }

            foreach (FrameworkElement TheBar in TheList.Children)
            {
                try
                {
                    TheBar.Width = Listb.ActualWidth - 10;
                }
                catch { }
            }
        }

        private async void ReCallButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLoadListComplete) return;

            if (MusicListData.listShowName == "default")
                TitleTb.Text = "默认播放列表";
            else
                TitleTb.Text = MusicListData.listShowName.Replace("default", "播放列表");

            IsLoadListComplete = false;
            int anl = await MusicUpdata.UpdataUserMusicList(this);
            IsLoadListComplete = true;

            TitleTb.Text = TitleTb.Text + $"（共 {anl} 首歌曲）";
        }

        private async void AddLocalButton_Click(object sender, RoutedEventArgs e)
        {
            string FilePath = TheSource.OpenWindowChoiceFlie("音频文件 (*.mp3;*.flac;*.wav;*.aac;*.ogg)|*.mp3;*.flac;*.wav;*.aac;*.ogg");
            if (FilePath == null)
            {
                TheParent.ShowBox("操作已取消", "已取消选择文件。");
                return;
            }

            TheMusicDatas.MusicData musicData = new TheMusicDatas.MusicData();

            try
            {
                TagLib.File musicTagFile = TagLib.File.Create(FilePath);
                TagLib.Tag musicTags = musicTagFile.Tag;
                if (!musicTags.IsEmpty)
                {
                    musicData = new TheMusicDatas.MusicData(musicTags.Title, null, musicTags.Performers[0], musicTags.Album, null, null, musicTags.Year.ToString(), null, TheMusicDatas.MusicFrom.localMusic, TheMusicDatas.MusicKbps.Kbps192, FilePath);
                }
                else
                {
                    System.IO.FileStream file = System.IO.File.OpenRead(FilePath);
                    musicData = new TheMusicDatas.MusicData(file.Name, null, "", "", null, null, "", null, TheMusicDatas.MusicFrom.localMusic, TheMusicDatas.MusicKbps.Kbps192, FilePath);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }

            await SongDataEdit.AddUserSong(musicData, MusicListData);
            MusicListData.songs = (await SongDataEdit.GetMusicListData(MusicListData)).songs;
            await MusicUpdata.UpdataUserMusicList(this);
        }

        private void PlayModChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            Point point = PlayModChoiceButton.PointToScreen(new Point(0d, 0d));
            PlayModPopup.UserShow(point.X, point.Y + PlayModChoiceButton.ActualHeight + 2);
        }

        public void znButton_Click(object sender, RoutedEventArgs e)
        {
            PlayModPopup.UserClose();
            TheParent.Activate();

            MyC.znButton znButton = sender as MyC.znButton;

            switch (znButton.Name)
            {
                case "ListPlayModBtn":
                    MusicPlayMod.NowPlayMod = MusicPlayMod.PlayMod.Seq;
                    break;
                case "RandomPlayModBtn":
                    MusicPlayMod.NowPlayMod = MusicPlayMod.PlayMod.Random;
                    break;
                case "LoopPlayModBtn":
                    MusicPlayMod.NowPlayMod = MusicPlayMod.PlayMod.Loop;
                    break;
                default:
                    break;
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.MusicPlayListContent.SetAlbumListPage();
        }

        private void ReCallButton_Click_1(object sender, RoutedEventArgs e)
        {

        }

        public bool _IsSeleceted = false;
        public bool IsSelection
        {
            get { return _IsSeleceted; }
            set
            {
                _IsSeleceted = value;

                if (value)
                {
                    ReturnButton.Visibility = Visibility.Collapsed;
                    ReCallButton.Visibility = Visibility.Collapsed;
                    AllPlayButton.Visibility = Visibility.Collapsed;
                    AddLocalButton.Visibility = Visibility.Collapsed;
                    AddLocalFolderButton.Visibility = Visibility.Collapsed;
                    BatchOperationButton.Visibility = Visibility.Collapsed;

                    ExitBatchOperationButton.Visibility = Visibility.Visible;
                    AllSelectButton.Visibility = Visibility.Visible;
                    PlaySelectButton.Visibility = Visibility.Visible;
                    RemoveSelectButton.Visibility = Visibility.Visible;
                    AddSelectButton.Visibility = Visibility.Visible;
                    DownloadSelectButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ReturnButton.Visibility = Visibility.Visible;
                    ReCallButton.Visibility = Visibility.Visible;
                    AllPlayButton.Visibility = Visibility.Visible;
                    AddLocalButton.Visibility = Visibility.Visible;
                    AddLocalFolderButton.Visibility = Visibility.Visible;
                    BatchOperationButton.Visibility = Visibility.Visible;

                    ExitBatchOperationButton.Visibility = Visibility.Collapsed;
                    AllSelectButton.Visibility = Visibility.Collapsed;
                    PlaySelectButton.Visibility = Visibility.Collapsed;
                    RemoveSelectButton.Visibility = Visibility.Collapsed;
                    AddSelectButton.Visibility = Visibility.Collapsed;
                    DownloadSelectButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void BatchOperationButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLoadListComplete)
            {
                TheParent.ShowBox("错误", "请等待歌曲列表加载完成。");
                return;
            }

            try
            {
                foreach (ItemBar itemBar in TheList.Children)
                {
                    itemBar.IsSelection = !IsSelection;
                }
                IsSelection = !IsSelection;
            }
            catch { }
        }

        private async void AllPlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLoadListComplete) return;
            foreach (var i in MusicListData.songs)
            {
                App.window.NowPlayList.Add(i);
            }
            await App.window.PlaySet(MusicListData.songs[0]);
        }

        private void AllSelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ItemBar itemBar in TheList.Children)
                {
                    itemBar.IsSelected = !itemBar.IsSelected;
                }
            }
            catch { }
        }

        private async void PlaySelectButton_Click(object sender, RoutedEventArgs e)
        {
            bool isfirst = false;
            foreach (ItemBar itemBar in TheList.Children)
            {
                if (itemBar.IsSelected)
                {
                    App.window.NowPlayList.Add(itemBar.TheDatas);
                    if (!isfirst)
                    {
                        isfirst = true;
                        if (App.window.NowPlaySong.SongRid == null)
                        {
                            await App.window.PlaySet(itemBar.TheDatas);
                        }
                    }
                }
            }
            TheParent.ShowBox("播放选中歌曲", "已将选中的歌曲添加到播放列表。");
        }

        private async void RemoveSelectButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.ShowBox("请稍等", "正在移除所选歌曲...", MainWindow.ShowBoxStyle.Loading);
            try
            {
                foreach (ItemBar itemBar in TheList.Children)
                {
                    if (itemBar.IsSelected)
                    {
                        try
                        {
                            await SongDataEdit.RemoveUserSong(itemBar.TheDatas, MusicListData);
                        }
                        catch { }
                    }
                }
            }
            catch { }
            TheParent.ShowBox_ButtonClick(null, null);
            IsSelection = false;
            ReCallButton_Click(null, null);
        }

        private void AddSelectButton_Click(object sender, RoutedEventArgs e)
        {
            MyC.PopupContent.AddAlbumSongContent addAlbumSongContent = new MyC.PopupContent.AddAlbumSongContent();
            MyC.PopupWindow popupWindow = new MyC.PopupWindow()
            {
                Content = addAlbumSongContent,
                IsShowActivated = true,
                IsDeActivityClose = true,
                isWindowSmallRound = false,
                ForceAcrylicBlur = true
            };

            popupWindow.UserShow();

            addAlbumSongContent.ResultEvent += async (result) =>
            {
                popupWindow.UserClose();
                TheParent.ShowBox("请稍等", $"正在添加所选歌曲到 \"{result.listShowName}\"...", MainWindow.ShowBoxStyle.Loading);
                try
                {
                    foreach (ItemBar itemBar in TheList.Children)
                    {
                        if (itemBar.IsSelected)
                        {
                            await SongDataEdit.AddUserSong(itemBar.TheDatas, result.listName);
                        }
                    }
                }
                catch { }
                TheParent.ShowBox_ButtonClick(null, null);
            };

            popupWindow.Closed += (s, args) =>
            {
                popupWindow = null;
                addAlbumSongContent = null;
                GC.Collect();
            };
        }

        private void DownloadSelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (ItemBar itemBar in TheList.Children)
                {
                    if (itemBar.IsSelected)
                    {
                        TheParent.DownloadMusic(itemBar.TheDatas);
                    }
                }
            }
            catch { }
        }

        private void AddLocalFolderButton_Click(object sender, RoutedEventArgs e)
        {
            string path = TheParent.TheSource.OpenWindowChoiceFolder();
            if (path == null) return;

            System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(path);
            if (directoryInfo.GetFiles().Length == 0)
            {
                TheParent.ShowBox("错误", "文件夹为空。");
                return;
            }

            MyC.PopupContent.MusicAddSeq musicAddSeq = new MyC.PopupContent.MusicAddSeq();
            MyC.PopupWindow popupWindow = new MyC.PopupWindow() { ForceAcrylicBlur = true, IsShowActivated = true, IsDeActivityClose = true, Content = musicAddSeq, isWindowSmallRound = false };

            popupWindow.Closed += (s, e1) =>
            {
                musicAddSeq = null;
                popupWindow = null;
                GC.Collect();
            };

            popupWindow.UserShow(-1000, -1000);
            popupWindow.SetPositionWindowCenter(TheParent);

            musicAddSeq.ResultEvent += async (result) =>
            {
                popupWindow.UserClose();
                TheParent.ShowBox("添加歌曲文件夹", "正在将文件夹中的歌曲添加到列表...", MainWindow.ShowBoxStyle.Loading);

                System.IO.FileInfo[] fileInfos = await Task.Run(() => { return directoryInfo.GetFiles(); });
                switch (result)
                {
                    case "name-1":
                        Array.Sort(fileInfos, delegate (System.IO.FileInfo x, System.IO.FileInfo y) { return y.Name.CompareTo(x.Name); });
                        break;
                    case "name-2":
                        Array.Sort(fileInfos, delegate (System.IO.FileInfo x, System.IO.FileInfo y) { return x.Name.CompareTo(y.Name); });
                        break;
                    case "time-1":
                        Array.Sort(fileInfos, delegate (System.IO.FileInfo x, System.IO.FileInfo y) { return x.CreationTime.CompareTo(y.CreationTime); });
                        break;
                    case "time-2":
                        Array.Sort(fileInfos, delegate (System.IO.FileInfo x, System.IO.FileInfo y) { return y.CreationTime.CompareTo(x.CreationTime); });
                        break;
                }

                foreach (var file in fileInfos)
                {
                    string extension = await Task.Run(() => { return System.IO.Path.GetExtension(file.Name); });
                    string[] extensionList = new string[] { ".mp3", ".wma", ".aac", ".m4a", ".flac" };

                    foreach (string es in extensionList)
                    {
                        if (es == extension)
                        {
                            TheMusicDatas.MusicData musicData = new TheMusicDatas.MusicData();

                            try
                            {
                                TagLib.Tag musicTags = await Task.Run(() =>
                                {
                                    return TagLib.File.Create(file.FullName).Tag;
                                });

                                if (!musicTags.IsEmpty)
                                {
                                    musicData = new TheMusicDatas.MusicData(musicTags.Title, null, musicTags.Performers[0], musicTags.Album, null, null, musicTags.Year.ToString(), null, TheMusicDatas.MusicFrom.localMusic, TheMusicDatas.MusicKbps.Kbps192, file.FullName);
                                }
                                else
                                {
                                    musicData = new TheMusicDatas.MusicData(System.IO.Path.GetFileName(file.Name), null, "", "", null, null, "", null, TheMusicDatas.MusicFrom.localMusic, TheMusicDatas.MusicKbps.Kbps192, file.FullName);
                                }
                            }
                            catch
                            {
                                musicData = new TheMusicDatas.MusicData(System.IO.Path.GetFileName(file.Name), null, "", "", null, null, "", null, TheMusicDatas.MusicFrom.localMusic, TheMusicDatas.MusicKbps.Kbps192, file.FullName);
                            }

                            await SongDataEdit.AddUserSong(musicData, MusicListData);
                            continue;
                        }
                    }
                }

                MusicListData.songs = (await SongDataEdit.GetMusicListData(MusicListData)).songs;

                TheParent.ShowBox("添加歌曲文件夹", "添加完成。");
                ReCallButton_Click(null, null);
            };
        }

        int inscrollcount = 0;
        public bool isscrolllist = false;
        private async void Listb_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            inscrollcount++;
            isscrolllist = true;

            await System.Threading.Tasks.Task.Delay(400);

            if (inscrollcount <= 1)
            {
                isscrolllist = false;
            }
            inscrollcount--;
        }
    }
}
