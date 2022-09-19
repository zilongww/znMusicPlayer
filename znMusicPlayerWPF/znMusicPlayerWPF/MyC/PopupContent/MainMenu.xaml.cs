using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace znMusicPlayerWPF.MyC.PopupContent
{
    /// <summary>
    /// MainMenu.xaml 的交互逻辑
    /// </summary>
    public partial class MainMenu : Border
    {
        private MainWindow TheParent = null;

        public MainMenu(MainWindow TheParent)
        {
            InitializeComponent();
            this.TheParent = TheParent;
            TheParent.BlurThemeChangeEvent += (data) =>
            {
                patha.Fill = data.TextColor;
                PopupPPPath.Fill = data.TextColor;
                pathb.Fill = data.TextColor;
                foreach (var t in textBlocks) t.Foreground = data.TextColor;
                foreach (var z in znButtons) z.EnterColor = data.ButtonEnterColor;
                foreach (var p in packIcons) p.Foreground = data.TextColor;
                foreach (var o in OCButtons)
                {
                    o.Stroke = data.TextColor;
                    o.OCBackground = data.TextColor;
                    o.MouseEnterStroke = data.InColorTextColor;
                    o.MouseEnterOCBackground = data.InColorTextColor;
                    o.IsLockedStroke = data.InColorTextColor;
                    o.IsLockedBackground = data.InColorTextColor;
                    o.IsLockedOCBackground = data.TextColor;
                }
            };
        }

        private void VolumeSp_MouseWheel(object sender, MouseWheelEventArgs args)
        {
            TheParent.VolumeSp_MouseWheel(sender, args);
        }

        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            TheParent.Slider_ValueChanged_1(sender, args);
        }

        private void LastMusicButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.SetBeforeSong();
        }

        private void PauseMusicButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.PauseMusicButton_Click(sender, e);
        }

        private void NextMusicButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.SetNextSong();
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            TheParent.MenuOpen_Click(sender, e);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            TheParent.MenuExit_Click(null, null);
        }

        private void PopupMenu_LrcOCBtn_Checked(OCButton sender)
        {
            TheParent.PopupMenu_LrcOCBtn_Checked(sender);
        }

        private void PopupMenu_LrcLockOCBtn_Checked(OCButton sender)
        {
            TheParent.PopupMenu_LrcLockOCBtn_Checked(sender);
        }

        private void PopupMenu_LrcLockOCBtn_LockedClick(OCButton sender)
        {
            TheParent.PopupMenu_LrcLockOCBtn_LockedClick(sender);
        }

        Path patha;
        Path pathb;
        private void Path_Loaded(object sender, RoutedEventArgs e)
        {
            patha = sender as Path;
        }

        private void Path_Loaded_1(object sender, RoutedEventArgs e)
        {
            pathb = sender as Path;
        }

        List<MaterialDesignThemes.Wpf.PackIcon> packIcons = new List<MaterialDesignThemes.Wpf.PackIcon>();
        private void PackIcon_Loaded(object sender, RoutedEventArgs e)
        {
            packIcons.Add(sender as MaterialDesignThemes.Wpf.PackIcon);
        }

        List<TextBlock> textBlocks = new System.Collections.Generic.List<TextBlock>();
        private void TextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            textBlocks.Add(sender as TextBlock);
        }

        List<OCButton> OCButtons = new List<OCButton>();
        private void PopupMenu_LrcOCBtn_Loaded(object sender, RoutedEventArgs e)
        {
            OCButtons.Add(sender as OCButton);
        }

        List<znButton> znButtons = new List<znButton>();
        private void znButton_Loaded(object sender, RoutedEventArgs e)
        {
            znButtons.Add(sender as znButton);
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            TheParent.LrcWindowOpenButton_Click(sender, e);
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            TheParent.SetPage("Setting");
            TheParent.IsOpenBigPage = false;
            TheParent.UserShow();
        }
    }
}
