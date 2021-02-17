using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Windows
{
    /// <summary> Contains functionality to get windows' into. </summary>
    internal static class WindowProcessor
    {
        // Full scope of API functions in one C# file could be found also here:
        // https://gist.github.com/AlexanderBaggett/d1504da93727a1778e8b5b3453946fc1

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-getshellwindow
        /// <summary> Retrieves a handle to the Shell's desktop window </summary>
        /// <returns> handle of the Shell's desktop window </returns>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetShellWindow();

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-GetForegroundWindow
        /// <summary> Retrieves a handle to the foreground window (the window with which the user is currently working). 
        /// The system assigns a slightly higher priority to the thread that creates the foreground window than it does to other threads 
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-SetForegroundWindow
        /// <summary> Blinks windows in task bar </summary>
        /// <param name="hWnd"> Handler </param>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-GetWindow
        /// <summary>
        /// Retrieves a handle to a window that has the specified relationship (Z-Order or owner) to the specified window
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowFlags gwFlag);

        internal enum GetWindowFlags : uint
        {
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6,
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4
        }

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-GetTopWindow
        /// <summary>
        /// Examines the Z order of the child windows associated with the specified parent window 
        /// and retrieves a handle to the child window at the top of the Z order.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetTopWindow(IntPtr hWnd);

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-GetAncestor
        /// <summary>
        /// Retrieves the handle to the ancestor of the specified window
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags gaFlag);
        internal enum GetAncestorFlags : uint
        {
            GA_PARENT = 1,
            GA_ROOT = 2,
            GA_ROOTOWNER = 3
        }

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-GetWindowInfo
        /// <summary> Retrieves information about the specified window </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

        /// <summary> GetWindowInfo override, to reduce the number of input params </summary>
        private static WINDOWINFO GetWindowInfo(IntPtr hWnd)
        {
            WINDOWINFO info = new WINDOWINFO();
            info.cbSize = (uint) Marshal.SizeOf(info);
            GetWindowInfo(hWnd, ref info);
            return info;
        }

        /// <summary> defines a rectangle by the coordinates of its upper-left and lower-right corners </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct WINDOWINFO
        {
            public uint cbSize;
            public RECT rcWindow;
            public RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;

            public WINDOWINFO(Boolean? filler)
            : this() // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
            {
                cbSize = (UInt32)(Marshal.SizeOf(typeof(WINDOWINFO)));
            }
        }

        /// <summary> window rectangle information/current coordinates </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            private int _Left;
            private int _Top;
            private int _Right;
            private int _Bottom;
        }


        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-GetWindowText
        /// <summary> Retreives window's caption text by its WindowHandle </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary> Override for GetWindowText with fewer params usage. </summary>
        public static string GetWindowText(IntPtr hWnd)
        {
            int size = GetWindowTextLength(hWnd);
            if (size > 0)
            {
                StringBuilder sb = new StringBuilder(size + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                return sb.ToString();
            }

            return String.Empty;
        }

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-GetWindowTextLength
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-IsWindowVisible
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-EnumWindows
        /// <summary>
        /// Enumerates all top-level windows on the screen by passing the handle to each window, 
        /// in turn, to an application-defined callback function. 
        /// EnumWindows continues until the last top-level window is enumerated or the callback function returns FALSE.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

        // https://docs.microsoft.com/en-gb/previous-versions/windows/desktop/legacy/ms633498(v=vs.85)
        /// <summary> Delegate to filter which windows to include </summary>
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary> Minimizes (but does not destroy) the specified window. </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseWindow(IntPtr hWnd);

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-GetWindowThreadProcessId
        /// <summary>
        /// usage: 
        /// uint processId = 0;
        /// GetWindowThreadProcessId(hWnd, out processId);
        /// </summary>
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint lpdwProcessId);

        // https://docs.microsoft.com/en-gb/windows/win32/api/processthreadsapi/nf-processthreadsapi-openprocess
        /// <summary> Opens an existing local process object </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, uint processId);

        internal enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        //http://pinvoke.net/default.aspx/kernel32.QueryFullProcessImageName
        /// <summary> Retrieves the full name of the executable image for the specified process. (by Process Handle) </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool QueryFullProcessImageName(
            [In]IntPtr hProcess,
            [In]int dwFlags,
            [Out]StringBuilder lpExeName,
            ref int lpdwSize);

