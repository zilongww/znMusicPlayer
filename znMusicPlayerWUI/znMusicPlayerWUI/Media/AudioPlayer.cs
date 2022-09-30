using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NAudio;
using NAudio.Wave;
using NAudio.Dsp;
using NAudio.Midi;
using SoundTouch.Net;
using System.Net;
using NAudio.CoreAudioApi;
using static znMusicPlayerWUI.Media.AudioPlayer;
using Microsoft.UI.Xaml;
using System.Reflection.Metadata;
using znMusicPlayerWUI.Helpers;

namespace znMusicPlayerWUI.Media
{
    public struct OutDevice
    {
        public OutApi DeviceType { get; set; }
        public object Device { get; set; }
        public string DeviceName { get; set; }
        public OutDevice(OutApi deviceType, object device = null, string deviceName = "") : this()
        {
            DeviceType = deviceType;
            Device = device;
            DeviceName = deviceName;
        }
        public override string ToString()
        {
            return $"{DeviceType} - {DeviceName}";
        }

        /// <summary>
        /// 获取可以播放的音频输出设备列表
        /// </summary>
        /// <returns>OutDevice集合</returns>
        public static async Task<List<OutDevice>> GetOutDevices()
        {
            List<OutDevice> outDevices = new List<OutDevice>();
            string defaultName = "默认输出设备";

            await Task.Run(() =>
            {
                // Wasapi
                var enumerator = new MMDeviceEnumerator();
                // 添加默认设备
                OutDevice outDevice1 = new OutDevice(OutApi.Wasapi, enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID, defaultName);
                outDevices.Add(outDevice1);

                foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    OutDevice outDevice = new OutDevice(OutApi.Wasapi, wasapi.ID, wasapi.FriendlyName);
                    outDevices.Add(outDevice);
                }
                enumerator.Dispose();

                // WaveOut
                for (int n = -1; n < WaveOut.DeviceCount; n++)
                {
                    string name = WaveOut.GetCapabilities(n).ProductName;
                    OutDevice outDevice = new OutDevice(OutApi.WaveOut, n, name == "Microsoft 声音映射器" || name == "Microsoft Sound Mapper" ? defaultName : name);
                    outDevices.Add(outDevice);
                }

                // DirectSound
                foreach (var dev in DirectSoundOut.Devices)
                {
                    string name = dev.Description;
                    OutDevice outDevice = new OutDevice(OutApi.DirectSound, dev, name == "主声音驱动程序" ? defaultName : name);
                    outDevices.Add(outDevice);
                }

                // Asio
                foreach (var asio in AsioOut.GetDriverNames())
                {
                    OutDevice outDevice = new OutDevice(OutApi.Asio, asio, asio);
                    outDevices.Add(outDevice);
                }
            });

