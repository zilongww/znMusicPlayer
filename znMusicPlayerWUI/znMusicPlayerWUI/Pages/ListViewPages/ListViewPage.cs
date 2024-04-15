using System;
using Microsoft.UI.Xaml.Media.Animation;

namespace znMusicPlayerWUI.Pages.ListViewPages
{
    public static class ListViewPage
    {
        public static void SetPageToListViewPage<T>(object data)
        {
            var ttype = typeof(T);
            if (ttype != typeof(ItemListViewAlbum) && ttype != typeof(ItemListViewArtist) && ttype != typeof(ItemListViewPlayList) && ttype != typeof(ItemListViewSearch))
            {
                throw new ArgumentOutOfRangeException(nameof(T));
            }
            MainWindow.SetNavViewContent(
                typeof(T),
                data,
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
