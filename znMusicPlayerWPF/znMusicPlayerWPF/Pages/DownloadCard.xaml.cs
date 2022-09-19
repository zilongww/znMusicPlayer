using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// DownloadCard.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadCard : UserControl
    {
        public string SongRid = null;
        public string SongUri = null;
        public string SongPath = null;
        public string SongPic = null;
        public string SongLrc = null;
        public string Path = null;
        public MusicPlay.TheMusicDatas.MusicData TheDatas = new MusicPlay.TheMusicDatas.MusicData(null, null, null, null, null, null, null);
        public MainWindow TheParent = null;

        public DownloadCard()
        {
            InitializeComponent();

            SizeChanged += (sender, args) =>
            {
                if (this.ActualWidth <= 300) Time1.Visibility = Visibility.Collapsed;
                else Time1.Visibility = Visibility.Visible;
            };
            SetThemeAsync();
        }

        public async void SetThemeAsync()
        {
            await Task.Delay(1);
            TheParent.ThemeChangeEvent += (data) =>
            {
                mainborder.Background = TheParent.NowThemeData.ButtonBackColor;
            };
            mainborder.Background = TheParent.NowThemeData.ButtonBackColor;
        }

        public DownloadCard Set(string TheTitle, string TheMessage, double TheValue, string ThePp, string TheRid, string SongUri, string SongPath, string Path, MusicPlay.TheMusicDatas.MusicData TheDatas, string a, string b, MainWindow Parent)
        {
            Title.Text = TheTitle;
            Bars.Value = TheValue;
            Time.Text = ThePp;
            SongRid = TheRid;

            this.SongUri = SongUri;
            this.SongPath = SongPath;
            this.SongPic = a;
            this.SongLrc = b;
            this.TheDatas = TheDatas;
            this.Path = Path;
            this.TheParent = Parent;

            return this;
        }

        public void Set1(double TheValue, string ThePp, string ThePp2)
        {
            Bars.Value = TheValue;
            Time.Text = ThePp;
            Time1.Text = ThePp2;
        }

        public async void StartDownload(string TheWeb, string FilePath)
        {
            if (TheWeb == "") return;

            Bars.IsIndeterminate = true;

            System.Net.WebClient TheDownloader = new System.Net.WebClient() { Encoding = Encoding.UTF8 };

            TheParent.ShowMessage("正在下载", "已将 \"" + TheDatas.Title + " - " + TheDatas.Artist + "\" 添加到下载列表", 3).BalloonTipClicked += TheParent.Msg_BalloonTipClicked1;

            while (TheParent.NowDownloadList.Count > TheParent.TheDownload.DownloadLine)
            {
                Time.Text = "正在等待";
                await Task.Delay(800);
                if (TheParent.NowDownloadList.Count == 0 && TheParent.TheDownload.DownloadLine == 1) break;
            }

            TheParent.NowDownloadList.Add(this);

            Time.Text = "正在加载";

            try
            {
                TheDownloader.DownloadFileAsync(new Uri(TheWeb), FilePath);
                TheDownloader.BaseAddress = TheWeb;
            }
            catch
            {
                TheParent.ShowBox("下载错误", $"获取不到歌曲 \"{TheDatas.Title}\" 的网络地址，请重新下载。");
                Bars.IsIndeterminate = false;
            }

            DownloadBtn.IsEnabled = false;
            RetryBtn.IsEnabled = false;

            #region 下载进度刷新
            TheDownloader.DownloadProgressChanged += (s, e) =>
            {
                try
                {
                    Set1(e.ProgressPercentage, (Convert.ToDouble(e.BytesReceived) / Convert.ToDouble(e.TotalBytesToReceive) * 100).ToString("0.0") + "%", zilongcn.Others.GetAutoSizeString(e.BytesReceived, 2) + "/" + zilongcn.Others.GetAutoSizeString(e.TotalBytesToReceive, 2));
                    Bars.IsIndeterminate = false;
                }
                catch
                {
                    Time.Text = "下载失败";
                    DownloadBtn.IsEnabled = true;
                    RetryBtn.IsEnabled = true;
                    Bars.Value = 100;
                }
            };
            #endregion

            #region 下载完成
            TheDownloader.DownloadFileCompleted += async (s, e) =>
            {
                bool IsDownloadComplete = false;

                Time.Text = "下载完成";

                await Task.Run(() =>
                {
                    try
                    {
                        System.Net.WebClient TheWeb2 = new System.Net.WebClient() { Encoding = Encoding.UTF8 };
                        string Uris = TheDatas.PicUri;
                        TheWeb2.DownloadFile(new Uri(TheDatas.PicUri), SongPic);
                        TheWeb2.Dispose();

                        IsDownloadComplete = true;

                        /*
                        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(SongPic);
                        if (bitmap.Height > 500 || bitmap.Width > 500)
                        {
                            zilongcn.Others.KiResizeImage(bitmap, 500, 500).Save(SongPic);
                        }
                        */
                    }
                    catch { }
                });

                await Task.Run(() =>
                {
                    try
                    {
                        TagLib.File TheFile = TagLib.File.Create(FilePath);
                        TheFile.Tag.Title = TheDatas.Title;
                        TheFile.Tag.Performers = new[] { TheDatas.Artist };
                        TheFile.Tag.Album = TheDatas.Album;
                        TheFile.Tag.AlbumArtists = TheFile.Tag.Performers;
                        TheFile.Tag.Comment = MainWindow.BaseName;

                        if (IsDownloadComplete)
                        {
                            try { TheFile.Tag.Pictures = new[] { new TagLib.Picture(SongPic) }; }
                            catch { }
                        }

                        TheFile.Save();
                        TheFile.Dispose();

                        try { System.IO.File.Delete(SongPic); }
                        catch { }
                    }
                    catch { }
                });

                TheDownloader.Dispose();
                DownloadBtn.IsEnabled = true;
                RetryBtn.IsEnabled = true;
                TheParent.NowDownloadList.Remove(this);
                GC.Collect();
            };
            #endregion
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            StartDownload(SongUri, SongPath);
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            DeleteFile();
        }

        public async void DeleteFile(bool tip = true)
        {
            try
            {
                await Task.Run(() =>
                {
                    System.IO.File.Delete(SongPath);
                });
            }
            catch
            {
                if (tip)
                {
                    TheParent.ShowBox("删除失败", "此路径可能不正确，请重试。");
                    return;
                }
            }

            try { System.IO.File.Delete(SongPath.Replace(SongPath.Split('.')[1], "lrc")); }
            catch { }

            var storyboard = zilongcn.Animations.animateOpacity(this, 1, 0, 0.25, IsAnimate: TheParent.Animation);
            storyboard.Completed += async (s, args) =>
            {
                zilongcn.Animations.animateHeight(this, ActualHeight, 0, 0.25, IsAnimate: TheParent.Animation).Begin();
                await Task.Delay(250);
                TheParent.TheDownload.ListDownload.Items.Remove(this);
                await Task.Delay(500);
                if (TheParent.TheDownload.ListDownload.Items.Count <= 0) TheParent.TheDownload.ShowMainTextAniamte();
                GC.Collect();
            };
            storyboard.Begin();
        }
    }
}
