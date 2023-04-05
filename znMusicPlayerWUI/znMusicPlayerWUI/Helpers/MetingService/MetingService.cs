using Meting4Net.Core.Models.Netease;
using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Http.Diagnostics;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Helpers.MetingService;

namespace znMusicPlayerWUI.Helpers
{
    public interface IMetingService
    {
        public Meting4Net.Core.Meting Services { get; set; }
        public Task<string> GetUrl(string id, int br);
        public Task<Tuple<string, string>> GetLyric(string id);
        public Task<Tuple<string, string>> GetPic(string id);
        public Task<object> GetSearch(string keyword, int pageNumber = 1, int pageSize = 30, SearchDataType type = default);
        public Task<MusicListData> GetPlayList(string id);
        public Task<Artist> GetArtist(string id);
    }

    public class MetingServices
    {
        public int RetryCount = 15;
        public string NeteaseCookie = "MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a10b50b01f699cc9e2f4a20f99ad18b886993166e004087dd3f60bf2240c7dfc5a10f0afff2020d86345ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684";
        private Meting4Net.Core.Meting NeteaseMeting = new(Meting4Net.Core.ServerProvider.Netease);
        public IMetingService NeteaseServices { get; set; }

        public MetingServices()
        {
            NeteaseServices = new NeteaseMeting(NeteaseMeting);
        }

        public async void InitMeting()
        {
            NeteaseMeting.Cookie("os=pc; " + NeteaseCookie);
        }

    }
}
