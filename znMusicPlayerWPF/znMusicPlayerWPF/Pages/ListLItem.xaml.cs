using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// ListLItem.xaml 的交互逻辑
    /// </summary>
    public partial class ListLItem : UserControl
    {
        private bool _IsPlay = false;

        public bool IsPlay
        {
            get { return _IsPlay; }
            set
            {
                _IsPlay = value;
                if (value)
                {
                    StatePath.Data = FindResource("暂停") as Geometry;

                    UserControl_MouseEnter(null, null);
                    LeftBar.Visibility = Visibility.Visible;
                    var st = zilongcn.Animations.animateOpacity(LeftBar, LeftBar.Opacity, 1, 0.2, IsAnimate: App.window.Animation);
                    st.Completed += (s, e) => LeftBar.Visibility = Visibility.Visible;
                    st.Begin();
                }
                else
                {
                    StatePath.Data = FindResource("播放") as Geometry;

                    if (App.window.NowPlaySong.MD5 != MusicData.MD5)
                    {
                        UserControl_MouseLeave(null, null);
                        var st = zilongcn.Animations.animateOpacity(LeftBar, LeftBar.Opacity, 0, 0.2, IsAnimate: App.window.Animation);
                        st.Completed += (s, e) => LeftBar.Visibility = Visibility.Collapsed;
                        st.Begin();
                    }
                }
            }
        }

        public TheMusicDatas.MusicData MusicData = new TheMusicDatas.MusicData();

        public ListLItem()
        {
            InitializeComponent();
        }

        public ListLItem Set(TheMusicDatas.MusicData TheData)
        {
            ButtonsStackPanel.Visibility = Visibility.Collapsed;
            ButtonsStackPanel.Opacity = 0;
            LeftBar.Visibility = Visibility.Collapsed;
            LeftBar.Opacity = 0;
            BackOpacityMask.Opacity = 0;

            MusicData = TheData;

            Name1.Text = TheData.Title;
            Artist.Text = TheData.Artist;

            App.window.NowPlaySongChangeEvent += Window_NowPlaySongChangeEvent;
            UpdataIsPlay();
            return this;
        }


        bool isaddevent = false;
        private void Window_NowPlaySongChangeEvent(TheMusicDatas.MusicData musicData)
        {
            if (musicData.MD5 == MusicData.MD5)
            {
                App.window.PlayStateChangeEvent += TheParent_PlayStateChangeEvent;
                isaddevent = true;
            }
            else
            {
                if (isaddevent)
                {
                    App.window.PlayStateChangeEvent -= TheParent_PlayStateChangeEvent;
                    UpdataIsPlay();
                    isaddevent = false;
                }
            }
        }

        private void TheParent_PlayStateChangeEvent(MainWindow.PlayState nowPlayState)
        {
            UpdataIsPlay();
        }

        private void UpdataIsPlay()
        {
            if (App.window.NowPlaySong.MD5 == MusicData.MD5)
            {
                IsPlay = App.window.NowPlayState == MainWindow.PlayState.Play ? true : false;
            }
            else
            {
                IsPlay = false;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.window.NowPlaySong.MD5 == MusicData.MD5) App.window.NowPlayState = App.window.NowPlayState == MainWindow.PlayState.Play ? MainWindow.PlayState.Pause : MainWindow.PlayState.Play;
            else
            {
                App.window.NowPlaySong = MusicData;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            App.window.NowPlayList.Remove(MusicData);
        }

        public void Delete()
        {
            App.window.NowPlaySongChangeEvent -= Window_NowPlaySongChangeEvent;
            App.window.PlayStateChangeEvent -= TheParent_PlayStateChangeEvent;
        }

        ~ListLItem()
        {
            Delete();
        }

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ButtonsStackPanel.Visibility = Visibility.Visible;
            zilongcn.Animations animations = new zilongcn.Animations(App.window.Animation);
            animations.animateOpacity(ButtonsStackPanel, ButtonsStackPanel.Opacity, 1, 0.2);
            animations.animateOpacity(BackOpacityMask, BackOpacityMask.Opacity, 0.1, 0.2);

            animations.storyboard.Completed += (s, args) =>
            {
                ButtonsStackPanel.Visibility = Visibility.Visible;
            };

            animations.Begin();
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsPlay || App.window.NowPlaySong.MD5 == MusicData.MD5) return;

            zilongcn.Animations animations = new zilongcn.Animations(App.window.Animation);
            animations.animateOpacity(ButtonsStackPanel, ButtonsStackPanel.Opacity, 0, 0.2);
            animations.animateOpacity(BackOpacityMask, BackOpacityMask.Opacity, 0, 0.2);

            animations.storyboard.Completed += (s, args) =>
            {
                ButtonsStackPanel.Visibility = Visibility.Collapsed;
            };

            animations.Begin();
        }
    }
}
