using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeJournal
{
    class NativeMethods
    {
        public static class Win32Utils
        {
            [DllImport("user32.dll")]
            private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

            public static void ShowWindow(IntPtr hWnd)
            {
                ShowWindowAsync(hWnd, 5);
            }
            public static void HideWindow(IntPtr hWnd)
            {
                ShowWindowAsync(hWnd, 0);
            }

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

            [StructLayout(LayoutKind.Sequential)]
            private struct FLASHWINFO
            {
                public uint cbSize;
                public IntPtr hwnd;
                public uint dwFlags;
                public uint uCount;
                public uint dwTimeout;
            }

            const uint FLASHW_ALL = 3;
            const uint FLASHW_TIMERNOFG = 12;

            public static void Flash(System.Windows.Window window)
            {
                FLASHWINFO fi = new FLASHWINFO();
                fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
                fi.hwnd = (new WindowInteropHelper(window)).Handle;
                fi.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
                fi.uCount = 10;
                fi.dwTimeout = 0;
                FlashWindowEx(ref fi);
            }
        }
    }
}
