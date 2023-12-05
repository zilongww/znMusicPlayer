﻿using Microsoft.UI;
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
using Windows.UI;
using znMusicPlayerWUI.Background;

namespace znMusicPlayerWUI.Controls
{
    public partial class DownloadCard : Grid
    {
        public DownloadData downloadData { get; set; } = null;

        public DownloadCard()
        {
            InitializeComponent();
            DataContextChanged += DownloadCard_DataContextChanged;
            App.downloadManager.OnDownloading += DownloadManager_OnDownloading;
            App.downloadManager.OnDownloadedPreview += DownloadManager_OnDownloadedPreview;
            App.downloadManager.OnDownloaded += DownloadManager_OnDownloaded;
            App.downloadManager.OnDownloadError += DownloadManager_OnDownloadError;
        }

        private void DownloadManager_OnDownloaded(DownloadData data)
        {
            if (data != downloadData) return;
            SetDownloaded();
        }

        private void DownloadManager_OnDownloadedPreview(DownloadData data)
        {
            if (data != downloadData) return;
            SetDownloadedPreview();
        }

        private void DownloadManager_OnDownloading(DownloadData data)
        {
            if (data != downloadData) return;
            SetProgressValue(data.DownloadPercent);
        }
        private void DownloadManager_OnDownloadError(DownloadData data)
        {
            if (data != downloadData) return;
            SetError();
        }


        private void DownloadCard_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            downloadData = DataContext as DownloadData;
            if (downloadData is null) return;
            TitleTb.Text = downloadData.MusicData.Title;
            ButtonNameTb.Text = downloadData.MusicData.ButtonName;
            switch (downloadData.DownloadState)
            {
                case DownloadStates.Downloading:
                    SetProgressValue();
                    break;
                case DownloadStates.DownloadedPreview:
                    SetDownloadedPreview();
                    break;
                case DownloadStates.Downloaded:
                    SetDownloaded();
                    break;
                case DownloadStates.Error:
                    SetError();
                    break;
                default:
                    SetWaiting();
                    break;
            }
        }

        public void SetWaiting()
        {
            DownloadProgress.IsIndeterminate = true;
            CompletedBackgroundBase.Fill = null;
            CoverRectangle.Rect = new();
            DownloadProgress.Value = 0;
            MessageTb.Text = "正在等待";
            FontIconBase.Glyph = "";
        }

        public void SetProgressValue(decimal value)
        {
            DownloadProgress.IsIndeterminate = false;
            CompletedBackgroundBase.Fill = App.Current.Resources["AccentAAFillColorDefaultBrush"] as SolidColorBrush;
            CoverRectangle.Rect = new(0, 0, CompletedBackgroundBase.ActualWidth * ((double)value / 100), CompletedBackgroundBase.ActualHeight);
            DownloadProgress.Value = (double)value;
            MessageTb.Text = $"{CodeHelper.GetAutoSizeString(downloadData.DownloadedSize, 2)}/{CodeHelper.GetAutoSizeString(downloadData.FileSize, 2)} | {value}%";
            FontIconBase.Glyph = "";
        }

        public void SetProgressValue()
        {
            SetProgressValue(downloadData.DownloadPercent);
        }

        public void SetDownloadedPreview()
        {
            CompletedBackgroundBase.Fill = App.Current.Resources["AccentAAFillColorDefaultBrush"] as SolidColorBrush;
            DownloadProgress.IsIndeterminate = false;
            DownloadProgress.Value = 100;
            MessageTb.Text = "即将完成";
            FontIconBase.Glyph = "";
        }
        
        public async void SetDownloaded()
        {
            CompletedBackgroundBase.Fill = App.Current.Resources["SystemFillColorSuccessBrush"] as SolidColorBrush;
            DownloadProgress.IsIndeterminate = false;
            CoverRectangle.Rect = new(0, 0, CompletedBackgroundBase.ActualWidth, CompletedBackgroundBase.ActualHeight);
            DownloadProgress.Value = 100;
            await Task.Delay(10);
            MessageTb.Text = "下载完成";
            FontIconBase.Glyph = "\uE73E";
        }
        
        public async void SetError()
        {
            CompletedBackgroundBase.Fill = App.Current.Resources["SystemFillColorCriticalBrush"] as SolidColorBrush;
            DownloadProgress.IsIndeterminate = false;
            CoverRectangle.Rect = new(0, 0, CompletedBackgroundBase.ActualWidth, CompletedBackgroundBase.ActualHeight);
            DownloadProgress.Value = 100;
            await Task.Delay(10);
            MessageTb.Text = "下载错误";
            FontIconBase.Glyph = "\uE711";
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

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (downloadData is null) return;
            switch (downloadData.DownloadState)
            {
                case DownloadStates.Downloading:
                    SetProgressValue();
                    break;
                case DownloadStates.DownloadedPreview:
                    SetDownloadedPreview();
                    break;
                case DownloadStates.Downloaded:
                    SetDownloaded();
                    break;
                case DownloadStates.Error:
                    SetError();
                    break;
                default:
                    SetWaiting();
                    break;
            }
        }
    }
}
