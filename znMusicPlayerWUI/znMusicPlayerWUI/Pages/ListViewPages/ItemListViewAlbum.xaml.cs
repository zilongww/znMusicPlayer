﻿using System;
using System.Linq;
using System.Numerics;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Composition;
using znMusicPlayerWUI.Media;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.Controls;
using znMusicPlayerWUI.DataEditor;
using CommunityToolkit.WinUI.UI;

namespace znMusicPlayerWUI.Pages
{
    public partial class ItemListViewAlbum : Page, IPage
    {
        public bool IsNavigatedOutFromPage { get; set; } = false;
        private ScrollViewer scrollViewer { get; set; }
        public Album NavToObj { get; set; }
        public MusicFrom NowMusicFrom { get; set; } = MusicFrom.neteaseMusic;

        public ItemListViewAlbum()
        {
            InitializeComponent();
            DataContext = this;
            SearchBox.ItemsSource = searchMusicDatas;
            MainWindow.InKeyDownEvent += MainWindow_InKeyDownEvent;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //PlayAllButton.Foreground = new SolidColorBrush(CodeHelper.IsAccentColorDark() ? Colors.White : Colors.Black);
            base.OnNavigatedTo(e);
            IsNavigatedOutFromPage = false;
            Album a = (Album)e.Parameter;
            NavToObj = a;
            musicListData = new() { ListDataType = DataType.专辑 };
            InitData();
            MainWindow.MainViewStateChanged += MainWindow_MainViewStateChanged;
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            IsNavigatedOutFromPage = true;
            await Task.Delay(500);
            MainWindow.MainViewStateChanged -= MainWindow_MainViewStateChanged;
            MainWindow.InKeyDownEvent -= MainWindow_InKeyDownEvent;
            scrollViewer?.ScrollToVerticalOffset(0);

            MusicDataList?.Clear();
            searchMusicDatas.Clear();
            Children.ItemsSource = null;
            SearchBox.ItemsSource = null;

            Album_Image.Dispose(); AlbumLogo.Dispose();
            musicListData = null;

            NavToObj.Songs?.Songs.Clear();
            NavToObj = null;
            UnloadObject(this);
        }

        private void MainWindow_MainViewStateChanged(bool isView)
        {
            AutoScrollViewerControl.Pause = !isView;
        }

        private void CrateShadow()
        {
            var visual = ElementCompositionPreview.GetElementVisual(AlbumLogoRoot);
            compositor = visual.Compositor;

            var basicRectVisual = compositor.CreateSpriteVisual();
            basicRectVisual.Size = AlbumLogoRoot.RenderSize.ToVector2();

            dropShadow = compositor.CreateDropShadow();
            dropShadow.BlurRadius = 45f;
            dropShadow.Color = Colors.Black;
            dropShadow.Opacity = 0.3f;
            dropShadow.Offset = new Vector3(0, 4, 0);

            basicRectVisual.Shadow = dropShadow;
            ElementCompositionPreview.SetElementChildVisual(AlbumLogo_DropShadowBase, basicRectVisual);
        }

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
            LoadingTipControl.ShowLoading();
            var obj = await App.metingServices.NeteaseServices.GetAlbum(NavToObj.ID);
            if (IsNavigatedOutFromPage) return;
            if (obj == null)
            {
                MainWindow.AddNotify("加载专辑信息时出现错误", "无法加载专辑信息，请重试。",
                    NotifySeverity.Error);
                return;
            }
            NavToObj = obj;
            musicListData = NavToObj.Songs;
            if (string.IsNullOrEmpty(obj.Title2))
            {
                Title2_Text.Visibility = Visibility.Collapsed;
            }
            else
                Title2_Text.Text = obj.Title2;

            if (musicListData != null)
            {
                LoadImage();
                DescribeeText.Text = obj.Describee;
                await Task.Delay(100);
                var dpi = CodeHelper.GetScaleAdjustment(App.WindowLocal);

                MusicDataList.Clear();
                int count = 0;
                foreach (var i in musicListData.Songs)
                {
                    count++;
                    i.Count = count;
                    MusicDataList.Add(new() { MusicData = i, ImageScaleDPI = dpi, MusicListData = musicListData, ShowAlbumName = false });
                }
            }
            LoadingTipControl.UnShowLoading();
            Debug.WriteLine("[ItemListViewAlbum] 加载完成");
            await Task.Delay(1000);
            UpdateShyHeader();
        }

