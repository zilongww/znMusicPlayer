using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using znMusicPlayerWUI.Helpers;
using NAudio.Wave;
using Microsoft.UI.Xaml.Media;
using znMusicPlayerWUI.Controls;
using System.Diagnostics;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Windowing;
using znMusicPlayerWUI.DataEditor;
using Windows.System;
using Microsoft.UI.Input;
using System.Xml.Linq;
using CommunityToolkit.WinUI.UI;
using System.Runtime.Intrinsics.Arm;
using CommunityToolkit.WinUI.UI.Animations;

namespace znMusicPlayerWUI.Pages.MusicPages
{
    public partial class MusicPage : Page, IMusicPage
    {
        private GestureRecognizer _recognizer;
        public MusicPageViewStateChangeDelegate MusicPageViewStateChange { get; set; }

        public string Title
        {
            get => App.audioPlayer.MusicData.Title;
        }

        private bool _showLrcPage = true;
        public bool ShowLrcPage
        {
            get => _showLrcPage;
            set
            {/*
                if (value == _showLrcPage) return;
                _showLrcPage = value;
                if (LrcButton.IsChecked != value) LrcButton.IsChecked = value;

                if (value)
                {
                    LrcPageColumn.MaxWidth = double.MaxValue;
                    LrcBaseGrid.Visibility = Visibility.Visible;
                    SelectedChangedDo();
                }
                else
                {
                    LrcPageColumn.MaxWidth = 0;
                    LrcBaseGrid.Visibility = Visibility.Collapsed;
                }
                UpdataInterfaceDesign();*/
            }
        }

        private TextAlignment _lrcTextAlignment = TextAlignment.Left;
        public TextAlignment LrcTextAlignment
        {
            get => _lrcTextAlignment;
            set
            {
                _lrcTextAlignment = value;
            }
        }

        private ImageSource _imageSources = null;
        public ImageSource ImageSources
        {
            get => _imageSources;
            set
            {
                _imageSources = value;
            }
        }

        public MusicPage()
        {
            InitializeComponent();
            MusicPageViewStateChange = new MusicPageViewStateChangeDelegate(ViewChange);
            DataContext = this;
            SizeChanged += MusicPage_SizeChanged;

            UpdataInterfaceDesign();
        }

        private void UpdataWhenDataLated()
        {
            AudioPlayer_SourceChanged(App.audioPlayer);
            AudioPlayer_PlayStateChanged(App.audioPlayer);
            AudioPlayer_CacheLoadedChanged(App.audioPlayer);
            AudioPlayer_TimingChanged(App.audioPlayer);
            PlayingList_NowPlayingImageLoaded(App.playingList.NowPlayingImage, null);
            isCodeChangedLrcItem = true;
            LyricManager_PlayingLyricSelectedChange1(App.lyricManager.NowLyricsData);
            isCodeChangedLrcItem = false;
            App.audioPlayer.ReCallTiming();
            //Debug.WriteLine("MusicPage Updataed Events.");
        }

        bool isAddEvents = false;
        private void AddEvents()
        {
            if (isAddEvents) return;
            App.audioPlayer.SourceChanged += AudioPlayer_SourceChanged;
            App.audioPlayer.PlayStateChanged += AudioPlayer_PlayStateChanged;
            App.audioPlayer.TimingChanged += AudioPlayer_TimingChanged;
            App.audioPlayer.PlayEnd += AudioPlayer_PlayEnd;
            App.audioPlayer.CacheLoadingChanged += AudioPlayer_CacheLoadingChanged;
            App.audioPlayer.CacheLoadedChanged += AudioPlayer_CacheLoadedChanged;
            App.playingList.NowPlayingImageLoading += PlayingList_NowPlayingImageLoading;
            App.playingList.NowPlayingImageLoaded += PlayingList_NowPlayingImageLoaded;
            isAddEvents = true;
            UpdataWhenDataLated();
            //Debug.WriteLine("MusicPage Added Events.");
        }

