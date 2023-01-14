using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Composition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.DataEditor;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace znMusicPlayerWUI.Controls
{
    public partial class LyricItem : Grid
    {
        static TextAlignment _textAlignments = TextAlignment.Left;
        public static TextAlignment TextAlignments
        {
            get => _textAlignments;
            set
            {
                _textAlignments = value;
                foreach (var item in SLItems)
                {
                    item.LyricTextBlock.HorizontalTextAlignment = value;
                    item.TranslateTextBlock.HorizontalTextAlignment = value;
                }
            }
        }

        public static List<LyricItem> SLItems = new();
        public LyricItem()
        {
            InitializeComponent();
            SLItems.Add(this);
        }

        public async void UpdataHeight()
        {
            int num = 0;
            while (num < 10)
            {
                if (!string.IsNullOrEmpty(TranslateTextBlock.Text))
                {
                    TranslateTextBlock.Visibility = Visibility.Visible;
                    break;
                }
                else
                {
                    TranslateTextBlock.Visibility = Visibility.Collapsed;
                }
                await Task.Delay(10);
            }
            System.Diagnostics.Debug.WriteLine(num);
        }
    }
}
