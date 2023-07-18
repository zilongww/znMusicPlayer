﻿using System;
using System.IO;
using System.Text;
using static ATL.ChannelsArrangements;
using System.Collections.Generic;
using System.Globalization;
using static ATL.TagData;
using System.Threading.Tasks;
using static ATL.AudioData.FileStructureHelper;
using System.Linq;
using Commons;

namespace ATL.AudioData.IO
{
    /// <summary>
    /// Class for Audible Formats 2 to 4 files manipulation (extensions : .AA)
    /// 
    /// Implementation notes
    /// 
    ///   - Only the editing of existing zones has been tested, not the adding of new zones (e.g. tagging a tagless AA, adding a picture to a pictureless AA)
    ///   due to the lack of empty test files
    ///   
    /// </summary>
	class AA : MetaDataIO, IAudioDataIO
    {

        public const int AA_MAGIC_NUMBER = 1469084982;

        public const int TOC_HEADER_TERMINATOR = 1;
        public const int TOC_CONTENT_TAGS = 2;
        public const int TOC_AUDIO = 10;
        public const int TOC_COVER_ART = 11;

        public const string CODEC_MP332 = "mp332";
        public const string CODEC_ACELP85 = "acelp85";
        public const string CODEC_ACELP16 = "acelp16";

        public const string ZONE_TOC = "toc";
        public const string ZONE_TAGS = "2";
        public const string ZONE_IMAGE = "11";


        // Mapping between MP4 frame codes and ATL frame codes
        private static Dictionary<string, Field> frameMapping = new Dictionary<string, Field>() {
            { "title", Field.TITLE },
            { "parent_title", Field.ALBUM},
            { "narrator", Field.ARTIST },
            { "description", Field.COMMENT},
            { "pubdate", Field.PUBLISHING_DATE},
            { "provider", Field.PUBLISHER},
            { "author", Field.COMPOSER },
            { "long_description", Field.GENERAL_DESCRIPTION},
            { "copyright", Field.COPYRIGHT },
        };


        private string codec;
        private long tocOffset;
        private long tocSize;

        private readonly string fileName;
        private readonly Format audioFormat;

        private IDictionary<int, TocEntry> toc;

        private sealed class TocEntry
        {
            public readonly long TocOffset;
            public readonly int Section;
            public readonly uint Offset;
            public readonly uint Size;

            public TocEntry(long tocOffset, int section, uint offset, uint size)
            {
                TocOffset = tocOffset;
                Section = section;
                Offset = offset;
                Size = size;
            }

            public override string ToString()
            {
                return "[" + Section + "] @" + Offset + " (" + Size + ")";
            }
        }


        // ---------- INFORMATIVE INTERFACE IMPLEMENTATIONS & MANDATORY OVERRIDES

        // IAudioDataIO
        public bool IsVBR
        {
            get { return false; }
        }
        public Format AudioFormat
        {
            get
            {
                Format f = new Format(audioFormat);
                if (codec.Length > 0)
                    f.Name = f.Name + " (" + codec + ")";
                else
                    f.Name = f.Name + " (Unknown)";
                return f;
            }
        }
        public int CodecFamily
        {
            get { return AudioDataIOFactory.CF_LOSSY; }
        }
        public double BitRate
        {
            get
            {
                switch (codec)
                {
                    case CODEC_MP332:
                        return 32 / 8.0;
                    case CODEC_ACELP16:
                        return 16 / 8.0;
                    case CODEC_ACELP85:
                        return 8.5 / 8.0;
                    default:
                        return 1;
                }
            }
        }
        public double Duration
        {
            get { return getDuration(); }
        }
        public int SampleRate
        {
            get
            {
                switch (codec)
                {
                    case CODEC_MP332:
                        return 22050;
                    case CODEC_ACELP16:
                        return 16000;
                    case CODEC_ACELP85:
                        return 8500;
                    default:
                        return 1;
                }
            }
        }

        public int BitDepth => -1; // Irrelevant for lossy formats

        public string FileName
        {
            get { return fileName; }
        }
        public bool IsMetaSupported(MetaDataIOFactory.TagType metaDataType)
        {
            return metaDataType == MetaDataIOFactory.TagType.NATIVE;
        }
        public ChannelsArrangement ChannelsArrangement
        {
            get { return MONO; }
        }

        // MetaDataIO
        protected override int getDefaultTagOffset()
        {
            return TO_BUILTIN;
        }

        protected override MetaDataIOFactory.TagType getImplementedTagType()
        {
            return MetaDataIOFactory.TagType.NATIVE;
        }