        private void RemoveEvents()
        {
            App.audioPlayer.SourceChanged -= AudioPlayer_SourceChanged;
            App.audioPlayer.PlayStateChanged -= AudioPlayer_PlayStateChanged;
            App.audioPlayer.TimingChanged -= AudioPlayer_TimingChanged;
            App.audioPlayer.PlayEnd -= AudioPlayer_PlayEnd;
            App.audioPlayer.CacheLoadingChanged -= AudioPlayer_CacheLoadingChanged;
            App.audioPlayer.CacheLoadedChanged -= AudioPlayer_CacheLoadedChanged;
            App.playingList.NowPlayingImageLoading -= PlayingList_NowPlayingImageLoading;
            App.playingList.NowPlayingImageLoaded -= PlayingList_NowPlayingImageLoaded;
            App.lyricManager.PlayingLyricSourceChange -= LyricManager_PlayingLyricSourceChange;
            App.lyricManager.PlayingLyricSelectedChange -= LyricManager_PlayingLyricSelectedChange1;
            isAddEvents = false;
            //Debug.WriteLine("MusicPage Removed Events.");
        }

        public MusicPageViewState ViewState = MusicPageViewState.Hidden;
        private async void ViewChange(MusicPageViewState musicPageViewState)
        {
            ViewState = musicPageViewState;
            if (ViewState == MusicPageViewState.View)
            {
                LrcBaseListView.ItemsSource = App.lyricManager.NowPlayingLyrics;
                AddEvents();
            }
            else
            {
                ConnectedAnimation canimation =
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("upAnimation", AlbumImageBorder);
                canimation.Configuration = new BasicConnectedAnimationConfiguration();
                ConnectedAnimation canimation1 =
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("upAnimation1", TitleRunText);
                canimation1.Configuration = new BasicConnectedAnimationConfiguration();
                ConnectedAnimation canimation2 =
                    ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("upAnimation2", ArtistRunText);
                canimation2.Configuration = new BasicConnectedAnimationConfiguration();
                if (App.lyricManager.NowPlayingLyrics.Any())
                {
                    var e = (isMiniPage ? LrcSecondListView : LrcBaseListView).ContainerFromIndex(LrcBaseListView.SelectedIndex) as UIElement;
                    if (e != null)
                    {
                        ConnectedAnimation canimation3 =
                            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("upAnimation3", e);
                        canimation3.Configuration = new BasicConnectedAnimationConfiguration();
                    }
                }

                RemoveEvents();
                RemoveLyricListItemSourceAsync();
            }
#if DEBUG
            Debug.WriteLine($"MusicPage: ViewState 已被设置为 {musicPageViewState}.");
#endif
        }

        private async void RemoveLyricListItemSourceAsync()
        {
            await Task.Delay(200);
            if (ViewState == MusicPageViewState.Hidden)
            {
                LrcBaseListView.ItemsSource = null;
#if DEBUG
                Debug.WriteLine($"MusicPage: LrcBaseListView.ItemSource 已被设置为 null.");
#endif
            }
        }

        private void PlayingList_NowPlayingImageLoading(ImageSource imageSource, string _)
        {
            /*if (App.audioPlayer.MusicData?.AlbumID != MusicData?.AlbumID)
            {
                if (BackgroundBaseImage.Source != null)
                    BackgroundBaseImageAnimate.Source = null;
                BackgroundBaseImageAnimate.Source = BackgroundBaseImage.Source;
                BackgroundBaseImage.Source = null;
                //AlbumImageBase.Dispose();
            }*/
        }

        private void PlayingList_NowPlayingImageLoaded(ImageSource imageSource, string _)
        {
            if (imageSource == ImageSources) return;
            ImageSources = imageSource;
            BackgroundBaseImage.Source = ImageSources;
            AlbumImageBase.Source = imageSource;
#if DEBUG
            Debug.WriteLine($"MusicPage: 图片已被更改.");
#endif

        }

        private void BackgroundBaseImage_Loaded(object sender, RoutedEventArgs e)
        {
            CreatBlurVisualToImage();
        }

        Microsoft.Graphics.Canvas.Effects.GaussianBlurEffect blur = new Microsoft.Graphics.Canvas.Effects.GaussianBlurEffect()
        {
            Source = new Microsoft.UI.Composition.CompositionEffectSourceParameter("Source"),
            BlurAmount = 250
        };
        private void CreatBlurVisualToImage()
        {
            return;
            var imageVisual = ElementCompositionPreview.GetElementVisual(BackgroundBaseImage);
            var compositor = imageVisual.Compositor;
            var blurFactory = compositor.CreateEffectFactory(blur);
            var blurBrush = blurFactory.CreateBrush();
            blurBrush.SetSourceParameter("Source", compositor.CreateBackdropBrush());
            var blurVisual = compositor.CreateSpriteVisual();
            blurVisual.Brush = blurBrush;
            blurVisual.RelativeSizeAdjustment = new System.Numerics.Vector2(1f, 1f);
            ElementCompositionPreview.SetElementChildVisual(BackgroundBaseImage, blurVisual);
        }

