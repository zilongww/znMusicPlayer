using System;
using Microsoft.UI.Xaml.Media.Animation;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Pages.ListViewPages
{
    public static class ListViewPage
    {
        public static void SetPageToListViewPage(object data)
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
                paramData = data;
                pageType = typeof(PlayListPage);
            }
            else if (tType == typeof(SearchData))
            {
                paramData = data;
                pageType = typeof(ItemListViewSearch);
            }
            else if (tType == typeof(string))
            {
                paramData = data;
                pageType = typeof(PlayListPage);
            }

            MainWindow.SetNavViewContent(
                pageType,
                paramData,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
