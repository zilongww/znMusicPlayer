using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Composition;
using znMusicPlayerWUI.Media;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.DataEditor;

namespace znMusicPlayerWUI.Controls
{
    public sealed partial class MusicDataItem : UserControl
    {
        static bool isStaticInited = false;
        static List<MusicDataItem> staticMusicDataItem = [];
        ArrayList arrayList;
        static void initListen()
        {
            if (isStaticInited) return;
            isStaticInited = true;
            App.audioPlayer.SourceChanged += (_) =>
            {
                foreach (MusicDataItem item in staticMusicDataItem)
                {
                    item.InitPlayingState();
                }
            };
        }

        public static void TryHighlightPlayingItem()
        {
            foreach (MusicDataItem item in staticMusicDataItem)
            {
                if (!item.IsMusicDataPlaying) continue;
                item.SetHighlight();
            }
        }

        public static void TryHighlight(SongItemBindBase songItemBind)
        {
            foreach (MusicDataItem item in staticMusicDataItem)
            {
                if (item.songItemBind == songItemBind)
                {
                    item.SetHighlight();
                    break;
                }
            }
        }

        public static void TryHighlight(MusicData musicData)
        {
            foreach (MusicDataItem item in staticMusicDataItem)
            {
                if (item.songItemBind.MusicData == musicData)
                {
                    item.SetHighlight();
                }
            }
        }

        public bool IsMusicDataPlaying
        {
            get => songItemBind?.MusicData == App.audioPlayer.MusicData;
        }

        bool isUnloaded = false;
        SongItemBindBase songItemBind;
        public MusicDataItem()
        {
            initListen();
            InitializeComponent();
            InitVisuals();
            //arrayList = new ArrayList(10000000);
        }

        void InitInfo()
        {
            if (isUnloaded) return;
            if (songItemBind == null) return;
            Info_Texts_CountRun.Text = songItemBind.MusicData.Count == 0 ? null : $"{songItemBind.MusicData.Count}.";
            Info_Texts_TitleRun.Text = songItemBind.MusicData.Title;
            Info_Texts_Title2Run.Text = songItemBind.MusicData.Title2;
            Info_Texts_ButtonNameTextBlock.Text = songItemBind.MusicData.ButtonName;
        }

        int initImageCallCount = 0;
        async void InitImage()
        {
            if (Info_Image == null) return;
            Info_Image.Source = null;
            Info_Image_Root.Visibility = Visibility.Visible;
            SetImageBorder(false);
            if (isUnloaded) return;
            if (songItemBind == null) return;
            initImageCallCount++;
            await Task.Delay(150);
            initImageCallCount--;
            if (initImageCallCount != 0) return;
            if (isUnloaded) return;
            if (songItemBind == null) return;
            if (songItemBind.MusicListData?.ListDataType == DataType.ר��) return;
            if (songItemBind.MusicData.From == MusicFrom.localMusic)
            {
                if (Path.GetExtension(songItemBind.MusicData.InLocal) == ".mid")
                {
                    Info_Image.Source = null;
                    Info_Image_Root.Visibility = Visibility.Collapsed;
                    SetImageBorder(false);
                    return;
                }
            }

            MusicData musicData = songItemBind.MusicData;
            ImageSource result = null;
            var bitmapTuple = await ImageManage.GetImageSource(musicData, (int)(56 * songItemBind.ImageScaleDPI), (int)(56 * songItemBind.ImageScaleDPI), true);
            result = bitmapTuple.Item1;

            if (isUnloaded) result = null;
            if (musicData == songItemBind.MusicData)
            {
                if (result != null)
                {
                    Info_Image.Source = result;
                    SetImageBorder(true);
                }
                else
                {
                    Info_Image.Source = null;
                    Info_Image_Root.Visibility = Visibility.Collapsed;
                    SetImageBorder(false);
                }
            }
        }

