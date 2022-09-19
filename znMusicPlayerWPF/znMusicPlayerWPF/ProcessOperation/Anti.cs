using System.Diagnostics;

namespace znMusicPlayerWPF.ProcessOperation
{
    class Anti
    {
        public static bool ReCallAnti()
        {
            foreach (Process p in Process.GetProcesses())
            {
                string processString = p.ProcessName.ToLower().Replace("_", "");

                if (processString.Contains("cheat") && processString.Contains("engine")) return true;
            }
            return false;
        }
    }
}
