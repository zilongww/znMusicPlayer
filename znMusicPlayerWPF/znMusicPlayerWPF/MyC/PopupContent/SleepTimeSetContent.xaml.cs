using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// SleepTimeSetContent.xaml 的交互逻辑
    /// </summary>
    public partial class SleepTimeSetContent : UserControl
    {
        public SleepTimeSetContent()
        {
            InitializeComponent();
            ThemeChangeAsync();
        }

        public async void ThemeChangeAsync()
        {
            await Task.Delay(10);
            App.window.BlurThemeChangeEvent += Window_BlurThemeChangeEvent;
            ThemeChanger(App.window.NowBlurThemeData);
        }

        private void Window_BlurThemeChangeEvent(MainWindow.BlurThemeData BlurThemeData)
        {
            ThemeChanger(BlurThemeData);
        }

        private void ThemeChanger(MainWindow.BlurThemeData BlurThemeData)
        {
            Foreground = BlurThemeData.TextColor;
            t1.Foreground = BlurThemeData.TextColor;
            t2.Foreground = BlurThemeData.TextColor;
            t3.Foreground = BlurThemeData.TextColor;
            t4.Foreground = BlurThemeData.TextColor;
            ocb1.Stroke = BlurThemeData.TextColor;
            ocb1.OCBackground = BlurThemeData.TextColor;
            ocb2.Stroke = BlurThemeData.TextColor;
            ocb2.OCBackground = BlurThemeData.TextColor;
            zncb1.Foreground = BlurThemeData.TextColor;
            zncb2.Foreground = BlurThemeData.TextColor;
            OkBtn.Background = BlurThemeData.ButtonBackColor;
            OkBtn.EnterColor = BlurThemeData.ButtonEnterColor;
            OkBtn.Foreground = BlurThemeData.InColorTextColor;
            OkBtn.BorderBrush = App.window.NowThemeData.ALineColor;
            CancelBtn.Background = BlurThemeData.ButtonBackColor;
            CancelBtn.EnterColor = BlurThemeData.ButtonEnterColor;
            CancelBtn.Foreground = BlurThemeData.InColorTextColor;
            CancelBtn.BorderBrush = App.window.NowThemeData.ALineColor;
        }

        private void zncb1_ContentClick(znComboBox znComboBox, object data)
        {
            znComboBox.Text = data.ToString();
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan timeSpan = TimeSpan.FromMinutes(15);
            switch (zncb1.Text)
            {
                case "30分钟":
                    timeSpan = TimeSpan.FromMinutes(30);
                    break;
                case "45分钟":
                    timeSpan = TimeSpan.FromMinutes(45);
                    break;
                case "1小时":
                    timeSpan = TimeSpan.FromHours(1);
                    break;
                case "2小时":
                    timeSpan = TimeSpan.FromHours(2);
                    break;
                case "3小时":
                    timeSpan = TimeSpan.FromHours(3);
                    break;
                case "4小时":
                    timeSpan = TimeSpan.FromHours(4);
                    break;
                case "5小时":
                    timeSpan = TimeSpan.FromHours(5);
                    break;
            }

            MainWindow.SleepTaskEndDos sleepTaskEndDo = MainWindow.SleepTaskEndDos.PauseMusic;
            switch (zncb2.Text)
            {
                case "退出程序":
                    sleepTaskEndDo = MainWindow.SleepTaskEndDos.ExitSoftware;
                    break;
                case "注销":
                    sleepTaskEndDo = MainWindow.SleepTaskEndDos.Logout;
                    break;
                case "关机":
                    sleepTaskEndDo = MainWindow.SleepTaskEndDos.ShutDown;
                    break;
            }

            App.window.SleepTaskStart(
                new MainWindow.SleepTask(timeSpan, sleepTaskEndDo, ocb2.IsChecked, ocb1.IsChecked));
            App.window.SettingPages.popupw.UserClose();
            App.window.Activate();
            App.window.ShowBox("睡眠定时", "定时已开始。");
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            App.window.SettingPages.popupw.UserClose();
            App.window.Activate();
        }

        private void ocb1_Checked(OCButton sender)
        {
            if (sender.IsChecked)
            {
                sp1.Visibility = Visibility.Visible;
                sp2.Visibility = Visibility.Visible;
                sp3.Visibility = Visibility.Visible;
            }
            else
            {
                sp1.Visibility = Visibility.Collapsed;
                sp2.Visibility = Visibility.Collapsed;
                sp3.Visibility = Visibility.Collapsed;
            }
        }
    }
}
