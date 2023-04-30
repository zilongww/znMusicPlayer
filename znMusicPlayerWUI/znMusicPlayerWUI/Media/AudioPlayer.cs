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
using SoundTouch.Net;
using System.Net;
using NAudio.CoreAudioApi;
using Microsoft.UI.Xaml;
using System.Reflection.Metadata;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.DataEditor;
using Windows.Media;
using Windows.Storage.Pickers;
using Windows.Media.Playback;
using Windows.Media.Core;
using Microsoft.VisualBasic.Logging;
using System.Windows.Forms;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using static znMusicPlayerWUI.Media.AudioPlayer;
using static znMusicPlayerWUI.DataEditor.DataFolderBase;

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
                // WaveOut
                for (int n = -1; n < WaveOut.DeviceCount; n++)
                {
                    string name = WaveOut.GetCapabilities(n).ProductName;
                    OutDevice outDevice = new OutDevice(OutApi.WaveOut, n, name == "Microsoft 声音映射器" || name == "Microsoft Sound Mapper" ? defaultName : name);
                    outDevices.Add(outDevice);
                }
                if (outDevices.Count < 2) outDevices.Clear();

                // DirectSound
                foreach (var dev in DirectSoundOut.Devices)
                {
                    string name = dev.Description;
                    OutDevice outDevice = new OutDevice(OutApi.DirectSound, dev, name == "主声音驱动程序" ? defaultName : name);
                    outDevices.Add(outDevice);
                }
                if (outDevices.Count < 2) outDevices.Clear();

                // Wasapi
                var enumerator = new MMDeviceEnumerator();

                try
                {
                    // 添加默认设备
                    OutDevice outDevice1 = new OutDevice(OutApi.Wasapi, enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia).ID, defaultName);
                    outDevices.Add(outDevice1);
                }
                catch { }

                foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    OutDevice outDevice = new OutDevice(OutApi.Wasapi, wasapi.ID, wasapi.FriendlyName);
                    outDevices.Add(outDevice);
                }
                enumerator.Dispose();

                // Asio
                foreach (var asio in AsioOut.GetDriverNames())
                {
                    OutDevice outDevice = new OutDevice(OutApi.Asio, asio, asio);
                    outDevices.Add(outDevice);
                }

                if (!outDevices.Any())
                {
                    outDevices.Add(new(OutApi.None, null, "无音频输出设备"));
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
        public enum OutApi { WaveOut, DirectSound, Wasapi, Asio, None }
        public Media.AudioFileReader FileReader { get; set; } = null;
        public AudioEffects.SoundTouchWaveProvider FileProvider { get; set; } = null;
        public IWavePlayer NowOutObj { get; set; } = null;
        public MidiFile MidiFile { get; set; } = null;
        public OutputDevice MidiOutputDevice { get; set; } = OutputDevice.GetByIndex(0);
        public Playback MidiPlayback { get; set; } = null;
        public MusicData MusicData { get; set; }
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
                {
                    return FileReader.isMidi
                        ? TimeSpan.FromMilliseconds((MidiPlayback.GetCurrentTime(TimeSpanType.Metric) as MetricTimeSpan).TotalMilliseconds)
                        : FileReader.CurrentTime - TimeSpan.FromMilliseconds(Latency);
                }
                else return TimeSpan.Zero;
            }
            set
            {
                if (FileReader != null)
                {
                    if (FileReader.isMidi)
                    {
                        MidiPlayback.MoveToTime(new MetricTimeSpan(value.Hours, value.Minutes, value.Seconds, value.Milliseconds));
                    }
                    else
                    {
                        FileReader.CurrentTime = value;
                    }
                    TimingChanged?.Invoke(this);
                }
            }
        }
        
        public TimeSpan TotalTime
        {
            get
            {
                if (FileReader != null)
                {
                    return FileReader.isMidi ? TimeSpan.FromMilliseconds((MidiPlayback.GetDuration(TimeSpanType.Metric) as MetricTimeSpan).TotalMilliseconds) : FileReader.TotalTime;// - TimeSpan.FromMilliseconds(Latency);
                }
                else return TimeSpan.Zero;
            }
        }

        public PlaybackState PlaybackState
        {
            get
            {
                if (FileReader != null)
                {
                    if (FileReader.isMidi)
                    {
                        if (MidiPlayback.IsRunning)
                            return PlaybackState.Playing;
                        else return PlaybackState.Paused;
                    }
                    else
                    {
                        if (NowOutObj != null)
                            return NowOutObj.PlaybackState;
                        else return PlaybackState.Stopped;
                    }
                }
                return PlaybackState.Stopped;
            }
        }

        private OutDevice _nowOutDevice = new() { DeviceType = OutApi.WaveOut, Device = null, DeviceName = "默认输出设备" };
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
                if (value < 0f) value = 0;
                else if (value > 1f) value = 1f;
                _volume = value;
                VolumeChanged?.Invoke(this, value);
                if (FileReader != null)
                {
                    if (!FileReader.isMidi)
                    {
                        FileReader.Volume = value;
                    }
                }
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
                if (FileReader.isMidi)
                {
                    MidiPlayback.Speed = value;
                }
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

        MusicData PointMusicData = null;
        string PointFilePath = null;
        List<MusicData> LoadingMusicDatas = new();
        public async Task SetSource(MusicData musicData)
        {
            if (musicData == MusicData)
            {
                if (FileReader != null)
                {
                    CurrentTime = TimeSpan.Zero;
                }
                return;
            }

            if (LoadingMusicDatas.Contains(musicData))
            {
                //TODO:用户视觉反馈
                return;
                throw new Exception("音频正在缓存，请稍后。");
            }

            PointMusicData = musicData;
            LoadingMusicDatas.Add(musicData);
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
                        LoadingMusicDatas.Remove(musicData);
                        CacheLoadedChanged?.Invoke(this);
                        throw new WebException("网络未连接，请连接网络后重试。");
                    }

                    string a = await WebHelper.GetAudioAddressAsync(musicData);
                    if (a != null)
                    {
                        //Debug.WriteLine(a);
                        string b = @$"{AudioCacheFolder}\{musicData.From}{musicData.ID}";

                        await Task.Run(() => File.Create(b).Close());

                        try
                        {
                            await WebHelper.DownloadFileAsync(a, b);
                        }
                        catch (Exception err)
                        {
                            await Task.Run(() => File.Delete(b));
                            LoadingMusicDatas.Remove(musicData);
                            CacheLoadedChanged?.Invoke(this);
                            throw new FileLoadException($"加载缓存文件失败：{err.Message}");
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
                bool notExists = await Task.Run(() =>
                {
                    if (!File.Exists(resultPath)) return true;
                    else return false;
                });

                // 当文件不存在
                if (notExists)
                {
                    LoadingMusicDatas.Remove(musicData);
                    CacheLoadedChanged?.Invoke(this);
                    throw new FileLoadException($"找不到音频文件：\"{resultPath}\"，\n可能是文件已被删除或移动。");
                }


                // 检查文件是否没有下载完成
                bool notDownloaded = await Task.Run(() =>
                {
                    if (File.ReadAllBytes(resultPath).Length <= 10)
                    {
                        return true;
                    }
                    return false;
                });

                // 当文件没有下载完成
                if (notDownloaded)
                {
                    LoadingMusicDatas.Remove(musicData);
                    CacheLoadedChanged?.Invoke(this);
                    await Task.Run(() =>
                    {
                        if (File.Exists(resultPath))
                            File.Delete(resultPath);
                    });
                    throw new FileLoadException("缓存文件不完整，请重新加载。");
                }

                LoadingMusicDatas.Remove(musicData);
                if (PointMusicData == musicData)
                {
                    var m = MusicData;
                    MusicData = musicData;
                    Exception exception = null;
                    try
                    {
                        _filePath = resultPath;
                        await SetSource(resultPath);
                    }
                    catch (Exception err)
                    {
                        MusicData = m;
                        exception = err;
                        Debug.WriteLine(err);
                    }
                    finally
                    {
                        isLoadingLocal = false;
                        PlayStateChanged?.Invoke(this);
                        CacheLoadedChanged?.Invoke(this);
                        TimingChanged?.Invoke(this);
                        LoadingMusicDatas.Remove(musicData);
                    }

                    if (exception != null)
                    {
                        throw exception;
                    }
                }
            }
            else
            {
                CacheLoadedChanged?.Invoke(this);
                LoadingMusicDatas.Remove(musicData);
                throw new Exception("缓存文件获取失败，可能是源服务器没有相关资源。");
            }
        }

        string _filePath = null;
        bool isLoadingLocal = false;
        public string FileType = null;
        public int FileSize = 0;
        public string AudioBitrate = null;
        public async Task SetSource(string filePath)
        {
            if (filePath != _filePath) return;
            isLoadingLocal = true;

            if (NowOutDevice.DeviceType == OutApi.None)
            {
                NowOutDevice = (await OutDevice.GetOutDevices()).First();
                if (NowOutDevice.DeviceType == OutApi.None)
                {
                    throw new Exception("当前没有音频输出设备，请检查音频设置是否正确、输出设备是否插入和音频设备驱动是否正常工作。");
                }
            }

            AudioFileReader fileReader = null;
            AudioEffects.SoundTouchWaveProvider fileProvider = null;

            await Task.Run(() =>
            {
                fileReader = new AudioFileReader(filePath);

                if (fileReader.isMidi)
                {
                    WaveInfo = "midi";
                    return;
                }

                fileProvider = new AudioEffects.SoundTouchWaveProvider(fileReader);
                fileReader.EqEnabled = EqEnabled;
                fileReader.Volume = Volume;
                fileProvider.Pitch = Pitch;
                fileProvider.Tempo = Tempo;
                fileProvider.Rate = Rate;

                FileSize = File.ReadAllBytes(filePath).Length;

                TagLib.File tagfile = null;
                try
                { tagfile = TagLib.File.Create(filePath); }
                catch { }

                if (tagfile != null)
                {
                    FileType = tagfile.MimeType.Replace("taglib/", "");
                    AudioBitrate = tagfile.Properties.AudioBitrate.ToString();
                }
                else
                {
                    FileType = new FileInfo(filePath).Extension;
                    AudioBitrate = (FileSize * 8 / TotalTime.TotalSeconds / 1000).ToString();
                }
                WaveInfo = $"{fileReader.WaveFormat.SampleRate / (decimal)1000}kHz-{AudioBitrate}kbps-{FileType}";
            });
            if (EqEnabled)
            {
                EqualizerBand = EqualizerBand;
            }

            IsInReading = false;

            PreviewSourceChanged?.Invoke(this);
            if (filePath != FileReader?.FileName)
                SourceChanged?.Invoke(this);

            await Task.Run(() => DisposeAll());

            FileReader = fileReader;
            FileProvider = fileProvider;

            if (!FileReader.isMidi)
            {
                bool notDefaultLatency = false;
                if (Latency != (int)SettingDefault[SettingParams.AudioLatency.ToString()])
                {
                    notDefaultLatency = true;
                }

                switch (NowOutDevice.DeviceType)
                {
                    case OutApi.WaveOut:
                        NowOutObj = new WaveOutEvent();
                        (NowOutObj as WaveOutEvent).DeviceNumber = NowOutDevice.Device == null ? -1 : (int)NowOutDevice.Device;
                        if (notDefaultLatency) (NowOutObj as WaveOutEvent).NumberOfBuffers = Latency;
                        NowOutObj.Init(FileProvider);
                        NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                        break;
                    case OutApi.DirectSound:
                        if (NowOutDevice.Device == null)
                        {
                            if (notDefaultLatency)
                                NowOutObj = new DirectSoundOut(Latency);
                            else
                                NowOutObj = new DirectSoundOut();
                        }
                        else
                        {
                            if (notDefaultLatency)
                                NowOutObj = new DirectSoundOut((NowOutDevice.Device as DirectSoundDeviceInfo).Guid, Latency);
                            else
                                NowOutObj = new DirectSoundOut((NowOutDevice.Device as DirectSoundDeviceInfo).Guid);
                        }
                        NowOutObj.Init(FileProvider);
                        NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                        break;
                    case OutApi.Wasapi:
                        var device = new MMDeviceEnumerator().GetDevice(NowOutDevice.Device as string);
                        NowOutObj = new WasapiOut(
                            device,
                            WasapiOnly ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared, false,
                            Latency);
                        NowOutObj.Init(FileProvider);
                        NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                        device.Dispose();
                        break;
                    case OutApi.Asio:
                        NowOutObj = new AsioOut(NowOutDevice.Device.ToString());
                        //if (notDefaultLatency) (NowOutObj as AsioOut).PlaybackLatency = Latency;
                        NowOutObj.Init(FileProvider);
                        NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                        break;
                }
            }
            else
            {
                MidiFile = MidiFile.Read(filePath);
                MidiPlayback = MidiFile.GetPlayback(MidiOutputDevice);
                MidiPlayback.Finished += (_, __) => MainWindow.Invoke(() => AudioPlayer_PlaybackStopped(null, null));
                MidiPlayback.Speed = Tempo;
            }

            isLoadingLocal = false;
        }

        public async Task Reload()
        {
            if (FileReader != null)
            {
                if (FileReader.isMidi) return;
                TimeSpan nowPosition = FileReader.CurrentTime;
                var nowPlayState = NowOutObj?.PlaybackState;
                string filePath = FileReader.FileName;

                await Task.Run(() => DisposeAll());
                await SetSource(filePath);

                if (FileReader != null)
                {
                    FileReader.CurrentTime = nowPosition;
                }
                if (nowPlayState == PlaybackState.Playing) SetPlay();
                else SetPause();
            }
        }

        public async void SetReloadAsync()
        {
            try
            {
                await Reload();
            }
            catch (Exception err) { Debug.WriteLine(err.ToString()); }
        }

        [Obsolete(message:"不建议使用，性能较差")]
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
                    var a = CurrentTime + TimeSpan.FromSeconds(1.5);
                    if (a >= TotalTime)
                        PlayEnd?.Invoke(this);
                }
            }
        }

        public bool SetPlay()
        {
            NowOutObj?.Play();
            MidiPlayback?.Start();
            PlayStateChanged?.Invoke(this);
            timer.Start();
            App.SMTC.PlaybackStatus = MediaPlaybackStatus.Playing;
            return true;
        }
        
        public bool SetPause()
        {
            NowOutObj?.Pause();
            MidiPlayback?.Stop();
            PlayStateChanged?.Invoke(this);
            App.SMTC.PlaybackStatus = MediaPlaybackStatus.Paused;
            return true;
        }
        
        public bool SetStop()
        {
            NowOutObj?.Stop();
            MidiPlayback?.Stop();
            PlayStateChanged?.Invoke(this);
            App.SMTC.PlaybackStatus = MediaPlaybackStatus.Stopped;
            return true;
        }

        public void ReCallTiming()
        {
            if (PlaybackState == PlaybackState.Playing)
            {
                TimingChanged?.Invoke(this);
            }
            else
            {
                timer.Stop();
            }
        }

        bool isDisposing = false;
        public void DisposeAll()
        {
            isDisposing = true;

            try
            {
                (NowOutObj as IDisposable)?.Dispose();
            }
            finally
            {
                NowOutObj = null;
            }

            try
            {
                MidiFile = null;
                MidiPlayback?.Dispose();
            }
            finally
            {
                MidiPlayback = null;
            }

            try
            {
                FileReader?.Dispose();
            }
            finally
            {
                FileReader = null;
            }

            try
            {
                FileProvider?.Clear();
            }
            finally
            {
                FileProvider = null;
            }

            isDisposing = false;
        }
    }
}
