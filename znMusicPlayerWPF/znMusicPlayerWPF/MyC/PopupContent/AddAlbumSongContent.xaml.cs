using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// AddAlbumSongContent.xaml 的交互逻辑
    /// </summary>
    public partial class AddAlbumSongContent : UserControl
    {
        public delegate void ResultDelegate(TheMusicDatas.MusicListData listName);
        public event ResultDelegate ResultEvent;

        public AddAlbumSongContent()
        {
            InitializeComponent();
            TipTb.Opacity = 0;
            CopyBtn.Visibility = Visibility.Collapsed;
            Refresh();

            tba.Foreground = App.window.FindResource("ATextColor_InBlur") as SolidColorBrush;
        }

        public async void Refresh()
        {
            Borders.Visibility = Visibility.Hidden;
            List<TheMusicDatas.MusicListData> musicListDatas = await SongDataEdit.GetAllMusicListData();

            foreach (var i in musicListDatas)
            {
                znButton znButtons = new znButton()
                {
                    Content = i.listShowName == "default" ? "默认播放列表" : i.listShowName.Replace("default", "播放列表"),
                    Tag = i,
                    Background = App.window.NowBlurThemeData.ButtonBackColor as SolidColorBrush,
                    EnterColor = App.window.NowBlurThemeData.ButtonEnterColor as SolidColorBrush,
                    Foreground = App.window.NowBlurThemeData.InColorTextColor as SolidColorBrush,
                    BorderBrush = null,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    FontSize = (double)(App.window.FindResource("设置字体大小") as double?),
                    Height = 34,
                    Margin = new Thickness(0, 0, 6, 0)
                };

                znButtons.Click += (s, e) =>
                {
                    zilongcn.Animations.animateOpacity(
                        TipTb, 0, 0, 0
                        ).Begin();

                    ResultEvent((TheMusicDatas.MusicListData)(znButtons.Tag as TheMusicDatas.MusicListData));

                    zilongcn.Animations.animateOpacity(
                        TipTb, 0, 1, 0.3
                        ).Begin();
                };

                Lists.Items.Add(znButtons);

                await Task.Delay(1);
            }

            zilongcn.Animations.animateOpacity(Borders, 0, 1, 0.2, IsAnimate: MainWindow.Animate).Begin();
            Borders.Visibility = Visibility.Visible;
        }
    }
}
