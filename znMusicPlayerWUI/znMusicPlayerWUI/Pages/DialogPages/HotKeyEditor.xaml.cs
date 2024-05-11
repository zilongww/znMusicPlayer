using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using TewIMP.Background.HotKeys;
using Vanara.PInvoke;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TewIMP.Pages.DialogPages
{
    public sealed partial class HotKeyEditor : UserControl
    {
        public HotKeyEditor()
        {
            InitializeComponent();
        }

        HotKey hotKey = null;
        User32.HotKeyModifiers hotKeyModifiers = User32.HotKeyModifiers.MOD_NONE;
        Windows.System.VirtualKey normalKey = Windows.System.VirtualKey.A;
        private void HotKeyEditor_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Key);

            switch (e.Key)
            {
                case Windows.System.VirtualKey.LeftWindows:
                case Windows.System.VirtualKey.RightWindows:
                    hotKeyModifiers = hotKeyModifiers | User32.HotKeyModifiers.MOD_WIN;
                    break;
                case Windows.System.VirtualKey.Control:
                    hotKeyModifiers = hotKeyModifiers | User32.HotKeyModifiers.MOD_CONTROL;
                    break;
                case Windows.System.VirtualKey.Shift:
                    hotKeyModifiers = hotKeyModifiers | User32.HotKeyModifiers.MOD_SHIFT;
                    break;
                case Windows.System.VirtualKey.Menu:
                    hotKeyModifiers = hotKeyModifiers | User32.HotKeyModifiers.MOD_ALT;
                    break;
                default:
                    normalKey = e.Key;
                    break;
            }

            hotKey = new HotKey(hotKeyModifiers, normalKey, default);
            HotKeyViewer.DataContext = hotKey;

            if (hotKeyModifiers == User32.HotKeyModifiers.MOD_NONE)
            {
                MainWindow.AsyncDialog.IsPrimaryButtonEnabled = false;
            }
            else
            {
                MainWindow.AsyncDialog.IsPrimaryButtonEnabled = true;
            }
        }

        private void AsyncDialog_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.LeftWindows:
                case Windows.System.VirtualKey.RightWindows:
                    hotKeyModifiers = hotKeyModifiers ^ User32.HotKeyModifiers.MOD_WIN;
                    break;
                case Windows.System.VirtualKey.Control:
                    hotKeyModifiers = hotKeyModifiers ^ User32.HotKeyModifiers.MOD_CONTROL;
                    break;
                case Windows.System.VirtualKey.Shift:
                    hotKeyModifiers = hotKeyModifiers ^ User32.HotKeyModifiers.MOD_SHIFT;
                    break;
                case Windows.System.VirtualKey.Menu:
                    hotKeyModifiers = hotKeyModifiers ^ User32.HotKeyModifiers.MOD_ALT;
                    break;
            }

            if (hotKey.HotKeyModifiers == User32.HotKeyModifiers.MOD_NONE)
            {
                MainWindow.AsyncDialog.IsPrimaryButtonEnabled = false;
            }
            else
            {
                MainWindow.AsyncDialog.IsPrimaryButtonEnabled = true;
            }
        }

        HotKey changedHotKey = null;
        bool isGettingHotKey = false;
        public async void ShowDialog(HotKey hotKey)
        {
            if (hotKey == null) return;
            changedHotKey = hotKey;

            NowHotKeyText.Text = $"��ǰ�ȼ���{HotKey.GetHotKeyIDString(hotKey.HotKeyID)}";
            HotKeyViewer.DataContext = changedHotKey;
            ShowDialog1();
            this.Focus(FocusState.Keyboard);
        }

        private async void ShowDialog1()
        {
            if (changedHotKey == null) return;
            MainWindow.AsyncDialog.PreviewKeyDown += HotKeyEditor_KeyDown;
            MainWindow.AsyncDialog.PreviewKeyUp += AsyncDialog_PreviewKeyUp;
            MainWindow.AsyncDialog.IsPrimaryButtonEnabled = false;
            var r = await MainWindow.ShowDialog("�����ȼ�", this, "ȡ��", "ȷ��", "����", ContentDialogButton.Primary);
            if (r == ContentDialogResult.Primary)
            {
                if (hotKey != null)
                {
                    hotKey.HotKeyID = changedHotKey.HotKeyID;
                    App.hotKeyManager.ChangeHotKey(hotKey);
                }
            }
            else if (r == ContentDialogResult.Secondary)
            {
                foreach (var k in HotKeyManager.DefaultRegisterHotKeysList)
                {
                    if (k.HotKeyID == changedHotKey.HotKeyID)
                    {
                        App.hotKeyManager.ChangeHotKey(k);
                        break;
                    }
                }
            }
            MainWindow.AsyncDialog.PreviewKeyDown -= HotKeyEditor_KeyDown;
            MainWindow.AsyncDialog.PreviewKeyUp -= AsyncDialog_PreviewKeyUp;
            MainWindow.AsyncDialog.IsPrimaryButtonEnabled = true;
        }
    }
}
