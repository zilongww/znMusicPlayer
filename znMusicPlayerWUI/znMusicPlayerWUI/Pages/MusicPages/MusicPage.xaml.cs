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

namespace znMusicPlayerWUI.Pages.MusicPages
{
    public partial class MusicPage : Page, IMusicPage
    {
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
            {
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
                UpdataInterfaceDesign();
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

        private void PlayingList_NowPlayingImageLoading(ImageSource imageSource)
        {
            if (App.audioPlayer.MusicData?.AlbumID != MusicData?.AlbumID)
            {
                if (BackgroundBaseImage.Source != null)
                    BackgroundBaseImageAnimate.Source = null;
                BackgroundBaseImageAnimate.Source = BackgroundBaseImage.Source;
                BackgroundBaseImage.Source = null;
                //AlbumImageBase.Dispose();
            }
        }

        private void PlayingList_NowPlayingImageLoaded(ImageSource imageSource)
        {
            if (ViewState == MusicPageViewState.View)
            {
                if (BackgroundBaseImage.Source != imageSource)
                {
                    ImageSources = imageSource;
                    if (App.audioPlayer?.MusicData?.AlbumID != MusicData?.AlbumID) return;
                    BackgroundBaseImage.Source = ImageSources;
                    AlbumImageBase.Source = imageSource;

                    AnimateHelper.AnimateScalar(
                        BackgroundBaseImage, 1, 1,
                        0.2f, 1f, 0.22f, 1f,
                        out var visual, out var compositor, out var animation);
                    visual.Opacity = 0;
                    visual.StartAnimation(nameof(visual.Opacity), animation);

                    System.Diagnostics.Debug.WriteLine($"MusicPage: 图片已被更改.");
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
                PlayingList_NowPlayingImageLoaded(App.playingList.NowPlayingImage);

                if (ShowLrcPage)
                {
                    //LrcBaseListView.SelectedItem = LrcBaseListView.Items.Last();
                    SelectedChangedDo();
                }
            }

            Debug.WriteLine($"MusicPage: ViewState 已被设置为 {musicPageViewState}.");
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

            var imageVisual1 = ElementCompositionPreview.GetElementVisual(BackgroundBaseImageAnimate);
            var compositor1 = imageVisual1.Compositor;
            var blurFactory1 = compositor1.CreateEffectFactory(blur);
            var blurBrush1 = blurFactory1.CreateBrush();
            blurBrush1.SetSourceParameter("Source", compositor1.CreateBackdropBrush());
            var blurVisual1 = compositor1.CreateSpriteVisual();
            blurVisual1.Brush = blurBrush1;
            blurVisual1.RelativeSizeAdjustment = new System.Numerics.Vector2(1f, 1f);
            ElementCompositionPreview.SetElementChildVisual(BackgroundBaseImageAnimate, blurVisual1);
        }

        public void UpdataInterfaceDesign()
        {
            if (!ShowLrcPage)
            {
                (InfoBaseGrid.Children[1] as Grid).Width = double.NaN;
                (InfoBaseGrid.Children[1] as Grid).HorizontalAlignment = HorizontalAlignment.Center;
                if (InfoBaseGrid.ActualHeight >= 850)
                {
                    Grid.SetRow((FrameworkElement)InfoBaseGrid.Children[0], 1);
                    Grid.SetRow((FrameworkElement)InfoBaseGrid.Children[1], 0);

                    InfoBaseGrid.RowDefinitions[1].Height = GridLength.Auto;
                    InfoBaseGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);

                    (InfoBaseGrid.Children[0] as StackPanel).Margin = new Thickness(0, 0, 0, 12);
                    (InfoBaseGrid.Children[1] as Grid).Margin = new Thickness(0, 0, 0, 48);

                    InfoBaseTitle.TextAlignment = TextAlignment.Left;
                }
                else
                {
                    Grid.SetRow((FrameworkElement)InfoBaseGrid.Children[0], 0);
                    Grid.SetRow((FrameworkElement)InfoBaseGrid.Children[1], 1);

                    InfoBaseGrid.RowDefinitions[0].Height = GridLength.Auto;
                    InfoBaseGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);

                    (InfoBaseGrid.Children[0] as StackPanel).Margin = new Thickness(0);
                    (InfoBaseGrid.Children[1] as Grid).Margin = new Thickness(0, 32, 0, 32);

                    InfoBaseTitle.TextAlignment = TextAlignment.Center;
                }
            }
            else
            {
                InfoBaseGrid.RowDefinitions[0].Height = GridLength.Auto;
                InfoBaseGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);

                (InfoBaseGrid.Children[1] as Grid).Width = double.NaN;
                (InfoBaseGrid.Children[1] as Grid).HorizontalAlignment = HorizontalAlignment.Center;
                Grid.SetColumn(LrcBaseGrid, 1);
                Grid.SetRow((FrameworkElement)InfoBaseGrid.Children[0], 0);
                Grid.SetRow((FrameworkElement)InfoBaseGrid.Children[1], 1);

                if (ActualWidth >= 800)
                {
                    BridgeTb.TextAlignment = TextAlignment.Left;
                    BridgeTb.HorizontalAlignment = HorizontalAlignment.Stretch;
                    LrcPageColumn.Width = new GridLength(1.35, GridUnitType.Star);
                    LrcPageColumn.MaxWidth = double.MaxValue;

                    InfoBaseGrid.RowDefinitions[0].Height = GridLength.Auto;
                    InfoBaseGrid.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);

                    (InfoBaseGrid.Children[0] as StackPanel).Margin = new Thickness(0);
                    (InfoBaseGrid.Children[1] as Grid).Margin = new Thickness(0, 32, 0, 32);
                    LrcBaseGrid.Margin = new Thickness(0);

                    InfoBaseTitle.TextAlignment = TextAlignment.Center;
                }
                else
                {
                    BridgeTb.TextAlignment = TextAlignment.Center;
                    BridgeTb.HorizontalAlignment = HorizontalAlignment.Center;
                    Grid.SetColumn(LrcBaseGrid, 0);
                    LrcPageColumn.MaxWidth = 0;
                    LrcBaseGrid.Margin = new Thickness(12, 112, 12,
                        (InfoBaseGrid.Children[2] as Grid).ActualHeight + (InfoBaseGrid.Children[3] as Grid).ActualHeight + 24
                        );

                    Grid.SetRow((FrameworkElement)InfoBaseGrid.Children[1], 0);
                    (InfoBaseGrid.Children[1] as Grid).Margin = new Thickness(0, 0, 0, 0);
                    (InfoBaseGrid.Children[1] as Grid).Width = 100;
                    (InfoBaseGrid.Children[1] as Grid).HorizontalAlignment = HorizontalAlignment.Left;
                    (InfoBaseGrid.Children[0] as StackPanel).Margin = new Thickness(112, 20, 0, 0);

                    InfoBaseTitle.TextAlignment = TextAlignment.Left;
                }
            }
        }

        public void SelectedChangedDo()
        {
            isCodeChangedLrcItem = true;
            LrcBaseListView.SelectedItem = App.lyricManager.NowLyricsData;
            isCodeChangedLrcItem = false;
            if (scrollViewer != null)
            {
                var b = LrcBaseListView.ContainerFromIndex(LrcBaseListView.SelectedIndex) as UIElement;
                if (b == null)
                {
                    LrcBaseListView.ScrollIntoView(App.lyricManager.NowLyricsData);
                    b = LrcBaseListView.ContainerFromIndex(LrcBaseListView.SelectedIndex) as UIElement;
                    LrcBaseListView.ScrollIntoView(App.lyricManager.NowLyricsData);
                }
                if (b != null)
                    scrollViewer.ChangeView(null, b.ActualOffset.Y + b.ActualSize.Y / 2, null);
            }
            Debug.WriteLine($"MusicPage: 选中歌词已被更改为: {App.lyricManager.NowLyricsData.Lyric}.");
        }

        //todo：优化性能
        private void LyricManager_PlayingLyricSelectedChange1(DataEditor.LyricData nowLyricsData)
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
            UpdataInterfaceDesign();

            CloseMusicPageButton.Width = MainWindow.SNavView.DisplayMode == NavigationViewDisplayMode.Minimal ? 86 : 44;
        }

        DataEditor.MusicData MusicData;
        private void AudioPlayer_SourceChanged(Media.AudioPlayer audioPlayer)
        {
            if (ViewState == MusicPageViewState.Hidden || audioPlayer.MusicData == null) return;
            InfoBaseTitle.Text = audioPlayer.MusicData.Title;
            InfoBaseArtist.Text = audioPlayer.MusicData.ButtonName;

            if (audioPlayer.MusicData?.AlbumID != MusicData?.AlbumID)
            {
                MusicData = audioPlayer.MusicData;
            }
        }

        private void AudioPlayer_PlayStateChanged(Media.AudioPlayer audioPlayer)
        {
            if (ViewState == MusicPageViewState.Hidden) return;
            MediaPlayStateViewer1.PlaybackState = audioPlayer.NowOutObj != null ? audioPlayer.NowOutObj.PlaybackState : PlaybackState.Paused;
        }

        private void AudioPlayer_TimingChanged(Media.AudioPlayer audioPlayer)
        {
            if (ViewState == MusicPageViewState.View)
            {
                if (audioPlayer.FileReader != null)
                {
                    PlaySlider.Minimum = 0;
                    PlaySlider.Maximum = audioPlayer.FileReader.TotalTime.Ticks;
                    isCodeChangedSliderValue = true;
                    PlaySlider.Value = audioPlayer.CurrentTime.Ticks;
                    isCodeChangedSliderValue = false;
                    NowPlayTimeTb.Text =
                        $"{audioPlayer.CurrentTime.ToString(@"mm\:ss")}/{audioPlayer.FileReader.TotalTime.ToString(@"mm\:ss")}";
                    NowAtherTimeTb.Text =
                        (audioPlayer.FileReader.TotalTime - audioPlayer.CurrentTime).ToString(@"mm\:ss");
                }
            }
        }

        private void AudioPlayer_PlayEnd(Media.AudioPlayer audioPlayer)
        {

        }

        private void AudioPlayer_CacheLoadingChanged(Media.AudioPlayer audioPlayer, object data)
        {
            if (ViewState == MusicPageViewState.Hidden) return;
            PlayButton.IsEnabled = false;
            AudioLoadingProressRing.IsIndeterminate = true;
        }

        private void AudioPlayer_CacheLoadedChanged(Media.AudioPlayer audioPlayer)
        {
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
            if (App.audioPlayer.NowOutObj?.PlaybackState == PlaybackState.Playing)
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
            MainWindow.OpenOrClosePlayingList();
        }

        private void VloumeButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenOrCloseVolume();
        }

        static ScrollViewer scrollViewer = null;
        private void LrcBaseListView_Loaded(object sender, RoutedEventArgs e)
        {
            var a = VisualTreeHelper.GetChild(LrcBaseListView, 0) as Border;
            if (a != null)
                scrollViewer = a.Child as ScrollViewer;
            UpdataInterfaceDesign();
        }

        bool isCodeChangedLrcItem = false;
        private void LrcBaseListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lrcItem = LrcBaseListView.SelectedItem as DataEditor.LyricData;
            if (lrcItem != null && !isCodeChangedLrcItem)
            {
                // 加1ms，否则会短时间判定到上一句歌词
                App.audioPlayer.CurrentTime = lrcItem.LyricTimeSpan + TimeSpan.FromMilliseconds(1);
                Debug.WriteLine("audio player time setted.");
            }
        }

        private void LrcBaseListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LrcBaseListView.Padding = new Thickness(0, LrcBaseListView.ActualHeight / 2, 0, LrcBaseListView.ActualHeight / 2);
        }

        private void pageRoot_Loaded(object sender, RoutedEventArgs e)
        {
            //ShowLrcPage = ShowLrcPage;
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
        private void PlaySlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (!isCodeChangedSliderValue)
            {
                App.audioPlayer.CurrentTime = TimeSpan.FromTicks((long)PlaySlider.Value);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OpenOrCloseMusicPage();
        }
    }
}