        protected override Field getFrameMapping(string zone, string ID, byte tagVersion)
        {
            Field supportedMetaId = Field.NO_FIELD;

            if (frameMapping.ContainsKey(ID)) supportedMetaId = frameMapping[ID];

            return supportedMetaId;
        }
        protected override bool isLittleEndian
        {
            get { return false; }
        }
        public override string EncodeDate(DateTime date)
        {
            return date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture).ToUpper();
        }

        public long AudioDataOffset { get; set; }

        public long AudioDataSize { get; set; }


        // ---------- CONSTRUCTORS & INITIALIZERS

        protected void resetData()
        {
            codec = "";
            tocOffset = 0;
            tocSize = 0;
            if (toc != null) toc.Clear();
            AudioDataOffset = -1;
            AudioDataSize = 0;
        }

        public AA(string fileName, Format format)
        {
            this.fileName = fileName;
            audioFormat = format;
            resetData();
        }

        public static bool IsValidHeader(byte[] data)
        {
            // Bytes 4 to 7
            byte[] usefulData = new byte[4];
            Array.Copy(data, 4, usefulData, 0, 4);

            return AA_MAGIC_NUMBER == StreamUtils.DecodeBEInt32(usefulData);
        }

        // ********************** Private functions & procedures *********************

        // Calculate duration time
        private double getDuration()
        {
            if (0 == BitRate)
                return 0;
            else
                return AudioDataSize / (BitRate * 1000);
        }

        // Read header data
        private bool readHeader(BufferedBinaryReader source)
        {
            byte[] buffer = new byte[8];
            source.Read(buffer, 0, buffer.Length);
            if (!IsValidHeader(buffer)) return false;

            uint fileSize = StreamUtils.DecodeBEUInt32(buffer);

            tagExists = true;
            AudioDataOffset = source.Position - 4;
            tocOffset = source.Position;
            toc = readToc(source);
            tocSize = source.Position - tocOffset;

            foreach (var entry in toc)
            {
                structureHelper.AddZone(entry.Value.Offset, (int)entry.Value.Size, entry.Key.ToString(), isSectionDeletable(entry.Key));
                structureHelper.AddIndex(entry.Value.TocOffset + 4, entry.Value.Offset, false, entry.Key.ToString());
                structureHelper.AddSize(entry.Value.TocOffset + 8, entry.Value.Size, entry.Key.ToString());
                if (TOC_AUDIO == entry.Key)
                {
                    AudioDataOffset = entry.Value.Offset;
                    AudioDataSize = entry.Value.Size;
                }
                if (TOC_CONTENT_TAGS == entry.Key)
                {
                    structureHelper.AddSize(0, fileSize, entry.Key.ToString());
                }
                if (TOC_COVER_ART == entry.Key)
                {
                    structureHelper.AddSize(0, fileSize, entry.Key.ToString());
                }
            }

            // Save TOC as a zone for future editing
            structureHelper.AddZone(tocOffset, tocSize, ZONE_TOC, false);
            structureHelper.AddSize(0, fileSize, ZONE_TOC);

            return true;
        }

        // The table of contents describes the layout of the file as triples of integers (<section>, <offset>, <length>)
        private IDictionary<int, TocEntry> readToc(BufferedBinaryReader s)
        {
            IDictionary<int, TocEntry> result = new Dictionary<int, TocEntry>();
            int nbTocEntries = StreamUtils.DecodeBEInt32(s.ReadBytes(4));
            s.Seek(4, SeekOrigin.Current); // Even FFMPeg doesn't know what this integer is
            for (int i = 0; i < nbTocEntries; i++)
            {
                long offset = s.Position;
                int section = StreamUtils.DecodeBEInt32(s.ReadBytes(4));
                uint tocEntryOffset = StreamUtils.DecodeBEUInt32(s.ReadBytes(4));
                uint tocEntrySize = StreamUtils.DecodeBEUInt32(s.ReadBytes(4));
                result[section] = new TocEntry(offset, section, tocEntryOffset, tocEntrySize);
            }
            return result;
        }

        private static bool isSectionDeletable(int sectionId)
        {
            return TOC_CONTENT_TAGS == sectionId || TOC_COVER_ART == sectionId;
        }

