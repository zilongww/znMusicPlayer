using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// DownloadPage.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadPage : UserControl
    {
        private MainWindow TheParent = null;
        private Source TheSource = null;
        public bool IsDownloading = false;
        public int DownloadLine = 3;

        public DownloadPage(MainWindow Parent)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = Parent.TheSource;
            SizeChanged += SizeChangedDo;
        }

        public DownloadCard AddACard(string TheTitle, string TheMessage, double TheValue, string ThePp, string TheSongRid, string SongUri, string SongPath, string Path, MusicPlay.TheMusicDatas.MusicData TheDatas, string PicPath, string LrcPath = null, MainWindow Parent = null)
        {
            DownloadCard TheCard = new DownloadCard().Set(TheTitle, TheMessage, TheValue, ThePp, TheSongRid, SongUri, SongPath, Path, TheDatas, PicPath, LrcPath, Parent);

            foreach (DownloadCard i in ListDownload.Items)
            {
                if (i.TheDatas.SongRid == TheDatas.SongRid)
                {
                    TheParent.ShowBox("错误", "此歌曲已下载，请到下载页面重新下载。");
                    return TheCard;
                }
            }

            ListDownload.Items.Add(TheCard);

            try
            {
                TheCard.StartDownload(SongUri, SongPath);
            }
            catch
            {
                TheCard.Time.Text = "下载失败";
                TheCard.Bars.Value = 100;
            }

            if (ListDownload.Items.Count > 0 && MainText.Visibility == Visibility.Visible)
            {
                ListDownload.Visibility = Visibility.Hidden;
                var storyboard = zilongcn.Animations.animateOpacity(MainText, 1, 0, 0.25, IsAnimate: TheParent.Animation);
                storyboard.Completed += async (o, s) =>
                {
                    MainText.Visibility = Visibility.Collapsed;
                    await Task.Delay(300);
                    zilongcn.Animations.animateOpacity(ListDownload, 0, 1, 0.35, IsAnimate: TheParent.Animation).Begin();
                    ListDownload.Visibility = Visibility.Visible;
                };
                storyboard.Begin();
            }

            return TheCard;
        }

        public void SizeChangedDo(object sender, object args)
        {
            try
            {
                foreach (FrameworkElement items in ListDownload.Items)
                {
                    items.Width = ListDownload.ActualWidth - 32;
                }
            }
            catch { }
        }

        public void ShowMainTextAniamte()
        {
            MainText.Visibility = Visibility.Visible;
            zilongcn.Animations.animateOpacity(MainText, 0, 1, 0.35, IsAnimate: TheParent.Animation).Begin();
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Source.OpenFileExplorer(SettingDataEdit.GetParam("DownloadPath"));
            }
            catch
            {
                TheParent.ShowBox("打开失败", "此路径可能不正确，请重试。");
            }
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            foreach (var i in TheParent.NowDownloadList)
            {
                try
                {
                    i.DeleteFile(false);
                }
                catch { }
            }
        }

        private void DownloadLineBtn_ContentClick(MyC.znComboBox znComboBox, object data)
        {
            DownloadLine = int.Parse((string)data);
            znComboBox.Text = $" 同时下载数：{DownloadLine}";
        }
    }
}
