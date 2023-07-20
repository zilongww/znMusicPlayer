using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.UI.Xaml.Media;
using ATL.CatalogDataReaders;
using System.Formats.Tar;
using Newtonsoft.Json;
using ATL;

namespace znMusicPlayerWUI.DataEditor
{
    public static class PlayListHelper
    {
        public static async Task<MusicListData[]> ReadAllPlayList()
        {
            var datas = new List<MusicListData>();
            await Task.Run(() =>
            {
                JObject jdatas = JObject.Parse(File.ReadAllText(DataFolderBase.PlayListDataPath));
                foreach (var list in jdatas)
                {
                    var a = JsonNewtonsoft.FromJSON<MusicListData>(list.Value.ToString());
                    datas.Add(a);
                }
            });
            return datas.ToArray();
        }

        public static async Task AddPlayList(MusicListData musicListData)
        {
            var jdata = await ReadData();
            jdata.Add(musicListData.ListName, JObject.FromObject(musicListData));
            await SaveData(jdata);
        }

        public static JObject AddPlayList(MusicListData musicListData, JObject addData)
        {
            addData.Add(musicListData.ListName, JObject.FromObject(musicListData));
            return addData;
        }

        public static async Task DeletePlayList(MusicListData musicListData)
        {
            var jdata = await ReadData();
            jdata.Remove(musicListData.ListName);
            await SaveData(jdata);
        }

        public static JObject AddMusicDataToPlayList(string listName, MusicData musicData, JObject jdata)
        {
            var ml = JsonNewtonsoft.FromJSON<MusicListData>(jdata[listName].ToString());

            if (ml.Songs == null)
            {
                ml.Songs = new();
            }

            // List.Contains不起作用
            bool contains = false;
            foreach (var i in ml.Songs)
            {
                if (i == musicData)
                {
                    contains = true;
                }
            }
            if (!contains)
            {
                ml.Songs.Insert(0, musicData);
                jdata[listName] = JObject.FromObject(ml);
            }

            return jdata;
        }

        public static async Task AddMusicDataToPlayList(string listName, MusicData musicData)
        {
            JObject text = await ReadData();
            await SaveData(AddMusicDataToPlayList(listName, musicData, text));
        }

        public static JObject DeleteMusicDataFromPlayList(string listName, MusicData musicData, JObject jdata)
        {
            var ml = JsonNewtonsoft.FromJSON<MusicListData>(jdata[listName].ToString());
            for (int mc = 0; mc < ml.Songs.Count; mc++)
            {
                if (ml.Songs[mc] == musicData)
                {
                    System.Diagnostics.Debug.WriteLine(musicData.ID);
                    ml.Songs.RemoveAt(mc);
                    break;
                }
            }
            jdata[listName] = JObject.FromObject(ml);
            return jdata;
        }

        public static async Task DeleteMusicDataFromPlayList(string listName, MusicData musicData)
        {
            await SaveData(DeleteMusicDataFromPlayList(listName, musicData, await ReadData()));
        }

        public static JObject MackJsPlayListData(MusicListData musicListData)
        {
            return new JObject() { { musicListData.ListName, JObject.FromObject(musicListData) } };
        }

        public static async Task SaveData(JObject data)
        {
            await Task.Run(() =>
            {
                File.WriteAllText(DataFolderBase.PlayListDataPath, data.ToString());
            });
        }

        public static async Task<JObject> ReadData()
        {
            return await Task.Run(() =>
            {
                return JObject.Parse(File.ReadAllText(DataFolderBase.PlayListDataPath));
            });
        }

        public static async Task<JObject> AddLocalMusicDataToPlayList(string listName, FileInfo localFile, JObject jdata)
        {
            if (localFile.Extension == ".cue")
            {
                await Task.Run(() =>
                {
                    CueSharp.CueSheet cueSheet = new CueSharp.CueSheet(localFile.FullName);
                    string path = $"{localFile.DirectoryName}\\{cueSheet.Tracks.First().DataFile.Filename}";
                    TimeSpan duration = default;/*
                    try
                    {
                        var tagFile = TagLib.File.Create(path);
                        duration = tagFile.Properties.Duration;
                        tagFile.Dispose();
                    }
                    catch
                    {*/
                        var track = new ATL.Track(path);
                        duration = TimeSpan.FromMilliseconds(track.DurationMs);
                    //}

                    List<MusicData> data = new List<MusicData>();
                    foreach (var t in cueSheet.Tracks)
                    {
                        //开始的时间
                        CueSharp.Index startIndex = t.Indices.Last();
                        TimeSpan startTime = new(0, 0, startIndex.Minutes, startIndex.Seconds, startIndex.Frames * 10);

                        //结束的时间
                        int endCount = t.TrackNumber;
                        CueSharp.Index endIndex = default;
                        TimeSpan endTime = default;
                        if (endCount < cueSheet.Tracks.Length)
                        {
                            endIndex = cueSheet.Tracks[t.TrackNumber].Indices.Last();
                            endTime = new(0, 0, endIndex.Minutes, endIndex.Seconds, endIndex.Frames * 10);
                        }
                        else
                        {
                            endTime = duration;
                        }

                        MusicData musicData = new(
                            t.Title, null,
                            new List<Artist>() { new(string.IsNullOrEmpty(t.Performer) ? cueSheet.Performer : t.Performer) },
                            new(cueSheet.Title))
                        {
                            From = MusicFrom.localMusic,
                            InLocal = path,
                            CUETrackData = new()
                            {
                                Index = t.TrackNumber,
                                StartDuration = startTime,
                                EndDuration = endTime,
                                Path = localFile.FullName
                            },
                            Index = t.TrackNumber
                        };
                        data.Add(musicData);
                    }
                    data.Reverse();
                    foreach (var i in data)
                    {
                        AddMusicDataToPlayList(listName, i, jdata);
                    }
                });

                return jdata;
            }
            else
            {
                MusicData localAudioData;
                TagLib.File tagFile = null;
                TagLib.Tag tag = null;
                //Track track = null;
                bool isError = false;

                try
                {/*
                    await Task.Run(() =>
                    {
                        track = new(localFile.FullName);
                    });*/
                    
                    await Task.Run(() =>
                    {
                        tagFile = TagLib.File.Create(localFile.FullName);
                        tag = tagFile.Tag;
                        if (tag.IsEmpty) isError = true;
                        if (tag.Performers == null) isError = true;
                    });
                }
                catch
                {
                    isError = true;
                }

                if (!isError)
                {
                    List<Artist> artists = new();
                    foreach (var art in tag.Performers)
                    {
                        artists.Add(new(art));
                    }

                    localAudioData = new MusicData(
                        tag.Title, null, artists, new(tag.Album),
                        inLocal: localFile.FullName, from: MusicFrom.localMusic
                        )
                    { Index = (int)tag.Track};
                }
                else
                {
                    localAudioData = new MusicData(
                        localFile.Name, null, null, new(null),
                        inLocal: localFile.FullName, from: MusicFrom.localMusic
                        );
                }

                localAudioData.RelaseTime = localFile.CreationTime.Ticks.ToString();
                return AddMusicDataToPlayList(listName, localAudioData, jdata);
            }
        }
    }
}
