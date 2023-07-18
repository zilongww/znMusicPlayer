﻿using ATL.AudioData.IO;
using Commons;
using System;
using System.IO;
using static ATL.ChannelsArrangements;

namespace ATL.AudioData
{
    /// <summary>
    /// General utility class to manipulate FLAC-like tags embedded in other formats (e.g. OGG)
    /// </summary>
    internal static class FlacHelper
    {
        /// <summary>
        /// Represents general information extracted from a FLAC file
        /// </summary>
        public sealed class FlacHeader
        {
            private const byte FLAG_LAST_METADATA_BLOCK = 0x80;

            private byte[] StreamMarker = new byte[4];
            private readonly byte[] MetaDataBlockHeader = new byte[4];
            private readonly byte[] Info = new byte[18];
            // 16-bytes MD5 Sum only applies to audio data

            /// <summary>
            /// Contruct a new FlacHeader object
            /// </summary>
            public FlacHeader()
            {
                Reset();
            }

            /// <summary>
            /// Reset all data
            /// </summary>
            public void Reset()
            {
                StreamMarker = new byte[4];
                Array.Clear(MetaDataBlockHeader, 0, 4);
                Array.Clear(Info, 0, 18);
            }

            /// <summary>
            /// Read data from the given stream
            /// </summary>
            /// <param name="source">Stream to read data from</param>
            public void fromStream(Stream source)
            {
                source.Read(StreamMarker, 0, 4);
                source.Read(MetaDataBlockHeader, 0, 4);
                source.Read(Info, 0, 18); // METADATA_BLOCK_STREAMINFO
                source.Seek(16, SeekOrigin.Current); // MD5 sum for audio data
            }

            /// <summary>
            /// True if the header has valid data; false if it doesn't
            /// </summary>
            /// <returns></returns>
            public bool IsValid()
            {
                return IsValidHeader(StreamMarker);
            }

            /// <summary>
            /// Get the channels arrangement
            /// </summary>
            /// <returns>Channels arrangement</returns>
            public ChannelsArrangement getChannelsArrangement()
            {
                int channels = (Info[12] >> 1) & 0x7;
                switch (channels)
                {
                    case 0b0000: return MONO;
                    case 0b0001: return STEREO;
                    case 0b0010: return ISO_3_0_0;
                    case 0b0011: return QUAD;
                    case 0b0100: return ISO_3_2_0;
                    case 0b0101: return ISO_3_2_1;
                    case 0b0110: return LRCLFECrLssRss;
                    case 0b0111: return LRCLFELrRrLssRss;
                    case 0b1000: return JOINT_STEREO_LEFT_SIDE;
                    case 0b1001: return JOINT_STEREO_RIGHT_SIDE;
                    case 0b1010: return JOINT_STEREO_MID_SIDE;
                    default: return UNKNOWN;
                }
            }

            /// <summary>
            /// Returns true if the metadata block exists; false if it doesn't
            /// </summary>
            public bool MetadataExists { get => 0 == (MetaDataBlockHeader[1] & FLAG_LAST_METADATA_BLOCK); }

            /// <summary>
            /// Sample rate
            /// </summary>
            public int SampleRate { get => Info[10] << 12 | Info[11] << 4 | Info[12] >> 4; }

            /// <summary>
            /// Bits per sample
            /// </summary>
            public byte BitsPerSample { get => (byte)(((Info[12] & 1) << 4) | (Info[13] >> 4) + 1); }

            /// <summary>
            /// Number of samples
            /// </summary>
            public long NbSamples { get => Info[14] << 24 | Info[15] << 16 | Info[16] << 8 | Info[17]; }
        }

        public static bool IsValidHeader(byte[] data)
        {
            return StreamUtils.ArrBeginsWith(data, FLAC.FLAC_ID);
        }

        /// <summary>
        /// Read FLAC headers from the given source
        /// </summary>
        /// <param name="source">Source to read data from</param>
        /// <returns>FLAC headers</returns>
        public static FlacHeader readHeader(Stream source)
        {
            // Read header data    
            FlacHeader flacHeader = new FlacHeader();
            flacHeader.fromStream(source);
            return flacHeader;
        }
    }
}
