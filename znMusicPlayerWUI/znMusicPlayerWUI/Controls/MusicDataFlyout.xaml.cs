using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using znMusicPlayerWUI.Helpers;

namespace znMusicPlayerWUI.Controls
{
    public sealed partial class MusicDataFlyout : UserControl
    {
        public ArrayList arrayList { get; set; }
        SongItemBindBase songItemBind = null;
        public SongItemBindBase SongItemBind
        {
            get => songItemBind;
            set
            {
                songItemBind = value;
                Init();
            }
        }

        public MusicDataFlyout()
        {
            InitializeComponent();
            //arrayList = new ArrayList(100000000);
        }

        void Init()
        {
            TitleTextblock.Text = SongItemBind.MusicData.Title;
        }

        public void ShowAt(FrameworkElement element)
        {
            root.ShowAt(element);
        }
        
        public void ShowAt(UIElement element, Point point)
        {
            root.ShowAt(element, point);
        }
        
        public void ShowAt(DependencyObject element, FlyoutShowOptions flyoutShowOptions)
        {
            root.ShowAt(element, flyoutShowOptions);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var menuFlyoutItem = sender as MenuFlyoutItem;
            switch (menuFlyoutItem.Tag as string)
            {
                case "play":
                    break;
            }
        }

        private void AddToPlayListSubItems_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
