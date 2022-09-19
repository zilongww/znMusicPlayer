using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// ItemBar.xaml 的交互逻辑
    /// </summary>
    public partial class ItemBar : UserControl, IDisposable
    {
        private MyC.PopupWindow Popups = null;
        private MyC.PopupContent.ItemBarPopup ItemBarPopup = null;
        public TheMusicDatas.MusicData TheDatas = new TheMusicDatas.MusicData();
        public bool IsInMusicList = false;
        public bool OutList = false;

        private Brush _CardColor = new SolidColorBrush(Color.FromArgb(255, 255, 228, 116));
        public Brush CardColor
        {
            get { return _CardColor; }
            set
            {
                CardColors.Fill = value;
                _CardColor = value;
            }
        }

        public static readonly DependencyProperty IsSelectionProperty = DependencyProperty.Register(
            "IsSelection",
            typeof(bool), typeof(ItemBar),
            new PropertyMetadata(false)
            );

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool), typeof(ItemBar),
            new PropertyMetadata(false)
            );

        public bool IsSelection
        {
            get { return (bool)GetValue(IsSelectionProperty); }
            set
            {
                SetValue(IsSelectionProperty, value);

                if (value)
                {
                    PlayStateBack.Visibility = Visibility.Collapsed;
                    MoreBtn.Visibility = Visibility.Collapsed;

                    SelectedBtn.Visibility = Visibility.Visible;
                }
                else
                {
                    ItemButtonsBack.Visibility = Visibility.Collapsed;

                    PlayStateBack.Visibility = Visibility.Visible;
                    MoreBtn.Visibility = Visibility.Visible;

                    SelectedBtn.Visibility = Visibility.Collapsed;

                    IsSelected = false;
                }
            }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set
            {
                SetValue(IsSelectedProperty, value);
                SelectedBtn.IsChecked = value;

                if (value)
                {
                    ItemActivity_MouseEnter(null, null);
                }
                else
                {
                    ItemActivity_MouseLeave(null, null);
                }
            }
        }

        private bool _IsPlay = false;

        public bool IsPlay
        {
            get { return _IsPlay; }
            set
            {
                _IsPlay = value;

                if (value)
                {
                    IsThemeColor = true;
                    PlayStatePath.Data = this.FindResource("暂停") as Geometry;
                }
                else
                {
                    IsThemeColor = false;
                    PlayStatePath.Data = this.FindResource("播放") as Geometry;
                }

                if (App.window.NowPlaySong.MD5 == TheDatas.MD5)
                {
                    ItemActivity_MouseEnter(null, null);
                }
                else
                {
                    ItemActivity_MouseLeave(null, null);
                }
            }
        }

        bool _IsThemeColor = false;
        public bool IsThemeColor
        {
            get { return _IsThemeColor; }
            set
            {
                _IsThemeColor = value;
                if (value)
                {
                    BackAnimateGrid.Fill = App.window.NowThemeData.ButtonBackColor;
                    no.Foreground = App.window.NowThemeData.InColorTextColor;
                    TheButton.Foreground = App.window.NowThemeData.InColorTextColor;
                    Artist.Foreground = App.window.NowThemeData.InColorTextColor;
                    Album.Foreground = App.window.NowThemeData.InColorTextColor;
                    TheButton.EnterForegroundColor = App.window.NowThemeData.InColorTextColor;
                    Artist.EnterForegroundColor = App.window.NowThemeData.InColorTextColor;
                    Album.EnterForegroundColor = App.window.NowThemeData.InColorTextColor;
                    PlayStatePath.Fill = App.window.NowThemeData.InColorTextColor;
                    MoreBtn.Foreground = App.window.NowThemeData.InColorTextColor;
                }
                else
                {
                    BackAnimateGrid.Fill = App.window.NowBlurThemeData.ButtonEnterColor;
                    no.Foreground = App.window.NowThemeData.TextColor;
                    TheButton.Foreground = App.window.NowThemeData.TextColor;
                    Artist.Foreground = App.window.NowThemeData.TextColor;
                    Album.Foreground = App.window.NowThemeData.TextColor;
                    TheButton.EnterForegroundColor = App.window.NowBlurThemeData.InColorTextColor;
                    Artist.EnterForegroundColor = App.window.NowBlurThemeData.InColorTextColor;
                    Album.EnterForegroundColor = App.window.NowBlurThemeData.InColorTextColor;
                    PlayStatePath.Fill = App.window.NowThemeData.TextColor;
                    MoreBtn.Foreground = App.window.NowThemeData.TextColor;
                }
            }
        }

        public ItemBar()
        {
            InitializeComponent();
            IsThemeColor = IsThemeColor;

            SizeChanged += ItemBar_SizeChanged;
            App.window.ThemeChangeEvent += Window_ThemeChangeEvent;

            if (OutList)
            {
                CardColors.Visibility = Visibility.Collapsed;
                MoreBtn.Visibility = Visibility.Collapsed;

                no.Text = "#";
                TheButton.Content = "歌名";
                Artist.Content = "歌手";
                Album.Content = "专辑";
            }
        }

        private void Window_ThemeChangeEvent(MainWindow.ThemeData themeData)
        {
            IsThemeColor = IsThemeColor;
        }

        private void ItemBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            rgClip.Rect = new Rect(0, 0, ActualWidth, ActualHeight);
        }

        public void Set(string TheNumber, TheMusicDatas.MusicData TheDatas, string pic = null, bool AEN = true)
        {
            ItemButtonsBack.Visibility = Visibility.Collapsed;
            no.Text = TheNumber;
            TheButton.Content = TheDatas.Title;
            Artist.Content = TheDatas.Artist;
            Album.Content = TheDatas.Album;

            Album.IsEnabled = AEN;

            try
            {
                TheButton.Click += TheButton_Click;
                Album.Click += Album_Click;
                Artist.Click += Artist_Click;
            }
            catch { }

            this.TheDatas = TheDatas;

            if (!IsInMusicList) CardColors.Visibility = Visibility.Collapsed;
            else CardColor = App.window.GetCardColor(TheDatas.From);

            if (App.window.NowPlaySong.MD5 == TheDatas.MD5)
            {
                IsPlay = true;
                BackAnimateGrid.Opacity = 1;
            }
            App.window.NowPlaySongChangeEvent += Window_NowPlaySongChangeEvent;
        }

        bool isaddevent = false;
        private void Window_NowPlaySongChangeEvent(TheMusicDatas.MusicData musicData)
        {
            if (musicData.MD5 == TheDatas.MD5)
            {
                App.window.PlayStateChangeEvent += TheParent_PlayStateChangeEvent;
                isaddevent = true;
            }
            else
            {
                if (isaddevent)
                {
                    App.window.PlayStateChangeEvent -= TheParent_PlayStateChangeEvent;
                    isaddevent = false;
                }
            }
            UpdataIsPlay();
        }

        private void TheParent_PlayStateChangeEvent(MainWindow.PlayState nowPlayState)
        {
            UpdataIsPlay();
        }

        private void UpdataIsPlay()
        {
            if (App.window.NowPlaySong.MD5 == TheDatas.MD5)
                IsPlay = App.window.NowPlayState == MainWindow.PlayState.Play ? true : false;
            else
                IsPlay = false;
        }

        private async void TheButton_Click(object sender, RoutedEventArgs e)
        {
            await App.window.PlaySet(TheDatas);
            App.window.NowPlayList.Add(TheDatas);
        }

        public void Artist_Click(object sender, object args)
        {
            if (App.window.TheSearchPage.SearchTextBox.Text == TheDatas.Artist) return;
            App.window.SetPage("List");
            App.window.TheSearchPage.SearchTextBox.Text = TheDatas.Artist;
            App.window.TheSearchPage.MusicFrom = TheDatas.From;
            App.window.TheSearchPage.Button_Click(null, null);
        }

        private void Album_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 未启用
            App.window.NowPlayState = MainWindow.PlayState.Pause;

            try
            {
                await App.window.PlaySet(TheDatas);
            }
            catch (Exception err)
            {
                App.window.ShowBox("错误！", "读取数据错误，请重试。\n\n错误代码: \n" + err.ToString());
            }
            Popups.UserClose();
        }

        private void UserControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (OutList) return;

            ItemBarPopup = new MyC.PopupContent.ItemBarPopup(TheDatas);
            Popups = new MyC.PopupWindow() { Content = ItemBarPopup, CloseExit = true, IsShowActivated = true, IsDeActivityClose = true, ForceAcrylicBlur = true, isWindowSmallRound = false };

            Popups.Closed += (s, args) =>
            {
                ItemBarPopup = null;
                Popups = null;
            };

            Popups.UserShow();
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            UserControl_MouseRightButtonUp(null, null);
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if (App.window.NowPlaySong.MD5 == TheDatas.MD5) App.window.NowPlayState = App.window.NowPlayState == MainWindow.PlayState.Play ? MainWindow.PlayState.Pause : MainWindow.PlayState.Play;
            else
            {
                TheButton_Click(sender, e);
            }
        }

        private void ItemActivity_MouseEnter(object sender, MouseEventArgs e)
        {
            if (App.window.MusicPlayListContent.MusicListPage != null)
            {
                if (App.window.MusicPlayListContent.MusicListPage.isscrolllist)
                    return;
            }

            if (!IsPlay && !IsSelected)
            {
                IsThemeColor = false;
            }

            zilongcn.Animations animations = new zilongcn.Animations(App.window.Animation);
            animations.animateOpacity(BackAnimateGrid, BackAnimateGrid.Opacity, 1, 0.2);
            animations.Begin();

            ItemButtonsBack.Visibility = Visibility.Visible;
            var st = zilongcn.Animations.animateOpacity(ItemButtonsBack, ItemButtonsBack.Opacity, 1, 0.2, IsAnimate: App.window.Animation);
            st.Completed += (a, b) => ItemButtonsBack.Visibility = Visibility.Visible;
            st.Begin();
        }

        private void ItemActivity_MouseLeave(object sender, MouseEventArgs e)
        {
            if (App.window.NowPlaySong.MD5 == TheDatas.MD5 || IsSelected) return;

            if (!IsPlay && !IsSelected) IsThemeColor = false;

            //if (BackAnimateGrid == null) return;
            zilongcn.Animations.animateOpacity(BackAnimateGrid, BackAnimateGrid.Opacity, 0, 0.2, IsAnimate: App.window.Animation && (App.window.MusicPlayListContent.MusicListPage != null ? !App.window.MusicPlayListContent.MusicListPage.isscrolllist : true)).Begin();

            var st = zilongcn.Animations.animateOpacity(ItemButtonsBack, ItemButtonsBack.Opacity, 0, 0.2, IsAnimate: App.window.Animation && (App.window.MusicPlayListContent.MusicListPage != null ? !App.window.MusicPlayListContent.MusicListPage.isscrolllist : true));
            st.Completed += (a, b) => ItemButtonsBack.Visibility = Visibility.Collapsed;
            st.Begin();
        }

        public void Delete()
        {
            TheButton.Click -= TheButton_Click;
            Artist.Click -= Artist_Click;
            Album.Click -= Album_Click;
            SizeChanged -= ItemBar_SizeChanged;
            App.window.NowPlaySongChangeEvent -= Window_NowPlaySongChangeEvent;
            App.window.PlayStateChangeEvent -= TheParent_PlayStateChangeEvent;
            App.window.ThemeChangeEvent -= Window_ThemeChangeEvent;
        }

        public void Dispose()
        {
            Delete();
        }

        ~ItemBar()
        {
            Delete();
        }

        private void SelectedBtn_Checked(MyC.OCButton sender)
        {
            if (IsSelected != sender.IsChecked) IsSelected = sender.IsChecked;
        }

        private void ItemActivity_MouseMove(object sender, MouseEventArgs e)
        {
            //App.window.LrcWindow.Lrc1.Text = new Random().Next(1, 100).ToString();
        }
    }
}