        private void readTags(BufferedBinaryReader source, long offset, ReadTagParams readTagParams)
        {
            source.Seek(offset, SeekOrigin.Begin);
            int nbTags = StreamUtils.DecodeBEInt32(source.ReadBytes(4));
            for (int i = 0; i < nbTags; i++)
            {
                source.Seek(1, SeekOrigin.Current); // No idea what this byte is
                int keyLength = StreamUtils.DecodeBEInt32(source.ReadBytes(4));
                int valueLength = StreamUtils.DecodeBEInt32(source.ReadBytes(4));
                string key = Encoding.UTF8.GetString(source.ReadBytes(keyLength));
                string value = Encoding.UTF8.GetString(source.ReadBytes(valueLength)).Trim();
                SetMetaField(key, value, readTagParams.ReadAllMetaFrames);
                if ("codec".Equals(key)) codec = value;
            }
        }

        private void readCover(BufferedBinaryReader source, long offset, PictureInfo.PIC_TYPE pictureType)
        {
            source.Seek(offset, SeekOrigin.Begin);
            int pictureSize = StreamUtils.DecodeBEInt32(source.ReadBytes(4));
            int picOffset = StreamUtils.DecodeBEInt32(source.ReadBytes(4));
            structureHelper.AddIndex(source.Position - 4, (uint)picOffset, false, ZONE_IMAGE);
            source.Seek(picOffset, SeekOrigin.Begin);

            PictureInfo picInfo = PictureInfo.fromBinaryData(source, pictureSize, pictureType, getImplementedTagType(), TOC_COVER_ART);
            tagData.Pictures.Add(picInfo);
        }

        private void readChapters(BufferedBinaryReader source, long offset, long size)
        {
            source.Seek(offset, SeekOrigin.Begin);
            if (null == tagData.Chapters) tagData.Chapters = new List<ChapterInfo>(); else tagData.Chapters.Clear();
            double cumulatedDuration = 0;
            int idx = 1;
            while (source.Position < offset + size)
            {
                uint chapterSize = StreamUtils.DecodeBEUInt32(source.ReadBytes(4));
                uint chapterOffset = StreamUtils.DecodeBEUInt32(source.ReadBytes(4));
                structureHelper.AddZone(chapterOffset, (int)chapterSize, "chp" + idx, false); // AA chapters are embedded into the audio chunk; they are _not_ deletable
                structureHelper.AddIndex(source.Position - 4, chapterOffset, false, "chp" + idx);

                ChapterInfo chapter = new ChapterInfo();
                chapter.Title = "Chapter " + idx++; // Chapters have no title metatada in the AA format
                chapter.StartTime = (uint)Math.Round(cumulatedDuration);
                cumulatedDuration += chapterSize / (BitRate * 1000);
                chapter.EndTime = (uint)Math.Round(cumulatedDuration);
                tagData.Chapters.Add(chapter);

                source.Seek(chapterSize, SeekOrigin.Current);
            }
        }

        // Read data from file
        public bool Read(Stream source, AudioDataManager.SizeInfo sizeInfo, ReadTagParams readTagParams)
        {
            return read(source, readTagParams);
        }

        protected override bool read(Stream source, ReadTagParams readTagParams)
        {
            BufferedBinaryReader reader = new BufferedBinaryReader(source);
            ResetData();
            if (!readHeader(reader)) return false;
            if (toc.ContainsKey(TOC_CONTENT_TAGS))
            {
                readTags(reader, toc[TOC_CONTENT_TAGS].Offset, readTagParams);
            }
            if (toc.ContainsKey(TOC_COVER_ART) && readTagParams.ReadPictures)
            {
                readCover(reader, toc[TOC_COVER_ART].Offset, PictureInfo.PIC_TYPE.Generic);
            }
            readChapters(reader, toc[TOC_AUDIO].Offset, toc[TOC_AUDIO].Size);

            return true;
        }