        Visual backgroundFillVisual;
        Visual rightButtonVisual;
        Visual strokeVisual;
        ScalarKeyFrameAnimation rightButtonVisualShowAnimation;
        ScalarKeyFrameAnimation rightButtonVisualHideAnimation;
        ScalarKeyFrameAnimation backgroundFillVisualShowAnimation;
        ScalarKeyFrameAnimation backgroundFillVisualHideAnimation;
        ScalarKeyFrameAnimation strokeVisualShowAnimation;
        void InitVisuals()
        {
            backgroundFillVisual = ElementCompositionPreview.GetElementVisual(Background_FillRectangle);
            rightButtonVisual = ElementCompositionPreview.GetElementVisual(Info_Buttons_Root);
            strokeVisual = ElementCompositionPreview.GetElementVisual(Background_HighlightRectangle);

            backgroundFillVisual.Opacity = 0;
            rightButtonVisual.Opacity = 0;
            strokeVisual.Opacity = 0;

            AnimateHelper.AnimateScalar(rightButtonVisual, 1, 0.1,
                0, 0, 0, 0,
                out rightButtonVisualShowAnimation);
            AnimateHelper.AnimateScalar(rightButtonVisual, 0, 0.1,
                0, 0, 0, 0,
                out rightButtonVisualHideAnimation);
            AnimateHelper.AnimateScalar(backgroundFillVisual,
                                        1, 0.1,
                                        0, 0, 0, 0,
                                        out backgroundFillVisualShowAnimation);
            AnimateHelper.AnimateScalar(backgroundFillVisual,
                0, 0.1,
                0, 0, 0, 0,
                out backgroundFillVisualHideAnimation);
            AnimateHelper.AnimateScalar(strokeVisual, 0, 3, 0, 0, 0, 0,
                out strokeVisualShowAnimation);
        }

        void InitPlayingState()
        {
            if (isUnloaded) return;
            if (songItemBind == null) return;

            if (IsMusicDataPlaying)
            {
                App.audioPlayer.PlayStateChanged -= AudioPlayer_PlayStateChanged;
                App.audioPlayer.PlayStateChanged += AudioPlayer_PlayStateChanged;
                SetPlayingIcon(App.audioPlayer.PlaybackState);
                UserControl_PointerEntered(null, null);
                Background_PlayingRectangle.Opacity = 1;
            }
            else
            {
                App.audioPlayer.PlayStateChanged -= AudioPlayer_PlayStateChanged;
                SetPlayingIcon(NAudio.Wave.PlaybackState.Paused);
                UserControl_PointerExited(null, null);
                Background_PlayingRectangle.Opacity = 0;
            }
        }

        void SetHighlight()
        {
            strokeVisual.Opacity = 1;
            strokeVisual.StartAnimation("Opacity", strokeVisualShowAnimation);
        }

        void SetImageBorder(bool isShow)
        {
            if (isShow)
            {
                Info_Image_Root.Opacity = 1;
            }
            else
                Info_Image_Root.Opacity = 0;
        }

        void SetPlayingIcon(NAudio.Wave.PlaybackState playbackState)
        {
            if (playbackState == NAudio.Wave.PlaybackState.Playing)
            {
                Info_Buttons_MediaStateIcon.Glyph = "\xE769";
            }
            else
            {
                Info_Buttons_MediaStateIcon.Glyph = "\xE768";
            }
        }

        private void MusicDataItem_Completed(object sender, CompositionBatchCompletedEventArgs args)
        {
            if (!isPointEnter) Info_Buttons_Root.Visibility = Visibility.Collapsed;
            rightButtonVisual.Compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed -= MusicDataItem_Completed;
        }

        private void AudioPlayer_PlayStateChanged(AudioPlayer audioPlayer)
        {
            SetPlayingIcon(audioPlayer.PlaybackState);
        }

