using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Controls
{
    public sealed partial class LoadingTip : Grid
    {
        public LoadingTip()
        {
            InitializeComponent();
        }

        public void ShowLoading()
        {
            LoadingRingBaseGrid.Visibility = Visibility.Visible;
            LoadingRingBaseGrid.Opacity = 1;
            LoadingRing.IsIndeterminate = true;
        }

        public async void UnShowLoading()
        {
            LoadingRingBaseGrid.Opacity = 0;
            await Task.Delay(500);
            LoadingRing.IsIndeterminate = false;
            LoadingRingBaseGrid.Visibility = Visibility.Collapsed;
        }

    }
}
