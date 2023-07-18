﻿using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATL
{
    /// <summary>
    /// Misc. utilities used by binary readers
    /// 
    /// TODO : Benchmark against System.Buffers.Binary.BinaryPrimitives when the library's minimum .NET version becomes 5
    /// </summary>
    internal static class StreamUtils
    {
        /// <summary>
        /// Handler signature to be used when needing to process a MemoryStream
        /// </summary>
        public delegate void StreamHandlerDelegate(ref MemoryStream stream);


        /// <summary>
        /// Determines if the contents of a string (character by character) is the same
        /// as the contents of a char array
        /// </summary>
        /// <param name="a">String to be tested</param>
        /// <param name="b">Char array to be tested</param>
        /// <returns>True if both contain the same character sequence; false if not</returns>
        public static bool StringEqualsArr(String a, char[] b)
        {
            return ArrEqualsArr(a.ToCharArray(), b);
        }


        /// <summary>
        /// Determines if two char arrays have the same contents
        /// </summary>
        /// <param name="a">First array to be tested</param>
        /// <param name="b">Second array to be tested</param>
        /// <returns>True if both arrays have the same contents; false if not</returns>
        private static bool ArrEqualsArr(char[] a, char[] b)
        {
            if (b.Length != a.Length) return false;
            for (int i = 0; i < b.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if two byte arrays have the same contents
        /// </summary>
        /// <param name="a">First array to be tested</param>
        /// <param name="b">Second array to be tested</param>
        /// <returns>True if both arrays have the same contents; false if not</returns>
        public static bool ArrEqualsArr(byte[] a, byte[] b)
        {
            if (b.Length != a.Length) return false;
            for (int i = 0; i < b.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Determines if the given byte array begins with the other
        /// </summary>
        /// <param name="data">First array to be tested</param>
        /// <param name="beginning">Second array to be tested</param>
        /// <returns>True if the first array begins with all bytes from the second array; false if not</returns>
        public static bool ArrBeginsWith(byte[] data, byte[] beginning)
        {
            if (data.Length < beginning.Length) return false;
            for (int i = 0; i < beginning.Length; i++)
            {
                if (data[i] != beginning[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Copies a given number of bytes from a given stream to another, starting at current stream positions
        /// i.e. first byte will be read at from.Position and written at to.Position
        /// NB : This method cannot be used to move data within one single stream; use CopySameStream instead
        /// </summary>
        /// <param name="from">Stream to start copy from</param>
        /// <param name="to">Stream to copy to</param>
        /// <param name="length">Number of bytes to copy (optional; default = 0 = all bytes until the end of the 'from' stream)</param>
        public static void CopyStream(Stream from, Stream to, long length = 0)
        {
            byte[] data = new byte[Settings.FileBufferSize];
            int bytesToRead;
            int totalBytesRead = 0;

            while (true)
            {
                if (length > 0)
                {
                    if (totalBytesRead + Settings.FileBufferSize < length) bytesToRead = Settings.FileBufferSize; else bytesToRead = toInt(length - totalBytesRead);
                }
                else // Read everything we can
                {
                    bytesToRead = Settings.FileBufferSize;
                }
                int bytesRead = from.Read(data, 0, bytesToRead);
                if (bytesRead == 0)
                {
                    break;
                }
                to.Write(data, 0, bytesRead);
                totalBytesRead += bytesRead;
            }
        }

        /// <summary>
        /// Copy data between the two given offsets within the given stream
        /// </summary>
        /// <param name="s">Stream to process</param>
        /// <param name="offsetFrom">Starting offset to copy data from</param>
        /// <param name="offsetTo">Starting offset to copy data to</param>
        /// <param name="length">Length of the data to copy</param>
        /// <param name="progress">Progress feedback to report with</param>
        public static void CopySameStream(Stream s, long offsetFrom, long offsetTo, long length, Action<float> progress = null)
        {
            CopySameStream(s, offsetFrom, offsetTo, length, Settings.FileBufferSize, progress);
        }

        /// <summary>
        /// Copy data between the two given offsets within the given stream, using the given buffer size
        /// </summary>
        /// <param name="s">Stream to process</param>
        /// <param name="offsetFrom">Starting offset to copy data from</param>
        /// <param name="offsetTo">Starting offset to copy data to</param>
        /// <param name="length">Length of the data to copy</param>
        /// <param name="bufferSize">Buffer size to use during the operation</param>
        /// <param name="progress">Progress feedback to report with</param>
        public static void CopySameStream(Stream s, long offsetFrom, long offsetTo, long length, int bufferSize, Action<float> progress = null)
        {
            if (offsetFrom == offsetTo) return;

            byte[] data = new byte[bufferSize];
            int bufSize;
            long written = 0;
            bool forward = offsetTo > offsetFrom;
            long nbIterations = (long)Math.Ceiling(length * 1f / bufferSize);
            long resolution = (long)Math.Ceiling(nbIterations / 10f);
            float iteration = 0;

            while (written < length)
            {
                bufSize = Math.Min(bufferSize, toInt(length - written));
                if (forward)
                {
                    s.Seek(offsetFrom + length - written - bufSize, SeekOrigin.Begin);
                    s.Read(data, 0, bufSize);
                    s.Seek(offsetTo + length - written - bufSize, SeekOrigin.Begin);
                }
                else
                {
                    s.Seek(offsetFrom + written, SeekOrigin.Begin);
                    s.Read(data, 0, bufSize);
                    s.Seek(offsetTo + written, SeekOrigin.Begin);
                }
                s.Write(data, 0, bufSize);
                written += bufSize;

                if (progress != null)
                {
                    iteration++;
                    if (0 == iteration % resolution) progress(iteration / nbIterations);
                }
            }
        }

        /// <summary>
        /// Convenient converter for the use of CopySameStream only, where this goes into Min immediately
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Same value as int, or int.MaxValue if out of bounds</returns>
        private static int toInt(long value)
        {
            if (value > int.MaxValue) return int.MaxValue; else return Convert.ToInt32(value);
        }

        /// <summary>
        /// Remove a portion of bytes within the given stream
        /// </summary>
        /// <param name="s">Stream to process; must be accessible for reading and writing</param>
        /// <param name="endOffset">End offset of the portion of bytes to remove</param>
        /// <param name="delta">Number of bytes to remove (starting from endOffset, towards the beginning of the file)</param>
        /// <param name="progress">Progress feedback to report with</param>
        public static void ShortenStream(Stream s, long endOffset, uint delta, Action<float> progress = null)
        {
            CopySameStream(s, endOffset, endOffset - delta, s.Length - endOffset, progress);
            s.SetLength(s.Length - delta);
        }

        /// <summary>
        /// Add bytes within the given stream
        /// </summary>
        /// <param name="s">Stream to process; must be accessible for reading and writing</param>
        /// <param name="oldIndex">Offset where to add new bytes</param>
        /// <param name="delta">Number of bytes to add</param>
        /// <param name="fillZeroes">If true, new bytes will all be zeroes (optional; default = false)</param>
        /// <param name="progress">Progress feedback to report with</param>
        public static void LengthenStream(Stream s, long oldIndex, uint delta, bool fillZeroes = false, Action<float> progress = null)
        {
            long newIndex = oldIndex + delta;
            CopySameStream(s, oldIndex, oldIndex + delta, s.Length - oldIndex, progress);

            if (fillZeroes)
            {
                // Fill the location of old copied data with zeroes
                s.Seek(oldIndex, SeekOrigin.Begin);
                for (long i = oldIndex; i < newIndex; i++) s.WriteByte(0);
            }
        }

        /// <summary>
        /// Decode an signed byte from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static sbyte DecodeSignedByte(byte[] data)
        {
            if (data.Length < 1) throw new InvalidDataException("Data should be at least 1 bytes long; found " + data.Length + " bytes");
            return (sbyte)(data[0]);
        }

        /// <summary>
        /// Decode an unsigned byte from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static byte DecodeUByte(byte[] data)
        {
            if (data.Length < 1) throw new InvalidDataException("Data should be at least 1 bytes long; found " + data.Length + " bytes");
            return data[0];
        }

        /// <summary>
        /// Encode the given value into an array of bytes as an unsigned Little-Endian 16-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeUInt16(ushort value)
        {
            return new byte[2] { (byte)(value & 0x00FF), (byte)((value & 0xFF00) >> 8) };
        }

        /// <summary>
        /// Decode an unsigned Big-Endian 16-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static ushort DecodeBEUInt16(byte[] data)
        {
            if (data.Length < 2) throw new InvalidDataException("Data should be at least 2 bytes long; found " + data.Length + " bytes");
            return (ushort)((data[0] << 8) | (data[1] << 0));
        }

        /// <summary>
        /// Encode the given value into an array of bytes as a Big-Endian 16-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeBEUInt16(ushort value)
        {
            // Output has to be big-endian
            return new byte[2] { (byte)((value & 0xFF00) >> 8), (byte)(value & 0x00FF) };
        }

        /// <summary>
        /// Decode an unsigned Little-Endian 16-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static ushort DecodeUInt16(byte[] data)
        {
            if (data.Length < 2) throw new InvalidDataException("Data should be at least 2 bytes long; found " + data.Length + " bytes");
            return (ushort)((data[0]) | (data[1] << 8));
        }

        /// <summary>
        /// Decode an signed Little-Endian 16-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static short DecodeInt16(byte[] data)
        {
            if (data.Length < 2) throw new InvalidDataException("Data should be at least 2 bytes long; found " + data.Length + " bytes");
            return (short)((data[0]) | (data[1] << 8));
        }

        /// <summary>
        /// Encode the given value into an array of bytes as a Big-Endian 16-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeBEInt16(short value)
        {
            // Output has to be big-endian
            return new byte[2] { (byte)((value & 0xFF00) >> 8), (byte)(value & 0x00FF) };
        }

        /// <summary>
        /// Decode a signed Big-Endian 16-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static short DecodeBEInt16(byte[] data)
        {
            if (data.Length < 2) throw new InvalidDataException("Data should be at least 2 bytes long; found " + data.Length + " bytes");
            return (short)((data[0] << 8) | (data[1] << 0));
        }

        /// <summary>
        /// Decode a signed Big-Endian 24-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static int DecodeBEInt24(byte[] data)
        {
            if (data.Length < 3) throw new InvalidDataException("Data should be at least 3 bytes long; found " + data.Length + " bytes");
            return (data[0] << 16) | (data[1] << 8) | (data[2] << 0);
        }

        /// <summary>
        /// Decode an unsigned Big-Endian 24-bit integer from the given array of bytes, starting from the given offset
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <param name="offset">Offset to read value from (default : 0)</param>
        /// <returns>Decoded value</returns>
        public static uint DecodeBEUInt24(byte[] data, int offset = 0)
        {
            if (data.Length - offset < 3) throw new InvalidDataException("Value should at least contain 3 bytes after offset; actual size=" + (data.Length - offset) + " bytes");
            return (uint)(data[offset] << 16 | data[offset + 1] << 8 | data[offset + 2]);
        }

        /// <summary>
        /// Encode the given value into an array of bytes as a Big-Endian 24-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeBEUInt24(uint value)
        {
            if (value > 0x00FFFFFF) throw new InvalidDataException("Value should not be higher than " + 0x00FFFFFF + "; actual value=" + value);

            // Output has to be big-endian
            return new byte[3] { (byte)((value & 0x00FF0000) >> 16), (byte)((value & 0x0000FF00) >> 8), (byte)(value & 0x000000FF) };
        }

        /// <summary>
        /// Decode an unsigned Big-Endian 32-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static uint DecodeBEUInt32(byte[] data)
        {
            if (data.Length < 4) throw new InvalidDataException("Data should be at least 4 bytes long; found " + data.Length + " bytes");
            return (uint)((data[0] << 24) | (data[1] << 16) | (data[2] << 8) | (data[3] << 0));
        }

        /// <summary>
        /// Decode an unsigned Little-Endian 32-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static uint DecodeUInt32(byte[] data)
        {
            if (data.Length < 4) throw new InvalidDataException("Data should be at least 4 bytes long; found " + data.Length + " bytes");
            return (uint)((data[0]) | (data[1] << 8) | (data[2] << 16) | (data[3] << 24));
        }

        /// <summary>
        /// Encode the given value into an array of bytes as a Big-Endian unsigned 32-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeBEUInt32(uint value)
        {
            // Output has to be big-endian
            return new byte[4] { (byte)((value & 0xFF000000) >> 24), (byte)((value & 0x00FF0000) >> 16), (byte)((value & 0x0000FF00) >> 8), (byte)(value & 0x000000FF) };
        }

        /// <summary>
        /// Decode a signed Big-Endian 32-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static int DecodeBEInt32(byte[] data)
        {
            if (data.Length < 4) throw new InvalidDataException("Data should be at least 4 bytes long; found " + data.Length + " bytes");
            return (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | (data[3] << 0);
        }

        /// <summary>
        /// Encode the given value into an array of bytes as a Big-Endian 32-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeBEInt32(int value)
        {
            // Output has to be big-endian
            return new byte[4] { (byte)((value & 0xFF000000) >> 24), (byte)((value & 0x00FF0000) >> 16), (byte)((value & 0x0000FF00) >> 8), (byte)(value & 0x000000FF) };
        }

        /// <summary>
        /// Encode the given value into an array of bytes as a Little-Endian 32-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeInt32(int value)
        {
            return new byte[4] { (byte)(value & 0x000000FF), (byte)((value & 0x0000FF00) >> 8), (byte)((value & 0x00FF0000) >> 16), (byte)((value & 0xFF000000) >> 24) };
        }

        /// <summary>
        /// Encode the given value into an array of bytes as an unsigned Little-Endian 32-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeUInt32(uint value)
        {
            return new byte[4] { (byte)(value & 0x000000FF), (byte)((value & 0x0000FF00) >> 8), (byte)((value & 0x00FF0000) >> 16), (byte)((value & 0xFF000000) >> 24) };
        }

        /// <summary>
        /// Decode a signed Little-Endian 32-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static int DecodeInt32(byte[] data)
        {
            if (data.Length < 4) throw new InvalidDataException("data should be at least 4 bytes long; found " + data.Length + " bytes");
            return (data[0]) | (data[1] << 8) | (data[2] << 16) | (data[3] << 24);
        }

        /// <summary>
        /// Decode an unsigned Little-Endian 64-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static ulong DecodeUInt64(byte[] data)
        {
            if (data.Length < 8) throw new InvalidDataException("Data should be at least 8 bytes long; found " + data.Length + " bytes");
            return data[0] | ((ulong)data[1] << 8) | ((ulong)data[2] << 16) | ((ulong)data[3] << 24) | ((ulong)data[4] << 32) | ((ulong)data[5] << 40) | ((ulong)data[6] << 48) | ((ulong)data[7] << 56);
        }

        /// <summary>
        /// Decode a signed Little-Endian 64-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static long DecodeInt64(byte[] data)
        {
            if (data.Length < 8) throw new InvalidDataException("data should be at least 8 bytes long; found " + data.Length + " bytes");
            return (data[0]) | ((long)data[1] << 8) | ((long)data[2] << 16) | ((long)data[3] << 24) | ((long)data[4] << 32) | ((long)data[5] << 40) | ((long)data[6] << 48) | ((long)data[7] << 56);
        }

        /// <summary>
        /// Decode a signed Big-Endian 64-bit integer from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static long DecodeBEInt64(byte[] data)
        {
            if (data.Length < 8) throw new InvalidDataException("Data should be at least 8 bytes long; found " + data.Length + " bytes");
            return ((long)data[0] << 56) | ((long)data[1] << 48) | ((long)data[2] << 40) | ((long)data[3] << 32) | ((long)data[4] << 24) | ((long)data[5] << 16) | ((long)data[6] << 8) | ((long)data[7] << 0);
        }

        /// <summary>
        /// Encode the given value into an array of bytes as a Little-Endian unsigned 64-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeUInt64(ulong value)
        {
            return new byte[8] { (byte)(value & 0x00000000000000FF), (byte)((value & 0x000000000000FF00) >> 8), (byte)((value & 0x0000000000FF0000) >> 16), (byte)((value & 0x00000000FF000000) >> 24), (byte)((value & 0x000000FF00000000) >> 32), (byte)((value & 0x0000FF0000000000) >> 40), (byte)((value & 0x00FF000000000000) >> 48), (byte)((value & 0xFF00000000000000) >> 56) };
        }

        /// <summary>
        /// Encode the given value into an array of bytes as a Big-Endian unsigned 64-bits integer
        /// </summary>
        /// <param name="value">Value to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeBEUInt64(ulong value)
        {
            // Output has to be big-endian
            return new byte[8] { (byte)((value & 0xFF00000000000000) >> 56), (byte)((value & 0x00FF000000000000) >> 48), (byte)((value & 0x0000FF0000000000) >> 40), (byte)((value & 0x000000FF00000000) >> 32), (byte)((value & 0x00000000FF000000) >> 24), (byte)((value & 0x0000000000FF0000) >> 16), (byte)((value & 0x000000000000FF00) >> 8), (byte)(value & 0x00000000000000FF) };
        }

        /// <summary>
        /// Decode a signed Big-Endian 64-bit floating-point from the given array of bytes
        /// </summary>
        /// <param name="data">Array of bytes to read value from</param>
        /// <returns>Decoded value</returns>
        public static double DecodeBEDouble(byte[] data)
        {
            if (data.Length < 8) throw new InvalidDataException("Data should be at least 8 bytes long; found " + data.Length + " bytes");
            if (BitConverter.IsLittleEndian)
                Array.Reverse(data);

            return BitConverter.ToDouble(data, 0);
        }

        /// <summary>
        /// Switch the format of a signed Int32 between big endian and little endian
        /// </summary>
        /// <param name="n">value to convert</param>
        /// <returns>converted value</returns>
        public static int ReverseInt32(int n)
        {
            byte b0;
            byte b1;
            byte b2;
            byte b3;

            b0 = (byte)((n & 0x000000FF) >> 0);
            b1 = (byte)((n & 0x0000FF00) >> 8);
            b2 = (byte)((n & 0x00FF0000) >> 16);
            b3 = (byte)((n & 0xFF000000) >> 24);

            return (b0 << 24) | (b1 << 16) | (b2 << 8) | (b3 << 0);
        }

        /// <summary>
        /// Guess the encoding from the file Byte Order Mark (BOM)
        /// http://en.wikipedia.org/wiki/Byte_order_mark 
        /// NB : This obviously only works for files that actually start with a BOM
        /// </summary>
        /// <param name="file">FileStream to read from</param>
        /// <returns>Detected encoding; system Default if detection failed</returns>
        public static Encoding GetEncodingFromFileBOM(FileStream file)
        {
            Encoding result;
            byte[] bom = new byte[4]; // Get the byte-order mark, if there is one
            file.Read(bom, 0, 4);
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) // utf-8
            {
                result = Encoding.UTF8;
            }
            else if (bom[0] == 0xfe && bom[1] == 0xff) // utf-16 and ucs-2
            {
                result = Encoding.BigEndianUnicode;
            }
            else if (bom[0] == 0xff && bom[1] == 0xfe) // ucs-2le, ucs-4le, and ucs-16le
            {
                result = Encoding.Unicode;
            }
            else if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) // ucs-4 / UTF-32
            {
                result = Encoding.UTF32;
            }
            else
            {
                // There might be some cases where the Default encoding reads illegal characters
                // e.g. "ß" encoded in Windows-1250 gives an illegal character when read with Chinese-simplified (gb2312)
                result = Settings.DefaultTextEncoding;
            }

            // Now reposition the file cursor back to the start of the file
            file.Seek(0, SeekOrigin.Begin);
            return result;
        }

        /// <summary>
        /// Read a null-terminated String from the given BinaryReader, according to the given Encoding
        /// Returns with the BinaryReader positioned after the last null-character(s)
        /// </summary>
        /// <param name="r">BinaryReader positioned at the beginning of the String to be read</param>
        /// <param name="encoding">Encoding to use for reading the stream</param>
        /// <returns>Read value</returns>
        public static string ReadNullTerminatedString(BinaryReader r, Encoding encoding)
        {
            return readNullTerminatedString(r.BaseStream, encoding, 0, false);
        }
        /// <summary>
        /// Read a null-terminated String from the given Stream, according to the given Encoding
        /// Returns with the BinaryReader positioned after the last null-character(s)
        /// </summary>
        /// <param name="s">Stream positioned at the beginning of the String to be read</param>
        /// <param name="encoding">Encoding to use for reading the stream</param>
        /// <returns>Read value</returns>
        public static string ReadNullTerminatedString(Stream s, Encoding encoding)
        {
            return readNullTerminatedString(s, encoding, 0, false);
        }

        /// <summary>
        /// Read a null-terminated String from the given BinaryReader, according to the given Encoding, within a given limit of bytes
        /// Returns with the BinaryReader positioned at (start+limit)
        /// </summary>
        /// <param name="r">BinaryReader positioned at the beginning of the String to be read</param>
        /// <param name="encoding">Encoding to use for reading the stream</param>
        /// <param name="limit">Maximum number of bytes to read</param>
        /// <returns>Read value</returns>
        public static string ReadNullTerminatedStringFixed(BinaryReader r, Encoding encoding, int limit)
        {
            return readNullTerminatedString(r.BaseStream, encoding, limit, true);
        }

        /// <summary>
        /// Read a null-terminated String from the given BufferedBinaryReader, according to the given Encoding, within a given limit of bytes
        /// Returns with the BinaryReader positioned at (start+limit)
        /// </summary>
        /// <param name="r">BufferedBinaryReader positioned at the beginning of the String to be read</param>
        /// <param name="encoding">Encoding to use for reading the stream</param>
        /// <param name="limit">Maximum number of bytes to read</param>
        /// <returns>Read value</returns>
        public static string ReadNullTerminatedStringFixed(BufferedBinaryReader r, Encoding encoding, int limit)
        {
            return readNullTerminatedString(r, encoding, limit, true);
        }

        /// <summary>
        /// Read a null-terminated string using the giver BinaryReader
        /// </summary>
        /// <param name="r">Stream reader to read the string from</param>
        /// <param name="encoding">Encoding to use to parse the read bytes into the resulting String</param>
        /// <param name="limit">Limit (in bytes) of read data (0=unlimited)</param>
        /// <param name="moveStreamToLimit">Indicates if the stream has to advance to the limit before returning</param>
        /// <returns>The string read, without the zeroes at its end</returns>
        private static string readNullTerminatedString(Stream r, Encoding encoding, int limit, bool moveStreamToLimit)
        {
            int nbChars = (encoding.Equals(Encoding.BigEndianUnicode) || encoding.Equals(Encoding.Unicode)) ? 2 : 1;
            byte[] readBytes = new byte[limit > 0 ? limit : 100];
            byte[] buffer = new byte[2];
            int nbRead = 0;
            long streamLength = r.Length;
            long initialPos = r.Position;
            long streamPos = initialPos;

            while (streamPos < streamLength && ((0 == limit) || (nbRead < limit)))
            {
                // Read the size of a character
                r.Read(buffer, 0, nbChars);

                if ((1 == nbChars) && (0 == buffer[0])) // Null character read for single-char encodings
                {
                    break;
                }
                else if ((2 == nbChars) && (0 == buffer[0]) && (0 == buffer[1])) // Null character read for two-char encodings
                {
                    break;
                }
                else // All clear; store the read char in the byte array
                {
                    if (readBytes.Length < nbRead + nbChars) Array.Resize<byte>(ref readBytes, readBytes.Length + 100);

                    readBytes[nbRead] = buffer[0];
                    if (2 == nbChars) readBytes[nbRead + 1] = buffer[1];
                    nbRead += nbChars;
                    streamPos += nbChars;
                }
            }

            if (moveStreamToLimit) r.Seek(initialPos + limit, SeekOrigin.Begin);

            return encoding.GetString(readBytes, 0, nbRead);
        }

        /// <summary>
        /// Extract a signed 32-bit integer from a byte array using the "synch-safe" convention
        /// as to ID3v2 definition (§6.2)
        /// </summary>
        /// <param name="bytes">Byte array containing data
        /// NB : Array size can vary from 1 to 5 bytes, as only 7 bits of each is actually used
        /// </param>
        /// <returns>Decoded Int32</returns>
        public static int DecodeSynchSafeInt(byte[] bytes)
        {
            if (bytes.Length > 5) throw new ArgumentException("Array too long : has to be 1 to 5 bytes; found : " + bytes.Length + " bytes");
            int result = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                result += bytes[i] * (int)Math.Floor(Math.Pow(2, (7 * (bytes.Length - 1 - i))));
            }
            return result;
        }

        /// <summary>
        /// Decode a signed 32-bit integer from a 4-byte array using the "synch-safe" convention
        /// as to ID3v2 definition (§6.2)
        /// NB : The actual capacity of the integer thus reaches 28 bits
        /// </summary>
        /// <param name="data">4-byte array containing to convert</param>
        /// <returns>Decoded Int32</returns>
        public static int DecodeSynchSafeInt32(byte[] data)
        {
            if (data.Length < 4) throw new ArgumentException("Array length has to be at least 4 bytes; found : " + data.Length + " bytes");

            return
                data[0] * 0x200000 +   //2^21
                data[1] * 0x4000 +     //2^14
                data[2] * 0x80 +       //2^7
                data[3];
        }

        /// <summary>
        /// Encode the given values as a (nbBytes*8)-bit integer to a (nbBytes)-byte array using the "synch-safe" convention
        /// as to ID3v2 definition (§6.2)
        /// </summary>
        /// <param name="value">Value to encode</param>
        /// <param name="nbBytes">Number of bytes to encode to (can be 1 to 5)</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeSynchSafeInt(int value, int nbBytes)
        {
            if ((nbBytes < 1) || (nbBytes > 5)) throw new ArgumentException("nbBytes has to be 1 to 5; found : " + nbBytes);
            byte[] result = new byte[nbBytes];
            int range;

            for (int i = 0; i < nbBytes; i++)
            {
                range = (7 * (nbBytes - 1 - i));
                result[i] = (byte)((value & (0x7F << range)) >> range);
            }

            return result;
        }

        /// <summary>
        /// Encode the given value as a 32-bit integer to a 4-byte array using the "synch-safe" convention
        /// as to ID3v2 definition (§6.2)
        /// </summary>
        /// <param name="value">Integer to be encoded</param>
        /// <returns>Encoded array of bytes</returns>
        public static byte[] EncodeSynchSafeInt32(int value)
        {
            byte[] result = new byte[4];
            result[0] = (byte)((value & 0xFE00000) >> 21);
            result[1] = (byte)((value & 0x01FC000) >> 14);
            result[2] = (byte)((value & 0x0003F80) >> 7);
            result[3] = (byte)(value & 0x000007F);

            return result;
        }


        /// <summary>
        /// Find a byte sequence within a stream
        /// </summary>
        /// <param name="stream">Stream to search into</param>
        /// <param name="sequence">Sequence to find</param>
        /// <param name="limit">Maximum distance (in bytes) of the sequence to find.
        /// Put 0 for an infinite distance</param>
        /// <returns>
        ///     true if the sequence has been found; the stream will be positioned on the 1st byte following the sequence
        ///     false if the sequence has not been found; the stream will keep its initial position
        /// </returns>
        public static bool FindSequence(Stream stream, byte[] sequence, long limit = 0)
        {
            int BUFFER_SIZE = 512;
            byte[] readBuffer = new byte[BUFFER_SIZE];

            int remainingBytes, bytesToRead;
            int iSequence = 0;
            int readBytes = 0;
            long initialPos = stream.Position;

            remainingBytes = (int)((limit > 0) ? Math.Min(stream.Length - stream.Position, limit) : stream.Length - stream.Position);

            while (remainingBytes > 0)
            {
                bytesToRead = Math.Min(remainingBytes, BUFFER_SIZE);

                stream.Read(readBuffer, 0, bytesToRead);

                for (int i = 0; i < bytesToRead; i++)
                {
                    if (sequence[iSequence] == readBuffer[i]) iSequence++;
                    else if (iSequence > 0) iSequence = 0;

                    if (sequence.Length == iSequence)
                    {
                        stream.Position = initialPos + readBytes + i + 1;
                        return true;
                    }
                }

                remainingBytes -= bytesToRead;
                readBytes += bytesToRead;
            }

            // If we're here, the sequence hasn't been found
            stream.Position = initialPos;
            return false;
        }

        /// <summary>
        /// Read the given number of bits from the given position and converts it to an unsigned int32
        /// according to big-endian convention
        /// 
        /// NB : reader position _always_ progresses by 4, no matter how many bits are needed
        /// </summary>
        /// <param name="source">Stream to read the data from</param>
        /// <param name="bitPosition">Position of the first _bit_ to read (scale is x8 compared to classic byte positioning) </param>
        /// <param name="bitCount">Number of bits to read</param>
        /// <returns>Unsigned int32 formed from read bits, according to big-endian convention</returns>
        public static uint ReadBEBits(Stream source, int bitPosition, int bitCount)
        {
            if (bitCount < 1 || bitCount > 32) throw new NotSupportedException("Bit count must be between 1 and 32");
            byte[] buffer = new byte[4];

            // Read a number of bits from file at the given position
            source.Seek(bitPosition / 8, SeekOrigin.Begin); // integer division =^ div
            source.Read(buffer, 0, buffer.Length);
            uint result = (uint)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3]);
            result = (result << (bitPosition % 8)) >> (32 - bitCount);

            return result;
        }

        public static void WriteBytes(Stream s, byte[] data)
        {
            s.Write(data, 0, data.Length);
        }

        public static void WriteUInt16(Stream s, ushort data)
        {
            s.Write(EncodeUInt16(data), 0, 2);
        }

        public static void WriteInt32(Stream s, int data)
        {
            s.Write(EncodeInt32(data), 0, 4);
        }

        public static void WriteUInt32(Stream s, uint data)
        {
            s.Write(EncodeUInt32(data), 0, 4);
        }

        public static void WriteBEInt32(Stream s, int data)
        {
            s.Write(EncodeBEInt32(data), 0, 4);
        }

        public static void WriteBEUInt32(Stream s, uint data)
        {
            s.Write(EncodeBEUInt32(data), 0, 4);
        }

        public static void WriteUInt64(Stream s, ulong data)
        {
            s.Write(EncodeUInt64(data), 0, 8);
        }


        /// <summary>
        /// Convert the given extended-format byte array (which is assumed to be in little-endian form) to a .NET Double,
        /// as closely as possible.Values which are too small to be
        /// represented are returned as an appropriately signed 0. 
        /// Values which are too large to be represented (but not infinite) 
        /// are returned as Double.NaN, as are unsupported values and actual NaN values.
        /// 
        /// Credits : Jon Skeet (http://groups.google.com/groups?selm=MPG.19a6985d4683f5d398a313%40news.microsoft.com)
        /// </summary>
        /// <param name="extended">Extended data to be converted</param>
        /// <returns>Converted value, or Double.NaN if unsupported or NaN</returns>
        public static double ExtendedToDouble(byte[] extended)
        {
            // Read information from the extended form - variable names as 
            // used in http://cch.loria.fr/documentation/
            // IEEE754/numerical_comp_guide/ncg_math.doc.html
            int s = extended[9] >> 7;

            int e = (((extended[9] & 0x7f) << 8) | (extended[8]));

            int j = extended[7] >> 7;
            long f = extended[7] & 0x7f;
            for (int i = 6; i >= 0; i--)
            {
                f = (f << 8) | extended[i];
            }

            // Now go through each possibility
            if (j == 1)
            {
                if (e == 0)
                {
                    // Value = (-1)^s * 2^16382*1.f 
                    // (Pseudo-denormal numbers)
                    // Anything pseudo-denormal in extended form is 
                    // definitely 0 in double form.
                    return FromComponents(s, 0, 0);
                }
                else if (e != 32767)
                {
                    // Value = (-1)^s * 2^(e-16383)*1.f  (Normal numbers)

                    // Lose the last 11 bits of the fractional part
                    f = f >> 11;

                    // Convert exponent to the appropriate one
                    e += 1023 - 16383;

                    // Out of range - too large
                    if (e > 2047)
                        return Double.NaN;

                    // Out of range - too small
                    if (e < 0)
                    {
                        // See if we can get a subnormal version
                        if (e >= -51)
                        {
                            // Put a 1 at the front of f
                            f = f | (1 << 52);
                            // Now shift it appropriately
                            f = f >> (1 - e);
                            // Return a subnormal version
                            return FromComponents(s, 0, f);
                        }
                        else // Return an appropriate 0
                        {
                            return FromComponents(s, 0, 0);
                        }
                    }

                    return FromComponents(s, e, f);
                }
                else
                {
                    if (f == 0)
                    {
                        // Value = positive/negative infinity
                        return FromComponents(s, 2047, 0);
                    }
                    else
                    {
                        // Don't really understand the document about the 
                        // remaining two values, but they're both NaN...
                        return Double.NaN;
                    }
                }
            }
            else // Okay, j==0
            {
                if (e == 0)
                {
                    // Either 0 or a subnormal number, which will 
                    // still be 0 in double form
                    return FromComponents(s, 0, 0);
                }
                else
                {
                    // Unsupported
                    return Double.NaN;
                }
            }
        }

        /// <summary>
        /// Returns a double from the IEEE sign/exponent/fraction components
        /// 
        /// Credits : Jon Skeet (http://groups.google.com/groups?selm=MPG.19a6985d4683f5d398a313%40news.microsoft.com)
        /// </summary>
        /// <param name="s">IEEE sign component</param>
        /// <param name="e">IEEE exponent component</param>
        /// <param name="f">IEEE fraction component</param>
        /// <returns>Converted double</returns>
        private static double FromComponents(int s, int e, long f)
        {
            byte[] data = new byte[8];

            // Put the data into appropriate slots based on endianness.
            if (BitConverter.IsLittleEndian)
            {
                data[7] = (byte)((s << 7) | (e >> 4));
                data[6] = (byte)(((e & 0xf) << 4) | (int)(f >> 48));
                data[5] = (byte)((f & 0xff0000000000L) >> 40);
                data[4] = (byte)((f & 0xff00000000L) >> 32);
                data[3] = (byte)((f & 0xff000000L) >> 24);
                data[2] = (byte)((f & 0xff0000L) >> 16);
                data[1] = (byte)((f & 0xff00L) >> 8);
                data[0] = (byte)(f & 0xff);
            }
            else
            {
                data[0] = (byte)((s << 7) | (e >> 4));
                data[1] = (byte)(((e & 0xf) << 4) | (int)(f >> 48));
                data[2] = (byte)((f & 0xff0000000000L) >> 40);
                data[3] = (byte)((f & 0xff00000000L) >> 32);
                data[4] = (byte)((f & 0xff000000L) >> 24);
                data[5] = (byte)((f & 0xff0000L) >> 16);
                data[6] = (byte)((f & 0xff00L) >> 8);
                data[7] = (byte)(f & 0xff);
            }

            return BitConverter.ToDouble(data, 0);
        }

        /// <summary>
        /// Advance the given Stream until a new value is encountered.
        /// NB : A series of successive identical bytes is called "padding zone"
        /// 
        /// Warning : There is no contract regarding the position within the stream at the end of the call.
        /// It might be anywhere around the end of the padding zone.
        /// </summary>
        /// <param name="source">Stream to advance, positioned at the beginning of a padding zone</param>
        /// <returns>Absolute offset of the end of the padding zone within the given Stream</returns>
        public static long TraversePadding(Stream source)
        {
            byte[] data = new byte[Settings.FileBufferSize];
            long initialPos = source.Position;
            int read, readTotal = 0;
            int lastValue = -1;

            read = source.Read(data, 0, Settings.FileBufferSize);
            while (read > 0)
            {
                for (int i = 0; i < read; i++)
                {
                    if (-1 == lastValue) lastValue = data[i];
                    if (data[i] != lastValue) return initialPos + readTotal + i;
                }
                readTotal += read;
                read = source.Read(data, 0, Settings.FileBufferSize);
            }

            return initialPos + readTotal;
        }

        /// <summary>
        /// Skip the given bytes in the given Stream, starting from its current position, in forward direction
        /// Returns with the given Stream positioned on the first non-skipped byte
        /// </summary>
        /// <param name="source">Stream to browse</param>
        /// <param name="dataToSkip">Value of the bytes to skip</param>
        /// <returns>Number of bytes skipped</returns>
        public static int SkipValues(Stream source, int[] dataToSkip)
        {
            int b;
            int nbBytes = -1;
            do
            {
                b = source.ReadByte();
                nbBytes++;
            }
            while (dataToSkip.Contains(b));
            source.Position = source.Position - 1;
            return nbBytes;
        }

        /// <summary>
        /// Skip the given bytes in the given Stream, starting from its current position, in reverse direction
        /// Returns with the given Stream positioned on the first non-skipped byte
        /// </summary>
        /// <param name="source">Stream to browse</param>
        /// <param name="dataToSkip">Value of the bytes to skip</param>
        /// <returns>Number of bytes skipped</returns>
        public static int SkipValuesEnd(Stream source, int[] dataToSkip)
        {
            int b;
            int nbBytes = -1;
            do
            {
                nbBytes++;
                source.Seek(-nbBytes - 1, SeekOrigin.Current);
                b = source.ReadByte();
            }
            while (dataToSkip.Contains(b));
            source.Position = source.Position + 1;
            return nbBytes;
        }
    }
}
