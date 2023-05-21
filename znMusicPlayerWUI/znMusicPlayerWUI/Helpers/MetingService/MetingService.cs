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
        public string NeteaseCookie = "MUSIC_U=00FE900B1D3716D73B32D9205D54DFE53636F646C4C3AE44FE8BCAFB6413A560AEE8BC2D76C31986DC23A84796D502A90F6A13B3D4FD9B58A9D155862AB862466C514220A65CBC2744859793671B40E806A5AEDF9EEBD44D147B53E11DAA021BEB65399359C28C2684CBCB580EED5E7FF8178E81ACF6638C2E08285F731EFF35EB41AE8A91741FF43DA4C2A145832DB151DEDFF9BA05B9A5E429F2FEE166A960ABFAADAE80BF0FDD883DAD03C678411FFA8BDC6AFACB4E90013F6113D2A60BCF8B4C151F15F3064899BC4620E9B7DD9020A47EFF348B93BD9BEE6A04A87BF97B0398A64DCC9CB2EC3E4E9F079A53C6D4868F2CFC33428F6BFA993087275DBB222EEA78B2BB0549D8CA28D29A040FD3013115707E0324EA09B7A8677EE48913040B363DC2F7685EF49DF17F398A058870450BD99D4495B603EE18EE82230CFB9A4FB37E365E9DA3CE2793DABEE8EAC4C41C3CEDD63F7A633AEB2E07B713155EE4BD14B5A9062C4FC6CDFE743A347DC608B7";
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
