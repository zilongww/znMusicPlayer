using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using NAudio.Dsp;
using NAudio.Codecs;
using System.IO;
using System.Linq;
using znMusicPlayerWUI.Helpers;
using NAudio.Midi;

namespace znMusicPlayerWUI.Media
{
    public class AudioFileReader : WaveStream, ISampleProvider
    {
        private List<BiQuadFilter[]> _filters = new();
        private WaveStream readerStream;
        private readonly SampleChannel sampleChannel;
        private readonly int destBytesPerSample;
        private readonly int sourceBytesPerSample;
        private readonly long length;
        private readonly object lockObject;

        public bool EqEnabled { get; set; }
        public string FileName { get; }
        public string FileAddr { get; private set; }

        public override WaveFormat WaveFormat => sampleChannel?.WaveFormat;

        public override long Length => length;

        public override long Position
        {
            get
            {
                return SourceToDest(readerStream.Position);
            }
            set
            {
                lock (lockObject)
                {
                    readerStream.Position = DestToSource(value);
                }
            }
        }

        public float Volume
        {
            get
            {
                return sampleChannel.Volume;
            }
            set
            {
                sampleChannel.Volume = value;
            }
        }

        public string addr = null;
        public bool isMidi = false;

        public AudioFileReader(string fileName)
        {
            lockObject = new object();
            FileName = fileName;
            CreateReaderStream(fileName);
            if (isMidi) return;
            sourceBytesPerSample = readerStream.WaveFormat.BitsPerSample / 8 * readerStream.WaveFormat.Channels;
            sampleChannel = new SampleChannel(readerStream, forceStereo: false);
            destBytesPerSample = 4 * sampleChannel.WaveFormat.Channels;
            length = SourceToDest(readerStream.Length);
            CreateFilters();
        }

        private void CreateReaderStream(string fileName)
        {
            if (File.Exists(fileName))
            {
                var f = File.ReadAllBytes(fileName);
                if (f.Length > 0)
                {
                    try
                    {
                        addr = FileHelper.FileTypeGet(fileName);
                    }
                    catch
                    {
                        addr = "-1";
                    }
                    //addr = "-1";
                    FileAddr = addr;
                    switch (addr)
                    {
                        case "10276":
                            readerStream = new NAudio.Flac.FlacReader(fileName);
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("AudioFileReader: 正在使用 Flac 解码器");
#endif
                            break;
                        case "7368":
                            readerStream = new Mp3FileReader(fileName);
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("AudioFileReader: 正在使用 MP3 解码器");
#endif
                            break;
                        case "8273":
                            readerStream = new WaveFileReader(fileName);
                            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
                            {
                                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                                readerStream = new BlockAlignReductionStream(readerStream);
#if DEBUG
                                System.Diagnostics.Debug.WriteLine("AudioFileReader: 正在使用 Wave 解码器");
#endif
                            }
                            break;
                        case "7079":
                            readerStream = new AiffFileReader(fileName);
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("AudioFileReader: 正在使用 Aiff 解码器");
#endif
                            break;
                        case "7784":
                            isMidi = true;
                            break;
                        default:
                            if (File.Exists(fileName))
                            {
#if DEBUG
                                System.Diagnostics.Debug.WriteLine($"AudioFileReader: 正在使用 Microsoft MediaFoundationReader 解码器，文件标识符为：{addr}");
#endif
                                readerStream = new MediaFoundationReader(fileName);
                            }
                            break;
                    }
                    return;
                }
            }
            throw new FileLoadException("无法读取此音频文件。");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            WaveBuffer waveBuffer = new WaveBuffer(buffer);
            int count2 = count / 4;
            return Read(waveBuffer.FloatBuffer, offset / 4, count2) * 4;
        }

        // 均衡器
        public void CreateFilters()
        {
            _filters.Clear();

            foreach (float[] floats in AudioEqualizerBands.NormalBands)
            {
                var filter = new BiQuadFilter[WaveFormat.Channels];
                for (int n = 0; n < WaveFormat.Channels; n++)
                {
                    filter[n] = BiQuadFilterPeak(floats[0], floats[1], floats[2]);
                }
                _filters.Add(filter);
            }
        }

        public BiQuadFilter BiQuadFilterPeak(float centreFrequency, float q, float dbGain)
        {
            BiQuadFilter filter = BiQuadFilter.PeakingEQ(WaveFormat.SampleRate, centreFrequency, q, dbGain);
            return filter;
        }

        // 在读取音频数据时加入均衡器数据
        public int Read(float[] buffer, int offset, int count)
        {
            lock (lockObject)
            {
                int samplesRead = sampleChannel.Read(buffer, offset, count);
                if (!EqEnabled) return samplesRead;

                for (var a = 0; a < AudioEqualizerBands.NormalBands.Count; a++)
                {
                    for (int i = 0; i < samplesRead; i++)
                    {
                        var ch = i % WaveFormat.Channels;
                        try
                        {
                            buffer[offset + i] = _filters[a][ch].Transform(buffer[offset + i]);
                        }
                        catch { }
                    }
                }
                return samplesRead;
            }
        }

        private long SourceToDest(long sourceBytes)
        {
            return destBytesPerSample * (sourceBytes / sourceBytesPerSample);
        }

        private long DestToSource(long destBytes)
        {
            return sourceBytesPerSample * (destBytes / destBytesPerSample);
        }

        public bool IsDisposed { get; set; } = false;
        protected override void Dispose(bool disposing)
        {
            if (disposing && readerStream != null)
            {
                readerStream.Dispose();
                readerStream = null;
            }

            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
