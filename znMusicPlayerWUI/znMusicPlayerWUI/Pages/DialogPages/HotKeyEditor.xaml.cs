using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vanara.PInvoke;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using znMusicPlayerWUI.Background.HotKeys;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace znMusicPlayerWUI.Pages.DialogPages
{
    public sealed partial class HotKeyEditor : UserControl
    {
        public HotKeyEditor()
        {
            this.InitializeComponent();
        }

        bool isWinPressed = false;
        bool isCtrlPressed = false;
        bool isShiftPressed = false;
        bool isAltPressed = false;
        Windows.System.VirtualKey modeKey = Windows.System.VirtualKey.Control;
        Windows.System.VirtualKey normalKey = Windows.System.VirtualKey.A;
        private void HotKeyEditor_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Key);
            HotKeyText.Text = "";

            switch (e.Key)
            {
                case Windows.System.VirtualKey.LeftWindows:
                case Windows.System.VirtualKey.RightWindows:
                    isWinPressed = true;
                    break;
                case Windows.System.VirtualKey.Control:
                    isCtrlPressed = true;
                    break;
                case Windows.System.VirtualKey.Shift:
                    isShiftPressed = true;
                    break;
                case Windows.System.VirtualKey.Menu:
                    isAltPressed = true;
                    break;
                default:
                    normalKey = e.Key;
                    break;
            }
            if (isWinPressed) HotKeyText.Text += "Win + ";
            if (isCtrlPressed) HotKeyText.Text += "Ctrl + ";
            if (isShiftPressed) HotKeyText.Text += "Shift + ";
            if (isAltPressed) HotKeyText.Text += "Alt + ";
            HotKeyText.Text += normalKey;
            if (isWinPressed == false && isCtrlPressed == false && isShiftPressed == false && isAltPressed == false)
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
                    isWinPressed = false;
                    break;
                case Windows.System.VirtualKey.Control:
                    isCtrlPressed = false;
                    break;
                case Windows.System.VirtualKey.Shift:
                    isShiftPressed = false;
                    break;
                case Windows.System.VirtualKey.Menu:
                    isAltPressed = false;
                    break;
            }
            if (!isWinPressed) HotKeyText.Text = HotKeyText.Text.Replace("Win + ", "");
            if (!isCtrlPressed) HotKeyText.Text = HotKeyText.Text.Replace("Ctrl + ", "");
            if (!isShiftPressed) HotKeyText.Text = HotKeyText.Text.Replace("Shift + ", "");
            if (!isAltPressed) HotKeyText.Text = HotKeyText.Text.Replace("Alt + ", "");

            if (isWinPressed == false && isCtrlPressed == false && isShiftPressed == false && isAltPressed == false)
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

            NowHotKeyText.Text = $"当前热键：{HotKey.GetHotKeyIDString(hotKey.HotKeyID)}";
            HotKeyText.Text = hotKey.ToString();
            ShowDialog1();
            this.Focus(FocusState.Keyboard);
        }

        private async void ShowDialog1()
        {
            MainWindow.AsyncDialog.PreviewKeyDown += HotKeyEditor_KeyDown;
            MainWindow.AsyncDialog.PreviewKeyUp += AsyncDialog_PreviewKeyUp;
            MainWindow.AsyncDialog.IsPrimaryButtonEnabled = false;
            var r = await MainWindow.ShowDialog("设置热键", this, "取消", "确定", "重置", ContentDialogButton.Primary);
            if (r == ContentDialogResult.Primary)
            {
                User32.HotKeyModifiers hotKeyModifiers = User32.HotKeyModifiers.MOD_NONE;
                if (isWinPressed) hotKeyModifiers = hotKeyModifiers | User32.HotKeyModifiers.MOD_WIN;
                if (isCtrlPressed) hotKeyModifiers = hotKeyModifiers | User32.HotKeyModifiers.MOD_CONTROL;
                if (isShiftPressed) hotKeyModifiers = hotKeyModifiers | User32.HotKeyModifiers.MOD_SHIFT;
                if (isAltPressed) hotKeyModifiers = hotKeyModifiers | User32.HotKeyModifiers.MOD_ALT;

                HotKey hotKey = new(hotKeyModifiers, normalKey, changedHotKey.HotKeyID);
                App.hotKeyManager.ChangeHotKey(hotKey);
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
