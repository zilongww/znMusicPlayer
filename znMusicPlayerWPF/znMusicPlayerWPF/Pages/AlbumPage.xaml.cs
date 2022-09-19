using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// AlbumPage.xaml 的交互逻辑
    /// </summary>
    public partial class AlbumPage : UserControl
    {
        private MainWindow TheParent = null;
        private Source TheSource = null;

        public ListPage LastPage = null;

        public AlbumPage(MainWindow Parent, Source theSource)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = theSource;
        }

        public void Set(string rid)
        {
            TheTitle.Text = "加载中";
            TheArtist.Text = "";
            TheData.Text = "";
            TheCard.Text = "";
            TheImage.Source = null;
            GetWeb("http://www.kuwo.cn/api/www/music/musicInfo?mid=" + rid + "&httpsStatus=1&reqId=xxxxxxxxxxxxxxxxxx");
        }

        private void GetWeb(string uri)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(StartGetWeb);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetWebComplete);
            bw.RunWorkerAsync(uri);
        }

        private void StartGetWeb(object sender, DoWorkEventArgs args)
        {
            string RESULT = new Source().GetWebHtml(args.Argument.ToString());
            args.Result = RESULT;
        }

        private async void GetWebComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                zilongcn.Animations.animateOpacity(TheTitle, 0, 1, 0.25, IsAnimate: TheParent.Animation).Begin();
                zilongcn.Animations.animateOpacity(TheArtist, 0, 1, 0.25, IsAnimate: TheParent.Animation).Begin();
                zilongcn.Animations.animateOpacity(TheData, 0, 1, 0.25, IsAnimate: TheParent.Animation).Begin();
                zilongcn.Animations.animateOpacity(TheCard, 0, 1, 0.25, IsAnimate: TheParent.Animation).Begin();

                //TheSource.ShowBox("", e.Result.ToString());
                JToken data = JObject.Parse(e.Result.ToString())["data"];
                //TheSource.ShowBox("", data.ToString());
                TheTitle.Text = data["album"].ToString();
                TheArtist.Text = "创建者: " + data["artist"].ToString();
                TheData.Text = "发行日期: " + data["releaseDate"].ToString();
                TheCard.Text = "专辑简介:\n" + data["albuminfo"].ToString();
                TheImage.Source = await Source.GetImage(data["albumpic"].ToString());
            }
            catch (NullReferenceException)
            {
                TheParent.ShowBox("错误", "获取专辑信息失败。");
            }
            catch (Exception err)
            {
                TheParent.ShowBox("错误", "专辑信息无法和接口兼容。\n\n错误代码：\n" + err.ToString());
            }
        }

        private void ReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            TheParent.ListPages = LastPage;
            TheParent.TheSearchPage.InPage.Content = LastPage;
            TheParent.SetPage("List");
            TheParent.TheSearchPage.SearchPageChangerGrid.Visibility = Visibility.Visible;
            TheParent.TheSearchPage.InPage.Margin = new Thickness(0, 75, 0, 50);
        }
    }
}