        protected override int write(TagData tag, Stream s, string zone)
        {
            int result = -1; // Default : leave as is
            byte[] intBuffer;

            if (zone.Equals(ZONE_TAGS))
            {
                long nbTagsOffset = s.Position;
                intBuffer = StreamUtils.EncodeInt32(0);
                s.Write(intBuffer, 0, 4); // Number of tags; will be rewritten at the end of the method

                // Mapped textual fields
                IDictionary<Field, string> map = tag.ToMap();
                foreach (Field frameType in map.Keys)
                {
                    if (map[frameType].Length > 0) // No frame with empty value
                    {
                        foreach (string str in frameMapping.Keys)
                        {
                            if (frameType == frameMapping[str])
                            {
                                string value = formatBeforeWriting(frameType, tag, map);
                                writeTagField(s, str, value);
                                result++;
                                break;
                            }
                        }
                    }
                }

                // Other textual fields
                foreach (MetaFieldInfo fieldInfo in tag.AdditionalFields)
                {
                    if ((fieldInfo.TagType.Equals(MetaDataIOFactory.TagType.ANY) || fieldInfo.TagType.Equals(getImplementedTagType())) && !fieldInfo.MarkedForDeletion)
                    {
                        writeTagField(s, fieldInfo.NativeFieldCode, FormatBeforeWriting(fieldInfo.Value));
                        result++;
                    }
                }

                s.Seek(nbTagsOffset, SeekOrigin.Begin);
                intBuffer = StreamUtils.EncodeBEInt32(result);
                s.Write(intBuffer, 0, 4); // Number of tags
            }
            if (zone.Equals(ZONE_IMAGE))
            {
                result = 0;
                foreach (PictureInfo picInfo in tag.Pictures)
                {
                    // Picture has either to be supported, or to come from the right tag standard
                    bool doWritePicture = !picInfo.PicType.Equals(PictureInfo.PIC_TYPE.Unsupported);
                    if (!doWritePicture) doWritePicture = (getImplementedTagType() == picInfo.TagType);
                    // It also has not to be marked for deletion
                    doWritePicture = doWritePicture && (!picInfo.MarkedForDeletion);

                    if (doWritePicture)
                    {
                        writePictureFrame(s, picInfo.PictureData);
                        return 1; // Stop here; there can only be one picture in an AA file
                    }
                }
            }

            return result;
        }

        private void writeTagField(Stream s, string key, string value)
        {
            s.WriteByte(0); // Unknown byte; always zero
            byte[] keyB = Encoding.UTF8.GetBytes(key);
            byte[] valueB = Encoding.UTF8.GetBytes(value);
            StreamUtils.WriteBEInt32(s, keyB.Length); // Key length
            StreamUtils.WriteBEInt32(s, valueB.Length); // Value length
            StreamUtils.WriteBytes(s, keyB);
            StreamUtils.WriteBytes(s, valueB);
        }

        private void writePictureFrame(Stream s, byte[] pictureData)
        {
            StreamUtils.WriteBEInt32(s, pictureData.Length); // Pic size
            StreamUtils.WriteInt32(s, 0); // Pic data absolute offset; to be rewritten later
            StreamUtils.WriteBytes(s, pictureData);
        }

        // Specific implementation for rewriting of the TOC after zone removal
        public override bool Remove(Stream s)
        {
            bool result = base.Remove(s);
            if (result)
            {
                int newTocSize = writeCoreToc(s);
                finalizeFile(s, newTocSize);
            }
            return result;
        }

        // Specific implementation for rewriting of the TOC after zone removal
        public override async Task<bool> RemoveAsync(Stream s)
        {
            bool result = await base.RemoveAsync(s);
            if (result)
            {
                int newTocSize = writeCoreToc(s);
                await finalizeFileAsync(s, newTocSize);
            }
            return result;
        }

        private int writeCoreToc(Stream s)
        {
            s.Seek(tocOffset, SeekOrigin.Begin);
            BufferedBinaryReader br = new BufferedBinaryReader(s);

            IDictionary<int, TocEntry> newToc = readToc(br);
            List<TocEntry> finalToc = newToc.Values.Where(e => !isSectionDeletable(e.Section)).ToList();
            int deltaBytes = (newToc.Count - finalToc.Count) * 12;
            s.Seek(tocOffset, SeekOrigin.Begin);
            StreamUtils.WriteBEInt32(s, finalToc.Count);
            s.Seek(4, SeekOrigin.Current); // Skip unfathomable byte
                                           // Rewrite table of contents (<section>, <offset>, <length>)
            foreach (TocEntry entry in finalToc)
            {
                StreamUtils.WriteBEInt32(s, entry.Section);
                StreamUtils.WriteBEUInt32(s, entry.Offset);
                StreamUtils.WriteBEUInt32(s, entry.Size);
            }
            int newTocSize = (int)(s.Position - tocOffset);
            // Process TOC resizing
            structureHelper.RewriteHeaders(s, null, -deltaBytes, ACTION.Edit, ZONE_TOC);
            return newTocSize;
        }

        // Remove unused data
        private void finalizeFile(Stream s, long newTocSize)
        {
            StreamUtils.ShortenStream(s, tocOffset + tocSize, (uint)(tocSize - newTocSize));
        }

        private async Task finalizeFileAsync(Stream s, long newTocSize)
        {
            await StreamUtilsAsync.ShortenStreamAsync(s, tocOffset + tocSize, (uint)(tocSize - newTocSize));
        }
    }
}