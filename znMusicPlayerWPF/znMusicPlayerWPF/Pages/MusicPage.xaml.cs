using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// MusicPage.xaml 的交互逻辑
    /// </summary>
    public partial class MusicPage : UserControl
    {
        public int LrcSpacing = 22;
        public int LrcSize = 22;
        public TimeSpan NowTime = new TimeSpan();
        public System.Drawing.Color MajorColor = System.Drawing.Color.FromArgb(255, 255, 255, 255);
        Dictionary<long, Tuple<string, string>> NowLrcDictionary = new Dictionary<long, Tuple<string, string>>();
        public string NowPicUrl = "";
        public string NowPlaySongMD5 = null;
        public LrcData NowLrcData = null;
        public LrcData NextLrcData = null;
        private string PrivateNowLrc = null;
        private string PrivateNextLrc = null;
        public bool LrcLoadComplete = true;

        public delegate void LrcChange(LrcData nowLrc, LrcData nextLrc);
        public event LrcChange LrcChangeEvent;

        /// <summary>
        /// 当前播放的歌词
        /// </summary>
        public string NowLrc
        {
            get { return PrivateNowLrc; }
            set
            {
                PrivateNowLrc = value;
            }
        }

        /// <summary>
        /// 当前播放的歌词的下一个歌词
        /// </summary>
        public string NextLrc
        {
            get { return PrivateNextLrc; }
            set
            {
                PrivateNextLrc = value;
            }
        }

        private bool _IshasTranslate = false;
        /// <summary>
        /// 歌词是否有翻译（不一定准确）
        /// </summary>
        public bool IsHasTranslate
        {
            get { return _IshasTranslate; }
            set
            {
                _IshasTranslate = value;
            }
        }

        private bool _IsReCall = false;
        /// <summary>
        /// 是否循环更新歌词（为false将会结束当前所有循环更新歌词）
        /// </summary>
        public bool IsReCall
        {
            get { return _IsReCall; }
            set
            {
                _IsReCall = value;
                if (value) InReCall();
            }
        }

        public bool VolumeDataShow
        {
            get { return SettingDataEdit.ToBool(SettingDataEdit.GetParam("VolumeDataShow")); }
            set
            {
                SettingDataEdit.SetParam("VolumeDataShow", value.ToString());
                VolumeDatasVb.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                VolumeDataBtn.Content = value ? "关闭频谱" : "打开频谱";

                if (TheParent.IsOpenBigPage && VolumeDatasVb.Visibility == Visibility.Visible)
                {
                    VolumeDataActivity();
                }
            }
        }

        public Brush NowLrcColor = new SolidColorBrush(Colors.Pink);
        public Brush OtherLrcColor = new SolidColorBrush(Color.FromArgb(255, 220, 220, 220));
        public TheMusicDatas.MusicData MusicData = new TheMusicDatas.MusicData();

        private MainWindow TheParent = null;
        private Source TheSource = null;

        private MyC.PopupContent.TimeGrid TimeGrid = new MyC.PopupContent.TimeGrid();
        private MyC.PopupWindow TimePopup = new MyC.PopupWindow() { CloseExit = false, IsShowActivated = true, IsDeActivityClose = true, ForceAcrylicBlur = true, isWindowSmallRound = false };

        /// <summary>
        /// 歌词信息
        /// </summary>
        public class LrcData
        {
            public string LrcText = null;
            public long LrcTime = 0;
            public string MD5 = null;
            public TextBlock TranslateLrcText = null;

            /// <summary>
            /// 设置歌词信息
            /// </summary>
            /// <param name="LrcText">歌词内容</param>
            /// <param name="LrcTime">歌词时间</param>
            /// <param name="MD5">歌词md5</param>
            /// <param name="TranslateLrcText">翻译或未翻译歌词的相互TextBlock对象</param>
            public LrcData(string LrcText, long LrcTime, string MD5 = null, TextBlock TranslateLrcText = null)
            {
                this.LrcText = LrcText;
                this.LrcTime = LrcTime;
                this.MD5 = MD5;
                this.TranslateLrcText = TranslateLrcText;
            }

            public override string ToString()
            {
                return LrcTime.ToString();
            }
        }

        bool gettingVolumeData = false;
        private async void VolumeDataActivity()
        {
            if (gettingVolumeData) return;
            gettingVolumeData = true;
            while (VolumeDataShow)
            {
                if (Back1.Visibility != Visibility.Visible || !TheParent.IsOpenBigPage || !TheParent.WindowIsOpen() || TheParent.NowPlayState == MainWindow.PlayState.Pause)
                {
                    break;
                }

                try
                {
                    VolumeDataChangeActivity(TheParent.audioPlayer.GetNowVolumeData());
                }
                catch { }
                await Task.Delay(10);
            }
            gettingVolumeData = false;
        }

        private void VolumeDataChangeActivity(double[] data)
        {
            if (data == null) return;

            if (VolumeDatasStackp.Children.Count != data.Length)
            {
                VolumeDatasStackp.Children.Clear();

                foreach (var i in Enumerable.Range(0, data.Length))
                {
                    VolumeDatasStackp.Children.Add(
                        new System.Windows.Shapes.Rectangle()
                        {
                            Fill = FindResource("ButtonPAMP") as SolidColorBrush,
                            Margin = new Thickness(4, 0, 0, 0),
                            Height = 84,
                            Width = 2,
                            MinHeight = 0,
                            MaxHeight = 104,
                            VerticalAlignment = VerticalAlignment.Bottom
                        }
                    );
                }

                return;
            }

            int counts = 0;
            foreach (System.Windows.Shapes.Shape shape in VolumeDatasStackp.Children)
            {
                try
                {
                    if (shape.Visibility == Visibility.Collapsed) continue;

                    double v = shape.Height;
                    double a = data[counts];//(double)border.Tag;
                    counts++;

                    v -= 0.9;

                    if (a > v)
                    {
                        v += a / 15 + 0.9;
                    }
                    else if (a < v)
                    {
                        v -= a / 15 + 0.9;
                    }

                    if (v <= 1) v = 1;

                    shape.Height = v;
                }
                catch { }
            }

            data = null;
            //GC.Collect();
        }

        public MusicPage(MainWindow Parent, Source theSource)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = theSource;
            LrcList.SizeChanged += LrcList_SizeChanged;
            if (BackImage != null) BackImage.Effect = new BlurEffect() { Radius = TheParent.Blurs, RenderingBias = TheParent.quality };
            TimePopup.Content = TimeGrid;

            //TheTimer.Interval = new TimeSpan(0, 0, 0, 0, 85);

            SizeChanged += MusicPage_SizeChanged;
            TheParent.PlayStateChangeEvent += (a) =>
            {
                if (a == MainWindow.PlayState.Play)
                    VolumeDataShow = VolumeDataShow;
            };
            TheParent.WindowStateChangedEvent += () =>
            {
                VolumeDataShow = VolumeDataShow;
            };

            TimeUpdata();
        }

        private void MusicPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        /// <summary>
        /// 设置歌词界面的内容
        /// </summary>
        /// <param name="data">歌曲信息</param>
        /// <param name="Animate">是否开启动画</param>
        public void Set(TheMusicDatas.MusicData data, bool Animate = true)
        {
            double animateTime = 0.34;
            int waitTime = 250;

            SetBlur(TheParent.Blurs);

            MusicData = data;

            NextMusicDataAniamte(Animate, animateTime, waitTime);
            NextMusicPicAnimate(data, Animate, animateTime, waitTime);
        }

        private void NextMusicDataAniamte(bool animate = true, double animateTime = 0.23, int waitTime = 250)
        {
            var st = zilongcn.Animations.animateOpacity(sv, sv.Opacity, 0, animateTime, IsAnimate: animate);
            st.Completed += async (s, e) =>
            {
                if (MusicData.SongRid != NowPlaySongMD5)
                {
                    GetSetMusicLrc(MusicData);
                    NowPlaySongMD5 = MusicData.MD5;
                }

                await Task.Delay(waitTime);

                zilongcn.Animations.animateOpacity(sv, sv.Opacity, 1, animateTime, IsAnimate: animate).Begin();
            };
            st.Begin();
        }

        private void NextMusicPicAnimate(TheMusicDatas.MusicData musicData, bool animate = true, double animateTime = 0.23, int waitTime = 250)
        {
            if (NowPicUrl != musicData.PicUri || MusicData.From == TheMusicDatas.MusicFrom.localMusic)
            {
                NowPicUrl = MusicData.PicUri;

                sv.Visibility = Visibility.Visible;
                zilongcn.Animations animations = new zilongcn.Animations(animate);
                animations.animateOpacity(TheImage, TheImage.Opacity, 0, animateTime);
                animations.animateOpacity(TheParent.PlayMusicAlbumPic, TheParent.PlayMusicAlbumPic.Opacity, 0, animateTime);

                animations.storyboard.Completed += async (s, e) =>
                {
                    TheImage.Source = null;
                    TheParent.PlayMusicAlbumPic.Source = null;
                    GC.Collect();

                    BitmapImage bitmapImage = null;

                    if (MusicData.From == TheMusicDatas.MusicFrom.localMusic)
                        bitmapImage = await Source.GetImage(MusicData.IsDownload, "local");
                    else
                        bitmapImage = await Source.GetImage(MusicData.PicUri);

                    if (MusicData.MD5 != musicData.MD5)
                    {
                        bitmapImage = null;
                        return;
                    }

                    TheImage.Source = bitmapImage;
                    TheParent.PlayMusicAlbumPic.Source = bitmapImage;

                    await Task.Delay(waitTime);

                    animations = new zilongcn.Animations(animate);
                    animations.animateOpacity(TheImage, 0, 1, animateTime);
                    animations.animateOpacity(TheParent.PlayMusicAlbumPic, 0, 1, animateTime);
                    animations.Begin();
                };

                animations.Begin();
            }
        }

        private bool IsInRecall = false;
        private async void InReCall()
        {
            if (IsInRecall) return;
            IsInRecall = true;

            while (true)
            {
                if (IsReCall)
                {
                    if (TheParent.NowPlayState != MainWindow.PlayState.Pause && LrcLoadComplete) ReCallTimer();

                    await Task.Delay(10);
                }
                else
                {
                    IsInRecall = false;
                    break;
                }
            }
        }

        private void LrcList_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (TextBlock TheLrc in LrcList.Children)
            {
                TheLrc.MaxWidth = LrcList.ActualWidth;
            }
        }

        private decimal HeightOffset = 0;
        private decimal SizeOffset = 0;
        private decimal SpacingOffset = 0;
        private decimal AllSizeSpacingOffset = 0;
        private decimal AllOffset = 0;
        private int Index = 0;

        int lrcIndex = 6;

        /// <summary>
        /// 更新歌词显示
        /// </summary>
        public void ReCallTimer()
        {
            if (TheParent.NowPlaySong.SongRid == MusicData.SongRid)
            {
                if (!TheParent.WindowIsOpen() && !TheParent.LrcWindow.IsShow) return;

                StackPanel LrcListBox = LrcList;
                MyC.znScrollViewer svs = sv;
                long NowPlayingTime = TheParent.audioPlayer.NowMusicPosition.Ticks;
                TextBlock Lrc = null;

                LrcData testData = null;
                LrcData lrcData = null;
                foreach (TextBlock TheLrc in LrcListBox.Children)
                {
                    try
                    {
                        // 用类型判断优化性能
                        var lrcTag = TheLrc.Tag;
                        if (lrcTag.GetType() != typeof(LrcData)) continue;

                        testData = lrcTag as LrcData;

                        if (testData.LrcTime < NowPlayingTime)
                        {
                            Lrc = TheLrc;
                            lrcData = lrcTag as LrcData;
                        }
                        else
                        {
                            // 优化性能
                            if (NowLrcData != null)
                            {
                                if (NowLrcData.MD5 == null && lrcData.MD5 == null) continue;
                                if (NowLrcData.MD5 == lrcData.MD5) break;
                            }

                            bool IsOpen = TheParent.IsOpenBigPage && TheParent.WindowIsOpen();
                            lrcIndex = LrcListBox.Children.IndexOf(Lrc);

                            if (lrcData.TranslateLrcText == null)
                            {
                                NowLrcData = lrcData;
                                NextLrcData = (LrcListBox.Children[lrcIndex + 2] as TextBlock).Tag as LrcData;

                            }
                            else
                            {
                                NowLrcData = lrcData;
                                NextLrcData = (LrcListBox.Children[lrcIndex - 1] as TextBlock).Tag as LrcData;
                            }

                            NowLrc = lrcData.LrcText;

                            if (NextLrcData != null)
                            {
                                NextLrc = NextLrcData.LrcText;
                            }

                            if (LrcChangeEvent != null)
                            {
                                if (lrcData.TranslateLrcText == null) LrcChangeEvent(NowLrcData, NextLrcData);
                                else LrcChangeEvent(NextLrcData, NowLrcData);
                            }

                            if (IsOpen)
                            {
                                Lrc.Foreground = TheParent.LrcColor;
                                Lrc.TextWrapping = TextWrapping.Wrap;
                                Lrc.TextTrimming = TextTrimming.None;

                                if (lrcData.TranslateLrcText != null)
                                {
                                    TextBlock t = LrcListBox.Children[lrcIndex - 1] as TextBlock;

                                    t.Foreground = TheParent.LrcColor;
                                    t.TextWrapping = TextWrapping.Wrap;
                                    t.TextTrimming = TextTrimming.None;
                                }


                                TextBlock lt = LrcListBox.Children[lrcIndex - 2] as TextBlock;
                                TextBlock lt1 = LrcListBox.Children[lrcIndex - 3] as TextBlock;
                                TextBlock lt2 = LrcListBox.Children[lrcIndex - 4] as TextBlock;

                                lt.Foreground = TheParent.OtherColor;
                                lt.TextWrapping = TextWrapping.NoWrap;
                                lt.TextTrimming = TextTrimming.CharacterEllipsis;

                                lt1.Foreground = TheParent.OtherColor;
                                lt1.TextWrapping = TextWrapping.NoWrap;
                                lt1.TextTrimming = TextTrimming.CharacterEllipsis;

                                lt2.Foreground = TheParent.OtherColor;
                                lt2.TextWrapping = TextWrapping.NoWrap;
                                lt2.TextTrimming = TextTrimming.CharacterEllipsis;


                                Vector vector = VisualTreeHelper.GetOffset(TheLrc);
                                Point currentPoint = new Point(vector.X, vector.Y);
                                double Index = vector.Y - svs.ViewportHeight / 2 + (IsHasTranslate ? -(TheLrc.ActualHeight * 2.4) : -24);
                                if (lrcData.TranslateLrcText != null)
                                {
                                    svs.VerticalScrollValue = Index;
                                }
                                else
                                {
                                    svs.VerticalScrollValue = Index;
                                }
                            }

                            break;
                        }

                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 将指定item移动到控件中心
        /// </summary>
        /// <param name="ListBoxHeight"></param>
        /// <param name="ContentSize"></param>
        /// <param name="ContentSpacing"></param>
        /// <param name="ContentIndex"></param>
        /// <returns>位于歌词列表中间的歌词的索引</returns>
        public int MathGetCenterContentIndex(double ListBoxHeight, int ContentSize, int ContentSpacing, int ContentIndex)
        {
            HeightOffset = (decimal)ListBoxHeight / 60;
            HeightOffset = Math.Round(HeightOffset, 2);

            /*
            SizeOffset = 0.97m / (ContentSize / 22m);
            SizeOffset = Math.Round(SizeOffset, 2);

            SpacingOffset = 0.97m / (ContentSpacing / 22m);
            SpacingOffset = Math.Round(SpacingOffset, 2);

            AllSizeSpacingOffset = SizeOffset * SpacingOffset;
            AllSizeSpacingOffset = Math.Round(AllSizeSpacingOffset, 2);
            */

            AllOffset = HeightOffset; //* AllSizeSpacingOffset;
            AllOffset = Math.Round(AllOffset, 2);

            Index = ContentIndex + (int)AllOffset;

            if (TheParent.Debug)
            {
                OffsetDebug.Visibility = Visibility.Visible;
                OffsetDebug.Text = $" Height: {LrcList.ActualHeight} \n LrcSize: {LrcSize} \n LrcSpacing: {LrcSpacing} \n HeightOffset: {HeightOffset} \n SizeOffset: {SizeOffset} \n SpacingOffset: {SpacingOffset} \n AllSizeSpacingOffset: {AllSizeSpacingOffset} \n AllOffset: {AllOffset} \n AddIndex: {Index - ContentIndex} \n LrcIndex: {ContentIndex} \n Index: {Index}\nWindowSize: {TheParent.ActualWidth}x{TheParent.ActualHeight}";
            }
            else
            {
                OffsetDebug.Visibility = Visibility.Collapsed;
            }

            return Index;
        }

        /// <summary>
        /// 设置歌曲界面背景高斯模糊数值
        /// </summary>
        /// <param name="Radius">高斯模糊数值</param>
        public void SetBlur(int Radius)
        {
            TheParent.Blurs = Radius;
            if (BackImage != null) BackImage.Effect = new BlurEffect() { Radius = TheParent.Blurs, RenderingBias = TheParent.quality };
        }

        /// <summary>
        /// 清空所有位于歌词列表的歌词的颜色
        /// </summary>
        public void CleanLrcColor()
        {
            foreach (TextBlock TheLrc in LrcList.Children)
            {
                TheLrc.Foreground = OtherLrcColor;
            }
        }

        /// <summary>
        /// 获取歌词
        /// </summary>
        /// <param name="rid"></param>
        private async void GetSetMusicLrc(TheMusicDatas.MusicData musicData)
        {
            if (musicData.MD5 == lrcMusicData.MD5) return;

            LrcLoadComplete = false;

            LrcList.Children.Clear();

            /*
            for (int i = 0; i < LrcList1.Children.Count; i++)
            {
                LrcList1.Children.RemoveAt(i);
                LrcList1.Items.RemoveAt(i);
                i--;
            }
            */

            string Result = null;

            await Task.Run(() =>
            {
                if (musicData.From == TheMusicDatas.MusicFrom.kwMusic)
                {
                    try
                    {
                        //Lyrics = JObject.Parse(new Network.znWebClient().GetString($"http://m.kuwo.cn/newh5/singles/songinfoandlrc?musicId={musicData.SongRid}&httpsStatus=1&reqId=84555600-3ddb-11ec-879c-27a53f4cf873"));
                        Result = new Network.znWebClient().GetString($"http://m.kuwo.cn/newh5/singles/songinfoandlrc?musicId={musicData.SongRid}&httpsStatus=1&reqId=84555600-3ddb-11ec-879c-27a53f4cf873"); //Lyrics.ToString();
                    }
                    catch { }
                }
                else if (musicData.From == TheMusicDatas.MusicFrom.neteaseMusic)
                {
                    try
                    {
                        //JObject Lrc = JObject.Parse(TheSource.GetNeteaseMusicLrc(musicData.SongRid));
                        //Result = Lrc["lrc"]["lyric"].ToString();
                        //if (Translate == "") Translate = null;

                        Result = TheSource.GetNeteaseMusicLrc(musicData.SongRid);
                    }
                    catch { }
                }
                else if (musicData.From == TheMusicDatas.MusicFrom.kgMusic)
                {
                    Result = musicData.ThekgMusicLrcs;
                }
                else if (musicData.From == TheMusicDatas.MusicFrom.qqMusic)
                {
                    if (Source.InternetConnect())
                    {
                        try
                        {
                            System.Net.WebClient webClient = new System.Net.WebClient() { Encoding = System.Text.Encoding.UTF8 };
                            webClient.Headers.Add("Referer", "https://y.qq.com/portal/player.html");
                            string lrc = webClient.DownloadString($"https://api.injahow.cn/meting/?server=tencent&type=lrc&id={musicData.SongRid}"); //webClient.DownloadString($"https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg?songmid={musicData.SongRid}&format=json&nobase64=1");
                            webClient.Dispose();
                            Result = lrc;//JObject.Parse(lrc)["lyric"].ToString();
                        }
                        catch { Result = null; }
                    }
                    else Result = null;
                }
                else if (musicData.From == TheMusicDatas.MusicFrom.miguMusic)
                {
                    if (Source.InternetConnect())
                    {
                        Result = MusicAddressGet.GetMiguMusicLrc(musicData);
                    }
                }
                else if (musicData.From == TheMusicDatas.MusicFrom.localMusic)
                {
                    string LrcPath = musicData.IsDownload.Split('.')[0] + ".lrc";
                    if (System.IO.File.Exists(LrcPath)) Result = System.IO.File.ReadAllText(LrcPath, zilongcn.Others.GetType(LrcPath));
                }
            });

            await GetLrcComplete(Result, musicData);
        }

        TheMusicDatas.MusicData lrcMusicData = new TheMusicDatas.MusicData();

        /// <summary>
        /// 获取歌词结束后歌词显示操作
        /// </summary>
        /// <param name="Result"></param>
        /// <param name="Translate"></param>
        private async Task GetLrcComplete(string Result, TheMusicDatas.MusicData musicData)
        {
            lrcMusicData = musicData;

            LrcList.Children.Clear();

            LrcData NullDataLrc = new LrcData("", 0);

            if (MusicData.SongRid != MusicData.SongRid) return;

            Dictionary<long, Tuple<string, string>> LrcDictionary = new Dictionary<long, Tuple<string, string>>();

            if (Result == null || Result == "" || Result == "{}")
            {
                string TheText = Source.InternetConnect() ? "歌曲无歌词" : "网络未连接，无法显示歌词";

                LrcList.Children.Add(new TextBlock() { Text = TheText, FontSize = LrcSize, Focusable = false, Foreground = OtherLrcColor, Tag = NullDataLrc, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Left, Background = null });
                NowLrc = "歌曲无歌词";
                NextLrc = "";

                TheParent.LrcWindow.MakeCenter(true);

                IsReCall = false;
                return;
            }

            int anl = 0;
            int AddTranslate = 0;
            long LastLrcTime = 0;
            string neteaseLineLyricText = null;
            string neteaseTranslateText = null;
            if (musicData.From != TheMusicDatas.MusicFrom.kwMusic) Result += "[100:00:00.00]Debuging╯︿╰\n";

            #region 解析歌词并转换成字典
            bool ResultState = await Task.Run(() =>
            {
                if (musicData.From == TheMusicDatas.MusicFrom.kwMusic)
                {
                    var lrcLists = JObject.Parse(Result)["data"]["lrclist"];

                    foreach (var a in lrcLists)
                    {
                        JObject data = JObject.Parse(a.ToString());

                        string lineLyric = data["lineLyric"].ToString();
                        string timeString = data["time"].ToString();
                        string millisecounds = timeString.Split('.')[1];

                        switch (millisecounds.Count())
                        {
                            case 1:
                                millisecounds = millisecounds + "00";
                                break;
                            case 2:
                                millisecounds = millisecounds + "0";
                                break;
                            default:
                                millisecounds = millisecounds.Substring(0, 3);
                                break;
                        }

                        long time = new TimeSpan(0, 0, 0, int.Parse(timeString.Split('.')[0]), int.Parse(millisecounds)).Ticks;

                        if (LrcDictionary.ContainsKey(time))
                        {
                            string TranslateLrc = LrcDictionary[time].Item1;
                            LrcDictionary.Remove(time);
                            LrcDictionary.Add(time, new Tuple<string, string>(lineLyric, TranslateLrc));
                            AddTranslate++;
                        }
                        else
                        {
                            LrcDictionary.Add(time, new Tuple<string, string>(lineLyric, null));
                        }
                    }
                }
                else
                {
                    foreach (string a in Result.Split('\n'))
                    {
                        try
                        {
                            string i = a.Replace("\r", "");
                            string[] Lrcs = i.Split(']');
                            int LrcTimeCount = Lrcs.Count();

                            // 加载多句重复歌词(如果有)
                            for (int c = 0; c <= LrcTimeCount; c++)
                            {
                                string NowLoadLrc = Lrcs[c];

                                if (!NowLoadLrc.Contains("[")) continue;

                                string time = NowLoadLrc.Replace("[", "");
                                string lineLyric = Lrcs.Last();

                                Tuple<string, string> TranText = new Tuple<string, string>(lineLyric, null);

                                if (lineLyric == "") continue;

                                int Min = int.Parse(time.Split(':')[0]);
                                int Sec = 0;
                                int MinSec = 0;

                                try
                                {
                                    Sec = int.Parse(time.Split(':')[1].Split('.')[0]);
                                    MinSec = int.Parse(time.Split(':')[1].Split('.')[1]);
                                    MinSec = MinSec >= 100 ? MinSec : MinSec * 10;
                                }
                                catch
                                {
                                    Sec = int.Parse(time.Split(':')[1]);
                                }

                                long TotalTime = new TimeSpan(0, 0, Min, Sec, MinSec).Ticks;

                                // 检查是否有翻译
                                if (LrcDictionary.ContainsKey(TotalTime))
                                {
                                    LrcDictionary.TryGetValue(TotalTime, out TranText);
                                    LrcDictionary.Remove(TotalTime);
                                    LrcDictionary.Add(TotalTime, new Tuple<string, string>(lineLyric, TranText.Item1));
                                    AddTranslate++;
                                }
                                else if ((musicData.From == TheMusicDatas.MusicFrom.neteaseMusic || musicData.From == TheMusicDatas.MusicFrom.qqMusic) || neteaseTranslateText != null)
                                {
                                    try
                                    {
                                        LrcDictionary.Add(TotalTime, new Tuple<string, string>(lineLyric, neteaseTranslateText));
                                    }
                                    catch
                                    {
                                        LrcDictionary.Add(TotalTime, new Tuple<string, string>(lineLyric, null));
                                    }

                                    neteaseTranslateText = null;
                                }
                                else
                                {
                                    LrcDictionary.Add(TotalTime, new Tuple<string, string>(lineLyric, null));
                                }

                                if ((musicData.From == TheMusicDatas.MusicFrom.neteaseMusic || musicData.From == TheMusicDatas.MusicFrom.qqMusic) && lineLyric.Contains(" (") && lineLyric.Contains(")"))
                                {
                                    try
                                    {
                                        string[] splitString = new string[] { " (" };
                                        string translateText = lineLyric.Split(splitString, StringSplitOptions.None)[1].Replace(")", "");

                                        neteaseLineLyricText = lineLyric.Replace(" (" + translateText + ")", "");

                                        neteaseTranslateText = translateText;

                                        AddTranslate++;
                                        //LrcDictionary[TotalTime] = new Tuple<string, string>(neteaseLineLyricText, ne);
                                        //LrcDictionary.Add(TotalTime, new Tuple<string, string>(translateText, null));
                                    }
                                    catch { neteaseTranslateText = null; }
                                }
                            }
                        }
                        catch { }
                    }
                }

                // neteaseMusic清除歌词翻译
                if ((musicData.From == TheMusicDatas.MusicFrom.neteaseMusic || musicData.From == TheMusicDatas.MusicFrom.qqMusic) && AddTranslate >= 10)
                {
                    LrcDictionary.Add(LrcDictionary.Last().Key + 10, new Tuple<string, string>("", ""));

                    Dictionary<long, Tuple<string, string>> CopyLrcDictionary = new Dictionary<long, Tuple<string, string>>();
                    foreach (var i in LrcDictionary)
                    {
                        CopyLrcDictionary.Add(i.Key, i.Value);
                    }

                    foreach (var value in CopyLrcDictionary)
                    {
                        foreach (var i in CopyLrcDictionary)
                        {
                            string a = $" ({i.Value.Item2})";
                            //MessageBox.Show($"{a}\n{value.Value.Item1}\n{value.Value.Item1.Replace(a, "")}");

                            try
                            {
                                if (LrcDictionary[value.Key].Item1.Contains(a))
                                {
                                    LrcDictionary.Remove(value.Key);
                                    LrcDictionary.Add(value.Key, new Tuple<string, string>(value.Value.Item1.Replace(a, ""), value.Value.Item2));
                                    break;
                                    //LrcDictionary[value.Key] = new Tuple<string, string>(value.Value.Item1.Replace(a, ""), value.Value.Item2);
                                }
                            }
                            catch { }
                        }
                        //neteaseTranslateText = value.Value.Item2;
                    }
                }

                // 以字典Key值顺序排序
                var linqSeq = from pair in LrcDictionary orderby pair.Key ascending select pair;
                LrcDictionary = new Dictionary<long, Tuple<string, string>>();

                foreach (var value in linqSeq)
                {
                    LrcDictionary.Add(value.Key, value.Value);
                }
                //LrcDictionary.OrderBy(o => o.Key).ToDictionary(o => o.Key, p => p.Value);

                // 判断为true时为有翻译
                if (AddTranslate >= 10)
                {
                    TheParent.LrcWindow.IsTranslate = true;
                    IsHasTranslate = true;
                    Dictionary<long, Tuple<string, string>> CopyDictionary = new Dictionary<long, Tuple<string, string>>();
                    foreach (KeyValuePair<long, Tuple<string, string>> kwTranslateDebug in LrcDictionary)
                    {
                        CopyDictionary.Add(kwTranslateDebug.Key, kwTranslateDebug.Value);
                    }

                    foreach (KeyValuePair<long, Tuple<string, string>> kwTranslateDebug in CopyDictionary)
                    {
                        bool IsFound = false;
                        Tuple<string, string> TranText = new Tuple<string, string>(kwTranslateDebug.Value.Item1, null);

                        foreach (KeyValuePair<long, Tuple<string, string>> kwTranslateNext in LrcDictionary)
                        {
                            if (IsFound)
                            {
                                LrcDictionary.TryGetValue(kwTranslateNext.Key, out TranText);
                                LrcDictionary.Remove(kwTranslateDebug.Key);
                                LrcDictionary.Add(kwTranslateDebug.Key, new Tuple<string, string>(kwTranslateDebug.Value.Item1, TranText.Item2));
                                break;
                            }
                            if (kwTranslateNext.Key == kwTranslateDebug.Key) IsFound = true;
                        }
                    }
                }
                else { TheParent.LrcWindow.IsTranslate = false; IsHasTranslate = false; }

                return true;
            });

            if (!ResultState) return;
            #endregion

            #region 添加歌词列表顶部间距
            for (int TheNumber = 0; TheNumber < 10; TheNumber++)
            {
                LrcList.Children.Add(new TextBlock() { FontSize = LrcSpacing, Focusable = false, Width = 0, Text = "", Tag = NullDataLrc, Background = null });
            }
            #endregion

            if (MusicData.MD5 != musicData.MD5) return;

            #region 从转换的歌词字典中添加到歌词列表
            foreach (KeyValuePair<long, Tuple<string, string>> ALrc in LrcDictionary)
            {
                string LrcText = System.Web.HttpUtility.HtmlDecode(ALrc.Value.Item1);
                string TranslateText = System.Web.HttpUtility.HtmlDecode(ALrc.Value.Item2);
                long LrcTime = ALrc.Key;
                string LrcMD5 = zilongcn.Others.ToMD5(LrcText + TranslateText + LrcTime);

                LrcData lrcData = new LrcData(LrcText, LrcTime, LrcMD5);

                TextBlock TheTextBlock = new TextBlock() { Text = LrcText, FontSize = LrcSize, FontWeight = FontWeights.Normal, Foreground = OtherLrcColor, Tag = lrcData, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Left, TextWrapping = TextWrapping.NoWrap, TextTrimming = TextTrimming.CharacterEllipsis, Focusable = false, Background = null };
                TextBlock TheTextBlock1 = new TextBlock() { Text = LrcText, FontSize = LrcSize, FontWeight = FontWeights.Normal, Foreground = OtherLrcColor, Tag = lrcData, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Left, TextWrapping = TextWrapping.NoWrap, TextTrimming = TextTrimming.CharacterEllipsis, Focusable = false, Background = null };

                TextBlock TheTranslateBlock = null, TheTranslateBlock1 = null;
                if (TranslateText != null)
                {
                    LrcData TranslateData = new LrcData(TranslateText, LrcTime, LrcMD5);

                    TheTranslateBlock = new TextBlock() { Text = TranslateText, FontSize = LrcSize, FontWeight = FontWeights.Normal, Foreground = OtherLrcColor, Tag = TranslateData, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Left, TextWrapping = TextWrapping.NoWrap, TextTrimming = TextTrimming.CharacterEllipsis, Focusable = false, Background = null };
                    TheTranslateBlock1 = new TextBlock() { Text = TranslateText, FontSize = LrcSize, FontWeight = FontWeights.Normal, Foreground = OtherLrcColor, Tag = TranslateData, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Left, TextWrapping = TextWrapping.NoWrap, TextTrimming = TextTrimming.CharacterEllipsis, Focusable = false, Background = null };
                }

                if (LrcText != "//" && LrcText != "腾讯享有本翻译作品的著作权" && LrcText != "" && LrcText != "纯音乐，请欣赏" && LrcText != "Debuging╯︿╰")
                {
                    LrcList.Children.Add(TheTextBlock);
                    if (TranslateText != null)
                    {
                        LrcList.Children.Add(TheTranslateBlock);
                        (TheTextBlock.Tag as LrcData).TranslateLrcText = TheTranslateBlock;
                        (TheTextBlock1.Tag as LrcData).TranslateLrcText = TheTranslateBlock1;
                        (TheTranslateBlock.Tag as LrcData).TranslateLrcText = TheTextBlock;
                        (TheTranslateBlock1.Tag as LrcData).TranslateLrcText = TheTextBlock1;
                    }
                }

                LrcList.Children.Add(new TextBlock() { FontSize = 30, Width = 0, Text = "", Tag = "10000.0", Focusable = false, Background = null });

                anl += 1;

                LastLrcTime = LrcTime;

                //await Task.Delay(1);
            }

            LrcList.Children.Add(new TextBlock() { FontSize = LrcSpacing, Text = "", Tag = new LrcData("", LastLrcTime + 5000000, "null"), Focusable = false, Background = null });
            #endregion

            #region 添加歌词列表尾部间距
            for (int TheNumber = 0; TheNumber < 10; TheNumber++)
            {
                LrcList.Children.Add(new TextBlock() { FontSize = LrcSpacing, Text = "", Tag = NullDataLrc, Focusable = false, Background = null });
                //await Task.Delay(1);
            }
            #endregion

            await Task.Delay(20);
            try { sv.VerticalScrollValue = 100; }
            catch { }

            bool IsRubbishMusic = false;
            foreach (string i in TheParent.垃圾音乐黑名单)
            {
                if (MusicData.Title == i) IsRubbishMusic = true;
            }

            if (anl == 0 || IsRubbishMusic)
            {
                LrcList.Children.Clear();

                string TheText = "歌曲无歌词";
                if (IsRubbishMusic) TheText = "垃圾歌曲不配拥有歌词";

                LrcList.Children.Add(new TextBlock() { Text = TheText, FontSize = LrcSize, Foreground = OtherLrcColor, Tag = NullDataLrc, VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Left, Focusable = false, Background = null });

                IsReCall = false;

                NowLrc = "歌曲无歌词";
                NextLrc = "";

                TheParent.LrcWindow.MakeCenter(true);
            }

            NowLrcDictionary = LrcDictionary;
            LrcLoadComplete = true;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!TheParent.IsFullScreen)
            {
                base.OnMouseLeftButtonDown(e);
                try
                {
                    TheParent.DragMove();
                }
                catch { }
            }
        }

        private void LrcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            try
            {
                if (((TextBlock)LrcList.SelectedItem).Text != "歌曲无歌词")
                {
                    if (((TextBlock)LrcList.SelectedItem).Text != "")
                    {
                        LrcData lrcData = ((TextBlock)LrcList.SelectedItem).Tag as LrcData;
                        long TheTag = lrcData.LrcTime;
                        TimeSpan TheTime = new TimeSpan(TheTag);

                        TheParent.audioPlayer.NowMusicPosition = TheTime;
                        CleanLrcColor();
                    }
                }
            }
            catch { }*/
        }

        private void LrcList1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            try
            {
                if (((TextBlock)LrcList1.SelectedItem).Text != "歌曲无歌词")
                {
                    if (((TextBlock)LrcList1.SelectedItem).Text != "")
                    {
                        LrcData lrcData = ((TextBlock)LrcList1.SelectedItem).Tag as LrcData;
                        long TheTag = lrcData.LrcTime;
                        TimeSpan TheTime = new TimeSpan(TheTag);

                        TheParent.audioPlayer.NowMusicPosition = TheTime;
                        CleanLrcColor();
                    }
                }
            }
            catch { }*/
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TheParent.ShowBox(MusicData);
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!TimeButton.IsFocused)
            {
                TheParent.IsFullScreen = !TheParent.IsFullScreen;
            }
        }

        private void TimeButton_Click(object sender, RoutedEventArgs e)
        {
            return;
            Point point = TheParent.WindowButton_Close.PointToScreen(new Point(0, 0));
            double x = point.X - 200 + TheParent.WindowButton_Close.ActualWidth - 5;
            double y = point.Y + TheParent.WindowButton_Close.ActualHeight + 5;

            TimePopup.UserShow(x, y);
            ReCallTimeUpdata();
        }

        public void TimeUpdata(bool IsSort = false)
        {
            string APM = "上午";
            int Hour = DateTime.Now.Hour;
            string Min = DateTime.Now.Minute.ToString();
            string Sec = DateTime.Now.Second.ToString();
            if (Hour >= 13) { Hour -= 12; APM = "下午"; }
            if (Min.Count() == 1) Min = $"0{Min}";
            if (Sec.Count() == 1) Sec = $"0{Sec}";

            TimeButton.Content = $"{APM} {Hour}:{Min}";
            if (IsSort) return;
            TimeGrid.TimeGrid1.Time_HourMin.Text = $"{APM} {Hour}:{Min}";
            TimeGrid.TimeGrid1.Time_Sec.Text = $"{Sec}";
            TimeGrid.TimeGrid1.Time_YearMouth.Text = $"{DateTime.Now.Year}年{DateTime.Now.Month}月{DateTime.Now.Day}日 {System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(DateTime.Now.DayOfWeek)}";
        }

        public bool IsReCallTime = false;
        public async void ReCallTimeUpdata()
        {
            if (IsReCallTime) return;
            IsReCallTime = true;
            while (IsReCallTime && TimePopup.IsShow)
            {
                if (TheParent.WindowIsOpen() && TheParent.IsOpenBigPage)
                {
                    if (TimeGrid.Visibility == Visibility.Visible)
                    {
                        TimeUpdata();
                        await Task.Delay(250);
                    }
                    else
                    {
                        IsReCallTime = false;
                        break;
                    }
                }
                else
                {
                    IsReCallTime = false;
                    break;
                }
            }
        }

        private void VolumeDataBtn_Click(object sender, RoutedEventArgs e)
        {
            VolumeDataShow = !VolumeDataShow;
        }

        private void ParallaxImageLayer_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void TheImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TheImage.Clip = new RectangleGeometry(new Rect(0, 0, TheImage.ActualWidth, TheImage.ActualHeight), 8, 8);
        }
    }
}
