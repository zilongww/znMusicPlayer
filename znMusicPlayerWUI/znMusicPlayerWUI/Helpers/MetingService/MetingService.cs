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
        public string NeteaseCookie = "_ntes_nnid=0590a8d6b6be9cad6bc23b97c07f7cdc,1672820325011; _ntes_nuid=0590a8d6b6be9cad6bc23b97c07f7cdc; NMTID=00OqTw4mwkO_kYwG04pove_JRrgaqsAAAGGaJuk0A; WEVNSM=1.0.0; WNMCID=ndnpsx.1676792265620.01.0; ntes_kaola_ad=1; WM_TID=Ie6AN9mqQ15AUQAFAEPRdqrUMWAesOmG; JSESSIONID-WYYY=IbqNOCgpbDIXHFVfoXWK\\mKvE3ipWsVADkYB0oFm65735ViWAMdRCqqKj1t0DCr8pijUGOwp4PfWWN7QVq8d\\XiVWKnBkfqXMf\\twbaG06UYBFtvt/lZBMSu1606uC077wPG\\v4DrEXs5dt\\8uRJ2IgVuyNG3l38o64hprzkdvaW\\Yut:1678615945193; _iuqxldmzr_=33; WM_NI=g7pwmMKY83xfG6UtJeugUUaSBuE9B9nFjAdvuhoX/sfy39h5C1B3h3C7V6AdjXeSg6hhBJjVXh7FI+7jEz6LycIlicrSHA+otNdRl+uvlSd86Z9JI51pQLLZHkrmyfQHTWs=; WM_NIKE=9ca17ae2e6ffcda170e2e6ee9bf879a395b6afaa5b8f8e8bb3c15a978b8ab0d44890bca6a4ef33a3928ab0e62af0fea7c3b92aa5e9c098f0478987f8d4fb6e85ea9b95c96fb7b99bb6e45982ab8bdac133ae9aa6a2e77eb7b8a1b9ae79a6a7fddaeb3f81ef8ed4eb3981e99ad7c470aa88abb4e63eacb88eabe95e9cb8f9a7cc21f5b68b9bb548bbbd878dec69b28d88a2aa65b892acb3ef21b7aaa4b4e734f686f7abcc3ff3b09fadcf528cb5be8efc6e93989cb8f237e2a3; __snaker__id=wqds5CFyYKQdacfe; gdxidpyhxdE=EGggXWK+xxHdYJQm23R4Mce6K1BDpCwKaspZXJmcDkB681tOGY+lcnbC42RbHMOKs\\8jbjwsmoLCo/s5glbPhiDE\\aXfQSxa0GdCj5ssvQxiQKdfx9PRyDORcY71Bz201Yv62/w4dLZpV0rbxIks07YQ/jnMyDkvx3d5ZsxHiqY9/7gJ:1678615047214; YD00000558929251:WM_NI=msridgQWJhE5/OOwEtIHqiPSuD9rGFlv8WpLfUL+vUTOFPTjc0Hgg0dAI3gi9Clg9OqvWlg9peMAfXj4xZuANgjN5p9Hshv6x4H9BNF1oxW48wbAm3MF3Yk+Gv4rsqYcRnY=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6eeb1d164b4e8fc8ef934b0e78ea7c54e979b8eb0d15ca692a6b8eb8090ed8493ef2af0fea7c3b92aafadbe8ae76d93898689c83fed918198c8808b98bf8ad359b79cf793c64eb0aaf8b8f348a68b84ccf55f98918ab1d762a1f08888e63ca89f87d4e663fb9da488bc45b3aa998de633b4bb83a2e554aaecb8bbf84b8eb9a6afb87cfb98bd89f53ebcec88dac14686efa199d640fca9aea2f0398f9afdbaea2185aafc93f27b9bbd9e8fdc37e2a3; YD00000558929251:WM_TID=ggJS8OLoa/REBAREUAfUMuuQNTULqUR2; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a159fec49253a4f6303f6580f44bc9a47a993166e004087dd3c88917cb907b87c0bf811f80b248a2b245ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684; __csrf=1fc6f74519194c0038e6df8b780fe024";
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
