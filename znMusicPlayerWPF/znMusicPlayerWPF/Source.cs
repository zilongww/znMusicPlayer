using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using znMusicPlayerWPF.MusicPlay;

namespace znMusicPlayerWPF
{
    public class Source
    {
        public MainWindow Parent = null;
        public Source Set(MainWindow TheParent)
        {

            Parent = TheParent;
            return this;
        }
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern int NtSetInformationProcess(IntPtr hProcess, int processInformationClass, ref int processInformation, int processInformationLength);

        public void BSOD()
        {
            int isCritical = 1;  // we want this to be a Critical Process
            int BreakOnTermination = 0x1D;  // value for BreakOnTermination (flag)

            try
            {
                Process.EnterDebugMode();  //acquire Debug Privileges
            }
            catch
            {
                Parent.ShowBox("权限不足", "请以管理员身份运行此程序。");
                return;
            }

            // setting the BreakOnTermination = 1 for the current process
            NtSetInformationProcess(Process.GetCurrentProcess().Handle, BreakOnTermination, ref isCritical, sizeof(int));

            Parent.MenuExit_Click(null, null);
        }

        public void ShowBox(string Title, string Text)
        {
            Parent.Flash();
            System.Windows.MessageBox.Show(Text, Title);
        }

        //连接到uri指示的链接, 并返回string类型的网页源代码
        public string GetWebHtml(string uri, string referer = "")
        {
            bool com = false;
            int MaxTimes = 0;
            while (com == false)
            {
                try
                {
                    MaxTimes += 1;
                    if (MaxTimes > 15)
                    {
                        com = true;
                    }
                    WebClient wb = new WebClient() { Encoding = Encoding.UTF8 };
                    wb.Headers.Add("Accept", "*/*");
                    wb.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
                    wb.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36 Edg/88.0.705.63");
                    wb.Headers.Add("Cookie", "_ga=GA1.2.1083049585.1590317697; _gid=GA1.2.2053211683.1598526974; _gat=1; Hm_lvt_cdb524f42f0ce19b169a8071123a4797=1597491567,1598094297,1598096480,1598526974; Hm_lpvt_cdb524f42f0ce19b169a8071123a4797=1598526974; kw_token=HYZQI4KPK3P");
                    wb.Headers.Add("Referer", referer);
                    wb.Headers.Add("csrf", "HYZQI4KPK3P");
                    string theHtml = wb.DownloadString(uri);
                    com = true;
                    wb.Dispose();

                    return theHtml;
                }
                catch
                {
                    //TheSource.ShowBox("连接出错", err.ToString());
                }
            }
            return "{}";
        }

        public string GetMusicInfo(string MusicId)
        {
            return GetWebHtml("http://www.kuwo.cn/api/www/music/musicInfo?mid=" + MusicId);
        }

        public string GetMusicLrc(string MusicId)
        {
            return GetWebHtml("http://m.kuwo.cn/newh5/singles/songinfoandlrc?musicId=" + MusicId);
        }

        public string GetNeteaseMusicLrc(string MusicId)
        {
            //return GetWebHtml($"http://music.163.com/api/song/lyric?id={MusicId}&lv=1");

            string a = GetWebHtml($"https://api.injahow.cn/meting/?server=netease&type=song&id={MusicId}");
            string lrcUrl = JObject.Parse(a.Replace("[", "").Replace("]", ""))["lrc"].ToString();
            return GetWebHtml(lrcUrl);
        }

