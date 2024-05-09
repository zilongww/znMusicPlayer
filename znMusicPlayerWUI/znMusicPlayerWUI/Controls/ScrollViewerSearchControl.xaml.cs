using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace znMusicPlayerWUI.Controls
{
    public sealed partial class ScrollViewerSearchControl : Grid
    {
        public event DependencyPropertyChangedEventHandler IsOpenChanged;

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                "IsOpen",
                typeof(bool),
                typeof(ScrollViewerSearchControl),
                new(false, OnIsOpenChanged)
                );
        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewerSearchControl ssc = d as ScrollViewerSearchControl;
            var value = (bool)e.NewValue;
            ssc.SetIsOpen(value);
            ssc.IsOpenChanged?.Invoke(d, e);
        }

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value);
        }

        void SetIsOpen(bool value)
        {
            if (value)
            {
                Opacity = 1;
                IsHitTestVisible = true;
            }
            else
            {
                Opacity = 0;
                IsHitTestVisible = false;
            }
        }

        public ScrollViewerSearchControl()
        {
            InitializeComponent();
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Tag as string)
            {
                case "0":
                    break;
                case "1":
                    break;
                case "2":
                    IsOpen = !IsOpen;
                    break;
            }
        }
    }
}
