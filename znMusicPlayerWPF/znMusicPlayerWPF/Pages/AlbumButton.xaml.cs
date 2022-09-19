using Newtonsoft.Json.Linq;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// AlbumButton.xaml 的交互逻辑
    /// </summary>
    public partial class AlbumButton : UserControl
    {
        public TheMusicDatas.MusicListData MusicListData = new TheMusicDatas.MusicListData();
        public string AlbumArtist = null;

        public AlbumButton()
        {
            InitializeComponent();
            updataOthers();
            updataTheme();
            dseBorder.Visibility = Visibility.Collapsed;
            SizeChanged += (s, e) => updataOthers();
            App.window.ThemeChangeEvent += (data) => updataTheme();
        }

        private void updataOthers()
        {
            rg.Rect = new Rect(0, 0, ActualWidth, ActualHeight);
            rg1.Rect = new Rect(0, 0, img.ActualWidth, img.ActualHeight + 6);
        }

        private void updataTheme()
        {
            if (App.window.NowThemeData.MD5 == MainWindow.DarkThemeData.MD5)
            {
                backBorder.Background = App.window.NowThemeData.ButtonBackColor;

                darkModMask.Visibility = Visibility.Visible;
                backBorder.BorderBrush = null;
                textTb.Foreground = App.window.NowThemeData.InColorTextColor;
                listCount.Foreground = App.window.NowThemeData.InColorTextColor;
            }
            else
            {
                darkModMask.Visibility = Visibility.Collapsed;
                backBorder.Background = App.window.NowThemeData.ButtonBackColor;
                backBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(50, 155, 155, 155));
                textTb.Foreground = App.window.NowThemeData.InColorTextColor;
                listCount.Foreground = App.window.NowThemeData.InColorTextColor;
            }
        }

        public void Delete()
        {
            img.Source = null;
            img = null;
            MusicListData.songs.Clear();
            MusicListData = null;
        }

        public async void Set(TheMusicDatas.MusicListData musicListData)
        {
            //btnStackPanel.Opacity = 0;
            //BackGrid.Opacity = 0.25;
            img.Opacity = 0;

            MusicListData = musicListData;

            if (MusicListData.listShowName == "default")
                title.Text = "默认播放列表";
            else
                title.Text = musicListData.listShowName.Replace("default", "播放列表");
            listCount.Text = $"{musicListData.listCount}首歌曲";
            musicListData.songs.Clear();

            zilongcn.Animations.animateOpacity(
                this, 0, 1, 0.5,
                IsAnimate: App.window.Animation).Begin();

            imgLoadingIcon.Pause = false;

            try
            {
                double DpiX = VisualTreeHelper.GetDpi(this).DpiScaleX;
                double DpiY = VisualTreeHelper.GetDpi(this).DpiScaleY;
                img.Source = await Source.GetImage(
                    musicListData.picPath,
                    musicListData.listFrom == TheMusicDatas.MusicFrom.localMusic ? "localAlbum" : "internet",
                    (int)(154 * DpiX), (int)(154 * DpiY));
            }
            catch { }

            imgLoadingIcon.Pause = true;

            zilongcn.Animations.animateOpacity(
                img, 0, 1, 0.3,
                IsAnimate: App.window.Animation).Begin();
        }

        //bool animationEnd1 = true;
        //bool animationEnd2 = true;
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (true)
            {
                //animationEnd1 = false;
                dseBorder.Visibility = Visibility.Visible;
                var st = zilongcn.Animations.animateOpacity(dseBorder, dseBorder.Opacity, 0.14, 0.25, 0.5, 0.5, IsAnimate: App.window.Animation);
                st.Completed += (s, args) =>
                {
                    //animationEnd1 = true;
                    dseBorder.Visibility = Visibility.Visible;
                };
                st.Begin();

                zilongcn.Animations.animatePosition(backBorder, backBorder.Margin, new Thickness(0, 0, 0, 2), 0.25, 1, 0, IsAnimate: App.window.Animation).Begin();
            }
            else
            {
                //await Task.Delay(20);
                //UserControl_MouseEnter(sender, e);
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (true)
            {
                //animationEnd2 = false;
                var st = zilongcn.Animations.animateOpacity(dseBorder, dseBorder.Opacity, 0, 0.25, 0.5, 0.5, IsAnimate: App.window.Animation);
                st.Completed += (s, args) =>
                {
                    dseBorder.Visibility = Visibility.Collapsed;
                    //animationEnd2 = true;
                };
                st.Begin();

                zilongcn.Animations.animatePosition(backBorder, backBorder.Margin, new Thickness(0), 0.25, 1, 0, IsAnimate: App.window.Animation).Begin();
            }
            else
            {
                //await Task.Delay(20);
                //UserControl_MouseLeave(sender, e);
            }
        }

        bool MouseDown1 = false;
        private void mainback_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown1 = true;
            UserControl_MouseLeave(sender, e);
        }

        private void mainback_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseDown1)
            {
                MouseDown1 = false;

                znButton_Click(null, e);

                if (false)
                {
                    MyC.PopupWindow popupWindow = new MyC.PopupWindow() { ForceAcrylicBlur = true, IsShowActivated = true, IsDeActivityClose = true, CloseExit = true, isWindowSmallRound = false };
                    MyC.PopupContent.AlbumButtonRightMenu albumButtonRightMenu = new MyC.PopupContent.AlbumButtonRightMenu(this);
                    popupWindow.Content = albumButtonRightMenu;

                    popupWindow.Closed += (s, args) =>
                    {
                        popupWindow = null;
                        albumButtonRightMenu = null;
                        GC.Collect();
                    };

                    popupWindow.UserShow();
                }
            }
        }

        bool MouseDown2 = false;
        private void mainback_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown2 = true;
        }

        private void mainback_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (MouseDown2)
            {
                MouseDown2 = false;

                MyC.PopupWindow popupWindow = new MyC.PopupWindow() { ForceAcrylicBlur = true, IsShowActivated = true, IsDeActivityClose = true, CloseExit = true, isWindowSmallRound = false };
                MyC.PopupContent.AlbumButtonRightMenu albumButtonRightMenu = new MyC.PopupContent.AlbumButtonRightMenu(this);
                popupWindow.Content = albumButtonRightMenu;

                popupWindow.Closed += (s, args) =>
                {
                    popupWindow = null;
                    albumButtonRightMenu = null;
                    GC.Collect();
                };

                popupWindow.UserShow();
            }
        }

        public async void znButton_Click(object sender, RoutedEventArgs e)
        {
            MusicListData = await SongDataEdit.GetMusicListData(MusicListData);
            App.window.MusicPlayListContent.SetMusicListPage(MusicListData);
            App.window.Activate();
        }

        public void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (MusicListData.listName == "default")
            {
                App.window.ShowBox("注意", "不能删除默认播放列表。");
                return;
            }

            MyC.PopupContent.InteractContent interactContent = new MyC.PopupContent.InteractContent("是否确定删除此列表？");
            MyC.PopupWindow popupWindow = new MyC.PopupWindow()
            {
                Content = interactContent,
                IsShowActivated = true,
                IsDeActivityClose = true,
                isWindowSmallRound = false
            };

            popupWindow.UserShow();

            popupWindow.MouseLeftButtonDown += (s, args) => popupWindow.DragMove();

            interactContent.ResultEvent += async (args) =>
            {
                popupWindow.UserClose();
                if (args)
                {
                    await SongDataEdit.RemoveList(MusicListData);
                    App.window.MusicPlayListContent.AlbumListRefresh();
                }
            };
        }

        MyC.PopupContent.AlbumEditContent albumEditContent = null;
        public void znButton_Click_2(object sender, RoutedEventArgs e)
        {
            if (albumEditContent != null) return;

            albumEditContent = new MyC.PopupContent.AlbumEditContent();
            MyC.PopupWindow popupWindow = new MyC.PopupWindow()
            {
                Content = albumEditContent,
                IsShowActivated = true,
                IsDeActivityClose = true,
                isWindowSmallRound = false,
                ForceAcrylicBlur = true
            };

            albumEditContent.ImgPath.Text = MusicListData.picPath == "" ? "不选择" : MusicListData.picPath;
            albumEditContent.ListNameTbox.Text = MusicListData.listShowName;

            if (MusicListData.listShowName == "default")
            {
                albumEditContent.CantEditRun.Text = " 不能编辑默认列表的名称";
                albumEditContent.ListNameTbox.IsEnabled = false;
            }

            popupWindow.Closed += (s, args) => albumEditContent = null;

            popupWindow.UserShow(-1000, -1000);
            popupWindow.SetPositionWindowCenter(App.window);

            albumEditContent.ChoiceImgBtn.Click += (s, args) =>
            {
                popupWindow.IsDeActivityClose = false;

                string path = App.window.TheSource.OpenWindowChoiceFlie("图片文件|*.jpg;*.png|所有文件|*.*");
                albumEditContent.ImgPath.Text = path ?? "不选择";

                popupWindow.Activate();
                popupWindow.IsDeActivityClose = true;
            };

            albumEditContent.EnterBtn.Click += async (s, args) =>
            {
                string listName = albumEditContent.ListNameTbox.Text;

                if (listName == "")
                {
                    App.window.ShowBox("错误", "名称不能为空。");
                    return;
                }

                var lists = await SongDataEdit.GetAllMusicListData();
                foreach (var listData in lists)
                {
                    if (listData.listShowName == listName && listData.listShowName != MusicListData.listShowName)
                    {
                        App.window.ShowBox("错误", "已存在一个同名的播放列表，请换个名称试试。");
                        return;
                    }
                }

                try
                {
                    JObject data = BaseFoldsPath.TheUserSongData;
                    data[MusicListData.listName]["picPath"] = albumEditContent.ImgPath.Text == "不选择" ? null : albumEditContent.ImgPath.Text;
                    data[MusicListData.listName]["name"] = listName;
                    BaseFoldsPath.TheUserSongData = data;

                    App.window.MusicPlayListContent.AlbumListRefresh();
                }
                catch (NullReferenceException)
                {
                    App.window.ShowBox("错误", "播放列表不存在。");
                }
                catch (Exception err)
                {
                    App.window.ShowBox("错误", "保存数据时出现错误，错误信息：" + err.Message);
                }

                popupWindow.UserClose();
                popupWindow = null;
                albumEditContent = null;
                GC.Collect();
            };

            albumEditContent.CancelBtn.Click += (s, args) =>
            {
                popupWindow.UserClose();
                popupWindow = null;
                albumEditContent = null;
                GC.Collect();
            };
        }
    }
}