        bool isMiniPage = false;
        bool isMiniPageOnlyLyric = false;
        bool isMiniPageLyricCenter = false;
        public void UpdataInterfaceDesign()
        {
            if (!ShowLrcPage)
            {
                if (InfoBaseGrid.ActualHeight >= 850)
                {
                }
                else
                {
                }
            }
            else
            {
                if (ActualWidth >= 900)
                {
                    isMiniPage = false;
                    LyricSccondRow.Height = new(0);
                    LrcPageColumn.Width = new(1.4, GridUnitType.Star);
                    BridgeTb.TextAlignment = TextAlignment.Left;
                    InfoBaseGrid.Margin = new(0, 0, 30, 0);

                    ImageVer.Height = new(1, GridUnitType.Star);
                    AlbumImageBorder.VerticalAlignment = VerticalAlignment.Center;
                    AlbumImageBorder.MaxWidth = double.MaxValue;
                    AlbumImageBorder.MaxHeight = double.MaxValue;
                }
                else
                {
                    isMiniPage = true;
                    LyricSccondRow.Height = new(1, GridUnitType.Star);
                    LrcPageColumn.Width = new(0);
                    BridgeTb.TextAlignment = TextAlignment.Center;
                    InfoBaseGrid.Margin = new(0);
                    AlbumImageBorder.VerticalAlignment = VerticalAlignment.Top;
                    AlbumImageBorder.MaxWidth = InfoBaseGrid.ActualHeight;
                    AlbumImageBorder.MaxHeight = InfoBaseGrid.ActualHeight / 1.5;

                    if (InfoBaseGrid.ActualHeight <= 616 || InfoBaseGrid.ActualWidth <= 160)
                    {
                        isMiniPageOnlyLyric = true;
                        ImageVer.Height = new(0, GridUnitType.Pixel);
                    }
                    else
                    {
                        isMiniPageOnlyLyric = false;
                        ImageVer.Height = new(1, GridUnitType.Auto);
                    }

                    if (LyricSecondPlaceGrid.ActualHeight >= 260 || isMiniPageOnlyLyric)
                        isMiniPageLyricCenter = true;
                    else
                        isMiniPageLyricCenter = false;
                }
                SelectedChangedDo(true);
            }
        }

        public async void SelectedChangedDo(bool disableAnimation = false)
        {
            if (App.lyricManager.NowLyricsData == null) return;

            isCodeChangedLrcItem = true;
            LrcBaseListView.SelectedItem = App.lyricManager.NowLyricsData;
            LrcSecondListView.SelectedItem = App.lyricManager.NowLyricsData;
            isCodeChangedLrcItem = false;

            var sv = isMiniPage ? scrollViewer1 : scrollViewer;
            if (sv != null && !inScroll && App.lyricManager.NowLyricsData.Lyric != null)
            {
                var c = isMiniPage ?
                    LrcSecondListView.ContainerFromIndex(LrcBaseListView.SelectedIndex) as UIElement :
                    LrcBaseListView.ContainerFromIndex(LrcBaseListView.SelectedIndex) as UIElement;
                if (c == null)
                {
                    if (!isMiniPage)
                    {
                        LrcBaseListView.ScrollIntoView(App.lyricManager.NowLyricsData, ScrollIntoViewAlignment.Default);
                        c = LrcBaseListView.ContainerFromIndex(LrcBaseListView.SelectedIndex) as UIElement;
                        LrcBaseListView.ScrollIntoView(App.lyricManager.NowLyricsData, ScrollIntoViewAlignment.Default);
                    }
                    else
                    {
                        LrcSecondListView.ScrollIntoView(App.lyricManager.NowLyricsData, ScrollIntoViewAlignment.Default);
                        c = LrcSecondListView.ContainerFromIndex(LrcSecondListView.SelectedIndex) as UIElement;
                        LrcSecondListView.ScrollIntoView(App.lyricManager.NowLyricsData, ScrollIntoViewAlignment.Default);
                    }
                }
                if (c != null)
                {
                    if (!isMiniPage)
                    {
                        //ScrollViewerBehavior.(scrollViewer, c.ActualOffset.Y + c.ActualSize.Y / 2 + LrcBaseListView.ActualHeight / 25 + 48);
                        //sv.GetAnimationBaseValue(sv.ver);
                        sv.ChangeView(null, c.ActualOffset.Y + c.ActualSize.Y / 2, null, disableAnimation);
                    }
                    else
                    {
                        if (isMiniPageLyricCenter)
                            await LrcSecondListView.SmoothScrollIntoViewWithItemAsync(App.lyricManager.NowLyricsData, ScrollItemPlacement.Center, disableAnimation);
                        else
                            await LrcSecondListView.SmoothScrollIntoViewWithItemAsync(App.lyricManager.NowLyricsData, ScrollItemPlacement.Top, disableAnimation);
                    }
                }
            }
#if DEBUG
            //Debug.WriteLine($"MusicPage: 选中歌词已被更改为: {App.lyricManager.NowLyricsData?.Lyric[0]}");
#endif
        }

