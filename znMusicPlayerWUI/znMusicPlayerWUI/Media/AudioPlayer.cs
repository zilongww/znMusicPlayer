﻿using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using TewIMP.DataEditor;
using static TewIMP.Media.AudioPlayer;

namespace TewIMP.Media
{
    public class OutDevice : OnlyClass
    {
        public OutApi DeviceType { get; set; }
        public object Device { get; set; }
        public string DeviceName { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public long Latency { get; set; }
        public bool IsDefaultDevice { get; set; } = false;
        public OutDevice(OutApi deviceType, object device = null, string deviceName = "")
        {
            DeviceType = deviceType;
            Device = device;
            DeviceName = deviceName;
        }

        public override string ToString()
        {
            if (DeviceType == OutApi.None)
            {
                return "无音频输出设备";
            }
            return $"{DeviceType} - {(IsDefaultDevice ? defaultName : DeviceName)}";
        }

        public override string GetMD5()
        {
            return ToString();
        }

        public static OutDevice GetWasapiDefaultDevice(MMDeviceEnumerator enumerator)
        {
            var dout = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            var od = new OutDevice(OutApi.Wasapi, dout.ID, dout.FriendlyName) { IsDefaultDevice = true };
            od.SampleRate = dout.AudioClient.MixFormat.SampleRate;
            od.Channels = dout.AudioClient.MixFormat.Channels;
            return od;
        }

        public static OutDevice GetWasapiDefaultDevice()
        {
            var enumerator = new MMDeviceEnumerator();
            var result = GetWasapiDefaultDevice(enumerator);
            enumerator.Dispose();
            return result;
        }

        public static string defaultName = "默认输出设备";
        /// <summary>
        /// 获取可以播放的音频输出设备列表
        /// </summary>
        /// <returns>OutDevice集合</returns>
        public static async Task<List<OutDevice>> GetOutDevicesAsync()
        {
            List<OutDevice> outDevices = new List<OutDevice>();
            await Task.Run(() =>
            {
                // WaveOut
                for (int n = -1; n < WaveOut.DeviceCount; n++)
                {
                    var wocb = WaveOut.GetCapabilities(n);
                    string name = wocb.ProductName;
                    OutDevice outDevice = new OutDevice(OutApi.WaveOut, n, name) { IsDefaultDevice = name == "Microsoft 声音映射器" || name == "Microsoft Sound Mapper" ? true : false };
                    outDevices.Add(outDevice);
                }
                if (outDevices.Count < 2) outDevices.Clear();

                // DirectSound
                foreach (var dev in DirectSoundOut.Devices)
                {
                    string name = dev.Description;
                    OutDevice outDevice = new OutDevice(OutApi.DirectSound, dev, name) { IsDefaultDevice = name == "主声音驱动程序" ? true : false };

                    if (dev != null)
                        foreach (var device in outDevices)
                        {
                            if (device != outDevice)
                            {
                                outDevices.Add(outDevice);
                                break;
                            }
                        }
                }
                if (outDevices.Count < 2) outDevices.Clear();

                if (outDevices.Any())
                {
                    // Wasapi
                    var enumerator = new MMDeviceEnumerator();
                    try
                    {
                        // 添加默认设备
                        outDevices.Add(GetWasapiDefaultDevice(enumerator));
                    }
                    catch { }

                    foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                    {
                        OutDevice outDevice = new OutDevice(OutApi.Wasapi, wasapi.ID, wasapi.FriendlyName);
                        outDevice.SampleRate = wasapi.AudioClient.MixFormat.SampleRate;
                        outDevice.Channels = wasapi.AudioClient.MixFormat.Channels;
                        outDevices.Add(outDevice);
                    }
                    enumerator.Dispose();

                    // Asio
                    foreach (var asio in AsioOut.GetDriverNames())
                    {
                        OutDevice outDevice = new OutDevice(OutApi.Asio, asio, asio);
                        outDevices.Add(outDevice);
                    }

                }
                if (!outDevices.Any())
                {
                    outDevices.Add(new(OutApi.None, null, "无音频输出设备"));
                }
            });

            return outDevices;
        }

        public static async Task<OutDevice> GetWasapiDeviceFromOtherAPI(OutDevice outDevice)
        {
            if (outDevice.DeviceType == OutApi.Wasapi) return outDevice;
            if (outDevice.DeviceType == OutApi.Asio) return null;
            var outDevices = await GetOutDevicesAsync();
            int audioOutDeviceCount = 0;
            foreach (var device in outDevices)
            {
                if (device.DeviceType == OutApi.Wasapi) audioOutDeviceCount++;
            }

            OutDevice result = null;
            switch (outDevice.DeviceType)
            {
                case OutApi.WaveOut:
                    result = outDevices[outDevices.IndexOf(App.audioPlayer.NowOutDevice) + audioOutDeviceCount * 2];
                    break;
                case OutApi.DirectSound:
                    result = outDevices[outDevices.IndexOf(App.audioPlayer.NowOutDevice) + audioOutDeviceCount];
                    break;
            }
            return result;
        }
    }