// -------------------------------------------------------------------
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(HookType hook, IntPtr callback, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);
        /// <summary>
        /// Hook Types. See the documentation of SetWindowsHookEx for reference.
        /// </summary>
        public enum HookType : int
        {
            ///
            WH_JOURNALRECORD = 0,
            ///
            WH_JOURNALPLAYBACK = 1,
            ///
            WH_KEYBOARD = 2,
            ///
            WH_GETMESSAGE = 3,
            ///
            WH_CALLWNDPROC = 4,
            ///
            WH_CBT = 5,
            ///
            WH_SYSMSGFILTER = 6,
            ///
            WH_MOUSE = 7,
            ///
            WH_HARDWARE = 8,
            ///
            WH_DEBUG = 9,
            ///
            WH_SHELL = 10,
            ///
            WH_FOREGROUNDIDLE = 11,
            ///
            WH_CALLWNDPROCRET = 12,
            ///
            WH_KEYBOARD_LL = 13,
            ///
            WH_MOUSE_LL = 14
        }
        // -------------------------------------------------------------------

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-SendMessage
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessage(
            IntPtr hWnd, 
            uint Msg, 
            IntPtr wParam, 
            IntPtr lParam);

        // https://docs.microsoft.com/en-gb/windows/win32/api/winuser/nf-winuser-GetMessage
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
            public uint lPrivate;
        }

        /// <summary> defines the x- and y- coordinates of a point </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public long x;
            public long y;
        }

        // https://docs.microsoft.com/en-us/windows/win32/winmsg/window-notifications
        // "c:\Program Files (x86)\Microsoft Visual Studio 9.0\SmartDevices\SDK\PocketPC2003\Include\winuser.h"
        internal enum wmMsg : uint
        {
            WM_CREATE = 0x0001,
            WM_DESTROY = 0x0002,
            WM_CLOSE = 0x0010,
            WM_QUIT = 0x0012
        }

        // other WinAPI functions to use:
        // GetWindowPlacement

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        // private const int SW_HIDE = 0;
        // private const int SW_SHOW = 5;

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        public static void MinimizeWindow(IntPtr hWnd)
        {
            CloseWindow(hWnd);
        }

        public static void BringWindowToFront(IntPtr hWnd)
        {
            SetForegroundWindow(hWnd);
        }

        public static void KillWindow(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero) return;
            SendMessage(hWnd, (uint)wmMsg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }


        /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
        /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
        public static IDictionary<IntPtr, uint> GetOpenWindows()
        {
            IntPtr shellWindow = GetShellWindow();
            IDictionary<IntPtr, uint> windows = new Dictionary<IntPtr, uint>();

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;
                if (!IsMainWindow(hWnd)) return true;
                if (GetWindowTextLength(hWnd) == 0) return true;

                WINDOWINFO wInfo = GetWindowInfo(hWnd);
                if (wInfo.atomWindowType == 50345) return true; // skip "Drag & Drop" handle
                if (wInfo.atomWindowType == 49434) return true; // skip "ICA Seamless Host Agent" handle [comes from Citrix]
                if (wInfo.atomWindowType == 49375) return true; // skip Inernal Window such as "Start Menu"

                windows[hWnd] = GetProcessId(hWnd);
                // windows.Keys.ToList().Sort(new IntPtrComparer()); --> this approach doesn't work here.
                // windows.Keys.ToList() must be stored it a new object, sorted and then
                // windows dictionary must be recreated again out of that list of "keys" object.
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary> Find all parent windows that contain the given title text </summary>
        /// <param name="titleText"> The text that the window title must contain. </param>
        public static IDictionary<IntPtr, uint> GetOpenWindows(string title)
        {
            IntPtr shellWindow = GetShellWindow();
            IDictionary<IntPtr, uint> windows = new Dictionary<IntPtr, uint>();

            EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (hWnd == shellWindow) return true;
                if (!IsWindowVisible(hWnd)) return true;
                if (!IsMainWindow(hWnd)) return true;
                if (GetWindowTextLength(hWnd) == 0) return true;

                if (GetWindowText(hWnd).ToLower().Contains(title.ToLower()))
                {
                    windows[hWnd] = GetProcessId(hWnd);
                }
                // windows.Keys.ToList().Sort(new IntPtrComparer()); --> this approach doesn't work here.
                // windows.Keys.ToList() must be stored it a new object, sorted and then
                // windows dictionary must be recreated again out of that list of "keys" object.
                return true;
            }, IntPtr.Zero);
            return windows;
        }

        public static IDictionary<IntPtr, WindowProcess> GetOpenWindowProcesses()
        {
            IDictionary<IntPtr, WindowProcess> windows = new Dictionary<IntPtr, WindowProcess>();
            List<IntPtr> handles = new List<IntPtr>(GetOpenWindows().Keys);
            // handles.Sort(new IntPtrComparer());
            foreach (IntPtr hWnd in handles)
            {
                windows[hWnd] = new WindowProcess(hWnd);
            }
            return windows;
        }

        public static bool IsMainWindow(IntPtr hWnd)
        {
            return GetWindow(hWnd, GetWindowFlags.GW_OWNER) == IntPtr.Zero && IsWindowVisible(hWnd);
        }

        public static uint GetProcessId(IntPtr hWnd)
        {
            uint pid = 0;
            GetWindowThreadProcessId(hWnd, out pid);
            return pid;
        }
        public static string GetProcessFilePath(uint pid)
        {
            IntPtr hProc = OpenProcess(
            ProcessAccessFlags.QueryLimitedInformation | ProcessAccessFlags.VirtualMemoryRead,
            false, pid);

            int capacity = 1024;
            StringBuilder sb = new StringBuilder(capacity);
            QueryFullProcessImageName(hProc, 0, sb, ref capacity);
            return sb.ToString(0, capacity);
        }

        /// <summary> Find a windows and return true is it's active and visible </summary>
        /// <param name="hWnd"> Window's handle. </param>
        public static bool IsActive(IntPtr hWnd)
        {
            WINDOWINFO wInfo = GetWindowInfo(hWnd);
            return (wInfo.dwWindowStatus == 1) && (hWnd == GetForegroundWindow());
        }

    }
}