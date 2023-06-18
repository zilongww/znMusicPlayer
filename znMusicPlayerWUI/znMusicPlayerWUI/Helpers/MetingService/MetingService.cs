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
        public string NeteaseCookie = "MUSIC_U=004A28C57777F3EEDC7C067A574502CB9734524762962C49DCD1C02DBBFEDEB6B6DAF3689A12EA315261FB043E00FD4C8008536A76F79503099DB988E36CCAA8D842497C9DE3C344D1EC5F4CB9BE45CD3D63B865691E5B7BE7DBCF8607FCCA2131C5365472B318CA08C030CBBAFA3A153BE547C775B0BF6AA58B44D638CA96AD4DCF63F47E495BB554B5C893AEA31BE9660709250D4D03E41D6AFFD6E021B8A33E1F97731832677FE5CBF7DF6C0FDA36B1E3F27413A52B85BE2AD79CAA4D9BB93F32F3394C405835D4A443CDA3C584A59D46F51E795356270D570390107A20E91A56ABBD2DFA5284DB67187F725986F6FAAC4656398538111D2D99B8029B8BEA2B27CBC13E1A39BEE6D493245740B48E0AB96F24BCA03E954065399C58340052D2F97BC0BE2ED6470DAE4A84500ACE5F37B6E24455625615B8258B535E60E9B654A0D47A7254314F7BF1CA5A4F90D17D91B8FD44998E08689F5DBAEA6F38D12CE0AA28F6A5D2BB6946615D5B2C88843381";
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
