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
using znMusicPlayerWUI.Media;

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
            SetOutInfoText();
        }

        private void SetFileSourceInfoText()
        {
            FileSourceInfoPathTB.Text = App.audioPlayer.FileReader.FileName;
            FileSourceInfoSizeTB.Text = CodeHelper.GetAutoSizeString(App.audioPlayer.FileSize, 2);
        }

        private async void SetAudioInfoText()
        {
            if (App.audioPlayer.FileReader != null)
            {
                if (App.audioPlayer.FileReader.isMidi)
                {
                    AudioInfoGrid.Visibility = Visibility.Collapsed;
                    return;
                }
            }

            ATL.Track tfile = null;
            if (App.audioPlayer.tfile == null)
            {
                await Task.Run(() =>
                {
                    tfile = new ATL.Track(App.audioPlayer.FileReader.FileName);
                });
            }
            else
            {
                tfile = App.audioPlayer.tfile;
            }

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
                    ((TextBlock)AudioInfoSp.Children[3]).Text = $"{tfile.BitDepth} bit";

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

        private async void SetOutInfoText()
        {
            if (App.audioPlayer.FileReader.isMidi)
            {
                ((TextBlock)OutInfoSp.Children[2]).Text = $"Midi -> {App.audioPlayer.MidiOutputDevice.Name}";
                ((TextBlock)OutInfoSp.Children[3]).Visibility = Visibility.Collapsed;
                ((TextBlock)OutInfoSp.Children[4]).Visibility = Visibility.Collapsed;
                ((TextBlock)OutInfoSp.Children[5]).Visibility = Visibility.Collapsed;
                ((TextBlock)OutInfoSp.Children[6]).Visibility = Visibility.Collapsed;
                ((TextBlock)OutInfoSp.Children[7]).Visibility = Visibility.Collapsed;
                ((TextBlock)OutInfoSp.Children[8]).Visibility = Visibility.Collapsed;
            }
            else
            {
                string SampleRateText = "未知";
                var getd = await OutDevice.GetWasapiDeviceFromOtherAPI(App.audioPlayer.NowOutDevice);
                if (App.audioPlayer.WasapiOnly && App.audioPlayer.NowOutDevice.DeviceType == AudioPlayer.OutApi.Wasapi)
                {
                    if (App.audioPlayer.NowOutObj.OutputWaveFormat.SampleRate != App.audioPlayer.FileReader.WaveFormat.SampleRate)
                        SampleRateText = $"{App.audioPlayer.FileReader.WaveFormat.SampleRate} Hz -> {App.audioPlayer.NowOutObj.OutputWaveFormat.SampleRate} Hz（重采样）";
                    else
                        SampleRateText = $"{App.audioPlayer.NowOutObj.OutputWaveFormat.SampleRate} Hz";
                }
                else
                {
                    if (getd.SampleRate != App.audioPlayer.FileReader.WaveFormat.SampleRate)
                        SampleRateText = $"{App.audioPlayer.FileReader.WaveFormat.SampleRate} Hz -> {getd.SampleRate} Hz（重采样）";
                    else
                        SampleRateText = $"{App.audioPlayer.NowOutObj.OutputWaveFormat.SampleRate} Hz（SRC）";

                }

                string channelsText = null;
                if (App.audioPlayer.FileReader.WaveFormat.Channels != getd.Channels)
                {
                    channelsText = $"{App.audioPlayer.FileReader.WaveFormat.Channels} 声道 -> {getd.Channels} 声道";
                }
                else
                {
                    channelsText = $"{App.audioPlayer.FileReader.WaveFormat.Channels} 声道";
                }

                ((TextBlock)OutInfoSp.Children[2]).Text = $"{App.audioPlayer.NowOutDevice.DeviceType} -> {App.audioPlayer.NowOutDevice.DeviceName}";
                ((TextBlock)OutInfoSp.Children[4]).Text = SampleRateText;
                ((TextBlock)OutInfoSp.Children[6]).Text = channelsText;
                ((TextBlock)OutInfoSp.Children[8]).Text = $"{App.audioPlayer.Latency} ms";
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
