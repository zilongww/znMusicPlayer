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

            if (tType == typeof(Album)) pageType = typeof(ItemListViewAlbum);
            else if (tType == typeof(Artist)) pageType = typeof(ItemListViewArtist);
            else if (tType == typeof(MusicListData)) pageType = typeof(PlayListPage);
            else if (tType == typeof(SearchData)) pageType = typeof(ItemListViewSearch);

            MainWindow.SetNavViewContent(
                pageType,
                data,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
