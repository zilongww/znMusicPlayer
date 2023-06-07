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
        public string NeteaseCookie = "MUSIC_U=00DA0B9FD5198B46B3A35030DF92C44F18F730F2513DDEA9DF254D5ABC34341C988BA98F012F089EC15D92D973CE9376DE038DD7C64DD650453A7ECCDFF8BEF7970AAB7963823FFE97AA1BF8C25CD6859A801B1E37A8138E182D95E9EF528B2CDEAAAB3889A6FF72B004DCEFDA9AAC3B04C841CDD27082E75303EF383D045732F3E5A17FFBAC6141F61D5ED7AB2B568C3E664286B3417E25AEEA9A58BE4BFF0587AAF7FF42F3A5568F612DF94D52D3AF4575FA2080DDBD756B94BB8B72004A881F47A9455B76B51628F7A2608A964AF6C3CBD1B3736554F1804B6AE59054C113EA954EF9F62FE43CC5F8A5D5FEACFDF0C989F02D52F5766A237FA9F54636B6418B399CFB4D0F1DB866890E6A3FEFB6F4CDBF4232939693C4AC5ABD2E750516A8182C3C0037A4EFA38D342C942A5402C6FE3822B2910F081E21E00A86B9F881DCC8C335AB3399C9FBDD7469290FD6704F70B629F1376AE96CAAE4B2837C52472BC02EC8631D7913E296AD74FC20C4803EF3";
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
