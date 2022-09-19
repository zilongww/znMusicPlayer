using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : System.Windows.Controls.UserControl
    {
        private MainWindow TheParent = null;
        private Source TheSource = null;

        public Setting(MainWindow Parent, Source Source)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = Source;

            SizeChanged += Parent_SizeChanged;

            UpdataSysInfo();
        }

        public async void UpdataSysInfo()
        {
            if (await zilongcn.Others.GetDirctoryLength(BaseFoldsPath.SongsFolderPath) > double.Parse(SettingDataEdit.GetParam("CacheMaximumSpace")))
            {
                await DeleteCache();
            }

            Microsoft.VisualBasic.Devices.ComputerInfo computerInfo = new Microsoft.VisualBasic.Devices.ComputerInfo();
            string SysOSFullName = computerInfo.OSFullName;
            string SysOSVersion = computerInfo.OSVersion;
            string SysOSPlatform = computerInfo.OSPlatform;
            string SysOSTotalPhysicalMemory = (computerInfo.TotalPhysicalMemory / (decimal)Math.Pow(1024, 3)).ToString("0.00") + " G";
            string SysOSTotalVirtualMemory = (computerInfo.TotalVirtualMemory / (decimal)Math.Pow(1024, 3)).ToString("0.00") + " G";
            string SysOSUsedPhysicalMemory = ((computerInfo.TotalPhysicalMemory - computerInfo.AvailablePhysicalMemory) / (decimal)Math.Pow(1024, 3)).ToString("0.00") + " G";
            string SysOSUsedVirtualMemory = ((computerInfo.TotalVirtualMemory - computerInfo.AvailableVirtualMemory) / (decimal)Math.Pow(1024, 3)).ToString("0.00") + " G";
            string SysOSAvaPhysicalMemory = (computerInfo.AvailablePhysicalMemory / (decimal)Math.Pow(1024, 3)).ToString("0.00") + " G";
            string SysOSAvaVirtualMemory = (computerInfo.AvailableVirtualMemory / (decimal)Math.Pow(1024, 3)).ToString("0.00") + " G";

            //SysInfo.Text = "--------------------------------------" +
            string t = "\n系统：" + SysOSFullName +
                "\n系统版本：" + SysOSVersion +
                "\n系统平台：" + SysOSPlatform +
                "\n--------------------------------------" +
                "\n总物理内存：" + SysOSTotalPhysicalMemory +
                "\n已用物理内存：" + SysOSUsedPhysicalMemory +
                "\n可用物理内存：" + SysOSAvaPhysicalMemory +
                "\n--------------------------------------" +
                "\n总虚拟内存：" + SysOSTotalVirtualMemory +
                "\n已用虚拟内存：" + SysOSUsedVirtualMemory +
                "\n可用虚拟内存：" + SysOSAvaVirtualMemory +
                "\n--------------------------------------" +
                "\n系统语言：" + computerInfo.InstalledUICulture.DisplayName +
                "\n--------------------------------------";

        }

        public void OpenSettingDo()
        {
            try
            {
                ApiChooseCombo.Text = TheParent.audioPlayer.NowOutDevice.DeviceType.ToString();
                PlayModChooseCombo.Text = MusicPlay.MusicPlayMod.PlayModToString(MusicPlay.MusicPlayMod.NowPlayMod);
            }
            catch { }
        }

        public async void UpdataCacheLength()
        {
            await Task.Delay(50);

            try
            {
                cachesl.Pause = false;
                string size = zilongcn.Others.GetAutoSizeString(await zilongcn.Others.GetDirctoryLength(BaseFoldsPath.SongsFolderPath), 2);
                string maximum = await Task.Run(() =>
                {
                    return SettingDataEdit.GetParam("CacheMaximumSpace");
                });

                CacheSetBar.Describe = "缓存占用空间: " + size;
                CacheMaximumSpaceBar.Describe = zilongcn.Others.GetAutoSizeString(double.Parse(maximum), 0);
                CacheMaximumSpaceSlider.Value = double.Parse(maximum);
                cachesl.Pause = true;
            }
            catch (Exception er) { MessageBox.Show(er.ToString()); }
        }

        public async void OpenAndCloseSettingBar()
        {
            try
            {
                foreach (SettingBar settingBar in (ListVisual.Content as StackPanel).Children)
                {
                    settingBar.StyleOpen = true;
                }
                await Task.Delay(30);
                foreach (SettingBar settingBar in (ListVisual.Content as StackPanel).Children)
                {
                    settingBar.StyleOpen = false;
                }
            }
            catch { }
        }

        public void Parent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (ListVisual.ActualWidth >= 1158)
                {
                    ListVisual.HorizontalContentAlignment = HorizontalAlignment.Left;
                    foreach (FrameworkElement frameworkElement in (ListVisual.Content as StackPanel).Children)
                    {
                        frameworkElement.Width = 1130;
                        frameworkElement.HorizontalAlignment = HorizontalAlignment.Left;
                    }
                }
                else
                {
                    ListVisual.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                    foreach (FrameworkElement frameworkElement in (ListVisual.Content as StackPanel).Children)
                    {
                        frameworkElement.Width = double.NaN;
                        frameworkElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                    }
                }
            }
            catch
            { }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if ((string)(sender as Button).Tag == "1")
            {
                Source.OpenFileExplorer(TheParent.LoadPath);
            }
            else
            {
                Source.OpenFileExplorer(TheParent.DownloadPath);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            string Chooser = TheSource.OpenWindowChoiceFolder();
            if (Chooser != null)
            {
                UriAddress.Describe = Chooser;
                TheParent.LoadPath = UriAddress.Describe;
                SettingDataEdit.SetParam("LoadPath", Chooser);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            string FilePath = TheSource.OpenWindowChoiceFlie("音频文件 (*.mp3;*.flac;*.aac;*.wav;*.ra;*.ram;*.au;*.aiff)|*.mp3;*.flac;*.aac;*.wav;*.ra;*.ram;*.au;*.aiff|所有文件 (*.*)|*.*");
            if (FilePath == null) return;

            TheParent.SetBigPage("Set");
            TheParent.SetMusicPage.Set(FilePath);
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            if (TheParent.SetMusicPage.NowFilePaths != null)
            {
                TheParent.SetBigPage("Set");
            }
            else
            {
                TheParent.ShowBox("错误", "没有上次打开的文件");
            }
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            TheSource.JoystickExists();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            string Chooser = TheSource.OpenWindowChoiceFolder();
            if (Chooser != null)
            {
                UriDownloadAddress.Describe = Chooser;
                TheParent.DownloadPath = UriDownloadAddress.Describe;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            TheParent.Debug = (sender as CheckBox).IsChecked == true;
        }

        private void Button_Click_20(object sender, RoutedEventArgs e)
        {
            TheParent.MainBackImage.Source = null;
            TheParent.MainBackImage.Effect = null;
            TheParent.MainBackShadow.Opacity = 0;
            SettingDataEdit.SetParam("Background", false.ToString());
        }

        private void Button_Click_21(object sender, RoutedEventArgs e)
        {
            string path = TheSource.OpenWindowChoiceFlie("图像文件 (*.jpg;*.png)|*.jpg;*.png|所有文件 (*.*)|*.*");
            if (path != null)
            {
                if (TheParent.MainBackImage.Source != null)
                {
                    (TheParent.MainBackImage.Source as BitmapImage).StreamSource = null;
                    TheParent.MainBackImage.Source = null;
                }

                File.Copy(path, BaseFoldsPath.BackgroundPath, true);
                SetBG(path);
                SettingDataEdit.SetParam("Background", true.ToString());
            }
        }

        public void SetBG(string path)
        {
            try
            {
                TheParent.MainBackImage.Source = new BitmapImage(new Uri(path));
            }
            catch (Exception e)
            {
                TheParent.ShowBox("错误", "无法加载图片，请重试。\n\n错误信息: " + e.Message);
                return;
            }
            TheParent.MainBackImage.Margin = new Thickness(-int.Parse(SettingDataEdit.GetParam("BackgroundBlurRadius")));
            TheParent.MainBackImage.Effect = new System.Windows.Media.Effects.BlurEffect() { Radius = int.Parse(SettingDataEdit.GetParam("BackgroundBlurRadius")) };
            TheParent.MainBackShadow.Opacity = double.Parse(SettingDataEdit.GetParam("BackgroundShadow"));
            TheParent.MainBackShadow.Background = new SolidColorBrush(SettingDataEdit.ToColor(SettingDataEdit.GetParam("BackgroundShadowColor")));
            //TheParent.LeftBar.Background = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255));

            //BackgroundBlurRadiusTextBox.Text = SettingDataEdit.GetParam("BackgroundBlurRadius");
            //BackgroundShadowTextBox.Text = SettingDataEdit.GetParam("BackgroundShadow");
            //BackgroundShadowColorPicker.Color = SettingDataEdit.ToColor(SettingDataEdit.GetParam("BackgroundShadowColor"));
        }

        private void Button_Click_22(object sender, RoutedEventArgs e)
        {
            Source.OpenFileExplorer(UriAddress.Describe);
        }

        private void Button_Click_23(object sender, RoutedEventArgs e)
        {
            Source.OpenFileExplorer(UriDownloadAddress.Describe);
        }

        public async static Task<bool> DeleteCache()
        {
            return await Task.Run(() =>
            {
                List<string> LastNameList = new List<string> { "mp3", "wma", "aac", "m4a", "flac" };
                var FileList = from wdl in System.IO.Directory.GetFiles(BaseFoldsPath.SongsFolderPath)
                               where LastNameList.Contains(wdl.Split('.')[1])
                               select wdl;

                foreach (string i in FileList)
                {
                    try
                    {
                        File.Delete(i);
                    }
                    catch { return false; }
                }
                return true;
            });
        }

        private async void znButton_Click(object sender, RoutedEventArgs e)
        {
            bool IsDelete = await DeleteCache();

            TheParent.ShowBox("删除缓存", "正在删除缓存文件......", MainWindow.ShowBoxStyle.Loading);

            if (IsDelete)
            {
                TheParent.ShowBox("删除缓存", "已将所有缓存文件删除。");
            }
            else
            {
                TheParent.ShowBox("删除缓存", "一些缓存文件因被程序占用而无法删除，但未被占用的缓存已删除。");
            }

            UpdataCacheLength();
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            TheParent.MainBack.Background = new SolidColorBrush(Color.FromArgb(70, 255, 255, 255));
        }

        private void znButton_Click_2(object sender, RoutedEventArgs e)
        {
            TheSource.BSOD();
        }

        private void OCButton_Checked_2(MyC.OCButton sender)
        {
            /*
            TheParent.Topmost = sender.IsChecked;

            if (sender.IsChecked) { TopmostText.Text = "开"; SettingDataEdit.SetParam("WindowTopmost", true.ToString()); }
            else { TopmostText.Text = "关"; SettingDataEdit.SetParam("WindowTopmost", false.ToString()); }*/
        }

        private void ReCallButton_Click(object sender, RoutedEventArgs e)
        {
            UpdataSysInfo();
        }

        public static void CreateShortcut(string lnkFilePath, string args = "")
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            var shortcut = shell.CreateShortcut(lnkFilePath);
            shortcut.TargetPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            shortcut.Arguments = args;
            shortcut.WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            shortcut.Save();
        }

        public static void CreateShortcut(string path, string lnkFilePath, string args = "")
        {
            var shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            var shortcut = shell.CreateShortcut(lnkFilePath);
            shortcut.TargetPath = path;
            shortcut.Arguments = args;
            shortcut.WorkingDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            shortcut.Save();
        }

        private void StartUpOCBtn_Checked(MyC.OCButton sender)
        {
            /*
            string Path = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\kwMusicDownloadStartUpShortcut.lnk";
            if (sender.IsChecked)
            {
                if (!File.Exists(Path)) CreateShortcut(Path, "NormalMod-SUM");
                StartUpText.Text = "开";
                SettingDataEdit.SetParam("StartUp", true.ToString());
            }
            else
            {
                if (File.Exists(Path)) File.Delete(Path);
                StartUpText.Text = "关";
                SettingDataEdit.SetParam("StartUp", false.ToString());
            }*/
        }

        private void BlurLrcOCBtn_Checked(MyC.OCButton sender)
        {
            /*
            SettingDataEdit.SetParam("LrcBlurBackground", sender.IsChecked.ToString());
            TheParent.IsBlurDesktopLrc = sender.IsChecked;

            if (sender.IsChecked)
            {
                BlurLrcText.Text = "开";
            }
            else
            {
                BlurLrcText.Text = "关";
            }*/
        }

        private void znButton_Click_6(object sender, RoutedEventArgs e)
        {
            /*
            int Radius = 0;

            try
            {
                int.Parse(BackgroundBlurRadiusTextBox.Text);
            }
            catch
            {
                TheParent.ShowBox("错误：类型不正确", "请输入正确的int类型。");
                return;
            }

            Radius = int.Parse(BackgroundBlurRadiusTextBox.Text);
            if (Radius >= 100) Radius = 100;

            SettingDataEdit.SetParam("BackgroundBlurRadius", Radius.ToString());

            if (TheParent.MainBackImage.Source != null)
            {

                TheParent.MainBackImage.Effect = new System.Windows.Media.Effects.BlurEffect() { Radius = Radius };

                TheParent.MainBackImage.Margin = new Thickness(-Radius);

                BackgroundBlurRadiusTextBox.Text = Radius.ToString();
            }*/
        }

        private void znButton_Click_7(object sender, RoutedEventArgs e)
        {
            /*
            try
            {
                double.Parse(BackgroundShadowTextBox.Text);
            }
            catch
            {
                TheParent.ShowBox("错误：输入的类型不正确", "请输入正确的double类型。");
                return;
            }

            SettingDataEdit.SetParam("BackgroundShadow", double.Parse(BackgroundShadowTextBox.Text).ToString());

            if (TheParent.MainBackImage.Source == null) return;

            TheParent.MainBackShadow.Opacity = double.Parse(BackgroundShadowTextBox.Text);*/
        }

        private void znButton_Click_8(object sender, RoutedEventArgs e)
        {
            /*
            if ((sender as MyC.znButton).Content.ToString() == "取消")
            {
                BackgroundShadowColorPicker.Visibility = Visibility.Collapsed;
                BackgroundColorPickerCancelButton.Visibility = Visibility.Collapsed;
                return;
            }

            if (BackgroundShadowColorPicker.Visibility == Visibility.Collapsed)
            {
                (sender as MyC.znButton).Content = "确定";
                BackgroundShadowColorPicker.Visibility = Visibility.Visible;
                BackgroundColorPickerCancelButton.Visibility = Visibility.Visible;
            }
            else
            {
                Color color = BackgroundShadowColorPicker.Color;
                SettingDataEdit.SetParam("BackgroundShadowColor", $"{color.A},{color.R},{color.G},{color.B}");
                TheParent.MainBackShadow.Background = new SolidColorBrush(color);

                (sender as MyC.znButton).Content = "选择";
                BackgroundShadowColorPicker.Visibility = Visibility.Collapsed;
                BackgroundColorPickerCancelButton.Visibility = Visibility.Collapsed;
            }*/
        }

        private void AnimateOCBtn_LockedClick(MyC.OCButton sender)
        {
            TheParent.ShowBox("错误", "开启Cheat Engine时无法开启动画。");
        }

        private void znButton_Click_9(object sender, RoutedEventArgs e)
        {
            /*
            if (BGMS_Text.Text == "更多设置")
            {
                BGMS1.Visibility = Visibility.Visible;
                BGMS_Text.Text = "收起";
                BGMS_Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ExpandLess;
            }
            else
            {
                BGMS1.Visibility = Visibility.Collapsed;
                BGMS_Text.Text = "更多设置";
                BGMS_Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.ExpandMore;
            }*/
        }

        private void LittleMusicPageOCBtn_Checked(MyC.OCButton sender)
        {
            /*
            if (TheParent.IsLoadSettings) SettingDataEdit.SetParam("LittleMusicPage", sender.IsChecked.ToString());

            if (sender.IsChecked) LittleMusicPageText.Text = "开";
            else LittleMusicPageText.Text = "关";*/
        }

        private void VolumeDataSystemOCBtn_Checked(MyC.OCButton sender)
        {
            /*
            if (TheParent.IsLoadSettings) SettingDataEdit.SetParam("VolumeDataSystem", sender.IsChecked.ToString());

            if (sender.IsChecked) VolumeDataSystemText.Text = "开";
            else VolumeDataSystemText.Text = "关";*/
        }

        private void znButton_Click_10(object sender, RoutedEventArgs e)
        {
            TheParent.NowThemeData = TheParent.NowThemeData.MD5 == MainWindow.DefaultThemeData.MD5 ? MainWindow.DarkThemeData : MainWindow.DefaultThemeData;
        }

        // ( •̀ ω •́ )y 分割线
        public static MyC.OCButton OCButtonLoadedThemeChange(MyC.OCButton ocb)
        {
            if (App.window == null) return null;

            ocb.Stroke = App.window.NowThemeData.InColorTextColor;
            ocb.OCBackground = App.window.NowThemeData.InColorTextColor;
            ocb.MouseEnterBackground = App.window.NowThemeData.ButtonEnterColor;
            ocb.MouseEnterOCBackground = App.window.NowThemeData.InColorTextColor;
            ocb.MouseEnterStroke = App.window.NowThemeData.InColorTextColor;

            ocb.IsCMouseEnterBackground = App.window.NowThemeData.ButtonBackColor;
            ocb.IsCMouseEnterOCBackground = App.window.NowThemeData.InColorTextColor;
            ocb.IsCMouseEnterStroke = App.window.NowThemeData.InColorTextColor;
            ocb.IsCheckedStroke = App.window.NowThemeData.InColorTextColor;
            ocb.IsCheckedOCBackground = App.window.NowThemeData.InColorTextColor;
            ocb.IsCheckedBackground = App.window.NowThemeData.ButtonEnterColor;

            ocb.IsMouseDownOCBackground = App.window.NowThemeData.InColorTextColor;
            ocb.IsMouseDownStroke = App.window.NowThemeData.InColorTextColor;
            ocb.IsMouseDownBackground = App.window.NowBlurThemeData.ButtonEnterColor;

            return ocb;
        }

        #region 设置行
        private async void SettingBar_Click(SettingBar settingBar)
        {
            settingBar.StyleOpen = !settingBar.StyleOpen;
            switch ((string)settingBar.Tag)
            {
                case "cache":
                    if (settingBar.StyleOpen) UpdataCacheLength();
                    break;
            }

            if (settingBar.StyleOpen == false) return;
            Vector a = VisualTreeHelper.GetOffset(settingBar);
            ListVisual.VerticalScrollValue = a.Length - 2;
            //ListVisual.ScrollIntoView(settingBar);
        }

        private void SettingBar_Click_1(SettingBar settingBar)
        {
            if (settingBar.StyleOpen)
            {
                settingBar.StyleOpen = false;
                settingBar.CornerRadius = new CornerRadius(0);
            }
            else
            {
                settingBar.StyleOpen = true;
                settingBar.CornerRadius = new CornerRadius(0);
            }
        }
        #endregion

        #region 缓存设置行
        public SettingBar UriAddress;
        private void UriAddress_Loaded(object sender, RoutedEventArgs e)
        {
            UriAddress = sender as SettingBar;
            UriAddress.Describe = BaseFoldsPath.SongsFolderPath;
        }
        #endregion

        #region 下载设置行
        public SettingBar UriDownloadAddress;
        private void UriDownloadAddress_Loaded(object sender, RoutedEventArgs e)
        {
            UriDownloadAddress = sender as SettingBar;
            UriDownloadAddress.Describe = BaseFoldsPath.DownloadFolderPath;
        }
        #endregion

        #region 缓存占用行
        public SettingBar CacheSetBar;
        private void SettingBar_Loaded(object sender, RoutedEventArgs e)
        {
            CacheSetBar = sender as SettingBar;
        }

        public MyC.SimpleLoading cachesl;
        private void SimpleLoading_Initialized(object sender, RoutedEventArgs e)
        {
            cachesl = sender as MyC.SimpleLoading;
        }
        #endregion

        #region 缓存最大占用空间
        public SettingBar CacheMaximumSpaceBar;
        public Slider CacheMaximumSpaceSlider;
        private void SettingBar_Loaded_17(object sender, RoutedEventArgs e)
        {
            CacheMaximumSpaceBar = sender as SettingBar;
        }
        private void znSlider_Loaded(object sender, RoutedEventArgs e)
        {
            CacheMaximumSpaceSlider = sender as Slider;
        }
        private void znSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                CacheMaximumSpaceBar.Describe = zilongcn.Others.GetAutoSizeString(CacheMaximumSpaceSlider.Value, 0);
                SettingDataEdit.SetParam("CacheMaximumSpace", CacheMaximumSpaceSlider.Value.ToString());
            }
            catch { }
        }
        #endregion

        #region 选择播放api行
        public SettingBar ApiChooseBar;
        public MyC.znComboBox ApiChooseCombo;
        private void SettingBar_Loaded_1(object sender, RoutedEventArgs e)
        {
            ApiChooseBar = sender as SettingBar;
            ApiChooseCombo = ApiChooseBar.ButtonContent as MyC.znComboBox;
            //ApiChooseCombo.Text = TheParent.audioPlayer.AudioOutApi.ToString();
        }

        private void znComboBox_ContentClick(MyC.znComboBox znComboBox, object data)
        {
            /*
            switch ((string)data)
            {
                case "WaveOut":
                    TheParent.audioPlayer.AudioOutApi = MusicPlay.AudioPlayer.AudioOutApiEnum.WaveOut;
                    break;
                case "DirectSound":
                    TheParent.audioPlayer.AudioOutApi = MusicPlay.AudioPlayer.AudioOutApiEnum.DirectSound;
                    break;
                case "Wasapi":
                    TheParent.audioPlayer.AudioOutApi = MusicPlay.AudioPlayer.AudioOutApiEnum.Wasapi;
                    break;
                case "Asio":
                    TheParent.audioPlayer.AudioOutApi = MusicPlay.AudioPlayer.AudioOutApiEnum.Asio;
                    break;
            }
            znComboBox.Text = (string)data;
            TheParent.audioPlayer.Reload();*/
        }
        #endregion

        #region 播放模式行
        public SettingBar PlayModChooseBar;
        public MyC.znComboBox PlayModChooseCombo;
        private void SettingBar_Loaded_2(object sender, RoutedEventArgs e)
        {
            PlayModChooseBar = sender as SettingBar;
            PlayModChooseCombo = PlayModChooseBar.ButtonContent as MyC.znComboBox;
            PlayModChooseCombo.Text = MusicPlay.MusicPlayMod.PlayModToString(MusicPlay.MusicPlayMod.NowPlayMod);
            MusicPlay.MusicPlayMod.PlayModChange += MusicPlayMod_PlayModChange;
        }

        private void znComboBox_ContentClick_1(MyC.znComboBox znComboBox, object data)
        {
            switch ((string)data)
            {
                case "顺序播放":
                    MusicPlay.MusicPlayMod.NowPlayMod = MusicPlay.MusicPlayMod.PlayMod.Seq;
                    break;
                case "随机播放":
                    MusicPlay.MusicPlayMod.NowPlayMod = MusicPlay.MusicPlayMod.PlayMod.Random;
                    break;
                case "单曲循环":
                    MusicPlay.MusicPlayMod.NowPlayMod = MusicPlay.MusicPlayMod.PlayMod.Loop;
                    break;
            }
            znComboBox.Text = data as string;
        }

        private void SettingBar_Unloaded(object sender, RoutedEventArgs e)
        {
            MusicPlay.MusicPlayMod.PlayModChange -= MusicPlayMod_PlayModChange;
        }

        private void MusicPlayMod_PlayModChange(MusicPlay.MusicPlayMod.PlayMod playMod)
        {
            PlayModChooseCombo.Text = MusicPlay.MusicPlayMod.PlayModToString(MusicPlay.MusicPlayMod.NowPlayMod);
        }
        #endregion

        #region Wasapi独占行
        public SettingBar WasapiNotShareBar;
        public MyC.OCButton WasapiNotShareOCBtn;
        public TextBlock WasapiNotShareText;
        private void SettingBar_Loaded_3(object sender, RoutedEventArgs e)
        {
            WasapiNotShareBar = sender as SettingBar;
            WasapiNotShareOCBtn = (WasapiNotShareBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            WasapiNotShareText = (WasapiNotShareBar.ButtonContent as StackPanel).Children[0] as TextBlock;

            WasapiNotShareOCBtn.IsChecked = TheParent.WasapiNotShare;
            OCButtonLoadedThemeChange(WasapiNotShareOCBtn);
        }

        private async void WasapiNotShareOCBtn_Checked(MyC.OCButton sender)
        {
            if (sender.IsChecked) WasapiNotShareText.Text = "开";
            else WasapiNotShareText.Text = "关";

            if (TheParent.WasapiNotShare == sender.IsChecked) return;

            TheParent.WasapiNotShare = !TheParent.WasapiNotShare;

            if (TheParent.audioPlayer.NowOutDevice.DeviceType == MusicPlay.AudioPlayer.AudioOutApiEnum.Wasapi) await TheParent.audioPlayer.Reload();
        }
        #endregion

        #region 桌面歌词居中行
        public SettingBar DesktopLrcCenterBar;
        public MyC.OCButton DesktopLrcCenterOCBtn;
        public TextBlock DesktopLrcCenterText;
        private void SettingBar_Loaded_4(object sender, RoutedEventArgs e)
        {
            DesktopLrcCenterBar = sender as SettingBar;
            DesktopLrcCenterOCBtn = (DesktopLrcCenterBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            DesktopLrcCenterText = (DesktopLrcCenterBar.ButtonContent as StackPanel).Children[0] as TextBlock;

            DesktopLrcCenterOCBtn.IsChecked = TheParent.LrcWindow.IsCenter;
            OCButtonLoadedThemeChange(DesktopLrcCenterOCBtn);
        }

        private void DesktopCenterOCButton_Checked(MyC.OCButton sender)
        {
            if (sender.IsChecked) DesktopLrcCenterText.Text = "开";
            else DesktopLrcCenterText.Text = "关";

            if (sender.IsChecked == TheParent.LrcWindow.IsCenter) return;
            TheParent.LrcWindow.IsCenter = !TheParent.LrcWindow.IsCenter;
        }
        #endregion

        #region 桌面歌词只显示翻译行
        public SettingBar ShowTranslateOnlyBar;
        public MyC.OCButton ShowTranslateOnlyOCBtn;
        public TextBlock ShowTranslateOnlyText;
        private void SettingBar_Loaded_5(object sender, RoutedEventArgs e)
        {
            ShowTranslateOnlyBar = sender as SettingBar;
            ShowTranslateOnlyOCBtn = (ShowTranslateOnlyBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            ShowTranslateOnlyText = (ShowTranslateOnlyBar.ButtonContent as StackPanel).Children[0] as TextBlock;

            ShowTranslateOnlyOCBtn.IsChecked = bool.Parse(SettingDataEdit.GetParam("ShowTranslateOnly"));
            OCButtonLoadedThemeChange(ShowTranslateOnlyOCBtn);
        }

        private void ShowTranslateOnlyOCBtn_Checked(MyC.OCButton sender)
        {
            SettingDataEdit.SetParam("ShowTranslateOnly", sender.IsChecked.ToString());
            if (sender.IsChecked) ShowTranslateOnlyText.Text = "开";
            else ShowTranslateOnlyText.Text = "关";
        }
        #endregion

        #region 跟随鼠标的桌面歌词行
        public SettingBar FollowMouseLrcBar;
        public MyC.OCButton FollowMouseLrcOCBtn;
        public TextBlock FollowMouseLrcText;
        private void SettingBar_Loaded_6(object sender, RoutedEventArgs e)
        {
            FollowMouseLrcBar = sender as SettingBar;
            FollowMouseLrcOCBtn = (FollowMouseLrcBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            FollowMouseLrcText = (FollowMouseLrcBar.ButtonContent as StackPanel).Children[0] as TextBlock;
            FollowMouseLrcOCBtn.IsChecked = false;

            OCButtonLoadedThemeChange(FollowMouseLrcOCBtn);
        }

        MyC.PopupWindow popupWindow = null;
        SolidColorBrush lrcTextColor = new SolidColorBrush(Colors.White);
        private void FollowMouseLrcOCBtn_Checked(MyC.OCButton sender)
        {
            if (sender.IsChecked && popupWindow == null)
            {
                FollowMouseLrcText.Text = "开";

                popupWindow = new MyC.PopupWindow() { ForceAcrylicBlur = false };
                try
                {
                    Color BackColor = (TheParent.LrcColor as SolidColorBrush).Color;

                    //popupWindow.BlurBackgroundColor = Color.FromArgb(25, BackColor.R, BackColor.G, BackColor.B);
                    popupWindow.Content = new TextBlock() { Text = MainWindow.BaseName, Foreground = TheParent.NowBlurThemeData.TextColor, FontSize = (double)this.FindResource("提示字体大小"), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    popupWindow.UserShow();
                    popupWindow.FollowMouse = true;

                    TheParent.MusicPages.LrcChangeEvent += async (nowLrcData, nextLrcData) =>
                    {
                        if (popupWindow == null) return;

                        try
                        {
                            (popupWindow.Content as TextBlock).Text = TheParent.LrcWindow.IsTranslate ? $"{nowLrcData.LrcText}\n{nowLrcData.TranslateLrcText.Text}" : nowLrcData.LrcText;

                        }
                        catch { return; }

                        await Task.Delay(1);
                        popupWindow.SetSize();
                    };
                }
                catch { }
            }
            else
            {
                FollowMouseLrcText.Text = "关";

                try { popupWindow.UserClose(); }
                catch { }

                popupWindow = null;
            }
        }
        #endregion

        #region 画面渲染

        #region 对其边界
        public SettingBar UseLayoutRoundingBar;
        public MyC.OCButton UseLayoutRoundingOCBtn;
        public TextBlock UseLayoutRoundingText;
        private void SettingBar_Loaded_7(object sender, RoutedEventArgs e)
        {
            UseLayoutRoundingBar = sender as SettingBar;
            UseLayoutRoundingOCBtn = (UseLayoutRoundingBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            UseLayoutRoundingText = (UseLayoutRoundingBar.ButtonContent as StackPanel).Children[0] as TextBlock;

            UseLayoutRoundingOCBtn.IsChecked = bool.Parse(SettingDataEdit.GetParam("UseLayoutRounding"));
            OCButtonLoadedThemeChange(UseLayoutRoundingOCBtn);
        }

        private void UseLayoutRoundingOCBtn_Checked(MyC.OCButton sender)
        {
            if (sender.IsChecked) UseLayoutRoundingText.Text = "开";
            else UseLayoutRoundingText.Text = "关";

            if (TheParent.UseLayoutRounding == sender.IsChecked) return;

            TheParent.UseLayoutRounding = !TheParent.UseLayoutRounding;
            SettingDataEdit.SetParam("UseLayoutRounding", sender.IsChecked.ToString());
        }
        #endregion

        #region 对其显示器像素单位
        public SettingBar SnapsToDevicePixelsBar;
        public MyC.OCButton SnapsToDevicePixelsOCBtn;
        public TextBlock SnapsToDevicePixelsText;
        private void SettingBar_Loaded_8(object sender, RoutedEventArgs e)
        {
            SnapsToDevicePixelsBar = sender as SettingBar;
            SnapsToDevicePixelsOCBtn = (SnapsToDevicePixelsBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            SnapsToDevicePixelsText = (SnapsToDevicePixelsBar.ButtonContent as StackPanel).Children[0] as TextBlock;

            SnapsToDevicePixelsOCBtn.IsChecked = bool.Parse(SettingDataEdit.GetParam("SnapsToDevicePixels"));
            OCButtonLoadedThemeChange(SnapsToDevicePixelsOCBtn);
        }

        private void SnapsToDevicePixelsOCBtn_Checked(MyC.OCButton sender)
        {
            if (sender.IsChecked) SnapsToDevicePixelsText.Text = "开";
            else SnapsToDevicePixelsText.Text = "关";

            if (TheParent.SnapsToDevicePixels == sender.IsChecked) return;

            TheParent.SnapsToDevicePixels = !TheParent.SnapsToDevicePixels;
            SettingDataEdit.SetParam("SnapsToDevicePixels", sender.IsChecked.ToString());
        }
        #endregion

        #region 清晰字体
        public SettingBar TextHintingModeBar;
        public MyC.OCButton TextHintingModeOCBtn;
        public TextBlock TextHintingModeText;
        private void SettingBar_Loaded_9(object sender, RoutedEventArgs e)
        {
            TextHintingModeBar = sender as SettingBar;
            TextHintingModeOCBtn = (TextHintingModeBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            TextHintingModeText = (TextHintingModeBar.ButtonContent as StackPanel).Children[0] as TextBlock;

            TextHintingModeOCBtn.IsChecked = SettingDataEdit.ToTextHintingMode(SettingDataEdit.GetParam("TextHintingMode")) == TextHintingMode.Fixed ? true : false;
            OCButtonLoadedThemeChange(TextHintingModeOCBtn);
        }

        private void TextHintingModeOCBtn_Checked(MyC.OCButton sender)
        {
            if (sender.IsChecked) TextHintingModeText.Text = "开";
            else TextHintingModeText.Text = "关";

            if (sender.IsChecked)
            {
                if (TextOptions.GetTextHintingMode(TheParent) != TextHintingMode.Fixed)
                {
                    TextOptions.SetTextHintingMode(TheParent, TextHintingMode.Fixed);
                    TextOptions.SetTextHintingMode(TheParent.LrcWindow, TextHintingMode.Fixed);
                }
            }
            else
            {
                if (TextOptions.GetTextHintingMode(TheParent) != TextHintingMode.Animated)
                {
                    TextOptions.SetTextHintingMode(TheParent, TextHintingMode.Animated);
                    TextOptions.SetTextHintingMode(TheParent.LrcWindow, TextHintingMode.Animated);
                }
            }

            SettingDataEdit.SetParam("TextHintingMode", sender.IsChecked ? TextHintingMode.Fixed.ToString() : TextHintingMode.Animated.ToString());
        }
        #endregion

        #endregion

        #region 动画
        public SettingBar AnimationBar;
        public MyC.OCButton AnimationOCBtn;
        public TextBlock AnimationText;
        private void SettingBar_Loaded_10(object sender, RoutedEventArgs e)
        {
            AnimationBar = sender as SettingBar;
            AnimationOCBtn = (AnimationBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            AnimationText = (AnimationBar.ButtonContent as StackPanel).Children[0] as TextBlock;

            AnimationOCBtn.IsChecked = bool.Parse(SettingDataEdit.GetParam("Animate"));
            OCButtonLoadedThemeChange(AnimationOCBtn);
        }

        private void OCButton_Checked_1(MyC.OCButton sender)
        {
            TheParent.Animation = sender.IsChecked;

            if (sender.IsChecked) { AnimationText.Text = "开"; SettingDataEdit.SetParam("Animate", true.ToString()); }
            else { AnimationText.Text = "关"; SettingDataEdit.SetParam("Animate", false.ToString()); }
        }
        #endregion

        #region 背景图
        public SettingBar BackImageBar;
        public MyC.znButton BackImageSetBtn;
        public MyC.znButton BackImageDefaultBtn;
        private void SettingBar_Loaded_11(object sender, RoutedEventArgs e)
        {
            BackImageBar = sender as SettingBar;
        }

        private void znButton_Click_11(object sender, RoutedEventArgs e)
        {
            string path = TheSource.OpenWindowChoiceFlie("图像文件 (*.jpg;*.png)|*.jpg;*.png|所有文件 (*.*)|*.*");
            if (path != null)
            {
                if (TheParent.MainBackImage.Source != null)
                {
                    (TheParent.MainBackImage.Source as BitmapImage).StreamSource = null;
                    TheParent.MainBackImage.Source = null;
                }

                File.Copy(path, BaseFoldsPath.BackgroundPath, true);
                SetBG(path);
                SettingDataEdit.SetParam("Background", true.ToString());
            }
        }

        private void znButton_Click_13(object sender, RoutedEventArgs e)
        {
            TheParent.MainBackImage.Source = null;
            TheParent.MainBackImage.Effect = null;
            TheParent.MainBackShadow.Opacity = 0;
            SettingDataEdit.SetParam("Background", false.ToString());
        }
        #endregion

        #region 高斯模糊设置
        public SettingBar BlurSetBar;
        public TextBox BlurSetTBox;
        public MyC.znButton BlurSetBtn;
        private void SettingBar_Loaded_12(object sender, RoutedEventArgs e)
        {
            BlurSetBar = sender as SettingBar;
            BlurSetTBox = (BlurSetBar.ButtonContent as StackPanel).Children[0] as TextBox;
            BlurSetBtn = (BlurSetBar.ButtonContent as StackPanel).Children[1] as MyC.znButton;

            BlurSetTBox.Text = TheParent.Blurs.ToString();
        }

        private void znButton_Click_12(object sender, RoutedEventArgs e)
        {
            try
            {
                int radius = int.Parse(BlurSetTBox.Text);
                if (radius >= 100) radius = 100;

                TheParent.Blurs = radius;
                SettingDataEdit.SetParam("BlurRadius", radius.ToString());
                BlurSetTBox.Text = radius.ToString();
            }
            catch
            {
                TheParent.ShowBox("错误", "请输入正确的int类型。");
            }
        }
        #endregion

        #region 恢复默认设置
        private void znButton_Click_14(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(BaseFoldsPath.SettingDataPath, BaseFoldsPath.NormalSettingText);
            TheParent.WaitSettings();
            TheParent.SetPage("Main");
            TheParent.ShowBox("恢复成功", "所有保存的设置数据已恢复到默认。");
        }

        #endregion

        #region 重载设置数据
        private void znButton_Click_15(object sender, RoutedEventArgs e)
        {
            TheParent.WaitSettings();
            TheParent.ThemeChangeDo();
        }
        #endregion

        #region 退出程序
        private void znButton_Click_16(object sender, RoutedEventArgs e)
        {
            TheParent.MenuExit_Click(null, null);
        }
        #endregion

        #region 软件渲染
        public SettingBar SoftwareVisualBar;
        public MyC.OCButton SoftwareVisualOCBtn;
        public TextBlock SoftwareVisualText;
        private void SettingBar_Loaded_13(object sender, RoutedEventArgs e)
        {
            SoftwareVisualBar = sender as SettingBar;
            SoftwareVisualOCBtn = (SoftwareVisualBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            SoftwareVisualText = (SoftwareVisualBar.ButtonContent as StackPanel).Children[0] as TextBlock;

            SoftwareVisualOCBtn.IsChecked = TheParent.SoftwareVisual;
            OCButtonLoadedThemeChange(SoftwareVisualOCBtn);
        }

        private void OCButton_Checked(MyC.OCButton sender)
        {
            if (sender.IsChecked) SoftwareVisualText.Text = "开";
            else SoftwareVisualText.Text = "关";

            if (TheParent.SoftwareVisual == sender.IsChecked) return;

            TheParent.SoftwareVisual = !TheParent.SoftwareVisual;
        }
        #endregion

        #region 显示桌面歌词窗口进程
        public SettingBar LrcWindowTaskBarShowBar;
        public MyC.OCButton LrcWindowTaskBarShowOCBtn;
        public TextBlock LrcWindowTaskBarShowText;
        private void SettingBar_Loaded_14(object sender, RoutedEventArgs e)
        {
            LrcWindowTaskBarShowBar = sender as SettingBar;
            //LrcWindowTaskBarShowOCBtn = (LrcWindowTaskBarShowBar.ButtonContent as StackPanel).Children[1] as MyC.OCButton;
            //LrcWindowTaskBarShowText = (LrcWindowTaskBarShowBar.ButtonContent as StackPanel).Children[0] as TextBlock;

            //LrcWindowTaskBarShowOCBtn.IsChecked = TheParent.SoftwareVisual;
            //OCButtonLoadedThemeChange(LrcWindowTaskBarShowOCBtn);
        }

        private void znButton_Click_3(object sender, RoutedEventArgs e)
        {
            TheParent.SetBigPage("LrcContent");
        }

        private void OCButton_Checked_3(MyC.OCButton sender)
        {

            if (sender.IsChecked) LrcWindowTaskBarShowText.Text = "开";
            else LrcWindowTaskBarShowText.Text = "关";

            if (TheParent.LrcWindowTaskBarShow == sender.IsChecked) return;
            TheParent.LrcWindowTaskBarShow = !TheParent.LrcWindowTaskBarShow;
        }
        #endregion

        #region 主题色
        public SettingBar ThemeColorBar;
        public MyC.OCButton ThemeColorOCBtn;
        public TextBlock ThemeColorText;
        private void SettingBar_Loaded_15(object sender, RoutedEventArgs e)
        {

        }

        private void znButton_Click_4(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region 睡眠定时
        public SettingBar SleepBar = null;
        private void SettingBar_Loaded_16(object sender, RoutedEventArgs e)
        {
            SleepBar = sender as SettingBar;
        }

        public MyC.PopupWindow popupw = null;
        MyC.PopupContent.SleepTimeSetContent SleepTimeSetContent = new MyC.PopupContent.SleepTimeSetContent();
        private void znButton_Click_5(object sender, RoutedEventArgs e)
        {
            if (popupw != null) return;

            popupw = new MyC.PopupWindow()
            {
                isWindowSmallRound = false,
                IsShowActivated = true,
                CloseExit = true,
                ForceAcrylicBlur = true,
                Content = SleepTimeSetContent,
                Owner = App.window,
                Topmost = false
            };

            App.window.LocationChanged += (s, args) =>
            {
                if (popupw != null) popupw.SetPositionWindowCenter(App.window);
            };

            App.window.SizeChanged += (s, args) =>
            {
                if (popupw != null)
                {
                    popupw.SetPositionWindowCenter(App.window);
                    popupw.Height = App.window.ActualHeight;
                    popupw.Width = 400;
                    popupw.MaxHeight = 400;
                }
            };
            App.window.BlurThemeChangeEvent += (a) =>
            {
                if (popupw != null) popupw.Background = a.BackColor;
            };

            popupw.Closed += (s, args) =>
            {
                popupw = null;
            };

            popupw.UserShow(-1000, -1000);
            popupw.Height = App.window.ActualHeight;
            popupw.Width = 400;
            popupw.MaxHeight = 400;
            popupw.SetPositionWindowCenter(App.window);

        }
        #endregion
    }
}
