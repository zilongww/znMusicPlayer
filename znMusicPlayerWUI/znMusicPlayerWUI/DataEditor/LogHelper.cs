using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace znMusicPlayerWUI.DataEditor
{
    public static class LogHelper
    {
        public static async void WriteLog(string title, string message, bool showDialog = true)
        {
            if (showDialog)
            {
                ShowDialog(title, message);
            }
            var last = await File.ReadAllTextAsync(DataFolderBase.LogDataPath, Encoding.UTF8);
            string log = $"{last}----------{DateTime.Now}|{title}----------\n{message}\n";
            await File.WriteAllTextAsync(DataFolderBase.LogDataPath, log);
        }
        private static async void ShowDialog(string title, string message)
        {
            await MainWindow.ShowDialog(title, message);
        }
    }
}
