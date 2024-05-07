using System;
using Microsoft.UI.Xaml.Media.Animation;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Pages.ListViewPages
{
    public static class ListViewPage
    {
        public static void SetPageToListViewPage(IIsListPage data)
        {
            var tType = data.GetType();
            Type pageType = null;
            object paramData = null;

            if (tType == typeof(Album))
            {
                paramData = data;
                pageType = typeof(ItemListViewAlbum);
            }
            else if (tType == typeof(Artist))
            {
                paramData = data;
                pageType = typeof(ItemListViewArtist);
            }
            else if (tType == typeof(MusicListData))
            {
                paramData = (data as MusicListData).MD5;
                pageType = typeof(PlayListPage);
            }
            else if (tType == typeof(SearchData))
            {
                paramData = data;
                pageType = typeof(ItemListViewSearch);
            }

            MainWindow.SetNavViewContent(
                pageType,
                paramData,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
