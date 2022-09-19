using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// LrcContentPage.xaml 的交互逻辑
    /// </summary>
    public partial class LrcContentPage : UserControl
    {
        double windowWidth;
        double windowHeight;

        bool _OpenPlayBar = false;
        public bool OpenPlayBar
        {
            get { return _OpenPlayBar; }
            set
            {
                _OpenPlayBar = value;
                if (value)
                {
                    App.window.MinHeight = PlayBar.ActualHeight + 16;
                    zilongcn.Animations.animatePosition(PlayBar, PlayBar.Margin, new Thickness(8), 0.3, 1, 0, IsAnimate: MainWindow.Animate).Begin();
                    App.window.TopBtnGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    App.window.MinHeight = 64;
                    zilongcn.Animations.animatePosition(PlayBar, PlayBar.Margin, new Thickness(8, 0, 8, -PlayBar.ActualHeight), 0.3, 0, 1, System.Windows.Media.Animation.EasingMode.EaseIn, IsAnimate: MainWindow.Animate).Begin();
                    App.window.TopBtnGrid.Visibility = Visibility.Collapsed;
                }
            }
        }

        public double LrcFontSize
        {
            get { return Lrc1TB.FontSize; }
            set
            {
                if (value < 1) return;
                Lrc1TB.FontSize = value;
                Lrc2TB.FontSize = value;
                PlayBar_FontSizeTB.Text = value.ToString();
            }
        }

        public string AText = "技术原因，目前无法实现透明控件，但可以使用obs自带的扣像（色值滤镜功能）达到透明的效果。\n" +
            "1. 在obs来源里添加一个窗口采集，窗口选择此程序的窗口：（[znMusicPlayerWPF.exe]: kwMusicDownload）；\n" +
            "2. 确保obs对此程序窗口的采集框在obs里能正常显示；\n" +
            "3. 在obs设置此程序的窗口采集滤镜（一般在右键菜单中的滤镜选项卡）；\n" +
            "4. 在 弹出的滤镜窗口 中点击 效果滤镜处 的最下方的 加号，选择色值，名称随意，然后点击确定；\n" +
            "5. 在 效果滤镜处 点击刚刚添加的色值选项卡，右边会有其一系列属性；\n" +
            "6. 将 关键的颜色类型 选择自定义，点击下方的 关键的颜色 右侧的 选择颜色 按钮；\n" +
            "7. 在弹出的窗口中 右下角的确定按钮上方 的 HTML 更改为 #272727，然后点击确定；\n" +
            "8. 相似度 设置为 1，平滑 设置为 1000，至此色值滤镜的设置结束；\n" +
            "9. 在 obs 和 此程序 里设置好 窗口大小和字体大小，直至抠出来的歌词字体边缘不锐利。\n" +
            "此功能旨在推流/录制全屏游戏时可录制歌词，仅为替代方案，全屏游戏里显示器不可见此控件。";
        public LrcContentPage()
        {
            InitializeComponent();
            MyC.PopupWindow.SetPopupShow(questTb, AText, followMouse: true, endTime: 0, ForceAcrylicBlur: false);
            this.FontFamily = FindResource("znNormal") as FontFamily;
            App.window.BigPageStateChangeEvent += Window_BigPageStateChangeEvent;
            Lrc1TB.FontWeight = FontWeights.Black;
            Lrc2TB.FontWeight = FontWeights.Black;
            Lrc1TB.Foreground = App.window.OtherColor;
            Lrc2TB.Foreground = App.window.OtherColor;
        }

        private async void Window_BigPageStateChangeEvent(object state)
        {
            bool isopen = (bool)state;
            if (isopen)
            {
                if (App.window.NowPlaySong.SongRid != null)
                    PlayBar_NowPlayTitle.Text = $"正在播放：{App.window.NowPlaySong.Title} - {App.window.NowPlaySong.Artist}";

                PlayBar.Background = App.window.NowThemeData.ButtonBackColor;
                Foreground = App.window.NowThemeData.InColorTextColor;
                Background = MainWindow.DarkThemeData.BackColor;

                windowHeight = App.window.ActualHeight;
                windowWidth = App.window.ActualWidth;

                App.window.PlayBar.Visibility = Visibility.Collapsed;
                App.window.MusicPages.LrcChangeEvent += MusicPages_LrcChangeEvent;
                App.window.NowPlaySongChangeEvent += Window_NowPlaySongChangeEvent;

                OpenPlayBar = false;
                await Task.Delay(10);
                OpenPlayBar = true;
                fb.Focus();
            }
            else
            {
                App.window.TopBtnGrid.Visibility = Visibility.Visible;
                App.window.Height = windowHeight;
                App.window.Width = windowWidth;

                App.window.PlayBar.Visibility = Visibility.Visible;
                App.window.MusicPages.LrcChangeEvent -= MusicPages_LrcChangeEvent;
                App.window.BigPageStateChangeEvent -= Window_BigPageStateChangeEvent;
                App.window.NowPlaySongChangeEvent -= Window_NowPlaySongChangeEvent;
            }
        }

        private void Window_NowPlaySongChangeEvent(MusicPlay.TheMusicDatas.MusicData musicData)
        {
            PlayBar_NowPlayTitle.Text = $"正在播放：{musicData.Title} - {musicData.Artist}";
            Lrc1TB.Text = App.window.MusicPages.NowLrc;
            Lrc2TB.Text = App.window.MusicPages.NextLrc;
        }

        MusicPage.LrcData NextLrc = new MusicPage.LrcData("", 11L, "");
        private void MusicPages_LrcChangeEvent(MusicPage.LrcData nowLrc, MusicPage.LrcData nextLrc)
        {
            if (nowLrc.TranslateLrcText != null)
            {
                Lrc1TB.TextAlignment = TextAlignment.Center;
                Lrc2TB.TextAlignment = TextAlignment.Center;

                Lrc1TB.Foreground = App.window.LrcColor;
                Lrc2TB.Foreground = App.window.OtherColor;

                Lrc1TB.Text = nowLrc.LrcText;
                Lrc2TB.Text = nextLrc.LrcText;
            }
            else
            {
                Lrc1TB.TextAlignment = TextAlignment.Left;
                Lrc2TB.TextAlignment = TextAlignment.Right;

                if (NextLrc.MD5 == nowLrc.MD5)
                {
                    Lrc1TB.Text = nextLrc.LrcText;
                    Lrc2TB.Text = nowLrc.LrcText;
                    Lrc1TB.Foreground = App.window.OtherColor;
                    Lrc2TB.Foreground = App.window.LrcColor;

                    NextLrc = nowLrc;
                }
                else
                {
                    Lrc1TB.Text = nowLrc.LrcText;
                    Lrc2TB.Text = nextLrc.LrcText;
                    Lrc1TB.Foreground = App.window.LrcColor;
                    Lrc2TB.Foreground = App.window.OtherColor;

                    NextLrc = nextLrc;
                }
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Y:
                    OpenPlayBar = !OpenPlayBar;
                    break;
                default:
                    break;
            }
        }

        private void fb_Click(object sender, RoutedEventArgs e)
        {
            LrcFontSize += 1;
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            LrcFontSize -= 1;
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            App.window.DragMove();
        }
    }
}
