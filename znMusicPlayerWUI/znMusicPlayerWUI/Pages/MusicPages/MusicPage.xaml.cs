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

namespace znMusicPlayerWUI.Pages.MusicPages
{
    public partial class MusicPage : Page, IMusicPage
    {
        private GestureRecognizer _recognizer;
        public MusicPageViewStateChangeDelegate MusicPageViewStateChange { get; set; }

        public string Title
        {
            get => App.playingList.NowPlayingMusicData.Title;
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
            App.audioPlayer.SourceChanged += AudioPlayer_SourceChanged;
            App.audioPlayer.PlayStateChanged += AudioPlayer_PlayStateChanged;
            App.audioPlayer.TimingChanged += AudioPlayer_TimingChanged;
            App.audioPlayer.PlayEnd += AudioPlayer_PlayEnd;
            App.audioPlayer.CacheLoadingChanged += AudioPlayer_CacheLoadingChanged;
            App.audioPlayer.CacheLoadedChanged += AudioPlayer_CacheLoadedChanged;
            App.lyricManager.PlayingLyricSourceChange += LyricManager_PlayingLyricSourceChange;
            App.lyricManager.PlayingLyricSelectedChange += LyricManager_PlayingLyricSelectedChange1;
            App.playingList.NowPlayingImageLoading += PlayingList_NowPlayingImageLoading;
            App.playingList.NowPlayingImageLoaded += PlayingList_NowPlayingImageLoaded;
            SizeChanged += MusicPage_SizeChanged;

            LrcBaseListView.ItemsSource = App.lyricManager.NowPlayingLyrics;
            UpdataInterfaceDesign();
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
            if (ViewState == MusicPageViewState.View)
            {
                if (BackgroundBaseImage.Source != imageSource)
                {
                    ImageSources = imageSource;
                    if (App.audioPlayer?.MusicData?.AlbumID != MusicData?.AlbumID) return;
                    BackgroundBaseImage.Source = ImageSources;
                    AlbumImageBase.Source = imageSource;
#if DEBUG
                    Debug.WriteLine($"MusicPage: 图片已被更改.");
#endif
                }
            }
        }

        public MusicPageViewState ViewState = MusicPageViewState.Hidden;
        private void ViewChange(MusicPageViewState musicPageViewState)
        {
            ViewState = musicPageViewState;
            //System.Diagnostics.Debug.WriteLine(viewState);
            if (ViewState == MusicPageViewState.View)
            {
                AudioPlayer_SourceChanged(App.audioPlayer);
                AudioPlayer_PlayStateChanged(App.audioPlayer);
                PlayingList_NowPlayingImageLoaded(App.playingList.NowPlayingImage, null);

                if (ShowLrcPage)
                {
                    //LrcBaseListView.SelectedItem = LrcBaseListView.Items.Last();
                    SelectedChangedDo();
                }
            }
#if DEBUG
            Debug.WriteLine($"MusicPage: ViewState 已被设置为 {musicPageViewState}.");
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
            return;/*
            var imageVisual = ElementCompositionPreview.GetElementVisual(BackgroundBaseImage);
            var compositor = imageVisual.Compositor;
            var blurFactory = compositor.CreateEffectFactory(blur);
            var blurBrush = blurFactory.CreateBrush();
            blurBrush.SetSourceParameter("Source", compositor.CreateBackdropBrush());
            var blurVisual = compositor.CreateSpriteVisual();
            blurVisual.Brush = blurBrush;
            blurVisual.RelativeSizeAdjustment = new System.Numerics.Vector2(1f, 1f);
            ElementCompositionPreview.SetElementChildVisual(BackgroundBaseImage, blurVisual);

            var imageVisual1 = ElementCompositionPreview.GetElementVisual(BackgroundBaseImageAnimate);
            var compositor1 = imageVisual1.Compositor;
            var blurFactory1 = compositor1.CreateEffectFactory(blur);
            var blurBrush1 = blurFactory1.CreateBrush();
            blurBrush1.SetSourceParameter("Source", compositor1.CreateBackdropBrush());
            var blurVisual1 = compositor1.CreateSpriteVisual();
            blurVisual1.Brush = blurBrush1;
            blurVisual1.RelativeSizeAdjustment = new System.Numerics.Vector2(1f, 1f);
            ElementCompositionPreview.SetElementChildVisual(BackgroundBaseImageAnimate, blurVisual1);*/
        }

        int updataCount = 0;
        async void UpdatingInterfaceDesign()
        {
            updataCount++;
            await Task.Delay(150);
            if (updataCount <= 1)
            {
                try
                {
                    UpdataInterfaceDesign();
                }
                catch { }
            }
            updataCount--;
        }

