using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace znMusicPlayerWPF.MusicPlay
{
    public class AudioPlayer
    {
        #region 参数
        public struct OutDevice
        {
            public AudioOutApiEnum DeviceType { get; set; }
            public object Device { get; set; }
            public string DeviceName { get; set; }
            public OutDevice(AudioOutApiEnum deviceType, object device = null, string deviceName = "") : this()
            {
                DeviceType = deviceType;
                Device = device;
                DeviceName = deviceName;
            }
        }


        public OutDevice NowOutDevice = new OutDevice(AudioOutApiEnum.Wasapi);
        private MainWindow TheParent = null;

        public long fileSize = 0;
        private WaveOutEvent _WaveOut = null;
        private DirectSoundOut _DirectSoundOut = null;
        private WasapiOut _WasapiOut = null;
        private AsioOut _AsioOut = null;
        private AudioFileReader _audioFile = null;
        private WasapiLoopbackCapture _cap = null;

        private float _Volume = 0.5f;
        private TimeSpan _NowMusicTotalTime = new TimeSpan();
        private string _PlayMusicPath = null;
        private double[] _MusicVolumeData = null;

        public delegate void PlayEndDelegate();
        public event PlayEndDelegate PlayEndEvent;

        /// <summary>
        /// 每个单声道数据样本的位数，例如 16位，24位，32位
        /// </summary>
        public int BitsPerSample { get; set; }

        /// <summary>
        /// 采样率，例如 44.1Khz ，就是 44100
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// 通道数，例如 2
        /// </summary>
        public int ChannelCount { get; set; }

        public AudioFileReader audioFile
        {
            get { return _audioFile; }
        }

        public float Volume
        {
            get { return _Volume; }
            set
            {
                _Volume = value;
                if (audioFile != null) audioFile.Volume = value;
            }
        }

        public TimeSpan NowMusicTotalTime
        {
            get { return _NowMusicTotalTime; }
        }

        public TimeSpan NowMusicPosition
        {
            get
            {
                try
                {
                    return audioFile.CurrentTime;
                }
                catch
                {
                    return TimeSpan.Zero;
                }
            }
            set
            {
                if (audioFile != null) audioFile.CurrentTime = value;
            }
        }

        public string PlayMusicPath
        {
            get { return _PlayMusicPath; }
            set
            {
                _PlayMusicPath = value;
            }
        }

        public double[] MusicVolumeData
        {
            get { return _MusicVolumeData; }
        }

        public enum AudioOutApiEnum { WaveOut, DirectSound, Wasapi, Asio }

        public bool WasapiNotShare { get; set; } = false;

        private float WasapiNotShareSystemVolume = 0.2f;

        #endregion

        #region 主方法

        public AudioPlayer(MainWindow TheParent) => this.TheParent = TheParent;

        /// <summary>
        /// 获取可以播放的音频输出设备列表
        /// </summary>
        /// <returns>OutDevice集合</returns>
        public static List<OutDevice> GetOutDevices()
        {
            List<OutDevice> outDevices = new List<OutDevice>();
            string defaultName = "默认输出设备";

            // Wasapi
            // 添加默认设备
            OutDevice outDevice1 = new OutDevice(AudioOutApiEnum.Wasapi, null, defaultName);
            outDevices.Add(outDevice1);

            var enumerator = new MMDeviceEnumerator();
            foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                OutDevice outDevice = new OutDevice(AudioOutApiEnum.Wasapi, wasapi, wasapi.FriendlyName);
                outDevices.Add(outDevice);
            }

            // WaveOut
            for (int n = -1; n < WaveOut.DeviceCount; n++)
            {
                string name = WaveOut.GetCapabilities(n).ProductName;
                OutDevice outDevice = new OutDevice(AudioOutApiEnum.WaveOut, n, name == "Microsoft 声音映射器" ? defaultName : name);
                outDevices.Add(outDevice);
            }

            // DirectSound
            foreach (var dev in DirectSoundOut.Devices)
            {
                string name = dev.Description;
                OutDevice outDevice = new OutDevice(AudioOutApiEnum.DirectSound, dev, name == "主声音驱动程序" ? defaultName : name);
                outDevices.Add(outDevice);
            }

            // Asio
            foreach (var asio in AsioOut.GetDriverNames())
            {
                OutDevice outDevice = new OutDevice(AudioOutApiEnum.Asio, asio, asio);
                outDevices.Add(outDevice);
            }

            return outDevices;
        }

        /// <summary>
        /// 设置播放源文件
        /// </summary>
        /// <param name="MusicFilePath">音乐文件位置</param>
        public async Task<bool> Set(string MusicFilePath, OutDevice outDevice, bool canDelete = false)
        {
            try
            {
                FileStream fileStream = File.Open(MusicFilePath, FileMode.Open);
                fileSize = fileStream.Length;
                fileStream.Close();
            }
            catch
            {
                fileSize = 0;
            }

            string loadState = await Task.Run(() =>
            {
                DisposeAll();

                if (!File.Exists(MusicFilePath))
                {
                    return "fileNotFound";
                }

                try
                {
                    _audioFile = new AudioFileReader(MusicFilePath);
                    SampleArray_AudioFile = new AudioFileReader(MusicFilePath);
                }
                catch
                {
                    return "fileLoadError";
                }

                return null;
            });

            switch (loadState)
            {
                case "fileNotFound":
                    TheParent.ShowBox("错误：找不到文件", $"无法找到文件 \"{MusicFilePath}\"。");
                    return false;

                case "fileLoadError":
                    switch (canDelete)
                    {
                        case true:
                            File.Delete(MusicFilePath);
                            TheParent.ShowBox("加载失败", "缓存文件出现错误，请重试。");
                            break;
                        case false:
                            TheParent.ShowBox("加载失败", "本地音频文件出现错误。");
                            break;
                    }
                    return false;

                default:
                    break;
            }

            _NowMusicTotalTime = audioFile.TotalTime;

            switch (outDevice.DeviceType)
            {
                case AudioOutApiEnum.WaveOut:
                    _WaveOut = new WaveOutEvent();
                    _WaveOut.DeviceNumber = outDevice.Device == null ? -1 : (int)outDevice.Device;
                    bool state = await Task.Run(() =>
                    {
                        try
                        {
                            _WaveOut.Init(audioFile);
                            return true;
                        }
                        catch (MmException)
                        {
                            return false;
                        }
                    });

                    if (!state)
                    {
                        TheParent.ShowBox("播放错误", "播放时出现问题，请检查播放设备是否工作正常或播放文件是否损坏。");
                        return state;
                    }

                    _WaveOut.PlaybackStopped += PlaybackStopped;

                    break;

                case AudioOutApiEnum.DirectSound:
                    if (outDevice.Device == null)
                    {
                        _DirectSoundOut = new DirectSoundOut(300);
                    }
                    else
                    {
                        _DirectSoundOut = new DirectSoundOut((outDevice.Device as DirectSoundDeviceInfo).Guid, 300);
                    }
                    await Task.Run(() =>
                    {
                        _DirectSoundOut.Init(audioFile);
                    });

                    _DirectSoundOut.PlaybackStopped += PlaybackStopped;
                    break;

                case AudioOutApiEnum.Wasapi:
                    bool state1 = false;
                    if (!TheParent.WasapiNotShare)
                    {
                        if (outDevice.Device == null)
                        {
                            _WasapiOut = new WasapiOut(AudioClientShareMode.Shared, 300);
                        }
                        else
                        {
                            _WasapiOut = new WasapiOut(outDevice.Device == null ? null : (MMDevice)outDevice.Device, AudioClientShareMode.Shared, true, 300);
                        }
                        state1 = await Task.Run(() =>
                        {
                            try
                            {
                                _WasapiOut.Init(audioFile);
                                return true;
                            }
                            catch (System.Runtime.InteropServices.COMException)
                            {
                                return false;
                            }
                        });
                    }
                    else
                    {
                        try
                        {
                            if (outDevice.Device == null)
                            {
                                _WasapiOut = new WasapiOut(AudioClientShareMode.Exclusive, 300);
                            }
                            else
                            {
                                _WasapiOut = new WasapiOut((MMDevice)outDevice.Device, AudioClientShareMode.Exclusive, true, 300);
                            }
                            _WasapiOut.Init(audioFile);
                            state1 = true;
                        }
                        catch (System.Runtime.InteropServices.COMException)
                        {
                            state1 = false;
                        }
                    }

                    if (!state1)
                    {
                        TheParent.ShowBox("播放错误", "找不到输出设备。");
                        return state1;
                    }
                    _WasapiOut.PlaybackStopped += PlaybackStopped;

                    break;

                case AudioOutApiEnum.Asio:
                    try
                    {
                        _AsioOut = new AsioOut(outDevice.Device.ToString());
                        await Task.Run(() =>
                        {
                            _AsioOut.Init(audioFile);
                        });
                        _AsioOut.PlaybackStopped += PlaybackStopped;
                    }
                    catch (ArgumentException)
                    {
                        TheParent.ShowBox("声卡不支持", "此设备没有支持Asio接口的声卡，已自动调回WaveOut接口。");

                        return await Reload(); ;
                    }
                    break;

                default:
                    break;
            }

            PlayMusicPath = MusicFilePath;

            try
            {
                TheParent.PlayMusicBit.Text = $"{audioFile.WaveFormat.SampleRate / (decimal)1000}kHz-{(int)(fileSize * 8 / audioFile.TotalTime.TotalSeconds / 1000)}kbps";
            }
            catch
            {
                TheParent.PlayMusicBit.Text = "";
            }
            if (fileSize == 0) TheParent.PlayMusicBit.Text = "";

            BitsPerSample = audioFile.WaveFormat.BitsPerSample;
            SampleRate = audioFile.WaveFormat.SampleRate;
            ChannelCount = audioFile.WaveFormat.Channels;

            this.Value = new double[100];
            this.SampleArray = new float[3042];
            /*
            var readBuffer = new float[audioFile.WaveFormat.SampleRate * audioFile.WaveFormat.Channels];
            int a = audioFile.Read(readBuffer, 0, readBuffer.Length);
            System.Windows.MessageBox.Show(a.ToString());
            */

            return true;
        }

        public async Task<bool> Reload()
        {
            if (_audioFile == null || PlayMusicPath == null) return false;

            TimeSpan TheTime = NowMusicPosition;

            bool TaskState = await Set(PlayMusicPath, NowOutDevice);

            if (!TaskState)
            {
                TheParent.NowPlayState = MainWindow.PlayState.Pause;
                return false;
            }

            TheParent.NowPlayState = MainWindow.PlayState.Play;

            if (NowOutDevice.DeviceType == AudioOutApiEnum.DirectSound) await Task.Delay(50);
            await Task.Delay(10);
            try
            {
                _audioFile.CurrentTime = TheTime;
            }
            catch { }

            return TaskState;
        }

        private void PlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlayEndDo();
        }

        private void PlayEndDo()
        {
            try
            {
                if (PlayEndEvent != null && audioFile.CurrentTime + new TimeSpan(0, 1, 1, 1, 500) >= audioFile.TotalTime)
                {
                    PlayEndEvent();
                }
            }
            catch { }
        }

        private TimeSpan WasapiPauseTimeSpan = new TimeSpan(0);
        public async Task<bool> Play()
        {
            if (PlayMusicPath != null && audioFile != null)
            {
                audioFile.Volume = Volume;

                string ResultState = await Task.Run(() =>
                {
                    try
                    {
                        switch (NowOutDevice.DeviceType)
                        {
                            case AudioOutApiEnum.WaveOut:
                                _WaveOut.Play();
                                break;

                            case AudioOutApiEnum.DirectSound:
                                _DirectSoundOut.Play();
                                break;

                            case AudioOutApiEnum.Wasapi:
                                if (_WasapiOut != null)
                                {
                                    _WasapiOut.Play();
                                }
                                break;

                            case AudioOutApiEnum.Asio:
                                _AsioOut.Play();
                                break;

                            default:
                                break;
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        return $"播放时遇到问题。请检查音频输出设备是否工作正常或播放文件是否损坏\n\n错误信息：{e.Message}";
                    }
                    catch (Exception e) { return $"错误信息：{e.Message}"; }

                    return "Normal";
                });


                if (TheParent.WasapiNotShare && NowOutDevice.DeviceType == AudioOutApiEnum.Wasapi && _WasapiOut != null)
                {
                    if (WasapiNotShareSystemVolume == 0.2f) WasapiNotShareSystemVolume = _WasapiOut.Volume;
                    _WasapiOut.Volume = WasapiNotShareSystemVolume;
                }

                if (ResultState != "Normal")
                {
                    TheParent.ShowBox("播放失败", ResultState);
                    return false;
                }

                TheParent.PlayDeviceText.Text = $"{NowOutDevice.DeviceType}: {NowOutDevice.DeviceName}";
                return true;
            }

            return false;
        }

        public async Task<bool> Pause()
        {
            if (PlayMusicPath != null)
            {
                // 防止卡重复音
                if (TheParent.WasapiNotShare && NowOutDevice.DeviceType == AudioOutApiEnum.Wasapi && _WasapiOut != null)
                {
                    if (_WasapiOut.Volume != 0f) WasapiNotShareSystemVolume = _WasapiOut.Volume;
                    _WasapiOut.Volume = 0f;
                }

                return await Task.Run(() =>
                {
                    try
                    {
                        switch (NowOutDevice.DeviceType)
                        {
                            case AudioOutApiEnum.WaveOut:
                                if (_WaveOut != null) _WaveOut.Pause();
                                break;

                            case AudioOutApiEnum.DirectSound:
                                if (_DirectSoundOut != null) _DirectSoundOut.Pause();
                                break;

                            case AudioOutApiEnum.Wasapi:
                                if (_WasapiOut != null) _WasapiOut.Pause();
                                break;

                            case AudioOutApiEnum.Asio:
                                if (_AsioOut != null) _AsioOut.Pause();
                                break;

                            default:
                                break;
                        }
                    }
                    catch { return false; }

                    return true;
                });
            }

            return true;
        }

        public void Stop()
        {
            NowMusicPosition = TimeSpan.Zero;
            TheParent.NowPlayState = MainWindow.PlayState.Pause;
        }

        public void DisposeAll()
        {
            if (_audioFile != null)
            {
                _audioFile.Dispose();
                _audioFile = null;
            }

            if (SampleArray_AudioFile != null)
            {
                SampleArray_AudioFile.Dispose();
                SampleArray_AudioFile = null;
            }

            if (_WaveOut != null)
            {
                _WaveOut.PlaybackStopped -= PlaybackStopped;
                _WaveOut.Dispose();
                _WaveOut = null;
            }

            if (_DirectSoundOut != null)
            {
                _DirectSoundOut.PlaybackStopped -= PlaybackStopped;
                _DirectSoundOut.Dispose();
                _DirectSoundOut = null;
            }

            if (_WasapiOut != null)
            {
                _WasapiOut.PlaybackStopped -= PlaybackStopped;
                _WasapiOut.Dispose();
                _WasapiOut = null;
            }

            if (_AsioOut != null)
            {
                _AsioOut.PlaybackStopped -= PlaybackStopped;
                _AsioOut.Dispose();
                _AsioOut = null;
            }

            if (_cap != null)
            {
                _cap.StopRecording();
                _cap.Dispose();
                _cap = null;
            }

            GC.Collect();
        }

        ~AudioPlayer()
        {
            DisposeAll();
        }

        #endregion

        #region 可视化

        private void _cap_DataAvailable(object s, WaveInEventArgs args)
        {
            float[] samples = Enumerable
                                  .Range(0, args.BytesRecorded / 4)
                                  .Select(i => BitConverter.ToSingle(args.Buffer, i * 4))
                                  .ToArray();

            int log = (int)Math.Ceiling(Math.Log(samples.Length, 2));
            float[] filledSamples = new float[(int)Math.Pow(2, log)];
            Array.Copy(samples, filledSamples, samples.Length);

            int sampleRate = (s as WasapiLoopbackCapture).WaveFormat.SampleRate;
            Complex[] complexSrc = filledSamples.Select(v => new Complex() { X = v }).ToArray();
            FastFourierTransform.FFT(false, log, complexSrc);
            _MusicVolumeData = complexSrc.Select(v => Math.Sqrt(v.X * v.X + v.Y * v.Y)).ToArray();
        }


        public double[] GetNowVolumeData()
        {
            /*
            if (TheParent.SettingPages.VolumeDataSystemOCBtn.IsChecked)
            {
                if (_cap == null)
                {
                    _cap = new WasapiLoopbackCapture();
                    _cap.DataAvailable += _cap_DataAvailable;
                    _cap.StartRecording();
                }

                return _MusicVolumeData;
            }*/
            try
            {
                if (SampleArray_AudioFile != null)
                {
                    GetSampleArray();
                    Foo();
                }
                else { return new double[0]; }

                return Value;
            }
            catch
            {
                return new double[0];
            }

        }

        #region 获取音频采样信息
        /// <summary>
        /// 音频采样数据
        /// </summary>
        public float[] SampleArray { get; set; }
        public AudioFileReader SampleArray_AudioFile = null;

        /// <summary>
        /// 从音频文件获取采样数据
        /// </summary>
        public void GetSampleArray()
        {
            try
            {
                SampleArray_AudioFile.Position = _audioFile.Position - new TimeSpan(0, 0, 0, 0, 6).Ticks;
                SampleArray_AudioFile.Read(this.SampleArray, 0, this.SampleArray.Length);
                //_audioFile.Read(this.SampleArray, 0, this.SampleArray.Length);
            }
            catch { }
        }
        #endregion

        #region 获取频域数据
        /// <summary>
        /// 采样数据的对象锁，防止未分离左右通道就进入下一次采样
        /// </summary>
        private object _sampleLock = new object();

        /// <summary>
        /// 处理数据，不知道叫啥名，皆可Foo
        /// </summary>
        public void Foo()
        {
            try
            {
                #region 分离左右通道

                //假设 SampleArray 中已经有数据
                float[][] chanelSampleArray;
                lock (this._sampleLock)//防止未分离完左右通道就进入下一次调用 SampleArray
                {
                    chanelSampleArray = Enumerable
                        .Range(0, 2)//分离通道
                        .Select(chanel => Enumerable//对每个通过的数据进行处理
                            .Range(0, this.SampleArray.Length / this.ChannelCount)//每个通道的数组长度
                            .Select(i => this.SampleArray[chanel + i * this.ChannelCount])//左右左右，这样读取
                            .ToArray())
                        .ToArray();
                }

                #endregion

                #region 合并左右通道并取平均值

                float[] chanelAverageSample = Enumerable
                    .Range(0, chanelSampleArray[0].Length)
                    .Select(index => Enumerable//每次读取一个左右数据合并、取平均值
                        .Range(0, this.ChannelCount)
                        .Select(chanel => chanelSampleArray[chanel][index])
                        .Average())
                    .ToArray();

                #endregion

                #region 傅里叶变换
                //NAudio 提供了快速傅里叶变换的方法, 通过傅里叶变换, 可以将时域数据转换为频域数据
                // 取对数并向上取整
                int log = (int)Math.Ceiling(Math.Log(chanelAverageSample.Length, 2));
                //对于快速傅里叶变换算法, 需要数据长度为 2 的 n 次方
                int length = (int)Math.Pow(2, log);
                float[] filledSample = new float[length];
                //拷贝到新数组
                Array.Copy(chanelAverageSample, filledSample, chanelAverageSample.Length);
                //将采样转化为复数
                Complex[] complexArray = filledSample
                    .Select((value, index) => new Complex() { X = value })
                    .ToArray();
                //进行傅里叶变换
                FastFourierTransform.FFT(false, log, complexArray);

                #endregion

                #region 提取需要的频域信息

                Complex[] halfComeplexArray = complexArray
                    .Take(complexArray.Length / 2)//数据是左右对称的，所以只取一半
                    .ToArray();

                //这个已经是频域数据了
                double[] resultArray = complexArray
                    .Select(value => Math.Sqrt(value.X * value.X + value.Y * value.Y))//复数取模
                    .ToArray();

                //我们取 最小频率 ~ 20000Hz
                //对于变换结果, 每两个数据之间所差的频率计算公式为 采样率/采样数, 那么我们要取的个数也可以由 20000 / (采样率 / 采样数) 来得出
                //当然，因为我这里并没有指定频率与幅值，所以顺便取几个数就行，若有需要可以再去细分各个频率的幅值
                int count = 20000 / (this.SampleRate / length);
                double[] finalData = resultArray.Take(count).ToArray();

                #endregion

                this.Value = finalData.Take(100).ToArray();
            }
            catch { this.Value = new double[100]; }
        }

        #endregion

        /// <summary>
        /// 频域数据
        /// </summary>
        public double[] Value { get; set; }

        #endregion
    }
}