        //todo：优化性能
        private void LyricManager_PlayingLyricSelectedChange1(LyricData nowLyricsData)
        {
            if (ShowLrcPage)
            {
                if (nowLyricsData != null) App.lyricManager.ReCallUpdata();
                SelectedChangedDo();
            }
        }

        private void LyricManager_PlayingLyricSourceChange(System.Collections.ObjectModel.ObservableCollection<DataEditor.LyricData> nowPlayingLyrics)
        {

        }

        private void MusicPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdataInterfaceDesign();

            CloseMusicPageButton.Width = MainWindow.SNavView.DisplayMode == NavigationViewDisplayMode.Minimal ? 86 : 44;
        }

        MusicData MusicData;
        private void AudioPlayer_SourceChanged(Media.AudioPlayer audioPlayer)
        {
            if (audioPlayer.MusicData == null) return;
            TitleRunText.Text = audioPlayer.MusicData.Title;
            ArtistRunText.Text = audioPlayer.MusicData.ArtistName;
            AlbumRunText.Text = audioPlayer.MusicData.Album.Title;
            OtherRunText.Text = audioPlayer.MusicData.From.ToString();
            AudioInfoRunText.Text = audioPlayer.WaveInfo;

            if (audioPlayer.MusicData != MusicData)
            {
                MusicData = audioPlayer.MusicData;
            }
        }

        private void AudioPlayer_PlayStateChanged(Media.AudioPlayer audioPlayer)
        {
            MediaPlayStateViewer1.PlaybackState = audioPlayer.PlaybackState;

            if (audioPlayer.PlaybackState == PlaybackState.Playing)
            {
                App.lyricManager.PlayingLyricSourceChange += LyricManager_PlayingLyricSourceChange;
                App.lyricManager.PlayingLyricSelectedChange += LyricManager_PlayingLyricSelectedChange1;
            }
            else
            {
                App.lyricManager.PlayingLyricSourceChange -= LyricManager_PlayingLyricSourceChange;
                App.lyricManager.PlayingLyricSelectedChange -= LyricManager_PlayingLyricSelectedChange1;
            }
        }

        private void AudioPlayer_TimingChanged(Media.AudioPlayer audioPlayer)
        {
            if (audioPlayer.FileReader != null)
            {
                isCodeChangedSliderValue = true;
                PlaySlider.Minimum = 0;
                PlaySlider.Maximum = audioPlayer.TotalTime.Ticks;
                PlaySlider.Value = audioPlayer.CurrentTime.Ticks;
                isCodeChangedSliderValue = false;
                NowPlayTimeTb.Text =
                    $"{audioPlayer.CurrentTime:mm\\:ss}/{audioPlayer.TotalTime.ToString(@"mm\:ss")}";
                NowAtherTimeTb.Text =
                    (audioPlayer.TotalTime - audioPlayer.CurrentTime).ToString(@"mm\:ss");
            }
        }

        private void AudioPlayer_PlayEnd(Media.AudioPlayer audioPlayer)
        {
            PlaySlider.Maximum = 0;
            PlaySlider.Value = 0;
        }

        private void AudioPlayer_CacheLoadingChanged(Media.AudioPlayer audioPlayer, object data)
        {
            isCodeChangedSliderValue = true;
            PlayButton.IsEnabled = true;
            AudioLoadingProressRing.IsIndeterminate = true;
        }

        private void AudioPlayer_CacheLoadedChanged(Media.AudioPlayer audioPlayer)
        {
            isCodeChangedSliderValue = false;
            PlayButton.IsEnabled = true;
            AudioLoadingProressRing.IsIndeterminate = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenOrCloseMusicPage();
        }

