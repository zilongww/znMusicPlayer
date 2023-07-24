using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace znMusicPlayerWUI.Media.Decoder.FFmpeg
{
    public unsafe class FFmpegDecoder
    {
        public enum MediaState { Play, Pause, Stop, None }
        public MediaState State { get; protected set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan frameDuration { get; protected set; }
        public TimeSpan Position { get => OffsetClock + clock.Elapsed; }
        public AVCodecID CodecId { get; set; }
        public string CodecName { get; set; }
        public long Bitrate { get; set; }
        public int Channels { get; set; }
        public AVChannelLayout* ChannelLayout { get; set; }
        public int SampleRate { get; set; }
        public AVSampleFormat SampleFormat { get; set; }
        public int BitsPerSample { get; set; }
        public bool IsPlaying { get; protected set; }

        object SyncLock = new object();
        bool isNextFrame = true;
        //播放上一帧的时间
        TimeSpan lastTime;
        TimeSpan OffsetClock;
        Stopwatch clock = new Stopwatch();
        bool isNexFrame = true;
        public delegate void MediaHandler(TimeSpan duration);
        public event MediaHandler MediaCompleted;

        AVFormatContext* format;
        AVStream* stream;
        AVCodecContext* codec_ctx;
        int streamIndex;
        nint buffer;
        byte* buffer_ptr;
        AVPacket* packet;
        AVFrame* frame;
        public unsafe int InitDecodecAudio(string path)
        {
            // 寻找音频流
            format = ffmpeg.avformat_alloc_context();
            var tempFormat = format;
            ffmpeg.avformat_open_input(&tempFormat, path, null, null);
            ffmpeg.avformat_find_stream_info(format, null);

            // 获取音频流索引
            AVCodec* codec;
            int streamIndex = ffmpeg.av_find_best_stream(format, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, &codec, 0);
            if (streamIndex < 0)
                return -1;

            // 获取音频流
            stream = format->streams[streamIndex];

            // 创建解码上下文
            codec_ctx = ffmpeg.avcodec_alloc_context3(codec);

            if (ffmpeg.avcodec_parameters_to_context(codec_ctx, stream->codecpar) < 0)
                return -2;

            if (ffmpeg.avcodec_open2(codec_ctx, codec, null) < 0)
                return -3;

            Duration = TimeSpan.FromMilliseconds(format->duration / 100);
            CodecId = codec->id;
            CodecName = ffmpeg.avcodec_get_name(CodecId);
            Bitrate = codec_ctx->bit_rate;
            Channels = codec_ctx->channels;
            ChannelLayout = codec->ch_layouts;
            SampleRate = codec_ctx->sample_rate;
            SampleFormat = codec_ctx->sample_fmt;
            BitsPerSample = ffmpeg.av_samples_get_buffer_size(null, Channels, codec_ctx->frame_size, SampleFormat, codec_ctx->block_align);

            buffer = Marshal.AllocHGlobal(BitsPerSample);
            buffer_ptr = (byte*)buffer;
            InitConvert(
                (int)ChannelLayout,
                SampleFormat,
                SampleRate,
                (int)ChannelLayout,
                SampleFormat,
                SampleRate);

            packet = ffmpeg.av_packet_alloc();
            frame = ffmpeg.av_frame_alloc();

            return 0;
        }

        SwrContext* convert;
        bool InitConvert(int occ, AVSampleFormat osf, int osr, int icc, AVSampleFormat isf, int isr)
        {
            //创建一个重采样转换器
            convert = ffmpeg.swr_alloc();
            //设置重采样转换器参数
            convert = ffmpeg.swr_alloc_set_opts(convert, occ, osf, osr, icc, isf, isr, 0, null);
            if (convert == null)
                return false;
            //初始化重采样转换器
            ffmpeg.swr_init(convert);
            return true;
        }

        /// <summary>
        /// 尝试读取下一帧
        /// </summary>
        /// <param name="outFrame"></param>
        /// <returns></returns>
        public bool TryReadNextFrame(out AVFrame outFrame)
        {
            if (lastTime == TimeSpan.Zero)
            {
                lastTime = Position;
                isNextFrame = true;
            }
            else
            {
                if (Position - lastTime >= frameDuration)
                {
                    lastTime = Position;
                    isNextFrame = true;
                }
                else
                {
                    outFrame = *frame;
                    return false;
                }
            }
            if (isNextFrame)
            {

                lock (SyncLock)
                {
                    int result = -1;
                    //清理上一帧的数据
                    ffmpeg.av_frame_unref(frame);
                    while (true)
                    {
                        //清理上一帧的数据包
                        ffmpeg.av_packet_unref(packet);
                        //读取下一帧，返回一个int 查看读取数据包的状态
                        result = ffmpeg.av_read_frame(format, packet);
                        //读取了最后一帧了，没有数据了，退出读取帧
                        if (result == ffmpeg.AVERROR_EOF || result < 0)
                        {
                            outFrame = *frame;
                            StopPlay();
                            return false;
                        }
                        //判断读取的帧数据是否是视频数据，不是则继续读取
                        if (packet->stream_index != streamIndex)
                            continue;
                        //将包数据发送给解码器解码
                        ffmpeg.avcodec_send_packet(codec_ctx, packet);
                        //从解码器中接收解码后的帧
                        result = ffmpeg.avcodec_receive_frame(codec_ctx, frame);
                        if (result < 0)
                            continue;
                        //计算当前帧播放的时长
                        frameDuration = TimeSpan.FromTicks((long)Math.Round(TimeSpan.TicksPerMillisecond * 1000d * frame->nb_samples / frame->sample_rate, 0));
                        outFrame = *frame;
                        return true;
                    }
                }
            }
            else
            {
                outFrame = *frame;
                return false;
            }
        }
        void StopPlay()
        {
            lock (SyncLock)
            {
                if (State == MediaState.None) return;
                IsPlaying = false;
                OffsetClock = TimeSpan.FromSeconds(0);
                clock.Reset();
                clock.Stop();
                var tempFormat = format;
                ffmpeg.avformat_free_context(tempFormat);
                format = null;
                var tempCodecContext = codec_ctx;
                ffmpeg.avcodec_free_context(&tempCodecContext);
                var tempPacket = packet;
                ffmpeg.av_packet_free(&tempPacket);
                var tempFrame = frame;
                ffmpeg.av_frame_free(&tempFrame);
                var tempConvert = convert;
                ffmpeg.swr_free(&tempConvert);

                Marshal.FreeHGlobal(buffer);
                buffer_ptr = null;

                stream = null;
                streamIndex = -1;
                //视频时长
                Duration = TimeSpan.FromMilliseconds(0);
                //编解码器名字
                CodecName = String.Empty;
                CodecId = AVCodecID.AV_CODEC_ID_NONE;
                //比特率
                Bitrate = 0;
                //帧率

                Channels = 0;
                ChannelLayout = null;
                SampleRate = 0;
                BitsPerSample = 0;
                State = MediaState.None;

                lastTime = TimeSpan.Zero;
                MediaCompleted?.Invoke(Duration);
            }
        }
        /// <summary>
        /// 更改进度
        /// </summary>
        /// <param name="seekTime">更改到的位置（秒）</param>
        public void SeekProgress(int seekTime)
        {
            if (format == null || streamIndex != -1)
                return;
            lock (SyncLock)
            {
                IsPlaying = false;//将视频暂停播放
                clock.Stop();
                //将秒数转换成视频的时间戳
                var timestamp = seekTime / ffmpeg.av_q2d(stream->time_base);
                //将媒体容器里面的指定流（视频）的时间戳设置到指定的位置，并指定跳转的方法；
                ffmpeg.av_seek_frame(format, streamIndex, (long)timestamp, ffmpeg.AVSEEK_FLAG_BACKWARD | ffmpeg.AVSEEK_FLAG_FRAME);
                ffmpeg.av_frame_unref(frame);//清除上一帧的数据
                ffmpeg.av_packet_unref(packet); //清除上一帧的数据包
                int error = 0;
                //循环获取帧数据，判断获取的帧时间戳已经大于给定的时间戳则说明已经到达了指定的位置则退出循环
                while (packet->pts < timestamp)
                {
                    do
                    {
                        do
                        {
                            ffmpeg.av_packet_unref(packet);//清除上一帧数据包
                            error = ffmpeg.av_read_frame(format, packet);//读取数据
                            if (error == ffmpeg.AVERROR_EOF)//是否是到达了视频的结束位置
                                return;
                        } while (packet->stream_index != streamIndex);//判断当前获取的数据是否是视频数据
                        ffmpeg.avcodec_send_packet(codec_ctx, packet);//将数据包发送给解码器解码
                        error = ffmpeg.avcodec_receive_frame(codec_ctx, frame);//从解码器获取解码后的帧数据
                    } while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));
                }
                OffsetClock = TimeSpan.FromSeconds(seekTime);//设置时间偏移
                clock.Restart();//时钟从新开始
                IsPlaying = true;//视频开始播放
                lastTime = TimeSpan.Zero;
            }
        }
        /// <summary>
        /// 将音频帧转换成字节数组
        /// </summary>
        /// <param name="sourceFrame"></param>
        /// <returns></returns>
        public byte[] FrameConvertBytes(AVFrame* sourceFrame)
        {
            var tempBufferPtr = buffer_ptr;
            //重采样音频
            var outputSamplesPerChannel = ffmpeg.swr_convert(convert, &tempBufferPtr, frame->nb_samples, sourceFrame->extended_data, sourceFrame->nb_samples);
            //获取重采样后的音频数据大小
            var outPutBufferLength = ffmpeg.av_samples_get_buffer_size(null, 2, outputSamplesPerChannel, AVSampleFormat.AV_SAMPLE_FMT_S16, 1);
            if (outputSamplesPerChannel < 0)
                return null;
            byte[] bytes = new byte[outPutBufferLength];
            //从内存中读取转换后的音频数据
            Marshal.Copy(buffer, bytes, 0, bytes.Length);
            return bytes;
        }
        public void Play()
        {
            if (State == MediaState.Play)
                return;
            clock.Start();
            IsPlaying = true;
            State = MediaState.Play;

        }
        public void Pause()
        {
            if (State != MediaState.Play)
                return;
            IsPlaying = false;
            OffsetClock = clock.Elapsed;
            clock.Stop();
            clock.Reset();

            State = MediaState.Pause;
        }
        public void Stop()
        {
            if (State == MediaState.None)
                return;
            StopPlay();
        }
    }
}