        private async void LoadImage()
        {
            AlbumLogo.BorderThickness = new(0);
            if (musicListData.ListDataType == DataType.本地歌单)
            {
                Album_Image.Source = await FileHelper.GetImageSource(musicListData.PicturePath);
            }
            else if (musicListData.ListDataType == DataType.歌单)
            {
                Album_Image.Source = await FileHelper.GetImageSource(await ImageManage.GetImageSource(musicListData));
            }
            else if (musicListData.ListDataType == DataType.专辑)
            {
                var art = NavToObj;
                Album_Image.Source = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(art.PicturePath));
            }
            AlbumLogo.Source = Album_Image.Source;
            AlbumLogo.SaveName = NavToObj.Title;
            AlbumLogo.BorderThickness = new(1);
            UpdateShyHeader();
            await Task.Delay(10);
            UpdateShyHeader();
            await Task.Delay(100);
            UpdateShyHeader();
            await Task.Delay(200);
            UpdateShyHeader();
            Debug.WriteLine("[ItemListViewAlbum] 图片加载完成");
        }

        CompositionPropertySet scrollerPropertySet;
        Compositor compositor;
        Visual headerVisual;
        Visual massAlbumRootVisual;
        Visual blurAlbumRootVisual;
        Visual ImageScrollVisual;
        Visual logoVisual;
        Visual logoShadowVisual;
        Visual infoTextsRootVisual;
        Visual commandbarVisual;
        Visual describeeRootVisual;
        Visual searchBaseVisual;
        public void UpdateShyHeader()
        {
            if (scrollViewer == null) return;

            double anotherHeight = 168;
            String progress = $"Clamp(-scroller.Translation.Y / {anotherHeight}, 0, 1.0)";
            String describeeProgress = $"Clamp(-scroller.Translation.Y / 80, 0, 1.0)";
            String blurProgress = $"Clamp((-scroller.Translation.Y - 20) / {anotherHeight}, 0, 1.0)";
            String massProgress = $"Clamp((-scroller.Translation.Y - 150) / {anotherHeight}, 0, 1.0)";

            if (scrollerPropertySet == null)
            {
                scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
                compositor = scrollerPropertySet.Compositor;
                headerVisual = ElementCompositionPreview.GetElementVisual(menu_border);
                massAlbumRootVisual = ElementCompositionPreview.GetElementVisual(MassAlbumRoot);
                blurAlbumRootVisual = ElementCompositionPreview.GetElementVisual(BlurAlbumRoot);
                ImageScrollVisual = ElementCompositionPreview.GetElementVisual(Album_ImageBaseBorder);
                infoTextsRootVisual = ElementCompositionPreview.GetElementVisual(InfoTextsRoot);
                logoVisual = ElementCompositionPreview.GetElementVisual(AlbumLogoRoot);
                logoShadowVisual = ElementCompositionPreview.GetElementVisual(AlbumLogo_DropShadowBase);
                commandbarVisual = ElementCompositionPreview.GetElementVisual(ToolsCommandBar);
                describeeRootVisual = ElementCompositionPreview.GetElementVisual(DescribeeTextRoot);
                searchBaseVisual = ElementCompositionPreview.GetElementVisual(SearchBase);
                CrateShadow();
            }

            logoVisual.CenterPoint = new(0, logoVisual.Size.Y, 1);
            logoShadowVisual.CenterPoint = new(0, logoVisual.Size.Y, 1);

            var offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {anotherHeight}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            var blurAlbumRootVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            blurAlbumRootVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            blurAlbumRootVisual.StartAnimation("Opacity", blurAlbumRootVisualOpacityAnimation);

            var massAlbumRootVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(1, 0, {progress})");
            massAlbumRootVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            massAlbumRootVisual.StartAnimation("Opacity", massAlbumRootVisualOpacityAnimation);
/*
            var backgroundVisualScaleAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(1, 1, 1), Vector3(1, 0.4, 1), {progress})");
            backgroundVisualScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            backgroundVisual.StartAnimation(nameof(backgroundVisual.Scale), backgroundVisualScaleAnimation);
*/
            var describeeVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(1, 0, {describeeProgress})");
            describeeVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            describeeRootVisual.StartAnimation("Opacity", describeeVisualOpacityAnimation);

            var ImageVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0,0,0), Vector3(0,{anotherHeight / 1.2},0), {progress})");
            ImageVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            ImageScrollVisual.StartAnimation(nameof(ImageScrollVisual.Offset), ImageVisualOffsetAnimation);

            var sizeDouble = 0.391;
            var logoVisualScaleAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(1, 1, 1), Vector3({sizeDouble}, {sizeDouble}, 1), {progress})");
            logoVisualScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoVisual.StartAnimation(nameof(logoVisual.Scale), logoVisualScaleAnimation);
            
            var logoShadowVisualScaleAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(1, 1, 1), Vector3({sizeDouble}, {sizeDouble}, 1), {progress})");
            logoShadowVisualScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            logoShadowVisual.StartAnimation(nameof(logoShadowVisual.Scale), logoShadowVisualScaleAnimation);

            var toolsCommandBarVisualOffsetYAnimation = compositor.CreateExpressionAnimation($"Lerp({(282 - commandbarVisual.Size.Y)}, {114 - commandbarVisual.Size.Y}, {progress})");
            toolsCommandBarVisualOffsetYAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            commandbarVisual.StartAnimation("Offset.Y", toolsCommandBarVisualOffsetYAnimation);

            var infoTextsRootVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3({logoVisual.Size.X + 12},0,0), Vector3({logoVisual.Size.X * sizeDouble + 12},{anotherHeight},0), {progress})");
            infoTextsRootVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            infoTextsRootVisual.StartAnimation(nameof(infoTextsRootVisual.Offset), infoTextsRootVisualOffsetAnimation);

            var searchBaseVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(16,{headerVisual.Size.Y + 12},0), Vector3(16,{anotherHeight + 132 + 12},0), {progress})");
            searchBaseVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            searchBaseVisual.StartAnimation(nameof(searchBaseVisual.Offset), searchBaseVisualOffsetAnimation);
        }

        private async void UpdateCommandToolBarWidth()
        {
            ToolsCommandBar.Width = 0;
            await Task.Delay(1);
            ToolsCommandBar.Width = double.NaN;
        }

        Vector3 ATBOffset = default;
        private void menu_border_Loaded(object sender, RoutedEventArgs e)
        {
            if (scrollViewer == null)
            {
                scrollViewer = (VisualTreeHelper.GetChild(Children, 0) as Border).Child as ScrollViewer;
                scrollViewer.CanContentRenderOutsideBounds = true;
                scrollViewer.ViewChanging += ScrollViewer_ViewChanging;

                // 设置header为顶层
                var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)Children.Header);
                var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
                Canvas.SetZIndex(headerContainer, 1);
            }

            UpdateCommandToolBarWidth();
            Result_BaseGrid_SizeChanged(null, null);
            CrateShadow();
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            UpdateShyHeader();
            if (scrollViewer != null)
                AlbumLogoRoot.CornerRadius = new(Math.Min(Math.Max(scrollViewer.VerticalOffset / 8, 8), 15));
            if (logoVisual != null)
            {
                var a = ActualWidth - (logoVisual.Scale.X * AlbumLogoRoot.ActualWidth + 44);
                if (a > 0)
                {
                    InfoTextsRoot.Width = a;
                    ToolsCommandBar.MaxWidth = a;
                }
            }
            if (headerVisual != null) headerVisual.IsPixelSnappingEnabled = true;
            //BackColorBaseRectangle.Margin = new(0, Math.Min(scrollViewer.VerticalOffset, 180), 0, 0);
        }

        private void Result_BaseGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //menu_border.MinHeight = LittleBarGrid.ActualHeight;
            //try { menu_border.Height = ActualHeight; }
            //catch { }
            //ImageClip.Rect = new(0, 0, ActualWidth, ActualHeight);
            ScrollViewer_ViewChanging(null, null);
            CrateShadow();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Children.Items.Any()) return;
            if (App.playingList.PlayBehavior == znMusicPlayerWUI.Background.PlayBehavior.随机播放)
            {
                App.playingList.ClearAll();
            }
            foreach (var songItem in MusicDataList)
            {
                App.playingList.Add(songItem.MusicData, false);
            }
            await App.playingList.Play(MusicDataList.First().MusicData, true);
            App.playingList.SetRandomPlay(App.playingList.PlayBehavior);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InitData();
        }

        DropShadow dropShadow;
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (SelectItemButton.IsChecked == true)
            {
                PlayAllButton.Visibility = Visibility.Collapsed;
                RefreshButton.Visibility = Visibility.Collapsed;

                SelectorSeparator.Visibility = Visibility.Visible;
                AddSelectedToPlayingListButton.Visibility = Visibility.Visible;
                AddSelectedToPlayListButton.Visibility = Visibility.Visible;
                DeleteSelectedButton.Visibility = Visibility.Visible;
                DownloadSelectedButton.Visibility = Visibility.Visible;
                SelectReverseButton.Visibility = Visibility.Visible;
                SelectAllButton.Visibility = Visibility.Visible;

                Children.SelectionMode = ListViewSelectionMode.Multiple;
                Children.AllowDrop = true;
                Children.CanReorderItems = true;
            }
            else
            {
                PlayAllButton.Visibility = Visibility.Visible;
                RefreshButton.Visibility = Visibility.Visible;

                SelectorSeparator.Visibility = Visibility.Collapsed;
                AddSelectedToPlayingListButton.Visibility = Visibility.Collapsed;
                AddSelectedToPlayListButton.Visibility = Visibility.Collapsed;
                DeleteSelectedButton.Visibility = Visibility.Collapsed;
                DownloadSelectedButton.Visibility = Visibility.Collapsed;
                SelectReverseButton.Visibility = Visibility.Collapsed;
                SelectAllButton.Visibility = Visibility.Collapsed;

                Children.SelectionMode = ListViewSelectionMode.None;
                Children.AllowDrop = false;
                Children.CanReorderItems = false;
            }
            UpdateCommandToolBarWidth();
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
            foreach (SongItemBindBase item in Children.Items)
            {
                if (Children.SelectedItems.Contains(item))
                {
                    Children.SelectedItems.Remove(item);
                }
                else
                {
                    Children.SelectedItems.Add(item);
                }
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
            var text = await PlayListHelper.ReadData();
            foreach (SongItemBindBase item in Children.SelectedItems)
            {
                MainWindow.SetLoadingText($"正在添加：{item.MusicData.Title} - {item.MusicData.ButtonName}");
                
                text = PlayListHelper.AddMusicDataToPlayList(
                    ((sender as MenuFlyoutItem).Tag as MusicListData).ListName,
                    item.MusicData, text);
            }
            await PlayListHelper.SaveData(text);
            MainWindow.HideDialog();
        }

        private void AddToPlayListFlyout_Closed(object sender, object e)
        {
            //AddToPlayListFlyout.Items.Clear();
        }
        private async void Button_Click_6(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Tag)
            {
                case "0":
                    scrollViewer.ChangeView(null, 0, null);
                    break;
                case "1":
                    scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
                    break;
                case "2":
                    foreach (var i in MusicDataList)
                    {
                        if (i.MusicData == App.audioPlayer.MusicData)
                        {
                            await Children.SmoothScrollIntoViewWithItemAsync(i, ScrollItemPlacement.Center);
                            await Children.SmoothScrollIntoViewWithItemAsync(i, ScrollItemPlacement.Center, true);
                            foreach (var j in SongItem.StaticSongItems)
                            {
                                if (j != null)
                                    if (j.MusicData == App.audioPlayer.MusicData)
                                        j.AnimateStroke();
                            }
                        }
                    }
                    break;
            }
        }


        ObservableCollection<SongItemBindBase> searchMusicDatas = new();
        private async void ChangeViewToSearchItem(SongItemBindBase item)
        {
            if (searchMusicDatas.Any())
            {
                searchNum = searchMusicDatas.IndexOf(item) - 1;
                SearchResultTextBlock.Text = $"{searchNum + 1} of {searchMusicDatas.Count}";
                await Children.SmoothScrollIntoViewWithItemAsync(item, ScrollItemPlacement.Center);
                await Children.SmoothScrollIntoViewWithItemAsync(item, ScrollItemPlacement.Center, true);
                foreach (var s in SongItem.StaticSongItems)
                {
                    if (s.MusicData == item.MusicData) s.AnimateStroke();
                }
            }
        }

        private async void ChangeViewToSearchItem(bool add = true)
        {
            if (searchMusicDatas.Any())
            {
                if (add) searchNum++;
                else searchNum--;

                if (searchNum > searchMusicDatas.Count - 1) searchNum = 0;
                if (searchNum <= -1) searchNum = searchMusicDatas.Count - 1;

                var item = searchMusicDatas[searchNum];
                SearchResultTextBlock.Text = $"{searchNum + 1} of {searchMusicDatas.Count}";
                await Children.SmoothScrollIntoViewWithItemAsync(item, ScrollItemPlacement.Center);
                await Children.SmoothScrollIntoViewWithItemAsync(item, ScrollItemPlacement.Center, true);

                foreach (var s in SongItem.StaticSongItems)
                {
                    if (s.MusicData == item.MusicData) s.AnimateStroke();
                }
            }
            else
            {
                SearchResultTextBlock.Text = "0 of 0";
            }
        }

        bool isQuery = false;
        int searchNum = -1;
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ChangeViewToSearchItem();
            AutoSuggestBox_TextChanged(null, new() { Reason = AutoSuggestionBoxTextChangeReason.UserInput });
        }

        private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (SearchModeComboBox.SelectedIndex == 0)
                SearchBox.Text = (args.SelectedItem as SongItemBindBase).MusicData.Title;
            else if (SearchModeComboBox.SelectedIndex == 1)
                SearchBox.Text = (args.SelectedItem as SongItemBindBase).MusicData.Title;
            else if (SearchModeComboBox.SelectedIndex == 2)
                SearchBox.Text = (args.SelectedItem as SongItemBindBase).MusicData.ArtistName;
            else if (SearchModeComboBox.SelectedIndex == 3)
                SearchBox.Text = (args.SelectedItem as SongItemBindBase).MusicData.Album.Title;

            searchNum = searchMusicDatas.IndexOf(args.SelectedItem as SongItemBindBase) - 1;
            SearchResultTextBlock.Text = $"{searchNum + 2} of {searchMusicDatas.Count}";
            //ChangeViewToSearchItem(args.SelectedItem as SongItemBindBase);
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput) // 有 bug
            {
                if (string.IsNullOrEmpty(SearchBox.Text)) return;
                searchMusicDatas.Clear();
                string text = CompareString(SearchBox.Text);

                switch (SearchModeComboBox.SelectedIndex)
                {
                    case 0:
                        foreach (var i in MusicDataList)
                        {
                            if (CompareString(i.MusicData.Title).Contains(text))
                                searchMusicDatas.Add(i);
                            else if (i.MusicData.Title2 is not null)
                            {
                                if (CompareString(i.MusicData.Title2).Contains(text))
                                    searchMusicDatas.Add(i);
                            }
                            else if (CompareString(i.MusicData.ArtistName).Contains(text))
                                searchMusicDatas.Add(i);
                            else if (CompareString(i.MusicData.Album.Title).Contains(text))
                                searchMusicDatas.Add(i);
                        }
                        break;
                    case 1:
                        foreach (var i in MusicDataList)
                        {
                            if (CompareString(i.MusicData.Title).Contains(text))
                                searchMusicDatas.Add(i);
                            else if (i.MusicData.Title2 is not null)
                            {
                                if (CompareString(i.MusicData.Title2).Contains(text))
                                    searchMusicDatas.Add(i);
                            }
                        }
                        break;
                    case 2:
                        foreach (var i in MusicDataList)
                        {
                            if (CompareString(i.MusicData.ArtistName).Contains(text))
                                searchMusicDatas.Add(i);
                        }
                        break;
                    case 3:
                        foreach (var i in MusicDataList)
                        {
                            if (CompareString(i.MusicData.Album.Title).Contains(text))
                                searchMusicDatas.Add(i);
                        }
                        break;
                }
                searchNum = 0;
                SearchResultTextBlock.Text = $"All of {searchMusicDatas.Count}";
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchBase.IsHitTestVisible)
            {
                SearchBase.Opacity = 0;
                menu_border.Margin = new(0, 0, 0, 12);
            }
            else
            {
                SearchBase.Opacity = 1;
                menu_border.Margin = new(0, 0, 0, searchBaseVisual.Size.Y + 12 + 12);
                SearchBox.Focus(FocusState.Keyboard);
            }
            SearchBase.IsHitTestVisible = !SearchBase.IsHitTestVisible;
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Tag as string == "0")
            {
                ChangeViewToSearchItem(false);
            }
            else
            {
                ChangeViewToSearchItem();
            }
        }

        private string CompareString(string str)
        {
            return (bool)LowerCheckBox.IsChecked ? str : str?.ToLower();
        }

        private void MainWindow_InKeyDownEvent(Windows.System.VirtualKey key)
        {
            if (MainWindow.isControlDown)
            {
                if (key == Windows.System.VirtualKey.F)
                {
                    SearchButton_Click(null, null);
                    if (!SearchBase.IsHitTestVisible)
                        ToolsCommandBar.Focus(FocusState.Programmatic);
                }
            }
        }
    }
}