            return outDevices;
        }
    }

    public class AudioPlayer
    {
        public delegate void AudioPlayerDelegate(AudioPlayer audioPlayer);
        public delegate void AudioPlayerDataDelegate(AudioPlayer audioPlayer, object data);
        public event AudioPlayerDelegate PlayEnd;
        public event AudioPlayerDelegate SourceChanged;
        public event AudioPlayerDelegate PreviewSourceChanged;
        public event AudioPlayerDelegate TimingChanged;
        public event AudioPlayerDelegate PlayStateChanged;
        public event AudioPlayerDataDelegate VolumeChanged;
        public event AudioPlayerDataDelegate CacheLoadingChanged;
        public event AudioPlayerDelegate CacheLoadedChanged;
        public event AudioPlayerDelegate EqualizerBandChanged;

        DispatcherTimer timer;
        public enum OutApi { WaveOut, DirectSound, Wasapi, Asio }
        public Media.AudioFileReader FileReader { get; set; } = null;
        public AudioEffects.SoundTouchWaveProvider FileProvider { get; set; } = null;
        public IWavePlayer NowOutObj { get; set; } = null;
        public DataEditor.MusicData MusicData { get; set; }
        public bool IsReloadErrorFile { get; set; }

        public string NameOfBand { get; set; }
        public string NameOfBandCH { get; set; }
        private List<float[]> _equalizerBand = AudioEqualizerBands.NormalBands;
        public List<float[]> EqualizerBand
        {
            get
            {
                return _equalizerBand;
            }
            set
            {
                if (value != null)
                {
                    _equalizerBand = value;
                    if (FileReader != null)
                    {
                        for (int i = 0; i < value.Count - 1; i++)
                        {
                            AudioEqualizerBands.NormalBands[i][2] = value[i][2];
                        }
                        FileReader.CreateFilters();
                    }
                    EqualizerBandChanged?.Invoke(this);
                }
            }
        }

        private bool _eqEnalbed = false;
        public bool EqEnabled
        {
            get
            {
                return _eqEnalbed;
            }
            set
            {
                _eqEnalbed = value;
                if (FileReader != null) FileReader.EqEnabled = value;
            }
        }
        
        private bool _wasapiOnly = false;
        public bool WasapiOnly
        {
            get
            {
                return _wasapiOnly;
            }
            set
            {
                _wasapiOnly = value;
                if (NowOutObj == null) return;
                if (NowOutObj.GetType() == typeof(WasapiOut))
                {
                    SetReloadAsync();
                }
            }
        }

        private int _latency = 50;
        public int Latency
        {
            get { return _latency; }
            set
            {
                _latency = value;
                SetReloadAsync();
            }
        }

        public TimeSpan CurrentTime
        {
            get
            {
                if (FileReader != null)
                    return FileReader.CurrentTime;// - TimeSpan.FromMilliseconds(Latency);
                else return TimeSpan.Zero;
            }
            set
            {
                FileReader.CurrentTime = value;
            }
        }

        private OutDevice _nowOutDevice = new() { DeviceType = OutApi.Wasapi, Device = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID, DeviceName = "默认输出设备" };
        public OutDevice NowOutDevice
        {
            get => _nowOutDevice;
            set
            {
                _nowOutDevice = value;
            }
        }

        private float _volume = 0f;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                VolumeChanged?.Invoke(this, value);
                if (FileReader != null) FileReader.Volume = value;
            }
        }

        /// <summary>
        /// 声调
        /// </summary>
        private double _pitch = 1f;
        public double Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                if (FileProvider != null) FileProvider.Pitch = value;
            }
        }
        
        /// <summary>
        /// 速度
        /// </summary>
        private double _tempo = 1f;
        public double Tempo
        {
            get => _tempo;
            set
            {
                _tempo = value;
                if (FileProvider != null) FileProvider.Tempo = value;
            }
        }
        
        /// <summary>
        /// 流百分比
        /// </summary>
        private double _rate = 1f;
        public double Rate
        {
            get => _rate;
            set
            {
                _rate = value;
                if (FileProvider != null) FileProvider.Rate = value;
            }
        }

        public string WaveInfo { get; set; } = "";

        public bool IsInReading = false;

        public AudioPlayer()
        {
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };
            timer.Tick += (_, __) => ReCallTiming();
        }

        public async Task<bool> SetSource(DataEditor.MusicData musicData)
        {
            if (musicData == MusicData)
            {
                if (FileReader != null)
                {
                    FileReader.CurrentTime = TimeSpan.Zero;
                }
                return true;
            }

            CacheLoadingChanged?.Invoke(this, 0);

            // 获取音频缓存路径
            string cachePath = await FileHelper.GetAudioCache(musicData);
            // 最终播放文件路径
            string resultPath = null;

            if (musicData.InLocal != null)
            {
                resultPath = musicData.InLocal;
            }
            else
            {
                if (cachePath == null) // 未查询到音频缓存路径，下载音频到音频文件夹
                {
                    if (!WebHelper.IsNetworkConnected)
                    {
                        await MainWindow.ShowDialog("网络未连接", "请连接网络后再试。");
                        CacheLoadedChanged?.Invoke(this);
                        return false;
                    }

                    string a = await WebHelper.GetAudioAddressAsync(musicData);
                    if (a != null)
                    {
                        //Debug.WriteLine(a);
                        string b = @$"{DataEditor.DataFolderBase.AudioCacheFolder}\{musicData.From}{musicData.ID}";

                        await Task.Run(() => File.Create(b).Close());

                        try
                        {
                            await WebHelper.DownloadFileAsync(a, b);
                        }
                        catch (Exception err)
                        {
                            await Task.Run(() => File.Delete(b));
                            await MainWindow.ShowDialog("加载缓存文件失败", err.Message);
                            CacheLoadedChanged?.Invoke(this);
                            return false;
                        }

                        resultPath = b;
                    }
                }
                else
                {
                    resultPath = cachePath;
                }
            }
            //Debug.WriteLine(resultPath);

            if (resultPath != null)
            {
                // 检查文件是否没有下载完成
                bool downloaded = await Task.Run(() =>
                {
                    try
                    {
                        if (File.ReadAllBytes(resultPath).Length <= 10)
                        {
                            return true;
                        }
                    }
                    catch (Exception err)
                    {
                        Debug.WriteLine(err.ToString());
                        return true;
                    }
                    return false;
                });

                // 当文件没有下载完成
                if (downloaded)
                {
                    await MainWindow.ShowDialog("播放缓存失败", "此缓存文件无法播放。");
                    CacheLoadedChanged?.Invoke(this);
                    await Task.Run(() =>
                    {
                        if (File.Exists(resultPath))
                            File.Delete(resultPath);
                    });
                    return false;
                }
            
                var m = MusicData;
                MusicData = musicData;
                var loaded = await SetSource(resultPath);
                if (!loaded)
                {
                    MusicData = m;
                }
                CacheLoadedChanged?.Invoke(this);
                return loaded;
            }
            else
            {
                await MainWindow.ShowDialog("播放缓存失败", "播放缓存文件时出现未知错误。");
                CacheLoadedChanged?.Invoke(this);
                return false;
            }
        }

        public async Task<bool> SetSource(string filePath)
        {
            if (IsInReading) return false;

            IsInReading = true;
            AudioFileReader fileReader = null;
            AudioEffects.SoundTouchWaveProvider fileProvider = null;
            try
            {
                await Task.Run(() =>
                {
                    fileReader = new AudioFileReader(filePath);
                    fileProvider = new AudioEffects.SoundTouchWaveProvider(fileReader);
                    fileReader.EqEnabled = EqEnabled;
                    fileReader.Volume = Volume;
                    fileProvider.Pitch = Pitch;
                    fileProvider.Tempo = Tempo;
                    fileProvider.Rate = Rate;

                    try
                    {
                        var tagfile = TagLib.File.Create(filePath);
                        if (!tagfile.Tag.IsEmpty)
                        {
                            WaveInfo = $"{tagfile.Properties.Codecs.First().Description} - {fileReader.WaveFormat.SampleRate / (decimal)1000}kHz-{tagfile.Properties.AudioBitrate}kbps";
                        }
                    }
                    catch
                    {
                        WaveInfo = $"{fileReader.WaveFormat.AsStandardWaveFormat().Encoding} - {fileReader.WaveFormat.SampleRate / (decimal)1000}kHz-{(int)(File.ReadAllBytes(filePath).Length * 8 / fileReader.TotalTime.TotalSeconds / 1000)}kbps";
                    }
                });
                if (EqEnabled)
                {
                    EqualizerBand = EqualizerBand;
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine(err.ToString());
                await MainWindow.ShowDialog("播放错误", $"播放文件时出现错误：\n\r{err.Message}");
                IsInReading = false;
                CacheLoadedChanged?.Invoke(this);
                return false;
            }
            IsInReading = false;

            PreviewSourceChanged?.Invoke(this);
            if (filePath != FileReader?.FileName)
                SourceChanged?.Invoke(this);

            DisposeAll();

            FileReader = fileReader;
            FileProvider = fileProvider;

            switch (NowOutDevice.DeviceType)
            {
                case OutApi.WaveOut:
                    NowOutObj = new WaveOutEvent();
                    (NowOutObj as WaveOutEvent).DeviceNumber = NowOutDevice.Device == null ? -1 : (int)NowOutDevice.Device;
                    (NowOutObj as WaveOutEvent).NumberOfBuffers = Latency;
                    NowOutObj.Init(FileProvider);
                    NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                    break;
                case OutApi.DirectSound:
                    if (NowOutDevice.Device == null)
                        NowOutObj = new DirectSoundOut(Latency);
                    else
                        NowOutObj = new DirectSoundOut((NowOutDevice.Device as DirectSoundDeviceInfo).Guid, Latency);
                    NowOutObj.Init(FileProvider);
                    NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                    break;
                case OutApi.Wasapi:
                    var a = new MMDeviceEnumerator().GetDevice(NowOutDevice.Device as string);
                    NowOutObj = new WasapiOut(
                        a,
                        WasapiOnly ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared, false,
                        Latency);
                    NowOutObj.Init(FileProvider);
                    NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                    a.Dispose();
                    break;
                case OutApi.Asio:
                    NowOutObj = new AsioOut(NowOutDevice.Device.ToString());
                    NowOutObj.Init(FileProvider);
                    NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                    break;
            }

            return true;
        }

        string[] ReloadErrorExtension = new string[] { ".flac" };
        public async Task<bool> Reload()
        {
            bool result = false;
            if (FileReader != null)
            {
                TimeSpan nowPosition = FileReader.CurrentTime;
                var nowPlayState = NowOutObj.PlaybackState;
                string filePath = FileReader.FileName;
                nowPosition = FileReader.CurrentTime;

                DisposeAll();
                result = await SetSource(filePath);

                if (IsReloadErrorFile)
                {
                    SetPlay();
                    await Task.Delay(1);
                    FileReader.CurrentTime = nowPosition;
                }
                else
                {
                    FileReader.CurrentTime = nowPosition;
                }
                if (nowPlayState == PlaybackState.Playing) SetPlay();
                else SetPause();
            }

            return result;
        }

        public async void SetReloadAsync()
        {
            try
            {
                await Reload();
            }
            catch (Exception err) { Debug.WriteLine(err.ToString()); }
        }

        [Obsolete]
        public void SetEqualizer(int position, float db)
        {
            EqualizerBand[position][2] = db;
            if (FileReader != null)
                FileReader.CreateFilters();
        }

        private void AudioPlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (FileReader != null)
            {
                if (FileReader.IsDisposed)
                    PlayEnd?.Invoke(this);
                else
                {
                    var a = FileReader.CurrentTime + TimeSpan.FromSeconds(1.5);
                    if (a >= FileReader.TotalTime)
                        PlayEnd?.Invoke(this);
                }
            }
        }

        public bool SetPlay()
        {
            NowOutObj?.Play();
            PlayStateChanged?.Invoke(this);
            timer.Start();
            return true;
        }
        
        public bool SetPause()
        {
            NowOutObj?.Pause();
            PlayStateChanged?.Invoke(this);
            return true;
        }
        
        public bool SetStop()
        {
            NowOutObj?.Stop();
            PlayStateChanged?.Invoke(this);
            return true;
        }

        public void ReCallTiming()
        {
            if (NowOutObj?.PlaybackState == PlaybackState.Playing)
            {
                TimingChanged?.Invoke(this);
            }
            else
            {
                timer.Stop();
            }
        }

        public void DisposeAll()
        {
            SetStop();
            (NowOutObj as IDisposable)?.Dispose();
            NowOutObj = null;
            FileReader?.Dispose();
            FileReader = null;
            FileProvider?.Clear();
            FileProvider = null;
        }
    }
}
