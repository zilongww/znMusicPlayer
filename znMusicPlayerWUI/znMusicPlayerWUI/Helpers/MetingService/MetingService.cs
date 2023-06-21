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
        public string NeteaseCookie = "MUSIC_U=00D359768DEF27C909C9FDB3A5C8D20E457A89AB0DBD8AAE202B5A26CF7FA2DC4C35F43DEDCE9D43B8F8067DFF1065C444224EFDD41A05939F1E9873780511BBA6C0381C1AFDDF39A8B5ECA05FD618A198EB5EA2844FAEC83B9008515E08EE947DD940E09E639EA97DBC28741929F9285EBC0207ED26DD43F14A39C520427DB1CD840B78BCFFB97EBE4D0EE6B3233D51369380CD188D9D49AACB3C13F4D6D23C267AD15EB1AEB5F18B6740CF65AD35EEF708C27BF450650A30E2668E15A469DF3236BD2B18E0D360AC63F51CC08B94DC1FC05F03CC1DDD0F263B4EFF8DB3D847B423EE3682CEDBA415BCF1105642D5EEC21F98E23C0CE74E7E1C0C9DAD28F86117EB1F669842F021AF79BFBF30DD28ADACF43A990CBEBD5AF6875D8E095AEFAA941A58364CB24C96ED1A7804E13C37F282EBFCD2C453B97049FCBD7DCEC5082B1BCA880A40051621646B8784B60A1660A7B2F3395387A6B3394563110279CBC3A2BBBAD31C2DB4615D15F8F8FD1C070067";
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