        bool isMiniPage = false;
        public async void UpdataInterfaceDesign()
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
                if (ActualWidth >= 1000)
                {
                    isMiniPage = false;
                    LyricSccondRow.Height = new(0);
                    LrcPageColumn.Width = new(1.4, GridUnitType.Star);
                    BridgeTb.TextAlignment = TextAlignment.Left;
                    InfoBaseGrid.Margin = new(0, 0, 30, 0);
                }
                else
                {
                    isMiniPage = true;
                    LyricSccondRow.Height = new(0.65, GridUnitType.Star);
                    LrcPageColumn.Width = new(0);
                    BridgeTb.TextAlignment = TextAlignment.Center;
                    InfoBaseGrid.Margin = new(0);
                }
                await Task.Delay(1);
                SelectedChangedDo(true);
            }
        }

        public async void SelectedChangedDo(bool disableAnimation = false)
        {
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
                        sv.ChangeView(null, c.ActualOffset.Y + c.ActualSize.Y / 2 + LrcBaseListView.ActualHeight / 25 + 48, null, disableAnimation);
                    else
                        await LrcSecondListView.SmoothScrollIntoViewWithItemAsync(App.lyricManager.NowLyricsData, ScrollItemPlacement.Top, disableAnimation);
                }
            }
#if DEBUG
            //Debug.WriteLine($"MusicPage: 选中歌词已被更改为: {App.lyricManager.NowLyricsData?.Lyric[0]}");
#endif
        }

        //todo：优化性能
        private void LyricManager_PlayingLyricSelectedChange1(LyricData nowLyricsData)
        {
            if (ShowLrcPage && ViewState == MusicPageViewState.View)
            {
                SelectedChangedDo();
            }
        }

        private void LyricManager_PlayingLyricSourceChange(System.Collections.ObjectModel.ObservableCollection<DataEditor.LyricData> nowPlayingLyrics)
        {

        }

        private void MusicPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatingInterfaceDesign();

            CloseMusicPageButton.Width = MainWindow.SNavView.DisplayMode == NavigationViewDisplayMode.Minimal ? 86 : 44;
        }

        MusicData MusicData;
        private void AudioPlayer_SourceChanged(Media.AudioPlayer audioPlayer)
        {
            if (ViewState == MusicPageViewState.Hidden || audioPlayer.MusicData == null) return;
            TitleRunText.Text = audioPlayer.MusicData.Title;
            ArtistRunText.Text = audioPlayer.MusicData.ArtistName;
            AlbumRunText.Text = audioPlayer.MusicData.Album;
            OtherRunText.Text = audioPlayer.MusicData.From.ToString();
            AudioInfoRunText.Text = audioPlayer.WaveInfo;

            if (audioPlayer.MusicData != MusicData)
            {
                MusicData = audioPlayer.MusicData;
            }
        }

        private void AudioPlayer_PlayStateChanged(Media.AudioPlayer audioPlayer)
        {
            if (ViewState == MusicPageViewState.Hidden) return;
            MediaPlayStateViewer1.PlaybackState = audioPlayer.PlaybackState;
        }

        private void AudioPlayer_TimingChanged(Media.AudioPlayer audioPlayer)
        {
            if (ViewState == MusicPageViewState.View)
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
        }

        private void AudioPlayer_PlayEnd(Media.AudioPlayer audioPlayer)
        {
            PlaySlider.Maximum = 0;
            PlaySlider.Value = 0;
        }

        private void AudioPlayer_CacheLoadingChanged(Media.AudioPlayer audioPlayer, object data)
        {
            isCodeChangedSliderValue = true;
            if (ViewState == MusicPageViewState.Hidden) return;
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
                // 加1ms，否则会短时间判定到上一句歌词
                App.audioPlayer.CurrentTime = lrcItem.LyricTimeSpan + TimeSpan.FromMilliseconds(1);
#if DEBUG
                Debug.WriteLine("audio player time setted.");
#endif
            }
        }

        private void LrcBaseListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LrcBaseListView.Padding = new Thickness(0, LrcBaseListView.ActualHeight / 2 + 68, 0, LrcBaseListView.ActualHeight / 2);
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
            UpdatingInterfaceDesign();
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
        {
            foreach (var data in App.playingList.NowPlayingMusicData.Artists)
            {
                var item = new MenuFlyoutItem()
                {
                    Text = data.Name,
                    Tag = data
                };
                item.Click += (sender, e) =>
                {
                    MainWindow.SetNavViewContent(
                    typeof(ItemListViewArtist),
                    (sender as MenuFlyoutItem).Tag,
                    new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    MainWindow.OpenOrCloseMusicPage();
                };
                ArtistFlyout.Items.Add(item);
            }
        }

        private void ArtistFlyout_Opening(object sender, object e)
        {
            ArtistFlyout.Items.Clear();
            foreach (var data in App.playingList.NowPlayingMusicData.Artists)
            {
                var item = new MenuFlyoutItem()
                {
                    Text = data.Name,
                    Tag = data
                };
                item.Click += (sender, e) =>
                {
                    MainWindow.SetNavViewContent(
                        typeof(ItemListViewArtist),
                        (sender as MenuFlyoutItem).Tag,
                        new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
                    MainWindow.OpenOrCloseMusicPage();
                };
                ArtistFlyout.Items.Add(item);
            }
        }

        private async void ArtistFlyout_Opened(object sender, object e)
        {
            await Task.Delay(1);
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
            MainWindow.SetNavViewContent(
                typeof(ItemListViewSearch),
                new List<object> {
                    MusicData.Title,
                    MusicData.From,
                    SearchDataType.歌曲
                },
                new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromRight });
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
        }

        private async void AudioInfoButton_Click(object sender, RoutedEventArgs e)
        {
            await MainWindow.ShowDialog("播放信息", new DialogPages.AudioInfoPage());
        }
    }
}
