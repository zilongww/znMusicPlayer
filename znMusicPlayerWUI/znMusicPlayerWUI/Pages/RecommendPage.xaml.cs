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
using System.Diagnostics;
using ATL.CatalogDataReaders;
using znMusicPlayerWUI.DataEditor;

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
            //await App.playingList.Play(App.playListReader.NowMusicListDatas[0].Songs[0]);

            //CueSharp.CueSheet cueSheet = new CueSharp.CueSheet("E:\\vedio\\anime\\[170816] TVアニメ「Fate／Apocrypha」OPテーマ「英雄 運命の詩」／EGOIST [通常盤] [FLAC+CUE]\\VVCL-1080.cue");

            //GC.Collect();
            try
            {
                abcd.Source = App.playingList.NowPlayingList[new Random().Next(0, App.playingList.NowPlayingList.Count - 1)].Album.PicturePath;
            }
            catch { }
            return;
            //App.audioPlayerBass.LoadAudio();
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
