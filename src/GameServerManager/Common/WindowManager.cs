using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Crpg.GameServerManager.Common;

public static class WindowManager
{
    public static void SetProcessWindowTitle(Process process, string newTitle)
    {
        if (process == null || process.MainWindowHandle == IntPtr.Zero)
        {
            Console.WriteLine("ERROR: Unable to find process window.");
            return;
        }

        if (!SetWindowText(process.MainWindowHandle, newTitle))
        {
            Console.WriteLine("ERROR: Failed to set window title.");
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowText(IntPtr hWnd, string lpString);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
}