        public void GetAlbumData(JObject data)
        {
            try
            {
                Parent.AlbumPages = new Pages.AlbumPage(Parent, this);
                Parent.AlbumPages.Set(data["rid"].ToString());
                Parent.TheSearchPage.InPage.Content = Parent.AlbumPages;
                Parent.AlbumPages.InPage.Content = Parent.LoadingPage;
                Parent.SetPage("List");
                Parent.TheSearchPage.InPage.Margin = new Thickness(0, 75, 0, 0);
                Parent.TheSearchPage.SearchPageChangerGrid.Visibility = Visibility.Collapsed;
                BackgroundWorker bw = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };
                bw.DoWork += new DoWorkEventHandler(StartGetAlbumData);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetAlbumDataComplete);
                bw.RunWorkerAsync(data);
            }
            catch { Parent.ShowBox("错误", "读取不到专辑信息。"); }
        }

        public static BitmapImage GetBitmapImage(Uri uri, int decodePixelWidth = 0, int decodePixelHeight = 0)
        {
            try
            {
                BitmapImage defaultImage = new BitmapImage();
                defaultImage.BeginInit();

                defaultImage.UriSource = uri;
                defaultImage.DecodePixelWidth = decodePixelWidth;
                defaultImage.DecodePixelHeight = decodePixelHeight;

                defaultImage.EndInit();
                defaultImage.Freeze();

                return defaultImage;
            }
            catch
            {
                return new BitmapImage(uri);
            }
        }

        public static async Task<BitmapImage> GetImage(string uri, string type = "internet", int decodePixelWidth = 0, int decodePixelHeight = 0)
        {
            BitmapImage defaultImage = GetBitmapImage(new Uri(@"pack://application:,,,/Images/SugarAndSalt.jpg"), decodePixelWidth, decodePixelHeight);

            try
            {
                if (type == "local")
                {
                    BitmapImage bitmapImage = null;
                    await Task.Run(() =>
                    {
                        bitmapImage = GetMusicAlbumImageBitmapImage(uri, decodePixelWidth, decodePixelHeight);
                    });

                    if (bitmapImage == null)
                    {
                        return defaultImage;
                    }

                    return bitmapImage;
                }
                else if (type == "localAlbum")
                {
                    if (!File.Exists(uri))
                    {
                        return defaultImage;
                    }

                    return GetBitmapImage(new Uri(uri), decodePixelWidth, decodePixelHeight);
                }
                else if (type == "default" || uri == "")
                {
                    return defaultImage;
                }

                BitmapImage bitmapImage1 = await TheMusicDatas.GetNetImageAsync(uri);
                if (bitmapImage1 != null)
                {
                    return bitmapImage1;
                }
            }
            catch { }

            defaultImage.EndInit();
            defaultImage.Freeze();
            return defaultImage;
        }

        public static MemoryStream GetCover(string path)
        {
            try
            {
                TagLib.File f = TagLib.File.Create(path);
                if (f.Tag.Pictures != null && f.Tag.Pictures.Length != 0)
                {
                    var bin = (byte[])(f.Tag.Pictures[0].Data.Data);
                    return new MemoryStream(bin);
                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public static BitmapImage GetMusicAlbumImageBitmapImage(string MusicFile, int decodePixelWidth = 0, int decodePixelHeight = 0)
        {
            var ss = GetCover(MusicFile);

            try
            {
                if (ss != null)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ss;
                    bitmap.DecodePixelWidth = decodePixelWidth;
                    bitmap.DecodePixelHeight = decodePixelHeight;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    ss.Close();
                    ss.Dispose();
                    return bitmap;
                }
                else
                {
                    return null;
                }
            }
            catch { }
            return null;
        }

        public static async Task<BitmapImage> InitImage(string filePath)
        {
            BitmapImage bitmapImage = null;

            await Task.Run(() =>
            {
                try
                {
                    using (BinaryReader reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
                    {
                        FileInfo fi = new FileInfo(filePath);
                        byte[] bytes = reader.ReadBytes((int)fi.Length);
                        reader.Close();

                        //image = new Image();
                        bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = new MemoryStream(bytes);
                        bitmapImage.EndInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        //image.Source = bitmapImage;
                        reader.Close();
                        reader.Dispose();
                    }
                }
                catch
                {
                    bitmapImage = null;
                }
            });

            return bitmapImage;
        }

        public void SaveBitmapImage(BitmapImage BitmapImage, string FilePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(BitmapImage));

            using (var fileStream = new System.IO.FileStream(FilePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }

        }

        public string OpenWindowChoiceFlie(string TheFilter = "所有文件 (*.*)|*.*")
        {
            bool LrcWindowIsShow = Parent.LrcWindow.IsShow;
            Parent.LrcWindow.IsShow = false;

            var openFileDialog = new Microsoft.Win32.OpenFileDialog() { Filter = TheFilter };
            Parent.ShowBox("选择文件", "程序正在等待使用者选择文件。", MainWindow.ShowBoxStyle.Loading);
            var result = openFileDialog.ShowDialog();

            Parent.LrcWindow.IsShow = LrcWindowIsShow;

            if (result == true)
            {
                Parent.ShowBox("选择文件结果", $"已选择文件 \"{openFileDialog.FileName}\"");
                return openFileDialog.FileName;
            }
            else
            {
                Parent.ShowBox("选择文件", "操作已取消。", Animate: false);
                return null;
            }

        }

        public string OpenWindowChoiceFolder()
        {
            bool LrcWindowIsShow = Parent.LrcWindow.IsShow;
            Parent.LrcWindow.IsShow = false;

            Parent.ShowBox("选择文件夹", "程序正在等待使用者选择文件夹。", MainWindow.ShowBoxStyle.Loading);
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ThePath = folderBrowserDialog.SelectedPath + "\\";
                Parent.LrcWindow.IsShow = LrcWindowIsShow;
                Parent.ShowBox_ButtonClick(null, null);
                return ThePath;
            }
            else
            {
                Parent.ShowBox_ButtonClick(null, null);
                Parent.LrcWindow.IsShow = LrcWindowIsShow;
                return null;
            }
        }

        public string SavingWindow(string Path = "", string DName = "", string DExt = "", string Filter = "所有文件 (*.*)|*.*")
        {
            bool LrcWindowIsShow = Parent.LrcWindow.IsShow;
            Parent.LrcWindow.IsShow = false;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = DName;
            dlg.DefaultExt = DExt;
            dlg.Filter = Filter;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                Parent.LrcWindow.IsShow = LrcWindowIsShow;
                return dlg.FileName;
            }

            Parent.LrcWindow.IsShow = LrcWindowIsShow;
            Parent.ShowBox_ButtonClick(null, null);
            return null;
        }

        public string GetSoftwareAddress()
        {
            //获取应用程序的当前工作目录。

            string str = System.AppDomain.CurrentDomain.BaseDirectory;
            return str;
        }

        public string GetDownloadPath()
        {
            return Parent.SettingPages.UriDownloadAddress.Describe;
        }

        public string GetLoadPath()
        {
            return Parent.SettingPages.UriAddress.Describe;
        }


        private void StartGetAlbumData(object sender, DoWorkEventArgs args)
        {
            try
            {
                JObject items = (JObject)args.Argument;
                JObject data = JObject.Parse(GetWebHtml("http://m.kuwo.cn/newh5app/api/mobile/v1/music/album/" + items["albumid"] + "?rn=100"));

                args.Result = new List<JObject>() { items, data };
            }
            catch (Exception err) { ShowBox("错误", "无法连接到服务器!\n\n错误代码:\n" + err.ToString()); args.Result = null; }
        }

        private void GetAlbumDataComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null) return;

            List<JObject> datas = (List<JObject>)e.Result;

            try
            {
                Parent.AlbumPages.LastPage = Parent.ListPages;
                Pages.ListPage TheList = new Pages.ListPage(Parent, this);
                Parent.AddAlbumCards(datas[1], false, TheList);
                Parent.TheSearchPage.InPage.Content = Parent.AlbumPages;
                Parent.AlbumPages.InPage.Content = TheList;
            }
            catch (NullReferenceException)
            {
                GetAlbumData(datas[0]);
                Parent.ShowMessage("重试", "正在重新获取专辑信息。");
            }
        }

        private void NullCall() { }

        public void tFullScreen()
        {
            Parent.IsFullScreen = true;
        }

        public void tExitFullScreen()
        {
            Parent.IsFullScreen = false;
        }

        public void WindowTop(bool IsTop)
        {
            Parent.Topmost = IsTop;
        }

        public void OpenWeb(string WebAddress)
        {
            System.Diagnostics.Process.Start(WebAddress);
        }

        public static void OpenFileExplorer(string Path)
        {
            System.Diagnostics.Process.Start(Path);
        }

        public void MemoryClean()
        {
            GC.Collect();
        }

        public bool JoystickExists()
        {/*
            var dirInput = new SharpDX.DirectInput.DirectInput();
            var typeJoystick = SharpDX.DirectInput.DeviceType.Gamepad;
            var allDevices = dirInput.GetDevices();
            string LastText = "";
            string Text = "";
            foreach (var item in allDevices)
            {
                string Text1 = item.ProductName + " / " + item.Type.ToString() + "\n";
                if (LastText == Text1) continue;
                
                LastText = Text1;
                Text += Text1;

                if (typeJoystick == item.Type)
                {
                    SharpDX.DirectInput.Joystick curJoystick = new SharpDX.DirectInput.Joystick(dirInput, item.InstanceGuid);
                    curJoystick.Acquire();
                }
            }

            Parent.ShowBox("Devices", Text);
            */
            return true;
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connectionDescription, int reservedValue);
        public static bool InternetConnect()
        {
            var connection = 0;
            if (!InternetGetConnectedState(out connection, 0)) return false;
            return true;
        }

        public class ScreenManager
        {
            /// <summary>
            /// 获取DPI百分比
            /// </summary>
            /// <param name="window"></param>
            /// <returns></returns>
            public static double GetDpiRatio(Window window)
            {
                var currentGraphics = System.Drawing.Graphics.FromHwnd(new WindowInteropHelper(window).Handle);
                return currentGraphics.DpiX / 96;
            }

            public static double GetDpiRatio()
            {
                return GetDpiRatio(Application.Current.MainWindow);
            }

            public static double GetScreenHeight()
            {
                return SystemParameters.PrimaryScreenHeight * GetDpiRatio();
            }

            public static double GetScreenActualHeight()
            {
                return SystemParameters.PrimaryScreenHeight;
            }

            public static double GetScreenWidth()
            {
                return SystemParameters.PrimaryScreenWidth * GetDpiRatio();
            }

            public static double GetScreenActualWidth()
            {
                return SystemParameters.PrimaryScreenWidth;
            }
        }


        public enum MusicFrom { kwMusic, kgMusic, qqMusic, neteaseMusic, localMusic }

        public enum MusicKbps { aac, wma, Kbps128, Kbps192, Kbps320, Kbps1000 }

        public struct MusicData
        {
            public string Title { get; set; }
            public string SongRid { get; set; }
            public string Artist { get; set; }
            public string Album { get; set; }
            public string AlbumID { get; set; }
            public string PicUri { get; set; }
            public string Time { get; set; }
            public object OtherData { get; set; }
            public MusicFrom From { get; set; }
            public MusicKbps Kbps { get; set; }
            public string IsDownload { get; set; }
            public JObject NeteaseAlbumData { get; set; }
            public string ThekgMusicBackupHash { get; set; }
            public string ThekgMusicLrcs { get; set; }

            public MusicData(string Thetitle, string TheSongRid, string TheArtist, string TheAlbum = null, string TheAlbumID = null, string ThePicUri = null, string TheTime = null, object TheOtherData = null, MusicFrom TheFrom = MusicFrom.kwMusic, MusicKbps TheKbps = MusicKbps.Kbps192, string TheDownload = "No", JObject TheNeteaseAlbumData = null, string kgBackupHash = null, string KgMusicLrcs = null) : this()
            {
                this.Title = Thetitle;
                this.SongRid = TheSongRid;
                this.Artist = TheArtist;
                this.Album = TheAlbum;
                this.AlbumID = TheAlbumID;
                this.PicUri = ThePicUri;
                this.Time = TheTime;
                this.OtherData = TheOtherData;
                this.From = TheFrom;
                this.Kbps = TheKbps;
                this.IsDownload = TheDownload;
                this.NeteaseAlbumData = TheNeteaseAlbumData;
                this.ThekgMusicBackupHash = kgBackupHash;
                this.ThekgMusicLrcs = KgMusicLrcs;
            }
        }
    }
}
