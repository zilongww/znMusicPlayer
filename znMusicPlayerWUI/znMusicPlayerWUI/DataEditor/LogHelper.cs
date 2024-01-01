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
        static object _lockfile = new();
        public static void WriteLog(string title, string message, bool showDialog = true)
        {
            lock (_lockfile)
            {
                if (showDialog)
                {
                    ShowDialog(title, message);
                }
                var last = File.ReadAllText(DataFolderBase.LogDataPath, Encoding.UTF8);
                string log = $"{last}[{DateTime.Now} | {title}] {message}\n";
                File.WriteAllText(DataFolderBase.LogDataPath, log);
            }
        }

        private static async void ShowDialog(string title, string message)
        {
            await MainWindow.ShowDialog(title, message);
        }
    }
}
