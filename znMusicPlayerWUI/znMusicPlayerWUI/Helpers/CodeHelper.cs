using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using Windows.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WinRT.Interop;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using System.IO;
using Microsoft.UI.Xaml.Media;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using static System.Net.Mime.MediaTypeNames;
using Windows.UI.ViewManagement;

namespace znMusicPlayerWUI.Helpers
{
    /// <summary>
    /// Esay to creat a simple animation
    /// </summary>
    public static class AnimateHelper
    {
        public static Storyboard AnimateOpacity(UIElement element, double from, double to, double timeSecond, EasingFunctionBase easingFunction = null)
        {
            var storyboard = new Storyboard();
            DoubleAnimation doubleAnimation = new DoubleAnimation();

            doubleAnimation.From = from;
            doubleAnimation.To = to;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(timeSecond));
            doubleAnimation.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation, element);
            Storyboard.SetTargetProperty(doubleAnimation, "Opacity");

            storyboard.Children.Add(doubleAnimation);
            return storyboard;
        }

        public static void AnimateScalar(UIElement element, float scalar, double TimeSecond,
                                          float cubicBezierEasing1, float cubicBezierEasing2, float cubicBezierEasing3, float cubicBezierEasing4,
                                          out Visual elementVisual, out Compositor compositor, out ScalarKeyFrameAnimation animation)
        {
            elementVisual = ElementCompositionPreview.GetElementVisual(element);
            compositor = elementVisual.Compositor;

            animation = compositor.CreateScalarKeyFrameAnimation();
            var easing = compositor.CreateCubicBezierEasingFunction(new Vector2(cubicBezierEasing1, cubicBezierEasing2), new Vector2(cubicBezierEasing3, cubicBezierEasing4));

            animation.Duration = TimeSpan.FromSeconds(TimeSecond);
            animation.InsertKeyFrame((float)TimeSecond, scalar, easing);
        }

        public static void AnimateOffset(UIElement element, float offsetX, float offsetY, float offsetZ, double TimeSecond,
                                         float cubicBezierEasing1, float cubicBezierEasing2, float cubicBezierEasing3, float cubicBezierEasing4,
                                         out Visual elementVisual, out Compositor compositor, out Vector3KeyFrameAnimation animation)
        {
            elementVisual = ElementCompositionPreview.GetElementVisual(element);
            compositor = elementVisual.Compositor;

            var easing = compositor.CreateCubicBezierEasingFunction(new Vector2(cubicBezierEasing1, cubicBezierEasing2), new Vector2(cubicBezierEasing3, cubicBezierEasing4));
            animation = compositor.CreateVector3KeyFrameAnimation();

            animation.Duration = TimeSpan.FromSeconds(TimeSecond);
            animation.InsertKeyFrame((float)TimeSecond, new Vector3(offsetX, offsetY, offsetZ), easing);
        }

        public static Storyboard AnimateTransform(
            UIElement element, UIElement parent,
            double fromValueX = double.NaN, double fromValueY = double.NaN,
            double toValueX = double.NaN, double toValueY = double.NaN,
            double timeSecondX = 1.0, double timeSecondY = 1.0)
        {
            if (element.RenderTransform == null)
            {
                element.RenderTransform = new Microsoft.UI.Xaml.Media.TranslateTransform();
            }

            if (fromValueX == double.NaN || fromValueY == double.NaN)
            {
                var position = element.TransformToVisual(parent).TransformPoint(new Point());

                if (fromValueX == double.NaN)
                    fromValueX = position.X;

                if (fromValueY == double.NaN)
                    fromValueY = position.Y;
            }

            var storyboard = new Storyboard();

            // x
            var xAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(timeSecondX)),
                From = fromValueX,
                To = toValueX,
                EnableDependentAnimation = true
            };
            Storyboard.SetTarget(xAnimation, element);
            Storyboard.SetTargetProperty(xAnimation, "(UIElement.RenderTransform).(TranslateTransform.X)");

            // y
            var yAnimation = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromSeconds(timeSecondY)),
                From = fromValueY,
                To = toValueY,
                EnableDependentAnimation = true
            };
            Storyboard.SetTarget(yAnimation, element);
            Storyboard.SetTargetProperty(yAnimation, "(UIElement.RenderTransform).(TranslateTransform.Y)");

            storyboard.Children.Add(xAnimation);
            storyboard.Children.Add(yAnimation);

            return storyboard;
        }
    }

    public static class CodeHelper
    {
        #region 取字符中间
        public static string StringBetween(string str, string leftstr, string rightstr)
        {
            Regex rg = new Regex("(?<=(" + leftstr + "))[.\\s\\S]*?(?=(" + rightstr + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(str).Value;
        }
        #endregion

        #region 设置目标窗体大小，位置
        /// <summary>
        /// 设置目标窗体大小，位置
        /// </summary>
        /// <param name="hWnd">目标句柄</param>
        /// <param name="x">目标窗体新位置X轴坐标</param>
        /// <param name="y">目标窗体新位置Y轴坐标</param>
        /// <param name="nWidth">目标窗体新宽度</param>
        /// <param name="nHeight">目标窗体新高度</param>
        /// <param name="BRePaint">是否刷新窗体</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);

        [DllImport("Shcore.dll", SetLastError = true)]
        internal static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

        internal enum Monitor_DPI_Type : int
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }

        public static DisplayArea GetDisplayArea(Window window)
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(window);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return DisplayArea.GetFromWindowId(wndId, DisplayAreaFallback.Primary);
        }

        public static double GetScaleAdjustment(Window window)
        {
            DisplayArea displayArea = GetDisplayArea(window);
            IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

            // Get DPI.
            int result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _);
            if (result != 0)
            {
                throw new Exception("Could not get DPI for monitor.");
            }

            uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
            return scaleFactorPercent / 100.0;
        }
        #endregion

        #region 获取文件夹大小
        public static async Task<double> GetDirctoryLength(string dir)
        {
            if (Directory.Exists(dir))
            {
                double totalFileSize = 0;

                DirectoryInfo directoryInfo = new DirectoryInfo(dir);
                FileInfo[] fileInfos = directoryInfo.GetFiles();

                await Task.Run(() =>
                {
                    foreach (FileInfo fileInfo in fileInfos)
                    {
                        totalFileSize += fileInfo.Length;
                    }
                });

                return totalFileSize;
            }
            else
            {
                //throw new DirectoryNotFoundException($"找不到路径 \"{dir}\"。");
                return 0;
            }
        }
        #endregion

        #region 计算文件大小
        private const double KBCount = 1024;
        private const double MBCount = KBCount * 1024;
        private const double GBCount = MBCount * 1024;
        private const double TBCount = GBCount * 1024;

        /// <summary>
        /// 得到适应的大小
        /// </summary>
        /// <param name="path"></param>
        /// <returns>string</returns>
        public static string GetAutoSizeString(double size, int roundCount)
        {
            if (KBCount > size)
            {
                return Math.Round(size, roundCount) + "B";
            }
            else if (MBCount > size)
            {
                return Math.Round(size / KBCount, roundCount) + "KB";
            }
            else if (GBCount > size)
            {
                return Math.Round(size / MBCount, roundCount) + "MB";
            }
            else if (TBCount > size)
            {
                return Math.Round(size / GBCount, roundCount) + "GB";
            }
            else
            {
                return Math.Round(size / TBCount, roundCount) + "TB";
            }
        }
        #endregion

        #region 检查是否最小化
        [DllImport("user32")]
        public static extern bool IsIconic(IntPtr hwnd);
        #endregion

        #region 合法文件名
        public static string ReplaceBadCharOfFileName(string fileName)
        {
            string str = fileName;
            str = str.Replace("\\", string.Empty);
            str = str.Replace("/", string.Empty);
            str = str.Replace(":", string.Empty);
            str = str.Replace("*", string.Empty);
            str = str.Replace("?", string.Empty);
            str = str.Replace("\"", string.Empty);
            str = str.Replace("<", string.Empty);
            str = str.Replace(">", string.Empty);
            str = str.Replace("|", string.Empty);
            return str;
        }
        #endregion

        public static async Task<ImageSource> GetCover(string path)
        {
            try
            {
                TagLib.File f = TagLib.File.Create(path);
                if (f.Tag.Pictures != null && f.Tag.Pictures.Length != 0)
                {
                    var bin = f.Tag.Pictures[0].Data.Data;
                    f.Dispose();
                    return await SaveToImageSource(bin);
                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<ImageSource> SaveToImageSource(this byte[] imageBuffer)
        {
            ImageSource imageSource = null;
            try
            {
                using (MemoryStream stream = new MemoryStream(imageBuffer))
                {
                    var ras = stream.AsRandomAccessStream();
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(BitmapDecoder.JpegDecoderId, ras);
                    var provider = await decoder.GetPixelDataAsync();
                    byte[] buffer = provider.DetachPixelData();
                    WriteableBitmap bitmap = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                    await bitmap.PixelBuffer.AsStream().WriteAsync(buffer, 0, buffer.Length);
                    imageSource = bitmap;
                }
            }
            catch { }
            return imageSource;
        }

        public static string ToMD5(string strs)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes = Encoding.Default.GetBytes(strs);//将要加密的字符串转换为字节数组
            byte[] encryptdata = md5.ComputeHash(bytes);//将字符串加密后也转换为字符数组
            return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为加密字符串
        }

        public static bool IsAccentColorDark()
        {
            var uiSettings = new UISettings();
            var c = uiSettings.GetColorValue(UIColorType.Accent);
            bool isDark = (5 * c.G + 2 * c.R + c.B) <= 8 * 128;
            return isDark;
        }
    }

    public static class LyricHelper
    {
        public static string NoneLyricString = "·········";
        public async static Task<DataEditor.LyricData[]> LyricToLrcData(string lyricText)
        {
            Dictionary<TimeSpan, DataEditor.LyricData> lyricDictionary = new();
            await Task.Run(() =>
            {
                foreach (var lyric in lyricText.Split('\n'))
                {
                    var timesAndLyric = lyric.Split(']');
                    //当一句歌词在不同时间段时
                    if (timesAndLyric.Count() > 2)
                    {
                        for (int i = timesAndLyric.Count(); i > 0; i--)
                        {
                            var times = timesAndLyric[i - 1].Replace("[", "").Split('.');
                            var timesa = TimeSpan.TryParse("00:" + times[0], null, out TimeSpan timesb);

                            if (times.Length == 1) continue;
                            if (!timesa) continue;

                            var timeMillsStr = times[1];
                            var parse = int.TryParse(timeMillsStr, out int iparse);
                            if (!parse) continue;

                            switch (timeMillsStr.Length)
                            {
                                case 1: timeMillsStr += "00"; break;
                                case 2: timeMillsStr += "0"; break;
                                case 3: break;
                                default: System.Diagnostics.Debug.WriteLine("Warning：歌词精度可能会降低。"); break;
                            }
                            var timeMills = TimeSpan.FromMilliseconds(int.Parse(timeMillsStr));
                            var timesResult = timesb + timeMills;

                            if (!lyricDictionary.ContainsKey(timesResult))
                            {
                                lyricDictionary.Add(timesResult, new(new() { timesAndLyric.Last() }, null, timesResult));
                            }
                        }
                    }
                    //当一句歌词在只在同一时间段时
                    else
                    {
                        var times = timesAndLyric[0].Replace("[", "").Split('.');
                        var timesa = TimeSpan.TryParse("00:" + times[0], null, out TimeSpan timesb);

                        if (times.Length == 1) continue;
                        if (!timesa) continue;

                        var timeMillsStr = times[1];
                        var parse = int.TryParse(timeMillsStr, out int iparse);
                        if (!parse) continue;

                        switch (timeMillsStr.Length)
                        {
                            case 1: timeMillsStr += "00"; break;
                            case 2: timeMillsStr += "0"; break;
                            case 3: break;
                            default: System.Diagnostics.Debug.WriteLine("Warning：歌词精度可能会降低。"); break;
                        }
                        var timeMills = TimeSpan.FromMilliseconds(int.Parse(timeMillsStr));
                        var timesResult = timesb + timeMills;

                        var text = timesAndLyric[1];
                        if (text == "") text = NoneLyricString;

                        //当有相同时间的歌词时
                        if (lyricDictionary.ContainsKey(timesResult))
                        {
                            var l = lyricDictionary[timesResult];
                            if (text != NoneLyricString)
                            {
                                l.Lyric.Add(text);
                            }
                        }
                        //当没有相同时间的歌词时
                        else
                        {
                            lyricDictionary.Add(timesResult, new(new() { text }, null, timesResult));
                        }
                    }
                }

                if (lyricDictionary.Any())
                {
                    //lastLyric
                    lyricDictionary.Add(lyricDictionary.Last().Key + TimeSpan.FromSeconds(0.1),
                        new(null, null, lyricDictionary.Last().Key + TimeSpan.FromSeconds(0.1)));
                }
            });

            var sorter = from pair in lyricDictionary orderby pair.Key ascending select pair;
            List<DataEditor.LyricData> lyricList = new();
            foreach (var l in sorter) lyricList.Add(l.Value);
            return lyricList.ToArray();
        }
    }
}
