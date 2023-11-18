using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using System.Diagnostics;
using PInvoke;

namespace znMusicPlayerWUI.Windowed
{
    public partial class TaskBarInfoWindow : Window
    {
        public nint Handle { get; private set; }
        public string IconPath { get; private set; }
        public string IconPathUsing { get; private set; }

        public TaskBarInfoWindow()
        {
            InitializeComponent();
            InitTaskbarInfo();
            InitCallBack();
            App.audioPlayer.PlayStateChanged += (_) =>
            {
                SetTaskbarButtonIcon(_.PlaybackState);
            };
            App.audioPlayer.SourceChanged += (_) =>
            {
                if (_.MusicData == null)
                    Title = App.AppName;
                else
                {
                    Title = $"{_.MusicData.Title} - {_.MusicData.ArtistName} · {App.AppName}";
                }
            };
            App.playingList.NowPlayingImageLoaded += (_, __) =>
            {
                IconPath = __;
            };
            Activated += (_, __) =>
            {
                App.WindowLocal.Activate();
            };
            AppWindow.Closing += (_, __) =>
            {
                __.Cancel = true;
                App.AppWindowLocalOverlappedPresenter.Restore();
                App.WindowLocal.Activate();
            };
        }

        static string localPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "Images");
        nint pauseIconHandle = (Bitmap.FromFile(Path.Combine(localPath, "任务栏暂停.png")) as Bitmap).GetHicon();
        nint playIconHandle = (Bitmap.FromFile(Path.Combine(localPath, "任务栏播放.png")) as Bitmap).GetHicon();
        System.Drawing.Image bmp = null;
        public async void SetTaskbarImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            if (IconPathUsing == filePath) return;
            bool canBreak = false;
            bmp = await Task.Run(() => Bitmap.FromFile(filePath));
            int size = 160;
            for (int i = 0; i < 50; i++)
            {
                if (canBreak) break;
                var hBitmap = bmp.GetThumbnailImage(size, size, null, 0) as Bitmap;
                var hBitmapNint = hBitmap.GetHbitmap();
                try
                {
                    var a = await Task.Run(() => NativeMethods.DwmSetIconicThumbnail(Handle, hBitmapNint, NativeMethods.DWM_SIT.None));
                    if (a != 0)
                    {
                        //Debug.WriteLine($"{size}x{size} failed.");
                        size -= 2;
                        canBreak = false;
                    }
                    else
                    {
                        Debug.WriteLine($"[TaskBarInfoWindow]: TaskBar Image {size}x{size} completed.");
                        IconPathUsing = filePath;
                        canBreak = true;
                    }
                }
                catch
                {
                }
            }
        }

        public async void InitTaskbarInfo()
        {
            Title = App.AppName;
            Handle = WindowHelperzn.WindowHelper.GetWindowHandle(this);
            AppWindow.Hide();

            int attributeTrue = (int)NativeMethods.TRUE;
            var hresult = NativeMethods.DwmSetWindowAttribute(Handle, NativeMethods.DWMWA.HAS_ICONIC_BITMAP, ref attributeTrue, sizeof(int));
            if ((hresult != 0))
                throw Marshal.GetExceptionForHR(hresult);
            hresult = NativeMethods.DwmSetWindowAttribute(Handle, NativeMethods.DWMWA.FORCE_ICONIC_REPRESENTATION, ref attributeTrue, sizeof(int));
            if ((hresult != 0))
                throw Marshal.GetExceptionForHR(hresult);

            Helpers.SDKs.TaskbarProgress.MyTaskbarInstance.HrInit();
            await Task.Delay(100);

            Helpers.SDKs.TaskbarProgress.MyTaskbarInstance.RegisterTab(Handle, App.AppWindowLocalHandle);
            Helpers.SDKs.TaskbarProgress.MyTaskbarInstance.SetTabOrder(Handle, App.AppWindowLocalHandle);
            Helpers.SDKs.TaskbarProgress.THUMBBUTTON[] taskbarInfoButtonPauseStyle = new Helpers.SDKs.TaskbarProgress.THUMBBUTTON[]
            {
                new Helpers.SDKs.TaskbarProgress.THUMBBUTTON(){ iId = 1, dwMask = Helpers.SDKs.TaskbarProgress.THUMBBUTTONMASK.THB_ICON, dwFlags = Helpers.SDKs.TaskbarProgress.THUMBBUTTONFLAGS.THBF_ENABLED, hIcon = (Bitmap.FromFile(Path.Combine(localPath, "上一首.png")) as Bitmap).GetHicon(), szTip = "上一首" },
                new Helpers.SDKs.TaskbarProgress.THUMBBUTTON(){ iId = 2, dwMask = Helpers.SDKs.TaskbarProgress.THUMBBUTTONMASK.THB_ICON, dwFlags = Helpers.SDKs.TaskbarProgress.THUMBBUTTONFLAGS.THBF_ENABLED, hIcon = playIconHandle, szTip = "播放" },
                new Helpers.SDKs.TaskbarProgress.THUMBBUTTON(){ iId = 3, dwMask = Helpers.SDKs.TaskbarProgress.THUMBBUTTONMASK.THB_ICON, dwFlags = Helpers.SDKs.TaskbarProgress.THUMBBUTTONFLAGS.THBF_ENABLED, hIcon = (Bitmap.FromFile(Path.Combine(localPath, "下一首.png")) as Bitmap).GetHicon(), szTip = "下一首" },
            };
            Helpers.SDKs.TaskbarProgress.MyTaskbarInstance.ThumbBarAddButtons(Handle, 3, taskbarInfoButtonPauseStyle);
            Helpers.SDKs.TaskbarProgress.MyTaskbarInstance.ThumbBarUpdateButtons(Handle, 3, taskbarInfoButtonPauseStyle);
            AppWindow.SetIcon("icon.ico");
            SetTaskbarImage(Path.Combine(localPath, "SugarAndSalt.jpg"));
        }

        private void InitCallBack()
        {
            hotKeyPrc = HotKeyPrc;
            var hotKeyPrcPointer = Marshal.GetFunctionPointerForDelegate(hotKeyPrc);
            origPrc =
                Marshal.GetDelegateForFunctionPointer<Windows.Win32.UI.WindowsAndMessaging.WNDPROC>(
                    Windows.Win32.PInvoke.SetWindowLongPtr(
                        new Windows.Win32.Foundation.HWND(Handle),
                        Windows.Win32.UI.WindowsAndMessaging.WINDOW_LONG_PTR_INDEX.GWL_WNDPROC,
                        hotKeyPrcPointer)
                    );
        }

        private void SetTaskbarButtonIcon(NAudio.Wave.PlaybackState playbackState)
        {
            Helpers.SDKs.TaskbarProgress.THUMBBUTTON[] changer;
            if (playbackState == NAudio.Wave.PlaybackState.Playing)
            {
                changer = new Helpers.SDKs.TaskbarProgress.THUMBBUTTON[]
                {
                    new Helpers.SDKs.TaskbarProgress.THUMBBUTTON() { iId = 2, dwMask = Helpers.SDKs.TaskbarProgress.THUMBBUTTONMASK.THB_ICON, dwFlags = Helpers.SDKs.TaskbarProgress.THUMBBUTTONFLAGS.THBF_ENABLED, hIcon = pauseIconHandle, szTip = "播放" }
                };
            }
            else
            {
                changer = new Helpers.SDKs.TaskbarProgress.THUMBBUTTON[]
                {
                    new Helpers.SDKs.TaskbarProgress.THUMBBUTTON() { iId = 2, dwMask = Helpers.SDKs.TaskbarProgress.THUMBBUTTONMASK.THB_ICON, dwFlags = Helpers.SDKs.TaskbarProgress.THUMBBUTTONFLAGS.THBF_ENABLED, hIcon = playIconHandle, szTip = "播放" }
                };
            }
            try
            {
                Helpers.SDKs.TaskbarProgress.MyTaskbarInstance.ThumbBarUpdateButtons(Handle, 3, changer);
            }
            catch(Exception ex)
            {
                DataEditor.LogHelper.WriteLog(nameof(ex), ex.ToString());
            }
        }

        nint appIconHandle = (Bitmap.FromFile(Path.Combine(localPath, "opacityMask.png")).GetThumbnailImage(1, 1, null, 0) as Bitmap).GetHbitmap();
        private const uint WM_HOTKEY = 0x0312;
        private Windows.Win32.UI.WindowsAndMessaging.WNDPROC origPrc;
        private Windows.Win32.UI.WindowsAndMessaging.WNDPROC hotKeyPrc;
        /// <summary>
        /// 窗口获得的系统消息在这里处理
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="uMsg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        private Windows.Win32.Foundation.LRESULT HotKeyPrc(Windows.Win32.Foundation.HWND hwnd,
            uint uMsg,
            Windows.Win32.Foundation.WPARAM wParam,
            Windows.Win32.Foundation.LPARAM lParam)
        {
            //System.Diagnostics.Debug.WriteLine($"Get system message: {uMsg}\n    {wParam.Value}");
            if (uMsg == 806)
            {
                if (bmp != null)
                {
                    // 到了屏幕外面就看不见了 :-)
                    NativeMethods.NativePoint offset = new(-5000, -5000);
                    // 只知道设置1x1的缩略图进去后看起来是正常的...
                    var a = NativeMethods.DwmSetIconicLivePreviewBitmap(Handle, appIconHandle, ref offset, 0);
                }

            }
            else if (uMsg == 273)
            {
                TaskbarButtonInvoke(wParam);
            }
            else if (uMsg == 127)
            {
                SetTaskbarImage(IconPath);
            }

            return Windows.Win32.PInvoke.CallWindowProc(origPrc, hwnd, uMsg, wParam, lParam);
        }

        private async void TaskbarButtonInvoke(Windows.Win32.Foundation.WPARAM wParam)
        {
            switch (wParam.Value)
            {
                case 402653185:
                    await App.playingList.PlayPrevious();
                    break;
                case 402653186:
                    if (App.audioPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                        App.audioPlayer.SetPause();
                    else
                        App.audioPlayer.SetPlay();
                    break;
                case 402653187:
                    await App.playingList.PlayNext();
                    break;
            }
            App.playingList.NowPlayingImageLoaded += PlayingList_NowPlayingImageLoaded;
        }

        private void PlayingList_NowPlayingImageLoaded(ImageSource imageSource, string path)
        {
            App.playingList.NowPlayingImageLoaded -= PlayingList_NowPlayingImageLoaded;
            SetTaskbarImage(path);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("dwmapi.dll")]
        public static extern int DwmSetIconicThumbnail(IntPtr hwnd, IntPtr hbmp, DWM_SIT dwSITFlags);

        [DllImport("dwmapi.dll")]
        internal static extern int DwmSetIconicLivePreviewBitmap(
            IntPtr hwnd,
            IntPtr hbitmap,
            ref NativePoint ptClient,
            uint flags);

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWA dwAttribute, ref int pvAttribute, int cbAttribute);

        public enum DWM_SIT
        {
            None,
            DISPLAYFRAME = 1
        }

        public enum DWMWA
        {
            NCRENDERING_ENABLED = 1,
            NCRENDERING_POLICY,
            TRANSITIONS_FORCEDISABLED,
            ALLOW_NCPAINT,
            CAPTION_BUTTON_BOUNDS,
            NONCLIENT_RTL_LAYOUT,
            FORCE_ICONIC_REPRESENTATION,
            FLIP3D_POLICY,
            EXTENDED_FRAME_BOUNDS,
            // New to Windows 7:
            HAS_ICONIC_BITMAP,
            DISALLOW_PEEK,
            EXCLUDED_FROM_PEEK
            // LAST
        }

        public const uint TRUE = 1;
        /// <summary>
        /// A wrapper for the native POINT structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NativePoint
        {
            /// <summary>
            /// Initialize the NativePoint
            /// </summary>
            /// <param name="x">The x coordinate of the point.</param>
            /// <param name="y">The y coordinate of the point.</param>
            public NativePoint(int x, int y)
                : this()
            {
                X = x;
                Y = y;
            }

            /// <summary>
            /// The X coordinate of the point
            /// </summary>        
            public int X { get; set; }

            /// <summary>
            /// The Y coordinate of the point
            /// </summary>                                
            public int Y { get; set; }

            /// <summary>
            /// Determines if two NativePoints are equal.
            /// </summary>
            /// <param name="first">First NativePoint</param>
            /// <param name="second">Second NativePoint</param>
            /// <returns>True if first NativePoint is equal to the second; false otherwise.</returns>
            public static bool operator ==(NativePoint first, NativePoint second)
            {
                return first.X == second.X
                    && first.Y == second.Y;
            }

            /// <summary>
            /// Determines if two NativePoints are not equal.
            /// </summary>
            /// <param name="first">First NativePoint</param>
            /// <param name="second">Second NativePoint</param>
            /// <returns>True if first NativePoint is not equal to the second; false otherwise.</returns>
            public static bool operator !=(NativePoint first, NativePoint second)
            {
                return !(first == second);
            }

            /// <summary>
            /// Determines if this NativePoint is equal to another.
            /// </summary>
            /// <param name="obj">Another NativePoint to compare</param>
            /// <returns>True if this NativePoint is equal obj; false otherwise.</returns>
            public override bool Equals(object obj)
            {
                return (obj != null && obj is NativePoint) ? this == (NativePoint)obj : false;
            }

            /// <summary>
            /// Gets a hash code for the NativePoint.
            /// </summary>
            /// <returns>Hash code for the NativePoint</returns>
            public override int GetHashCode()
            {
                int hash = X.GetHashCode();
                hash = hash * 31 + Y.GetHashCode();
                return hash;
            }
        }
    }
}
