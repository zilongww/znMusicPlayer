using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace znMusicPlayerWUI.Pages
{
    public interface IPage
    {
        public bool IsNavigatedOutFromPage { get; set; }
    }
}
