using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Media;
using znMusicPlayerWUI.DataEditor;
using NAudio.Wave;

namespace znMusicPlayerWUI.Pages.DialogPages
{
    public partial class AudioInfoPage : Page
    {
        public AudioInfoPage()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            SetFileSourceInfoText();
            SetAudioInfoText();
        }

        private void SetFileSourceInfoText()
        {
            FileSourceInfoPathTB.Text = App.audioPlayer.FileReader.FileName;
            FileSourceInfoSizeTB.Text = CodeHelper.GetAutoSizeString(App.audioPlayer.FileSize, 2);
        }

        private void SetAudioInfoText()
        {
            AudioInfoTB.Text = $"{App.audioPlayer.FileType}  {App.audioPlayer.FileReader.WaveFormat.BitsPerSample}bit  {App.audioPlayer.FileReader.WaveFormat.SampleRate / (decimal)1000}kHz  {App.audioPlayer.AudioBitrate}kbps  {App.audioPlayer.FileReader.WaveFormat.Channels} 声道";
            AudioInfoABPSTB.Text = App.audioPlayer.FileReader.WaveFormat.AverageBytesPerSecond.ToString();
            AudioInfoBATB.Text = App.audioPlayer.FileReader.WaveFormat.BlockAlign.ToString();
            AudioInfoESTB.Text = App.audioPlayer.FileReader.WaveFormat.ExtraSize.ToString();
        }
    }
}
