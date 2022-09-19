using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using Microsoft.UI.Xaml.Media;
using znMusicPlayerWUI.DataEditor;
using NAudio.Wave;

namespace znMusicPlayerWUI.Controls
{
    public partial class MediaPlayStateViewer : Grid
    {
        private PlaybackState playbackState = default;
        public PlaybackState PlaybackState
        {
            get
            {
                return playbackState;
            }
            set
            {
                playbackState = value;
                if (value == PlaybackState.Playing)
                {
                    PlayIcon.Visibility = Visibility.Collapsed;
                    PauseIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    PlayIcon.Visibility = Visibility.Visible;
                    PauseIcon.Visibility = Visibility.Collapsed;
                }
            }
        }

        public MediaPlayStateViewer()
        {
            InitializeComponent();
        }
    }
}
