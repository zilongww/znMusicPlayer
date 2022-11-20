using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Composition;
using WinRT;
using znMusicPlayerWUI.Helpers;
using znMusicPlayerWUI.Pages;
using znMusicPlayerWUI.Pages.MusicPages;
using Microsoft.UI.Xaml.Input;
using Windows.Graphics;
using System.Runtime.InteropServices;
using WinRT.Interop;
using NAudio.Wave;
using Microsoft.UI.Xaml.Hosting;
using System.Collections.ObjectModel;

namespace znMusicPlayerWUI.Windowed
{
    public sealed partial class DesktopLyricWindow : Window
    {
        public DesktopLyricWindow()
        {
            InitializeComponent();
            App.AppDesktopLyricWindow = WindowHelper.GetAppWindowForCurrentWindow(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var a = OverlappedPresenter.Create();
            a.IsMaximizable = true;
            a.IsMinimizable = false;
            a.SetBorderAndTitleBar(true, true);
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                App.AppDesktopLyricWindow.SetPresenter(a);
                App.AppDesktopLyricWindow.TitleBar.ExtendsContentIntoTitleBar = true;
                App.AppDesktopLyricWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                App.AppDesktopLyricWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            SetBackdrop(BackdropType.DesktopAcrylic);
        }

        #region Enable Window Backdrop
        public static ApplicationTheme ActualTheme = ApplicationTheme.Dark;
        static ElementTheme requestedTheme = ElementTheme.Default;
        public static ElementTheme RequestedTheme
        {
            get => requestedTheme;
            set
            {
                requestedTheme = value;

                switch (value)
                {
                    case ElementTheme.Dark: ActualTheme = ApplicationTheme.Dark; break;
                    case ElementTheme.Light: ActualTheme = ApplicationTheme.Light; break;
                    case ElementTheme.Default:
                        var uiSettings = new Windows.UI.ViewManagement.UISettings();
                        var defaultthemecolor = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);
                        ActualTheme = defaultthemecolor == Colors.Black ? ApplicationTheme.Dark : ApplicationTheme.Light;
                        break;
                }
            }
        }

        public enum BackdropType
        {
            Mica,
            DesktopAcrylic,
            DefaultColor,
        }

        static WindowsSystemDispatcherQueueHelper m_wsdqHelper;
        static BackdropType m_currentBackdrop;
        static MicaController m_micaController;
        static DesktopAcrylicController m_acrylicController;
        static SystemBackdropConfiguration m_configurationSource;

        public void SetBackdrop(BackdropType type)
        {
            m_currentBackdrop = BackdropType.DefaultColor;
            if (m_micaController != null)
            {
                m_micaController.Dispose();
                m_micaController = null;
            }
            if (m_acrylicController != null)
            {
                m_acrylicController.Dispose();
                m_acrylicController = null;
            }
            this.Activated -= DesktopLyricWindow_Activated;
            this.Closed -= DesktopLyricWindow_Closed;
            m_configurationSource = null;

            if (type == BackdropType.Mica)
            {
                if (TrySetMicaBackdrop())
                {
                    m_currentBackdrop = type;
                }
                else
                {
                    type = BackdropType.DesktopAcrylic;
                }
            }
            if (type == BackdropType.DesktopAcrylic)
            {
                if (TrySetAcrylicBackdrop())
                {
                    m_currentBackdrop = type;
                }
                else
                {
                }
            }
        }

        bool TrySetMicaBackdrop()
        {
            if (MicaController.IsSupported())
            {
                m_configurationSource = new SystemBackdropConfiguration();
                this.Activated += DesktopLyricWindow_Activated;
                this.Closed += DesktopLyricWindow_Closed;

                m_configurationSource.IsInputActive = true;
                switch (RequestedTheme)
                {
                    case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
                    case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
                    case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
                }
                m_micaController = new MicaController();
                m_micaController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                m_micaController.SetSystemBackdropConfiguration(m_configurationSource);
                return true;
            }

            return false;
        }

        private void DesktopLyricWindow_Closed(object sender, WindowEventArgs args)
        {

        }

        private void DesktopLyricWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (m_currentBackdrop != BackdropType.DesktopAcrylic)
            {
                m_configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
            }
        }

        bool TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
                m_configurationSource = new SystemBackdropConfiguration();
                this.Activated += DesktopLyricWindow_Activated;
                this.Closed += DesktopLyricWindow_Closed;

                m_configurationSource.IsInputActive = true;
                switch (RequestedTheme)
                {
                    case ElementTheme.Dark: m_configurationSource.Theme = SystemBackdropTheme.Dark; break;
                    case ElementTheme.Light: m_configurationSource.Theme = SystemBackdropTheme.Light; break;
                    case ElementTheme.Default: m_configurationSource.Theme = SystemBackdropTheme.Default; break;
                }

                m_acrylicController = new DesktopAcrylicController()
                {
                    TintColor = Color.FromArgb(255, 40, 40, 40),
                    LuminosityOpacity = 0.9f,
                    TintOpacity = 0f
                };

                m_acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                m_acrylicController.SetSystemBackdropConfiguration(m_configurationSource);
                return true;
            }

            return false;
        }
        #endregion

    }
}