        private void UserControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender == null) return;
            if (sender.DataContext == null) return;
            if (sender.DataContext is not SongItemBindBase) return;
            songItemBind = sender.DataContext as SongItemBindBase;
            musicDataFlyout.SongItemBind = songItemBind;
            InitInfo();
            InitPlayingState();
            InitImage();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            staticMusicDataItem.Add(this);
            if (songItemBind == null) return;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (rightButtonVisual != null) 
            {
                rightButtonVisual.Compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed -= MusicDataItem_Completed;
            }
            App.audioPlayer.PlayStateChanged -= AudioPlayer_PlayStateChanged;
            staticMusicDataItem.Remove(this);
            isUnloaded = true;
            songItemBind = null;
            Info_Image.Source = null;
            rightButtonVisualShowAnimation.Dispose();
            rightButtonVisualHideAnimation.Dispose();
            backgroundFillVisualShowAnimation.Dispose();
            backgroundFillVisualHideAnimation.Dispose();
            strokeVisualShowAnimation.Dispose();
        }

        bool isPointEnter = false;
        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (songItemBind == null) return;
            isPointEnter = true;
            Info_Buttons_Root.Visibility = Visibility.Visible;
            backgroundFillVisual.StartAnimation("Opacity", backgroundFillVisualShowAnimation);
            rightButtonVisual.StartAnimation("Opacity", rightButtonVisualShowAnimation);
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (IsMusicDataPlaying) return;
            if (!isPointEnter) return;
            if (songItemBind == null)
            {
                rightButtonVisual.Compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed -= MusicDataItem_Completed;
                return;
            }
            isPointEnter = false;
            backgroundFillVisual.StartAnimation("Opacity", backgroundFillVisualHideAnimation);
            rightButtonVisual.StartAnimation("Opacity", rightButtonVisualHideAnimation);
            rightButtonVisual.Compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed -= MusicDataItem_Completed;
            rightButtonVisual.Compositor.GetCommitBatch(CompositionBatchTypes.Animation).Completed += MusicDataItem_Completed;
        }


        private async void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            await App.playingList.Play(songItemBind.MusicData, true);
        }

        private void UserControl_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
          musicDataFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private void UserControl_Holding(object sender, HoldingRoutedEventArgs e)
        {
            musicDataFlyout.ShowAt(sender as UIElement, e.GetPosition(sender as UIElement));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            musicDataFlyout.ShowAt(sender as FrameworkElement);
        }

        private void Info_Texts_ButtonNameTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Info_Texts_ButtonNameButton.Width = Info_Texts_ButtonNameTextBlock.ActualWidth;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsMusicDataPlaying)
            {
                if (App.audioPlayer.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                    App.audioPlayer.SetPause();
                else
                    App.audioPlayer.SetPlay();
            }
            else
                await App.playingList.Play(songItemBind.MusicData, true);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Pages.ListViewPages.ListViewPage.SetPageToListViewPage(songItemBind.MusicData.Album);
        }

        private void Info_Texts_FlyoutMenu_Artist_Item_Loaded(object sender, RoutedEventArgs e)
        {
            Info_Texts_FlyoutMenu_Album_Item.Text = $"ר����{songItemBind.MusicData.Album.Title}";
            Info_Texts_FlyoutMenu_Artist_Item.Items.Clear();
            foreach (var artist in songItemBind.MusicData.Artists)
            {
                var mfi = new MenuFlyoutItem()
                {
                    Text = artist.Name,
                    Tag = artist
                };
                mfi.Click += (_, __) =>
                {
                    Pages.ListViewPages.ListViewPage.SetPageToListViewPage((_ as FrameworkElement).Tag as Artist);
                };
                Info_Texts_FlyoutMenu_Artist_Item.Items.Add(mfi);
            }

        }

        private void Info_Texts_FlyoutMenu_Artist_Item_Unloaded(object sender, RoutedEventArgs e)
        {
            Info_Texts_FlyoutMenu_Artist_Item.Items.Clear();
        }
    }
}
