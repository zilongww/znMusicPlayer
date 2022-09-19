using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media;
using znMusicPlayerWUI.Media;
using znMusicPlayerWUI.DataEditor;
using Windows.UI;

namespace znMusicPlayerWUI.Controls
{
    public partial class DownloadCard : Grid
    {
        public DownloadManager.DownloadData downloadData { get; set; } = null;

        public DownloadCard(DownloadManager.DownloadData dm)
        {
            downloadData = dm;
            InitializeComponent();
            DataContext = this;
            TitleTb.Text = dm.MusicData.Title;
            ButtonNameTb.Text = dm.MusicData.ButtonName;
        }

        public void SetProgressValue(decimal value)
        {
            DownloadProgress.Foreground = App.Current.Resources["AccentAAFillColorDefaultBrush"] as SolidColorBrush;
            DownloadProgress.IsIndeterminate = false;
            DownloadProgress.Value = (double)value;
            MessageTb.Text = value.ToString() + "%";
        }

        public void SetProgressValue()
        {
            SetProgressValue(downloadData.DownloadPercent);
        }

        public void SetDownloadedPreview()
        {
            DownloadProgress.Foreground = App.Current.Resources["AccentAAFillColorDefaultBrush"] as SolidColorBrush;
            DownloadProgress.IsIndeterminate = false;
            DownloadProgress.Value = 100;
            MessageTb.Text = "即将完成";
        }
        
        public async void SetDownloaded()
        {
            DownloadProgress.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 200, 0));
            DownloadProgress.IsIndeterminate = false;
            DownloadProgress.Value = 100;
            await Task.Delay(10);
            MessageTb.Text = "下载完成";
        }
        
        public async void SetError()
        {
            DownloadProgress.Foreground = new SolidColorBrush(Color.FromArgb(255, 200, 0, 0));
            DownloadProgress.IsIndeterminate = false;
            DownloadProgress.Value = 0;
            await Task.Delay(10);
            MessageTb.Text = "下载错误";
        }

        private void UILoaded(object sender, RoutedEventArgs e)
        {
        }

        private void UIUnloaded(object sender, RoutedEventArgs e)
        {
        }

        private void Grid_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as UIElement).PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
            }
        }

        private void Grid_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(sender as UIElement).PointerDeviceType == Microsoft.UI.Input.PointerDeviceType.Mouse)
            {
            }
        }

        bool isPressed = false;
        private void Grid_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            isPressed = true;
        }

        private void Grid_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isPressed)
            {
            }
            isPressed = false;
        }
    }
}
