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
        public string NeteaseCookie = "MUSIC_U=00D606F3596A2025A6A68144758987B3EAFBCEC51331AA10948B15CC0DEEB02FDD33110953F4E9D1BCD3610679A771C286B57CC8C50B58D9F1C65DAB39CBF8A0E0139634A38FAA3D336F0D85734BF42C6F368DEB04523AA495D2A923D9815905EF2DD8AAA38AB8460EFB7F007D47A59CFD8A58F302F3D1D25DA9CE35BD02DB2B126256EB1A8FCAB5A2A0169937C59F038C06B6743FD61F85B29FE7EE321C7BE4DF4BF02DC7157689F6B2182AB82C836A08D5D50DDCF99913EADE58298CE0453B5242CEA066AE8B495C8006BE6AB6EEFC1473D4722DA36A07742587B7A4FBA2D5E7754C8955DC5B7D033045601D56EF42A4BD4655217811A4E691A482EF64F6F0722768DF3650ECCD2C9CAFFAD00F4E774C2972DAD51C7DE33DD52B4B72C7F0FE58EC7DB92BD19875F283570595803E0FD762798E4F0F35F3481682D17D6A6A5813FCBAB1EFCAB3647C4F0FB1BB717C6A7492BF8B8BF27FF91D9B55D0462411A924D311C40417785296038AF1E8587FCE6E";
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
