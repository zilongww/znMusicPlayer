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
        public string NeteaseCookie = "_ntes_nnid=0590a8d6b6be9cad6bc23b97c07f7cdc,1672820325011; _ntes_nuid=0590a8d6b6be9cad6bc23b97c07f7cdc; NMTID=00OqTw4mwkO_kYwG04pove_JRrgaqsAAAGGaJuk0A; WEVNSM=1.0.0; WNMCID=ndnpsx.1676792265620.01.0; ntes_kaola_ad=1; WM_TID=Ie6AN9mqQ15AUQAFAEPRdqrUMWAesOmG; __snaker__id=wqds5CFyYKQdacfe; YD00000558929251:WM_TID=ggJS8OLoa/REBAREUAfUMuuQNTULqUR2; __remember_me=true; WM_NI=IoxK7aJnykztdEfG1u10MkAN1YVSiFyJPjQGYhyXChI7m5w2gWKRZKSwJZKafIMai4hRigIsitemT3Aok8dH7dUkoM0BTPSltTTqxs0DF0PnN0FHBfs2QUTxiKBnk5iwd3o=; WM_NIKE=9ca17ae2e6ffcda170e2e6ee92e562b2bdfb83aa7089b08bb2c45e878e8b86c448b0bba9d4ec539c9ea7d9d32af0fea7c3b92a8bb1b8acb66b8ab2b78cf980a89baaa8b47ff3f0838cc446f3879ab4ec4ebb96a08bd44d9bb8b7b9c2648be9a3d6f63aa58985b3d94191a986bbca6ba6978382ec40ac8ea788cc79abacab8ed73983b1a5d9e744f5bc9b8cee47f49e9cb1d268b88c9c92fb79f49c8ad1d3668ef08aa9c16eb1ee9fb6d774ab958e8fea3faba9acd4ee37e2a3; JSESSIONID-WYYY=PFTMe9noV1Uz72mX2nYostvQSt0SwjsInUBGZNXtjO3pgNpV1phMtaeNFhU91loSTvc7ITjrq4tvscGPusUTfAjoHN/msjIyQo9XqqN6GXA\\8SuSNI1k2\\GUEFHi85xXu6NTkQkDklXHHq9c6zUxy8i/pUjhj7E+cNyCQRay\\s/ZYXuh:1679827501466; _iuqxldmzr_=33; gdxidpyhxdE=GOf0xCnST+g9rgOnAAWAVUQmIaJEjxMn9AB0aIaJKddPM1\\ISPnTfCic59fkDbsJz1pmT2/3xv8/ZrGuZ1vHzrrRmSf10e7SRyYlv9PzDU04MG0iAL4B+bTSbU+Tsf8VPAxTiArfa2HDHxxXpIN08Q3V48Ak1Z8iWEi0gTu+lVHA5GDJ:1679826602829; YD00000558929251:WM_NI=rXM40COhOpWg6fDqbkeweUq4z8y0IuksW0NStSBNZHc3EWuj15dIf4zLdavrt5XVRMI6rwNSmlTPHE0O1lCjIvuBVf1nbPlXvap2R+J8tU2lb20L1ie+xQz/ncDo2NagUjg=; YD00000558929251:WM_NIKE=9ca17ae2e6ffcda170e2e6ee99b85aa8affd8cef33b7968fa3d14f828a8aadc46fac90feadcc6192ef86b0c42af0fea7c3b92ab28883d5b3498db88191d66df6ba8c92e161a7a9abb6f440bae8b7d1cc7fa9ad99d1cd59b88d8eafd944a1ed8bb8f862f3ebacaae540ba94ac82f57ab8eb98b7d1478beabe98cd59afb49cb1cb4ff491ff8dc2628baabba3d56ef3b2aeb1ef54b6bfc0d3d1218ae8bcb6b168a2ee00adb463a7f5b8b2f925aebd8c90b146a3b99b8be637e2a3; MUSIC_U=1fa8ee41e5cb7a93558f6fe5781266a1845addae735e3d87ea43572553372035993166e004087dd3d5450e7c25c8adea87806083c390f96745ada784b502bc8c0b0c7730e42f4911d4dbf082a8813684; __csrf=039112f0db7bca9056390b66e50fe866; playerid=60813410";
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
