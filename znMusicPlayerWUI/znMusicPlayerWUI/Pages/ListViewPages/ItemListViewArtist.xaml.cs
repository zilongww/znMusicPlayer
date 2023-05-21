using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System;
using System.Numerics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using znMusicPlayerWUI.Pages;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Storage.Pickers;
using znMusicPlayerWUI.DataEditor;
using znMusicPlayerWUI.Media;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace znMusicPlayerWUI.Pages
{
    public partial class ItemListViewArtist : Page
    {
        private ScrollViewer scrollViewer { get; set; }
        public Artist NavToObj { get; set; }
        public MusicFrom NowMusicFrom { get; set; } = MusicFrom.neteaseMusic;

        public ItemListViewArtist()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlayAllButton.Foreground = new SolidColorBrush(CodeHelper.IsAccentColorDark() ? Colors.White : Colors.Black);
            Artist a = (Artist)e.Parameter;
            NavToObj = a;
            InitData();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            await Task.Delay(500);
            scrollViewer?.ScrollToVerticalOffset(0);

            MusicDataList.Clear();
            Artist_Image.Dispose();
            musicListData = null;
            NavToObj = null;
            UnloadObject(this);
            //GC.SuppressFinalize(this);
            //System.Diagnostics.Debug.WriteLine("Clear");
        }
/*
        private void CreatShadow()
        {
            var visual = ElementCompositionPreview.GetElementVisual(Artist_Image);
            compositor = visual.Compositor;

            var basicRectVisual = compositor.CreateSpriteVisual();
            basicRectVisual.Size = Artist_Image.RenderSize.ToVector2();

            dropShadow = compositor.CreateDropShadow();
            dropShadow.BlurRadius = 45f;
            dropShadow.Color = Colors.Black;
            dropShadow.Opacity = 0.3f;
            dropShadow.Offset = new Vector3(0, 4, 0);

            basicRectVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(Artist_Image_DropShadowBase, basicRectVisual);
        }
*/
        public ObservableCollection<SongItemBindBase> MusicDataList = new();
        MusicListData musicListData = null;
        static bool firstInit = false;
        int pageNumber = 1;
        int pageSize = 30;
        public async void InitData()
        {
            SelectorSeparator.Visibility = Visibility.Collapsed;
            AddSelectedToPlayingListButton.Visibility = Visibility.Collapsed;
            AddSelectedToPlayListButton.Visibility = Visibility.Collapsed;
            DeleteSelectedButton.Visibility = Visibility.Collapsed;
            DownloadSelectedButton.Visibility = Visibility.Collapsed;
            SelectReverseButton.Visibility = Visibility.Collapsed;
            SelectAllButton.Visibility = Visibility.Collapsed;

            Artist_TitleTextBlock.Text = NavToObj.Name;
            Artist_OtherTextBlock.Text = "";

            LoadingRing.Visibility = Visibility.Visible;
            LoadingRing.IsIndeterminate = true;
            var obj = await App.metingServices.NeteaseServices.GetArtist(NavToObj.ID);
            if (obj == null)
            {
                await MainWindow.ShowDialog("加载艺术家信息时出现错误", "无法加载艺术家信息，请重试。");
                return;
            }
            NavToObj = obj;
            musicListData = NavToObj.HotSongs;
            Artist_OtherTextBlock.Text = NavToObj.Describee;

            if (musicListData != null)
            {
                LoadImage();
                LoadingRing.Visibility = Visibility.Visible;
                LoadingRing.IsIndeterminate = true;
                await Task.Delay(100);
                var dpi = CodeHelper.GetScaleAdjustment(App.WindowLocal);
                MusicData[] array = null;

                switch (SortComboBox.SelectedIndex)
                {
                    case 0: //默认
                        array = musicListData.Songs.ToArray();
                        break;
                    case 1: //名称升序
                        array = musicListData.Songs.OrderBy(m => m.Title).ToArray();
                        break;
                    case 2: //名称降序
                        array = musicListData.Songs.OrderByDescending(m => m.Title).ToArray();
                        break;
                    case 3: //艺术家升序
                        array = musicListData.Songs.OrderBy(m => m.Artists[0].Name).ToArray();
                        break;
                    case 4: //艺术家降序
                        array = musicListData.Songs.OrderByDescending(m => m.Artists[0].Name).ToArray();
                        break;
                    case 5: //专辑升序
                        array = musicListData.Songs.OrderBy(m => m.Album).ToArray();
                        break;
                    case 6: //专辑降序
                        array = musicListData.Songs.OrderByDescending(m => m.Album).ToArray();
                        break;
                    case 7: //时间升序
                        array = musicListData.Songs.OrderBy(m => m.RelaseTime).ToArray();
                        break;
                    case 8: //时间降序
                        array = musicListData.Songs.OrderByDescending(m => m.RelaseTime).ToArray();
                        break;
                }

                MusicDataList.Clear();
                foreach (var i in array)
                {
                    MusicDataList.Add(new() { MusicData = i, ImageScaleDPI = dpi });
                }
            }
            LoadingRing.IsIndeterminate = false;
            LoadingRing.Visibility = Visibility.Collapsed;

            //Result_BaseGrid_SizeChanged(null, null);
            //UpdataShyHeader();
        }

        private async void LoadImage()
        {
            if (musicListData.ListDataType == DataType.本地歌单)
            {
                Artist_Image.Source = await FileHelper.GetImageSource(musicListData.PicturePath);
            }
            else if (musicListData.ListDataType == DataType.歌单)
            {
                Artist_Image.Source = await FileHelper.GetImageSource(await ImageManage.GetImageSource(musicListData));
            }
            else if (musicListData.ListDataType == DataType.艺术家)
            {
                var art = NavToObj;
                Artist_Image.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(art.PicturePath));
                System.Diagnostics.Debug.WriteLine(art.PicturePath);
            }
            Artist_Image1.Source = Artist_Image.Source;
        }

        CompositionPropertySet scrollerPropertySet;
        Compositor compositor;
        Visual headerVisual;
        Visual backgroundVisual;
        Visual logoVisual;
        Visual stackVisual;
        Visual tbVisual;
        Visual headerBaseGridVisual;
        Visual ImageScrollVisual;
        public void UpdataShyHeader()
        {
            if (scrollViewer == null) return;

            double anotherHeight = menu_border.ActualHeight - LittleBarGrid.ActualHeight;
            String progress = $"Clamp(-scroller.Translation.Y / {anotherHeight}, 0, 1.0)";

            if (scrollerPropertySet == null)
            {
                scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
                compositor = scrollerPropertySet.Compositor;
                headerVisual = ElementCompositionPreview.GetElementVisual(menu_border);
                backgroundVisual = ElementCompositionPreview.GetElementVisual(BackColorBaseRectangle);
                logoVisual = ElementCompositionPreview.GetElementVisual(Artist_Image_BaseGrid);
                stackVisual = ElementCompositionPreview.GetElementVisual(InfosBaseStackPanel);
                tbVisual = ElementCompositionPreview.GetElementVisual(ArtistTb);
                headerBaseGridVisual = ElementCompositionPreview.GetElementVisual(HeaderBaseGrid);
                ImageScrollVisual = ElementCompositionPreview.GetElementVisual(Artist_ImageBaseBorder);
            }

            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {anotherHeight}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

/*
            var headerBaseGridVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(1, 0, {progress})");
            headerBaseGridVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            headerBaseGridVisual.StartAnimation("Opacity", headerBaseGridVisualOpacityAnimation);
            */
            var backgroundVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(1, 0, {progress})");
            backgroundVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            ImageScrollVisual.StartAnimation("Opacity", backgroundVisualOpacityAnimation);
            
            var tbVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            tbVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            tbVisual.StartAnimation("Opacity", tbVisualOpacityAnimation);

            var ImageVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0,0,0), Vector3(0,{menu_border.ActualHeight / 2},0), {progress})");
            ImageVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            ImageScrollVisual.StartAnimation(nameof(ImageScrollVisual.Offset), ImageVisualOffsetAnimation);
            /*
            // Logo scale and transform                                          from               to
            var logoHeaderScaleAnimation = compositor.CreateExpressionAnimation("Lerp(Vector2(1,1), Vector2(0.5, 0.5), " + progress + ")");
            logoHeaderScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Scale.xy", logoHeaderScaleAnimation);

            var logoVisualOffsetXAnimation = compositor.CreateExpressionAnimation($"Lerp(24, 24, {progress})");
            logoVisualOffsetXAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Offset.X", logoVisualOffsetXAnimation);

            var logoVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp(24, {anotherHeight} + 8, {progress})");
            logoVisualOffsetYAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation("Offset.Y", logoVisualOffsetYAnimation);

            var stackVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(246,24,0), Vector3(140,{anotherHeight} + 8,0), {progress})");
            stackVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            stackVisual.StartAnimation(nameof(stackVisual.Offset), stackVisualOffsetAnimation);
            */
            /*
                        Visual textVisual = ElementCompositionPreview.GetElementVisual(Result_Search_Header);
                        Vector3 finalOffset = new Vector3(0, (float)Result_Search_Header.ActualHeight, 0);
                        var headerOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0,0,0), finalOffset, {progress})");
                        headerOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
                        headerOffsetAnimation.SetVector3Parameter("finalOffset", finalOffset);
                        textVisual.StartAnimation(nameof(Visual.Offset), headerOffsetAnimation);*/

        }

        private async void UpdataCommandToolBarWidth()
        {
            ToolsCommandBar.Width = 0;
            //await Task.Delay(1);
            ToolsCommandBar.Width = double.NaN;
        }

        Vector3 ATBOffset = default;
        private async void menu_border_Loaded(object sender, RoutedEventArgs e)
        {
            if (scrollViewer == null)
            {
                scrollViewer = (VisualTreeHelper.GetChild(Children, 0) as Border).Child as ScrollViewer;
                scrollViewer.CanContentRenderOutsideBounds = true;

                // 设置header为顶层
                var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)Children.Header);
                var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
                Canvas.SetZIndex(headerContainer, 1);
            }

            ATBOffset = Artist_TitleTextBlock.ActualOffset;

            UpdataCommandToolBarWidth();
            Result_BaseGrid_SizeChanged(null, null);
        }

        private void Result_BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {/*
            Artist_ImageBaseBorder.Height = HLGrid.ActualHeight;
            InfosBaseStackPanel.Margin = new(24, ActualHeight / 2.5, 24, 12);*/
            menu_border.MinHeight = InfosBaseStackPanel.ActualHeight + LittleBarGrid.ActualHeight + 12;
            try { menu_border.Height = ActualHeight - 74; }
            catch { }
            ImageClip.Rect = new(0, 0, Artist_ImageBaseGrid.ActualWidth, Artist_ImageBaseGrid.ActualHeight);
            UpdataShyHeader();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Children.Items.Any()) return;
            foreach (SongItemBindBase songItem in Children.Items)
            {
                App.playingList.Add(songItem.MusicData, false);
            }
            await App.playingList.Play(MusicDataList.First().MusicData, true);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InitData();
        }

        DropShadow dropShadow;
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (Children.SelectionMode == ListViewSelectionMode.None)
            {
                SelectItemButton.Background = App.Current.Resources["AccentAAFillColorTertiaryBrush"] as Brush;
                Children.SelectionMode = ListViewSelectionMode.Multiple;

                SelectorSeparator.Visibility = Visibility.Visible;
                AddSelectedToPlayingListButton.Visibility = Visibility.Visible;
                AddSelectedToPlayListButton.Visibility = Visibility.Visible;
                DeleteSelectedButton.Visibility = Visibility.Visible;
                DownloadSelectedButton.Visibility = Visibility.Visible;
                SelectReverseButton.Visibility = Visibility.Visible;
                SelectAllButton.Visibility = Visibility.Visible;

                Children.AllowDrop = true;
                Children.CanReorderItems = true;
            }
            else
            {
                SelectItemButton.Background = new SolidColorBrush(Colors.Transparent);
                Children.SelectionMode = ListViewSelectionMode.None;

                SelectorSeparator.Visibility = Visibility.Collapsed;
                AddSelectedToPlayingListButton.Visibility = Visibility.Collapsed;
                AddSelectedToPlayListButton.Visibility = Visibility.Collapsed;
                DeleteSelectedButton.Visibility = Visibility.Collapsed;
                DownloadSelectedButton.Visibility = Visibility.Collapsed;
                SelectReverseButton.Visibility = Visibility.Collapsed;
                SelectAllButton.Visibility = Visibility.Collapsed;

                Children.AllowDrop = false;
                Children.CanReorderItems = false;
            }
            UpdataCommandToolBarWidth();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            pageNumber = 1;
            InitData();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            if (pageNumber - 1 > 0)
            {
                pageNumber--;
                InitData();
            }
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            pageNumber++;
            InitData();
        }

        private void AddSelectedToPlayingListButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                foreach (SongItemBindBase item in Children.SelectedItems)
                {
                    App.playingList.Add(item.MusicData);
                }
            }
        }

        private void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                foreach (SongItemBindBase item in Children.SelectedItems) 
                {
                    MusicDataList.Remove(item);
                }

            }
        }

        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            Children.SelectAll();
        }

        private void SelectReverseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SongItemBindBase item in MusicDataList)
            {
                try
                {
                    var a = Children.ContainerFromIndex(Children.Items.IndexOf(item)) as ListViewItem;
                    if (a!=null)
                        a.IsSelected = !a.IsSelected;
                }
                catch { }
            }
        }

        private void AppBarButton_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void DownloadSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (Children.SelectedItems.Any())
            {
                foreach (SongItemBindBase songItem in Children.SelectedItems)
                {
                    App.downloadManager.Add(songItem.MusicData);
                }
            }
        }

        private async void AddToPlayListFlyout_Opened(object sender, object e)
        {
            AddToPlayListFlyout.Items.Clear();
            foreach (var m in await PlayListHelper.ReadAllPlayList())
            {
                var a = new MenuFlyoutItem()
                {
                    Text = m.ListShowName,
                    Tag = m
                };
                a.Click += A_Click;

                AddToPlayListFlyout.Items.Add(a);
            }
        }

        private async void A_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowLoadingDialog();
            var text = JObject.Parse(await PlayListHelper.ReadData());
            foreach (SongItemBindBase item in Children.SelectedItems)
            {
                MainWindow.SetLoadingText($"正在添加：{item.MusicData.Title} - {item.MusicData.ButtonName}");
                
                text = PlayListHelper.AddMusicDataToPlayList(
                    ((sender as MenuFlyoutItem).Tag as MusicListData).ListName,
                    item.MusicData, text);
            }
            await PlayListHelper.SaveData(text.ToString());
            MainWindow.HideDialog();
        }

        private void AddToPlayListFlyout_Closed(object sender, object e)
        {
            //AddToPlayListFlyout.Items.Clear();
        }

        bool isfirst = true;
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isfirst)
            {
                isfirst = false;
                return;
            }
            InitData();
        }
    }
}