    public class NotificationClientImplementation : IMMNotificationClient
    {
        public delegate void OnDefaultDeviceChangedDelegate(DataFlow dataFlow, Role deviceRole, string defaultDeviceId);
        public event OnDefaultDeviceChangedDelegate OnDefaultDeviceChangedEvent;

        public delegate void OnPropertyValueChangedDelegate(string deviceId);
        public event OnPropertyValueChangedDelegate OnDeviceAddedEvent;
        public event OnPropertyValueChangedDelegate OnDeviceRemovedEvent;

        public delegate void OnDeviceStateChangedDelegate(string deviceId, DeviceState newState);
        public event OnDeviceStateChangedDelegate OnDeviceStateChangedEvent;
        
        public delegate void OnOnPropertyValueChangedDelegate(string deviceId, PropertyKey propertyKey);
        public event OnOnPropertyValueChangedDelegate OnPropertyValueChangedEvent;

        public void OnDefaultDeviceChanged(DataFlow dataFlow, Role deviceRole, string defaultDeviceId)
        {
            Debug.WriteLine("[DeviceManage]: Default Device Changed.");
            OnDefaultDeviceChangedEvent?.Invoke(dataFlow, deviceRole, defaultDeviceId);
        }

        public void OnDeviceAdded(string deviceId)
        {
            Debug.WriteLine("[DeviceManage]: Device added.");
            OnDeviceAddedEvent?.Invoke(deviceId);
        }

        public void OnDeviceRemoved(string deviceId)
        {
            Debug.WriteLine("[DeviceManage]: Device removed.");
            OnDeviceRemovedEvent?.Invoke(deviceId);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            Debug.WriteLine($"[DeviceManage]: Device State changed. deviceId:{deviceId}/newState:{newState}.");
            OnDeviceStateChangedEvent?.Invoke(deviceId, newState);
        }

        public void OnPropertyValueChanged(string deviceId, PropertyKey propertyKey)
        {
            Debug.WriteLine($"[DeviceManage]: Device PropertyValue changed. deviceId: {deviceId} / propertyKey:{propertyKey.formatId.ToString()}.");
            OnPropertyValueChangedEvent?.Invoke(deviceId, propertyKey);
        }

        public NotificationClientImplementation()
        {

        }
    }

    public class ClientDeviceEvents
    {
        private MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
        public NotificationClientImplementation notificationClient;
        public IMMNotificationClient notifyClient;

