using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// SetMusicTagPage.xaml 的交互逻辑
    /// </summary>
    public partial class SetMusicTagPage : UserControl
    {
        private MainWindow TheParent = null;
        private Source TheSource = null;

        private bool IsChoiceAlbumImage = false;
        public string NowFilePaths = null;
        private string NowPicPaths = null;

        public SetMusicTagPage(MainWindow Parent, Source Source)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = Source;
        }

        public void Set(string FilePath)
        {
            NowFilePaths = FilePath;
            ThePageTitle.Text = NowFilePaths;
            IsChoiceAlbumImage = false;
            TheImage.Source = Source.GetMusicAlbumImageBitmapImage(FilePath);
            BackImage.Source = TheImage.Source;
            TagLib.File TheFile = null;

            try { TheFile = TagLib.File.Create(FilePath); }
            catch { TheParent.ShowBox("错误", "无法打开文件! \n请检查打开的文件是否符合ID标签。"); TheParent.SetBigPage("Set"); NowFilePaths = null; return; }

            try { Title.Text = TheFile.Tag.Title; }
            catch { Title.Text = FilePath; }

            try { Artist.Text = GetArists(TheFile.Tag.Performers); }
            catch { Artist.Text = "无信息"; }

            try { Album.Text = TheFile.Tag.Album; }
            catch { Album.Text = "无信息"; }

            try { Years.Text = TheFile.Tag.Year.ToString(); }
            catch { Years.Text = "无信息"; }

            try { Genre.Text = TheFile.Tag.Genres[0].ToString(); }
            catch { Genre.Text = "无信息"; }

            Comment.Text = TheFile.Tag.Comment;
        }

        private string GetArists(string[] Artists)
        {
            string EndResult = "";
            foreach (string artist in Artists)
            {
                if (EndResult == "") EndResult += artist;
                else EndResult += ";" + artist;
            }

            return EndResult;
        }

        private string[] SetArtists(string Artists)
        {
            string[] artistList = Artists.Split(';');
            return artistList;
        }

        private void SetImageSource(string uri)
        {
            try
            {
                BitmapImage ThePath = new BitmapImage(new Uri(uri));

                TheImage.Source = ThePath;
                BackImage.Source = TheImage.Source;
            }
            catch (Exception err)
            {
                TheParent.ShowBox("错误", "选中的文件不是图片文件\n\n错误代码:\n" + err.ToString());
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TagLib.File TheFile = TagLib.File.Create(NowFilePaths);

                TheFile.Tag.Title = Title.Text;
                TheFile.Tag.Performers = SetArtists(Artist.Text);
                TheFile.Tag.Album = Album.Text;
                TheFile.Tag.AlbumArtists = TheFile.Tag.Performers;
                TheFile.Tag.Genres = new[] { Genre.Text };
                TheFile.Tag.Comment = Comment.Text;
                TheFile.Tag.Year = uint.Parse(Years.Text);

                if (TheFile.Tag.Title == NowPicPaths) TheFile.Tag.Title = "";
                if (TheFile.Tag.Performers[0] == "无信息") TheFile.Tag.Performers = new[] { "" };
                if (TheFile.Tag.Album == "无信息") TheFile.Tag.Album = "";
                if (TheFile.Tag.Genres[0] == "无信息") TheFile.Tag.Genres = new[] { "" };

                if (IsChoiceAlbumImage) TheFile.Tag.Pictures = new[] { new TagLib.Picture(NowPicPaths) };

                TheFile.Save();
            }
            catch (Exception err)
            {
                TheParent.ShowBox("错误", "无法保存文件!\n\n以下是可能的原因:\n  -1 此文件路径已被更换\n  -2 此文件已被删除\n  -3 此文件已被其他应用程序打开\n\n请将可能打开了此文件的应用程序关闭后再重试保存。\n\n注意: 在本应用程序播放歌曲时会将歌曲文件占用而使其不可编辑。\n\n错误代码:\n" + err.ToString());
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            TheParent.SetBigPage("Set");
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            IsChoiceAlbumImage = true;

            string ImagePath = TheSource.OpenWindowChoiceFlie("图片文件 (*.jpg;*.png;*.bmp;*.dib;*.jpeg;*.jpe;*.jfif;*.tif;*.tiff;*.heic;*.ras;*.eps;*.pcx;*.pcd;*.tga)|*.jpg;*.png;*.bmp;*.dib;*.jpeg;*.jpe;*.jfif;*.tif;*.tiff;*.heic;*.ras;*.eps;*.pcx;*.pcd;*.tga|所有文件 (*.*)|*.*");

            if (ImagePath == null)
            {
                IsChoiceAlbumImage = false;
                return;
            }

            SetImageSource(ImagePath);
            NowPicPaths = ImagePath;
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            TheParent.NowPlayState = MainWindow.PlayState.Pause;
            TheParent.audioPlayer.Stop();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            string SavePath = TheSource.SavingWindow("", "专辑图片", ".jpg", "图片文件 (*.jpg)|*.jpg");

            if (SavePath != null)
            {
                try
                {
                    TheSource.SaveBitmapImage(Source.GetMusicAlbumImageBitmapImage(NowFilePaths), SavePath);
                    TheParent.ShowBox("保存成功", "文件已保存到'" + SavePath);
                }
                catch (Exception err)
                {
                    TheParent.ShowBox("错误", "无法保存图片!\n\n错误代码:\n" + err.ToString());
                }
            }
            else
            {
                TheParent.ShowBox("已取消", "操作已取消");
            }
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            TheParent.ShowBox("帮助", "关于此界面工作原理:\n\n" +
                "已使用的开源库: taglib-sharp。\n" +
                "使用taglib修改歌曲文件的ID标签。\n\n\n" +
                "点击歌曲图片以选择并修改歌曲图片;\n\n" +
                "修改标题下的输入框内容以修改歌曲标题;\n\n" +
                "修改歌手下的输入框内容以修改歌曲歌手 (注: 在ID标签中，歌手为一个或多个，所以输入多名歌手时请使用英文分号(;)分割每一个歌手。例如 'Arcaea;ak+q' 即歌手为Arcaea和ak+q);\n\n" +
                "修改专辑下的输入框内容以修改专辑名;\n\n" +
                "修改流派下的输入框内容以修改流派信息;\n\n" +
                "修改注释下的输入框内容以修改注释信息;\n\n" +
                "修改年份下的输入框内容以修改年份信息 (格式如2021)。\n\n\n" +
                "关于歌词文件:\n歌词无法在ID标签中修改，因为歌词是一个独立的且后缀名为.lrc的文件，通常歌词文件名和歌曲文件名一致（除后缀名）并且在同一文件夹下。具体请上网搜索。");
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!TheParent.IsFullScreen)
            {
                base.OnMouseLeftButtonDown(e);
                TheParent.DragMove();
            }
        }
    }
}
