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

namespace znMusicPlayerWUI.Controls
{
    public partial class SongItem : Grid, IDisposable
    {
        private bool _ShowImage = true;

        public bool CanClickPlay { get; set; } = true;
        public MusicData MusicData { get; set; }
        public MusicListData MusicListData { get; set; }
        public double ImageScaleDPI { get; set; } = 1.0;
        public bool ShowImage
        {
            get { return _ShowImage; }
            set
            {
                _ShowImage = value;
                AlbumImage_BaseBorder.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public SongItem(MusicData musicData, MusicListData musicListData = null)
        {
            InitializeComponent();
            MusicData = musicData;
            MusicListData = musicListData;
            DataContext = this;

            if (musicData.PicturePath == null) ShowImage = false;

            foreach (var i in musicData.Artists)
            {
                var a1 = new MenuFlyoutItem() { Text = i.Name, Tag = i };
                var a2 = new MenuFlyoutItem() { Text = i.Name, Tag = i };
                a1.Click += A_Click;
                a2.Click += A_Click;
                mfs.Items.Add(a1);
                rmfs.Items.Add(a2);
            }
            mfi.Text = $"专辑：{musicData.Album}";
            rmfi.Text = $"专辑：{musicData.Album}";


            MainWindow_DriveInTypeEvent(MainWindow.DriveInType);
            MainWindow.DriveInTypeEvent += MainWindow_DriveInTypeEvent;

            if (musicListData.ListDataType != DataType.本地歌单)
            {
                DeleteFlyoutBtn.Visibility = Visibility.Collapsed;
            }
        }

        private async void A_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetNavViewContent(
            typeof(ItemListView),
            new List<object> { DataType.艺术家, (Artist)(sender as MenuFlyoutItem).Tag },
            new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });

            //var artist = await App.metingServices.NeteaseServices.GetArtist(((Artist)(sender as MenuFlyoutItem).Tag).ID);
            //await MainWindow.ShowDialog("result", $"{artist.Name}\n{artist.PicturePath}\n{artist.Describee}\n{artist.HotSongs.Songs.Count}");
        }

        public void Dispose()
        {
            try
            {
                DataContext = null;
                AlbumImage.Dispose();
                MusicData = null;
                MainWindow.DriveInTypeEvent -= MainWindow_DriveInTypeEvent;
            }
            catch (Exception err)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(err.ToString());
#endif
            }
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow_DriveInTypeEvent(MainWindow.DriveInType);

            if (ShowImage)
            {
                ImageSource a = null;
                if (MusicData.InLocal != null)
                {
                    //a = await CodeHelper.GetCover(MusicData.InLocal);
                }
                else
                {
                    try
                    {
                        a = await FileHelper.GetImageSource(await Media.ImageManage.GetImageSource(MusicData), (int)(50 * ImageScaleDPI), (int)(50 * ImageScaleDPI), true);
                    }
                    catch { }
                }
                AlbumImage.Source = a;
            }
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            AlbumImage.Source = null;
        }

        // 当点击类型不是鼠标时为其添加间距以方便点击
        private void MainWindow_DriveInTypeEvent(Microsoft.UI.Input.PointerDeviceType deviceType)
        {
            if (deviceType != Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                InfoButton.Visibility = Visibility.Collapsed;
                RightToolBar.Visibility = Visibility.Visible;
                RightToolBar.Children[0].Visibility = Visibility.Collapsed;
            }
            else
            {
                InfoButton.Visibility = Visibility.Visible;
                RightToolBar.Visibility = Visibility.Collapsed;
                RightToolBar.Children[0].Visibility = Visibility.Visible;
            }

            if (deviceType == Microsoft.UI.Input.PointerDeviceType.Touch)
            {
                if (!ShowImage)
                    BaseGrid.Margin = new Thickness(0, 8, 0, 8);
            }
            else
            {
                BaseGrid.Margin = new Thickness(0);
            }
        }

        private void Button_RightTapped(object sender, Microsoft.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            rmf.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private void Grid_Holding(object sender, Microsoft.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            rmf.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        // 鼠标进入时改变颜色
        private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                RightToolBar.Visibility = Visibility.Visible;

                Storyboard storyboard = new Storyboard();
                DoubleAnimation doubleAnimation = new DoubleAnimation();

                doubleAnimation.From = BackgroundBaseGrid.Opacity;
                doubleAnimation.To = 0.06;
                doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));
                Storyboard.SetTarget(doubleAnimation, BackgroundBaseGrid);
                Storyboard.SetTargetProperty(doubleAnimation, "Opacity");

                storyboard.Children.Add(doubleAnimation);
                storyboard.Begin();
            }
        }

        // 鼠标离开时改变颜色
        private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                RightToolBar.Visibility = Visibility.Collapsed;

                Storyboard storyboard = new Storyboard();
                DoubleAnimation doubleAnimation = new DoubleAnimation();

                doubleAnimation.From = BackgroundBaseGrid.Opacity;
                doubleAnimation.To = 0;
                doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.1));
                Storyboard.SetTarget(doubleAnimation, BackgroundBaseGrid);
                Storyboard.SetTargetProperty(doubleAnimation, "Opacity");

                storyboard.Children.Add(doubleAnimation);
                storyboard.Begin();
            }
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
            await App.playingList.Play(MusicData);
        }
        
        // 单击添加到播放中列表按钮
        private void AddPlay_Click(object sender, RoutedEventArgs e)
        {
            App.playingList.Add(MusicData);
        }
        
        // 单击下一首播放按钮
        private void NextPlay_Click(object sender, RoutedEventArgs e)
        {
            App.playingList.SetNextPlay(App.playingList.NowPlayingMusicData, MusicData);
        }
        
        // 单击详细信息按钮
        private async void Info_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.ShowDialog($"{MusicData.Title} 的详细信息：", $"标题：{MusicData.Title}。\n艺术家&专辑：{MusicData.ButtonName}\nID：{MusicData.ID}\n来源：{MusicData.From}\n图片地址：{MusicData.PicturePath}");
        }

        // 双击元素 播放
        private void Grid_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                if (CanClickPlay)
                    Play_Click(null, null);
            }
        }

        // 除鼠标外的按下事件 单击播放
        private void Grid_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (e.PointerDeviceType != Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
                if (CanClickPlay)
                    Play_Click(null, null);
            }
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            App.downloadManager.Add(MusicData);
        }

        private async void rmf_Opened(object sender, object e)
        {
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
            if (MusicListData.ListDataType == DataType.本地歌单)
            {
                await PlayListHelper.DeleteMusicDataFromPlayList(MusicListData.ListName, MusicData);
            }
        }
    }
}
