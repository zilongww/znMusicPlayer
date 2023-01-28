using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Composition;
using WinRT;
using Windows.Foundation.Metadata;
using Microsoft.Windows.AppNotifications;
using znMusicPlayerWUI.Helpers;

namespace znMusicPlayerWUI.Pages
{
    public partial class RecommendPage : Page
    {
        public RecommendPage()
        {
            InitializeComponent();
            VersionRun.Text = App.AppVersion;
        }

        //int a = 0;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                abcd.Source = App.playingList.NowPlayingList[new Random().Next(0, 100)].PicturePath;
            }
            catch { }
            //System.Diagnostics.Debug.WriteLine(a[0].ListName);
            //MainWindow.SetBackdrop(MainWindow.BackdropType.DesktopAcrylic);
            //await App.audioPlayer.Reload();
            /*
            if (a == 0)
            {
                MainWindow.SetBackdrop(MainWindow.BackdropType.Mica);
                a++;
            }
            else if (a == 1)
            {
                MainWindow.SetBackdrop(MainWindow.BackdropType.DesktopAcrylic);
                a++;
            }
            else if (a == 2)
            {
                MainWindow.SetBackdrop(MainWindow.BackdropType.DefaultColor);
                a = 0;
            }
            */
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
