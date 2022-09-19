using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace zilongcn
{
    public class Animations
    {
        #region 主方法
        public Storyboard storyboard { get; set; } = null;

        public bool IsAnimate { get; set; } = true;

        public Animations(bool IsAnimate = true)
        {
            storyboard = new Storyboard();
            this.IsAnimate = IsAnimate;
        }

        public void Begin() => storyboard.Begin();

        public void animateColor(DependencyObject Node, Color FromColor, Color ToColor, double AnimateTime = 1, double dr = 0.5, double ar = 0.3, EasingMode easingMode = EasingMode.EaseOut, string TheType = "Background")
        {
            if (IsAnimate == false)
            {
                FromColor = ToColor;
                AnimateTime = 0.01;
            }

            ColorAnimation anime = new ColorAnimation(FromColor, ToColor, new Duration(TimeSpan.FromSeconds(AnimateTime)));
            anime.EasingFunction = new SineEase { EasingMode = easingMode };
            anime.DecelerationRatio = dr;
            anime.AccelerationRatio = ar;
            Storyboard.SetTarget(anime, Node);

            if (TheType == "Foreground")
                Storyboard.SetTargetProperty(anime, new PropertyPath("(TextBlock.Foreground).(SolidColorBrush.Color)"));
            else
                Storyboard.SetTargetProperty(anime, new PropertyPath("(Border.Background).(SolidColorBrush.Color)"));

            storyboard.Children.Add(anime);
        }

        public void animateOpacity(DependencyObject Node, double FromValue, double ToValue, double AnimateTime = 1, double dr = 0.5, double ar = 0, EasingMode easingMode = EasingMode.EaseOut)
        {
            if (IsAnimate == false)
            {
                AnimateTime = 0.01;
                FromValue = ToValue;
            }

            DoubleAnimation anime = new DoubleAnimation(FromValue, ToValue, new Duration(TimeSpan.FromSeconds(AnimateTime)));
            anime.EasingFunction = new SineEase { EasingMode = easingMode };
            anime.DecelerationRatio = dr;
            anime.AccelerationRatio = ar;
            Storyboard.SetTarget(anime, Node);
            Storyboard.SetTargetProperty(anime, new PropertyPath("Opacity"));

            storyboard.Children.Add(anime);
        }

        public void animatePosition(DependencyObject Node, Thickness FromPosition, Thickness ToPosition, double AnimateTime = 1, double dr = 0.65
            , double ar = 0, EasingMode easingMode = EasingMode.EaseOut)
        {
            if (IsAnimate == false)
            {
                AnimateTime = 0.01;
                FromPosition = ToPosition;
            }

            ThicknessAnimation anime = new ThicknessAnimation(FromPosition, ToPosition, new Duration(TimeSpan.FromSeconds(AnimateTime)));
            anime.EasingFunction = new SineEase { EasingMode = easingMode };
            anime.DecelerationRatio = dr;
            anime.AccelerationRatio = ar;
            Storyboard.SetTarget(anime, Node);
            Storyboard.SetTargetProperty(anime, new PropertyPath("Margin"));

            storyboard.Children.Add(anime);
        }

        public void animateWidth(DependencyObject Node, double FromValue, double ToValue, double AnimateTime = 1, double dr = 0.65
            , double ar = 0, EasingMode easingMode = EasingMode.EaseOut)
        {
            if (IsAnimate == false)
            {
                FromValue = ToValue;
                AnimateTime = 0.01;
            }

            DoubleAnimation animeWidth = new DoubleAnimation();
            animeWidth.EasingFunction = new SineEase { EasingMode = easingMode };
            animeWidth.DecelerationRatio = dr;
            animeWidth.AccelerationRatio = ar;
            animeWidth.Duration = new Duration(TimeSpan.FromSeconds(AnimateTime));
            animeWidth.From = FromValue;
            animeWidth.To = ToValue;

            Storyboard.SetTarget(animeWidth, Node);
            Storyboard.SetTargetProperty(animeWidth, new PropertyPath("Width"));

            storyboard.Children.Add(animeWidth);
        }

        public void animateHeight(DependencyObject Node, double FromValue, double ToValue, double AnimateTime = 1, double dr = 0.65
            , double ar = 0, EasingMode easingMode = EasingMode.EaseOut)
        {
            if (IsAnimate == false)
            {
                FromValue = ToValue;
                AnimateTime = 0.01;
            }

            DoubleAnimation animeHeight = new DoubleAnimation();
            animeHeight.EasingFunction = new SineEase { EasingMode = easingMode };
            animeHeight.DecelerationRatio = dr;
            animeHeight.AccelerationRatio = ar;
            animeHeight.Duration = new Duration(TimeSpan.FromSeconds(AnimateTime));
            animeHeight.From = FromValue;
            animeHeight.To = ToValue;

            Storyboard.SetTarget(animeHeight, Node);
            Storyboard.SetTargetProperty(animeHeight, new PropertyPath("Height"));

            storyboard.Children.Add(animeHeight);
        }

        public void animateWidHei(DependencyObject Node, double FromValueWidth, double FromValueHeight, double ToValueWidth, double ToValueHeight, double AnimateTime = 1, double dr = 0.9, double ar = 0.1, EasingMode easingMode = EasingMode.EaseOut)
        {
            if (IsAnimate == false)
            {
                FromValueWidth = ToValueWidth;
                FromValueHeight = ToValueHeight;
                AnimateTime = 0.01;
            }

            animateWidth(Node, FromValueWidth, ToValueWidth, AnimateTime, dr, ar, easingMode);
            animateHeight(Node, FromValueHeight, FromValueWidth, AnimateTime, dr, ar, easingMode);
        }

        public void animateRenderTransform(FrameworkElement Node, double FromValue, double ToValue, double AnimateTime = 1, double dr = 0.65
            , double ar = 0, EasingMode easingMode = EasingMode.EaseOut, bool IsAnimate = true)
        {
            RotateTransform rtf = new RotateTransform();
            Node.RenderTransform = rtf;
            DoubleAnimation dbAscending = new DoubleAnimation(FromValue, ToValue, new Duration(TimeSpan.FromSeconds(1)));

            Storyboard.SetTarget(dbAscending, Node);
            Storyboard.SetTargetProperty(dbAscending, new PropertyPath("RenderTransform.Angle"));

            storyboard.Children.Add(dbAscending);
        }

        #endregion

        #region 静态方法
        public static void animateScale(ScaleTransform Node, double FromValue, double ToValue, double AnimateTime = 1, double dr = 0.5, double ar = 0, EasingMode easingMode = EasingMode.EaseOut, bool IsAnimate = true)
        {
            if (IsAnimate == false)
            {
                FromValue = ToValue;
                AnimateTime = 0.01;
            }

            DoubleAnimation anime = new DoubleAnimation(FromValue, ToValue, new Duration(TimeSpan.FromSeconds(AnimateTime)));
            anime.EasingFunction = new SineEase { EasingMode = easingMode };
            anime.DecelerationRatio = dr;
            anime.AccelerationRatio = ar;
            var Clock = anime.CreateClock();
            Node.ApplyAnimationClock(ScaleTransform.ScaleXProperty, Clock);
        }

        public static async void animateShake(DependencyObject Node, int ShakeCount, Thickness NodeMargin, double ShakeScale, TimeSpan AnimateTime, animateShakeMode animateShakeMode = animateShakeMode.Horizontal, bool IsAnimate = true)
        {
            int animateCount = 0;
            bool isLeftTop = false;
            Thickness lastThickness = NodeMargin;
            Thickness thickness = NodeMargin;

            while (true)
            {
                animateCount += 1;
                isLeftTop = !isLeftTop;

                double theLeft;
                double theTop;

                if (animateShakeMode == animateShakeMode.Horizontal)
                {
                    if (isLeftTop)
                    {
                        theLeft = NodeMargin.Left - ShakeScale;
                        theTop = NodeMargin.Top;
                    }
                    else
                    {
                        theLeft = NodeMargin.Left + ShakeScale;
                        theTop = NodeMargin.Top;
                    }
                }
                else
                {
                    if (isLeftTop)
                    {
                        theLeft = NodeMargin.Left;
                        theTop = NodeMargin.Top - ShakeScale;
                    }
                    else
                    {
                        theLeft = NodeMargin.Left;
                        theTop = NodeMargin.Top + ShakeScale;
                    }
                }

                thickness = new Thickness(theLeft, theTop, NodeMargin.Right, NodeMargin.Bottom);

                Animations animations = new Animations(IsAnimate);
                animations.animatePosition(Node, lastThickness, thickness, AnimateTime.TotalSeconds, 0, 0);
                animations.Begin();

                lastThickness = thickness;
                await Task.Delay(AnimateTime);
                if (animateCount >= ShakeCount)
                {
                    Animations animations1 = new Animations(IsAnimate);
                    animations1.animatePosition(Node, lastThickness, NodeMargin, AnimateTime.TotalSeconds, 0, 0);
                    animations1.Begin();
                    break;
                }
            }
        }

        public enum animateShakeMode { Horizontal, Vertical }

        public static Storyboard animateColor(DependencyObject Node, Color FromColor, Color ToColor, double AnimateTime = 1, double Ratio1 = 0.5, double Ratio2 = 0.3, EasingMode easingMode = EasingMode.EaseOut, string TheType = "Background", bool IsAnimate = true)
        {
            if (!IsAnimate)
            {
                FromColor = ToColor;
                AnimateTime = 0.01;
            }

            Storyboard storyboard = new Storyboard();

            ColorAnimation anime = new ColorAnimation(FromColor, ToColor, new Duration(TimeSpan.FromSeconds(AnimateTime)));
            anime.EasingFunction = new SineEase { EasingMode = easingMode };
            anime.DecelerationRatio = Ratio1;
            anime.AccelerationRatio = Ratio2;
            Storyboard.SetTarget(anime, Node);

            if (TheType == "Foreground")
                Storyboard.SetTargetProperty(anime, new PropertyPath("(TextBlock.Foreground).(SolidColorBrush.Color)"));
            else
                Storyboard.SetTargetProperty(anime, new PropertyPath("(Border.Background).(SolidColorBrush.Color)"));

            storyboard.Children.Add(anime);

            return storyboard;
        }

        public static Storyboard animateOpacity(DependencyObject Node, double FromValue, double ToValue, double AnimateTime = 1, double Ratio1 = 1, double Ratio2 = 0, EasingMode easingMode = EasingMode.EaseOut, bool IsAnimate = true)
        {
            if (!IsAnimate)
            {
                FromValue = ToValue;
                AnimateTime = 0.01;
            }

            Storyboard storyboard = new Storyboard();

            DoubleAnimation anime = new DoubleAnimation(FromValue, ToValue, new Duration(TimeSpan.FromSeconds(AnimateTime)));
            anime.EasingFunction = new SineEase { EasingMode = easingMode };
            anime.DecelerationRatio = Ratio1;
            anime.AccelerationRatio = Ratio2;
            Storyboard.SetTarget(anime, Node);
            Storyboard.SetTargetProperty(anime, new PropertyPath("Opacity"));

            storyboard.Children.Add(anime);

            return storyboard;
        }

        public static Storyboard animatePosition(DependencyObject Node, Thickness FromPosition, Thickness ToPosition, double AnimateTime = 1, double Ratio1 = 0.8
            , double Ratio2 = 0, EasingMode easingMode = EasingMode.EaseOut, bool IsAnimate = true)
        {
            if (!IsAnimate)
            {
                FromPosition = ToPosition;
                AnimateTime = 0.01;
            }

            Storyboard storyboard = new Storyboard();

            ThicknessAnimation anime = new ThicknessAnimation(FromPosition, ToPosition, new Duration(TimeSpan.FromSeconds(AnimateTime)));
            anime.EasingFunction = new SineEase { EasingMode = easingMode };
            anime.DecelerationRatio = Ratio1;
            anime.AccelerationRatio = Ratio2;
            Storyboard.SetTarget(anime, Node);
            Storyboard.SetTargetProperty(anime, new PropertyPath("Margin"));

            storyboard.Children.Add(anime);

            return storyboard;
        }

        public static Storyboard animateWidth(DependencyObject Node, double FromValue, double ToValue, double AnimateTime = 1, double dr = 0.65
            , double ar = 0, EasingMode easingMode = EasingMode.EaseOut, bool IsAnimate = true)
        {
            if (IsAnimate == false)
            {
                FromValue = ToValue;
                AnimateTime = 0.01;
            }

            Storyboard storyboard = new Storyboard();

            DoubleAnimation animeWidth = new DoubleAnimation();
            animeWidth.EasingFunction = new SineEase { EasingMode = easingMode };
            animeWidth.DecelerationRatio = dr;
            animeWidth.AccelerationRatio = ar;
            animeWidth.Duration = new Duration(TimeSpan.FromSeconds(AnimateTime));
            animeWidth.From = FromValue;
            animeWidth.To = ToValue;

            Storyboard.SetTarget(animeWidth, Node);
            Storyboard.SetTargetProperty(animeWidth, new PropertyPath("Width"));

            storyboard.Children.Add(animeWidth);

            return storyboard;
        }

        public static Storyboard animateHeight(DependencyObject Node, double FromValue, double ToValue, double AnimateTime = 1, double dr = 0.65
            , double ar = 0, EasingMode easingMode = EasingMode.EaseOut, bool IsAnimate = true)
        {
            if (IsAnimate == false)
            {
                FromValue = ToValue;
                AnimateTime = 0.01;
            }

            Storyboard storyboard = new Storyboard();

            DoubleAnimation animeHeight = new DoubleAnimation();
            animeHeight.EasingFunction = new SineEase { EasingMode = easingMode };
            animeHeight.DecelerationRatio = dr;
            animeHeight.AccelerationRatio = ar;
            animeHeight.Duration = new Duration(TimeSpan.FromSeconds(AnimateTime));
            animeHeight.From = FromValue;
            animeHeight.To = ToValue;

            Storyboard.SetTarget(animeHeight, Node);
            Storyboard.SetTargetProperty(animeHeight, new PropertyPath("Height"));

            storyboard.Children.Add(animeHeight);

            return storyboard;
        }

        public static Storyboard animateRenderTransformStatic(FrameworkElement Node, double FromValue, double ToValue, double AnimateTime = 1, double dr = 0.65
            , double ar = 0, EasingMode easingMode = EasingMode.EaseOut, bool IsAnimate = true)
        {
            RotateTransform rtf = new RotateTransform();
            Node.RenderTransform = rtf;
            DoubleAnimation dbAscending = new DoubleAnimation(FromValue, ToValue, new Duration(TimeSpan.FromSeconds(AnimateTime)));
            Storyboard storyboard = new Storyboard();

            storyboard.Children.Add(dbAscending);
            Storyboard.SetTarget(dbAscending, Node);
            Storyboard.SetTargetProperty(dbAscending, new PropertyPath("RenderTransform.Angle"));

            return storyboard;
        }

        public static Storyboard animateWidHei(DependencyObject Node, double FromValueWidth, double FromValueHeight, double ToValueWidth, double ToValueHeight, double AnimateTime = 1, double dr = 0.9, double ar = 0.1, EasingMode easingMode = EasingMode.EaseOut, bool IsAnimate = true)
        {
            Animations animations = new Animations(IsAnimate);

            animations.animateWidth(Node, FromValueWidth, ToValueWidth, AnimateTime, dr, ar, easingMode);
            animations.animateHeight(Node, FromValueHeight, FromValueWidth, AnimateTime, dr, ar, easingMode);

            return animations.storyboard;
        }

        public static Int32Animation animateInt(int FromValue, int ToValue, double AnimateTime = 1, double dr = 0.9, double ar = 0.1, EasingMode easingMode = EasingMode.EaseOut, bool IsAnimate = true)
        {
            Int32Animation int32Animation = new Int32Animation();
            int32Animation.From = FromValue;
            int32Animation.To = ToValue;
            int32Animation.DecelerationRatio = dr;
            int32Animation.AccelerationRatio = ar;
            int32Animation.EasingFunction = new SineEase { EasingMode = easingMode };
            int32Animation.Duration = new Duration(TimeSpan.FromSeconds(AnimateTime));

            return int32Animation;
        }
        #endregion
    }

    public class Others
    {
        #region 在 Alt+Tab 任务视图中隐藏
        public static void HideAltTab(Window window)
        {
            var windowInterop = new System.Windows.Interop.WindowInteropHelper(window);
            var exStyle = GetWindowLong(windowInterop.Handle, -20);

            SetWindowLong(windowInterop.Handle, -20, new IntPtr(0x80));
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }
        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);


        #endregion

        #region 性能检测
        public enum TestResult { 高, 中, 低 }
        public static TestResult CheckIsFast()
        {
            int displayTier = RenderCapability.Tier >> 16;

            if (displayTier == 0)
            {
                return TestResult.低;
            }
            else if (displayTier == 1)
            {
                return TestResult.中;
            }
            else
            {
                return TestResult.高;
            }
        }
        #endregion

        #region MD5
        public static string ToMD5(string strs)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] bytes = Encoding.Default.GetBytes(strs);//将要加密的字符串转换为字节数组
            byte[] encryptdata = md5.ComputeHash(bytes);//将字符串加密后也转换为字符数组
            return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为加密字符串
        }
        #endregion

        #region 取字符中间
        public static string StringBetween(string str, string leftstr, string rightstr)
        {
            Regex rg = new Regex("(?<=(" + leftstr + "))[.\\s\\S]*?(?=(" + rightstr + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(str).Value;
        }
        #endregion

        #region 歌词解析
        public class GetReadLrc
        {
            private string _LrcText = null;
            public string LrcText { get { return _LrcText; } }
            private string _LrcTime = null;
            public string LrcTime { get { return _LrcTime; } }

            public GetReadLrc(string LrcText)
            {
                _LrcTime = StringBetween(LrcText, "\\[", "\\]");
                _LrcText = LrcText.Replace("[" + _LrcTime + "]", "");
            }
        }
        #endregion

        #region 判断文件编码格式

        #region 主方法重载
        public static System.Text.Encoding GetType(string FILE_NAME)
        {
            FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read);
            Encoding r = GetType(fs);
            fs.Close();
            return r;
        }
        #endregion

        #region 判断主方法
        public static Encoding GetType(FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            Encoding reVal = Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i;
            int.TryParse(fs.Length.ToString(), out i);
            byte[] ss = r.ReadBytes(i);
            if (IsUTF8Bytes(ss) || (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                reVal = Encoding.UTF8;
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = Encoding.Unicode;
            }
            r.Close();
            return reVal;
        }
        #endregion

        #region IsUtf8Bom
        private static bool IsUTF8Bytes(byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数 
            byte curByte; //当前分析的字节. 
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前 
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                throw new Exception("非预期的byte格式");
            }
            return true;
        }
        #endregion

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

        #region 获取图片主色调
        public static async Task<System.Drawing.Color> GetMajorColor(System.Drawing.Bitmap bitmap)
        {
            //色调的总和
            var sum_hue = 0d;
            //色差的阈值
            var threshold = 30;

            //计算色调总和
            await Task.Run(() =>
            {
                for (int h = 0; h < bitmap.Height; h++)
                {
                    for (int w = 0; w < bitmap.Width; w++)
                    {
                        var hue = bitmap.GetPixel(w, h).GetHue();
                        sum_hue += hue;
                    }
                }
            });

            var avg_hue = sum_hue / (bitmap.Width * bitmap.Height);

            //色差大于阈值的颜色值
            var rgbs = new List<System.Drawing.Color>();

            await Task.Run(() =>
            {
                for (int h = 0; h < bitmap.Height; h++)
                {
                    for (int w = 0; w < bitmap.Width; w++)
                    {
                        var color = bitmap.GetPixel(w, h);
                        var hue = color.GetHue();
                        //如果色差大于阈值，则加入列表
                        if (Math.Abs(hue - avg_hue) > threshold)
                        {
                            rgbs.Add(color);
                        }
                    }
                }
            });

            if (rgbs.Count == 0)
                return System.Drawing.Color.Black;

            //计算列表中的颜色均值，结果即为该图片的主色调
            int sum_r = 0, sum_g = 0, sum_b = 0;

            foreach (var rgb in rgbs)
            {
                sum_r += rgb.R;
                sum_g += rgb.G;
                sum_b += rgb.B;
            }

            return System.Drawing.Color.FromArgb(255,
                sum_r / rgbs.Count,
                sum_g / rgbs.Count,
                sum_b / rgbs.Count);
        }
        #endregion

        #region bitmapimage转bitmap
        public static System.Drawing.Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (System.IO.MemoryStream outStream = new System.IO.MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return bitmap;
            }
        }
        #endregion

        #region win11圆角窗口
        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,    // 默认样式
            DWMWCP_DONOTROUND = 1, // 无圆角
            DWMWCP_ROUND = 2,      // 有圆角
            DWMWCP_ROUNDSMALL = 3  // 有小圆角
        }

        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern long DwmSetWindowAttribute(IntPtr hwnd,
                                                         DWMWINDOWATTRIBUTE attribute,
                                                         ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                         uint cbAttribute);

        // 封装上面的方法使其更易于使用
        public static long SetWindowRound(Window window,
                                           DWMWINDOWATTRIBUTE attribute,
                                           ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                           uint cbAttribute)
        {
            var windowInterop = new System.Windows.Interop.WindowInteropHelper(window);
            return DwmSetWindowAttribute(windowInterop.Handle, attribute, ref pvAttribute, cbAttribute);
        }

        #endregion

        #region 提取资源

        /// <summary>
        /// 从资源文件中抽取资源文件
        /// </summary>
        /// <param name="resFileName">资源文件名称（资源文件名称必须包含目录，目录间用“.”隔开,最外层是项目默认命名空间）</param>
        /// <param name="outputFile">输出文件</param>
        public static void ExtractResFile(string resFileName, string outputFile)
        {
            BufferedStream inStream = null;
            FileStream outStream = null;
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly(); //读取嵌入式资源
                inStream = new BufferedStream(asm.GetManifestResourceStream(resFileName));
                outStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[1024];
                int length;

                while ((length = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, length);
                }
                outStream.Flush();
            }
            finally
            {
                if (outStream != null)
                {
                    outStream.Close();
                }
                if (inStream != null)
                {
                    inStream.Close();
                }
            }
        }
        #endregion

        #region 提取Windows中安装的某程序
        public struct SoftwaveData
        {
            public string name { get; set; }
            public string publisher { get; set; }
            public string version { get; set; }
            public bool isHas { get; set; }

            public SoftwaveData(string name, string publisher, string version, bool IsHas = false) : this()
            {
                this.name = name;
                this.publisher = publisher;
                this.version = version;
                this.isHas = isHas;
            }
        }

        public static SoftwaveData CheckSoftwaveIsInstalled(string swName)
        {
            SoftwaveData softwaveData = new SoftwaveData();
            bool isTrue = false;

            for (int index = 0; ; index++)
            {
                StringBuilder productCode = new StringBuilder(39);
                if (MsiEnumProducts(index, productCode) != 0)
                {
                    break;
                }

                int counts = 0;

                foreach (string property in new string[] { "ProductName", "Publisher", "VersionString", })
                {
                    int charCount = 512;
                    StringBuilder value = new StringBuilder(charCount);
                    if (MsiGetProductInfo(productCode.ToString(), property, value, ref charCount) == 0)
                    {
                        value.Length = charCount;
                        if (counts == 0)
                        {
                            if (value.ToString() == swName)
                            {
                                isTrue = true;
                                softwaveData.name = value.ToString();
                                softwaveData.isHas = true;
                            }
                        }
                        else if (counts == 1 && isTrue) softwaveData.publisher = value.ToString();
                        else if (counts == 2 && isTrue) softwaveData.version = value.ToString();

                        if (isTrue) counts++;
                    }
                }

                if (isTrue) break;
            }

            return softwaveData;
        }

        [DllImport("msi.dll", SetLastError = true)]
        static extern int MsiEnumProducts(int iProductIndex, StringBuilder lpProductBuf);
        [DllImport("msi.dll", SetLastError = true)]
        static extern int MsiGetProductInfo(string szProduct, string szProperty, StringBuilder lpValueBuf, ref int pcchValueBuf);
        #endregion

        #region 安装程序
        public void AddRegedit(string setupPath, string name)
        {
        }
        #endregion

        #region 图片处理
        public static System.Drawing.Bitmap KiResizeImage(System.Drawing.Bitmap bmp, int newW, int newH)
        {
            try
            {
                System.Drawing.Bitmap b = new System.Drawing.Bitmap(newW, newH);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b);
                // 插值算法的质量
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(bmp, new System.Drawing.Rectangle(0, 0, newW, newH), new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.GraphicsUnit.Pixel);
                g.Dispose();
                return b;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region 获取ftp文件
        public static int FtpFileDownload(string savePath, string web, string ftpUserID, string ftpPassword)
        {
            string ftpServerPath = web; //":23"指定的端口，不填默认端口为21
            string ftpUser = ftpUserID;
            string ftpPwd = ftpPassword;
            string saveFilePath = savePath;

            FileStream outputStream = null;
            FtpWebResponse response = null;
            FtpWebRequest reqFTP;

            try
            {
                outputStream = new FileStream(saveFilePath, FileMode.Create);

                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(ftpServerPath));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.KeepAlive = false;
                reqFTP.Timeout = 3000;
                reqFTP.Credentials = new NetworkCredential(ftpUser, ftpPwd);
                response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                int bufferSize = 2048;
                int readCount;
                int ftpFileReadSize = 0;
                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    ftpFileReadSize += readCount;
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }

                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FtpClient异常：" + ex.ToString());
                if (outputStream != null)
                {
                    outputStream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }

                return -1;
            }

            return 0;
        }
        #endregion

        #region 亮/暗主题获取
        public static bool AppsUseLightTheme()
        {
            try
            {
                using (RegistryKey personalize = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", false))
                {
                    if (personalize.GetValueNames().Contains("AppsUseLightTheme"))
                    {
                        return (int)personalize.GetValue("AppsUseLightTheme") == 1;
                    }
                }
            }
            catch
            {
                return true;
            }

            return true;
        }
        #endregion

        #region 获取主题色
        public static Color GetSystemAccentColor()
        {
            using (RegistryKey dwm = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM", false))
            {
                if (dwm.GetValueNames().Contains("AccentColor"))
                {
                    // 这里不要尝试转换成uint，因为有可能符号位为 1（负数）,会导致强制转换错误
                    // 直接进行下面的位操作即可
                    int accentColor = (int)dwm.GetValue("AccentColor");
                    // 注意：读取到的颜色为 AABBGGRR
                    return Color.FromArgb(
                        (byte)((accentColor >> 24) & 0xFF),
                        (byte)(accentColor & 0xFF),
                        (byte)((accentColor >> 8) & 0xFF),
                        (byte)((accentColor >> 16) & 0xFF));
                }
            }

            return SystemParameters.WindowGlassColor;  // 近似的系统主题色
        }
        #endregion

        #region 获取是否开启亚克力
        public static bool SystemEnableBlurBehind()
        {
            using (RegistryKey dwm = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", false))
            {
                if (dwm.GetValueNames().Contains("EnableTransparency"))
                {
                    int result = (int)dwm.GetValue("EnableTransparency");
                    if (result == 1) return true;
                    else return false;
                }
                else throw new InvalidDataException();
            }
        }
        #endregion

        #region 获取是否开启系统动画
        public static bool SystemEnableAnimation()
        {
            using (RegistryKey dwm = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects\ControlAnimations", false))
            {
                if (dwm.GetValueNames().Contains("CheckedValue"))
                {
                    int result = (int)dwm.GetValue("CheckedValue");
                    if (result == 1) return true;
                    else return false;
                }
                else throw new InvalidDataException();
            }
        }
        #endregion

        #region win11云母
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);

        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_MICA_EFFECT = 1029
        }

        public static void EnableMica(Window window, bool darkThemeEnabled)
        {
            IntPtr handle = new System.Windows.Interop.WindowInteropHelper(window).EnsureHandle(); ;

            int trueValue = 0x01;
            int falseValue = 0x00;

            // 深色主题
            if (darkThemeEnabled)
                DwmSetWindowAttribute(handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
            else
                DwmSetWindowAttribute(handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref falseValue, Marshal.SizeOf(typeof(int)));

            DwmSetWindowAttribute(handle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(int)));
        }
        #endregion
    }
}
