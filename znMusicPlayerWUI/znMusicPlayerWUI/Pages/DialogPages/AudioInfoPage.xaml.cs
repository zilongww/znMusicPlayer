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
using ATL.AudioData;

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

        private async void SetAudioInfoText()
        {
            ATL.Track tfile = null;
            await Task.Run(() =>
            {
                tfile = new ATL.Track(App.audioPlayer.FileReader.FileName);
            });

            if (tfile != null)
            {
                ((TextBlock)AudioInfoSp.Children[1]).Text = $"{App.audioPlayer.FileType}" +
                    $"{(ConvertCodecFamilyIntToString(tfile.CodecFamily) == null ? "" : $"  {ConvertCodecFamilyIntToString(tfile.CodecFamily)}")}" +
                    $"  {tfile.SampleRate} Hz  {tfile.Bitrate} kbps" +
                    $"  {tfile.ChannelsArrangement.NbChannels} 声道  {App.audioPlayer.FileReader.TotalTime.ToString("hh\\:mm\\:ss\\.f")}({tfile.Duration}s)";

                if (tfile.BitDepth == -1)
                {
                    AudioInfoSp.Children[2].Visibility = Visibility.Collapsed;
                    AudioInfoSp.Children[3].Visibility = Visibility.Collapsed;
                }
                else
                    ((TextBlock)AudioInfoSp.Children[3]).Text = tfile.BitDepth.ToString();

                //((TextBlock)AudioInfoSp.Children[5]).Text = App.audioPlayer.FileReader.WaveFormat.AverageBytesPerSecond.ToString();
                //((TextBlock)AudioInfoSp.Children[7]).Text = App.audioPlayer.FileReader.WaveFormat.BlockAlign.ToString();
                //((TextBlock)AudioInfoSp.Children[7]).Text = App.audioPlayer.FileReader.WaveFormat.ExtraSize.ToString();

                if (tfile.AdditionalFields.ContainsKey("VORBIS-VENDOR"))
                {
                    ((TextBlock)AudioInfoSp.Children[5]).Text = tfile.AdditionalFields["VORBIS-VENDOR"];
                }
                else
                {
                    AudioInfoSp.Children[4].Visibility = Visibility.Collapsed;
                    AudioInfoSp.Children[5].Visibility = Visibility.Collapsed;
                }

                string additionalFields = "";
                for (int i = 0; i < tfile.AdditionalFields.Count; i++)
                {
                    var element = tfile.AdditionalFields.ElementAt(i);
                    if (element.Key == "VORBIS-VENDOR") continue;
                    additionalFields += $"● {element.Key}: {element.Value}{(i == tfile.AdditionalFields.Count -1 ? "" : "\n")}";
                }
                ((TextBlock)AudioInfoSp.Children[7]).Text = string.IsNullOrEmpty(additionalFields) ? "无内容" : additionalFields;

                //((TextBlock)AudioInfoSp.Children[8]).Text = "";
            }
            else
            {

            }
        }

        private string ConvertCodecFamilyIntToString(int codecFamily)
        {
            switch (codecFamily)
            {
                case 0:
                    return "Lossy";
                case 1:
                    return "Lossless";
                default:
                    return null;
            }
        }
    }
}
