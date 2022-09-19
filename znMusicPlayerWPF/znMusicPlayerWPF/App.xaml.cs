using Microsoft.VisualBasic.Devices;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;

namespace znMusicPlayerWPF
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {

        public static MainWindow window = null;
        public static string BaseName = "znMusicPlayer";
        public static string BaseVersion = "1.3.0 Beta";
        public static string BasePublisher = "zilongcn";
        public static string OSVersion = new ComputerInfo().OSFullName.Split(' ')[2];

        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        private enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        public static void OpenWithGreen()
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = Assembly.GetExecutingAssembly().Location;
            startInfo.Arguments = "GreenOpenMod";
            System.Diagnostics.Process.Start(startInfo);
            App.Current.Shutdown();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            bool uninstallMod = false;
            bool GreenOpenMod = false;
            if (e.Args.Length != 0)
            {
                foreach (string arg in e.Args)
                {
                    if (arg.Contains("uninstall"))
                    {
                        uninstallMod = true;
                    }
                    else if (arg.Contains("GreenOpenMod"))
                    {
                        GreenOpenMod = true;
                    }
                }
            }

            SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);

            zilongcn.Others.SoftwaveData softwaveData = new zilongcn.Others.SoftwaveData();
            if (!File.Exists(BaseFoldsPath.SoftwareDataPath))
            {
                softwaveData.isHas = false;
            }
            else
            {
                softwaveData.isHas = true;
                string data = System.Text.Encoding.Default.GetString(System.Convert.FromBase64String(File.ReadAllText(BaseFoldsPath.SoftwareDataPath)));
                softwaveData.name = data.Split('\n')[0];
                softwaveData.publisher = BasePublisher;
                softwaveData.version = data.Split('\n')[1];
            }

            //MessageBox.Show($"{softwaveData.isHas.ToString()}\n{softwaveData.version.Split(' ')[0]}\n{BaseVersion.Split(' ')[0]}");
            //MessageBox.Show($"{softwaveData.isHas.ToString()}\n{(e.Args.Length > 0 ? e.Args[0] : "")}");

            if (!GreenOpenMod)
            {
                if (!softwaveData.isHas || new Version(softwaveData.version.Split(' ')[0]) < new Version(BaseVersion.Split(' ')[0]) || uninstallMod)
                {
                    /**
                    * 当前用户是管理员的时候，直接启动应用程序
                    * 如果不是管理员，则使用启动对象启动程序，以确保使用管理员身份运行
                    */
                    //获得当前登录的Windows用户标示
                    WindowsIdentity identity = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    //判断当前登录用户是否为管理员
                    if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                    {
                        //如果是管理员，则直接运行
                        SetupWindow setupWindow = new SetupWindow(uninstallMod);
                        setupWindow.Show();
                    }
                    else
                    {
                        //创建启动对象
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.UseShellExecute = true;
                        startInfo.WorkingDirectory = Environment.CurrentDirectory;
                        startInfo.FileName = Assembly.GetExecutingAssembly().Location;
                        //设置启动动作,确保以管理员身份运行
                        startInfo.Verb = "runas";
                        try
                        {
                            System.Diagnostics.Process.Start(startInfo);
                        }
                        catch
                        {
                            MessageBox.Show("需要管理员权限才能安装。", "权限不足");
                        }
                        //退出
                        Application.Current.Shutdown();
                    }
                    return;
                }

                if (!softwaveData.isHas || new Version(softwaveData.version.Split(' ')[0]) > new Version(BaseVersion.Split(' ')[0]))
                {
                    MessageBox.Show("已安装更新的版本，请使用最新版本。", BaseName);
                    return;
                }
            }

            base.OnStartup(e);
            window = new MainWindow(e.Args);

            //处理未捕获的UI异常
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            //全局异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //处理Task未捕获的异常
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (window != null) if (window.Debug) window.ShowBox($"Task错误: {e.Exception.Message}", e.Exception.ToString());

            //异常标记为“已察觉到”，这样就不会导致程序崩溃
            e.SetObserved();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (window != null) window.ShowBox($"错误", e.ExceptionObject.ToString());
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (window != null) if (window.IsLoadSettings && e.Exception.GetType() != typeof(InvalidOperationException)) window.ShowBox($"错误: {e.Exception.Message}", e.Exception.ToString());
            if (window != null) if (e.Exception.GetType() == typeof(NAudio.MmException) && e.Exception.Message == "BedDeviceId calling waveOutGetDevCaps") window.ShowBox("无法切换到此输出设备", "此输出设备未正常工作或未初始化完成，请重试。");
        }
    }
}

