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
        public string NeteaseCookie = "_ntes_nnid=0590a8d6b6be9cad6bc23b97c07f7cdc,1672820325011; _ntes_nuid=0590a8d6b6be9cad6bc23b97c07f7cdc; _ntes_newsapp_install=false; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a1ce704ecdea925bd82ed80bafd4d7a025993166e004087dd3a838d7a8250550d0be6c8eb235e822d945ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684; __csrf=1e7ed32c15aa38af2f03ce3386586b31; NMTID=00OqTw4mwkO_kYwG04pove_JRrgaqsAAAGGaJuk0A; WEVNSM=1.0.0; WNMCID=ndnpsx.1676792265620.01.0; JSESSIONID-WYYY=Ks8zKUR7a7XR8UkTs77/URU4hofK0\\wTv+Kd9kyNtwEZ8BWxI\\nznyq5lYYwRKkUffD63bo7\\TTXbH9IGWUellS/RkjTeKd4l20My\\0yScryzSIl21rDjkBndYjKWdUIVtdDsmt1oS9B4fh+9W0j2lxCnv/U5KO8eyElBsXachHoXYwq:1676794065657; _iuqxldmzr_=33; ntes_kaola_ad=1";
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
