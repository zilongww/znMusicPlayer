using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace znMusicPlayerWPF
{
    /// <summary>
    /// SetupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetupWindow : Window
    {
        public int NowInstallPageCount = 0;

        public string InstallPath
        {
            get { return installPathTbox.Text; }
            set
            {
                installPathTbox.Text = value;
            }
        }

        public bool UninstallMod = false;
        WindowBlurEffect.WindowAccentCompositor compositor = null;
        public SetupWindow(bool uninstallMod = false)
        {
            Process processes = Process.GetCurrentProcess();
            foreach (var item in Process.GetProcesses())
            {
                if (item.ProcessName == "znMusicPlayer" && item.Id != processes.Id)
                {
                    item.Kill();
                }
            }

            UninstallMod = uninstallMod;
            this.FontFamily = this.FindResource("znNormal") as FontFamily;
            InitializeComponent();
            InstallPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + $"\\{App.BaseName}";
            titleTb.Text = $"{App.BaseName}安装程序 - {App.BaseVersion}";

            UpdataTheme();
            ModernWpf.ThemeManager.Current.ActualApplicationThemeChanged += (s, ev) => UpdataTheme();

            Start();
        }

        public void UpdataTheme()
        {
            if (zilongcn.Others.AppsUseLightTheme())
            {
                if (App.OSVersion == "11")
                    zilongcn.Others.EnableMica(this, false);
                else
                {
                    Resources["backColorTitle"] = MainWindow.DefaultThemeData.BackColor;
                    Resources["backColorBottom"] = MainWindow.DefaultThemeData.BackColor;
                    Resources["backColor"] = MainWindow.DefaultThemeData.BackColor;

                }
                Resources["textColor"] = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0));
                Resources["buttonBackColor"] = new SolidColorBrush(Color.FromArgb(100, 255, 0, 0));
                Resources["buttonBackColor"] = new SolidColorBrush(Color.FromArgb(120, 255, 0, 0));
            }
            else
            {
                if (App.OSVersion == "11")
                    zilongcn.Others.EnableMica(this, true);
                else
                {
                    Resources["backColorTitle"] = MainWindow.DarkThemeData.BackColor;
                    Resources["backColorBottom"] = MainWindow.DarkThemeData.BackColor;
                    Resources["backColor"] = MainWindow.DarkThemeData.BackColor;
                }
                Resources["textColor"] = new SolidColorBrush(Color.FromArgb(140, 255, 255, 255));
                Resources["buttonBackColor"] = new SolidColorBrush(Color.FromArgb(20, 255, 255, 255));
                Resources["buttonBackColor"] = new SolidColorBrush(Color.FromArgb(45, 255, 255, 255));
            }

        }

        public void Start()
        {
            foreach (FrameworkElement frameworkElement in pages.Children)
            {
                frameworkElement.Visibility = Visibility.Collapsed;
            }

            if (!UninstallMod)
            {
                ShowInstallPage1();
            }
            else
            {
                ShowInstallPage5();
            }
            page.Visibility = Visibility.Visible;
        }

        public async Task ClearPage()
        {
            foreach (FrameworkElement frameworkElement in pages.Children)
            {
                var st = new zilongcn.Animations();
                st.animateOpacity(frameworkElement, 1, 0, 0.25);
                st.animatePosition(frameworkElement, frameworkElement.Margin, new Thickness(-50, frameworkElement.Margin.Top, 50, frameworkElement.Margin.Bottom), 0.28, 1, 0);
                st.storyboard.Completed += (s, e) =>
                {
                    frameworkElement.Visibility = Visibility.Collapsed;
                    zilongcn.Animations.animatePosition(frameworkElement, new Thickness(), new Thickness(), 0).Begin();
                };
                st.Begin();
            }
            await Task.Delay(350);
        }

        public async Task SetNextInstallPage()
        {
            await ClearPage();

            titleTb.Text = $"{App.BaseName}安装程序 - {App.BaseVersion}";
            BeforeBtn.IsEnabled = true;
            BeforeBtn.Visibility = Visibility.Visible;
            NextBtn.IsEnabled = true;
            NextBtn.Visibility = Visibility.Visible;
            CancelBtn.IsEnabled = true;
            CancelBtn.Visibility = Visibility.Visible;

            NextBtn.Content = "下一步";

            switch (NowInstallPageCount)
            {
                case 0:
                    ShowInstallPage1();
                    break;
                case 1:
                    ShowInstallPage2();
                    break;
                case 2:
                    ShowInstallPage3();
                    break;
                case 3:
                    ShowInstallPage4();
                    break;
                case 5:
                    ShowInstallPage6();
                    break;
            }

            page.Visibility = Visibility.Visible;
            var st = new zilongcn.Animations();
            st.animateOpacity(page, 0, 1, 0.25);
            st.animatePosition(page, new Thickness(30, page.Margin.Top, -30, page.Margin.Bottom), new Thickness(0), 0.6, 1, 0);
            st.Begin();
        }

        FrameworkElement page = null;

        public void ShowInstallPage1()
        {
            BeforeBtn.Visibility = Visibility.Collapsed;
            NowInstallPageCount = 1;

            installPage1_user.Text = Environment.UserName;

            page = installPage1;
        }

        public void ShowInstallPage2()
        {
            UserArgeeTb.Text = Pages.AboutPage.UserArgee;
            installPage2_notArgeeTb.Visibility = Visibility.Collapsed;
            NowInstallPageCount = 2;

            page = installPage2;
        }

        public void ShowInstallPage3()
        {
            NextBtn.Content = "安装";
            NowInstallPageCount = 3;

            page = installPage3;
        }

        public async void ShowInstallPage4()
        {
            NowInstallPageCount = 4;

            page = installPage4;

            UserArgeeTb.Text = Pages.AboutPage.UserArgee;


            string installState = "安装成功";
            installPage4_installingSl.Visibility = Visibility.Visible;
            NextBtn.Visibility = Visibility.Collapsed;
            BeforeBtn.IsEnabled = false;
            CancelBtn.IsEnabled = false;

            if (installPage4_installingSl.Pause) installPage4_installingSl.Pause = false;
            installPage4_installingTb.Text = "正在安装";

            await Task.Delay(10);
            installPage4_installStateTb.Text = "请稍等...";

            string installpath = InstallPath;

            bool isFolderExists = await Task.Run(() =>
            {
                try
                {
                    if (Directory.Exists(installpath))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            });

            if (!isFolderExists)
            {
                installPage4_installStateTb.Text = "即将安装的文件夹路径不存在，将尝试创建此路径...";

                string folderCreatState = await Task.Run(() =>
                {
                    try
                    {
                        Directory.CreateDirectory(installpath);
                        return "complete";
                    }
                    catch (Exception err)
                    {
                        return err.Message;
                    }
                });

                if (folderCreatState != "complete")
                {
                    installState = "安装失败";
                    installPage4_installStateTb.Text = $"创建安装文件夹路径失败，请重试。\n  -错误信息：{folderCreatState}";
                }
            }

            if (installState == "安装成功")
            {
                await Task.Delay(300);

                installPage4_installStateTb.Text = "复制所需文件...";
                string copySoftwaveState = await Task.Run(() =>
                {
                    try
                    {
                        if (File.Exists(installpath + $"\\{App.BaseName}.exe")) File.Delete(installpath + $"\\{App.BaseName}.exe");
                        File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, installpath + $"\\{App.BaseName}.exe");

                        return "complete";
                    }
                    catch (Exception err)
                    {
                        return err.Message;
                    }
                });

                if (copySoftwaveState != "complete")
                {
                    installState = "安装失败";
                    installPage4_installStateTb.Text = $"复制所需文件时出现错误，请重试。\n  -错误信息：{copySoftwaveState}";
                }
            }

            if (installState == "安装成功")
            {
                installPage4_installStateTb.Text = "更改注册表...";

                try
                {
                    RegistryKey key1 = Registry.CurrentUser.OpenSubKey(@"Software", true);
                    RegistryKey software1 = key1.CreateSubKey(App.BaseName + "WPF");
                    software1.SetValue("Path", installpath + $"\\{App.BaseName}.exe");

                    key1.Close();
                    software1.Close();

                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true);
                    RegistryKey software = key.CreateSubKey(App.BaseName + "WPF");

                    // 图标
                    software.SetValue("DisplayIcon", installpath + $"\\{App.BaseName}.exe");

                    // 显示名
                    software.SetValue("DisplayName", App.BaseName);

                    // 版本
                    software.SetValue("DisplayVersion", App.BaseVersion);

                    // 程序发行公司
                    software.SetValue("Publisher", App.BasePublisher);

                    // 安装位置
                    software.SetValue("InstallLocation", InstallPath);

                    // 安装源
                    software.SetValue("InstallSource", InstallPath);

                    // 帮助电话
                    // software.SetValue("HelpTelephone", "123456789");

                    // 卸载路径
                    software.SetValue("UninstallString", InstallPath + $"\\{App.BaseName}.exe -uninstall");
                    software.Close();
                    key.Close();
                }
                catch (Exception err)
                {
                    installState = "安装失败";
                    installPage4_installStateTb.Text = $"更改注册表时出现错误，请重试。\n  -错误信息：{err.Message}";
                }
            }

            if (installState == "安装成功")
            {
                installPage4_installStateTb.Text = "更改版本信息...";

                string writeVersionState = await Task.Run(() =>
                {
                    try
                    {
                        if (File.Exists(BaseFoldsPath.SoftwareDataPath)) File.Delete(BaseFoldsPath.SoftwareDataPath);
                        BaseFoldsPath.FirstLoad();
                        return "complete";
                    }
                    catch (Exception err)
                    {
                        return err.Message;
                    }
                });

                if (writeVersionState != "complete")
                {
                    installState = "安装失败";
                    installPage4_installStateTb.Text = $"修改数据文件时出现错误，请重试。\n  -错误信息：{writeVersionState}";
                }
            }

            if (installState == "安装成功")
            {
                installPage4_installStateTb.Text = "创建快捷方式...";

                try
                {
                    string Path = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + $"\\{App.BaseName}StartUpShortcut.lnk";

                    if (installSettings_StartUpOCBtn.IsChecked)
                    {
                        if (!File.Exists(Path)) Pages.Setting.CreateShortcut(installpath + "\\" + App.BaseName + ".exe", Path, "NormalMod-SUM");

                        SettingDataEdit.SetParam("StartUp", true.ToString());
                    }
                    else
                    {
                        if (File.Exists(Path)) File.Delete(Path);

                        SettingDataEdit.SetParam("StartUp", false.ToString());
                    }

                    string path = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + $"\\{App.BaseName}StartUpShortcut.lnk";
                    path = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + $"\\{App.BaseName}.lnk";
                    if (installSettings_StartMenusOCBtn.IsChecked)
                    {
                        if (!File.Exists(path)) Pages.Setting.CreateShortcut(installpath + "\\" + App.BaseName + ".exe", path, "NormalMod");
                    }
                    else
                    {
                        if (File.Exists(path)) File.Delete(path);
                    }

                    path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{App.BaseName}.lnk";
                    if (installSettings_desktopsOCBtn.IsChecked)
                    {
                        if (!File.Exists(path)) Pages.Setting.CreateShortcut(installpath + "\\" + App.BaseName + ".exe", path, "NormalMod");
                    }
                    else
                    {
                        if (File.Exists(path)) File.Delete(path);
                    }
                }
                catch (Exception err)
                {
                    installState = "安装失败";
                    installPage4_installStateTb.Text = $"修改数据文件时出现错误，请重试。\n  -错误信息：{err.Message}";
                }
            }

            installPage4_installingTb.Text = installState;
            installPage4_installingSl.Visibility = Visibility.Collapsed;
            installPage4_installingSl.Pause = true;
            BeforeBtn.IsEnabled = true;
            CancelBtn.IsEnabled = true;

            if (installState == "安装成功")
            {
                installPage4_installStateTb.Text = $"现在你可以在你的设备使用 {App.BaseName}。";
                NextBtn.Visibility = Visibility.Visible;
                CancelBtn.Visibility = Visibility.Visible;
                NextBtn.Content = "打开";
                CancelBtn.Content = "关闭";
                BeforeBtn.Visibility = Visibility.Collapsed;
            }
        }

        public void ShowInstallPage5()
        {
            BeforeBtn.Visibility = Visibility.Collapsed;
            NextBtn.Content = "卸载";
            NowInstallPageCount = 5;

            page = installPage5;
        }

        public async void ShowInstallPage6()
        {
            installPage6_uninstallingSl.Pause = false;
            installPage6_uninstallingSl.Visibility = Visibility.Visible;
            BeforeBtn.Visibility = Visibility.Collapsed;
            NextBtn.Visibility = Visibility.Collapsed;
            CancelBtn.Visibility = Visibility.Collapsed;
            NowInstallPageCount = 6;

            page = installPage6;

            string uninstallState = "卸载成功";

            try
            {
                installPage6_uninstallStateTb.Text = "获取程序安装位置...";
                RegistryKey key1 = Registry.CurrentUser.OpenSubKey(@"Software\" + App.BaseName + "WPF", true);
                InstallPath = (string)key1.GetValue("Path");
                key1.Close();
            }
            catch (Exception err)
            {
                uninstallState = "卸载失败";
                installPage6_uninstallStateTb.Text = "获取程序安装位置失败。\n  -错误信息：" + err.Message;
            }

            try
            {
                File.Delete(BaseFoldsPath.SoftwareDataPath);
            }
            catch { }

            if (uninstallState == "卸载成功" && installPage5_deleteCacheOCBtn.IsChecked)
            {
                installPage6_uninstallStateTb.Text = "删除歌曲缓存...";

                if (Directory.Exists(BaseFoldsPath.SongsFolderPath))
                {
                    string deleteCacheState = await Task.Run(() =>
                    {
                        try
                        {
                            string[] FileList = System.IO.Directory.GetFiles(BaseFoldsPath.SongsFolderPath);
                            foreach (string i in FileList)
                            {
                                File.Delete(i);
                            }

                            Directory.Delete(BaseFoldsPath.SongsFolderPath);

                            return "complete";
                        }
                        catch (Exception err)
                        {
                            return err.Message;
                        }
                    });

                    if (deleteCacheState != "complete")
                    {
                        uninstallState = "卸载失败";
                        installPage6_uninstallStateTb.Text = "删除歌曲缓存失败\n  -错误信息：" + deleteCacheState;
                    }
                }
            }

            if (uninstallState == "卸载成功" && installPage5_deleteDataOCBtn.IsChecked)
            {
                installPage6_uninstallStateTb.Text = "删除图片缓存...";

                if (Directory.Exists(BaseFoldsPath.ImageFolderPath))
                {
                    string deleteDataState = await Task.Run(() =>
                    {
                        try
                        {
                            string[] FileList = System.IO.Directory.GetFiles(BaseFoldsPath.ImageFolderPath);
                            foreach (string i in FileList)
                            {
                                File.Delete(i);
                            }

                            Directory.Delete(BaseFoldsPath.ImageFolderPath);

                            return "complete";
                        }
                        catch (Exception err)
                        {
                            return err.Message;
                        }
                    });

                    if (deleteDataState != "complete")
                    {
                        uninstallState = "卸载失败";
                        installPage6_uninstallStateTb.Text = "删除图片缓存失败\n  -错误信息：" + deleteDataState;
                    }
                }
            }

            if (uninstallState == "卸载成功" && installPage5_deleteDataOCBtn.IsChecked)
            {
                installPage6_uninstallStateTb.Text = "删除用户数据...";

                if (Directory.Exists(BaseFoldsPath.LastPath))
                {
                    string deleteDataState = await Task.Run(() =>
                    {
                        try
                        {
                            string[] FileList = System.IO.Directory.GetFiles(BaseFoldsPath.LastPath);
                            foreach (string i in FileList)
                            {
                                File.Delete(i);
                            }

                            Directory.Delete(BaseFoldsPath.LastPath);

                            return "complete";
                        }
                        catch (Exception err)
                        {
                            return err.Message;
                        }
                    });

                    if (deleteDataState != "complete")
                    {
                        uninstallState = "卸载失败";
                        installPage6_uninstallStateTb.Text = "删除用户数据失败\n  -错误信息：" + deleteDataState;
                    }
                }
            }

            if (uninstallState == "卸载成功" && installPage5_deleteDataOCBtn.IsChecked && installPage5_deleteCacheOCBtn.IsChecked)
            {
                installPage6_uninstallStateTb.Text = "删除数据文件夹...";

                if (Directory.Exists(BaseFoldsPath.BasePath))
                {
                    string deleteState = await Task.Run(() =>
                    {
                        try
                        {
                            string[] FileList = System.IO.Directory.GetDirectories(BaseFoldsPath.BasePath);
                            foreach (string i in FileList)
                            {
                                File.Delete(i);
                            }

                            Directory.Delete(BaseFoldsPath.BasePath);

                            return "complete";
                        }
                        catch (Exception err)
                        {
                            return err.Message;
                        }
                    });

                    if (deleteState != "complete")
                    {
                        uninstallState = "卸载失败";
                        installPage6_uninstallStateTb.Text = "删除数据文件夹失败\n  -错误信息：" + deleteState;
                    }
                }
            }

            if (uninstallState == "卸载成功")
            {
                installPage6_uninstallStateTb.Text = "删除应用程序...";

                string deleteState = await Task.Run(() =>
                {
                    try
                    {
                        File.Delete(InstallPath);
                        return "complete";
                    }
                    catch (Exception err)
                    {
                        return err.Message;
                    }
                });

                if (deleteState != "complete" && false)
                {
                    uninstallState = "卸载失败";
                    installPage6_uninstallStateTb.Text = "删除应用程序失败\n  -错误信息：" + deleteState;
                }
            }

            if (uninstallState == "卸载成功")
            {
                try
                {
                    installPage6_uninstallStateTb.Text = "删除注册表数据...";

                    RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true);

                    try
                    {
                        key.DeleteSubKey(App.BaseName + "WPF");
                    }
                    catch (NullReferenceException) { }

                    key.Close();

                    RegistryKey key1 = Registry.CurrentUser.OpenSubKey(@"Software", true);

                    try
                    {
                        key1.DeleteSubKey(App.BaseName + "WPF");
                    }
                    catch (NullReferenceException) { }
                }
                catch (Exception err)
                {
                    uninstallState = "卸载失败";
                    installPage6_uninstallStateTb.Text = "删除注册表数据失败\n  -错误信息：" + err.Message;
                }
            }

            if (uninstallState == "卸载成功")
            {
                installPage6_uninstallStateTb.Text = "删除快捷方式...";

                string deleteState = await Task.Run(() =>
                {
                    try
                    {
                        string Path = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + $"\\{App.BaseName}StartUpShortcut.lnk";
                        if (File.Exists(Path)) File.Delete(Path);

                        Path = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) + $"\\{App.BaseName}.lnk";
                        if (File.Exists(Path)) File.Delete(Path);

                        Path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"\\{App.BaseName}.lnk";
                        if (File.Exists(Path)) File.Delete(Path);

                        return "complete";
                    }
                    catch (Exception err)
                    {
                        return err.Message;
                    }
                });

                if (deleteState != "complete")
                {
                    uninstallState = "卸载失败";
                    installPage6_uninstallStateTb.Text = "删除开机启动快捷方式\n  -错误信息：" + deleteState;
                }
            }

            if (uninstallState != "卸载成功")
            {
                installPage6_uninstallingTb.Text = "卸载失败";
            }
            else
            {
                installPage6_uninstallingTb.Text = uninstallState;
                installPage6_uninstallStateTb.Text = $"{App.BaseName} 已从计算机中移除。";
            }

            installPage6_uninstallingSl.Pause = true;
            installPage6_uninstallingSl.Visibility = Visibility.Collapsed;
            CancelBtn.Visibility = Visibility.Visible;
            CancelBtn.Content = "完成";
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            if (NowInstallPageCount == 6)
            {
                File.Delete(System.IO.Path.GetDirectoryName(InstallPath) + "\\icon.ico");
                DeleteItself();
            }
            this.Close();
        }

        private async void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NowInstallPageCount == 2)
            {
                if (!installPage2_argeeOCBtn.IsChecked)
                {
                    installPage2_notArgeeTb.Visibility = Visibility.Visible;
                    zilongcn.Animations.animateShake(installPage2_notArgeeTb, 4, installPage2_notArgeeTb.Margin, 1.6, new TimeSpan(0, 0, 0, 0, 75));

                    NextBtn.IsEnabled = false;
                    await Task.Delay(402);
                    NextBtn.IsEnabled = true;

                    return;
                }
            }
            else if (NowInstallPageCount == 4)
            {
                System.Diagnostics.Process.Start(InstallPath + $"\\{App.BaseName}.exe");
                Close();
            }

            NextBtn.IsEnabled = false;
            await SetNextInstallPage();
            NextBtn.IsEnabled = true;
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InstallPath = folderBrowserDialog.SelectedPath;
            }
        }

        private async void BeforeBtn_Click(object sender, RoutedEventArgs e)
        {
            NowInstallPageCount -= 2;

            BeforeBtn.IsEnabled = false;
            await SetNextInstallPage();
            BeforeBtn.IsEnabled = true;
        }

        private static void DeleteItself()
        {
            string path = System.Windows.Forms.Application.ExecutablePath;
            string iconPath = System.IO.Path.GetDirectoryName(path) + "\\icon.ico";

            string vBatFile = System.IO.Path.GetDirectoryName(path) + "\\DeleteItself.bat";
            using (StreamWriter vStreamWriter = new StreamWriter(vBatFile, false, Encoding.Default))
            {
                vStreamWriter.Write(
                    ":del\r\n" +
                    $" del \"{path}\"\r\n" +
                    $"if exist \"{path}\" goto del\r\n" +
                    "del %0\r\n"
                );
            }

            // 执行批处理
            WinExec(vBatFile, 0);
        }

        [DllImport("kernel32.dll")]
        public static extern int WinExec(string exeName, int operType);

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            App.OpenWithGreen();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (App.OSVersion == "10")
            {
                compositor.Composite(WindowBlurEffect.WindowAccentCompositor.AccentState.ACCENT_ENABLE_GRADIENT, Color.FromArgb(1, 0, 0, 0), false);
            }
            DragMove();
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (App.OSVersion == "10")
            {
                compositor.Composite(WindowBlurEffect.WindowAccentCompositor.AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND, Color.FromArgb(1, 0, 0, 0), true);
            }
        }

        private void ListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            lrg.Rect = new Rect(0, 0, ual.ActualWidth, ual.ActualHeight);
        }
    }
}
