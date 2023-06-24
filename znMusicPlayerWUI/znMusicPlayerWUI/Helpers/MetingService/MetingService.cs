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
        public string NeteaseCookie = "MUSIC_U=006A62FD574BEC5E4C68A86F028E4DCCDF1C96D50A4E8050B1C3976AFDA1BE720B511F7D007C5DAB18437A0E5D33044775C32E5546DD61E778B6D3345885E18E4C3CC2E4626BA0D9C500C3C3E4AF5832CA8121C8CFEA1F59FD22204B404CF13BED9CFFF55C9E2F7194FBBF5B7BC47396047874A3C4ACE1EF9E05F5F9C4CF5B5B7EDFD36DB60D0016EB14887E804199F12552566DBEF0276C6CDE3C78DF245BFA4596B48279111A8A66C3C709B7B24962A34FF9A7B789788ACF13AAF9C9BF80F485E1FA6264B0A966D1D48052145A39F37E1083C4EF345CBAE4E5DA17B331701B4E9EAEEC7E633484C326735A472B3DF75F28C548F938C5D9BEB26D7BD3B12D2FCFC3AADD0E60B09CB641B1B007D5A39B6446713BE7A0D3D1E364742D43784F5620916B2E446975240DC6DB7033D6137AD40B4920825348170410636F7B0C540DD383CDD521887EEC22BD51B0FC9925E7A8ECAC24B429845BC0B0FA61A8549F91CB8FE778864549DD9E5A1301345F97B6BD";
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