        private async void BeforeButton_Click(object sender, RoutedEventArgs e)
        {
            await App.playingList.PlayPrevious();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.audioPlayer.PlaybackState == PlaybackState.Playing)
            {
                App.audioPlayer.SetPause();
            }
            else
            {
                App.audioPlayer.SetPlay();
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            await App.playingList.PlayNext();
        }

        private void PlayListButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenOrClosePlayingList(
                HorizontalAlignment.Left,
                flyoutPlacementMode: Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.LeftEdgeAlignedBottom,
                placementMargin: new(30, 0, 0, ControlBar.ActualHeight - PlaySlider.ActualHeight + 8));
        }

        private void VloumeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenOrCloseVolume(
                HorizontalAlignment.Left,
                flyoutPlacementMode: Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.LeftEdgeAlignedBottom,
                placementMargin: new(30, 0, 0, ControlBar.ActualHeight - PlaySlider.ActualHeight + 8));
        }

        static ScrollViewer scrollViewer = null;
        static ScrollViewer scrollViewer1 = null;
        private void LrcBaseListView_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void LrcBaseListView_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            ScrollingLrcView();
        }

        private void LrcBaseListView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(LrcBaseListView).PointerDeviceType != PointerDeviceType.Mouse)
                ScrollingLrcView();
            //e.Handled = true;
            //System.Diagnostics.Debug.WriteLine("M");
            //_recognizer.ProcessMoveEvents(e.GetIntermediatePoints(LrcBaseListView));
        }

        private void Recognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("U");
            /*double destY = (PlaybackDetailFrame.RenderTransform as CompositeTransform).TranslateY + args.Delta.Translation.Y;
            if (destY <= 0) destY = 0;
            if (destY >= ActualHeight) destY = ActualHeight;
            (PlaybackDetailFrame.RenderTransform as CompositeTransform).TranslateY = destY;*/
        }

        private void Recognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("C");
            /*
            double destY = (PlaybackDetailFrame.RenderTransform as CompositeTransform).TranslateY;
            SetPlaybackDetailFrameVisibility(destY <= ActualHeight * 0.4);*/
        }

        async void ScrollingLrcView()
        {
            scrollCount++;
            inScroll = true;
            await Task.Delay(2500);
            if (scrollCount <= 1)
            {
                inScroll = false;
                SelectedChangedDo();
            }
            scrollCount--;
        }

        bool inScroll = false;
        int scrollCount = 0;
        bool isCodeChangedLrcItem = false;
        bool isCodeScrollLrcViewer = false;
        int changeCount = 0;
        private void LrcBaseListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changeCount <= 1)
            {
                changeCount++;
                return;
            }

            var lrcItem = (sender as ListView).SelectedItem as LyricData;
            if (lrcItem != null && !isCodeChangedLrcItem)
            {
                if (lrcItem.Lyric == null) return;
                // 加1ms，否则会短时间判定到上一句歌词
                App.audioPlayer.CurrentTime = lrcItem.LyricTimeSpan + TimeSpan.FromMilliseconds(App.audioPlayer.Latency + 1);
#if DEBUG
                Debug.WriteLine("audio player time setted.");
#endif
            }
        }

        private void LrcBaseListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LrcBaseListView.Padding = new Thickness(0, LrcBaseListView.ActualHeight / 2, 0, LrcBaseListView.ActualHeight / 2);
            LrcSecondListView.Padding = new Thickness(0, LrcSecondListView.ActualHeight / 2, 0, LrcSecondListView.ActualHeight / 2);
        }

        private void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //ShowLrcPage = ShowLrcPage;
            var a = VisualTreeHelper.GetChild(LrcBaseListView, 0) as Border;
            var b = VisualTreeHelper.GetChild(LrcSecondListView, 0) as Border;
            if (a != null)
                scrollViewer = a.Child as ScrollViewer;
            if (b != null)
                scrollViewer1 = b.Child as ScrollViewer;
            scrollViewer.CanContentRenderOutsideBounds = false;
            scrollViewer1.CanContentRenderOutsideBounds = false;

            UpdataInterfaceDesign();
        }

        private void LrcButton_Checked(object sender, RoutedEventArgs e)
        {
            ShowLrcPage = true;
        }

        private void LrcButton_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowLrcPage = false;
        }

        bool isCodeChangedSliderValue = false;
        bool isFirstChangeValue = true;
        private void PlaySlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (isFirstChangeValue)
            {
                isFirstChangeValue = false;
                return;
            }
            if (!isCodeChangedSliderValue && !isCodeChangedLrcItem && App.audioPlayer.FileReader != null)
            {
                App.audioPlayer.CurrentTime = TimeSpan.FromTicks((long)PlaySlider.Value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenOrCloseMusicPage();
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.AppWindowLocal.Presenter.Kind == AppWindowPresenterKind.FullScreen)
            {
                FullScreenIcon.Visibility = Visibility.Visible;
                UnFullScreenIcon.Visibility = Visibility.Collapsed;
                App.AppWindowLocal.SetPresenter(AppWindowPresenterKind.Default);
            }
            else
            {
                FullScreenIcon.Visibility = Visibility.Collapsed;
                UnFullScreenIcon.Visibility = Visibility.Visible;
                App.AppWindowLocal.SetPresenter(AppWindowPresenterKind.FullScreen);
            }
        }

        private async void EqButton_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.ShowEqualizerDialog();
        }

        private void InfoBaseTitle_Loaded(object sender, RoutedEventArgs e)
        {/*
            foreach (var data in App.audioPlayer.MusicData.Artists)
            {
                var item = new MenuFlyoutItem()
                {
                    Text = data.Name,
                    Tag = data
                };
                item.Click += (sender, e) =>
                {
                    ListViewPages.ListViewPage.SetPageToListViewPage<ItemListViewArtist>((sender as MenuFlyoutItem).Tag);
                    MainWindow.OpenOrCloseMusicPage();
                };
                ArtistFlyout.Items.Add(item);
            }*/
        }

        private void ArtistFlyout_Opening(object sender, object e)
        {
            ArtistFlyout.Items.Clear();
            foreach (var data in App.audioPlayer.MusicData.Artists)
            {
                var item = new MenuFlyoutItem()
                {
                    Text = data.Name,
                    Tag = data
                };
                item.Click += (sender, e) =>
                {
                    ListViewPages.ListViewPage.SetPageToListViewPage<ItemListViewArtist>((sender as MenuFlyoutItem).Tag);
                    MainWindow.OpenOrCloseMusicPage();
                };
                ArtistFlyout.Items.Add(item);
            }
        }

        private void ArtistRunButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TitleRunButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AlbumRunButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void OtherRunButton_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = null;
            switch (MusicData.From)
            {
                case MusicFrom.neteaseMusic:
                    uri = new($"https://music.163.com/#/song?id={MusicData.ID}");
                    break;
            }

            if (uri != null)
            {
                var success = await Launcher.LaunchUriAsync(uri);
            }
        }

        private void TitleSearchMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListViewPages.ListViewPage.SetPageToListViewPage<ItemListViewSearch>(
                new List<object>
                {
                    MusicData.Title,
                    MusicData.From,
                    SearchDataType.歌曲
                });
            MainWindow.OpenOrCloseMusicPage();
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dp.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            dp.SetText(MusicData.Title);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
        }

        private async void AlbumMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ListViewPages.ListViewPage.SetPageToListViewPage<ItemListViewAlbum>(MusicData.Album);
            MainWindow.OpenOrCloseMusicPage();
        }

        private async void AudioInfoButton_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.ShowDialog("播放信息", new DialogPages.AudioInfoPage());
        }

        private void TitleFlyout_Opening(object sender, object e)
        {
            TitleMenuFlyoutText.Text = MusicData.Title;
        }

        private void AlbumFlyout_Opening(object sender, object e)
        {
            AlbumFlyout.Items.Clear();
            var a = new MenuFlyoutItem() { Text = "复制" };
            var a1 = new MenuFlyoutItem() { Text = "打开" };
            a.Click += A_Click;
            a1.Click += A_Click;
            AlbumFlyout.Items.Add(a);
            AlbumFlyout.Items.Add(a1);
        }

        private void A_Click(object sender, RoutedEventArgs e)
        {
            var a = sender as MenuFlyoutItem;
            if (a != null)
            {
                if  (a.Text == "复制")
                {
                    var dp = new Windows.ApplicationModel.DataTransfer.DataPackage();
                    dp.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                    dp.SetText(MusicData.Album.Title);
                    Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dp);
                }
                else
                {
                    ListViewPages.ListViewPage.SetPageToListViewPage<ItemListViewAlbum>(MusicData.Album);
                    MainWindow.OpenOrCloseMusicPage();
                }
            }
        }
    }
}
