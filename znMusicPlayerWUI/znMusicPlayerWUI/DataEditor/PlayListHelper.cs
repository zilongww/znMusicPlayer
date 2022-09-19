using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.UI.Xaml.Media;

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
            var jdata = JObject.Parse(await ReadData());
            jdata.Add(musicListData.ListName, JObject.FromObject(musicListData));
            await SaveData(jdata.ToString());
        }

        public static JObject AddPlayList(MusicListData musicListData, JObject addData)
        {
            addData.Add(musicListData.ListName, JObject.FromObject(musicListData));
            return addData;
        }

        public static async Task AddMusicDataToPlayList(string listName, MusicData musicData)
        {
            var text = await ReadData();
            var jdata = JObject.Parse(text);

            if (!jdata.ContainsKey(listName))
            {
                jdata = AddPlayList(new(listName, listName, null, MusicFrom.localMusic, null, null), jdata);
            }

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

            await SaveData(jdata.ToString());
        }

        public static async Task DeleteMusicDataFromPlayList(string listName, MusicData musicData)
        {
            var text = await ReadData();
            var jdata = JObject.Parse(text);

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

            await SaveData(jdata.ToString());
        }

        public static JObject MackJsPlayListData(MusicListData musicListData)
        {
            return new JObject() { { musicListData.ListName, JObject.FromObject(musicListData) } };
        }

        private static async Task SaveData(string data)
        {
            await Task.Run(() =>
            {
                File.WriteAllText(DataFolderBase.PlayListDataPath, data);
            });
        }

        private static async Task<string> ReadData()
        {
            return await Task.Run(() =>
            {
                return File.ReadAllText(DataFolderBase.PlayListDataPath);
            });
        }

        public static async Task AddLocalMusicDataToPlayList(string listName, FileInfo localFlie)
        {
            MusicData localAudioData;

            TagLib.File tagFile = TagLib.File.Create(localFlie.FullName);
            var tag = tagFile.Tag;

            if (!tag.IsEmpty)
            {
                List<Artist> artists = null;
                if (tag.Performers.Any())
                {
                    artists = new();
                    foreach (var a in tag.Performers)
                    {
                        artists.Add(new(a, null, null));
                    }
                }

                localAudioData = new MusicData(
                    tag.Title == null ? localFlie.Name : tag.Title, null, artists, tag.Album,
                    inLocal: localFlie.FullName, from: MusicFrom.localMusic
                    );
            }
            else
            {
                localAudioData = new MusicData(
                    localFlie.Name, null, null,
                    inLocal: localFlie.FullName, from: MusicFrom.localMusic
                    );
            }

            await AddMusicDataToPlayList(listName, localAudioData);
        }
    }
}