        public ClientDeviceEvents()
        {
            notificationClient = new NotificationClientImplementation();
            notifyClient = notificationClient;
            deviceEnum.RegisterEndpointNotificationCallback(notifyClient);
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

        public ClientDeviceEvents ClientDeviceEvents { get; private set; } = new();
        public Media.AudioFileReader FileReader { get; set; } = null;
        public AudioEffects.SoundTouchWaveProvider FileProvider { get; set; } = null;
        public enum OutApi { WaveOut, DirectSound, Wasapi, Asio, None }
        public IWavePlayer NowOutObj { get; set; } = null;
        public MidiFile MidiFile { get; set; } = null;
        public OutputDevice MidiOutputDevice { get; set; } = null;
        public Playback MidiPlayback { get; set; } = null;
        public MusicData MusicData { get; private set; }
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
                if (FileReader != null)
                {
                    EqualizerBand = EqualizerBand;
                    FileReader.EqEnabled = value;
                }
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

        public static TimeSpan ct = TimeSpan.Zero;
        public TimeSpan CurrentTime
        {
            get
            {
                if (localFileIniting) return default;
                if (FileReader != null)
                {
                    if (FileReader.isMidi)
                    {
                        if (MidiPlayback == null) return TimeSpan.Zero;
                        return TimeSpan.FromMilliseconds((MidiPlayback.GetCurrentTime(TimeSpanType.Metric) as MetricTimeSpan).TotalMilliseconds);
                    }
                    else
                    {
                        if (MusicData.CUETrackData != null)
                        {
                            //Debug.WriteLine($"{FileReader.CurrentTime}  --  {MusicData.CUETrackData.EndDuration}");
                            return FileReader.CurrentTime - MusicData.CUETrackData.StartDuration - TimeSpan.FromMilliseconds(Latency);
                        }
                        else
                        {
                            return FileReader.CurrentTime - (NowOutDevice.DeviceType != OutApi.Wasapi ? TimeSpan.FromMilliseconds(Latency) : TimeSpan.Zero);
                        }
                    }
                }
                return TimeSpan.Zero;
            }
            set
            {
                if (localFileIniting) return;
                if (FileReader != null)
                {
                    if (FileReader.isMidi)
                    {
                        if (MidiPlayback != null)
                            MidiPlayback.MoveToTime(new MetricTimeSpan(value.Hours, value.Minutes, value.Seconds, value.Milliseconds));
                    }
                    else
                    {
                        ct = value;
                        if (MusicData.CUETrackData != null)
                        {
                            FileReader.CurrentTime = MusicData.CUETrackData.StartDuration + value;
                        }
                        else
                        {
                            FileReader.CurrentTime = value;
                        }
                    }
                    TimingChanged?.Invoke(this);
                }
            }
        }
        
        public TimeSpan TotalTime
        {
            get
            {
                if (localFileIniting) return TimeSpan.MaxValue;
                if (FileReader != null)
                {
                    if (FileReader.isMidi)
                    {
                        if (MidiPlayback == null) return TimeSpan.Zero;
                        return TimeSpan.FromMilliseconds((MidiPlayback.GetDuration(TimeSpanType.Metric) as MetricTimeSpan).TotalMilliseconds);
                    }
                    else
                    {
                        if (MusicData.CUETrackData != null)
                        {
                            return MusicData.CUETrackData.Duration;
                        }
                        else
                        {
                            return FileReader.TotalTime;// - TimeSpan.FromMilliseconds(Latency);
                        }
                    }
                }
                return TimeSpan.Zero;
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
                        if (MidiPlayback == null) return PlaybackState.Stopped;
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

        private OutDevice _nowOutDevice = new(OutApi.None);
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
            get
            {
                return _volume;
            }
            set
            {
                if (value < 0f) value = 0;
                else if (value > 100f) value = 100f;
                _volume = value;
                VolumeChanged?.Invoke(this, value);
                if (FileReader != null)
                {
                    if (!FileReader.isMidi)
                    {
                        FileReader.Volume = value / 100;
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

        public AudioPlayer()
        {
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(250) };
            timer.Tick += (_, __) => { ReCallTiming(); };

            App.cacheManager.AddingCacheMusicData += CacheManager_AddingCacheMusicData;
            App.cacheManager.CachedMusicData += CacheManager_CachedMusicData;
            App.cacheManager.CachingStateChangeMusicData += CacheManager_CachingStateChangeMusicData;
            ClientDeviceEvents.notificationClient.OnDefaultDeviceChangedEvent += NotificationClient_OnDefaultDeviceChangedEvent;
            ClientDeviceEvents.notificationClient.OnDeviceRemovedEvent += NotificationClient_OnDeviceRemovedEvent;
        }

        private void CacheManager_AddingCacheMusicData(MusicData musicData, object value)
        {
            if (musicData != pointMusicData) return;
            CacheLoadingChanged?.Invoke(this, null);
        }

        private void CacheManager_CachedMusicData(MusicData musicData, object value)
        {
            if (musicData != pointMusicData) return;
            CacheLoadedChanged?.Invoke(this);
        }

        private void CacheManager_CachingStateChangeMusicData(MusicData musicData, object value)
        {
            if (musicData != pointMusicData) return;
            CacheLoadingChanged?.Invoke(this, value);
        }

        bool isInErrorDialog = false;
        bool isInReloadDefaultDeviceChanged = false;
        private async void NotificationClient_OnDefaultDeviceChangedEvent(DataFlow dataFlow, Role deviceRole, string defaultDeviceId)
        {
            await Task.Delay(100); // 不加会导致 集合被修改 的错误，DirectSound 导致的 >:(
            var devices = await OutDevice.GetOutDevicesAsync();
            if (NowOutObj == null)
            {
                NowOutDevice = devices.First();
                return;
            }
            if (devices.First().DeviceType == OutApi.None)
            {
                if (isInErrorDialog) return;
                isInErrorDialog = true;
                MainWindow.Invoke(() =>
                {
                    MainWindow.AddNotify("无音频输出设备", "似乎所有音频输出设备都已被拔出，程序找不到音频输出设备。\n" +
                        "请检查音频驱动是否正常工作，或检查音频输出设备的接口是否松动或拔出。\n" +
                        "如果检查完毕后仍然无法正常播放，请到 GitHub 里向项目提出 Issues。",
                        NotifySeverity.Error);
                    isInErrorDialog = false;
                });
                return;
            }
            MainWindow.Invoke(() => SetPlay());
            if (isInReloadDefaultDeviceChanged) return;
            if (NowOutObj.GetType() != typeof(DirectSoundOut) && NowOutObj.GetType() != typeof(WasapiOut)) return;
            if (!NowOutDevice.IsDefaultDevice) return;

            isInReloadDefaultDeviceChanged = true;
            if (NowOutObj.GetType() == typeof(WasapiOut)) NowOutDevice = OutDevice.GetWasapiDefaultDevice();
            MainWindow.Invoke(() =>
            {
                SetReloadAsync(true);
            });
            await Task.Delay(1);
            isInReloadDefaultDeviceChanged = false;
        }

        private void NotificationClient_OnDeviceRemovedEvent(string deviceId)
        {
            Debug.WriteLine("[DeviceManage]: Device Removed.");
        }

        public MusicData pointMusicData = null;
        int freezeSetSourceCount = 0;
        public async Task SetSource(MusicData musicData)
        {
            pointMusicData = musicData;
            freezeSetSourceCount++;
            await Task.Delay(200);
            freezeSetSourceCount--;
            if (freezeSetSourceCount > 0) return;

            isCUEEndCalled = false;
            if (musicData == MusicData)
            {
                if (FileReader != null)
                {
                    CurrentTime = TimeSpan.Zero;
                }
                return;
            }

            string resultPath = null;
            if (musicData.From == MusicFrom.localMusic) resultPath = musicData.InLocal;
            else
            {
                try
                {
                    resultPath = await App.cacheManager.StartCacheMusic(musicData);
                }
                catch (Exception e) { throw; }
            }

            if (await Task.Run(() => !File.Exists(resultPath)))
            {
                throw new FileNotFoundException($"找不到位于 \"{resultPath}\" 的音频文件。");
            }

            if (pointMusicData == musicData)
            {
                var m = MusicData;
                MusicData = musicData;
                Exception exception = null;
                _filePath = resultPath;
                Debug.WriteLine($"[AudioPlayer]: 正在加载 \"{resultPath}\".");
                try
                {
                    CacheLoadingChanged?.Invoke(this, null);
                    await SetSource(resultPath, musicData.CUETrackData != null);
                    CacheLoadedChanged?.Invoke(this);
                }
                catch (Exception err)
                {
                    MusicData = m;
                    exception = err;
                    Debug.WriteLine(err);
                }
                finally
                {
                    localFileIniting = false;
                    PlayStateChanged?.Invoke(this);
                    TimingChanged?.Invoke(this);
                }

                if (exception != null)
                {
                    throw exception;
                }
            }
        }

        List<IWavePlayer> WavePlayers { get; set; } = new();
        string _filePath = null;
        public string FileType = null;
        public int FileSize = 0;
        public string AudioBitrate = null;
        public bool localFileIniting = false;
        public ATL.Track tfile = null;
        public async Task SetSource(string filePath, bool cueFile = false)
        {
            MusicData musicData = MusicData;
            if (localFileIniting) return;
            if (filePath != _filePath) return;
            if (FileReader != null)
            {
                if (filePath == FileReader.FileName)
                {
                    if (MusicData.CUETrackData != null)
                        FileReader.CurrentTime = MusicData.CUETrackData.StartDuration;
                    else
                        FileReader.CurrentTime = TimeSpan.Zero;
                    PreviewSourceChanged?.Invoke(this);
                    SourceChanged?.Invoke(this);
                    localFileIniting = false;
                    return;
                }
            }

            var devices = await OutDevice.GetOutDevicesAsync();
            if (devices.First().DeviceType == OutApi.None)
            {
                throw new Exception("当前没有音频输出设备。\n请检查音频设置是否正确、输出设备是否插入和音频设备驱动是否正常工作。");
            }
            if (NowOutDevice.DeviceType == OutApi.None)
            {
                NowOutDevice = devices.First();
            }

            AudioFileReader fileReader = null;
            AudioEffects.SoundTouchWaveProvider fileProvider = null;
            localFileIniting = true;

            await Task.Run(() =>
            {
                FileSize = File.ReadAllBytes(filePath).Length;
                fileReader = new AudioFileReader(filePath, cueFile);

                if (fileReader.isMidi)
                {
                    WaveInfo = "midi";
                    return;
                }
                UpdateInfo();

                fileProvider = new AudioEffects.SoundTouchWaveProvider(fileReader);
                fileReader.EqEnabled = EqEnabled;
                fileReader.Volume = Volume / 100;
                fileProvider.Pitch = Pitch;
                fileProvider.Tempo = Tempo;
                fileProvider.Rate = Rate;
            });
            await Task.Run(() => DisposeAll());
            FileReader = fileReader;
            FileProvider = fileProvider;
            Debug.WriteLine($"[AudioPlayer]: FileReader filePath \"{fileReader.FileName}\".");
            if (EqEnabled && !fileReader.isMidi)
            {
                EqualizerBand = EqualizerBand;
            }

            if (MusicData.CUETrackData != null)
            {
                FileReader.CurrentTime = musicData.CUETrackData.StartDuration;
                TimingChanged += AudioPlayer_TimingChanged;
            }

            PreviewSourceChanged?.Invoke(this);

            if (FileReader.isMidi)
            {
                MidiOutputDevice = OutputDevice.GetByIndex(0);
                MidiFile = MidiFile.Read(filePath, new()
                {
                    NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
                    InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore
                });
                MidiPlayback = MidiFile.GetPlayback(MidiOutputDevice);
                MidiPlayback.Finished += (_, __) => MainWindow.Invoke(() => AudioPlayer_PlaybackStopped(null, null));
                MidiPlayback.Speed = Tempo;
            }
            else
            {/*
                bool notDefaultLatency = false;
                if (Latency != (int)SettingDefault[SettingParams.AudioLatency.ToString()])
                {
                    notDefaultLatency = true;
                }*/

                switch (NowOutDevice.DeviceType)
                {
                    case OutApi.WaveOut:
                        await Task.Run(() => NowOutObj = new WaveOutEvent());
                        (NowOutObj as WaveOutEvent).DeviceNumber = NowOutDevice.Device == null ? -1 : (int)NowOutDevice.Device;
                        (NowOutObj as WaveOutEvent).NumberOfBuffers = Latency;
                        NowOutObj.Init(FileProvider);
                        NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                        break;
                    case OutApi.DirectSound:
                        if (NowOutDevice.Device == null)
                        {
                            await Task.Run(() => NowOutObj = new DirectSoundOut(Latency));
                        }
                        else
                        {
                            await Task.Run(() => NowOutObj = new DirectSoundOut((NowOutDevice.Device as DirectSoundDeviceInfo).Guid, Latency));
                        }
                        NowOutObj.Init(FileProvider);
                        NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                        break;
                    case OutApi.Wasapi:

                        MMDevice device = null;
                        await Task.Run(() =>
                        {
                            device = new MMDeviceEnumerator().GetDevice(NowOutDevice.Device as string);
                            NowOutObj = new WasapiOut(
                                device,
                                WasapiOnly ? AudioClientShareMode.Exclusive : AudioClientShareMode.Shared, false,
                                Latency);
                        });
                        try
                        {
                            NowOutObj.Init(FileProvider);
                        }
                        catch (COMException)
                        {
                            if (WasapiOnly)
                                throw new Exception("当前输出设备似乎不支持独占模式。\n" +
                                    "请尝试到音频输出设备的 属性 页面的 高级 选项卡打开 允许应用程序独占控制该设备。");
                            throw new Exception("无法初始化音频输出，可能是其它应用程序独占了此音频输出设备，请尝试重新播放。");
                        }
                        NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                        device.Dispose();
                        break;
                    case OutApi.Asio:
                        await Task.Run(() => NowOutObj = new AsioOut(NowOutDevice.Device.ToString()));
                        //if (notDefaultLatency) (NowOutObj as AsioOut).PlaybackLatency = Latency;
                        NowOutObj.Init(FileProvider);
                        NowOutObj.PlaybackStopped += AudioPlayer_PlaybackStopped;
                        break;
                }
                Debug.WriteLine($"[AudioPlayer]: Inited FileReader filePath \"{fileReader.FileName}\".");
                Debug.WriteLine($"[AudioPlayer]: Inited MusicData \"{MusicData}\".");
            }

            SourceChanged?.Invoke(this);
            localFileIniting = false;
        }

        private void AudioPlayer_TimingChanged(AudioPlayer audioPlayer)
        {
        }

        public async Task Reload(TimeSpan? reloadedStreamPosition = null)
        {
            if (FileReader != null)
            {
                if (FileReader.isMidi) return;
                TimeSpan nowPosition = reloadedStreamPosition == null ? FileReader.CurrentTime : (TimeSpan)reloadedStreamPosition;
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

        public async void SetReloadAsync(bool autoPlay = false)
        {
            try
            {
                await Reload();
                if (autoPlay)
                {
                    await Task.Delay(10);
                    SetPlay();
                }
            }
            catch (Exception err) { Debug.WriteLine(err.ToString()); }
        }

        public void UpdateInfo()
        {
            try
            {
                tfile = new ATL.Track(_filePath);
            }
            catch { }
            if (tfile != null)
            {
                FileType = tfile.AudioFormat.MimeList.First().Split('/')[1];
                try
                {
                    WaveInfo = $"{(decimal)tfile.SampleRate / 1000}kHz-{tfile.Bitrate}kbps-{FileType}";
                }
                catch
                {
                    WaveInfo = "未知";
                }
            }
        }

        [Obsolete(message:"不建议使用，性能较差")]
        public void SetEqualizer(int position, float db)
        {
            EqualizerBand[position][2] = db;
            if (FileReader != null)
                FileReader.CreateFilters();
        }

        bool isCUEEndCalled = false;
        private void AudioPlayer_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            MainWindow.Invoke(() =>
            {
                if (FileReader != null)
                {
                    if (FileReader.IsDisposed)
                        PlayEnd?.Invoke(this);
                    else
                    {
                        var a = CurrentTime + TimeSpan.FromSeconds(1.5);
                        if (a >= TotalTime)
                        {
                            if (!isCUEEndCalled)
                            {
                                if (MusicData.CUETrackData != null) isCUEEndCalled = true;
                                PlayEnd?.Invoke(this);
                            }
                        }
                    }
                }
            });
        }

        public bool SetPlay()
        {
            if (localFileIniting) return false;
            NowOutObj?.Play();
            MidiPlayback?.Start();
            PlayStateChanged?.Invoke(this);
            ReCallTiming();
            return true;
        }
        
        public bool SetPause()
        {
            if (localFileIniting) return false;
            NowOutObj?.Pause();
            MidiPlayback?.Stop();
            PlayStateChanged?.Invoke(this);
            return true;
        }
        
        public bool SetStop()
        {
            if (localFileIniting) return false;
            NowOutObj?.Stop();
            MidiPlayback?.Stop();
            PlayStateChanged?.Invoke(this);
            return true;
        }

        public void ReCallTiming()
        {
            //.WriteLine($"ReCall Audio Player Timing Count {TimingChanged?.GetInvocationList()?.Length}.");
            timer.Start();
            if (PlaybackState != PlaybackState.Playing) timer.Stop();
            if (TimingChanged == null) timer.Stop();
            if (!timer.IsEnabled) return;

            TimingChanged?.Invoke(this);
            if (MusicData.CUETrackData != null) AudioPlayer_PlaybackStopped(null, null);
        }

        bool isDisposing = false;
        public void DisposeAll()
        {
            isDisposing = true;
            TimingChanged -= AudioPlayer_TimingChanged;

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
                MidiOutputDevice?.Dispose();
            }
            finally
            {
                MidiOutputDevice = null;
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
