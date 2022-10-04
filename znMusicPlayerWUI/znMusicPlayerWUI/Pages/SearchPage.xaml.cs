using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.Controls;

namespace znMusicPlayerWUI.Pages
{
    public partial class SearchPage : Page
    {
        public SearchPage()
        {
            InitializeComponent();
            var a = Enum.GetNames(typeof(DataEditor.MusicFrom)).ToList();
            a.RemoveAt(6);
            a.RemoveAt(5);
            SearchSourceComboBox.ItemsSource = a;
            SearchSourceComboBox.SelectedIndex = 3;

            var b = Enum.GetNames(typeof(DataEditor.DataType)).ToList();
            b.RemoveAt(2);
            SearchTypeComboBox.ItemsSource = b;
            SearchTypeComboBox.SelectedIndex = 0;
        }

        public void StartSearch(string title)
        {
            MainWindow.SetNavViewContent(
                typeof(ItemListView),
                new List<object> { DataEditor.DataType.歌曲, title },
                new DrillInNavigationTransitionInfo());
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string a;

            if (args.ChosenSuggestion != null)
            {
                a = args.ChosenSuggestion.ToString();
            }
            else
            {
                a = sender.Text;
            }

            if (a == "") return;

            //ContentFrame.Navigate(typeof(SearchPage));

            // 防止触发 NavView.SelectionChanged 事件
            //IsBackRequest = true;
            //NavView.SelectedItem = null;
            //IsBackRequest = false;
            StartSearch(a);
        }

    }
}
