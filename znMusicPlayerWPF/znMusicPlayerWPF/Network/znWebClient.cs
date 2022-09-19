using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace znMusicPlayerWPF.Network
{
    public class znWebClient
    {
        private bool IsComplete = false;
        public int TimeOut { get; set; }
        public bool IsTimeOut { get; set; } = false;

        public znWebClient(int TimeOut = 8500)
        {
            this.TimeOut = TimeOut;
        }

        private async void StartTimeSet()
        {
            int nowTime = 0;

            while (!IsComplete)
            {
                break;
                await Task.Delay(250);
                nowTime += 250;
                if (nowTime >= TimeOut)
                {
                    IsTimeOut = true;
                }
            }
        }

        public string GetString(string address)
        {
            WebClient wb = new WebClient() { Encoding = Encoding.UTF8 };
            wb.Headers.Add("Accept", "*/*");
            wb.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            wb.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.150 Safari/537.36 Edg/88.0.705.63");
            wb.Headers.Add("Cookie", "_ga=GA1.2.1083049585.1590317697; _gid=GA1.2.2053211683.1598526974; _gat=1; Hm_lvt_cdb524f42f0ce19b169a8071123a4797=1597491567,1598094297,1598096480,1598526974; Hm_lpvt_cdb524f42f0ce19b169a8071123a4797=1598526974; kw_token=HYZQI4KPK3P");
            wb.Headers.Add("csrf", "HYZQI4KPK3P");

            string s = null;
            try
            {
                s = wb.DownloadString(address);
            }
            catch
            {
                s = "";
            }

            IsComplete = true;
            wb.Dispose();

            return s;
        }

        public string GetFile(string address, string downloadPath)
        {
            StartTimeSet();

            WebClient webClient = new WebClient() { Encoding = Encoding.UTF8 };
            try
            {
                webClient.DownloadFile(address, downloadPath);
            }
            catch { }

            webClient.Dispose();

            if (IsTimeOut) return null;

            IsComplete = true;
            return downloadPath;
        }
    }
}
