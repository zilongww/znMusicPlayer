using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Composition;
using znMusicPlayerWUI.Media;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.DataEditor;
using Newtonsoft.Json.Linq;
using znMusicPlayerWUI.Controls;
using CommunityToolkit.WinUI.UI;
using Windows.Storage.Pickers;
using Windows.ApplicationModel.Search;

namespace znMusicPlayerWUI.Pages.ListViewPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayListPage : Page
    {
        MusicListData musicListData { get; set; } = null;
        ObservableCollection<SongItemBindBase> musicListBind { get; set; } = new();
        ScrollViewer scrollViewer;
        PageData pageData;
        public string md5;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            pageData = e.Parameter as PageData;
            md5 = pageData.Param as string;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            //(e.Parameter as PageData).VerticalOffset = scrollViewer.VerticalOffset;
            musicListData = null;
            MainWindow.WindowDpiChanged -= MainWindow_WindowDpiChanged;
        }

        public PlayListPage()
        {
            InitializeComponent();
        }

        // Items ����ʱ CommandBar ��Ȳ������ >:(
        async void UpdateCommandBarWidth()
        {
            ItemsList_Header_Info_CommandBar.Width = 0;
            await Task.Delay(50);
            ItemsList_Header_Info_CommandBar.Width = double.NaN;
        }
        void MultiSelectDo(bool isChecked)
        {
            if (isUnloaded) return;
            if (musicListBind == null) return;

            foreach (FrameworkElement element in ItemsList_Header_Info_CommandBar.PrimaryCommands)
            {
                if ((string)element.Tag == "multiSelect") continue;
                if (((string)element.Tag).Contains("multi"))
                    element.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
                else
                {
                    element.Visibility = isChecked ? Visibility.Collapsed : Visibility.Visible;
                }
            }
            UpdateCommandBarWidth();

            ItemsList.SelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.None;
        }

        void SelectedReverseDo()
        {
            foreach (SongItemBindBase item in ItemsList.Items.Cast<SongItemBindBase>())
            {
                if (ItemsList.SelectedItems.Contains(item))
                {
                    ItemsList.SelectedItems.Remove(item);
                }
                else
                {
                    ItemsList.SelectedItems.Add(item);
                }
            }
        }
        async void DeleteSelectedItemDo()
        {
            if (ItemsList.SelectedItems.Any())
            {
                var result = await MainWindow.ShowDialog("ɾ������", $"���Ҫ�Ӹ赥��ɾ����{ItemsList.SelectedItems.Count}�׸�����", "ȡ��", "ȷ��", defaultButton: ContentDialogButton.Close);
                if (result == ContentDialogResult.Primary)
                {
                    ItemsList_Header_Info_CommandBar.IsEnabled = false;
                    var item = MainWindow.AddNotify("ɾ������", "����׼��ɾ������...", NotifySeverity.Loading, TimeSpan.MaxValue);
                    var jdata = await PlayListHelper.ReadData();
                    int num = 0;
                    string listName = musicListData.ListName;
                    foreach (SongItemBindBase data in ItemsList.SelectedItems.Cast<SongItemBindBase>())
                    {
                        num++;
                        item.HorizontalAlignment = HorizontalAlignment.Stretch;
                        item.SetNotifyItemData("ɾ������", $"���ȣ�{Math.Round(((decimal)num / ItemsList.SelectedItems.Count) * 100, 1)}%\n����ɾ����{data.MusicData.Title} - {data.MusicData.ButtonName}", NotifySeverity.Loading);
                        item.SetProcess(ItemsList.SelectedItems.Count, num);
                        musicListData.Songs.Remove(data.MusicData);
                    }
                    jdata[musicListData.ListName] = JObject.FromObject(musicListData);

                    item.HorizontalAlignment = HorizontalAlignment.Center;
                    item.SetNotifyItemData("ɾ������", "���ڱ���...", NotifySeverity.Loading);
                    item.SetProcess(0, 0);
                    await PlayListHelper.SaveData(jdata);
                    await App.playListReader.Refresh();
                    item.SetNotifyItemData("ɾ������", "ɾ�������ɹ���", NotifySeverity.Complete);
                    MainWindow.NotifyCountDown(item);
                    ItemsList_Header_Info_CommandBar.IsEnabled = true;
                    InitInfo();
                    InitBindings();
                }
            }
        }
        void DownloadSelectedItemDo()
        {
            if (ItemsList.SelectedItems.Any())
            {
                foreach (SongItemBindBase songItem in ItemsList.SelectedItems)
                {
                    App.downloadManager.Add(songItem.MusicData);
                }
            }
        }
        void AddSelectedItemToPlayingDo()
        {
            if (ItemsList.SelectedItems.Any())
            {
                foreach (SongItemBindBase item in ItemsList.SelectedItems.Cast<SongItemBindBase>())
                {
                    App.playingList.Add(item.MusicData);
                }
            }
        }

        async void AddLocalFilesDo()
        {
            StackPanel stackPanel = new() { HorizontalAlignment = HorizontalAlignment.Stretch, Spacing = 4, Orientation = Orientation.Vertical };

            StackPanel stackPanelContent1 = new StackPanel() { Orientation = Orientation.Vertical };
            Grid fontIconBaseGrid = new Grid() { Margin = new(12), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid fontIconGrid = new() { Margin = new(-16, -12, 0, 0) };
            fontIconGrid.Children.Add(new FontIcon() { Glyph = "\uE729", FontSize = 30, Foreground = App.Current.Resources["ControlSolidFillColorDefaultBrush"] as SolidColorBrush });
            fontIconGrid.Children.Add(new FontIcon() { Glyph = "\uE7C3", FontSize = 30 });
            Grid fontIconGrid1 = new() { Margin = new(-8, -6, 0, 0) };
            fontIconGrid1.Children.Add(new FontIcon() { Glyph = "\uE729", FontSize = 30, Foreground = App.Current.Resources["ControlSolidFillColorDefaultBrush"] as SolidColorBrush });
            fontIconGrid1.Children.Add(new FontIcon() { Glyph = "\uE7C3", FontSize = 30 });
            Grid fontIconGrid2 = new();
            fontIconGrid2.Children.Add(new FontIcon() { Glyph = "\uE729", FontSize = 30, Foreground = App.Current.Resources["ControlSolidFillColorDefaultBrush"] as SolidColorBrush });
            fontIconGrid2.Children.Add(new FontIcon() { Glyph = "\uE7C3", FontSize = 30 });
            fontIconGrid2.Children.Add(new FontIcon() { Glyph = "\uEC4F", FontSize = 13, Margin = new(0, 10, 8, 0) });
            fontIconBaseGrid.Children.Add(fontIconGrid);
            fontIconBaseGrid.Children.Add(fontIconGrid1);
            fontIconBaseGrid.Children.Add(fontIconGrid2);
            stackPanelContent1.Children.Add(fontIconBaseGrid);
            stackPanelContent1.Children.Add(new TextBlock() { Text = "��� ����/��� �ļ�", TextTrimming = TextTrimming.CharacterEllipsis });

            StackPanel stackPanelContent2 = new StackPanel() { Orientation = Orientation.Vertical };
            Grid fontIconFolderGrid = new() { Margin = new(12) };
            fontIconFolderGrid.Children.Add(new FontIcon() { Glyph = "\uE8D5", FontSize = 30, Foreground = App.Current.Resources["ControlSolidFillColorDefaultBrush"] as SolidColorBrush });
            fontIconFolderGrid.Children.Add(new FontIcon() { Glyph = "\uE8B7", FontSize = 30 });
            fontIconFolderGrid.Children.Add(new FontIcon() { Glyph = "\uEC4F", FontSize = 13, Margin = new(0, 4, -0, 0) });
            stackPanelContent2.Children.Add(fontIconFolderGrid);
            stackPanelContent2.Children.Add(new TextBlock() { Text = "ɨ���ļ��е������ļ�", TextTrimming = TextTrimming.CharacterEllipsis });

            var ab = new Button() { Content = stackPanelContent1, HorizontalAlignment = HorizontalAlignment.Stretch, MinWidth = 164 };
            var bb = new Button() { Content = stackPanelContent2, HorizontalAlignment = HorizontalAlignment.Stretch };

            ab.Click += Ab_Click;
            bb.Click += Bb_Click;

            stackPanel.Children.Add(ab);
            stackPanel.Children.Add(bb);

            await MainWindow.ShowDialog("��ӱ����ļ�", stackPanel);

            ab.Click -= Ab_Click;
            bb.Click -= Bb_Click;
        }
        async void Ab_Click(object sender, RoutedEventArgs e)
        {
            var files = await FileHelper.UserSelectFiles(
                PickerViewMode.List, PickerLocationId.MusicLibrary);
            //App.SupportedMediaFormats);
            if (files.Any())
            {
                MainWindow.HideDialog();
                ItemsList_Header_Info_CommandBar.IsEnabled = false;
                var item = MainWindow.AddNotify("��ӱ��ظ���", "����׼����ӱ��ظ���...", NotifySeverity.Loading, TimeSpan.MaxValue);
                var jdata = await PlayListHelper.ReadData();
                int count = 0;
                string listName = musicListData.ListName;
                foreach (var i in files)
                {
                    item.HorizontalAlignment = HorizontalAlignment.Stretch;
                    item.SetNotifyItemData("��ӱ��ظ���", $"���ȣ�{count}/{files.Count}��{Math.Round(((decimal)count / files.Count) * 100, 1)}%\n������ӣ�{i.Name}", NotifySeverity.Loading);
                    item.SetProcess(files.Count, count);
                    FileInfo fi = null;
                    await Task.Run(() => fi = new FileInfo(i.Path));
                    jdata = await PlayListHelper.AddLocalMusicDataToPlayList(listName, fi, jdata);
                    count++;
                }
                item.SetProcess(0, 0);
                item.HorizontalAlignment = HorizontalAlignment.Center;
                item.SetNotifyItemData("��ӱ��ظ���", "���ڱ���...", NotifySeverity.Loading);
                await PlayListHelper.SaveData(jdata);
                await App.playListReader.Refresh();
                InitInfo();
                InitBindings();
                ItemsList_Header_Info_CommandBar.IsEnabled = true;
                item.SetNotifyItemData("��ӱ��ظ���", "��ӱ��ظ����ɹ���", NotifySeverity.Complete);
                MainWindow.NotifyCountDown(item);
            }
        }
        async void Bb_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.StorageFolder folder = await FileHelper.UserSelectFolder(PickerLocationId.MusicLibrary);
            if (folder != null)
            {
                var jdata = await PlayListHelper.ReadData();
                DirectoryInfo directory = null;
                await Task.Run(() => directory = Directory.CreateDirectory(folder.Path));
                foreach (var i in directory.GetFiles())
                {
                    if (App.SupportedMediaFormats.Contains(i.Extension))
                    {
                        jdata = await PlayListHelper.AddLocalMusicDataToPlayList(musicListData.ListName, i, jdata);
                    }
                }
                await PlayListHelper.SaveData(jdata);
                await App.playListReader.Refresh();
                InitInfo();
                InitBindings();
                MainWindow.AddNotify("��ӱ��ظ����ɹ���", null, NotifySeverity.Complete);
            }
        }

        CompositionPropertySet scrollerPropertySet;
        Compositor compositor;
        Visual scrollVisual;
        Visual headerVisual;
        Visual backgroundVisual;
        Visual imageVisual;
        Visual infoVisual;
        Visual commandBarVisual;
        Visual headerFootRootVisual;
        Visual searchRootVisual;
        ScalarKeyFrameAnimation commandBarVisualOpacityAnimation;
        void InitVisuals()
        {
            if (isUnloaded) return;
            MultiSelectDo(false);

            // ���� header Ϊ����
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)ItemsList.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex(headerContainer, 1);

            scrollViewer = (VisualTreeHelper.GetChild(ItemsList, 0) as Border).Child as ScrollViewer;
            scrollViewer.CanContentRenderOutsideBounds = true;
            scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

            compositor = scrollerPropertySet.Compositor;
            scrollVisual = ElementCompositionPreview.GetElementVisual(scrollViewer);
            headerVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Root);
            backgroundVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_ImageInfo_BackgroundFill);
            imageVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Image_Root);
            infoVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Info_Root);
            headerFootRootVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Foot_Root);
            searchRootVisual = ElementCompositionPreview.GetElementVisual(ItemList_Header_Search_Root);
            commandBarVisual = ElementCompositionPreview.GetElementVisual(ItemsList_Header_Info_CommandBar);

            commandBarVisual.Opacity = 0;
            AnimateHelper.AnimateScalar(commandBarVisual, 1, 0.3, 0, 0, 0, 0, out commandBarVisualOpacityAnimation);
        }

        ExpressionAnimation logoHeaderScaleAnimation;
        ExpressionAnimation offsetExpression;
        ExpressionAnimation backgroundVisualOpacityAnimation;
        ExpressionAnimation imageVisualOffsetAnimation;
        ExpressionAnimation infoVisualOffsetAnimation;
        ExpressionAnimation commandBarVisualOffsetAnimation;
        ExpressionAnimation headerFootRootVisualOffsetAnimation;
        ExpressionAnimation searchRootVisualOffsetAnimation;
        async void InitShyHeader(bool imageSizeOnly = false, bool delay = false)
        {
            if (scrollViewer == null) return;
            if (compositor == null) return;
            if (isUnloaded) return;
            var anotherHeight = 154;
            double imageSizeEnd = 0.45;
            string progress = $"Clamp(-scroller.Translation.Y / {anotherHeight}, 0, 1.0)";

            logoHeaderScaleAnimation?.Dispose();
            logoHeaderScaleAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector2(1, 1), Vector2({imageSizeEnd}, {imageSizeEnd}), {progress})");
            logoHeaderScaleAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            imageVisual.StartAnimation("Scale.xy", logoHeaderScaleAnimation);
            if (ItemsList_Header_Root.ActualWidth != 0)
                ItemsList_Header_Info_Root_SizeChanger.Width = ItemsList_Header_Root.ActualWidth - ItemsList_Header_Image_Root.ActualWidth * imageVisual.Scale.X - 32 - 16;
            if (imageSizeOnly) return;

            if (delay) await Task.Delay(10);

            offsetExpression?.Dispose();
            offsetExpression = compositor.CreateExpressionAnimation($"-scroller.Translation.Y - {progress} * {anotherHeight}");
            offsetExpression.SetReferenceParameter("scroller", scrollerPropertySet);
            headerVisual.StartAnimation("Offset.Y", offsetExpression);

            backgroundVisualOpacityAnimation?.Dispose();
            backgroundVisualOpacityAnimation = compositor.CreateExpressionAnimation($"Lerp(0, 1, {progress})");
            backgroundVisualOpacityAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            backgroundVisual.StartAnimation("Opacity", backgroundVisualOpacityAnimation);

            imageVisualOffsetAnimation?.Dispose();
            imageVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector2(0, 0), Vector2(0, {anotherHeight}), {progress})");
            imageVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            imageVisual.StartAnimation("Offset.xy", imageVisualOffsetAnimation);

            infoVisualOffsetAnimation?.Dispose();
            infoVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3({ItemsList_Header_Image_Root.ActualWidth + 16}, 0, 0), Vector3({(int)(ItemsList_Header_Image_Root.ActualWidth * imageSizeEnd) + 16}, {anotherHeight}, 0), {progress})");
            infoVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            infoVisual.StartAnimation(nameof(infoVisual.Offset), infoVisualOffsetAnimation);

            commandBarVisualOffsetAnimation?.Dispose();
            commandBarVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(-6, {imageVisual.Size.Y - commandBarVisual.Size.Y + 6}, 0), Vector3(-6, {imageVisual.Size.Y * imageSizeEnd - commandBarVisual.Size.Y + 6}, 0), {progress})");
            commandBarVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            commandBarVisual.StartAnimation(nameof(infoVisual.Offset), commandBarVisualOffsetAnimation);

            headerFootRootVisualOffsetAnimation?.Dispose();
            headerFootRootVisualOffsetAnimation = compositor.CreateExpressionAnimation(
                $"Lerp(" +
                    $"Vector3(" +
                        $"-16," +
                        $"{ActualHeight} - {headerFootRootVisual.Size.Y} - 8," +
                        $"0)," +
                    $"Vector3(" +
                        $"-16," +
                        $"{anotherHeight} + {ActualHeight} - {headerFootRootVisual.Size.Y} - 8," +
                        $"0)," +
                    $"{progress})");
            headerFootRootVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            headerFootRootVisual.StartAnimation("Offset", headerFootRootVisualOffsetAnimation);

            searchRootVisualOffsetAnimation?.Dispose();
            searchRootVisualOffsetAnimation = compositor.CreateExpressionAnimation($"Lerp(Vector3(0, {ItemsList_Header_Root.ActualHeight + 4}, 0), Vector3(0, {ItemsList_Header_Root.ActualHeight + 4}, 0), {progress})");
            searchRootVisualOffsetAnimation.SetReferenceParameter("scroller", scrollerPropertySet);
            searchRootVisual.StartAnimation(nameof(infoVisual.Offset), searchRootVisualOffsetAnimation);
        }
        void DisposeVisuals()
        {
            logoHeaderScaleAnimation?.Dispose();
            offsetExpression?.Dispose();
            backgroundVisualOpacityAnimation?.Dispose();
            imageVisualOffsetAnimation?.Dispose();
            infoVisualOffsetAnimation?.Dispose();
            commandBarVisualOffsetAnimation?.Dispose();
            headerFootRootVisualOffsetAnimation?.Dispose();
            commandBarVisualOpacityAnimation?.Dispose();

            scrollVisual = null;
            scrollerPropertySet = null;
            compositor = null;
            headerVisual = null;
            backgroundVisual = null;
            imageVisual = null;
            infoVisual = null;
            commandBarVisual = null;
            headerFootRootVisual = null;
            logoHeaderScaleAnimation = null;
            offsetExpression = null;
            backgroundVisualOpacityAnimation = null;
            imageVisualOffsetAnimation = null;
            infoVisualOffsetAnimation = null;
            commandBarVisualOffsetAnimation = null;
            headerFootRootVisualOffsetAnimation = null;
            commandBarVisualOpacityAnimation = null;
        }

        bool isInInitBindings = false;
        List<PlaySort> listSortEnum = null;
        async void InitBindings()
        {
            if (isUnloaded) return;
            if (isInInitBindings) return;
            isInInitBindings = true;
            var scs = musicListData.PlaySort;
            List<MusicData> array = [];
            await Task.Run(() =>
            {
                switch (scs)
                {
                    case PlaySort.Ĭ������:
                        array = musicListData.Songs;
                        break;
                    case PlaySort.Ĭ�Ͻ���:
                        MusicData[] list = new MusicData[musicListData.Songs.Count];
                        musicListData.Songs.CopyTo(list, 0);
                        var l = list.ToList();
                        l.Reverse();
                        array = l;
                        break;
                    case PlaySort.��������:
                        array = [.. musicListData.Songs.OrderBy(m => m.Title)];
                        break;
                    case PlaySort.���ƽ���:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.Title)];
                        break;
                    case PlaySort.����������:
                        array = [.. musicListData.Songs.OrderBy(m => m.Artists.Count != 0 ? m.Artists[0].Name : "δ֪")];
                        break;
                    case PlaySort.�����ҽ���:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.Artists.Count != 0 ? m.Artists[0].Name : "δ֪")];
                        break;
                    case PlaySort.ר������:
                        array = [.. musicListData.Songs.OrderBy(m => m.Album.Title)];
                        break;
                    case PlaySort.ר������:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.Album.Title)];
                        break;
                    case PlaySort.ʱ������:
                        array = [.. musicListData.Songs.OrderBy(m => m.RelaseTime)];
                        break;
                    case PlaySort.ʱ�併��:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.RelaseTime)];
                        break;
                    case PlaySort.��������:
                        array = [.. musicListData.Songs.OrderBy(m => m.Index)];
                        break;
                    case PlaySort.��������:
                        array = [.. musicListData.Songs.OrderByDescending(m => m.Index)];
                        break;
                }
            });

            musicListBind.Clear();
            if (isUnloaded || musicListData == null)
            {
                array = null;
                return;
            }
            int count = 1;
            var dpi = CodeHelper.GetScaleAdjustment(App.WindowLocal);
            foreach (var item in array)
            {
                item.Count = count;
                musicListBind.Add(new() { MusicData = item, MusicListData = musicListData, ImageScaleDPI = dpi });
                count++;
            }

            SortComboBox.SelectedIndex = (int)musicListData.PlaySort;
            isInInitBindings = false;
        }

        void InitInfo()
        {
            if (isUnloaded) return;
            foreach (var mld in App.playListReader.NowMusicListData)
            {
                if (mld.MD5 == md5)
                {
                    musicListData = mld;
                    break;
                }
            }

            if (musicListData == null) return;
            listSortEnum = Enum.GetValues(typeof(PlaySort)).Cast<PlaySort>().ToList();
            SortComboBox.ItemsSource = listSortEnum;
            SortComboBox.SelectedIndex = (int)musicListData.PlaySort;

            MainWindow.WindowDpiChanged -= MainWindow_WindowDpiChanged;
            MainWindow.WindowDpiChanged += MainWindow_WindowDpiChanged;
            ItemsList_Header_Info_TitleTextBlock.Text = musicListData.ListShowName;
            ItemsList_Header_Info_OtherTextBlock.Text = $"�� {musicListData.Songs.Count} �׸���";
        }

        static Thickness thickness0 = new(0);
        static Thickness thickness1 = new(1);
        async void InitImage()
        {
            if (isUnloaded) return;
            if (musicListData == null) return;
            ItemsList_Header_Image.BorderThickness = thickness0;
            ImageSource imageSource = null;
            if (musicListData.ListDataType == DataType.���ظ赥)
            {
                bool isExists = true;
                await Task.Run(() => { isExists = File.Exists(musicListData.PicturePath); });
                if (isExists) imageSource = await FileHelper.GetImageSource(musicListData.PicturePath);
            }
            else if (musicListData.ListDataType == DataType.�赥)
            {
                imageSource = (await ImageManage.GetImageSource(musicListData)).Item1;
            }
            if (imageSource == null)
            {
                imageSource = await FileHelper.GetImageSource("");
            }

            if (isUnloaded || musicListData == null) return;
            ItemsList_Header_Image.BorderThickness = thickness1;
            ItemsList_Header_Image.Source = imageSource;
            InitShyHeader();
            commandBarVisual.StartAnimation("Opacity", commandBarVisualOpacityAnimation);
        }

        void InitEvents()
        {
            MainWindow.InKeyDownEvent -= MainWindow_InKeyDownEvent;
            MainWindow.InKeyDownEvent += MainWindow_InKeyDownEvent;
            ItemList_Header_Search_Control.SearchingAItem -= ItemList_Header_Search_Control_SearchingAItem;
            ItemList_Header_Search_Control.SearchingAItem += ItemList_Header_Search_Control_SearchingAItem;
            ItemList_Header_Search_Control.IsOpenChanged -= ItemList_Header_Search_Control_IsOpenChanged;
            ItemList_Header_Search_Control.IsOpenChanged += ItemList_Header_Search_Control_IsOpenChanged;
            ItemsList_Header_Foot_Buttons.PositionToNowPlaying_Button.Click -= PositionToNowPlaying_Button_Click;
            ItemsList_Header_Foot_Buttons.PositionToNowPlaying_Button.Click += PositionToNowPlaying_Button_Click;
            ItemsList_Header_Foot_Buttons.PositionToTop_Button.Click -= PositionToNowPlaying_Button_Click;
            ItemsList_Header_Foot_Buttons.PositionToTop_Button.Click += PositionToNowPlaying_Button_Click;
            ItemsList_Header_Foot_Buttons.PositionToBottom_Button.Click -= PositionToNowPlaying_Button_Click;
            ItemsList_Header_Foot_Buttons.PositionToBottom_Button.Click += PositionToNowPlaying_Button_Click;
        }

        void RemoveEvents()
        {
            MainWindow.InKeyDownEvent -= MainWindow_InKeyDownEvent;
            ItemList_Header_Search_Control.SearchingAItem -= ItemList_Header_Search_Control_SearchingAItem;
            ItemList_Header_Search_Control.SearchingAItem -= ItemList_Header_Search_Control_SearchingAItem;
            ItemList_Header_Search_Control.IsOpenChanged -= ItemList_Header_Search_Control_IsOpenChanged;
            ItemsList_Header_Foot_Buttons.PositionToNowPlaying_Button.Click -= PositionToNowPlaying_Button_Click;
            ItemsList_Header_Foot_Buttons.PositionToTop_Button.Click -= PositionToNowPlaying_Button_Click;
            ItemsList_Header_Foot_Buttons.PositionToBottom_Button.Click -= PositionToNowPlaying_Button_Click;
        }

        void Init()
        {
            InitEvents();
            InitInfo();
            InitImage();
            InitVisuals();
            InitShyHeader();
            InitBindings();
        }

        public ArrayList arrayList { get; set; }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            ItemsList.ItemsSource = musicListBind;
            ItemList_Header_Search_Control.SongItemBinds = musicListBind;
            scrollViewer.ChangeView(null, pageData.VerticalOffset, null);
            //arrayList = new ArrayList(100000000);
        }

        bool isUnloaded = false;
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            isUnloaded = true;
            RemoveEvents();
            ItemList_Header_Search_Control.IsOpenChanged -= ItemList_Header_Search_Control_IsOpenChanged;
            MainWindow.WindowDpiChanged -= MainWindow_WindowDpiChanged;
            DisposeVisuals();
            if (ItemsList_Header_Image != null) ItemsList_Header_Image.Source = null;
            if (ItemsList != null) ItemsList.ItemsSource = null;
            if (SortComboBox != null) SortComboBox.ItemsSource = null;
            musicListBind?.Clear();
            musicListBind = null;
            listSortEnum?.Clear();
            listSortEnum = null;
            musicListData = null;
            if (scrollViewer != null)
                scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
            Bindings.StopTracking();
            UnloadObject(this);
        }

        bool isDelayInitShyHeaderWhenScroll = false;
        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (scrollViewer == null) return;
            //scrollViewer.ScrollToVerticalOffset(Math.Round(scrollViewer.VerticalOffset, 0));
            headerVisual.IsPixelSnappingEnabled = true;
            if (scrollViewer.VerticalOffset < 300)
            {
                isDelayInitShyHeaderWhenScroll = true;
                InitShyHeader(true);
                await Task.Delay(200);
                InitShyHeader(true);
            }
            else
            {
                if (isDelayInitShyHeaderWhenScroll)
                {
                    isDelayInitShyHeaderWhenScroll = false;
                    await Task.Delay(500);
                    InitShyHeader(true);
                    await Task.Delay(500);
                    InitShyHeader(true);
                }
            }
        }

        private void ItemsList_Header_Image_Root_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitShyHeader();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            InitShyHeader();
        }

        private void MainWindow_WindowDpiChanged()
        {
            InitShyHeader();
        }

        private async void PositionToNowPlaying_Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch ((ScrollFootButton.ButtonType)btn.Tag)
            {
                case ScrollFootButton.ButtonType.NowPlaying:
                    foreach (var i in musicListBind)
                    {
                        if (i.MusicData != App.audioPlayer.MusicData) continue;
                        await ItemsList.SmoothScrollIntoViewWithItemAsync(i, ScrollItemPlacement.Center);
                        await ItemsList.SmoothScrollIntoViewWithItemAsync(i, ScrollItemPlacement.Center, disableAnimation: true);
                        MusicDataItem.TryHighlightPlayingItem();
                    }
                    break;
                case ScrollFootButton.ButtonType.Top:
                    scrollViewer.ChangeView(null, 0, null);
                    break;
                case ScrollFootButton.ButtonType.Bottom:
                    scrollViewer.ChangeView(null, scrollViewer.ScrollableHeight, null);
                    break;
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            switch (btn.Tag)
            {
                case "playAll":
                    if (musicListBind.Count == 0) return;
                    if (App.playingList.PlayBehavior == znMusicPlayerWUI.Background.PlayBehavior.�������)
                    {
                        App.playingList.ClearAll();
                    }
                    foreach (var songItem in musicListBind)
                    {
                        App.playingList.Add(songItem.MusicData, false);
                    }
                    await App.playingList.Play(musicListBind.First().MusicData, true);
                    App.playingList.SetRandomPlay(App.playingList.PlayBehavior);
                    break;
                case "refresh":
                    InitBindings();
                    break;
                case "addLocal":
                    AddLocalFilesDo();
                    break;
                case "search":
                    ItemList_Header_Search_Control.IsOpen = !ItemList_Header_Search_Control.IsOpen;
                    break;
            }
        }

        private void AppBarToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarToggleButton;
            switch (btn.Tag)
            {
                case "multiSelect":
                    MultiSelectDo((bool)btn.IsChecked);
                    break;
                case "move":
                    break;
            }
        }

        private void multiButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            switch (btn.Tag)
            {
                case "multi_selectAll":
                    ItemsList.SelectAll();
                    break;
                case "multi_selectReverse":
                    SelectedReverseDo();
                    break;
                case "multi_deleteSelect":
                    DeleteSelectedItemDo();
                    break;
                case "multi_downloadSelect":
                    DownloadSelectedItemDo();
                    break;
                case "multi_addSelectToPlaying":
                    AddSelectedItemToPlayingDo();
                    break;
            }
        }

        bool isInSave = false;
        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isInSave) return;
            if (isInInitBindings) return;
            if (musicListData == null) return;
            if (SortComboBox == null) return;
            if (SortComboBox.SelectedIndex == -1) return;
            if (SortComboBox.SelectedIndex == (int)musicListData.PlaySort) return;
            isInSave = true;
            musicListData.PlaySort = (PlaySort)SortComboBox.SelectedIndex;
            var data = await PlayListHelper.ReadData();
            data[musicListData.ListName] = JObject.FromObject(musicListData);
            await PlayListHelper.SaveData(data);
            InitBindings();
            isInSave = false;
        }

        private void ItemList_Header_Search_Control_IsOpenChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                ItemList_Header_Search_Control.FocusToSearchBox();
                ItemsList_Header_Root.Margin = new(0, 0, 0, ItemList_Header_Search_Control.ActualHeight + 7);
            }
            else
            {
                ItemsList_Header_Root.Margin = new(0, 0, 0, 3);
            }
        }

        private void multi_addSelectToPlayList_flyout_Opening(object sender, object e)
        {
            MenuFlyout flyout = sender as MenuFlyout;
            foreach (var list in App.playListReader.NowMusicListData)
            {
                MenuFlyoutItem item = new MenuFlyoutItem()
                {
                    Text = list.ListShowName,
                    Tag = list
                };
                item.Click += Item_Click;
                flyout.Items.Add(item);
            }
        }

        private async void Item_Click(object sender, RoutedEventArgs e)
        {
            var flyoutItem = sender as MenuFlyoutItem;
            flyoutItem.Click -= Item_Click;
            MainWindow.ShowLoadingDialog();
            var text = await PlayListHelper.ReadData();
            var list = flyoutItem.Tag as MusicListData;
            var listName = list.ListName;
            foreach (SongItemBindBase item in ItemsList.SelectedItems.Cast<SongItemBindBase>())
            {
                MainWindow.SetLoadingText($"������ӣ�{item.MusicData.Title} - {item.MusicData.ButtonName}");
                MainWindow.SetLoadingProgressRingValue(ItemsList.SelectedItems.Count, ItemsList.SelectedItems.IndexOf(item));

                await Task.Run(() =>
                {
                    PlayListHelper.AddMusicDataToPlayList(item.MusicData, list);
                });
            }
            text[listName] = JObject.FromObject(list);
            await PlayListHelper.SaveData(text);
            await App.playListReader.Refresh();
            MainWindow.HideDialog();
        }

        private void multi_addSelectToPlayList_flyout_Closed(object sender, object e)
        {
            foreach (MenuFlyoutItem item in (sender as MenuFlyout).Items)
            {
                item.Click -= Item_Click;
            }
            (sender as MenuFlyout).Items.Clear();
        }

        private async void ItemList_Header_Search_Control_SearchingAItem(SongItemBindBase songItemBind)
        {
            var scrollPlacement = ActualHeight <= 450 ? ScrollItemPlacement.Bottom : ScrollItemPlacement.Center;
            await ItemsList.SmoothScrollIntoViewWithItemAsync(songItemBind, scrollPlacement);
            await ItemsList.SmoothScrollIntoViewWithItemAsync(songItemBind, scrollPlacement, true);
            MusicDataItem.TryHighlight(songItemBind);
        }

        private void MainWindow_InKeyDownEvent(Windows.System.VirtualKey key)
        {
            if (MainWindow.isControlDown)
            {
                if (key == Windows.System.VirtualKey.F)
                {
                    ItemList_Header_Search_Control.IsOpen = !ItemList_Header_Search_Control.IsOpen;
                    if (!ItemList_Header_Search_Control.IsOpen)
                        ItemsList_Header_Info_CommandBar.Focus(FocusState.Programmatic);
                }
            }
        }
    }
}
